#!/usr/bin/env node
'use strict';

// Run against the exact prepared WebGL artifact. All game input is a visible
// mouse/touch gesture; the opt-in console stream is observation only.
// VALORIA_WEB_ROOT=/path/to/build node tools/valoria-reference-clean/playtest_web.cjs
// Optional: VALORIA_PLAYTEST_OUT, VALORIA_SOURCE_SHA, VALORIA_PLAYTEST_TIMEOUT_SECONDS,
// VALORIA_WEB_LOAD_TIMEOUT_SECONDS, VALORIA_PLAYTEST_VIDEO=0, VALORIA_CHROMIUM_EXECUTABLE.
const fs = require('node:fs');
const fsp = require('node:fs/promises');
const path = require('node:path');
const http = require('node:http');
const crypto = require('node:crypto');
const { performance } = require('node:perf_hooks');
const { spawnSync } = require('node:child_process');

const PREFIX = 'VALORIA_PLAYTEST ';
const VIEWS = { landscape: { width: 960, height: 540 }, portrait: { width: 540, height: 960 } };
const sleep = ms => new Promise(resolve => setTimeout(resolve, ms));
const sha256 = value => crypto.createHash('sha256').update(value).digest('hex');
const round = value => Math.round(value * 1000) / 1000;
const slug = value => String(value).replace(/[^a-zA-Z0-9_-]+/g, '-').replace(/^-|-$/g, '').slice(0, 90);
const phase = state => typeof state?.march === 'string' ? state.march : state?.march?.phase || state?.march?.Phase || 'unknown';
const target = state => state?.target || state?.march?.target || state?.march?.TargetId || '';
const orientation = state => state?.width > state?.height ? 'landscape' : 'portrait';
const summary = state => state && ({
  width: state.width, height: state.height, inWorld: state.inWorld, selected: state.selected,
  objective: state.objective, narrative: state.narrative, revision: state.revision,
  wood: state.wood, stone: state.stone, sawmill: state.sawmill, barracks: state.barracks,
  bastion: state.bastion, trained: state.trained, configured: state.configured,
  scout: state.scout, engendro: state.engendro, busy: state.busy,
  march: phase(state), target: target(state), camera: state.camera, framing: state.framing || null,
  controls: state.controls ? { primary: state.controls.primary } : null,
  save_sha256: typeof state.save === 'string' ? sha256(state.save) : null
});

function need(condition, message) { if (!condition) throw new Error(message); }
function numericCamera(state) {
  const c = state?.camera;
  return c && ['x', 'y', 'z', 'zoom'].every(key => Number.isFinite(c[key])) ? c : null;
}
function cameraDifference(before, after) {
  const a = numericCamera(before), b = numericCamera(after);
  if (!a || !b) return null;
  return { dx: b.x - a.x, dy: b.y - a.y, dz: b.z - a.z,
    distance: Math.hypot(b.x - a.x, b.y - a.y, b.z - a.z), zoom: b.zoom - a.zoom };
}
function framingObservation(state) {
  const frame = state?.framing, projected = frame?.projected, visible = frame?.visible;
  const valid = rect => Array.isArray(rect) && rect.length === 4 && rect.every(Number.isFinite) && rect[2] > rect[0] && rect[3] > rect[1];
  if (!valid(projected) || !valid(visible)) return { valid: false, pass: false, projected, visible };
  const margins = { left: projected[0] - visible[0], bottom: projected[1] - visible[1],
    right: visible[2] - projected[2], top: visible[3] - projected[3] };
  return { valid: true, pass: frame.pass === true && Object.values(margins).every(value => value >= 0),
    engine_pass: frame.pass === true, projected, visible, margins_normalized: margins };
}
function stableSelectionView(before, after) {
  if (!before?.selected || before.selected !== after?.selected || before.inWorld !== after.inWorld ||
      before.width !== after.width || before.height !== after.height || before.narrative || after.narrative) return false;
  const camera = cameraDifference(before, after), a = framingObservation(before), b = framingObservation(after);
  if (!camera || camera.distance > 0.01 || Math.abs(camera.zoom) > 0.001 || !a.valid || !b.valid) return false;
  if (['projected', 'visible'].some(rect => a[rect].some((value, index) => Math.abs(value - b[rect][index]) > 0.0001))) return false;
  const first = before.controls?.primary, second = after.controls?.primary;
  return !first || !second || (first.enabled === second.enabled && Math.abs(first.x - second.x) <= 0.5 && Math.abs(first.y - second.y) <= 0.5);
}
function hotspotMap(state) {
  const list = state?.hotspots || [];
  return Array.isArray(list) ? Object.fromEntries(list.map(item => [item.id, item])) : list;
}
function projectedMovement(before, after) {
  const a = hotspotMap(before), b = hotspotMap(after), points = [];
  for (const [id, p] of Object.entries(a)) {
    const q = b[id];
    if (q && p.visible !== false && q.visible !== false && [p.x, p.y, q.x, q.y].every(Number.isFinite))
      points.push({ id, dx: (q.x / after.width - p.x / before.width) * 1000,
        dy: (q.y / after.height - p.y / before.height) * 1000 });
  }
  const median = list => list.sort((x, y) => x - y)[Math.floor(list.length / 2)];
  return points.length ? { common: points.length, dx_per_mille: median(points.map(p => p.dx)),
    dy_per_mille: median(points.map(p => p.dy)), points } : null;
}
function analyseMarchVisual(samples) {
  const result = {};
  for (const stage of ['outbound', 'gathering', 'returning']) {
    const frames = samples.filter(sample => sample.phase === stage);
    const visible = frames.filter(sample => sample.visible);
    const pairs = [];
    for (let i = 1; i < frames.length; i++) {
      const a = frames[i - 1], b = frames[i], dt = b.elapsed_s - a.elapsed_s;
      const camera = cameraDifference(a, b);
      if (!a.visible || !b.visible || a.width !== b.width || a.height !== b.height ||
          dt < 0.05 || dt > 3 || !camera || camera.distance > 0.01 || Math.abs(camera.zoom) > 0.001) continue;
      const dx = b.x - a.x, dy = b.y - a.y;
      pairs.push({ from_elapsed_s: a.elapsed_s, to_elapsed_s: b.elapsed_s,
        seconds: round(dt), dx_px: round(dx), dy_px: round(dy),
        distance_px: round(Math.hypot(dx, dy)),
        distance_per_mille: round(Math.hypot(dx / b.width, dy / b.height) * 1000),
        camera_distance: camera.distance, camera_zoom_delta: camera.zoom });
    }
    const moving = pairs.filter(pair => pair.distance_per_mille >= 1);
    result[stage] = { samples: frames.length, visible_samples: visible.length,
      stable_camera_pairs: pairs.length, moving_pairs: moving.length,
      status: moving.length ? 'OBSERVED_DISPLACEMENT' : visible.length < 2 ? 'INSUFFICIENT_VISIBLE_SAMPLES' :
        !pairs.length ? 'NO_STABLE_CAMERA_PAIR' : 'NO_DISPLACEMENT_RESOLVED',
      sampled_seconds: round(pairs.reduce((sum, pair) => sum + pair.seconds, 0)),
      displacement_px: round(pairs.reduce((sum, pair) => sum + pair.distance_px, 0)), pairs };
  }
  return result;
}
function mimeType(filename) {
  const plain = filename.replace(/\.(gz|br)$/i, '');
  if (/\.unityweb$/i.test(filename)) return 'application/octet-stream';
  if (/\.wasm$/i.test(plain)) return 'application/wasm';
  if (/\.m?js$/i.test(plain)) return 'application/javascript; charset=utf-8';
  if (/\.html?$/i.test(plain)) return 'text/html; charset=utf-8';
  if (/\.css$/i.test(plain)) return 'text/css; charset=utf-8';
  if (/\.json$/i.test(plain)) return 'application/json; charset=utf-8';
  if (/\.png$/i.test(plain)) return 'image/png';
  if (/\.jpe?g$/i.test(plain)) return 'image/jpeg';
  if (/\.svg$/i.test(plain)) return 'image/svg+xml';
  if (/\.ico$/i.test(plain)) return 'image/x-icon';
  return 'application/octet-stream';
}

