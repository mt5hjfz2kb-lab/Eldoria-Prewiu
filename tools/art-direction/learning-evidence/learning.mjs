import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';

const ROOT=path.dirname(fileURLToPath(import.meta.url));
const HEX=/^[0-9a-f]{64}$/;
const GIT_SHA=/^[0-9a-f]{40}$/;
const STATES=new Set(['REJECT_NOT_VISIBLE','REVIEW_REQUIRED','NOT_MEASURED']);
const ART=new Set(['ART_REJECTED','ART_UNREVIEWED']);
const TRUST=new Set(['run_log_reviewed','pending_verification']);
const positive=n=>Number.isSafeInteger(n)&&n>0;
const nonnegative=n=>Number.isSafeInteger(n)&&n>=0;
const filled=s=>typeof s==='string'&&s.trim().length>=12&&s.length<=1200;

export function validate(db) {
  if(!db||db.schema_version!==1||!Array.isArray(db.cases)||!HEX.test(db.reference_sha256||''))
    return ['Invalid catalog root'];
  const errors=[],ids=new Set();
  for(const c of db.cases){
    if(!c||typeof c!=='object'){errors.push('Not a case object');continue;}
    const id=c.id||'<missing>';
    const fail=t=>errors.push(id+': '+t);
    if(!/^eldoria-[a-z0-9-]+$/.test(id)) fail('invalid ID');
    if(ids.has(id))fail('duplicate case');ids.add(id);
    if(!positive(c.source?.run_id)||!GIT_SHA.test(c.source?.git_sha||'')||!TRUST.has(c.source?.evidence_level))
      fail('invalid run provenance');
    if(c.source?.url!=='https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/'+c.source?.run_id)
      fail('run URL/id mismatch');
    if(c.reference_sha256!==db.reference_sha256)fail('mismatched reference hash');
    for(const attr of ['target','view_id','render_pass'])
      if(!/^[a-zA-Z0-9_-]{5,110}$/.test(c[attr]||''))fail('invalid '+attr);
    if(!STATES.has(c.visibility_gate)||!ART.has(c.art_verdict)||c.art_approved!==false)
      fail('invalid verdict or false ART PASS');
    if(!filled(c.diagnosis)||!filled(c.lesson)||!filled(c.next_action))fail('incomplete learning loop');
    const m=c.measurement;
    if(m?.kind==='camera_pixels'){
      if(!positive(m.width)||!positive(m.height)||!nonnegative(m.changed_pixels)||
         m.changed_pixels>m.width*m.height||!Number.isFinite(m.threshold_rgb)||m.threshold_rgb<=0)
        fail('bad pixel measurement');
      else {
        const f=m.changed_pixels/(m.width*m.height);
        if(c.visibility_gate==='NOT_MEASURED'||(c.visibility_gate==='REJECT_NOT_VISIBLE'&&f>=0.001)||
           (c.visibility_gate==='REVIEW_REQUIRED'&&f<0.001))
          fail('visibility contradicts measurement');
      }
      if(c.art_verdict==='ART_REJECTED'&&c.visibility_gate!=='REJECT_NOT_VISIBLE')
        fail('cannot reject aesthetic quality using pixels alone');
    }else if(m?.kind==='mesh_vertices'){
      if(!positive(m.changed_vertices)||!nonnegative(m.protected_changes)||c.visibility_gate!=='NOT_MEASURED')
        fail('mesh cannot certify visibility');
    }else fail('invalid measurement type');
    if(c.source?.evidence_level==='pending_verification'&&c.art_verdict!=='ART_UNREVIEWED')
      fail('unverified record cannot assert art verdict');
  }
  return errors;
}

export function appendPending(db,candidate){
  if(candidate?.source?.evidence_level!=='pending_verification')
    throw Error('New entries must be pending_verification until original artifacts checked');
  if(db.cases.some(x=>x.id===candidate.id))throw Error('Duplicate case: append-only history');
  const next=structuredClone(db);next.cases.push(candidate);
  const errors=validate(next);if(errors.length)throw Error(errors.join('\n'));
  return next;
}

