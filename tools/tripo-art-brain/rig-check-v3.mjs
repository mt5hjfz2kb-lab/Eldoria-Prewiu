#!/usr/bin/env node
/**
 * OPTIONAL free Tripo v3 riggability check on a preserved source task.
 * --dry-run by default, always zero network. --live-free requires API key.
 * Never generate, upload, rig, retarget, or retry a POST automatically.
 */
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { auditAsset, loadCatalog } from './preflight.mjs';

const ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const BASE = 'https://openapi.tripo3d.ai/v3';
const ALLOWED = new Set(['spawnling', 'boar']);
const POLL_DELAY_MS = 2500;
const POLL_LIMIT = 24;

export function makeRigCheckPayload(asset) {
  if (!asset || !ALLOWED.has(asset.id) ||
      !/^[a-f0-9-]{36}$/i.test(asset.task_id) ||
      asset.new_credits !== 0 || asset.art_approved) throw new Error('RIG_CHECK_UNSAFE_SOURCE');
  return { input: asset.task_id };
}
export function assertZeroCharge(task) {
  if (!task || task.status !== 'success') throw new Error('RIG_CHECK_NOT_SUCCESSFUL');
  if (task.credits_consumed !== 0) throw new Error('RIG_CHECK_COST_NOT_CONFIRMED_ZERO');
  if (typeof task.output?.riggable !== 'boolean') throw new Error('RIG_CHECK_MISSING_RESULT');
  const t = task.output.rig_type;
  if (task.output.riggable && ![
    'biped', 'quadruped', 'hexapod', 'octopod', 'avian', 'serpentine', 'aquatic', 'others'
  ].includes(t)) throw new Error('RIG_CHECK_UNRECOGNIZED_RIG_TYPE');
  return { riggable: task.output.riggable, rig_type: t || null, credits_consumed: 0 };
}

async function jsonRequest(endpoint, key, method = 'GET', data) {
  const response = await fetch(BASE + endpoint, {
    method,
    headers: { authorization: 'Bearer ' + key, 'content-type': 'application/json' },
    body: data ? JSON.stringify(data) : undefined,
    signal: AbortSignal.timeout(25000)
  });
  if (!response.ok) throw new Error('TRIPO_API_HTTP_' + response.status);
  const result = await response.json();
  if (result.code !== 0 || !result.data) throw new Error('TRIPO_API_ERROR_' + (result.code || 'INVALID'));
  return result.data;
}

export async function liveFreeCheck(asset, key) {
  if (!key || typeof key !== 'string' || !key.startsWith('tsk_')) throw new Error('TRIPO_API_KEY_REQUIRED');
  const payload = makeRigCheckPayload(asset);
  // Exactly one free endpoint. A timeout never triggers another POST.
  const submitted = await jsonRequest('/animations/rig-check', key, 'POST', payload);
  const taskId = submitted.task_id;
  if (typeof taskId !== 'string' || !/^[a-zA-Z0-9_-]{8,128}$/.test(taskId)) {
    throw new Error('INVALID_RIG_CHECK_TASK_ID');
  }
  for (let n = 0; n < POLL_LIMIT; n++) {
    const task = await jsonRequest('/tasks/' + encodeURIComponent(taskId), key);
    if (task.status === 'failed' || task.status === 'cancelled') {
      throw new Error('RIG_CHECK_' + task.status.toUpperCase());
    }
    if (task.status === 'success') {
      return { task_id: taskId, source_task_id: asset.task_id, ...assertZeroCharge(task) };
    }
    await new Promise(resolve => setTimeout(resolve, POLL_DELAY_MS));
  }
  throw new Error('RIG_CHECK_PENDING_NO_RESUBMISSION:' + taskId);
}

async function main() {
  const [id = 'spawnling', mode = '--dry-run', flag, destination] = process.argv.slice(2);
  if (!ALLOWED.has(id) || !['--dry-run', '--live-free'].includes(mode) ||
      (flag && flag !== '--out') || (flag === '--out' && !destination)) {
    throw new Error('USAGE: node rig-check-v3.mjs spawnling|boar [--dry-run|--live-free] [--out /absolute/out.json]');
  }
  const item = auditAsset(ROOT, id, loadCatalog(ROOT));
  const payload = makeRigCheckPayload(item);
  if (mode === '--dry-run') {
    console.log(JSON.stringify({ status: 'DRY_RUN_ZERO_NETWORK', asset: id, payload,
      endpoint: BASE + '/animations/rig-check', credits_spent: 0, rig_check_executed: false,
      original_source_sha256: item.raw.sha256 }, null, 2));
    return;
  }
  const result = await liveFreeCheck(item, process.env.TRIPO_API_KEY);
  const report = { status: 'TRIPO_RIG_CHECK_ZERO_CHARGE_CONFIRMED',
    asset: id, source_sha256: item.raw.sha256, ...result,
    paid_rig_or_animation_submitted: false, art_approved: false };
  if (flag === '--out') {
    const out = path.resolve(destination);
    const rootPath = fs.realpathSync(ROOT);
    if (out === rootPath || out.startsWith(rootPath + path.sep)) throw new Error('OUTSIDE_REPO_REQUIRED');
    fs.writeFileSync(out, JSON.stringify(report, null, 2) + '\n', { flag: 'wx' });
  }
  console.log(JSON.stringify(report, null, 2));
}
if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  main().catch(error => {
    console.error('TRIPO_RIG_CHECK_FAIL: ' + error.message);
    process.exitCode = 2;
  });
}
