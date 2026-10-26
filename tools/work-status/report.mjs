#!/usr/bin/env node
// A bounded, read-only view of existing authorities, not another work registry.
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {execFileSync} from 'node:child_process';

const short=(v,n=360)=>typeof v==='string'?v.slice(0,n):null;
const seconds=(start,end)=>{
  const a=Date.parse(start),b=Date.parse(end);
  return Number.isFinite(a)&&Number.isFinite(b)&&b>=a?(b-a)/1000:null;
};
export function jobTiming(job,observedAt=new Date().toISOString()){
  const stages={preparation:0,build:0,test:0,delivery:0,other:0};
  let measured=0,unmeasured=0;
  for(const s of job.steps||[]){
    if(s.status!=='completed'||s.conclusion==='skipped'){unmeasured++;continue;}
    const duration=seconds(s.started_at,s.completed_at);
    if(duration===null){unmeasured++;continue;}
    const name=String(s.name||'');
    const category=/upload|publish|publication|attach|commit|push/i.test(name)?'delivery':
      /test|\bqa\b|probe|validat|verify/i.test(name)?'test':
      /build|compil|import.*unity|unity.*import/i.test(name)?'build':
      /checkout|set.?up|prepar|restore|download|install|preflight/i.test(name)?'preparation':'other';
    stages[category]+=duration;measured++;
  }
  return {queue_seconds:seconds(job.created_at,job.started_at),
    observed_wait_seconds:job.status==='queued'?seconds(job.created_at,observedAt):null,
    elapsed_seconds:seconds(job.started_at,job.status==='completed'?job.completed_at:observedAt),
    duration_final:job.status==='completed',stage_seconds:measured?stages:null,
    measured_steps:measured,unmeasured_steps:unmeasured,
    method:'Step-name categories are estimates; job timings are not summed across parallel jobs. Missing timestamps are unknown.'};
}

export function versionStates({head,delivery,android}){
  const qa=delivery?.current_revision_qa,candidate=delivery?.candidate;
  return {development:{source_sha:head||null,status:'development_not_certified_by_this_report'},
    web_candidate:{revision:candidate?.revision||null,source_sha:candidate?.source_sha||null,
      technical_qa_recorded:!!(candidate?.revision&&qa?.revision===candidate.revision&&qa.native_unity_pass===true&&qa.windows_gpu_webgl_pass===true),
      qa_revision:qa?.revision||null},
    web_available:{revision:delivery?.published?.remote_active_confirmed===true?delivery.published.revision||null:null,
      basis:'recorded_remote_confirmation_not_a_fresh_test'},
    android_channel:{version:android?.enabled===true?android.versionName||null:null,
      basis:'enabled_channel_manifest_not_installed_device_verification'}};
}
// Advisory classification. Never creates a lease, dispatches a job, declares CI/ART PASS
// or treats absence of Actions jobs as proof that a chat/PC is idle.
export function operationalSignal(workstream, liveBranch={status:'not_checked'}, windows={observation:'not_checked',running:[],queued:[]}) {
  const paused=/^OWNER_PAUSED_/i.test(workstream?.phase||'') ||
    /^PAUSED BY OWNER\b/i.test(workstream?.next||'');
  if(paused) return {code:'OWNER_PAUSED',severity:'hold',
    next_safe_action:'Respect the owner pause; do not dispatch, take ownership or merge.'};
  const explicitAvailable=workstream?.ownership_state==='available' &&
    workstream?.owner==null && workstream?.write_scope_released===true &&
    !(workstream?.resources||[]).length;
  if(explicitAvailable) return {code:'EXPLICITLY_AVAILABLE',severity:'informational',
    next_safe_action:'An authorized agent may claim this canonical workstream with the current registry SHA; availability is not a live runner check.'};
  if(!workstream?.owner) return {code:'UNASSIGNED_NOT_RELEASED',severity:'needs_reconciliation',
    next_safe_action:'Reconcile owner/release fields; a null owner alone does not authorize a claim.'};
  if(liveBranch.status!=='observed') return {code:'LIVE_EVIDENCE_UNKNOWN',severity:'unknown',
    next_safe_action:'Read the exact branch HEAD and live Actions/jobs; no inactivity or runner availability can be inferred.'};
  const checks=Array.isArray(liveBranch.checks)?liveBranch.checks:[];
  const restricted=checks.find(r=>r.conclusion==='action_required');
  if(restricted) return {code:'GITHUB_AUTHORIZATION_REQUIRED',severity:'external_blocker',
    run_id:restricted.run_id, next_safe_action:'Review the GitHub Actions approval requirement on this exact run. Do not bypass authorization or call it a test failure.'};
  const failed=checks.find(r=>['failure','timed_out','startup_failure'].includes(r.conclusion));
  if(failed) return {code:'CI_FAILURE_NEEDS_DIAGNOSIS',severity:'investigate',
    run_id:failed.run_id,next_safe_action:'Owner inspects run logs and fixes source; an automatic retry alone is not a repair.'};
  const running=checks.find(r=>['in_progress','queued','waiting','pending','requested'].includes(r.status));
  if(running) return {code:'EXTERNAL_ACTIONS_RUNNING',severity:'observed',
    run_id:running.run_id,next_safe_action:'Follow the exact run and consume its evidence before the next authorized step.'};
  const usesWindows=(workstream?.resources||[]).includes('windows-runner-heavy');
  const windowsComplete=windows?.observation==='complete_for_listed_actions_jobs';
  if(usesWindows) return {code:'WINDOWS_LEASE_WITHOUT_CONFIRMED_WORK',severity:'needs_reconciliation',
    next_safe_action:windowsComplete ?
      'A Windows resource claim exists; no matching live Actions job was established for this workstream. Confirm the owner and external PC processes before changing the lease.' :
      'A Windows resource claim exists and Actions observation is incomplete; reconcile without auto-releasing or stealing the runner.'};
  if(checks.some(r=>r.conclusion==='success'))
    return {code:'TECHNICAL_EVIDENCE_REVIEW',severity:'review',
      next_safe_action:'Inspect matching SHA, tests, visuals and deliverable; successful CI is not completion or artistic approval.'};
  return {code:'CLAIMED_WITHOUT_RECENT_RUN',severity:'unknown',
    next_safe_action:'Ask the owner to inspect the next allowed gate; latest-20 window cannot prove an abandoned task.'};
}

