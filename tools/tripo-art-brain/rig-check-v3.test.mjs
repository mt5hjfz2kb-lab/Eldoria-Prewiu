import { test } from 'node:test';
import assert from 'node:assert/strict';
import { makeRigCheckPayload, assertZeroCharge, liveFreeCheck } from './rig-check-v3.mjs';

const safeAsset = {
  id: 'spawnling',
  task_id: '351d1034-25ec-4300-a2e0-d03a0e68175a',
  new_credits: 0, art_approved: false
};

test('free endpoint gets ONLY a previously created model task ID', () => {
  assert.deepEqual(makeRigCheckPayload(safeAsset), { input: safeAsset.task_id });
  assert.throws(() => makeRigCheckPayload({ ...safeAsset, id: 'focus' }), /RIG_CHECK_UNSAFE_SOURCE/);
  assert.throws(() => makeRigCheckPayload({ ...safeAsset, new_credits: 1 }), /RIG_CHECK_UNSAFE_SOURCE/);
  assert.throws(() => makeRigCheckPayload({ ...safeAsset, art_approved: true }), /RIG_CHECK_UNSAFE_SOURCE/);
});

test('zero-cost confirmation requires a real successful task and documented boolean', () => {
  assert.deepEqual(assertZeroCharge({ status: 'success', credits_consumed: 0, output: {
    riggable: true, rig_type: 'biped'
  }}), { riggable: true, rig_type: 'biped', credits_consumed: 0 });
  assert.deepEqual(assertZeroCharge({ status: 'success', credits_consumed: 0, output: {
    riggable: false
  }}), { riggable: false, rig_type: null, credits_consumed: 0 });
});

test('missing charge receipt, nonzero credits, invalid result and unknown rig type fail closed', () => {
  assert.throws(() => assertZeroCharge({ status: 'success', output: { riggable: true } }), /RIG_CHECK_COST_NOT_CONFIRMED_ZERO/);
  assert.throws(() => assertZeroCharge({ status: 'success', credits_consumed: 25, output: { riggable: true } }), /RIG_CHECK_COST_NOT_CONFIRMED_ZERO/);
  assert.throws(() => assertZeroCharge({ status: 'failed', credits_consumed: 0, output: { riggable: false } }), /RIG_CHECK_NOT_SUCCESSFUL/);
  assert.throws(() => assertZeroCharge({ status: 'success', credits_consumed: 0, output: { riggable: 'yes' } }), /RIG_CHECK_MISSING_RESULT/);
  assert.throws(() => assertZeroCharge({ status: 'success', credits_consumed: 0, output: { riggable: true, rig_type: 'dragon' } }), /RIG_CHECK_UNRECOGNIZED_RIG_TYPE/);
});
test('live API check cannot start without a valid private key', async () => {
  await assert.rejects(liveFreeCheck(safeAsset, undefined), /TRIPO_API_KEY_REQUIRED/);
  await assert.rejects(liveFreeCheck(safeAsset, 'wrong'), /TRIPO_API_KEY_REQUIRED/);
});
