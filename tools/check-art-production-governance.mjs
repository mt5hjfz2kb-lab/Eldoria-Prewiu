import fs from 'node:fs';
import path from 'node:path';
import { execFileSync } from 'node:child_process';

const registryPath='pipeline/active-workstreams.json';
const requestDir='pipeline/art-production-requests';
const registry=JSON.parse(fs.readFileSync(registryPath,'utf8'));
const failures=[];

if(!Array.isArray(registry.active)) failures.push('Registry active must be an array.');

for(const workstream of registry.active || []){
  const routing=workstream.art_production_routing;
  if(!['required','not_applicable'].includes(routing)){ failures.push(workstream.id+': missing/invalid art_production_routing (required|not_applicable).'); continue; }
  if(routing==='not_applicable'){
    if(!workstream.art_production_routing_reason || !String(workstream.art_production_routing_reason).trim()) failures.push(workstream.id+': not_applicable requires art_production_routing_reason.');
    continue;
  }
  const expected=workstream.art_production_request || path.join(requestDir,workstream.id+'.json');
  if(!fs.existsSync(expected)){ failures.push(workstream.id+': required art-production request missing: '+expected); continue; }
  let req;
  try { req=JSON.parse(fs.readFileSync(expected,'utf8')); } catch(err){ failures.push(workstream.id+': invalid request JSON: '+err.message); continue; }
  if(req.workstream_id!==workstream.id) failures.push(workstream.id+': request workstream_id must exactly match active id.');
  if(req.request_id!==workstream.id) failures.push(workstream.id+': request_id must exactly match active id.');
  if(req.enabled!==true) failures.push(workstream.id+': required request must have enabled=true.');
  if(String(req.status||'').toUpperCase()!=='ACTIVE') failures.push(workstream.id+': required request status must be ACTIVE.');
  try { execFileSync(process.execPath,['tools/plan-art-production.mjs',expected],{stdio:'pipe'}); }
  catch(err){ failures.push(workstream.id+': planner FAIL for '+expected+': '+String(err.stderr||err.message).trim()); }
}

console.log('Art production governance: '+((registry.active||[]).length)+' active workstreams checked.');
if(failures.length){ for(const f of failures) console.error('FAIL:',f); console.error('ART_PRODUCTION_GOVERNANCE=FAIL'); process.exit(1); }
console.log('ART_PRODUCTION_GOVERNANCE=PASS');
