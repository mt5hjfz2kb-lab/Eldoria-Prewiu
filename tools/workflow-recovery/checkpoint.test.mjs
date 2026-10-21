import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import os from 'node:os';
import {execFileSync} from 'node:child_process';
import {capture,publish} from './checkpoint.mjs';

function fixture(){
  const temp=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-recovery-test-'));
  const root=path.join(temp,'work'),remote=path.join(temp,'remote.git'),peer=path.join(temp,'peer');
  const command=(cwd,args)=>execFileSync('git',args,{cwd,encoding:'utf8',stdio:['pipe','pipe','pipe']}).trim();
  const git=(...args)=>command(root,args);
  const write=(base,p,b)=>{const full=path.join(base,p);fs.mkdirSync(path.dirname(full),{recursive:true});fs.writeFileSync(full,b);};
  fs.mkdirSync(root);command(temp,['init','--bare',remote]);git('init','-b','feature');
  for(const base of [root]){command(base,['config','user.name','Fixture']);command(base,['config','user.email','fixture@example.invalid']);}
  write(root,'pipeline/active-workstreams.json',JSON.stringify({active:[{id:'native-fixture',branch:'feature',status:'active'}],history:[]}));
  write(root,'Unity/scene.unity','original scene\n');write(root,'Unity/delete.asset','original delete\n');
  write(root,'Unity/settings.asset','original settings\n');write(root,'docs/readme.md','old\n');
  git('add','.');git('commit','-m','baseline');git('remote','add','origin',remote);git('push','-u','origin','feature');
  command(temp,['clone','--branch','feature',remote,peer]);command(peer,['config','user.name','Peer']);command(peer,['config','user.email','peer@example.invalid']);
  const config={schema_version:1,workstream_id:'native-fixture',branch:'feature',outputs:['Unity/scene.unity','Unity/delete.asset','Unity/new.bin'],inputs:['Unity','tools']};
  return {temp,root,peer,git,write,config,checkpoint:path.join(temp,'checkpoint'),command,dispose:()=>fs.rmSync(temp,{recursive:true,force:true})};
}

test('publish exact binary/new/deleted outputs despite unrelated dirty editor settings, without touching worktree or index',()=>{
  const f=fixture();try{
    const head=f.git('rev-parse','HEAD');
    f.write(f.root,'Unity/scene.unity','validated scene\r\n');f.write(f.root,'Unity/new.bin',Buffer.from([0,1,255,128,10]));
    fs.unlinkSync(path.join(f.root,'Unity/delete.asset'));
    f.write(f.root,'Unity/settings.asset','editor residue must survive\n');
    f.git('add','Unity/settings.asset');
    const staged=f.git('write-tree'),status=f.git('status','--porcelain');
    const manifest=capture({root:f.root,out:f.checkpoint,config:f.config,validated:true,sourceSha:head,runId:'123'});
    assert.equal(manifest.files.length,3);
    const result=publish({root:f.root,checkpoint:f.checkpoint,config:f.config});
    assert.equal(result.status,'PUBLISHED');
    assert.equal(f.git('rev-parse','HEAD'),head);assert.equal(f.git('write-tree'),staged);
    assert.equal(f.git('status','--porcelain'),status);
    assert.equal(fs.readFileSync(path.join(f.root,'Unity/settings.asset'),'utf8'),'editor residue must survive\n');
    const raw=execFileSync('git',['show',result.commit+':Unity/new.bin'],{cwd:f.root});
    assert.deepEqual(raw,Buffer.from([0,1,255,128,10]));
    assert.equal(execFileSync('git',['show',result.commit+':Unity/scene.unity'],{cwd:f.root}).toString(),'validated scene\r\n');
    assert.equal(f.git('show',result.commit+':Unity/settings.asset'),'original settings');
    assert.ok(!f.git('ls-tree','-r','--name-only',result.commit).includes('delete.asset'));
    assert.equal(publish({root:f.root,checkpoint:f.checkpoint,config:f.config}).status,'ALREADY_PRESENT');
  }finally{f.dispose();}
});

