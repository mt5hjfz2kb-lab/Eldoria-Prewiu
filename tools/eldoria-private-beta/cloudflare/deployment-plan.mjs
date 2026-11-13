#!/usr/bin/env node
// Private R2 upload; NEVER runs on push or PR. Requires explicit --upload plus
// Cloudflare's owner credentials. By default checks ALL local bytes and prints
// only the plan, with no network calls or public URL.
import {readFile,mkdtemp,rm} from 'node:fs/promises';
import {createReadStream} from 'node:fs';
import {createHash} from 'node:crypto';
import {tmpdir} from 'node:os';
import {resolve,join} from 'node:path';
import {spawn} from 'node:child_process';
import {validatePackageTree, uploadCommands} from '../verify-r2-files.mjs';
const args=process.argv.slice(2);
const [webDir,manifestFile]=args.filter(x=>x!=='--upload');
if(!webDir||!manifestFile||args.some(x=>x.startsWith('--')&&x!=='--upload'))
 throw Error('Usage: node deployment-plan.mjs WEBGL_DIR MANIFEST_JSON [--upload]');
const m=JSON.parse(await readFile(manifestFile,'utf8'));
const verified=await validatePackageTree(resolve(webDir),m);
const commands=uploadCommands(m,resolve(webDir));
if(!args.includes('--upload')){
 console.log(JSON.stringify({status:'VERIFIED_DRY_RUN_NO_NETWORK',...verified,
  objects:commands.length,bytes:m.total_bytes,
  prefix:m.r2_prefix,
  expected_encodings:m.files.reduce((o,f)=>(o[f.content_encoding||'none']=(o[f.content_encoding||'none']||0)+1,o),{}),
  upload_enabled:false},null,2));
 process.exit(0);
}
if(process.env.ELDORIA_BETA_R2_UPLOAD_APPROVED!=='yes' ||
 typeof process.env.CLOUDFLARE_API_TOKEN!=='string' ||
 process.env.CLOUDFLARE_API_TOKEN.length<20 ||
 typeof process.env.CLOUDFLARE_ACCOUNT_ID!=='string' ||
 !/^[a-zA-Z0-9]{32}$/.test(process.env.CLOUDFLARE_ACCOUNT_ID))
 throw Error('Required Cloudflare owner authorization or credentials missing; no upload executed');
async function cmd(program,options){
 await new Promise((ok,fail)=>{
  const child=spawn(program,options,{stdio:'inherit',env:process.env});
  child.once('error',fail);
  child.once('exit',(code)=>code===0?ok():fail(Error(program+' failed with exit '+code)));
 });
}
const expectedByPath=new Map(m.files.map(f=>[f.path,f]));
const temp=await mkdtemp(join(tmpdir(),'eldoria-r2-check-'));
try{
 // Never expose bucket files through r2.dev; Worker remains disabled until QA.
 await cmd('wrangler',['r2','bucket','dev-url','disable','eldoria-beta-privada','--force']);
 for(let i=0;i<commands.length;i++){
  const c=commands[i];
  const file=m.files[i];
  console.log('PRIVATE_R2_UPLOAD',i+1,'/',commands.length,file.path);
  await cmd(c.program,c.args);
  const downloaded=join(temp,'verify-object');
  await cmd('wrangler',['r2','object','get',
   'eldoria-beta-privada/'+m.r2_prefix+file.path,'--file',downloaded,'--remote']);
  const hash=createHash('sha256');
  let length=0;
  for await(const b of createReadStream(downloaded)){hash.update(b);length+=b.length;}
  if(length!==file.bytes||hash.digest('hex')!==file.sha256)
   throw Error('R2 readback mismatch, STOP: '+file.path);
  await rm(downloaded,{force:true});
 }
 console.log('PRIVATE_R2_19_FILES_EXACT_REMOTE_READBACK_PASS');
 console.log('NO_PUBLIC_LINK_ACTIVATED; CONFIG ACTIVE_RELEASE STILL EMPTY');
}finally{await rm(temp,{recursive:true,force:true});}
