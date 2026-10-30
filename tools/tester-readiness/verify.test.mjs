import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, mkdirSync, writeFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, dirname } from 'node:path';
import { createHash } from 'node:crypto';
import { checkCandidate, verify, REQUIRED_VIEWS, ORIENTATIONS } from './verify.mjs';

const source = 'a'.repeat(40), artifact = 'b'.repeat(64);
function fixture() {
  const root = mkdtempSync(join(tmpdir(), 'eldoria-testers-'));
  const put = (p, s) => { const filename=join(root,p); mkdirSync(dirname(filename),{recursive:true});writeFileSync(filename,s);return createHash('sha256').update(s).digest('hex'); };
  const digest = put('proof/proof.txt', 'authentic fixture bytes');
  const evidence = {evidence_path:'proof/proof.txt',evidence_sha256:digest};
  const visual_evidence=[];
  for(const view of REQUIRED_VIEWS) for(const orientation of ORIENTATIONS)
    visual_evidence.push({view,orientation,reference_path:'proof/proof.txt',reference_sha256:digest,
      actual_path:'proof/proof.txt',actual_sha256:digest,review:{verdict:'APPROVED',reviewer:'Independent reviewer',same_camera:true,legible_on_mobile:true,interactions_clear:true}});
  const m={schema:1,state:'READY',source_sha:source,artifact_sha256:artifact,channels:['web'],
    visual_evidence,gameplay:{status:'PASS',source_sha:source,native_run_id:12345,
      uninterrupted_from_fresh_save:true,no_debug_skips:true,bastions:[1,2,3,4,5],
      buttons_and_timers_pass:true,saves_and_reload_pass:true,marches_and_combat_pass:true,...evidence},
    device_tests:[{channel:'web',status:'PASS',physical_device:true,model:'Test phone',os:'Test OS',
      source_sha:source,artifact_sha256:artifact,...evidence}],
    delivery:{web:{status:'PASS',hosted_gameplay_verified:true,source_sha:source,artifact_sha256:artifact,...evidence}},
    final_review:{verdict:'APPROVED',reviewer:'Owner',source_sha:source,...evidence}};
  const save = () => put('pipeline/tester-readiness/candidate.json',JSON.stringify(m));
  const release = (enabled) => put('pipeline/tester-readiness/release-request.json',JSON.stringify({enabled}));
  put('pipeline/android-updater-release-request.json','{"enabled":false}');
  put('pipeline/unity-publish-request.json','{"enabled":false,"release_approved":false}');
  save(); release(false);
  return {root,m,save,release,cleanup:()=>rmSync(root,{recursive:true,force:true})};
}
test('strict complete fixture succeeds only as evidence-consistency proof',()=>{
  const f=fixture();try{assert.deepEqual(checkCandidate(f.m,f.root),[]);}finally{f.cleanup();}
});
test('blocked without release request is not called ready',()=>{
  const f=fixture();try{f.m.state='BLOCKED';f.save();assert.equal(verify(f.root).status,'BLOCKED_NOT_RELEASE_READY');assert.equal(verify(f.root,{requireReady:true}).status,'FAIL');}finally{f.cleanup();}
});
test('missing or mismatched genuine PNG/log bytes fail closed',()=>{
  const f=fixture();try{f.m.visual_evidence[0].actual_sha256='f'.repeat(64);
    assert.match(checkCandidate(f.m,f.root).join(' '),/SHA256 mismatch/);
    f.m.visual_evidence[0].actual_path='../../etc/passwd';
    assert.match(checkCandidate(f.m,f.root).join(' '),/outside checkout/);}finally{f.cleanup();}
});
test('no fake mobile, no short Bastion test, no wrong source or auto aesthetic approval',()=>{
  const f=fixture();try{f.m.device_tests[0].physical_device=false;f.m.gameplay.bastions=[1,2];
    f.m.gameplay.source_sha='f'.repeat(40);f.m.visual_evidence[0].review.reviewer='AUTOMATED_PIXEL_SCORE';
    const e=checkCandidate(f.m,f.root).join(' ');
    assert.match(e,/physical device/);assert.match(e,/Bastion I-V/);assert.match(e,/independent visual/);}finally{f.cleanup();}
});
test('enabled external tester promotion while blocked is rejected',()=>{
  const f=fixture();try{f.m.state='BLOCKED';f.save();f.release(true);
    assert.equal(verify(f.root).status,'FAIL');assert.equal(verify(f.root).releaseEnabled,true);}finally{f.cleanup();}
});
test('different hosted/source build and missing final approval fail',()=>{
  const f=fixture();try{f.m.delivery.web.source_sha='c'.repeat(40);
    f.m.final_review.verdict='PENDING';
    const e=checkCandidate(f.m,f.root).join(' ');
    assert.match(e,/published\/hosted/);assert.match(e,/owner artistic approval/);}finally{f.cleanup();}
});

test('complete exact-candidate evidence can authorize external promotion',()=>{
  const f=fixture();try{
    f.release(true);
    assert.equal(verify(f.root).status,'READY_EVIDENCE_CONSISTENT');
    assert.equal(verify(f.root).releaseEnabled,true);
  }finally{f.cleanup();}
});
