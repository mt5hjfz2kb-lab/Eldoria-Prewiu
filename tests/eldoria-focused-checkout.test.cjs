'use strict';
const test=require('node:test');
const assert=require('node:assert/strict');
const fs=require('node:fs');
const os=require('node:os');
const path=require('node:path');
const {execFileSync}=require('node:child_process');
test('focused checkout removes stale game/output bytes and preserves only generated Library',()=>{
 const root=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-checkout-'));
 const git=(...args)=>execFileSync('git',args,{cwd:root,stdio:'pipe'});
 const write=(file,content)=>{fs.mkdirSync(path.dirname(path.join(root,file)),{recursive:true});fs.writeFileSync(path.join(root,file),content);};
 try{
  git('init');git('config','user.name','checkout-test');git('config','user.email','test@example.invalid');
  write('Unity/Assets/Runtime.cs','approved source');write('.gitignore','Unity/Library/\nUnity/Builds/\n');
  git('add','.');git('commit','-m','approved');const sha=git('rev-parse','HEAD').toString().trim();
  write('Unity/Assets/Runtime.cs','stale tracked edit');write('Unity/Library/Bee/cache','warm generated cache');
  write('Unity/Builds/WebGL/index.html','stale binary');write('Unity/Assets/Rogue.cs','untracked injected source');write('old-qa.json','stale evidence');
  git('reset','--hard',sha);git('clean','-ffdx','-e','Unity/Library/');
  assert.equal(fs.readFileSync(path.join(root,'Unity/Library/Bee/cache'),'utf8'),'warm generated cache');
  assert.equal(fs.readFileSync(path.join(root,'Unity/Assets/Runtime.cs'),'utf8'),'approved source');
  for(const stale of ['Unity/Builds','Unity/Assets/Rogue.cs','old-qa.json'])assert.equal(fs.existsSync(path.join(root,stale)),false,stale);
  git('diff','--exit-code',sha,'--','Unity');
 }finally{fs.rmSync(root,{recursive:true,force:true});}
});
