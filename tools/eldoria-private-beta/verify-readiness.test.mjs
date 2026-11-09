import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {assessBeta} from './verify-readiness.mjs';
const actual=JSON.parse(readFileSync(new URL('../../pipeline/eldoria-private-beta/readiness.json',import.meta.url)));
test('actual private beta is BLOCKED; no invented readiness',()=>{
 const r=assessBeta(actual);
 assert.equal(r.ready,false);assert.ok(r.errors.includes('private_host_not_deployed'));
 assert.ok(r.errors.includes('core_gameplay_events_not_verified'));
 assert.ok(r.errors.includes('physical_mobile_qa_missing'));
});
test('redacted or open database cannot be production ready',()=>{
 const b=structuredClone(actual);b.measurement.public_insert_allowed=true;
 assert.ok(assessBeta(b).errors.includes('sensitive_telemetry_not_secure'));
});
test('browser-only observation is not Unity gameplay telemetry',()=>{
 const b=structuredClone(actual);
 b.measurement.browser_observer_implemented_types=67;
 assert.ok(assessBeta(b).errors.includes('core_gameplay_events_not_verified'));
});
test('false self-certification or missing owner approval cannot go live',()=>{
 const b=structuredClone(actual);b.qa.no_new_tester_invites_before_validation=false;
 assert.ok(assessBeta(b).errors.includes('unsafe_invite_policy'));
 assert.ok(assessBeta(b).errors.includes('tester_invites_not_authorized'));
});
test('full verified candidate passes; no unnecessary hidden constraint',()=>{
 const b=structuredClone(actual);
 Object.assign(b.distribution,{service_account_connected:true,r2_files_uploaded:true,worker_deployed:true,links_enabled:true,frozen_files:19});
 Object.assign(b.measurement,{real_worker_to_supabase_request_verified:true,actual_events_verified_from_unity:9});
 Object.assign(b.qa,{hosted_private_gameplay_pass:true,full_native_i_v_normal_clock_verified:true,real_mobile_ios_pass:true,real_mobile_android_pass:true});
 b.owner_approval_to_invite=true;
 assert.deepEqual(assessBeta(b),{ready:true,errors:[]});
});
