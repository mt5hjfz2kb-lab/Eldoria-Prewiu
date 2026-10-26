// Advisory next-action selector for a SINGLE observed GitHub Actions run.
// Never owns/claims a workstream, executes code, moves a lease or approves art.
// Use only metadata (job/step status + artifact headers); never copy log contents.
const RUNNING=new Set(['queued','waiting','pending','requested','in_progress']);
const shorten=s=>typeof s==='string'?s.slice(0,160):null;
const stepsOf=j=>Array.isArray(j.steps)?j.steps.filter(s=>s&&typeof s==='object'):[];
export function triageRun({report,jobs,artifacts}) {
  if(!report||typeof report!=='object')throw Error('Missing mission report');
  if(report.observation_mode!=='report_only_no_issues')
    throw Error('No autonomous triage outside report-only current missions');
  const run=report.latest_run;
  const guardrails={read_only:true,dispatch_allowed:false,art_approved:false,
    scope:'one observed Actions run; not a live chat/Unity scene'};
  if(!run||!Number.isSafeInteger(run.id)||run.id<1)
    return {status:'NO_MATCHING_RUN',next_safe_action:'Owner checks the current branch and permitted production gate; no automatic dispatch.',...guardrails};
  if(!jobs||!Array.isArray(jobs.jobs)||!artifacts||!Array.isArray(artifacts.artifacts))
    return {status:'EVIDENCE_UNAVAILABLE',run_id:run.id,next_safe_action:'Jobs or artifacts could not be read; do not assume empty evidence.',...guardrails};
  const partial=(jobs.total_count??jobs.jobs.length)>jobs.jobs.length ||
    (artifacts.total_count??artifacts.artifacts.length)>artifacts.artifacts.length;
  const failing=jobs.jobs.flatMap(j=>stepsOf(j).filter(s=>s.conclusion==='failure').map(s=>({
    job_id:Number.isSafeInteger(j.id)?j.id:null,
    job:shorten(j.name),step:shorten(s.name)}))).slice(0,15);
  const artifactRefs=artifacts.artifacts.slice(0,25).filter(a=>a&&typeof a==='object').map(a=>({
    id:Number.isSafeInteger(a.id)?a.id:null,name:shorten(a.name),
    expired:a.expired===true,size_bytes:Number.isSafeInteger(a.size_in_bytes)?a.size_in_bytes:null
  }));
  let status,next_safe_action;
  if(report.state==='GITHUB_APPROVAL_REQUIRED'||run.conclusion==='action_required'){
    status='EXTERNAL_APPROVAL';
    next_safe_action='GitHub authorization needed for this run; authorized maintainer reviews it. Do not bypass.';
  }else if(report.source_matches_branch_head===false){
    status='STALE_SOURCE';
    next_safe_action='This run was built from an older source. Do not use it to certify current HEAD.';
  }else if(RUNNING.has(run.status)){
    status='WAIT_FOR_SAME_RUN';
    next_safe_action='The exact run is unfinished. Do not duplicate it; after it completes inspect its artifacts.';
  }else if(run.conclusion==='success'){
    status=partial?'EVIDENCE_PARTIAL':'REVIEW_VERIFIED_RUN_OUTPUTS';
    next_safe_action='Technical workflow succeeded; owning agent must inspect matching evidence, gameplay/visual requirements and approval gates.';
  }else if(run.conclusion==='failure'||run.conclusion==='timed_out'||run.conclusion==='cancelled'){
    status=partial?'EVIDENCE_PARTIAL':'DIAGNOSE_FAILED_STEPS';
    next_safe_action='Owning agent reviews failed step logs, corrects the underlying cause and reruns the permitted gate. No blind retry.';
  }else{
    status='RESULT_UNCERTAIN';
    next_safe_action='Check the exact Actions conclusion and owner scope before taking any action.';
  }
  return {status,run_id:run.id,source_matches_head:report.source_matches_branch_head,
    failed_steps:failing,artifacts:artifactRefs,
    job_count_observed:jobs.jobs.length,artifact_count_observed:artifactRefs.length,
    metadata_window:partial?'partial':'bounded_first_page',
    next_safe_action,...guardrails};
}
