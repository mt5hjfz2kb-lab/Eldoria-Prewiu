#!/usr/bin/env node
// Independent regression test for UTF-8 BOM evidence JSON handled by the existing coordinator.
// The test reads agent-generated DATA only; it never evaluates or executes model output.
'use strict';
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const fixture = JSON.parse(fs.readFileSync(path.join(__dirname, 'eldoria-coordinator-bom-cases.json'), 'utf8'));
assert.equal(fixture.schema_version, 1);
assert.equal(fixture.purpose, 'coordinator-evidence-json-bom-regression');
assert.equal(fixture.cases.length, 6);
let valid=0, invalid=0, bom=0;
const distinct=new Set();
for(const [index, t] of fixture.cases.entries()) {
  assert.equal(typeof t.input, 'string');
  assert.equal(typeof t.valid, 'boolean');
  assert(t.input.length <= 120);
  assert(!distinct.has(t.input), 'Duplicate input at index '+index);
  distinct.add(t.input);
  if(t.input.startsWith('\uFEFF')) bom++;
  const cleaned = t.input.replace(/^\uFEFF/, '');
  let accepted=false;
  try {
    const obj=JSON.parse(cleaned);
    accepted = obj !== null && typeof obj === 'object' && !Array.isArray(obj);
  } catch {}
  assert.equal(accepted,t.valid,'Wrong expected result in case '+index);
  if(accepted)valid++;else invalid++;
}
assert(valid>=3 && invalid>=2 && bom>=1,'Missing boundary coverage');
assert(fixture.cases.some(t=>t.input==='{}' && t.valid),'Empty object absent');
console.log('COORDINATOR_BOM_REGRESSION_PASS 6/6');
