import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {prepare} from './cycle-hooks.mjs';
import {verifyBriefUse} from './verify-brief-use.mjs';

const here=path.dirname(fileURLToPath(import.meta.url));
const catalog=fs.readFileSync(path.join(here,'observations.json'));
const prepared=prepare({target:'bastion1'});
const source=()=>({brief:Buffer.from(prepared.message),provenance:Buffer.from(JSON.stringify(prepared.provenance)),
  catalog,prompt:Buffer.from('### Verified history\n'+prepared.message+'\n### New task\nEdit only the active asset.')});

test('actual current learning brief passes only as prompt-file provenance, not art approval',()=>{
  const result=verifyBriefUse(source());
  assert.equal(result.status,'BRIEF_IN_PROMPT_FILE_VERIFIED');
  assert.equal(result.target,'bastion1');
  assert.match(result.limitations.join(' '),/does NOT prove that a model ran/);
});
test('missing brief in actual prompt fails closed',()=>{
  assert.throws(()=>verifyBriefUse({...source(),prompt:Buffer.from('Generate without prior lessons')}),/Verbatim memory brief missing/);
});
test('altered history, digest or newer reference prevents false reuse',()=>{
  assert.throws(()=>verifyBriefUse({...source(),brief:Buffer.from(prepared.message+'x')}),/SHA mismatch/);
  const changed=JSON.parse(catalog.toString());
  changed.current_reference_sha256='b'.repeat(64);
  assert.throws(()=>verifyBriefUse({...source(),catalog:Buffer.from(JSON.stringify(changed))}),/catalog has changed/);
});
test('unknown evidence status and fake approval are refused',()=>{
  const p={...prepared.provenance,status:'ART_APPROVED'};
  assert.throws(()=>verifyBriefUse({...source(),provenance:Buffer.from(JSON.stringify(p))}),/does not authorize art approval/);
});
