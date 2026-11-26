// Actual local renderer bridge for the authorized future session. No API
// credentials are passed to browser subprocesses. The host must first verify
// the immutable calibration artifact/run, grant and repository lease.
import {execFileSync} from "node:child_process";
import {readFileSync,writeFileSync,mkdirSync,existsSync,lstatSync} from "node:fs";
import {resolve,join,dirname} from "node:path";
import {fileURLToPath} from "node:url";
import {sha,SOURCE,REFERENCE,TARGET,assemble} from "../visual-benchmark/design.mjs";
const need=(v,m)=>{if(!v)throw Error(m)};
const json=p=>JSON.parse(readFileSync(p,"utf8"));
const aliases=["baseline","variant-72","variant-19"];
const script=name=>fileURLToPath(new URL("../visual-benchmark/"+name,import.meta.url));
const hashFile=p=>{need(lstatSync(p).isFile()&&!lstatSync(p).isSymbolicLink(),"Expected regular evidence file");return sha(readFileSync(p))};
export function verifyFrames(blind){
 const rows=json(join(blind,"frames-manifest.json"));need(Array.isArray(rows)&&rows.length===33,"Missing 33 original frames");
 const seen=new Set(),groups=new Map();
 for(const r of rows){
  need(["phone","compact","horizontal"].includes(r.viewport)&&["intro","gates","battle","victory","defeat"].includes(r.phase)&&aliases.includes(r.variant),"Invalid frame label");
  const key=r.viewport+"-"+r.phase,filename=key+"-"+r.variant+".png";
  need(!seen.has(filename)&&/^[a-f0-9]{64}$/.test(r.state_digest)&&hashFile(join(blind,filename))===r.sha256,"Duplicate or altered capture");
  seen.add(filename);if(!groups.has(key))groups.set(key,[]);groups.get(key).push(r);
 }
 need(groups.size===11&&Array.from(groups.values()).every(v=>v.length===3&&new Set(v.map(x=>x.state_digest)).size===1&&aliases.every(a=>v.some(x=>x.variant===a))),"Matched render state mismatch");
 return rows;
}
export function loadEvidence(root,kind,expected=null){
 need(["production","judge"].includes(kind),"Unknown evidence type");const blind=join(root,"blind-review"),role=join(root,"role-inputs");
 verifyFrames(blind);
 const report=json(join(root,"report.json"));
 need(report.target===TARGET&&report.source_sha===SOURCE&&report.reference_sha===REFERENCE&&report.errors.length===0&&report.matched_fixtures===11&&report.total_frames===33&&report.actual_game_logic_unchanged===true,"Invalid render provenance");
 if(kind==="production")need(report.mode==="hand-authored-calibration"&&report.variants.slice(1).every(x=>x.issues.length===0&&x.all_controls===true),"Calibration must pass all gates");
 else need(report.mode==="external-design-packet","Judge requires actual external candidate render");
 const proof=json(join(role,"role-input-proof.json")),names=kind==="judge"?["judge-phone.jpg","judge-small-horizontal.jpg"]:["production-baseline.jpg","production-reference.jpg"];
 const evidence={primary_sha256:hashFile(join(role,names[0])),secondary_sha256:hashFile(join(role,names[1])),reference_text_sha256:sha(Buffer.concat([readFileSync(join(blind,"reference-art-direction.md")),readFileSync(join(blind,"reference-shared-standard.md"))]))};
 const claimed=proof[kind+"_evidence"];need(claimed&&Object.keys(evidence).every(k=>claimed[k]===evidence[k]),"Role evidence digest mismatch");
 if(expected)need(Object.keys(evidence).every(k=>expected[k]===evidence[k]),"Evidence is not exact authorized input");
 return {report,evidence,images:names.map(n=>join(role,n)),refs:[join(blind,"reference-art-direction.md"),join(blind,"reference-shared-standard.md")]};
}
function childEnv(webRoot,out,pack){
 return {...Object.fromEntries(["PATH","HOME","LANG","LC_ALL","TMPDIR","NODE_PATH","PLAYWRIGHT_BROWSERS_PATH"].filter(k=>process.env[k]!==undefined).map(k=>[k,process.env[k]])),V3_VISUAL_WEBROOT:webRoot,V3_VISUAL_OUT:out,V3_VISUAL_PACK:pack};
}
export function actualRenderer(webRoot,scratch){
 need(webRoot&&scratch&&existsSync(join(webRoot,"tools/senda-brecha/visual-3d/index.html")),"Pinned source not materialized");
 return async (pack,round)=>{
  need(Number.isInteger(round)&&round>=1&&round<=2&&pack.packets.length===2,"Invalid render round");
  for(const p of pack.packets)assemble(p);
  const roundRoot=join(scratch,"render-round-"+round),packPath=join(roundRoot,"candidate-pack.json"),out=join(roundRoot,"evidence");
  mkdirSync(roundRoot,{recursive:false});writeFileSync(packPath,JSON.stringify(pack,null,2),{flag:"wx",mode:0o600});
  const env=childEnv(webRoot,out,packPath);
  try{execFileSync(process.execPath,[script("browser.mjs")],{env,encoding:"utf8",timeout:8*60*1000,maxBuffer:4*1024*1024,stdio:["ignore","pipe","pipe"]})}
  catch(error){
   // Browser intentionally returns nonzero on measured candidate gate failure.
   // No generic execution failure may become an accepted render.
   if(!existsSync(join(out,"report.json")))throw error;
   const p=json(join(out,"report.json"));need(p.errors.length===0&&p.variants.slice(1).some(x=>x.issues.length>0),"Unexpected render failure");
  }
  execFileSync(process.execPath,[script("role-inputs.mjs")],{env,encoding:"utf8",timeout:2*60*1000,maxBuffer:4*1024*1024});
  const j=loadEvidence(out,"judge");
  const mapping=json(join(out,"private-receipts","identity-map.json"));need(mapping.calibration===false,"Not an external render");
  // Only the two anonymous sheets and common references are handed to the
  // judge transport; fixture role-input packets from calibration never travel.
  return {report:j.report,judgeEvidence:j.evidence,design_hashes:mapping.aliases,judgeImages:j.images,judgeRefs:j.refs,evidenceRoot:out};
 };
}
