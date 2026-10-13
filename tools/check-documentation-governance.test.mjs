import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {auditDocs,localLinks} from './check-documentation-governance.mjs';

function fixture() {
  const root=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-doc-gate-'));
  const files=['AGENTS.md','README.md','docs/DOCUMENTATION_INDEX.md','docs/DOCUMENTATION_POLICY.md','docs/WORKFLOW_INDEX.md','docs/ELDORIA_ART_PIPELINE_INDEX.md'];
  for(const file of files){ fs.mkdirSync(path.dirname(path.join(root,file)),{recursive:true});fs.writeFileSync(path.join(root,file),file.includes('ELDORIA_ART_PIPELINE_INDEX')?'HISTÓRICO\n':'# Fixture\n'); }
  const entries=files.map(p=>({path:p,domain:'fixture',status:p.endsWith('ELDORIA_ART_PIPELINE_INDEX.md')?'historical':'current'}));
  fs.writeFileSync(path.join(root,'docs/documentation-catalog.json'),JSON.stringify({schema_version:1,entries}));
  return {root,files,entries,dispose:()=>fs.rmSync(root,{recursive:true,force:true})};
}

test('real repository catalog is internally consistent',()=>{
  const result=auditDocs(path.resolve(path.dirname(new URL(import.meta.url).pathname),'..'));
  assert.deepEqual(result.errors,[],result.errors.join('\n'));
  assert.ok(result.counts.catalogued>100);
});
test('simple valid miniature catalog passes',()=>{const f=fixture();try{assert.deepEqual(auditDocs(f.root).errors,[]);}finally{f.dispose();}});
test('detects unindexed direct docs',()=>{const f=fixture();try{fs.writeFileSync(path.join(f.root,'docs/new.md'),'# new');assert.match(auditDocs(f.root).errors.join('\n'),/not inventoried/);}finally{f.dispose();}});
test('rejects duplicate/invalid lifecycle entry',()=>{const f=fixture();try{const p=path.join(f.root,'docs/documentation-catalog.json'),o=JSON.parse(fs.readFileSync(p));o.entries.push({...o.entries[0],status:'approved_by_ci'});fs.writeFileSync(p,JSON.stringify(o));const e=auditDocs(f.root).errors.join('\n');assert.match(e,/Duplicate/);assert.match(e,/Unsupported/);}finally{f.dispose();}});
test('rejects broken local link in entry page',()=>{const f=fixture();try{fs.writeFileSync(path.join(f.root,'README.md'),'[Missing](docs/gone.md)');assert.match(auditDocs(f.root).errors.join('\n'),/Broken start-page link/);}finally{f.dispose();}});
test('rejects path traversal in catalog',()=>{const f=fixture();try{const p=path.join(f.root,'docs/documentation-catalog.json'),o=JSON.parse(fs.readFileSync(p));o.entries.push({path:'../outside.md',status:'current',domain:'oops'});fs.writeFileSync(p,JSON.stringify(o));assert.match(auditDocs(f.root).errors.join('\n'),/Unsafe/);}finally{f.dispose();}});
test('link parser ignores remote links and fragments',()=>{assert.deepEqual(localLinks('[a](https://x.com) [b](#section) [c](./docs/doc.md#anchor)'),['./docs/doc.md']);});