// This is a declared registry state, not evidence that an agent or machine is running.
export function declaredOperatingState(workstream){
  if(/^OWNER_PAUSED_/i.test(workstream?.phase||'') || /^PAUSED BY OWNER\b/i.test(workstream?.next||'')) return 'paused_by_owner';
  if(!workstream?.owner) return 'unassigned_in_registry';
  return 'claimed_in_registry';
}
export function summarize({registry,delivery=null,android=null,head=null,live=null,checkedAt=new Date().toISOString()}){
  if(!Array.isArray(registry?.active))throw Error('Canonical registry active array missing');
  const active=registry.active.map(w=>({id:w.id,title:short(w.title),owner:short(w.owner),branch:short(w.branch),declared_status:w.status,declared_phase:short(w.phase),declared_operating_state:declaredOperatingState(w),declared_next:short(w.next),declared_resources:Array.isArray(w.resources)?w.resources:[],operational_signal:operationalSignal(w,live?.branches?.[w.branch]||{status:'not_checked'},live?.windows),delivery_definition:{deliverable:short(w.delivery_definition?.deliverable),evidence:short(w.delivery_definition?.evidence),handoff:short(w.delivery_definition?.handoff)},live_branch:live?.branches?.[w.branch]||{status:'not_checked'}}));
  return {schema_version:1,checked_at:checkedAt,source_head:head,read_only:true,
    authority:'pipeline/active-workstreams.json; pipeline/playtest-delivery.json; updates/android/family.json',
    active,windows:live?.windows||{observation:'not_checked',running:[],queued:[],errors:[]},
    versions:versionStates({head,delivery,android}),
    web:delivery?{recorded_state:delivery.state,candidate_revision:delivery.candidate?.revision||null,candidate_source:delivery.candidate?.source_sha||null,confirmed_revision:delivery.published?.remote_active_confirmed===true?delivery.published.revision:null,uploaded_revision:delivery.uploaded?.revision||null,uploaded_activation_confirmed:delivery.uploaded?.remote_active_confirmed===true,last_remote_check:delivery.uploaded?.latest_remote_check?.checked_at||null,hosted_gameplay_verified:delivery.published?.hosted_gameplay_verified===true}:{recorded_state:'source_missing'},
    android:android?{enabled:android.enabled===true,version_code:android.versionCode,version_name:android.versionName,source_sha:android.sourceSha}:{enabled:null,status:'source_missing'},
    limits:['Declared resources are reservations, not a measurement of PC activity.','Live Actions jobs do not detect processes started outside Actions.','Recorded delivery is not a fresh remote-host or physical-device test.','No historical workstream is reactivated or certified by this report.']};
}

