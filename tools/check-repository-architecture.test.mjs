import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {execFileSync,spawnSync} from 'node:child_process';
import {fileURLToPath} from 'node:url';

const script=fileURLToPath(new URL('./check-repository-architecture.mjs',import.meta.url));
test('sparse CI checks committed structure; local mode checks deletions; committed losses fail',()=>{
  const root=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-structure-'));
  const git=(...args)=>execFileSync('git',args,{cwd:root,stdio:'pipe'});
  const write=(p,v)=>{fs.mkdirSync(path.dirname(path.join(root,p)),{recursive:true});fs.writeFileSync(path.join(root,p),v);};
  const run=(...args)=>spawnSync(process.execPath,[script,...args],{cwd:root,encoding:'utf8'});
  try {
    git('init');git('config','user.name','Fixture');git('config','user.email','fixture@example.invalid');
    write('pipeline/repository-architecture-invariants.json',JSON.stringify({requiredFiles:['Unity/a.glb'],requiredDirectories:['Unity'],minimumFileCounts:{Unity:2},registry:{path:'pipeline/active-workstreams.json',requiredArrays:['active','history']}}));
    write('pipeline/active-workstreams.json',JSON.stringify({active:[],history:[]}));
    write('Unity/a.glb','binary fixture');write('Unity/b.cs','source');
    git('add','.');git('commit','-m','fixture');
    assert.equal(run().status,0);
    assert.equal(run('--tracked-tree').status,0);
    fs.unlinkSync(path.join(root,'Unity/a.glb'));
    assert.equal(run('--tracked-tree').status,0,'omitted binary body does not change committed structure');
    assert.notEqual(run().status,0,'local uncommitted deletion is detected');
    git('add','-u');git('commit','-m','delete required asset');
    assert.notEqual(run('--tracked-tree').status,0,'committed deletion must fail');
    write('Unity/a.glb','untracked replacement');
    assert.notEqual(run('--tracked-tree').status,0,'untracked files cannot mask committed losses');
    git('add','.');git('commit','-m','restore');
    write('pipeline/active-workstreams.json',JSON.stringify({active:[{id:'duplicate',status:'active'},{id:'duplicate',status:'active'}],history:[]}));
    assert.notEqual(run('--tracked-tree').status,0,'registry validation remains active');
  } finally {fs.rmSync(root,{recursive:true,force:true});}
});
