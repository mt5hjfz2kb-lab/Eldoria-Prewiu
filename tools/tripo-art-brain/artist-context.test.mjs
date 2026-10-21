/**
 * CONTRACT TEST ONLY: mock loopback Ollama, actual pinned Tripo source bytes and
 * real archived UnityQA images. It is NOT a Qwen inference or art certification.
 */
import { test } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import http from 'node:http';
import crypto from 'node:crypto';
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { makeBundle } from './preflight.mjs';

const ROOT=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../..');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');

async function verifyExistingQwenContract(id,kind,{compact=false}={}) {
  const base=path.join(ROOT,'ArtSource/World4X',
    id==='spawnling'?'Spawnling_v1':'RiftFocus_v1','UnityQA');
  const reference=path.join(base,'front.png');
  const unityBefore=path.join(base,'strategic.png');
  const actualBytes=[fs.readFileSync(reference),fs.readFileSync(unityBefore)];
  for(const image of actualBytes) assert.equal(image.subarray(0,8).toString('hex'),'89504e470d0a1a0a');
  const tmp=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-qwen-contract-'));
  const inventory=path.join(tmp,'qwen-inventory.txt');
  const output=path.join(tmp,'inference');
  const realContext=(id==='spawnling'&&kind==='creature')?process.env.TRIPO_SOURCE_MESH_CONTEXT:null;
  const target=realContext?'Spawnling_Near_Authored':'Eldoria_SourceMesh_ContractOnly';
  fs.writeFileSync(inventory,makeBundle(ROOT,id).context);
  const received=[];
  const srv=http.createServer((req,res)=>{
    if(req.url!=='/api/chat'||req.method!=='POST'){
      res.writeHead(404);res.end('not found');return;
    }
    const chunks=[];
    req.on('data',z=>chunks.push(z));
    req.on('end',()=>{
      try {
        const body=JSON.parse(Buffer.concat(chunks).toString('utf8'));
        received.push(body);
        const plan={
          observed_defect:'The current fantasy asset has a camera-visible silhouette defect requiring real modelling review.',
          design_intent:'Improve the existing subject silhouette with one bounded spatial Blender edit while preserving the original.',
          asset_decision:'MODIFY',
          planned_actions:[{
            id:'contract-one',type:'sculpt_region',target,
            rationale:'Representative typed edit for contract only; never executed in Blender',
            parameters:{center:[0,0,0],radius:0.4,delta:[0,0.06,0.03]}
          }]
        };
        res.writeHead(200,{'content-type':'application/json'});
        res.end(JSON.stringify({message:{role:'assistant',content:JSON.stringify(plan)},
          eval_count:8,prompt_eval_count:128,done_reason:'stop'}));
      } catch(err){res.writeHead(400);res.end(String(err));}
    });
  });
  try {
    await new Promise((resolve,reject)=>{srv.once('error',reject);srv.listen(0,'127.0.0.1',resolve);});
    const port=srv.address().port;
    const args=[
      'tools/art-direction/ai-authoring/author_brief.py',
      '--reference',reference,'--unity-before',unityBefore,
      '--target-object',target,
      '--target-mode','existing','--inventory',inventory,
      '--out',output,'--endpoint','http://127.0.0.1:'+port+'/api/chat',
      '--repair-attempts','0','--max-actions','2'
    ];
    if(kind!=='terrain') args.push('--subject-kind',kind);
    if(compact)args.push('--cpu-compact');
    if(realContext) args.push('--mesh-context',realContext);
    const spawned=await new Promise((resolve,reject)=>{
      const child=spawn('python3',args,{cwd:ROOT,env:{...process.env,TRIPO_API_KEY:''}});
      let stdout='',stderr='';
      child.stdout.on('data',z=>stdout+=z);
      child.stderr.on('data',z=>stderr+=z);
      child.once('error',reject);
      child.once('close',code=>resolve({code,stdout,stderr}));
      setTimeout(()=>child.kill('SIGKILL'),20000).unref();
    });
    assert.equal(spawned.code,0,spawned.stderr||spawned.stdout);
    assert.equal(received.length,1);
    const body=received[0];
    assert.equal(body.model,'qwen3-vl:4b-instruct');
    assert.equal(body.messages[0].role,'system');
    assert.match(body.messages[0].content,/ELDORIA|VALORIA/);
    assert.equal(body.messages[1].role,'user');
    for(let i=0;i<2;i++){
      assert.equal(sha(Buffer.from(body.messages[1].images[i],'base64')),sha(actualBytes[i]));
    }
    const prompt=body.messages[1].content;
    assert.match(prompt,/SHA256/);
    assert.match(prompt,/Existing|existing/);
    if(kind==='creature'){
      assert.match(prompt,/CREATURE ART:/);
      if(realContext){
        const raw=fs.readFileSync(realContext);
        const actual=JSON.parse(raw);
        assert.equal(actual.target,target);
        assert.match(prompt,/local_aabb/);
        assert.match(prompt,new RegExp(actual.source_sha256));
      }
      assert.match(prompt,/skeletal deformation/);
      assert.doesNotMatch(prompt,/GENUINE UNITY V5 cliff|existing cliff rock face/);
    }
    if(kind==='mineral_prop'){
      assert.match(prompt,/MINERAL ENVIRONMENT PROP ART:/);
      assert.match(prompt,/ground contact/);
      assert.doesNotMatch(prompt,/GENUINE UNITY V5 cliff|existing cliff rock face/);
    }
    if(kind==='terrain'){
      assert.match(prompt,/cliff\/road edges/);
      assert.doesNotMatch(prompt,/CREATURE ART:|MINERAL ENVIRONMENT PROP ART:/);
    }
    const evidence=JSON.parse(fs.readFileSync(path.join(output,'artist-inference.json'),'utf8'));
    assert.equal(evidence.self_approval,false);
    assert.equal(evidence.reference_sha256,sha(actualBytes[0]));
    assert.equal(evidence.unity_before_sha256,sha(actualBytes[1]));
    assert.equal(evidence.asset_inventory_sha256,sha(Buffer.from(fs.readFileSync(inventory,'utf8').slice(0,compact?1800:4000))));
    assert.equal(evidence.cpu_compact,compact);
    if(realContext)assert.equal(evidence.mesh_context_sha256,sha(fs.readFileSync(realContext)));
    if(kind==='terrain')assert.ok(!('subject_kind' in evidence));
    else assert.equal(evidence.subject_kind,kind);
    const plan=JSON.parse(fs.readFileSync(path.join(output,'artist-plan.json'),'utf8'));
    assert.equal(plan.length,1);
    assert.equal(plan[0].target,target);
  } finally{
    await new Promise(resolve=>srv.close(resolve));
    fs.rmSync(tmp,{recursive:true,force:true});
  }
}

test('MOCK ONLY: audited Engendro assets reach the real Qwen CLI as creature context, with both genuine archived images',async()=>{
  await verifyExistingQwenContract('spawnling','creature');
});
test('MOCK ONLY: mineral focus uses source-aware art directions, never Valoria cliff instructions',async()=>{
  await verifyExistingQwenContract('focus','mineral_prop');
});
test('MOCK ONLY: legacy terrain prompt contract remains available and unchanged by default',async()=>{
  await verifyExistingQwenContract('spawnling','terrain');
});

test('MOCK ONLY: small animated creature compact schema never inherits 1-metre Valoria cliff limits',async()=>{
  await verifyExistingQwenContract('spawnling','creature',{compact:true});
});