export async function collectLive({github,repo,active,now=()=>new Date().toISOString()}){
  const branches={},errors=[],windows={observation:'complete_for_listed_actions_jobs',checked_at:now(),running:[],queued:[],errors:[]};
  for(const branch of [...new Set(active.map(w=>w.branch).filter(Boolean))]){
    try{
      const head=(await github.rest.repos.getBranch({...repo,branch})).data.commit.sha;
      const response=(await github.rest.actions.listWorkflowRunsForRepo({...repo,branch,per_page:20})).data;
      branches[branch]={status:'observed',head,checks:(response.workflow_runs||[]).filter(r=>r.head_sha===head&&r.name!=='Eldoria concise work status').map(r=>({run_id:r.id,name:r.name,status:r.status,conclusion:r.conclusion||null})),checks_window:'latest_20_runs_for_branch'};
      // One recent substantive run per branch, including older source revisions.
      // Separate from HEAD checks so an old successful build cannot certify new code.
      const sample=(response.workflow_runs||[]).find(r=>/native|unity|android|webgl|progression/i.test(r.name||''));
      branches[branch].timing_sample={status:'no_matching_run_in_latest_20'};
      if(sample){
        try{
          const data=(await github.rest.actions.listJobsForWorkflowRun({...repo,run_id:sample.id,per_page:100,filter:'latest'})).data;
          branches[branch].timing_sample={status:(data.total_count||0)>100?'partial':'observed',run_id:sample.id,source_sha:sample.head_sha||null,run_attempt:sample.run_attempt||null,
            jobs:(data.jobs||[]).slice(0,100).map(j=>({job_id:j.id,name:short(j.name),status:j.status,conclusion:j.conclusion||null,timing:jobTiming(j,now())}))};
        }catch{branches[branch].timing_sample={status:'unavailable',run_id:sample.id};}
      }
    }catch{branches[branch]={status:'unavailable'};errors.push('branch:'+branch);}
  }
  const runs=new Map();
  for(const status of ['in_progress','queued','waiting','pending']){
    try{
      const data=(await github.rest.actions.listWorkflowRunsForRepo({...repo,status,per_page:100})).data;
      if((data.total_count||0)>100)windows.errors.push('run_list_truncated:'+status);
      for(const r of data.workflow_runs||[])runs.set(r.id,r);
    }catch{windows.errors.push('run_list_unavailable:'+status);}
  }
  if(runs.size>30)windows.errors.push('job_observation_bounded_to_30_runs');
  for(const run of [...runs.values()].slice(0,30)){
    try{
      const data=(await github.rest.actions.listJobsForWorkflowRun({...repo,run_id:run.id,per_page:100})).data;
      if((data.total_count||0)>100)windows.errors.push('job_list_truncated:'+run.id);
      for(const j of data.jobs||[]){
        if(j.status==='completed')continue;
        const labels=(j.labels||[]).map(x=>String(x).toLowerCase());
        if(!labels.length){windows.errors.push('job_labels_unknown:'+j.id);continue;}
        if(!labels.includes('self-hosted')||!labels.includes('windows'))continue;
        const item={run_id:run.id,job_id:j.id,branch:run.head_branch,job:j.name,status:j.status,started_at:j.started_at||null,timing:jobTiming(j,now())};
        (j.status==='in_progress'?windows.running:windows.queued).push(item);
      }
    }catch{windows.errors.push('jobs_unavailable:'+run.id);}
  }
  if(windows.errors.length)windows.observation='partial';
  return {checked_at:now(),branches,windows,errors};
}

