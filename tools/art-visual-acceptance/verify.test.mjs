import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import crypto from 'node:crypto';
import zlib from 'node:zlib';
import {validateManifest, runCli} from './verify.mjs';

const H=(x)=>crypto.createHash('sha256').update(x).digest('hex');
function crc32(buf){
  let c=0xffffffff;
  for(const b of buf){c^=b;for(let i=0;i<8;i++)c=c&1?(c>>>1)^0xedb88320:c>>>1;}
  return (c^0xffffffff)>>>0;
}
function chunk(tag,data){
  const len=Buffer.alloc(4);len.writeUInt32BE(data.length);
  const body=Buffer.concat([Buffer.from(tag),data]);
  const crc=Buffer.alloc(4);crc.writeUInt32BE(crc32(body));
  return Buffer.concat([len,body,crc]);
}
function realPng(n){
  const head=Buffer.alloc(13);head.writeUInt32BE(1,0);head.writeUInt32BE(1,4);head[8]=8;head[9]=2;
  const bytes=Buffer.from([0,n%256,(n*17)%256,(n*47)%256]);
  return Buffer.concat([Buffer.from('89504e470d0a1a0a','hex'),chunk('IHDR',head),
    chunk('IDAT',zlib.deflateSync(bytes)),chunk('IEND',Buffer.alloc(0))]);
}
function fixture(){
  const root=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-art-gate-'));
  let k=1;const sha=x=>String(x).repeat(64);const asset=sha('a');
  function pngAB(unity=false){
    const a=realPng(k++),b=realPng(k++);
    const before='before-'+k+'.png',after='after-'+k+'.png';
    fs.writeFileSync(path.join(root,before),a);fs.writeFileSync(path.join(root,after),b);
    return {before_path:before,after_path:after,before_sha256:H(a),after_sha256:H(b),
      camera_sha256:sha('c'),render_profile:'fixed-angle-pbr',width:1,height:1,
      ...(unity?{engine:'Unity',source_asset_sha256:sha('d')}: {})};
  }
  let previous=asset;
  const iterations=[0,1,2].map(i=>{
    const output=i===2?sha('d'):sha(String(i+1));
    const it={run_id:38600000000+i,commit_sha:String(i+1).repeat(40),model:'qwen3-vl:8b',
      original_qwen_plan_sha256:sha('b'),
      input_asset_sha256:previous,output_asset_sha256:output,
      blender_ab:pngAB(),independent_review:{
        independent:true,reviewer_id:'independent-art-director',
        accepted:true,protected_regions_ok:true,severe_regressions:[],
        diagnosis:'A deliberately specific independent review of tower defects.',
        observed_improvement:'Real improvement of the existing coherent tower silhouette.',
        before:Object.fromEntries(['geometry','architecture','materials','style','camera'].map(x=>[x,5+i])),
        after:Object.fromEntries(['geometry','architecture','materials','style','camera'].map(x=>[x,6+i]))
      }};
    previous=output;return it;
  });
  iterations[2].unity_ab=pngAB(true);
  const manifest={schema_version:1,asset_id:'Valoria_Tower_CohV1',
    reference_sha256:sha('f'),frozen_original_asset_sha256:asset,
    status:'CALIBRATION_VALIDATED',iterations};
  return {root,manifest,dispose:()=>fs.rmSync(root,{recursive:true,force:true})};
}
test('three real cumulative A/B cycles + protected source + independent Unity approval PASS',()=>{
  const f=fixture();try{assert.deepEqual(validateManifest(f.manifest,f.root),[]);}finally{f.dispose();}
});
test('cannot call one technical success a validated three-cycle calibration',()=>{
  const f=fixture();try{f.manifest.iterations=f.manifest.iterations.slice(0,1);
    assert.match(validateManifest(f.manifest,f.root).join(' '),/needs >=3/);}finally{f.dispose();}
});
test('cannot pass missing actual PNG bytes or altered SHA',()=>{
  const f=fixture();try{
    f.manifest.iterations[1].blender_ab.after_path='made-up.png';
    assert.match(validateManifest(f.manifest,f.root).join(' '),/not materialized/);
  }finally{f.dispose();}
});
test('cannot self-approve or declare a protected regression',()=>{
  const f=fixture();try{
    f.manifest.iterations[0].independent_review.independent=false;
    f.manifest.iterations[0].independent_review.protected_regions_ok=false;
    assert.match(validateManifest(f.manifest,f.root).join(' '),/self-approval/);
    assert.match(validateManifest(f.manifest,f.root).join(' '),/protected\/severe/);
  }finally{f.dispose();}
});
test('cannot approve when cumulative SHA chain or score continuity is false',()=>{
  const f=fixture();try{
    f.manifest.iterations[1].input_asset_sha256='9'.repeat(64);
    f.manifest.iterations[1].independent_review.before.geometry=4;
    const e=validateManifest(f.manifest,f.root).join(' ');
    assert.match(e,/broken same-asset cumulative SHA chain/);
    assert.match(e,/score chain discontinuity/);
  }finally{f.dispose();}
});
test('cannot approve a stagnant or visually worse intervention',()=>{
  const f=fixture();try{
    f.manifest.iterations[0].independent_review.after.geometry=5;
    f.manifest.iterations[0].independent_review.after.architecture=4;
    assert.match(validateManifest(f.manifest,f.root).join(' '),/unacceptable regression/);
  }finally{f.dispose();}
});
test('cannot approve without real final Unity evidence',()=>{
  const f=fixture();try{delete f.manifest.iterations[2].unity_ab;
    assert.match(validateManifest(f.manifest,f.root).join(' '),/missing A\/B evidence/);
  }finally{f.dispose();}
});
test('empty approval directory makes no invented approval claims',()=>{
  const f=fixture();try{fs.mkdirSync(path.join(f.root,'approvals'));
    assert.equal(runCli(['--directory',path.join(f.root,'approvals')]),0);
  }finally{f.dispose();}
});
