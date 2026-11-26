import {strict as assert} from "node:assert";
import {createHash} from "node:crypto";
import {execFileSync} from "node:child_process";
import {mkdtempSync,writeFileSync,readFileSync,readdirSync,renameSync,statSync} from "node:fs";
import {tmpdir} from "node:os";
import {join} from "node:path";
import {canonical,runMission} from "./core.mjs";

const sha="1657b2caa6b1a955319538ce3a8b2906398599b3";
const path="tools/openai-development-studio/lessons.json";
const decisionPath="tools/openai-development-studio/quality-decisions.json";
const digest=s=>createHash("sha256").update(s,"utf8").digest("hex");
const git=arg=>execFileSync("git",["show",sha+":"+arg],{encoding:"utf8",maxBuffer:1024*1024});
const original=git(path);
const baseline=JSON.parse(original);
const reference=git(decisionPath);
const decisions=JSON.parse(reference);
const evidence=decisions.decisions.find(d=>d.run_id===45683014884&&d.evidence_run===45688957980);
assert.equal(evidence?.decision,"technical_only");
assert.equal(evidence.quality_score_before,null);
assert.equal(evidence.quality_score_after,null);
assert.ok(!baseline.lessons.some(x=>x.id==="L007"));
const scratch=mkdtempSync(join(tmpdir(),"eldoria-studio-v3-"));
writeFileSync(join(scratch,"baseline.json"),original,{flag:"wx",mode:0o600});
writeFileSync(join(scratch,"decision-reference.json"),reference,{flag:"wx",mode:0o600});
const s={schema:1,id:"v3-pinned-v2-memory-replay",mode:"offline",budget_usd:0,source_sha:sha,
  files:[{path,digest:digest(original)}],target:"LESSONS_APPEND_CANDIDATE",objective:"Append evidence-backed candidate learning without replacing any existing lesson",max_attempts:4,quality_required:false,
  required_checks:["schema","preserve-existing","evidence-link","unique-id"],producer:"fixture-producer",auditor:"fixture-independent-review"};
const correct={id:"L007",status:"verified",targets:["all"],rule:"A modest local visual improvement and technical pass are not grounds for production promotion. Preserve the source until the artistic acceptance criterion is met; do not invent quality scores.",evidence_run:45688957980};
function proposal({previous,required_pivot_from}){
  const candidate=JSON.parse(original);
  let strategy="direct-append";
  // Negative controls are intentionally defective. The third proposal reacts to actual state.
  if(!previous)candidate.lessons.push({...correct,evidence_run:45683014884});
  else if(required_pivot_from==="direct-append"){
    strategy="append-only-evidence";
    candidate.lessons.push(correct);
  }else {
    candidate.lessons.shift();candidate.lessons.push(correct);
  }
  return {target:s.target,producer:s.producer,strategy,operations:[{path,before:original,after:JSON.stringify(candidate,null,2)+"\n"}]};
}
function validation({files,candidate_digest}){
  const data=JSON.parse(files[path]),last=data.lessons.at(-1),ids=data.lessons.map(x=>x.id);
  const row=(id,pass,evidence)=>({id,pass,evidence,candidate_digest});
  return [
    row("schema",data.schema===baseline.schema&&Array.isArray(data.lessons),"Parsed pinned source and candidate schema"),
    row("preserve-existing",canonical(data.lessons.slice(0,-1))===canonical(baseline.lessons)&&data.lessons.length===baseline.lessons.length+1,"Compared each preexisting lesson"),
    row("evidence-link",last.evidence_run===evidence.evidence_run&&last.status==="verified","Checked against pinned quality decision, not producer assertion"),
    row("unique-id",new Set(ids).size===ids.length&&last.id==="L007","Checked identity and unique IDs")];
}
function audit({files,candidate_digest}){
  const b=JSON.parse(readFileSync(join(scratch,"baseline.json"),"utf8"));
  const c=JSON.parse(files[path]);const last=c.lessons.at(-1);
  // Separate deterministic check; not a live AI reviewer or artistic judgement.
  const acceptable=canonical(c.lessons.slice(0,-1))===canonical(b.lessons)&&canonical(last)===canonical(correct)&&c.lessons.length===b.lessons.length+1;
  return {reviewer:s.auditor,candidate_digest,decision:acceptable?"accept":"reject",reason:acceptable?"Append-only evidence-backed candidate; no promotion":"Candidate changes more than approved lesson",quality:"not_assessed"};
}
let checkpoints=0;
async function checkpoint(report){
  const serialized=JSON.stringify(report,null,2)+"\n";
  const p=join(scratch,"checkpoint-"+String(++checkpoints).padStart(2,"0")+".json");
  writeFileSync(p,serialized,{flag:"wx",mode:0o600});
  const temp=join(scratch,"journal-"+checkpoints+".tmp");
  writeFileSync(temp,serialized,{flag:"wx",mode:0o600});
  renameSync(temp,join(scratch,"journal.json"));
}
const result=await runMission(s,{[path]:original},{digest,propose:proposal,validate:validation,audit,checkpoint});
assert.equal(result.terminal,"candidate_ready");
assert.equal(result.rows.length,3);
assert.equal(result.rows[0].outcome,"technical_reject");
assert.equal(result.rows[0].checks.find(x=>x.id==="evidence-link").pass,false);
assert.equal(result.rows[1].checks.find(x=>x.id==="preserve-existing").pass,false);
assert.equal(result.rows[1].pivot_required,true);
assert.equal(result.rows[2].strategy,"append-only-evidence");
assert.equal(result.rows[2].outcome,"accepted_candidate");
assert.equal(result.api_calls,0);assert.equal(result.art_approved,false);
assert.equal(checkpoints,4);
const candidate=result.candidate.files[path];
writeFileSync(join(scratch,"candidate.json"),candidate,{flag:"wx",mode:0o600});
assert.equal(readFileSync(join(scratch,"candidate.json"),"utf8"),candidate);
assert.equal(readFileSync(join(scratch,"baseline.json"),"utf8"),original);
assert.equal(git(path),original);assert.equal(git(decisionPath),reference);
assert.equal(JSON.parse(readFileSync(join(scratch,"journal.json"),"utf8")).candidate.digest,result.candidate.digest);
const proof={schema:1,kind:"pinned-real-input-offline-replay",source_sha:sha,source_file:path,source_sha256:digest(original),evidence_file_sha256:digest(reference),candidate_sha256:digest(candidate),candidate_snapshot_sha256:result.candidate.digest,baseline_lessons:baseline.lessons.length,candidate_lessons:JSON.parse(candidate).lessons.length,source_unchanged:true,checkpoints,attempts:result.rows.map(r=>({attempt:r.attempt,strategy:r.strategy,outcome:r.outcome,failed_checks:r.checks?.filter(c=>!c.pass).map(c=>c.id),pivot_required:r.pivot_required??false})),scratch_files:readdirSync(scratch).sort(),api_calls:0,art_approved:false,promoted:false,live_model_roles:false};
writeFileSync(join(scratch,"proof.json"),JSON.stringify(proof,null,2)+"\n",{flag:"wx",mode:0o600});
console.log("STUDIO_V3_REPLAY_PROOF "+JSON.stringify(proof));
