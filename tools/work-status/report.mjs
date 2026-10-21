#!/usr/bin/env node
// A bounded, read-only view of existing authorities, not another work registry.
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {execFileSync} from 'node:child_process';

const short=(v,n=360)=>typeof v==='string'?v.slice(0,n):null;
export function summarize({registry,delivery=null,android=null,head=null,live=null,checkedAt=new Date().toISOString()}){
  if(!Array.isArray(registry?.active))throw Error('Canonical registry active array missing');
  const active=registry.active.map(w=>({id:w.id,title:short(w.title),owner:short(w.owner),branch:short(w.branch),declared_status:w.status,declared_phase:short(w.phase),declared_next:short(w.next),declared_resources:Array.isArray(w.resources)?w.resources:[],live_branch:live?.branches?.[w.branch]||{status:'not_checked'}}));
  return {schema_version:1,checked_at:checkedAt,source_head:head,read_only:true,
    authority:'pipeline/active-workstreams.json; pipeline/playtest-delivery.json; updates/android/family.json',
    active,windows:live?.windows||{observation:'not_checked',running:[],queued:[],errors:[]},
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
        const item={run_id:run.id,job_id:j.id,branch:run.head_branch,job:j.name,status:j.status,started_at:j.started_at||null};
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
  out+='| Trabajo activo | Rama | Fase declarada | Observación de rama |\n| --- | --- | --- | --- |\n';
  for(const w of report.active)out+='| '+md(w.title||w.id)+' | '+md(w.branch)+' | '+md(w.declared_phase||w.declared_status)+' | '+md(w.live_branch.status)+' |\n';
  out+='\nWindows: '+md(report.windows.observation)+'; '+report.windows.running.length+' jobs en ejecución observados, '+report.windows.queued.length+' en espera observados.\n';
  for(const j of [...report.windows.running,...report.windows.queued])out+='- Run '+j.run_id+', job '+j.job_id+': '+md(j.job)+' — '+md(j.status)+' — '+md(j.branch)+'\n';
  out+='\nWebGL: candidata '+md(report.web.candidate_revision)+'; última revisión confirmada en el manifiesto '+md(report.web.confirmed_revision)+'; subida '+md(report.web.uploaded_revision)+'; activación de esa subida confirmada: '+report.web.uploaded_activation_confirmed+'.\n';
  out+='\nAndroid: canal habilitado '+report.android.enabled+'; versión '+md(report.android.version_name)+' / código '+md(report.android.version_code)+'.\n';
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
