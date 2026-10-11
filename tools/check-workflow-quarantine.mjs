import fs from 'node:fs';
import path from 'node:path';

const manifest=JSON.parse(fs.readFileSync('pipeline/workflow-quarantine-v1.json','utf8'));
const failures=[];

const read=p=>{
  if(!fs.existsSync(p)){failures.push('Missing: '+p);return '';}
  return fs.readFileSync(p,'utf8');
};

function triggerBlock(text){
  const lines=text.split(/\r?\n/);
  const i=lines.findIndex(l=>l.trim()==='on:');
  if(i<0)return '';
  const out=[];
  for(let n=i+1;n<lines.length;n++){
    const l=lines[n];
    if(l.trim()===''){out.push(l);continue;}
    if(!/^\s/.test(l))break;
    out.push(l);
  }
  return out.join('\n');
}
function hasTrigger(block,name){
  return new RegExp('^\\s*'+name+'\\s*:','m').test(block);
}

for(const p of manifest.canonical_brain_do_not_disable||[]) read(p);

const brainChecks=[
 ['.github/workflows/art-production-governance.yml','push'],
 ['.github/workflows/repository-architecture-guard.yml','push'],
 ['.github/workflows/workflow-governance.yml','push'],
 ['.github/workflows/valoria-full-frame-convergence-control.yml','push'],
 ['.github/workflows/valoria-visual-authority-routing.yml','push']
];
for(const [p,t] of brainChecks){
  const text=read(p); if(!text)continue;
  if(!hasTrigger(triggerBlock(text),t)) failures.push('Canonical brain workflow lost automatic '+t+' trigger: '+p);
}

for(const p of manifest.conditional_manual_only||[]){
  const text=read(p); if(!text)continue;
  const block=triggerBlock(text);
  if(!hasTrigger(block,'workflow_dispatch')) failures.push('Quarantined workflow lacks workflow_dispatch: '+p);
  for(const t of manifest.rules.forbidden_trigger||[])
    if(hasTrigger(block,t)) failures.push('Quarantined workflow has forbidden automatic trigger '+t+': '+p);
}

const workflowDir='.github/workflows';
for(const name of fs.readdirSync(workflowDir)){
  if(!name.startsWith('valoria-camera-first-')||!name.endsWith('.yml'))continue;
  const p=path.join(workflowDir,name);
  const block=triggerBlock(read(p));
  if(!hasTrigger(block,'workflow_dispatch')) failures.push('Retired Camera-First workflow lacks manual dispatch: '+p);
  for(const t of manifest.rules.forbidden_trigger||[])
    if(hasTrigger(block,t)) failures.push('Retired Camera-First workflow auto-triggers via '+t+': '+p);
}

if(failures.length){
  console.error('WORKFLOW_QUARANTINE=FAIL');
  for(const f of failures)console.error('FAIL:',f);
  process.exit(1);
}
console.log('WORKFLOW_QUARANTINE=PASS');
console.log('BRAIN_ARCHITECTURE=PROTECTED');
console.log('NONCANONICAL_RND=AUTO_TRIGGER_DISABLED');