const md=v=>String(v??'—').replace(/[\r\n]/g,' ').replace(/[|<>\[\]`]/g,' ').slice(0,500);
export function markdown(report){
  let out='# Eldoria — estado operativo\n\nConsulta: '+md(report.checked_at)+' · HEAD: '+md(report.source_head)+'\n\n';
  out+='| Trabajo registrado | Rama | Situación declarada | Fase | Observación de rama |\n| --- | --- | --- | --- | --- |\n';
  for(const w of report.active)out+='| '+md(w.title||w.id)+' | '+md(w.branch)+' | '+md(w.declared_operating_state)+' | '+md(w.declared_phase||w.declared_status)+' | '+md(w.live_branch.status)+' |\n';
  out+='\nSeñales de continuidad (orientativas, sin ejecutar acciones):\n';
  for(const w of report.active){const s=w.operational_signal||{};out+='- '+md(w.title||w.id)+': '+md(s.code)+'; siguiente paso seguro: '+md(s.next_safe_action)+'\n';}
  const timedBranches=new Set();
  for(const w of report.active){
    out+='\nEntrega de '+md(w.title||w.id)+': '+md(w.delivery_definition.deliverable||'sin definición explícita registrada')+'; evidencia: '+md(w.delivery_definition.evidence)+'; cierre: '+md(w.delivery_definition.handoff)+'.\n';
    const sample=w.live_branch.timing_sample;
    // A branch can have several workstreams. Show its timing evidence once, not once per owner.
    if(sample && !timedBranches.has(w.branch)){
      timedBranches.add(w.branch);
      out+='\nMuestra de tiempos (rama '+md(w.branch)+'): '+md(sample.status)+'; run '+md(sample.run_id)+'; SHA '+md(sample.source_sha)+'. No certifica el HEAD actual.\n';
      if(sample.jobs?.length)out+='\n| Job | Espera (s) | Transcurrido (s) | Preparación | Build | Pruebas | Entrega | Otros |\n| --- | --- | --- | --- | --- | --- | --- | --- |\n';
      for(const j of sample.jobs||[]){const t=j.timing,s=t.stage_seconds;out+='| '+md(j.name)+' | '+md(t.queue_seconds)+' | '+md(t.elapsed_seconds)+(t.duration_final?'':' (en curso)')+' | '+['preparation','build','test','delivery','other'].map(k=>md(s?.[k])).join(' | ')+' |\n';}
    }
  }
  out+='\nTiempos en segundos; categorías estimadas por nombre de paso. — = desconocido. No sumar jobs paralelos; las muestras no incluyen procesos externos a Actions.\n';
  out+='\nWindows: '+md(report.windows.observation)+'; '+report.windows.running.length+' jobs en ejecución observados, '+report.windows.queued.length+' en espera observados.\n';
  for(const j of [...report.windows.running,...report.windows.queued])out+='- Run '+j.run_id+', job '+j.job_id+': '+md(j.job)+' — '+md(j.status)+' — '+md(j.branch)+'\n';
  out+='\nWebGL: candidata '+md(report.web.candidate_revision)+'; última revisión confirmada en el manifiesto '+md(report.web.confirmed_revision)+'; subida '+md(report.web.uploaded_revision)+'; activación de esa subida confirmada: '+report.web.uploaded_activation_confirmed+'.\n';
  out+='\nAndroid: canal habilitado '+report.android.enabled+'; versión '+md(report.android.version_name)+' / código '+md(report.android.version_code)+'.\n';
  out+='\nVersiones: desarrollo '+md(report.versions.development.source_sha)+'; candidata WebGL '+md(report.versions.web_candidate.revision)+' con QA técnica registrada para esa revisión: '+report.versions.web_candidate.technical_qa_recorded+'; disponible WebGL según confirmación registrada: '+md(report.versions.web_available.revision)+'; canal Android habilitado: '+md(report.versions.android_channel.version)+'. No se deduce equivalencia entre estas versiones.\n';
  out+='\nSituación declarada: paused_by_owner = pausa explícita; unassigned_in_registry = sin titular registrado (NO significa disponible); claimed_in_registry = titular registrado (NO implica agente ejecutándose).\n';
  out+='\nLas fases son declaraciones del registro; los jobs y conclusiones del JSON son observaciones separadas. Este resumen no aprueba arte, juego, móvil ni publicaciones. La ausencia de jobs observados no demuestra que el PC esté libre.\n';
  return out;
}

export function fromRoot(root){
  const read=p=>JSON.parse(fs.readFileSync(path.join(root,p),'utf8'));
  const optional=p=>fs.existsSync(path.join(root,p))?read(p):null;
  let head=null;try{head=execFileSync('git',['rev-parse','HEAD'],{cwd:root,encoding:'utf8',stdio:['ignore','pipe','ignore']}).trim();}catch{}
  return {registry:read('pipeline/active-workstreams.json'),delivery:optional('pipeline/playtest-delivery.json'),android:optional('updates/android/family.json'),head};
}
if(process.argv[1]&&path.resolve(process.argv[1])===fileURLToPath(import.meta.url)){
  try{
    const args=process.argv.slice(2),i=args.indexOf('--root');
    const root=i>=0?path.resolve(args[i+1]):path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../..');
    const report=summarize(fromRoot(root));
    process.stdout.write(args.includes('--json')?JSON.stringify(report,null,2)+'\n':markdown(report));
  }catch(e){console.error('WORK_STATUS_FAIL: '+e.message);process.exitCode=1;}
}
