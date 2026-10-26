import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {assembleArtistPromptFromFile,writeArtistPrompt} from './build-prompt.mjs';
import {verifyBriefUse} from './verify-brief-use.mjs';

const here=path.dirname(fileURLToPath(import.meta.url));
const db=path.join(here,'observations.json');
const input=Buffer.from('Create a carefully reviewed technical art proposal with real geometry changes, independent A/B, and protected gameplay.');
test('existing memory becomes part of the saved prompt and cryptographically verifies',()=>{
  const dir=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-memory-prompt-'));
  try{
    const base=path.join(dir,'brief.txt'),out=path.join(dir,'artist.txt');
    fs.writeFileSync(base,input);
    const result=writeArtistPrompt({basePath:base,dbPath:db,output:out,target:'bastion1'});
    assert.equal(result.status,'BRIEF_IN_PROMPT_FILE_VERIFIED');
    assert.deepEqual(fs.readFileSync(base),input);
    const prompt=fs.readFileSync(out);
    assert.match(prompt.toString(),/UNTRUSTED HISTORICAL ADVICE/);
    assert.match(prompt.toString(),/current owner decisions/);
    const confirmed=verifyBriefUse({prompt,brief:fs.readFileSync(result.lesson_file),
      provenance:fs.readFileSync(result.provenance_file),catalog:fs.readFileSync(db)});
    assert.equal(confirmed.target,'bastion1');
    assert.throws(()=>writeArtistPrompt({basePath:base,dbPath:db,output:out}),/overwrite/);
  }finally{fs.rmSync(dir,{recursive:true,force:true});}
});
test('changed catalog or missing memory cannot be mislabelled as completed artist input',()=>{
  const result=assembleArtistPromptFromFile({basePrompt:input,dbPath:db,target:'bastion1'});
  const bad=Buffer.from(result.prompt.toString().replace(result.lessons.toString(),''));
  assert.throws(()=>verifyBriefUse({prompt:bad,brief:result.lessons,
    provenance:Buffer.from(JSON.stringify(result.provenance)),catalog:fs.readFileSync(db)}),
    /Verbatim memory brief missing/);
});
test('unknown target and incomplete prompt fail safely without new files',()=>{
  assert.throws(()=>assembleArtistPromptFromFile({basePrompt:input,dbPath:db,target:'never_seen_2026'}),
    /No verified\/qualified historical observations/);
  assert.throws(()=>assembleArtistPromptFromFile({basePrompt:Buffer.from('short'),dbPath:db}),
    /Missing or unbounded base prompt/);
});
