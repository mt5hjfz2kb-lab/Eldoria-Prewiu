import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';

// This safeguards discovery of the owner's enduring agreement; it cannot certify independent reasoning.
export const MARKER='ELDORIA_OWNER_AI_COLLABORATION_V1';
export const ENTRYPOINTS=['AGENTS.md','tools/ELDORIA_AGENT_SESSION_BOOTSTRAP.md','docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md','docs/DOCUMENTATION_INDEX.md','README.md'];
export const OBLIGATIONS=['ROL_OWNER','ROL_AI','INDEPENDENT_CRITIQUE','EVIDENCE_FIRST','PIVOT_ON_FAILURE','TRUTHFUL_STATUS','HANDOFF','LIMITS'];
export function inspectContract(files){
  const errors=[];
  for(const name of ENTRYPOINTS){if(typeof files[name]!=='string' || !files[name].includes(MARKER))errors.push('Missing owner–AI pact reference: '+name);}
  const canonical=files['AGENTS.md']||'';
  for(const code of OBLIGATIONS)if(!canonical.includes('**'+code+':**'))errors.push('Canonical pact lacks duty: '+code);
  if(!canonical.includes('El propietario no es programador')&&!canonical.includes('**No es programador:**'))errors.push('Owner non-programmer role is missing');
  if(!canonical.includes('dos ciclos de corrección'))errors.push('No documented pivot trigger');
  if(!canonical.includes('los roles no se reinician'))errors.push('Role continuity missing');
  return errors;
}
if(process.argv[1]&&path.resolve(process.argv[1])===fileURLToPath(import.meta.url)){
  const files=Object.fromEntries(ENTRYPOINTS.map(p=>[p,fs.existsSync(p)?fs.readFileSync(p,'utf8'):null]));
  const errors=inspectContract(files);
  if(errors.length){for(const e of errors)console.error('FAIL:',e);process.exitCode=1;}
  else console.log('Owner–AI founding pact present in all agent handoff entrypoints. This is a discoverability check, NOT a judgment-quality certification.');
}
