import fs from 'node:fs';
import path from 'node:path';
const ROOT='.github/workflows';
const BASE='pipeline/art-knowledge/legacy-workflow-names-v1.json';
export function checkNewQwenWorkflows(root,knownNames){
 const problems=[];
 for(const file of fs.readdirSync(root).filter(x=>/\.ya?ml$/.test(x))){
  const text=fs.readFileSync(path.join(root,file),'utf8');
  if(knownNames.includes(file))continue; // Existing owner/running workflows are not taken over here.
  const directlyCallsQwen=/\b(?:python(?:3)?\s+|\$PYTHON\s+)?tools\/art-direction\/ai-authoring\/author_brief\.py\b/.test(text);
  if(directlyCallsQwen&&!/tools\/art-knowledge\/run-artist\.mjs/.test(text)){
   problems.push('New Qwen workflow bypasses art-memory adapter: '+file);
  }
 }
 return problems;
}
if(process.argv[1]&&path.resolve(process.argv[1])===new URL(import.meta.url).pathname){
 try{
  const base=JSON.parse(fs.readFileSync(BASE,'utf8'));
  if(base.schema_version!==1||!Array.isArray(base.workflows)||base.workflows.length<150)
   throw Error('Missing frozen workflow baseline');
  const problems=checkNewQwenWorkflows(ROOT,base.workflows);
  if(problems.length)throw Error(problems.join('; '));
  console.log('NEW_QWEN_MEMORY_ENTRYPOINT_POLICY=PASS known='+base.workflows.length);
 }catch(e){console.error('NEW_QWEN_MEMORY_ENTRYPOINT_POLICY=FAIL '+e.message);process.exitCode=1;}
}