async function serveBuild(directory) {
  const root = await fsp.realpath(directory), requests = [];
  need((await fsp.stat(path.join(root, 'index.html'))).isFile(), 'VALORIA_WEB_ROOT must contain the prepared index.html');
  const server = http.createServer(async (req, res) => {
    const record = { started_ms: performance.now(), method: req.method, path: '', status: 500, bytes: 0 };
    requests.push(record);
    const end = (status, body = '') => {
      record.status = status; record.bytes = Buffer.byteLength(body);
      res.writeHead(status); res.end(req.method === 'HEAD' ? undefined : body);
    };
    try {
      if (!['GET', 'HEAD'].includes(req.method)) { end(405, 'Read-only build server'); return; }
      const pathname = decodeURIComponent(new URL(req.url, 'http://127.0.0.1').pathname);
      record.path = pathname;
      const resolved = path.resolve(root, '.' + (pathname === '/' ? '/index.html' : pathname));
      if (!resolved.startsWith(root + path.sep)) { end(403, 'Outside build root'); return; }
      const filename = await fsp.realpath(resolved);
      if (!filename.startsWith(root + path.sep)) { end(403, 'Outside build root'); return; }
      const stat = await fsp.stat(filename);
      if (!stat.isFile()) { end(404, 'Not found'); return; }
      const etag = '"' + stat.size.toString(16) + '-' + Math.trunc(stat.mtimeMs).toString(16) + '"';
      res.setHeader('Content-Type', mimeType(filename));
      res.setHeader('Cache-Control', /\.html?$/i.test(filename) ? 'no-cache' : 'private, max-age=86400');
      res.setHeader('ETag', etag);
      res.setHeader('Last-Modified', stat.mtime.toUTCString());
      res.setHeader('X-Content-Type-Options', 'nosniff');
      // Unity .unityweb fallback files are deliberately served as raw bytes.
      if (/\.gz$/i.test(filename)) res.setHeader('Content-Encoding', 'gzip');
      if (/\.br$/i.test(filename)) res.setHeader('Content-Encoding', 'br');
      if (req.headers['if-none-match'] === etag) { end(304); return; }
      record.status = 200; record.bytes = req.method === 'HEAD' ? 0 : stat.size;
      res.setHeader('Content-Length', stat.size);
      res.writeHead(200);
      if (req.method === 'HEAD') res.end();
      else fs.createReadStream(filename).on('error', error => res.destroy(error)).pipe(res);
    } catch (error) {
      if (!res.headersSent) end(error.code === 'ENOENT' ? 404 : 500, error.code === 'ENOENT' ? 'Not found' : 'Build server error');
      else res.destroy(error);
    }
  });
  await new Promise((resolve, reject) => { server.once('error', reject); server.listen(0, '127.0.0.1', resolve); });
  return { root, requests, url: `http://127.0.0.1:${server.address().port}/`,
    close: () => new Promise(resolve => { server.close(resolve); server.closeIdleConnections?.(); }) };
}

async function fingerprint(directory) {
  const files = [];
  async function walk(relative = '') {
    for (const entry of await fsp.readdir(path.join(directory, relative), { withFileTypes: true })) {
      const name = path.join(relative, entry.name);
      if (entry.isDirectory()) await walk(name);
      else if (entry.isFile()) {
        const hash = crypto.createHash('sha256');
        for await (const chunk of fs.createReadStream(path.join(directory, name))) hash.update(chunk);
        files.push({ path: name.split(path.sep).join('/'), bytes: (await fsp.stat(path.join(directory, name))).size,
          sha256: hash.digest('hex') });
      }
    }
  }
  await walk(); files.sort((a, b) => a.path.localeCompare(b.path));
  return { bytes: files.reduce((sum, file) => sum + file.bytes, 0), files, manifest_sha256: sha256(JSON.stringify(files)) };
}

