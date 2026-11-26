import {createRequire} from "node:module";
import {readFileSync,writeFileSync,mkdirSync,statSync} from "node:fs";
import {join,resolve} from "node:path";
import assert from "node:assert/strict";
import {sha} from "./design.mjs";
import {request,createCandidates} from "./role-contract.mjs";
const require=createRequire(import.meta.url),{chromium}=require("playwright");
const root=resolve(process.env.V3_VISUAL_OUT||"");
assert.ok(process.env.V3_VISUAL_OUT);
const blind=join(root,"blind-review"),out=join(root,"role-inputs");mkdirSync(out);
const manifest=JSON.parse(readFileSync(join(blind,"frames-manifest.json"),"utf8"));
assert.equal(manifest.length,33);
const aliases=["baseline","variant-72","variant-19"];
function image(phase,viewport,alias){
 const r=manifest.find(x=>x.viewport===viewport&&x.phase===phase&&x.variant===alias);assert.ok(r);
 const bytes=readFileSync(join(blind,viewport+"-"+phase+"-"+alias+".png"));assert.equal(sha(bytes),r.sha256);
 return "data:image/png;base64,"+bytes.toString("base64");
}
const browser=await chromium.launch({headless:true});
async function sheet(name,rows,width){
 const page=await browser.newPage({viewport:{width,height:600},deviceScaleFactor:1});
 const cols=rows[0].length;
 const html='<html><head><style>body{margin:0;padding:12px;background:#101820;color:#ede9dc;font:16px system-ui}.row{display:grid;grid-template-columns:repeat('+cols+',minmax(0,1fr));gap:8px;margin-bottom:12px}.cell{min-width:0;text-align:center}img{display:block;width:100%;height:auto;object-fit:contain;background:#080a10}p{margin:5px 0 8px}</style></head><body>'+rows.map(row=>'<div class="row">'+row.map(x=>'<div class="cell"><p>'+x.caption+'</p><img src="'+x.src+'"></div>').join("")+'</div>').join("")+'</body></html>';
 await page.setContent(html);
 await page.evaluate(async()=>Promise.all(Array.from(document.images,i=>i.decode())));
 const file=join(out,name+".jpg");
 await page.screenshot({path:file,type:"jpeg",quality:88,fullPage:true,animations:"disabled"});
 assert.ok(statSync(file).size>10000&&statSync(file).size<=3*1024*1024,"Visual payload size");
 await page.close();return sha(readFileSync(file));
}
const cell=(phase,vp,alias,caption)=>({src:image(phase,vp,alias),caption});
try{
 const common=await sheet("production-baseline",[
  [cell("intro","phone","baseline","Entrada · 390 × 844"),cell("gates","phone","baseline","Puertas · 390 × 844"),cell("battle","phone","baseline","Combate · 390 × 844")],
  [cell("gates","compact","baseline","Pantalla compacta"),cell("victory","phone","baseline","Victoria"),cell("defeat","phone","baseline","Derrota")]
 ],1200);
 const refBytes=readFileSync(join(blind,"reference-panel.png"));
 const ref=await sheet("production-reference",[[{caption:"Material original Corona de la Brecha",src:"data:image/png;base64,"+refBytes.toString("base64")},cell("gates","horizontal","baseline","Escena real horizontal, sin rediseño")]],1100);
 const commonText=sha(Buffer.concat([readFileSync(join(blind,"reference-art-direction.md")),readFileSync(join(blind,"reference-shared-standard.md"))]));
 const j1=await sheet("judge-phone",["intro","gates","battle","victory","defeat"].map(phase=>aliases.map(alias=>cell(phase,"phone",alias,alias+" · "+phase))),1200);
 const j2=await sheet("judge-small-horizontal",[
  aliases.map(alias=>cell("gates","compact",alias,alias+" · compacta")),
  aliases.map(alias=>cell("battle","horizontal",alias,alias+" · horizontal")),
  aliases.map(alias=>cell("intro","horizontal",alias,alias+" · entrada horizontal"))
 ],1200);
 const productionEvidence={primary_sha256:common,secondary_sha256:ref,reference_text_sha256:commonText},judgeEvidence={primary_sha256:j1,secondary_sha256:j2,reference_text_sha256:commonText};
 const pack=JSON.parse(readFileSync(new URL("./calibration.json",import.meta.url),"utf8")).packets;
 const prepared=[];
 const r=await createCandidates(productionEvidence,async q=>{prepared.push(q);const p=q.role==="single"?pack[0]:q.role==="director"?pack[1].direction:q.role==="illustrator"?pack[1].art:pack[1].layout;return {request_digest:q.request_digest,payload:p}});
 const judge=request("judge",judgeEvidence);
 for(const q of [...prepared,judge])writeFileSync(join(out,q.role+"-request.json"),JSON.stringify(q,null,2));
 const proof={schema:1,mode:"offline calibration",production_evidence:productionEvidence,judge_evidence:judgeEvidence,prepared_roles:[...prepared,judge].map(x=>({role:x.role,request_digest:x.request_digest})),fixture_role_receipts:r.receipts,design_hashes:r.render_hashes,judge_invoked:false,api_calls:0,scores:null};
 writeFileSync(join(out,"role-input-proof.json"),JSON.stringify(proof,null,2));
 console.log("STUDIO_V3_VISUAL_ROLE_INPUTS "+JSON.stringify(proof));
}finally{await browser.close()}
