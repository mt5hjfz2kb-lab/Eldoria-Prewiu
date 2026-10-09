import {createHash} from 'node:crypto';
export const digest=x=>createHash('sha256').update(JSON.stringify(x)).digest('hex');
export function publishedIdentity({manifest,files,observed,expected}){
 const errors=[];
 if(!manifest||!Array.isArray(manifest.files)||!expected||!observed)return {verified:false,errors:['IDENTITY_INCOMPLETE']};
 if(manifest.source_sha!==expected.source_sha||manifest.build_sha256!==expected.build_sha256||manifest.artifact_id!==expected.artifact_id||manifest.deployment_id!==expected.deployment_id||manifest.policy_digest!==expected.policy_digest)errors.push('MANIFEST_CANDIDATE_MISMATCH');
 if(observed.deployment_id!==manifest.deployment_id||observed.build_sha256!==manifest.build_sha256||observed.source_sha!==manifest.source_sha)errors.push('DEPLOYMENT_SUPERSEDED_OR_WRONG_VERSION');
 for(const f of manifest.files){
  if(!f.path||!/^([a-f0-9]{64})$/i.test(f.sha256)||!files||typeof files[f.path]!=='string') {errors.push('SERVED_FILE_MISSING:'+f.path);continue;}
  const actual=createHash('sha256').update(Buffer.from(files[f.path],'base64')).digest('hex');
  if(actual!==f.sha256)errors.push('SERVED_BYTES_MISMATCH:'+f.path);
 }
 if(!manifest.files.length)errors.push('NO_SERVED_FILES');
 return {verified:errors.length===0,errors,identity_digest:digest(manifest)};
}
export function appendEvent(journal,event){
 if(!Array.isArray(journal)||!event||!['ISSUED','REVOKED','SUPERSEDED'].includes(event.kind)||!event.certificate_id||!event.actor||!event.approval_id||!event.at)throw Error('INVALID_HISTORY_EVENT');
 const last=journal.at(-1);
 const record={index:journal.length,prev_hash:last?.hash||'GENESIS',event};
 record.hash=digest({index:record.index,prev_hash:record.prev_hash,event:record.event});
 if(last&&last.event.certificate_id===event.certificate_id&&last.event.kind===event.kind&&last.hash===record.hash)return journal;
 return [...journal,record];
}
export function verifyJournal(journal){
 if(!Array.isArray(journal))return false;
 let prev='GENESIS';
 for(let i=0;i<journal.length;i++){const e=journal[i];if(e.index!==i||e.prev_hash!==prev||e.hash!==digest({index:e.index,prev_hash:e.prev_hash,event:e.event}))return false;prev=e.hash;}
 return true;
}
export function certificateState(journal,id){
 if(!verifyJournal(journal))return 'INVALID_HISTORY';
 const a=journal.filter(e=>e.event.certificate_id===id);
 return a.length?a.at(-1).event.kind:'UNKNOWN';
}
