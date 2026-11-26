import {test} from "node:test";
import {strict as assert} from "node:assert";
import {SOURCE,PATH,MARK,CLAUSES,digest,frozenSource,planFor,baselineEvidence,makeEdit,guardCandidate,validator} from "./senda-pilot.mjs";
const src=frozenSource(),base=baselineEvidence(src);
test("real pinned Senda defect reproduced; route baseline exists",()=>{assert.equal(base.routes.count,729);assert.ok(base.routes.wins>0&&base.routes.losses>0);assert.ok(base.invalid.failed.includes("negative-shields"));assert.ok(base.invalid.failed.includes("missing-score"));assert.ok(planFor(src).source_sha256===digest(src))});
test("complete trusted candidate fixes all malformed states and preserves all routes",()=>{const c=makeEdit(src,CLAUSES.slice(0,18));assert.equal(guardCandidate(src,c).length,18);const rows=validator(src,base)({files:{[PATH]:c},candidate_digest:digest(c)});assert.ok(rows.every(r=>r.pass),JSON.stringify(rows))});
test("partial correction rejected",()=>{const c=makeEdit(src,["source.shields < 0"]);const rows=validator(src,base)({files:{[PATH]:c},candidate_digest:digest(c)});assert.equal(rows[1].pass,false)});
test("overrestrictive correction fails actual route parity",()=>{const c=makeEdit(src,[...CLAUSES.slice(0,18),"source.units > 22"]);assert.equal(validator(src,base)({files:{[PATH]:c},candidate_digest:digest(c)})[2].pass,false)});
test("model cannot execute arbitrary code, modify elsewhere, or bypass history order",()=>{assert.throws(()=>guardCandidate(src,src.replace(MARK,"    process.exit(0);\n"+MARK)));assert.throws(()=>guardCandidate(src,makeEdit(src,CLAUSES.slice(0,18)).replace("units:22","units:23")));assert.throws(()=>guardCandidate(src,makeEdit(src,["source.history.length !== source.stage","!Array.isArray(source.history)"])))});