export function comparisonGroups(db){
  const groups=new Map();
  for(const c of db.cases){
    if(c.measurement.kind!=='camera_pixels'||c.source.evidence_level!=='run_log_reviewed')continue;
    const m=c.measurement;
    const key=[c.reference_sha256,c.target,c.view_id,c.render_pass,m.width,m.height,m.threshold_rgb].join('|');
    if(!groups.has(key))groups.set(key,[]);
    groups.get(key).push({run_id:c.source.run_id,changed_pixels:m.changed_pixels,
      visible_fraction:m.changed_pixels/(m.width*m.height),gate:c.visibility_gate});
  }
  return [...groups.values()];
}

export function report(db){
  let out='# Eldoria — Evidence-based artistic memory\n\n';
  out+='Reference SHA-256: '+db.reference_sha256+'\n';
  out+='Visibility is NOT artistic quality or a model-weights update.\n\n';
  out+='| GitHub run | View | Measurement | Art verdict |\n| --- | --- | --- | --- |\n';
  for(const c of db.cases){
    const m=c.measurement;
    const measurement=m.kind==='camera_pixels'
      ? String(m.changed_pixels)+'/'+String(m.width*m.height)+' ('+
        (100*m.changed_pixels/(m.width*m.height)).toFixed(4)+'%) '+c.visibility_gate
      : String(m.changed_vertices)+' vertices; protected='+m.protected_changes+'; visibility unmeasured';
    out+='| [#'+c.source.run_id+']('+c.source.url+') | '+c.view_id+
      ' | '+measurement+' | '+c.art_verdict+' |\n';
  }
  out+='\nOnly matched camera, resolution, reference and render pass may be compared:\n';
  for(const group of comparisonGroups(db)){
    out+='- '+group.map(x=>'#'+x.run_id+': '+(x.visible_fraction*100).toFixed(4)+'%').join(' -> ');
    out+=group.length<2?' (no comparable trend)\n':' (technical visibility only)\n';
  }
  out+='\nAesthetic trend: UNDETERMINED; independent common-rubric ratings are absent.\n';
  return out;
}

export function brief(db, target='bastion1',maxChars=6000){
  if(!Number.isSafeInteger(maxChars)||maxChars<300||maxChars>12000)throw Error('Invalid brief size');
  let out='EVIDENCE-BASED ART MEMORY: not model-weight training, not ART APPROVED.\n';
  out+='Pinned approved reference: '+db.reference_sha256+'\n';
  const byLesson=new Map();
  for(const c of db.cases){
    if(c.target!==target||c.source.evidence_level!=='run_log_reviewed')continue;
    const key=c.lesson_id||c.id;
    if(!byLesson.has(key))byLesson.set(key,{lesson:c.lesson,runs:[]});
    byLesson.get(key).runs.push(c.source.run_id);
  }
  for(const [key,v] of byLesson){
    const line='LESSON '+key+' (runs '+v.runs.join(',')+'): '+v.lesson+'\n';
    if(out.length+line.length+130>maxChars)break;
    out+=line;
  }
  return out+'Never use pixel fraction as a beauty score. Require matched Unity evidence and independent art review.\n';
}

function load(p){return JSON.parse(fs.readFileSync(p,'utf8'));}
const isCli=process.argv[1]&&path.resolve(process.argv[1])===fileURLToPath(import.meta.url);
if(isCli){
  try{
    const args=process.argv.slice(2);
    const op=args[0]||'audit';
    const dbPath=args.includes('--db')?args[args.indexOf('--db')+1]:path.join(ROOT,'observations.json');
    const db=load(dbPath),errors=validate(db);
    if(errors.length)throw Error(errors.join('\n'));
    if(op==='audit')console.log('ART_MEMORY_AUDIT_PASS cases='+db.cases.length);
    else if(op==='report')process.stdout.write(report(db));
    else if(op==='brief')process.stdout.write(brief(db,args.includes('--target')?args[args.indexOf('--target')+1]:'bastion1'));
    else if(op==='ingest'){
      const e=args.indexOf('--event'),o=args.indexOf('--out');
      if(e<0||o<0||!args[e+1]||!args[o+1])throw Error('Require --event file.json --out new.json');
      if(path.resolve(args[o+1])===path.resolve(dbPath))throw Error('Refuse in-place rewriting of evidence');
      const next=appendPending(db,load(args[e+1]));
      fs.writeFileSync(args[o+1],JSON.stringify(next,null,2)+'\n',{flag:'wx'});
      console.log('STAGED_FOR_ARTIFACT_VERIFICATION');
    }else throw Error('Unknown operation');
  }catch(e){console.error('ART_MEMORY_FAIL '+e.message);process.exitCode=1;}
}
