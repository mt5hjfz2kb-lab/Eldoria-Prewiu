import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync,mkdtempSync,mkdirSync,writeFileSync,rmSync} from 'node:fs';
import {tmpdir} from 'node:os';
import {join} from 'node:path';
import {spawnSync} from 'node:child_process';
import {validateUploadManifest,uploadCommands} from '../verify-r2-files.mjs';
const manifest=JSON.parse(readFileSync(new URL('../../../pipeline/eldoria-private-beta/r2-upload-manifest.json',import.meta.url),'utf8'));
const script=new URL('./deployment-plan.mjs',import.meta.url).pathname;
const codes=uploadCommands(manifest,'/tmp/readonly-webgl-source');
test('native325 manifest is precise and rejects future/editable content',()=>{
 assert.equal(validateUploadManifest(manifest),true);
 assert.equal(manifest.files.length,19);
 assert.equal(manifest.total_bytes,490317080);
 assert.equal(manifest.release,'native325-fb1292b3d9ce');
 assert.equal(manifest.access,'private-r2-only');
 assert.equal(manifest.deployment_status,'NOT_DEPLOYED');
 assert.equal(validateUploadManifest({...manifest,release:'native326'}),false);
 assert.equal(validateUploadManifest({...manifest,access:'public'}),false);
 assert.equal(validateUploadManifest({...manifest,deployment_status:'DEPLOYED'}),false);
});
test('upload commands preserve exact MIME and compressed encoding',()=>{
 assert.equal(codes.length,19);
 for(let i=0;i<codes.length;i++){
  const {program,args}=codes[i],file=manifest.files[i];
  assert.equal(program,'wrangler');
  assert.deepEqual(args.slice(0,3),['r2','object','put']);
  assert.ok(args[3].startsWith('eldoria-beta-privada/releases/native325-fb1292b3d9ce/'));
  assert.equal(args[args.indexOf('--content-type')+1],file.content_type);
  assert.equal(args.includes('--content-encoding'),Boolean(file.content_encoding));
  if(file.content_encoding)
   assert.equal(args[args.indexOf('--content-encoding')+1],file.content_encoding);
  assert.ok(args.includes('--remote'));
  assert.ok(args.includes('--file'));
  assert.equal(args.includes('r2.dev'),false);
 }
});
test('deployment is default no-network and fails closed if data missing',()=>{
 const root=mkdtempSync(join(tmpdir(),'beta-r2-no-network-'));
 try{
  const file=join(root,'manifest.json');
  writeFileSync(file,JSON.stringify(manifest));
  const r=spawnSync(process.execPath,[script,join(root,'empty'),file],{encoding:'utf8',env:{...process.env,ELDORIA_BETA_R2_UPLOAD_APPROVED:'yes'},timeout:8000});
  assert.notEqual(r.status,0);
  assert.doesNotMatch(r.stdout,/PRIVATE_R2_19_FILES_EXACT_REMOTE_READBACK_PASS/);
  assert.doesNotMatch(r.stdout,/upload_enabled":true/);
 }finally{rmSync(root,{recursive:true,force:true});}
});
test('flags cannot be passed as accidental upload or skipped gate',()=>{
 const root=mkdtempSync(join(tmpdir(),'beta-r2-extra-'));
 try{
  const file=join(root,'manifest.json');
  writeFileSync(file,JSON.stringify(manifest));
  const r=spawnSync(process.execPath,[script,root,file,'--publish'],{encoding:'utf8',timeout:8000});
  assert.notEqual(r.status,0);
 }finally{rmSync(root,{recursive:true,force:true});}
});
