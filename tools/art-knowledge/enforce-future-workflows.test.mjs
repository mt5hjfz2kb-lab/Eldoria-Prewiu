import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {checkNewQwenWorkflows} from './enforce-future-workflows.mjs';
test('new workflows cannot bypass memory; legacy owners left untouched',()=>{
 const d=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-workflow-policy-'));
 try{
  fs.writeFileSync(path.join(d,'new.yml'),'run: python3 tools/art-direction/ai-authoring/author_brief.py --out=/tmp/x');
  fs.writeFileSync(path.join(d,'legacy.yml'),'run: python3 tools/art-direction/ai-authoring/author_brief.py');
  let errs=checkNewQwenWorkflows(d,['legacy.yml']);
  assert.deepEqual(errs,['New Qwen workflow bypasses art-memory adapter: new.yml']);
  fs.writeFileSync(path.join(d,'new.yml'),'run: node tools/art-knowledge/run-artist.mjs --run -- python3 tools/art-direction/ai-authoring/author_brief.py');
  assert.match(checkNewQwenWorkflows(d,['legacy.yml']).join(),/must preserve/);
  fs.writeFileSync(path.join(d,'new.yml'),'run: node tools/art-knowledge/run-artist.mjs --run -- python3 tools/art-direction/ai-authoring/author_brief.py\nuses: actions/upload-artifact@v4\npath: /tmp/job/*.cycle-feedback.json');
  assert.deepEqual(checkNewQwenWorkflows(d,['legacy.yml']),[]);
 }finally{fs.rmSync(d,{force:true,recursive:true});}
});
