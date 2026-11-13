// Programmatic release gate. A source unit-test PASS is never a player-facing PASS.
import {readFileSync} from 'node:fs';
import {resolve} from 'node:path';
import {fileURLToPath} from 'node:url';
export function assessBeta(b){
 const errors=[];
 if(b?.schema!==1)errors.push('bad_schema');
 if(b?.frozen_archive?.sha256_verified!==true ||
    b?.frozen_archive?.archive_private_draft!==true) errors.push('freeze_not_certified');
 const d=b?.distribution||{};
 if(d.service==='GitHub Pages'){
  if(d.live_identity_verified!==true)errors.push('public_host_identity_unverified');
  if(d.access_control!=='public URL; not authenticated')errors.push('public_access_not_explicit');
 }else if(d.service_account_connected!==true||d.r2_files_uploaded!==true||d.worker_deployed!==true)
  errors.push('private_host_not_deployed');
 if(d.links_enabled!==true)errors.push('links_inactive');
 if(d.byte_checksums_verified!==true)errors.push('web_files_not_verified');
 if(d.service!=='GitHub Pages'&&d.frozen_files!==19)errors.push('web_files_not_exact');
 const m=b?.measurement||{};
 if(m.row_level_security!==true||m.public_select_allowed!==false||m.public_insert_allowed!==false)
  errors.push('sensitive_telemetry_not_secure');
 if(m.real_backend_to_supabase_request_verified!==true&&m.real_worker_to_supabase_request_verified!==true)
  errors.push('no_real_telemetry_delivery_proof');
 if(m.metrics_specified!==135||m.event_types_schema_accepted!==67)
  errors.push('metrics_spec_changed');
 // Browser load/FPS metrics alone cannot answer whether the actual campaign works.
 if(!Number.isInteger(m.actual_events_verified_from_unity)||m.actual_events_verified_from_unity<9)
  errors.push('core_gameplay_events_not_verified');
 const qa=b?.qa||{};
 if((qa.hosted_gameplay_pass!==true&&qa.hosted_private_gameplay_pass!==true)||qa.full_native_i_v_normal_clock_verified!==true)
  errors.push('hosted_i_v_not_proven');
 if(qa.real_mobile_ios_pass!==true||qa.real_mobile_android_pass!==true)
  errors.push('physical_mobile_qa_missing');
 if(qa.no_new_tester_invites_before_validation!==true)errors.push('unsafe_invite_policy');
 if(b?.owner_approval_to_invite!==true)errors.push('tester_invites_not_authorized');
 return {ready:errors.length===0,errors};
}
if(process.argv[1]&&resolve(process.argv[1])===fileURLToPath(import.meta.url)){
 const f=process.argv[2]||'pipeline/eldoria-private-beta/readiness.json';
 const m=JSON.parse(readFileSync(f,'utf8'));
 const report=assessBeta(m);
 console.log(JSON.stringify({status:report.ready?'READY':'BLOCKED',...report},null,2));
 if(!report.ready)process.exitCode=1;
}
