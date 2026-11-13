import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {EVENT_NAMES, EVENT_COUNT, ALLOWED_PAYLOAD_FIELDS} from './events-catalog.mjs';
const c=JSON.parse(readFileSync(new URL('./measurement-catalog.json',import.meta.url)));
const forbidden=/\b(card_acquired|card_state|codex_state|duel_state|equipment_state)\b/i;
test('catalogo se limita a Bastion I-V y refiere metricas validas sin duplicados',()=>{
 assert.equal(c.schema,3);assert.equal(c.scope.bastion_min,1);assert.equal(c.scope.bastion_max,5);
 assert.equal(c.metric_count,c.metrics.length);assert.equal(c.area_count,new Set(c.metrics.map(x=>x.area)).size);
 assert.equal(new Set(c.metrics.map(x=>x.id)).size,c.metrics.length);
 assert.ok(c.metrics.length>=110);
 for(const m of c.metrics){assert.ok(EVENT_NAMES.includes(m.source_event),m.id);assert.match(m.eligible_from,/^(?:I(?:-V|I-V|I|I-|V)?|II-V|III-V|III|IV|V|I-V)$/);assert.ok(m.evidence_source)}
});
test('servidor, catalogo y esquema comparten exactamente eventos I-V, sin futuras cartas',()=>{
 assert.deepEqual(c.events.map(e=>e.name),EVENT_NAMES);
 assert.equal(EVENT_COUNT,c.event_count);assert.equal(new Set(EVENT_NAMES).size,EVENT_COUNT);
 assert.doesNotMatch(EVENT_NAMES.join(' '),forbidden);
 assert.doesNotMatch(ALLOWED_PAYLOAD_FIELDS.join(' '),/rarity_code/i);
});
test('no secret ni datos personales en el contrato',()=>{
 assert.doesNotMatch(JSON.stringify(c),/service_role|secret_key|api_key|password|credit_card/i);
});
