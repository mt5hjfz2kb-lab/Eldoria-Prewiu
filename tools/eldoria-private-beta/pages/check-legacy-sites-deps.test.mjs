import test from 'node:test';
import assert from 'node:assert/strict';
import {auditLegacyShell,extractDependencies} from './check-legacy-sites-deps.mjs';
const site='https://historic.example.test/';
const sha='be49d62e3daf490f41dba0bf8123a8e2eee64480';
const base='https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/beta-native325/';
const manifest={
 loader:base+'Build/old-loader.js',framework:base+'Build/old-framework.js',
 code:base+'Build/old.wasm.unityweb',data:base+'Build/old.data.unityweb',
 decoded_parts:{[base+'Build/old.data.unityweb']:[{url:base+'payload-decoded/old-0.gz'},{url:base+'payload-decoded/old-30.gz'}]}
};
const mock=(code=()=>200)=>async url=>{
 if(url===site+'release-identity.json')return {ok:true,json:async()=>({revision:'native325-mobile10-pretest-ux',source_sha:sha})};
 if(url===site+'build-manifest.json')return {ok:true,json:async()=>manifest};
 const status=code(url);return {ok:status>=200&&status<300,status};
};
test('The old shell references version-specific WASM and data, never implicit latest',()=>{
 const deps=extractDependencies(manifest,site+'build-manifest.json');
 assert.equal(deps.length,5);
 assert.ok(deps.some(d=>d.url.endsWith('/Build/old.wasm.unityweb')));
 assert.ok(deps.some(d=>d.url.endsWith('/payload-decoded/old-0.gz')));
});
test('Only all-present original assets count as an intact legacy release',async()=>{
 const r=await auditLegacyShell({fetchImpl:mock(),base:site});
 assert.equal(r.pass,true);assert.equal(r.missing.length,0);
});
test('Detects removed old assets even if latest Pages Unity succeeds',async()=>{
 const r=await auditLegacyShell({fetchImpl:mock(url=>/old\.wasm|old-0/.test(url)?404:200),base:site});
 assert.equal(r.pass,false);
 assert.equal(r.missing.length,2);
 assert.ok(r.missing.every(d=>d.status===404));
});
test('Rejects a stale link masquerading as beta10 by query parameters',async()=>{
 const altered=async u=>u===site+'release-identity.json'?
   {ok:true,json:async()=>({revision:'native325-mobile11-building-anchor',source_sha:'new'})}:mock()(u);
 await assert.rejects(auditLegacyShell({fetchImpl:altered,base:site}),/immutable build/);
});
test('Never fetches unknown dependency hosts or missing source chunks',()=>{
 assert.throws(()=>extractDependencies({...manifest,loader:'https://random.invalid/private'},site+'build-manifest.json'),/Unexpected legacy dependency host/);
 assert.throws(()=>extractDependencies({...manifest,decoded_parts:{}},site+'build-manifest.json'),/Missing original data chunks/);
});
