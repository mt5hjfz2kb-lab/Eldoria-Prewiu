import {test} from "node:test";
import assert from "node:assert/strict";
import {mkdtempSync,mkdirSync,writeFileSync,rmSync} from "node:fs";
import {tmpdir} from "node:os";
import {join} from "node:path";
import {verifyFrames} from "./runtime.mjs";
import {sha} from "../visual-benchmark/design.mjs";
function fixture(){const root=mkdtempSync(join(tmpdir(),"v3-frame-test-"));const rows=[];for(const [v,p] of [["phone","intro"],["phone","gates"],["phone","battle"],["phone","victory"],["phone","defeat"],["compact","intro"],["compact","gates"],["compact","battle"],["horizontal","intro"],["horizontal","gates"],["horizontal","battle"]])for(const a of ["baseline","variant-72","variant-19"]){const bytes=Buffer.from(v+p+a);const n=v+"-"+p+"-"+a+".png";writeFileSync(join(root,n),bytes);rows.push({viewport:v,phase:p,variant:a,sha256:sha(bytes),state_digest:sha(v+p)})}writeFileSync(join(root,"frames-manifest.json"),JSON.stringify(rows));return {root,rows}}
test("exact 33 bytes and same 11 state hashes",()=>{const f=fixture();try{assert.equal(verifyFrames(f.root).length,33)}finally{rmSync(f.root,{recursive:true,force:true})}});
test("tampered image rejected",()=>{const f=fixture();try{writeFileSync(join(f.root,"phone-gates-variant-19.png"),"tampered");assert.throws(()=>verifyFrames(f.root),/altered capture/)}finally{rmSync(f.root,{recursive:true,force:true})}});
test("mismatched fixture state rejected",()=>{const f=fixture();try{f.rows[1].state_digest=sha("wrong");writeFileSync(join(f.root,"frames-manifest.json"),JSON.stringify(f.rows));assert.throws(()=>verifyFrames(f.root),/Matched render state/)}finally{rmSync(f.root,{recursive:true,force:true})}});