test('preserve concurrent documentation commits and refuse concurrent runtime/output changes',()=>{
  const f=fixture();try{
    f.write(f.root,'Unity/scene.unity','validated output\n');
    capture({root:f.root,out:f.checkpoint,config:f.config,validated:true});
    f.write(f.peer,'docs/readme.md','peer document change\n');
    f.command(f.peer,['add','.']);f.command(f.peer,['commit','-m','docs']);f.command(f.peer,['push']);
    const result=publish({root:f.root,checkpoint:f.checkpoint,config:f.config});
    assert.equal(f.git('show',result.commit+':docs/readme.md'),'peer document change');
    f.command(f.peer,['pull','--ff-only']);
    f.write(f.root,'Unity/scene.unity','second validated output\n');
    const next=path.join(f.temp,'next');capture({root:f.root,out:next,config:f.config,validated:true});
    f.write(f.peer,'Unity/settings.asset','other runtime edit\n');
    f.command(f.peer,['add','.']);f.command(f.peer,['commit','-m','runtime']);f.command(f.peer,['push']);
    assert.throws(()=>publish({root:f.root,checkpoint:next,config:f.config}),/Protected source changed/);
    assert.equal(fs.readFileSync(path.join(next,'files/Unity/scene.unity'),'utf8'),'second validated output\n');
  }finally{f.dispose();}
});

test('a concurrent change to the same generated file is never overwritten',()=>{
  const f=fixture();try{
    f.write(f.root,'Unity/scene.unity','ours\n');capture({root:f.root,out:f.checkpoint,config:f.config,validated:true});
    f.write(f.peer,'Unity/scene.unity','theirs\n');f.command(f.peer,['add','.']);f.command(f.peer,['commit','-m','other']);f.command(f.peer,['push']);
    assert.throws(()=>publish({root:f.root,checkpoint:f.checkpoint,config:f.config}),/Concurrent output edit/);
    assert.ok(fs.existsSync(path.join(f.checkpoint,'checkpoint.json')));
  }finally{f.dispose();}
});

test('a rejected push preserves the original checkpoint and recovery succeeds without rerunning native work',()=>{
  const f=fixture();try{
    f.write(f.root,'Unity/scene.unity','expensive validated result\n');
    capture({root:f.root,out:f.checkpoint,config:f.config,validated:true});
    const originalManifest=fs.readFileSync(path.join(f.checkpoint,'checkpoint.json'));
    const hook=path.join(f.temp,'remote.git/hooks/pre-receive');
    fs.writeFileSync(hook,'#!/bin/sh\nexit 1\n');fs.chmodSync(hook,0o755);
    assert.throws(()=>publish({root:f.root,checkpoint:f.checkpoint,config:f.config,retries:2}),/checkpoint retained/);
    assert.deepEqual(fs.readFileSync(path.join(f.checkpoint,'checkpoint.json')),originalManifest);
    fs.unlinkSync(hook);
    const recovered=publish({root:f.root,checkpoint:f.checkpoint,config:f.config});
    assert.equal(recovered.status,'PUBLISHED');
    assert.equal(f.git('show',recovered.commit+':Unity/scene.unity'),'expensive validated result');
  }finally{f.dispose();}
});

test('failed QA, altered bytes, duplicate paths and missing ownership block publication',()=>{
  const f=fixture();try{
    f.write(f.root,'Unity/scene.unity','result\n');capture({root:f.root,out:f.checkpoint,config:f.config,validated:false});
    assert.throws(()=>publish({root:f.root,checkpoint:f.checkpoint,config:f.config}),/QA must pass/);
    const valid=path.join(f.temp,'valid');capture({root:f.root,out:valid,config:f.config,validated:true});
    f.write(valid,'files/Unity/scene.unity','tampered\n');
    assert.throws(()=>publish({root:f.root,checkpoint:valid,config:f.config}),/bytes changed/);
    const valid2=path.join(f.temp,'valid2');capture({root:f.root,out:valid2,config:f.config,validated:true});
    f.write(f.peer,'pipeline/active-workstreams.json',JSON.stringify({active:[],history:[]}));f.command(f.peer,['add','.']);f.command(f.peer,['commit','-m','close']);f.command(f.peer,['push']);
    assert.throws(()=>publish({root:f.root,checkpoint:valid2,config:f.config}),/must remain active/);
    assert.throws(()=>capture({root:f.root,out:valid2,config:f.config}),/EEXIST/);
  }finally{f.dispose();}
});

test('traversal and symlink output paths cannot enter a checkpoint',()=>{
  const f=fixture();try{
    assert.throws(()=>capture({root:f.root,out:f.checkpoint,config:{...f.config,outputs:['../outside']}}),/Unsafe/);
    fs.symlinkSync(path.join(f.root,'docs/readme.md'),path.join(f.root,'Unity/new.bin'));
    assert.throws(()=>capture({root:f.root,out:f.checkpoint,config:f.config}),/regular output/);
    assert.ok(!fs.existsSync(f.checkpoint));
  }finally{f.dispose();}
});
