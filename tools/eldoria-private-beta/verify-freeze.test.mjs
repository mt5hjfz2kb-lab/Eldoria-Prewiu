import test from 'node:test';
import assert from 'node:assert/strict';
import {mkdtemp,writeFile,rm,mkdir} from 'node:fs/promises';
import {tmpdir} from 'node:os';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {validateManifest,locatePackage} from './verify-freeze.mjs';
const pinned={schema:1,source_sha:'948ffa9837168a0b68dab80be76b66350422b23a',web_version:'native325-fb1292b3d9ce',
artifact_id:13583674142,artifact_run_id:42397405907,
artifact_workflow_head_sha:'e4dbedaff72963203b72448cb29cdcbfcf82e9e1',
inner_package_sha256:'fb1292b3d9ce4d6096d69fa18b417bc0a8cc6016f100defb3deaf29fbdb95771',
artifact_zip_digest:'sha256:2e1e34480eabd2f5134f09f2d17e57c98ca8d72a603cfe040549f218b789c80f',
distribution_enabled:false,archive_publication:'draft_only',
hosted_gameplay_verified:false,physical_mobile_tested:false,no_code_changes:true};
test('Pinned manifest exact',()=>assert.deepEqual(validateManifest(pinned),[]));
test('Cannot promote to testers',()=>assert.ok(validateManifest({...pinned,distribution_enabled:true}).length));
test('Cannot replace source or artifact',()=>{
 assert.ok(validateManifest({...pinned,source_sha:'0'.repeat(40)}).length);
 assert.ok(validateManifest({...pinned,inner_package_sha256:'0'.repeat(64)}).length);
});
test('Cannot claim physical or hosted proof',()=>{
 assert.ok(validateManifest({...pinned,physical_mobile_tested:true}).length);
 assert.ok(validateManifest({...pinned,hosted_gameplay_verified:true}).length);
});
test('Reject missing package without false PASS',async()=>{
 const d=await mkdtemp(join(tmpdir(),'beta-verify-'));
 try{await assert.rejects(locatePackage(d,pinned),/exactly one/);}
 finally{await rm(d,{recursive:true,force:true});}
});
test('Reject unexpected links in artifact',async()=>{
 const d=await mkdtemp(join(tmpdir(),'beta-links-'));
 try{
   await mkdir(join(d,'nested'));
   await writeFile(join(d,'nested','readme.txt'),'safe');
   // Small data will not accidentally match a 490MB package.
   await assert.rejects(locatePackage(d,pinned),/exactly one/);
 }finally{await rm(d,{recursive:true,force:true});}
});
test('Hash matches a real file only, not filename',async()=>{
 const d=await mkdtemp(join(tmpdir(),'beta-hash-'));
 try{
   const bytes=Buffer.alloc(100_000_001,17); // isolated synthetic artifact
   await writeFile(join(d,'fake.zip'),bytes);
   await assert.rejects(locatePackage(d,pinned),/exactly one/);
 }finally{await rm(d,{recursive:true,force:true});}
});
