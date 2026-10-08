// Eldoria authorized orchestration core. Pure reducer: no shell, network, spending or implicit approvals.
export const STATES=Object.freeze(['PROPOSED','APPROVED','QUEUED','RUNNING','VERIFYING','CORRECTING','BLOCKED','ACCEPTED','REPORTED']);
const terminal=new Set(['BLOCKED','REPORTED']);
const fail=(message)=>{throw new Error(message)};
export function evolve(state,event){
  if(!state||state.schema_version!==1||!Array.isArray(state.jobs)||!Array.isArray(state.events))fail('INVALID_STATE');
  if(!event||typeof event.id!=='string'||!/^[a-zA-Z0-9_-]{4,90}$/.test(event.id))fail('INVALID_EVENT');
  const prior=state.events.find(x=>x.id===event.id);
  if(prior){if(JSON.stringify(prior)!==JSON.stringify(event))fail('EVENT_ID_COLLISION');return structuredClone(state)}
  if(typeof event.job_id!=='string'||!/^[a-zA-Z0-9_-]{4,90}$/.test(event.job_id))fail('INVALID_JOB_ID');
  const out=structuredClone(state),j=out.jobs.find(x=>x.id===event.job_id);
  if(event.type==='PROPOSE'){
    if(j||typeof event.owner!=='string'||!event.owner||typeof event.scope!=='string'||!event.scope||!Number.isInteger(event.retry_limit)||event.retry_limit<0||event.retry_limit>3)fail('INVALID_PROPOSAL');
    out.jobs.push({id:event.job_id,owner:event.owner,scope:event.scope,state:'PROPOSED',retry_limit:event.retry_limit,attempt:0,approval:null,evidence:[],history:[]});
  }else{
    if(!j||terminal.has(j.state))fail('JOB_NOT_ACTIVE');
    switch(event.type){
      case 'APPROVE':
        if(j.state!=='PROPOSED'||event.actor!=='owner'||typeof event.authorization!=='string'||event.authorization.length<8)fail('OWNER_APPROVAL_REQUIRED');
        j.approval=event.authorization;j.state='APPROVED';break;
      case 'QUEUE':
        if(!['APPROVED','CORRECTING'].includes(j.state)||!j.approval||event.actor!=='coordinator')fail('UNAUTHORIZED_DISPATCH');
        j.state='QUEUED';break;
      case 'START':
        if(j.state!=='QUEUED'||event.actor!=='runner'||typeof event.run_id!=='string'||!event.run_id)fail('INVALID_START');
        j.state='RUNNING';j.run_id=event.run_id;j.attempt++;break;
      case 'SUBMIT':
        if(j.state!=='RUNNING'||event.actor!=='runner'||typeof event.artifact!=='string'||!event.artifact||!/^[a-f0-9]{40}$/.test(event.sha))fail('EVIDENCE_REQUIRED');
        j.state='VERIFYING';j.evidence.push({run_id:j.run_id,artifact:event.artifact,sha:event.sha});break;
      case 'REVIEW':
        if(j.state!=='VERIFYING'||event.actor!=='independent-qa'||typeof event.proof!=='string'||!event.proof||typeof event.pass!=='boolean')fail('INDEPENDENT_REVIEW_REQUIRED');
        j.history.push({attempt:j.attempt,pass:event.pass,proof:event.proof});
        if(event.pass){j.state='ACCEPTED';break}
        j.state=j.attempt<=j.retry_limit?'CORRECTING':'BLOCKED';break;
      case 'REPORT':
        if(j.state!=='ACCEPTED'||event.actor!=='directorate'||typeof event.report!=='string'||!event.report)fail('DIRECTORATE_REPORT_REQUIRED');
        j.state='REPORTED';j.report=event.report;break;
      default:fail('UNKNOWN_EVENT');
    }
  }
  out.events.push(structuredClone(event));return out;
}
export const empty=()=>({schema_version:1,jobs:[],events:[]});
export function pending(state){return state.jobs.filter(j=>['QUEUED','CORRECTING','BLOCKED','ACCEPTED'].includes(j.state)).map(j=>({job_id:j.id,phase:j.state,owner:j.owner}));}
