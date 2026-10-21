#!/usr/bin/env node
// Exact, allowlisted output checkpoints. Publication uses a temporary Git index;
// never clean, switch, stash, rebase or reset the editor's working tree.
import fs from 'node:fs';
import path from 'node:path';
import os from 'node:os';
import crypto from 'node:crypto';
import {spawnSync} from 'node:child_process';
import {fileURLToPath} from 'node:url';

const sha256=b=>crypto.createHash('sha256').update(b).digest('hex');
const fail=s=>{throw Error(s);};
function safe(p){
  if(typeof p!=='string'||!p||p.startsWith('/')||p.includes('\\')||p.includes(':')||/[\0\r\n\t]/.test(p)||p.split('/').some(s=>s==='.'||s==='..'||!s)) fail('Unsafe repository path');
  return p;
}
const covered=(p,roots)=>roots.some(r=>p===r||p.startsWith(r+'/'));
function git(root,args,{input,env={},allowFailure=false}={}){
  const r=spawnSync('git',args,{cwd:root,input,env:{...process.env,GIT_TERMINAL_PROMPT:'0',...env},maxBuffer:64*1024*1024,encoding:input instanceof Buffer?undefined:'utf8',windowsHide:true});
  if(r.error)throw r.error;
  if(r.status!==0&&!allowFailure)fail('Git operation failed: '+args[0]+' (exit '+r.status+'). Original outputs/checkpoint retained.');
  return {status:r.status,out:Buffer.isBuffer(r.stdout)?r.stdout.toString('utf8'):r.stdout||''};
}
const split0=s=>s.split('\0').filter(Boolean);
function configCheck(config){
  if(config.schema_version!==1||!Array.isArray(config.outputs)||!config.outputs.length||!Array.isArray(config.inputs)||!config.inputs.length)fail('Invalid recovery config');
  config.outputs.forEach(safe);config.inputs.forEach(safe);
  if(config.outputs.some(p=>p==='.git'||p.startsWith('.git/')||p.split('/').some(x=>/^\.env(?:\.|$)/.test(x))))fail('Sensitive output scope forbidden');
  if(typeof config.branch!=='string'||!/^[A-Za-z0-9][A-Za-z0-9._/-]*$/.test(config.branch)||!config.workstream_id)fail('Explicit branch and workstream required');
  return config;
}
function tree(root,ref){
  const entries=new Map();
  for(const record of split0(git(root,['ls-tree','-rz',ref]).out)){
    const tab=record.indexOf('\t'),[mode,type,hash]=record.slice(0,tab).split(' ');
    entries.set(record.slice(tab+1),{mode,type,hash});
  }
  return entries;
}
function realFile(root,relative){
  const full=path.join(root,safe(relative));
  let stat;try{stat=fs.lstatSync(full);}catch(e){if(e.code==='ENOENT')return null;throw e;}
  if(!stat.isFile()||stat.isSymbolicLink())fail('Only regular output files can be checkpointed: '+relative);
  const resolved=fs.realpathSync(full),base=fs.realpathSync(root)+path.sep;
  if(!resolved.startsWith(base))fail('Output escapes its repository');
  return {full,stat};
}
export function capture({root,out,config,validated=false,sourceSha,runId=null,maxBytes=512*1024*1024}){
  root=path.resolve(root);out=path.resolve(out);configCheck(config);
  if(out===root||out.startsWith(root+path.sep))fail('Checkpoint must be outside the working repository');
  git(root,['check-ref-format','--branch',config.branch]);
  const head=git(root,['rev-parse','HEAD']).out.trim();
  if(sourceSha&&sourceSha!==head)fail('Checkout differs from requested native source SHA');
  const base=tree(root,head);
  const changed=new Set(split0(git(root,['diff','--name-only','--no-renames','-z','HEAD','--',...config.outputs]).out));
  for(const p of split0(git(root,['ls-files','--others','--exclude-standard','-z','--',...config.outputs]).out))changed.add(p);
  const files=[];let total=0;
  for(const p of [...changed].sort()){
    safe(p);if(!covered(p,config.outputs))fail('Output outside allowlist');
    const before=base.get(p);if(before&&before.type!=='blob')fail('Unsupported base output');
    const f=realFile(root,p);
    if(f){total+=f.stat.size;if(total>maxBytes)fail('Checkpoint size limit exceeded; original outputs retained');}
    files.push({path:p,before:before?.hash||null,mode:f?(before?.mode||'100644'):null,deleted:!f,size:f?.stat.size||0});
  }
  // Exclusive creation keeps an earlier checkpoint intact on a retry.
  fs.mkdirSync(out,{recursive:false});fs.mkdirSync(path.join(out,'files'));
  for(const f of files){
    if(f.deleted)continue;
    const bytes=fs.readFileSync(realFile(root,f.path).full);
    if(bytes.length!==f.size)fail('Output changed while checkpointing');
    f.sha256=sha256(bytes);f.git_blob=git(root,['hash-object','--stdin'],{input:bytes}).out.trim();
    const dest=path.join(out,'files',f.path);fs.mkdirSync(path.dirname(dest),{recursive:true});fs.writeFileSync(dest,bytes,{flag:'wx'});
  }
  const manifest={schema_version:1,kind:'native-output-checkpoint',source_sha:head,branch:config.branch,workstream_id:config.workstream_id,config_sha256:sha256(JSON.stringify(config)),validated:validated===true,run_id:runId,created_at:new Date().toISOString(),files,total_bytes:total};
  fs.writeFileSync(path.join(out,'checkpoint.json'),JSON.stringify(manifest,null,2)+'\n',{flag:'wx'});
  return manifest;
}
export function publish({root,checkpoint,config,retries=3}){
  root=path.resolve(root);checkpoint=path.resolve(checkpoint);configCheck(config);
  git(root,['check-ref-format','--branch',config.branch]);
  const m=JSON.parse(fs.readFileSync(path.join(checkpoint,'checkpoint.json'),'utf8'));
  if(m.schema_version!==1||m.kind!=='native-output-checkpoint'||m.validated!==true)fail('Native QA must pass before publication');
  if(m.config_sha256!==sha256(JSON.stringify(config))||m.branch!==config.branch||m.workstream_id!==config.workstream_id||!/^([a-f0-9]{40}|[a-f0-9]{64})$/.test(m.source_sha))fail('Checkpoint source/config mismatch');
  if(!Array.isArray(m.files)||!m.files.length)fail('No generated outputs to publish');
  const seen=new Set();
  for(const f of m.files){
    safe(f.path);if(!covered(f.path,config.outputs)||seen.has(f.path))fail('Checkpoint path outside scope or duplicated');seen.add(f.path);
    if(f.before!==null&&!/^[a-f0-9]{40}$/.test(f.before))fail('Invalid original blob identity');
    if(!f.deleted){
      if(!['100644','100755'].includes(f.mode))fail('Unsupported output mode');
      const payload=realFile(path.join(checkpoint,'files'),f.path);if(!payload)fail('Checkpoint payload missing');
      const bytes=fs.readFileSync(payload.full);
      if(bytes.length!==f.size||sha256(bytes)!==f.sha256||git(root,['hash-object','--stdin'],{input:bytes}).out.trim()!==f.git_blob)fail('Checkpoint bytes changed; refusing publication');
    }
  }
  // A recovered artifact can be used from a shallow checkout. Fetch metadata,
  // not an editor rebuild; still require the original source to be an ancestor.
  if(git(root,['rev-parse','--is-shallow-repository']).out.trim()==='true'){
    git(root,['fetch','--no-tags','--filter=blob:none','--unshallow','origin','refs/heads/'+config.branch]);
  }
  const before=tree(root,m.source_sha);
  for(const f of m.files)if((before.get(f.path)?.hash||null)!==f.before)fail('Forged original blob identity');
  const temp=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-recovery-index-'));
  try{
    for(let attempt=1;attempt<=retries;attempt++){
      git(root,['fetch','--no-tags','--no-recurse-submodules','origin','refs/heads/'+config.branch]);
      const remote=git(root,['rev-parse','FETCH_HEAD']).out.trim();
      if(git(root,['merge-base','--is-ancestor',m.source_sha,remote],{allowFailure:true}).status!==0)fail('Remote branch no longer descends from verified source');
      const remoteTree=tree(root,remote);
      const matches=f=>(remoteTree.get(f.path)?.hash||null)===(f.deleted?null:f.git_blob);
      if(m.files.every(matches))return {status:'ALREADY_PRESENT',commit:remote,qa_source_sha:m.source_sha,certifies_other_remote_changes:false};
      const changes=split0(git(root,['diff','--name-only','--no-renames','-z',m.source_sha,remote,'--']).out);
      const foreign=changes.filter(p=>!seen.has(p)&&covered(p,[...config.inputs,'.gitattributes','.gitmodules']));
      if(foreign.length)fail('Protected source changed remotely; recover outputs, then reassess QA: '+foreign.slice(0,5).join(', '));
      for(const f of m.files)if((remoteTree.get(f.path)?.hash||null)!==f.before&&!matches(f))fail('Concurrent output edit; original checkpoint retained: '+f.path);
      const registry=JSON.parse(git(root,['show',remote+':pipeline/active-workstreams.json']).out);
      if(!registry.active?.some(w=>w.id===config.workstream_id&&w.status==='active'&&w.branch===config.branch))fail('Existing native workstream must remain active on its branch');
      const index=path.join(temp,'index-'+attempt),env={GIT_INDEX_FILE:index};
      git(root,['read-tree',remote],{env});
      let entries='';
      for(const f of m.files){
        if(f.deleted)entries+='0 '+ '0'.repeat(40)+'\t'+f.path+'\0';
        else{
          const bytes=fs.readFileSync(realFile(path.join(checkpoint,'files'),f.path).full);
          const blob=git(root,['hash-object','-w','--stdin'],{input:bytes}).out.trim();
          if(blob!==f.git_blob)fail('Output blob mismatch');
          entries+=f.mode+' '+blob+'\t'+f.path+'\0';
        }
      }
      git(root,['update-index','-z','--index-info'],{input:entries,env});
      const resultTree=git(root,['write-tree'],{env}).out.trim();
      const author={GIT_AUTHOR_NAME:'github-actions[bot]',GIT_AUTHOR_EMAIL:'41898282+github-actions[bot]@users.noreply.github.com',GIT_COMMITTER_NAME:'github-actions[bot]',GIT_COMMITTER_EMAIL:'41898282+github-actions[bot]@users.noreply.github.com'};
      const commit=git(root,['commit-tree',resultTree,'-p',remote,'-m','feat(native): recover verified '+config.workstream_id+' outputs\n\nNative source: '+m.source_sha+'\nNative run: '+m.run_id],{env:author}).out.trim();
      const pushed=git(root,['push','origin',commit+':refs/heads/'+config.branch],{allowFailure:true});
      if(pushed.status===0)return {status:'PUBLISHED',commit,qa_source_sha:m.source_sha,checkpoint_retained:true};
    }
    fail('Remote branch kept advancing; checkpoint retained, no force push attempted');
  }finally{fs.rmSync(temp,{recursive:true,force:true});}
}
if(process.argv[1]&&path.resolve(process.argv[1])===fileURLToPath(import.meta.url)){
  try{
    const args=process.argv.slice(2),value=f=>args[args.indexOf(f)+1];
    for(const f of ['--root','--config'])if(!args.includes(f)||!value(f)||value(f).startsWith('--'))fail('Missing '+f);
    const root=path.resolve(value('--root')),config=JSON.parse(fs.readFileSync(path.join(root,safe(value('--config'))),'utf8'));
    let result;
    if(args[0]==='capture'){
      if(!args.includes('--out'))fail('Missing --out');
      result=capture({root,out:value('--out'),config,validated:args.includes('--validated'),sourceSha:process.env.GITHUB_SHA,runId:process.env.GITHUB_RUN_ID||null});
    }else if(args[0]==='publish'){
      if(!args.includes('--checkpoint'))fail('Missing --checkpoint');
      result=publish({root,checkpoint:value('--checkpoint'),config});
    }else fail('Use capture or publish');
    console.log(JSON.stringify(result,null,2));
  }catch(e){console.error('NATIVE_RECOVERY_FAIL: '+e.message);process.exitCode=1;}
}
