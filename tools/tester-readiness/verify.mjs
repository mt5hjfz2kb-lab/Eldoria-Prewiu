#!/usr/bin/env node
// Eldoria: mandatory evidence check for the NEXT Unity external tester candidate.
// PASS of this program confirms evidence consistency, not artistic quality by itself.
// No Unity runner, external dependencies, paid services, or game-source changes.
import { readFileSync, existsSync } from 'node:fs';
import { resolve, relative, isAbsolute } from 'node:path';
import { createHash } from 'node:crypto';
import { fileURLToPath } from 'node:url';

export const REQUIRED_VIEWS = [
  'city-clean', 'building-selected', 'construction-in-progress',
  'dialogue-aldric', 'dialogue-lyra', 'dialogue-orin',
  'march-near', 'march-regional', 'world-panorama', 'bastion-v-city'
];
export const ORIENTATIONS = ['landscape', 'portrait'];
const SHA40 = /^[a-f0-9]{40}$/i;
const SHA64 = /^[a-f0-9]{64}$/i;
const isObject = x => x !== null && typeof x === 'object' && !Array.isArray(x);

function readJson(path) { return JSON.parse(readFileSync(path, 'utf8')); }
function sha(bytes) { return createHash('sha256').update(bytes).digest('hex'); }
export function readVerifiedEvidence(root, file, hash, errors, name) {
  if (typeof file !== 'string' || !file || typeof hash !== 'string' || !SHA64.test(hash)) {
    errors.push(name + ': path and SHA256 evidence are required'); return;
  }
  const absolute = resolve(root, file);
  const rel = relative(root, absolute);
  if (isAbsolute(rel) || rel.startsWith('..') || rel === '' || !existsSync(absolute)) {
    errors.push(name + ': evidence missing or outside checkout'); return;
  }
  if (sha(readFileSync(absolute)) !== hash.toLowerCase())
    errors.push(name + ': real evidence byte SHA256 mismatch');
}
function inspectPng(root, file, errors, label, orientation) {
  if (typeof file !== 'string' || !file.toLowerCase().endsWith('.png')) {
    errors.push(label + ': PNG file required'); return;
  }
  const full = resolve(root, file);
  const rel = relative(root, full);
  if (isAbsolute(rel) || rel.startsWith('..') || !existsSync(full)) return;
  const b = readFileSync(full);
  if (b.length < 64 || b.subarray(0, 8).toString('hex') !== '89504e470d0a1a0a' ||
      b.subarray(12, 16).toString('ascii') !== 'IHDR') {
    errors.push(label + ': invalid PNG header'); return;
  }
  const w = b.readUInt32BE(16), h = b.readUInt32BE(20);
  if (orientation === 'landscape' && !(w > h && w >= 720 && h >= 320))
    errors.push(label + ': actual Unity landscape capture resolution/orientation invalid');
  if (orientation === 'portrait' && !(h > w && h >= 720 && w >= 320))
    errors.push(label + ': actual Unity portrait capture resolution/orientation invalid');
  if (w > 8192 || h > 8192) errors.push(label + ': unreasonable image dimensions');
}
export function checkCandidate(m, root) {
  const errors = [];
  if (!isObject(m) || m.schema !== 1) return ['manifest: schema 1 required'];
  if (m.state !== 'READY') return ['candidate BLOCKED: state is not READY'];
  if (!SHA40.test(m.source_sha || '')) errors.push('source_sha: genuine 40-character Git SHA required');
  if (!SHA64.test(m.artifact_sha256 || '')) errors.push('artifact_sha256: exact built candidate digest required');
  if (!Array.isArray(m.channels) || m.channels.length < 1 ||
      m.channels.some(x => !['web', 'android'].includes(x)) ||
      new Set(m.channels).size !== m.channels.length)
    errors.push('channels: at least one distinct real distribution channel required');

  const screens = Array.isArray(m.visual_evidence) ? m.visual_evidence : [];
  for (const view of REQUIRED_VIEWS) for (const orientation of ORIENTATIONS) {
    const name = view + '/' + orientation;
    const matches = screens.filter(x => isObject(x) && x.view === view && x.orientation === orientation);
    if (matches.length !== 1) { errors.push('visual: expected exactly one actual Unity screenshot for ' + name); continue; }
    const s = matches[0];
    readVerifiedEvidence(root, s.reference_path, s.reference_sha256, errors, name + ' reference');
    readVerifiedEvidence(root, s.actual_path, s.actual_sha256, errors, name + ' Unity capture');
    inspectPng(root, s.reference_path, errors, name + ' reference', null);
    inspectPng(root, s.actual_path, errors, name + ' Unity capture', orientation);
    if (s.reference_path === s.actual_path || s.reference_sha256 === s.actual_sha256)
      errors.push(name + ': concept/reference cannot be the same bytes as a real Unity capture');
    if (s.source_sha !== m.source_sha || !Number.isSafeInteger(s.native_run_id) || s.native_run_id <= 0)
      errors.push(name + ': actual Unity capture must be tied to exact native source and run');
    if (!isObject(s.review) || s.review.verdict !== 'APPROVED' ||
        typeof s.review.reviewer !== 'string' || !s.review.reviewer.trim() ||
        s.review.reviewer === 'AUTOMATED_PIXEL_SCORE' || s.review.same_camera !== true ||
        s.review.legible_on_mobile !== true || s.review.interactions_clear !== true)
      errors.push(name + ': independent visual/UX review and matched camera required');
  }
  if (screens.length !== REQUIRED_VIEWS.length * ORIENTATIONS.length)
    errors.push('visual: unexpected/duplicate or missing screenshot entries');
  const actualDigests = screens.map(x => x?.actual_sha256);
  if (new Set(actualDigests).size !== actualDigests.length)
    errors.push('visual: repeated captures cannot certify different Unity screens');

  const g = m.gameplay;
  if (!isObject(g) || g.status !== 'PASS' || g.source_sha !== m.source_sha ||
      !Number.isSafeInteger(g.native_run_id) || g.native_run_id <= 0 ||
      g.uninterrupted_from_fresh_save !== true || g.no_debug_skips !== true ||
      JSON.stringify(g.bastions) !== '[1,2,3,4,5]' ||
      g.buttons_and_timers_pass !== true || g.saves_and_reload_pass !== true ||
      g.marches_and_combat_pass !== true)
    errors.push('gameplay: real exact-source uninterrupted Bastion I-V Unity gate missing');
  if (isObject(g)) readVerifiedEvidence(root, g.evidence_path, g.evidence_sha256, errors, 'gameplay test log');

  const devices = Array.isArray(m.device_tests) ? m.device_tests : [];
  for (const channel of m.channels || []) {
    const matching = devices.filter(x => isObject(x) && x.channel === channel);
    if (matching.length !== 1) { errors.push(channel + ': one actual physical-device test required'); continue; }
    const d = matching[0];
    if (d.status !== 'PASS' || d.physical_device !== true ||
        !d.model || !d.os || d.source_sha !== m.source_sha ||
        d.artifact_sha256 !== m.artifact_sha256)
      errors.push(channel + ': physical device, exact candidate and PASS required');
    readVerifiedEvidence(root, d.evidence_path, d.evidence_sha256, errors, channel + ' device QA');
  }
  if (devices.length !== (m.channels || []).length) errors.push('device_tests: unexpected or incomplete device evidence');

  const delivery = m.delivery;
  for (const channel of m.channels || []) {
    const d = isObject(delivery) ? delivery[channel] : null;
    if (!isObject(d) || d.status !== 'PASS' || d.hosted_gameplay_verified !== true ||
        d.source_sha !== m.source_sha || d.artifact_sha256 !== m.artifact_sha256)
      errors.push(channel + ': exact published/hosted artifact has not passed gameplay QA');
    if (isObject(d)) readVerifiedEvidence(root, d.evidence_path, d.evidence_sha256, errors, channel + ' hosted QA');
  }
  const owner = m.final_review;
  if (!isObject(owner) || owner.verdict !== 'APPROVED' || owner.source_sha !== m.source_sha ||
      typeof owner.reviewer !== 'string' || !owner.reviewer.trim())
    errors.push('final_review: owner artistic approval of exact candidate missing');
  if (isObject(owner)) readVerifiedEvidence(root, owner.evidence_path, owner.evidence_sha256, errors, 'owner review');
  return errors;
}
// Internal owner builds (itch Restricted, family APK, old Pages) are allowed
// as candidates: blocking them would prevent physical/hosted verification.
// Only the separate NEXT EXTERNAL TESTER promotion is subject to this gate.
export function activeReleaseRequests(root) {
  const p = 'pipeline/tester-readiness/release-request.json';
  try {
    const x = readJson(resolve(root, p));
    return x.enabled === true ? [p] : [];
  } catch { return [p]; } // fail closed if the external tester request is unreadable
}
export function verify(root, opts = {}) {
  const manifestPath = resolve(root, 'pipeline/tester-readiness/candidate.json');
  let manifest, errors = [];
  try { manifest = readJson(manifestPath); } catch { errors.push('candidate manifest unreadable'); }
  const active = activeReleaseRequests(root);
  const gate = opts.requireReady === true || active.length > 0;
  if (manifest && (gate || manifest.state === 'READY')) errors.push(...checkCandidate(manifest, root));
  // When publishing is enabled, evidence is mandatory; valid evidence can pass.
  // This monitoring check does not replace a direct publisher preflight invocation.
  // An enabled production release is never silently waved through by this
  // pre-merge monitoring job. Wire verify --require-ready into the publishing
  // preflight itself when that currently-owned workflow is available to edit.
  return { status: errors.length ? 'FAIL' : gate ? 'READY_EVIDENCE_CONSISTENT' : 'BLOCKED_NOT_RELEASE_READY',
    releaseEnabled: active.length > 0, errors };
}
if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const root = resolve(process.cwd());
  const result = verify(root, { requireReady: process.argv.includes('--require-ready') });
  console.log(JSON.stringify(result, null, 2));
  if (result.errors.length) process.exitCode = 1;
}
