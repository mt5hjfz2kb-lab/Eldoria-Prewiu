import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {prepareInvocation,verifyArtistConsumption} from './run-artist.mjs';

const request='pipeline/art-production-requests/eldoria-tripo-art-brain-phase2-20261021.json';
test('existing Qwen author consumes selected memory via its original --inventory argument',()=>{
 const dir=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-qwen-bridge-'));
 try{
  const p=prepareInvocation({
   requestFile:request,prefix:path.join(dir,'engendro'),
   artistArgs:['--reference','references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg',
    '--unity-before','test-actual-frame.png','--target-object','Spawnling_Near_Authored',
    '--cpu-compact','--subject-kind','creature','--out',dir+'/qwen-output']
  });
  assert.equal(p.command[1],'tools/art-direction/ai-authoring/author_brief.py');
  assert.equal(p.command.at(-2),'--inventory');
  assert.equal(p.command.at(-1),p.inventory);
  assert.ok(p.selected.selected_ids.length>0);
  assert.match(fs.readFileSync(p.inventory,'utf8'),/ELDORIA HISTORICAL ART KNOWLEDGE/);
  assert.equal(p.selected.qa_boundary.qwen_has_received_inventory,false);
  assert.throws(()=>verifyArtistConsumption(p),/inference evidence not found/);
 }finally{fs.rmSync(dir,{recursive:true,force:true});}
});
test('Qwen adapter refuses overrides or incomplete inputs',()=>{
 const dir=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-qwen-deny-'));
 try{
  for(const args of [
   ['--reference','fake','--unity-before','fake','--target-object','object','--out','output','--inventory','unsafe.txt'],
   ['--out','output','--target-object','object']
  ]){
   assert.throws(()=>prepareInvocation({requestFile:request,prefix:path.join(dir,'refuse'),artistArgs:args}));
  }
 }finally{fs.rmSync(dir,{recursive:true,force:true});}
});