async function main() {
  const buildRoot = process.env.VALORIA_WEB_ROOT;
  need(buildRoot, 'Set VALORIA_WEB_ROOT to the downloaded, prepared WebGL build directory');
  const out = path.resolve(process.env.VALORIA_PLAYTEST_OUT || 'docs/evidence/valoria-reference-clean/web-playtest');
  need(!fs.existsSync(out) || fs.readdirSync(out).length === 0, 'VALORIA_PLAYTEST_OUT must be empty so separate playthrough evidence cannot be mixed');
  fs.mkdirSync(path.join(out, 'screenshots'), { recursive: true });
  const born = performance.now();
  const report = {
    schema_version: 1, status: 'RUNNING', started_at: new Date().toISOString(),
    source_sha: process.env.VALORIA_SOURCE_SHA || process.env.GITHUB_SHA || null,
    source_sha_provenance: 'Supplied by the build job; exact tested bytes are identified by build_manifest.json',
    scope: 'LOCAL_WEB_VISIBLE_CONTROLS_NORMAL_TIME_I_II',
    method: { fresh_browser_context: true, engine_commands: false, game_clock_modified: false,
      input: 'Playwright mouse and emulated touchscreen on visible canvas controls',
      manual_human_playthrough: false, physical_device_tested: false, sites_or_authentication_used: false,
      visual_review: 'REQUIRED: inspect screenshots and continuous browser video independently' },
    measurements_scope: 'Local loopback server and CI Chromium; these are not Internet, Sites, Safari, physical-phone or FPS measurements',
    march_visual_measurement: 'Consecutive visible marker samples with unchanged viewport and stable camera (distance <= 0.01 world units, zoom delta <= 0.001). Resolved displacement >= 1 per mille of screen dimensions. Missing/out-of-frame/unstable samples do not prove missing animation. Inspect video for perceived motion and occlusion.',
    selection_framing_measurement: 'Before each real primary action and after a completed operation that retains its selection/view: at least three telemetry frames across >=0.7 s with the same selection, viewport, camera and panel/projection bounds. Completion observations allow objective advancement and need no enabled next action; view/selection/narrative changes are recorded as interruptions. Full projected model envelope must be inside the HUD free viewport; transient camera/layout frames are observed without failing immediately. Rectangles use Unity normalized bottom-left viewport coordinates.',
    cold_load_definition: 'Empty browser context with no prior HTTP/IndexedDB cache; operating-system disk cache is not claimed empty',
    checks: {}, loads: [], orientations: [], actions: [], operations: [], marches: [], navigation: [], selection_framing: [],
    transitions: [], screenshots: [], failures: [], warnings: [], runtime_errors: [], network_failures: [], diagnostics: []
  };
  const ndjson = (file, value) => fs.appendFileSync(path.join(out, file), JSON.stringify(value) + '\n');
  const elapsed = () => round((performance.now() - born) / 1000);
  let server, browser, context, page, latest = null, sequence = 0, lastTelemetryAt = 0, documentCommittedAt = 0;
  let videoStartedAt = 0;
  let activeOperation = null, currentMarch = null, captureNumber = 0, previousFrame = null;
  let flowDeadline = Infinity, crashed = false, telemetryBroken = false;
  const actionKeys = new Set(), capturedPhases = new Set(), seenObjectives = new Set();
  const loadTimeout = Number(process.env.VALORIA_WEB_LOAD_TIMEOUT_SECONDS || 180) * 1000;
  const flowTimeout = Number(process.env.VALORIA_PLAYTEST_TIMEOUT_SECONDS || 300) * 1000;
  need(Number.isFinite(loadTimeout) && loadTimeout >= 10000 && Number.isFinite(flowTimeout) && flowTimeout >= 10000, 'Invalid timeout configuration');

  function recordFrame(state) {
    const emittedAt = Date.parse(state.utc);
    need(Number.isFinite(emittedAt), 'Telemetry has no valid observation timestamp');
    if (emittedAt < documentCommittedAt) return;
    const now = performance.now();
    latest = state; sequence++; lastTelemetryAt = now;
    ndjson('telemetry.ndjson', { elapsed_s: elapsed(), sequence, state });
    if (state.clock !== 'SystemClock' || !Number.isFinite(state.timeScale) || Math.abs(state.timeScale - 1) > 0.00001) {
      telemetryBroken = true;
      report.failures.push(`Unexpected runtime timing contract: clock=${state.clock}, scale=${state.timeScale}`);
    }
    if (previousFrame && previousFrame.inWorld !== state.inWorld)
      report.transitions.push({ elapsed_s: elapsed(), from: previousFrame.inWorld ? 'world' : 'city', to: state.inWorld ? 'world' : 'city', revision: state.revision });
    const p = phase(state), oldPhase = previousFrame ? phase(previousFrame) : 'idle';
    if (p !== 'idle' && (!currentMarch || (currentMarch.target && target(state) !== currentMarch.target))) {
      currentMarch = { index: report.marches.length + 1, target: target(state), started_elapsed_s: elapsed(),
        start_resources: { wood: previousFrame?.wood ?? state.wood, stone: previousFrame?.stone ?? state.stone },
        phases: [], visual_samples: [], visual_motion: null, completed: false };
      report.marches.push(currentMarch);
    }
    if (currentMarch && p !== 'idle') {
      const marker = state.marchVisual;
      currentMarch.visual_samples.push({ elapsed_s: elapsed(), phase: p, marker_phase: marker?.id || null,
        x: marker?.x ?? null, y: marker?.y ?? null, width: state.width, height: state.height, camera: state.camera,
        visible: !!(state.inWorld && marker?.visible && marker.id === p &&
          Number.isFinite(marker.x) && Number.isFinite(marker.y) &&
          marker.x > 0 && marker.x < state.width && marker.y > 0 && marker.y < state.height) });
      currentMarch.visual_motion = analyseMarchVisual(currentMarch.visual_samples);
    }
    if (currentMarch && (p !== oldPhase || currentMarch.phases.length === 0)) {
      let saved = null; try { saved = JSON.parse(state.save); } catch { /* validated separately */ }
      currentMarch.phases.push({ phase: p, elapsed_s: elapsed(), revision: state.revision, inWorld: state.inWorld,
        pending_wood: saved?.March?.PendingWood ?? null, pending_stone: saved?.March?.PendingStone ?? null });
      if (p === 'idle') {
        currentMarch.completed = true; currentMarch.finished_elapsed_s = elapsed();
        currentMarch.observed_seconds = round(currentMarch.finished_elapsed_s - currentMarch.started_elapsed_s);
        currentMarch.returned_resource_delta = { wood: state.wood - currentMarch.start_resources.wood, stone: state.stone - currentMarch.start_resources.stone };
        const returning = currentMarch.phases.find(entry => entry.phase === 'returning');
        currentMarch.pending_rewards_delivered = !!returning &&
          currentMarch.returned_resource_delta.wood >= (returning.pending_wood || 0) &&
          currentMarch.returned_resource_delta.stone >= (returning.pending_stone || 0);
        currentMarch = null;
      }
    }
    if (activeOperation) {
      const operation = activeOperation;
      if (!operation.accepted && state.revision > operation.before.revision) {
        operation.accepted = true; operation.accepted_elapsed_s = elapsed(); operation.accepted_revision = state.revision;
        operation.immediate_wallet_delta = { wood: state.wood - operation.before.wood, stone: state.stone - operation.before.stone };
      }
      const finished = operation.accepted && !state.busy && (
        operation.kind === 'sawmill' ? state.sawmill > operation.before.sawmill :
        operation.kind === 'barracks' ? state.barracks > operation.before.barracks :
        operation.kind === 'bastion' ? state.bastion > operation.before.bastion :
        operation.kind === 'training' ? state.trained > operation.before.trained :
        operation.kind === 'configuration' ? state.configured : p === 'idle');
      if (finished && !operation.completed) {
        operation.completed = true; operation.completed_elapsed_s = elapsed();
        operation.observed_seconds = round(operation.completed_elapsed_s - operation.started_elapsed_s);
        operation.after = summary(state);
        operation.normal_duration_observed = operation.expected_seconds == null || operation.observed_seconds >= operation.expected_seconds - 0.8;
      }
    }
    previousFrame = state;
  }

  function health() {
    need(!crashed, 'Chromium page crashed');
    need(!telemetryBroken, 'Runtime timing or telemetry contract failed; see report');
    need(performance.now() < flowDeadline, 'Normal-time playthrough exceeded its bounded timeout');
    if (latest) need(performance.now() - lastTelemetryAt < 20000, 'The game stopped emitting ready telemetry for 20 seconds');
  }
  async function waitState(predicate, label, timeout = 10000, after = -1) {
    const deadline = performance.now() + timeout;
    while (performance.now() < deadline) {
      health();
      if (latest && sequence > after && predicate(latest)) return latest;
      await sleep(80);
    }
    throw new Error(`${label}: timed out after ${timeout / 1000}s; last state ${JSON.stringify(summary(latest))}`);
  }
  async function canvasBox() {
    const box = await page.locator('canvas').first().boundingBox();
    need(box && box.width > 20 && box.height > 20, 'Visible Unity canvas not found');
    return box;
  }
  async function screenPoint(point, state = latest) {
    need(state && Number.isFinite(point?.x) && Number.isFinite(point?.y) && state.width > 0 && state.height > 0, 'Missing screen-space point');
    need(point.x > 0 && point.x < state.width && point.y > 0 && point.y < state.height, 'Control or hotspot is outside the rendered canvas');
    const box = await canvasBox();
    const result = { x: box.x + point.x / state.width * box.width, y: box.y + (1 - point.y / state.height) * box.height };
    const viewport = page.viewportSize();
    need(result.x > 0 && result.y > 0 && result.x < viewport.width && result.y < viewport.height, 'Visible control is clipped outside the viewport');
    return result;
  }
  async function clickPoint(point, label, mode) {
    const before = latest, position = await screenPoint(point, before);
    const used = mode || (orientation(before) === 'portrait' ? 'touch' : 'mouse');
    const action = { elapsed_s: elapsed(), kind: used === 'touch' ? 'touch-hold-release' : 'click',
      label, css: position, held_ms: used === 'touch' ? 160 : 90, before: summary(before) };
    report.actions.push(action); ndjson('actions.ndjson', action);
    if (used === 'touch') {
      // Playwright touchscreen.tap dispatches start and end with no human-length
      // hold. In native139 Windows GPU the portrait City control missed the
      // immediate tap after genuine world drags, while all four pan directions
      // and wheel zoom worked. Exercise Chromium's real touch input protocol
      // with an ordinary 160ms press, never DOM events or Unity/Button.Invoke.
      const cdp = await page.context().newCDPSession(page);
      const point = { x: position.x, y: position.y, radiusX: 2, radiusY: 2, force: 1 };
      let down = false;
      try {
        await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [point] });
        down = true;
        await sleep(160);
        await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });
        down = false;
      } finally {
        if (down) await cdp.send('Input.dispatchTouchEvent', { type: 'touchCancel', touchPoints: [] }).catch(() => {});
        await cdp.detach().catch(() => {});
      }
    } else await page.mouse.click(position.x, position.y, { delay: 90 });
    // A telemetry frame emitted during pointer-down predates the click's effect.
    // Require another observation after actual pointer-up/touch-end.
    const deliveredSequence = sequence;
    await waitState(() => true, `Fresh observation after ${label}`, 10000, deliveredSequence);
    return before;
  }
  async function clickControl(name) {
    const control = latest?.controls?.[name];
    need(control?.enabled, `Visible control '${name}' is unavailable (${latest?.objective}, selected=${latest?.selected})`);
    return clickPoint(control, name);
  }
  async function capture(label, framingCheck = null) {
    if (captureNumber >= 75) { report.warnings.push('Screenshot cap reached; video and telemetry remain available'); return; }
    const before = latest, started = elapsed();
    const videoTime = Math.max(0, (performance.now() - videoStartedAt) / 1000);
    const relative = `screenshots/${String(++captureNumber).padStart(3, '0')}-${slug(label)}-${orientation(before)}.jpg`;
    // SwiftShader did not answer either live Chromium compositor screenshot.
    // Mark the instant in the continuously recorded REAL browser video; extract
    // that exact time after the WebM closes. The game input and clock are untouched.
    report.screenshots.push({ path: relative, label,
      video_second: round(videoTime), source: 'CHROMIUM_RECORDED_VIDEO_FRAME',
      requested_elapsed_s: started, before: summary(before), after: summary(latest),
      phase_stable_during_capture: true, framing_check: framingCheck,
      full_selection_framed_before: framingObservation(before).pass,
      full_selection_framed_after: framingObservation(before).pass,
      extraction_pending: true });
  }
  async function waitForSelectionFraming(afterOperation = false) {
    const initial = latest, started = performance.now();
    const check = { index: report.selection_framing.length + 1, selected: initial.selected, objective: initial.objective,
      orientation: orientation(initial), inWorld: initial.inWorld, started_elapsed_s: elapsed(),
      context: afterOperation ? 'after-operation' : 'before-primary-action',
      status: 'OBSERVING', pass: false, samples: [] };
    report.selection_framing.push(check);
    let anchor = null, stableSince = 0, stableFrames = 0, lastSequence = -1;
    while (performance.now() - started < 12000) {
      health();
      if (sequence === lastSequence) { await sleep(80); continue; }
      lastSequence = sequence;
      const state = latest, observation = framingObservation(state), now = performance.now();
      check.samples.push({ elapsed_s: elapsed(), sequence, camera: state.camera, ...observation });
      if (state.narrative || state.busy || phase(state) !== 'idle' || state.selected !== initial.selected ||
          (!afterOperation && state.objective !== initial.objective) || state.inWorld !== initial.inWorld ||
          state.width !== initial.width || state.height !== initial.height) {
        check.status = 'INTERRUPTED_BY_GAME_TRANSITION'; check.finished_elapsed_s = elapsed(); return null;
      }
      if (!observation.valid || (!afterOperation && !state.controls?.primary?.enabled)) {
        anchor = null; stableFrames = 0; await sleep(80); continue;
      }
      if (!anchor || !stableSelectionView(anchor, state)) { anchor = state; stableSince = now; stableFrames = 1; }
      else stableFrames++;
      check.stable_frames = stableFrames; check.stable_seconds = round((now - stableSince) / 1000);
      if (stableFrames >= 3 && now - stableSince >= 700 && observation.pass) {
        check.status = 'PASS'; check.pass = true; check.finished_elapsed_s = elapsed();
        check.observation = observation; check.after = summary(state); return check;
      }
      await sleep(80);
    }
    check.status = !framingObservation(latest).valid ? 'MISSING_FRAMING_OBSERVATION' :
      check.stable_frames >= 3 && check.stable_seconds >= 0.7 ? 'STABLE_SELECTION_OUTSIDE_FREE_VIEWPORT' : 'CAMERA_OR_PANEL_DID_NOT_SETTLE';
    check.finished_elapsed_s = elapsed(); check.after = summary(latest);
    await capture(`selection-framing-failure-${check.selected}`, check.index);
    throw new Error(`Selection framing ${check.status}: ${check.selected} in ${check.orientation}; see normalized bounds and settled-frame samples`);
  }
  async function setOrientation(name) {
    if (page.viewportSize().width === VIEWS[name].width && orientation(latest) === name) return;
    const seq = sequence;
    await page.setViewportSize(VIEWS[name]);
    await waitState(state => orientation(state) === name, `Canvas responds to ${name} viewport`, 15000, seq);
    const box = await canvasBox();
    need((name === 'portrait') === (box.height > box.width), 'Exported canvas did not follow viewport orientation; fix the build wrapper, not the test DOM');
    if (!report.orientations.includes(name)) report.orientations.push(name);
    await sleep(250);
  }
  async function dismissNarrative() {
    let attempts = 0;
    while (latest?.narrative) {
      need(++attempts <= 8, 'Narrative did not close using its visible Continue button');
      await capture(`narrative-${latest.objective}`);
      await clickControl('continue');
      await sleep(150);
    }
  }
  async function closeDetails() {
    await dismissNarrative();
    if (latest.controls?.close?.enabled) {
      await clickControl('close');
      await waitState(state => !state.selected, 'Close selected destination');
    }
  }
  async function toggleWorld(open) {
    await dismissNarrative();
    if (latest.inWorld === open) return;
    await clickControl('world');
    try {
      await waitState(state => state.inWorld === open, open ? 'Open world' : 'Return to city');
    } catch (failure) {
      // Diagnostic only: a failed touch stays a failed acceptance gate.
      // Differentiate touch handling from a completely blocked visible button.
      if (!open && orientation(latest) === 'portrait' && latest?.controls?.world?.enabled) {
        const before=summary(latest), point=await screenPoint(latest.controls.world);
        await page.mouse.click(point.x, point.y, {delay:140});
        const afterClick=sequence;
        const mouseSucceeded=await waitState(state => !state.inWorld,
          'Diagnostic mouse click of same visible City control', 4000, afterClick)
          .then(() => true).catch(() => false);
        report.warnings.push('City return diagnostic: first genuine touch failed; real mouse click also tested; succeeded='+mouseSucceeded);
        report.diagnostics.push({kind:'city-return-control-path',before,mouseSucceeded,after:summary(latest),point});
      }
      throw failure;
    }
    await sleep(350);
  }
  async function drag(dx, dy, label) {
    await closeDetails();
    const box = await canvasBox();
    // Genuine run 39168956357 showed that the portrait 'up' drag began on
    // the actual 'Entrar en Valoria' world label after the previous down drag.
    // That label correctly captures pointer-down, so moving away afterwards
    // cannot pan. Choose an exposed ground origin using projected hot spots;
    // do not disable UI raycasts or edit the world camera/gameplay rules.
    const view = orientation(latest), spots = Object.values(hotspotMap(latest))
      .filter(h => h.visible !== false && Number.isFinite(h.x) && Number.isFinite(h.y));
    // Exact browser video from run 39223329742 revealed a real UI hit:
    // at 960x540 the previous (.16,.58) candidate is (154,227), directly
    // INSIDE the left chapter panel (x=17..205). Hotspot clearance alone
    // cannot detect UI and so the up swipe was swallowed by its GraphicRaycaster.
    // Pick only exposed world ground without disabling the legitimate HUD.
    const candidates = view === 'portrait'
      ? [[.83,.56],[.15,.56],[.83,.69],[.17,.70],[.75,.49]]
      : [[.58,.56],[.74,.55],[.54,.66],[.85,.63],[.31,.59]];
    const points = candidates.map(([nx,ny]) => {
      const screen = {x:nx*latest.width,y:(1-ny)*latest.height};
      const clearance = Math.min(...spots.map(h => Math.hypot(h.x-screen.x,h.y-screen.y)),Math.max(latest.width,latest.height));
      const inside = nx+dx>.07 && nx+dx<.93 && ny+dy>.33 && ny+dy<.88;
      // Mission occupies the left chapter lane in landscape. Its projected
      // bounding region is outside this lane; retain the original raycasters.
      const notHud = view !== 'landscape' || (nx > .26 && nx < .90);
      return {nx,ny,clearance,inside:inside&&notHud};
    }).filter(p=>p.inside).sort((a,b)=>b.clearance-a.clearance);
    need(points.length>0, 'No on-screen ground location for the requested real drag');
    const chosen = points[0];
    need(chosen.clearance > Math.min(110,latest.width*.2),
      'All drag start points overlap projected interactive destinations');
    const start = { x: box.x + box.width * chosen.nx, y: box.y + box.height * chosen.ny };
    const end = { x: start.x + dx * box.width, y: start.y + dy * box.height };
    const before = latest;
    const action = { elapsed_s: elapsed(), kind: 'mouse-drag', label, start, end,
      interactive_hotspot_clearance_px: round(chosen.clearance), origin: 'open-ground',
      before: summary(before) };
    report.actions.push(action); ndjson('actions.ndjson', action);
    await page.mouse.move(start.x, start.y); await page.mouse.down(); await sleep(100);
    // Real elapsed gesture time lets Unity observe held/moved/released frames,
    // including on a slower software renderer. No game timer is altered.
    for (let step = 1; step <= 12; step++) {
      await page.mouse.move(start.x + (end.x - start.x) * step / 12, start.y + (end.y - start.y) * step / 12);
      await sleep(35);
    }
    await sleep(100); await page.mouse.up();
    const deliveredSequence = sequence;
    await waitState(state => state.inWorld, 'World observation after drag', 8000, deliveredSequence);
    await sleep(650);
    return { before, after: latest, camera: cameraDifference(before, latest), projected: projectedMovement(before, latest) };
  }
  async function navigationCheck() {
    await toggleWorld(true);
    for (const view of ['landscape', 'portrait']) {
      await setOrientation(view); await closeDetails(); await capture('world-navigation-start');
      for (const [label, dx, dy] of [['right', 0.12, 0], ['left', -0.12, 0], ['down', 0, 0.12], ['up', 0, -0.12]]) {
        const attempts = [];
        let movement = await drag(dx, dy, label); attempts.push(movement);
        const moved = item => (item.camera?.distance > 0.05) ||
          (item.projected?.common >= 2 && Math.hypot(item.projected.dx_per_mille, item.projected.dy_per_mille) > 1.5);
        if (!moved(movement)) {
          // A legitimate map boundary can prevent the first gesture. Move inward
          // with visible input once, then repeat; never move the camera through JS.
          attempts.push(await drag(-dx * 1.8, -dy * 1.8, `${label}-boundary-retreat`));
          movement = await drag(dx, dy, `${label}-retry`); attempts.push(movement);
        }
        const projection = movement.projected;
        const followsGesture = projection?.common >= 2 ? (dx !== 0 ?
          projection.dx_per_mille * Math.sign(dx) > 1 :
          projection.dy_per_mille * -Math.sign(dy) > 1) : null;
        const entry = { view, direction: label, pass: moved(movement) && followsGesture !== false,
          projected_landmarks_follow_gesture: followsGesture,
          attempts: attempts.map(item => ({ camera: item.camera, projected: item.projected,
            before: summary(item.before), after: summary(item.after) })) };
        report.navigation.push(entry); await capture(`world-pan-${label}`);
        need(entry.pass, `World pan ${label} did not move the camera/landmarks in the requested direction in ${view}`);
      }
    }
    await closeDetails();
    const beforeZoom = latest, box = await canvasBox(), seq = sequence;
    // Use the same exposed ground lane that allowed real portrait panning.
    // The world city label is a genuine UI control and must keep intercepting
    // wheel/pointer input when hovered. Observe an actual zoom-in wheel tick,
    // never set camera values through browser automation.
    await page.mouse.move(box.x + box.width * 0.83, box.y + box.height * 0.56);
    report.actions.push({ elapsed_s: elapsed(), kind: 'mouse-wheel', label: 'world-zoom-in',
      deltaY: -240, origin: 'exposed-world-ground', before: summary(beforeZoom) });
    await page.mouse.wheel(0, -240);
    await waitState(() => true, 'Zoom-in observation', 8000, seq);
    await sleep(850);
    const zoomDifference = cameraDifference(beforeZoom, latest);
    need(zoomDifference && zoomDifference.zoom < -0.65,
      'A real world mouse-wheel tick did not visibly zoom toward the selected world ground');
    report.checks.world_zoom = { pass: true, input_delta_y: +240,
      initial_at_regional_fitting_limit: beforeZoom.camera.zoom >= 31.9,
      zoom_direction: zoomDifference.zoom < 0 ? 'in' : 'out',
      before: beforeZoom.camera, after: latest.camera };
    const retained = latest;
    await capture('world-before-city-return'); await toggleWorld(false); await capture('city-world-transition');
    await toggleWorld(true); await sleep(900);
    const restored = cameraDifference(retained, latest);
    report.checks.world_view_retained = { pass: !!restored && restored.distance <= 0.2 && Math.abs(restored.zoom) <= 0.05,
      before: retained.camera, after: latest.camera, difference: restored };
    await capture('world-restored');
    need(report.checks.world_view_retained.pass, 'World position/zoom changed after a city/world round trip');
    report.checks.world_four_directions_both_orientations = report.navigation.length === 8 && report.navigation.every(item => item.pass);
  }
  function describeOperation(state) {
    if (state.selected === 'sawmill' && state.sawmill === 0) return { kind: 'sawmill', expected_seconds: 6 };
    if (state.selected === 'barracks' && state.barracks === 0) return { kind: 'barracks', expected_seconds: 8 };
    if (state.selected === 'barracks' && state.objective === 'b2.train-archers') return { kind: 'training', expected_seconds: 7 };
    if (state.selected === 'bastion') return { kind: 'bastion', expected_seconds: 10 };
    if (state.selected === 'march-main') return { kind: 'configuration', expected_seconds: null };
    if (['forest-valoria', 'quarry-valoria'].includes(state.selected)) return { kind: 'gathering', expected_seconds: 9 };
    if (['corrupt-scout', 'engendro-valoria'].includes(state.selected)) return { kind: 'combat', expected_seconds: 4 };
    throw new Error(`Objective selected an unsupported visible action: ${state.objective} / ${state.selected}`);
  }
  async function normalPlaythrough() {
    flowDeadline = performance.now() + flowTimeout;
    let checkedNavigation = false;
    while (true) {
      health(); await dismissNarrative();
      if (activeOperation?.completed) {
        need(activeOperation.normal_duration_observed, `Operation completed faster than its normal cadence: ${JSON.stringify(activeOperation)}`);
        // Completion can replace a plot/construction model while keeping the same
        // selection and landscape panel bounds. Observe the resulting real frame.
        // A finished building may have no enabled next action, and its objective
        // can advance normally while the camera settles.
        if (latest.selected === activeOperation.selected && latest.inWorld === activeOperation.before.inWorld &&
            orientation(latest) === activeOperation.orientation &&
            !latest.narrative && !latest.busy && phase(latest) === 'idle') {
          const framing = await waitForSelectionFraming(true);
          if (framing) activeOperation.completion_framing_check = framing.index;
          else activeOperation.completion_framing_interrupted = true;
        }
        await capture(`completed-${activeOperation.kind}`, activeOperation.completion_framing_check || null); activeOperation = null;
      }
      if (activeOperation || latest.busy || phase(latest) !== 'idle') {
        if (activeOperation) need(elapsed() - activeOperation.started_elapsed_s < 90, `Operation stalled: ${activeOperation.kind}`);
        const key = `${report.marches.length}-${phase(latest)}-${latest.busy}-${latest.selected}`;
        if (!capturedPhases.has(key)) { capturedPhases.add(key); await capture(`operation-${target(latest) || latest.selected}-${phase(latest)}${latest.busy ? '-working' : ''}`); }
        await sleep(100); continue;
      }
      if (latest.objective === 'b2.complete') break;
      if (!checkedNavigation && latest.sawmill > 0) { await navigationCheck(); checkedNavigation = true; continue; }
      const desired = ['b1.build-sawmill', 'b1.gather-stone', 'b1.ascend', 'b2.train-archers', 'b2.defeat-engendro'].includes(latest.objective) ? 'portrait' : 'landscape';
      await setOrientation(desired); await dismissNarrative();
      const previousObjective = latest.objective;
      await clickControl('objective');
      await waitState(state => state.narrative || state.busy || state.selected || state.objective !== previousObjective, 'Objective locates an actionable destination', 10000);
      if (latest.narrative || latest.busy || phase(latest) !== 'idle' || latest.objective !== previousObjective) continue;
      await waitState(state => state.narrative || state.busy || state.controls?.primary?.enabled, 'Visible primary action becomes available', 10000);
      if (latest.narrative || latest.busy || phase(latest) !== 'idle') continue;
      const framing = await waitForSelectionFraming();
      if (!framing) continue;
      const state = latest, key = `${state.revision}:${state.objective}:${state.selected}`;
      need(!actionKeys.has(key), `Refusing duplicate action at unchanged revision: ${key}`);
      need(report.operations.length < 35, 'Objective guidance exceeded 35 actions without completing Bastion II');
      if (!seenObjectives.has(`${state.objective}:${state.selected}`)) {
        seenObjectives.add(`${state.objective}:${state.selected}`); await capture(`selected-${state.objective}-${state.selected}`, framing.index);
        if (latest.narrative) continue;
      }
      if (!stableSelectionView(framing.after, latest) || !framingObservation(latest).pass) {
        framing.invalidated_after_capture = true; continue;
      }
      need(!latest.busy && phase(latest) === 'idle', 'Operation became busy before the next visible action');
      const operation = { index: report.operations.length + 1, ...describeOperation(latest), selected: latest.selected,
        objective: latest.objective, orientation: orientation(latest), started_elapsed_s: elapsed(),
        before: summary(latest), framing_check: framing.index, accepted: false, completed: false };
      activeOperation = operation; report.operations.push(operation); actionKeys.add(key);
      await clickControl('primary');
      await waitState(stateAfter => operation.accepted || stateAfter.narrative, 'Visible action acknowledges a state revision', 10000);
      if (!operation.accepted && latest.narrative) {
        operation.interrupted_by_narrative = true; activeOperation = null; actionKeys.delete(key);
      }
    }
    report.checks.complete_i_ii = latest.bastion === 2 && latest.sawmill > 0 && latest.barracks > 0 &&
      latest.trained >= 20 && latest.configured && latest.scout && latest.engendro && phase(latest) === 'idle' && !latest.busy;
    need(report.checks.complete_i_ii, 'Bastion II completion did not retain all required systems and the returned march');
    report.checks.real_primary_actions_both_orientations = ['landscape', 'portrait'].every(view =>
      report.operations.some(operation => operation.orientation === view && operation.accepted && operation.completed));
    need(report.checks.real_primary_actions_both_orientations, 'Progression did not exercise a real primary action in both orientations');
    report.checks.selection_framing_before_all_actions = report.operations.filter(operation => operation.accepted)
      .every(operation => report.selection_framing[operation.framing_check - 1]?.pass && framingObservation(operation.before).pass);
    report.checks.selection_framing_both_orientations = ['landscape', 'portrait'].every(view =>
      report.operations.some(operation => operation.orientation === view && operation.accepted && operation.completed &&
        report.selection_framing[operation.framing_check - 1]?.pass));
    need(report.checks.selection_framing_before_all_actions && report.checks.selection_framing_both_orientations,
      'A real action lacked stable full-model framing inside the free viewport, or an orientation was not covered');
    report.checks.normal_operation_durations = report.operations.filter(operation => operation.accepted).every(operation => operation.completed && operation.normal_duration_observed);
    report.checks.march_phases_and_rewards = ['forest-valoria', 'quarry-valoria', 'corrupt-scout', 'engendro-valoria'].every(id =>
      report.marches.some(march => march.target === id && march.completed && march.pending_rewards_delivered &&
        (id.includes('forest') || id.includes('quarry') ? ['outbound', 'gathering', 'returning', 'idle'] : ['outbound', 'returning', 'idle'])
          .every(required => march.phases.some(entry => entry.phase === required))));
    need(report.checks.march_phases_and_rewards, 'A march phase or its returned rewards was not observed; inspect telemetry/video rather than inferring animation absence');
    report.checks.march_visual_outbound = report.marches.some(march => march.completed && march.visual_motion?.outbound.moving_pairs > 0);
    report.checks.march_visual_returning = report.marches.some(march => march.completed && march.visual_motion?.returning.moving_pairs > 0);
    need(report.checks.march_visual_outbound && report.checks.march_visual_returning,
      'Visible march displacement was not resolved for at least one outbound and one return with stable camera; see each visual_motion status and video. This is incomplete motion evidence, not a claim that animation is absent');
  }
  async function load(label, reload = false) {
    latest = null; previousFrame = null; lastTelemetryAt = 0;
    const start = performance.now(), requestStart = server.requests.length;
    if (reload) await page.reload({ waitUntil: 'domcontentloaded', timeout: loadTimeout });
    else await page.goto(server.url + '?eldoriaPlaytest=1', { waitUntil: 'domcontentloaded', timeout: loadTimeout });
    const entry = page.locator('#play');
    await entry.waitFor({ state: 'visible', timeout: loadTimeout });
    const playClickedAt = performance.now();
    const entryAction = { elapsed_s: elapsed(), kind: page.viewportSize().height > page.viewportSize().width ? 'dom-tap' : 'dom-click',
      label: 'Entrar en Valoria', selector: '#play', load: label };
    report.actions.push(entryAction); ndjson('actions.ndjson', entryAction);
    if (entryAction.kind === 'dom-tap') await entry.tap(); else await entry.click();
    await waitState(state => state.ready, `${label} playable readiness`, loadTimeout);
    await page.locator('canvas').first().waitFor({ state: 'visible', timeout: loadTimeout });
    const readyAt = performance.now(), duration = readyAt - start;
    // Browser performance entries are read-only observations, never engine access.
    const timing = await page.evaluate(() => ({
      navigation: performance.getEntriesByType('navigation').map(entry => entry.toJSON()),
      resources: performance.getEntriesByType('resource').map(entry => ({
        name: entry.name, initiatorType: entry.initiatorType, duration: entry.duration,
        transferSize: entry.transferSize, encodedBodySize: entry.encodedBodySize, decodedBodySize: entry.decodedBodySize
      })), userAgent: navigator.userAgent
    }));
    const requests = server.requests.slice(requestStart);
    report.loads.push({ label, fresh_context: !reload, ready_seconds: round(duration / 1000),
      seconds_from_play_click_to_visible_ready: round((readyAt - playClickedAt) / 1000),
      server_response_body_bytes: requests.reduce((sum, request) => sum + request.bytes, 0),
      server_requests: requests.length, timing, initial_state: summary(latest) });
    need(latest.clock === 'SystemClock' && latest.timeScale === 1, 'The loaded artifact is not running the normal system clock');
  }
  async function stableSave() {
    const start = performance.now(); let candidate = latest.save, stableSince = start;
    need(typeof candidate === 'string' && candidate.length > 20, 'Read-only exact save projection is missing');
    JSON.parse(candidate);
    while (performance.now() - stableSince < 2200) {
      health(); need(performance.now() - start < 12000, 'Final save never became stable');
      if (latest.save !== candidate) { candidate = latest.save; stableSince = performance.now(); }
      await sleep(100);
    }
    return candidate;
  }

  try {
    server = await serveBuild(buildRoot);
    const manifest = await fingerprint(server.root);
    fs.writeFileSync(path.join(out, 'build_manifest.json'), JSON.stringify(manifest, null, 2));
    report.build = { bytes: manifest.bytes, files: manifest.files.length, manifest_sha256: manifest.manifest_sha256 };
    const { chromium } = require('playwright');
    report.playwright_version = require('playwright/package.json').version;
    // The single existing Windows PC can expose native D3D11 graphics.
    // Linux hosted Chromium/SwiftShader rendered fewer than 5 telemetry
    // frames in 35 seconds, without runtime errors. Never force software
    // ANGLE when exercising the real Windows Chromium connection.
    const launchArgs = process.platform === 'win32'
      ? ['--no-sandbox', '--use-angle=d3d11', '--ignore-gpu-blocklist', '--enable-webgl']
      : ['--no-sandbox', '--disable-dev-shm-usage', '--use-angle=swiftshader', '--enable-unsafe-swiftshader'];
    browser = await chromium.launch({ headless: true,
      executablePath: process.env.VALORIA_CHROMIUM_EXECUTABLE || undefined,
      args: launchArgs });
    report.chromium_launch = { platform:process.platform, args:launchArgs,
      existing_executable:process.env.VALORIA_CHROMIUM_EXECUTABLE||null };
    report.browser_version = browser.version();
    const options = { viewport: VIEWS.landscape, deviceScaleFactor: 1, hasTouch: true };
    need(process.env.VALORIA_PLAYTEST_VIDEO !== '0', 'Video capture is required to validate authentic frame evidence');
    options.recordVideo = { dir: path.join(out, 'video'), size: { width: 960, height: 720 } };
    context = await browser.newContext(options); page = await context.newPage(); videoStartedAt = performance.now();
    page.on('framenavigated', frame => {
      // An old Unity frame can log while reload is starting. It must never
      // satisfy the next document's readiness or exact-save verification.
      if (frame === page.mainFrame()) {
        latest = null; previousFrame = null; lastTelemetryAt = 0; documentCommittedAt = Date.now();
      }
    });
    page.on('console', message => {
      const text = message.text(), where = text.indexOf(PREFIX);
      if (where >= 0) {
        try { recordFrame(JSON.parse(text.slice(where + PREFIX.length).split('\n')[0])); }
        catch (error) { telemetryBroken = true; report.failures.push('Invalid read-only telemetry: ' + error.message); }
        return;
      }
      ndjson('browser-console.ndjson', { elapsed_s: elapsed(), type: message.type(), text, location: message.location() });
      const favicon = /favicon\.ico/.test(message.location()?.url || '');
      if ((message.type() === 'error' && !favicon) || /NullReferenceException|IndexOutOfRangeException|InvalidOperationException|OutOfMemoryException|RuntimeError:|VALORIA_INTEGRATION_MISSING_BINDING|VALORIA_SAVE_PRESERVED/.test(text))
        report.runtime_errors.push({ elapsed_s: elapsed(), type: message.type(), text });
    });
    page.on('pageerror', error => report.runtime_errors.push({ elapsed_s: elapsed(), type: 'pageerror', text: error.stack || error.message }));
    page.on('crash', () => { crashed = true; report.runtime_errors.push({ elapsed_s: elapsed(), type: 'crash' }); });
    page.on('requestfailed', request => {
      const failure = { elapsed_s: elapsed(), url: request.url(), error: request.failure()?.errorText };
      report.network_failures.push(failure);
    });
    page.on('response', response => {
      if (response.status() >= 400 && !/favicon\.ico(?:\?|$)/.test(response.url()))
        report.network_failures.push({ elapsed_s: elapsed(), url: response.url(), status: response.status() });
    });
    await load('cold-empty-browser-context'); report.orientations.push('landscape');
    report.initial_state = summary(latest);
    report.checks.fresh_save = latest.objective === 'b1.build-sawmill' && latest.sawmill === 0 && latest.barracks === 0 && latest.bastion === 1 && latest.trained === 0 && !latest.engendro;
    need(report.checks.fresh_save, 'Fresh browser context did not start a new Bastion I game');
    await capture('arrival'); await setOrientation('portrait'); await capture('arrival'); await dismissNarrative();
    report.checks.early_world_control_locked = latest.controls?.world?.enabled === false;
    need(report.checks.early_world_control_locked, 'World control is enabled before the first sawmill with the normal initial wallet');
    const gate = hotspotMap(latest).gate;
    if (gate?.visible) {
      await capture('visible-gate-before-unlock'); await clickPoint(gate, 'visible-gate-before-unlock');
      await sleep(500);
      report.checks.early_gate_visible_click = { attempted: true, remained_in_city: !latest.inWorld,
        limitation: 'Visible projected collider centre clicked; screenshot records any HUD overlap' };
      need(!latest.inWorld, 'The visible city gate bypassed the world access rule');
    } else report.checks.early_gate_visible_click = { attempted: false, reason: 'Gate collider centre outside current visible frame; native gate regression remains separate' };
    await normalPlaythrough();
    await dismissNarrative(); await toggleWorld(false); await closeDetails();
    for (const view of ['landscape', 'portrait']) { await setOrientation(view); await capture('bastion-ii-complete-city'); }
    const saved = await stableSave(); fs.writeFileSync(path.join(out, 'final-save.json'), saved);
    report.final_state = summary(latest); report.final_save_sha256 = sha256(saved);
    flowDeadline = Infinity;
    await load('warm-reload-same-browser-context', true);
    await waitState(state => state.objective === 'b2.complete' && phase(state) === 'idle' && !state.busy, 'Reload restores completed Bastion II', 15000);
    const restored = await stableSave(); fs.writeFileSync(path.join(out, 'reloaded-save.json'), restored);
    report.checks.exact_save_reload = { pass: saved === restored, before_sha256: sha256(saved), after_sha256: sha256(restored) };
    need(report.checks.exact_save_reload.pass, 'Reload changed the exact completed saved state');
    await capture('warm-reload-complete');
    report.checks.normal_system_clock = true;
    report.checks.no_runtime_errors = report.runtime_errors.length === 0;
    report.checks.no_failed_build_requests = report.network_failures.filter(item => item.error !== 'net::ERR_ABORTED').length === 0;
    need(report.checks.no_runtime_errors, 'Browser or Unity runtime errors were observed; inspect browser-console.ndjson');
    need(report.checks.no_failed_build_requests, 'A build request failed; inspect report.network_failures');
    report.status = 'PASS';
  } catch (error) {
    report.status = 'FAIL'; report.failures.push(error.stack || error.message); report.last_state = summary(latest);
    if (page && !page.isClosed() && !crashed) {
      try { await capture('failure'); } catch (captureError) { report.warnings.push('Failure screenshot unavailable: ' + captureError.message); }
    }
    process.exitCode = 1;
  } finally {
    const video = page?.video();
    if (context) await context.close().catch(error => report.warnings.push('Context close: ' + error.message));
    if (video) {
      try {
        const recorded=await video.path();
        need(fs.existsSync(recorded)&&fs.statSync(recorded).size>4096,'Chromium video has no usable bytes');
        report.video=path.relative(out,recorded).split(path.sep).join('/');
        const frameFailures=[];
        for(const shot of report.screenshots){
          const destination=path.join(out,shot.path);
          const args=['-y','-hide_banner','-loglevel','error','-ss',String(Math.max(0,shot.video_second-.08)),
            '-i',recorded,'-frames:v','1','-q:v','3','-update','1',destination];
          const p=spawnSync('ffmpeg',args,{timeout:25000,encoding:'utf8'});
          let bytes=fs.existsSync(destination)?fs.readFileSync(destination):null;
          let realFrame=p.status===0&&bytes&&bytes.length>2048&&bytes[0]===0xff&&bytes[1]===0xd8;
          if(!realFrame){
            // Playwright video can stop adding encoded frames during an unchanged
            // still scene, even while SystemClock telemetry remains alive. Retrieve
            // the LAST genuinely recorded frame without pretending it depicts a
            // later gesture. Preserve this explicit qualification in the report.
            const tail=spawnSync('ffmpeg',['-y','-hide_banner','-loglevel','error',
              '-sseof','-0.35','-i',recorded,'-frames:v','1','-q:v','3',
              '-update','1',destination],{timeout:25000,encoding:'utf8'});
            bytes=fs.existsSync(destination)?fs.readFileSync(destination):null;
            realFrame=tail.status===0&&bytes&&bytes.length>2048&&bytes[0]===0xff&&bytes[1]===0xd8;
            if(realFrame){
              shot.last_recorded_frame_fallback=true;
              shot.event_frame_verified=false;
              report.warnings.push('Exact-time frame unavailable for '+shot.label+
                '; saved final genuinely recorded frame, NOT proof of that later event.');
            }
          }
          if(!realFrame)
            frameFailures.push({file:shot.path,reason:p.error?.message||p.stderr?.slice(-400)||'no genuine JPEG frame'});
          else{shot.extraction_pending=false;shot.bytes=bytes.length;
            shot.extracted_after_video_finalization=true;
            if(shot.event_frame_verified!==false)shot.event_frame_verified=true;}
        }
        const first=report.screenshots.find(shot=>shot.label.startsWith('narrative-'));
        if(first){
          const clip='video/eldoria-original-dialogue-short.webm';
          const args=['-y','-hide_banner','-loglevel','error','-ss',
            String(Math.max(0,first.video_second-1.5)),'-i',recorded,'-t','28',
            '-an','-c:v','copy',path.join(out,clip)];
          const p=spawnSync('ffmpeg',args,{timeout:30000,encoding:'utf8'});
          if(p.status===0&&fs.existsSync(path.join(out,clip))&&fs.statSync(path.join(out,clip)).size>4096)
            report.dialogue_clip=clip;
          else report.warnings.push('Short original dialogue video unavailable: '+(p.error?.message||p.stderr?.slice(-400)));
        }
        if(frameFailures.length){report.failures.push('Genuine Chromium video frames missing: '+JSON.stringify(frameFailures.slice(0,10)));
          report.status='FAIL';process.exitCode=1;}
        report.visual_source='Authentic Chromium WebM and time-matched FFmpeg still frames';
      }catch(error){
        report.failures.push('Required real browser video/screenshots unavailable: '+(error.stack||error.message));
        report.status='FAIL';process.exitCode=1;
      }
    } else if(report.screenshots.length){
      report.failures.push('Required real Chromium video unavailable');report.status='FAIL';process.exitCode=1;
    }
    if (browser) await browser.close().catch(error => report.warnings.push('Browser close: ' + error.message));
    if (server) {
      fs.writeFileSync(path.join(out, 'server-requests.json'), JSON.stringify(server.requests, null, 2));
      await server.close();
    }
    report.ended_at = new Date().toISOString(); report.total_seconds = elapsed();
    fs.writeFileSync(path.join(out, 'report.json'), JSON.stringify(report, null, 2));
    console.log(`VALORIA_WEB_PLAYTEST ${JSON.stringify({ status: report.status, out, checks: report.checks, failures: report.failures, runtime_errors: report.runtime_errors.slice(-8), network_failures: report.network_failures.slice(-8), last_action: report.actions.at(-1), video: report.video, video_frames: report.screenshots.length, loads: report.loads.map(item => ({ label: item.label, ready_seconds: item.ready_seconds })) })}`);
  }
}

module.exports = { serveBuild, mimeType, phase, target, hotspotMap, projectedMovement, cameraDifference, analyseMarchVisual, framingObservation, stableSelectionView };
if (require.main === module) main().catch(error => { console.error(error.stack || error.message); process.exitCode = 1; });
