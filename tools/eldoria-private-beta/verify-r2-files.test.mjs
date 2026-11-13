import test from 'node:test';
import assert from 'node:assert/strict';
import {validateUploadManifest,uploadCommands} from './verify-r2-files.mjs';
import {readFile} from 'node:fs/promises';
const m=JSON.parse(await readFile(new URL('../../pipeline/eldoria-private-beta/r2-upload-manifest.json',import.meta.url),'utf8'));
test('19 pinned WebGL objects and 490317080 bytes',()=>{
 assert.equal(validateUploadManifest(m),true);
 assert.equal(m.files.length,19);
});
test('no alternative game source or version admitted',()=>{
 assert.equal(validateUploadManifest({...m,gameplay_source_sha:'0'.repeat(40)}),false);
 assert.equal(validateUploadManifest({...m,release:'native326'}),false);
});
test('no missing or duplicated assets',()=>{
 assert.equal(validateUploadManifest({...m,files:m.files.slice(0,18)}),false);
 assert.equal(validateUploadManifest({...m,files:[...m.files.slice(0,18),m.files[0]]}),false);
});
test('Brotli metadata mandatory for 9 chunks',()=>{
 const altered=structuredClone(m);const f=altered.files.find(x=>x.path.includes('data-0.bin.br'));
 f.content_encoding=null;
 assert.equal(validateUploadManifest(altered),false);
});
test('Unity wasm and framework must be served as gzip',()=>{
 for(const suffix of ['.wasm.unityweb','.framework.js.unityweb']){
  const altered=structuredClone(m);
  altered.files.find(x=>x.path.endsWith(suffix)).content_encoding=null;
  assert.equal(validateUploadManifest(altered),false);
 }
});
test('reject path traversal and edited release totals',()=>{
 const altered=structuredClone(m);altered.files[0].path='../secret';
 assert.equal(validateUploadManifest(altered),false);
 assert.equal(validateUploadManifest({...m,total_bytes:m.total_bytes-1}),false);
});
test('metadata commands keep original bytes and mark remote R2',()=>{
 const cmds=uploadCommands(m,'/isolated/native325','eldoria-beta-privada');
 assert.equal(cmds.length,19);
 assert.ok(cmds.every(c=>c.program==='wrangler'&&c.args.includes('--remote')));
 assert.equal(cmds.filter(c=>c.args.includes('br')).length,9);
 assert.equal(cmds.filter(c=>c.args.includes('gzip')).length,2);
 assert.ok(cmds.every(c=>c.args.at(3).startsWith('eldoria-beta-privada/releases/native325-')));
});
