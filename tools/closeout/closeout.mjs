#!/usr/bin/env node
/**
 * Eldoria closeout diagnostics (read-only, stdlib-only).
 * This tool NEVER dispatches CI, changes source/leases, approves art, or publishes.
 * CLI:
 *   node tools/closeout/closeout.mjs triage --log job.log [--report report.json] [--change qa_only|product|unknown]
 *   node tools/closeout/closeout.mjs preflight-browser --artifact build.zip --sha256 <64hex>
 *        --source-sha <40hex> --validated-source-sha <40hex> [--script tools/.../playtest_web.cjs]
 */
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';
import { createRequire } from 'node:module';

const require = createRequire(import.meta.url);
const sha40 = /^[a-f0-9]{40}$/i;
const sha64 = /^[a-f0-9]{64}$/i;
const same = (a, b) => typeof a === 'string' && typeof b === 'string' && a.toLowerCase() === b.toLowerCase();
const ERROR_CATEGORIES = [
  { id: 'SOURCE_INTEGRITY', pattern: /(?:input (?:manifest|fingerprint|signature)|source fingerprint|artifact|saved (?:bytes|state|hash))[^\n\r]{0,130}(?:mismatch|differs|invalid|changed)|(?:mismatch|differs)[^\n\r]{0,80}(?:sha-?256|source|input manifest)/i,
    action: 'Bloquear promoción. Comparar SHA y manifiestos de fuentes/artefacto; no desactivar la comprobación.' },
  { id: 'ENVIRONMENT_FFMPEG', pattern: /(?:spawnSync ffmpeg ENOENT|ffmpeg (?:not found|unavailable|ENOENT)|Genuine Chromium video frames missing)/i,
    action: 'Reparar FFmpeg en el entorno antes de repetir Chromium; conservar el artefacto exacto.' },
  { id: 'ENVIRONMENT_UNITY_EDITOR', pattern: /(?:IndexationOnStartup|SearchDatabase|Search\.index[^\n\r]{0,90}(?:exception|corrupt|failed)|UI certification path[^\n\r]{0,60}incorrect)/i,
    action: 'Corregir el entorno del Editor en su procedimiento de recuperación acotado; preservar el juego.' },
  { id: 'ENVIRONMENT_BROWSER', pattern: /(?:Playwright package resolution failed|browser executable[^\n\r]{0,80}(?:not found|missing)|SwiftShader[^\n\r]{0,80}(?:slow|several seconds)|ffmpeg-win64\.exe[^\n\r]{0,60}ENOENT)/i,
    action: 'Comprobar navegador GPU, Playwright y rendimiento del entorno antes del recorrido completo.' },
  { id: 'PLATFORM_COMPILE', pattern: /(?:error CS\d+|AndroidJNIModule|AndroidJavaObject|AndroidJavaClass|Build Failed, Result: Failure)/i,
    action: 'Corregir el error de compilación específico de la plataforma; este cambio sí requiere recompilar.' },
  { id: 'TEST_INPUT_OR_TIMING', pattern: /(?:Graphic has depth=-1|same-call-stack uGUI|(?:gesture|drag)[^\n\r]{0,100}(?:HUD|intercepted|hotspot)|pointer[^\n\r]{0,80}HUD interception)/i,
    action: 'Revisar el tiempo de Canvas y la ubicación visible del gesto; repetir el control real, nunca Button.onClick.Invoke.' },
  { id: 'INTERACTION_UNRESOLVED', pattern: /(?:Return to city: timed out|World pan (?:up|down|left|right)[^\n\r]{0,120}did not move|(?:touch|tap|click)[^\n\r]{0,90}(?:not registered|missed|timed out))/i,
    action: 'Fallo de interacción observado, causa no determinada. Comparar vídeo, raycasts, evento táctil y clic real sobre el MISMO WebGL; no atribuirlo aún al juego ni a la prueba.' },
  { id: 'TIMEOUT', pattern: /(?:timed out after \d+s|preflight exceeded|own .* timeout)/i,
    action: 'Revisar etapa, reloj y registros; no incrementar el timeout como sustituto del diagnóstico.' },
];

function parseReport(logText, explicit) {
  if (explicit && typeof explicit === 'object') return explicit;
  const marker = 'VALORIA_WEB_PLAYTEST ';
  const lines = String(logText || '').split(/\r?\n/);
  const last = lines.filter(line => line.includes(marker)).at(-1);
  if (!last) return null;
  try { return JSON.parse(last.slice(last.indexOf(marker) + marker.length)); }
  catch { return {status: 'INVALID_REPORT', failures: ['Reported JSON is malformed']}; }
}

export function triage({ logText = '', report = null, change = 'unknown' } = {}) {
  if (!['qa_only', 'product', 'unknown'].includes(change)) throw new Error('change must be qa_only, product, or unknown');
  const parsed = parseReport(logText, report);
  const failureTexts = Array.isArray(parsed?.failures) ? parsed.failures.map(String) : [];
  const observations = [logText, ...failureTexts].filter(Boolean).join('\n');
  const issues = ERROR_CATEGORIES.filter(x => x.pattern.test(observations))
    .map(x => ({category: x.id, next_action: x.action}));
  const failed = parsed?.status === 'FAIL' || parsed?.status === 'INVALID_REPORT' ||
    (Array.isArray(parsed?.runtime_errors) && parsed.runtime_errors.length > 0) ||
    (Array.isArray(parsed?.network_failures) && parsed.network_failures.length > 0) ||
    /(?:##\[error\]|Process completed with exit code [1-9]|Build Failed, Result: Failure|\bERROR\b)/i.test(logText);
  if (Array.isArray(parsed?.runtime_errors) && parsed.runtime_errors.length && !issues.some(x => x.category === 'INTERACTION_UNRESOLVED')) {
    issues.push({category:'RUNTIME_ERRORS', next_action:'Reproducir los errores de runtime en el artefacto exacto; investigar el código antes de publicar.'});
  }
  if (Array.isArray(parsed?.network_failures) && parsed.network_failures.length) {
    issues.push({category:'NETWORK_FAILURES', next_action:'Separar fallo de descarga/carga del comportamiento del juego y registrar respuestas HTTP.'});
  }
  if (failed && !issues.length) issues.push({category:'UNCLASSIFIED_FAILURE', next_action:'Conservar log, vídeo y manifest; localizar primera causa fallida antes de cambiar código o repetir compilación.'});
  return {
    tool: 'eldoria-closeout-triage-v1',
    verdict: (failed || issues.length > 0) ? 'BLOCKED_NOT_CLOSED' : 'NO_FAILURE_ESTABLISHED_NOT_CERTIFIED',
    report_status: parsed?.status || 'not_provided',
    issues,
    source_change_declared: change,
    // A changed QA script cannot change signed game bytes. But a changed runtime/asset requires a new build.
    next_build: change === 'product' ? 'REQUIRED_AFTER_GAME_CHANGE' : change === 'qa_only'
      ? 'REUSE_EXACT_ARTIFACT_IF_SHA_AND_PROVENANCE_MATCH' : 'CHECK_SOURCE_DIFF_BEFORE_DECIDING',
    next_validation: failed ? 'TARGETED_REPRO_FIRST_THEN_FULL_GATES' : 'REVIEW_REQUIRED_GATES_AND_EVIDENCE',
    release_approved: false,
    note: 'No automatic classification certifies a root cause, pass, artistic improvement, physical mobile QA or release.'
  };
}

function findExecutable(name, env = process.env) {
  const cmd = process.platform === 'win32' ? 'where.exe' : 'which';
  const result = spawnSync(cmd, [name], {encoding:'utf8', timeout:1500, windowsHide:true, env});
  if (result.status !== 0) return null;
  const first = result.stdout?.trim().split(/\r?\n/)[0];
  return first && fs.existsSync(first) ? first : null;
}

function resolveBrowser(env, exists) {
  const given = env.VALORIA_CHROMIUM_EXECUTABLE;
  if (given) return exists(given) ? given : null;
  if (process.platform === 'win32') {
    for (const base of [env['PROGRAMFILES(X86)'],env.ProgramFiles,env.LOCALAPPDATA].filter(Boolean)) {
      for (const suffix of ['Microsoft/Edge/Application/msedge.exe','Google/Chrome/Application/chrome.exe']) {
        const p = path.join(base, suffix);
        if (exists(p)) return p;
      }
    }
  }
  return findExecutable('chromium', env) || findExecutable('google-chrome', env) || findExecutable('msedge', env);
}

function locatePlaywright(env) {
  const bases = [process.cwd(), ...(env.NODE_PATH || '').split(path.delimiter).filter(Boolean)];
  for (const base of bases) {
    try { const resolved = require.resolve('playwright/package.json', {paths: [base]}); if (fs.existsSync(resolved)) return resolved; }
    catch {}
    const direct = path.join(base,'playwright','package.json');
    if (fs.existsSync(direct)) return direct;
  }
  return null;
}

function hashFile(file) {
  const hash = crypto.createHash('sha256');
  const fd = fs.openSync(file,'r');
  const buffer = Buffer.allocUnsafe(1024 * 1024);
  try { for (;;) { const count = fs.readSync(fd,buffer,0,buffer.length,null); if (!count) break; hash.update(buffer.subarray(0,count)); } }
  finally { fs.closeSync(fd); }
  return hash.digest('hex');
}

export function browserPreflight({ artifact, artifactSha256, sourceSha, validatedSourceSha,
    scriptPath = null, environment = process.env, fileExists = fs.existsSync,
    findBrowser = null, findFfmpeg = null, findPlaywright = null, computeHash = hashFile,
    checkScript = null } = {}) {
  const checks = [];
  const record = (name, pass, detail) => checks.push({name, status:pass?'READY':'BLOCKED', detail});
  record('source_provenance', sha40.test(sourceSha || '') && sha40.test(validatedSourceSha || '') && same(sourceSha, validatedSourceSha),
    sha40.test(sourceSha || '') && sha40.test(validatedSourceSha || '') ? (same(sourceSha,validatedSourceSha)?'Exact SHA matches':'Source SHA differs') : 'Both full Git source SHAs are required');
  record('artifact_expected_sha', sha64.test(artifactSha256 || ''), sha64.test(artifactSha256 || '') ? 'Pinned digest supplied' : 'Expected SHA-256 required');
  const realFile = Boolean(artifact && fileExists(artifact) && fs.statSync(artifact).isFile());
  record('artifact_file', realFile, realFile ? 'Existing immutable input file' : 'Artifact file not supplied/found');
  if (realFile && sha64.test(artifactSha256 || '')) {
    try { const observed = computeHash(artifact); record('artifact_bytes', same(observed,artifactSha256), same(observed,artifactSha256)?'Bytes match pinned SHA-256':'Artifact digest mismatch'); }
    catch (e) { record('artifact_bytes', false, 'Cannot calculate SHA-256: '+e.code); }
  }
  const playwright = findPlaywright ? findPlaywright(environment) : locatePlaywright(environment);
  record('playwright_package', Boolean(playwright), playwright ? 'Local dependency discovered' : 'Playwright unavailable on this host');
  const browser = findBrowser ? findBrowser(environment) : resolveBrowser(environment,fileExists);
  record('chromium_or_edge', Boolean(browser), browser ? 'Browser discovered on this host' : 'Browser unavailable on this host');
  // The current Chromium replay actually spawns ffmpeg by name: a file path
  // in an unrelated variable would NOT make that command available.
  const ffmpeg = findFfmpeg ? findFfmpeg(environment) : findExecutable('ffmpeg',environment);
  record('ffmpeg', Boolean(ffmpeg), ffmpeg ? 'Video frame extractor discovered on this host' : 'FFmpeg unavailable: video evidence cannot be checked');
  if (scriptPath) {
    let valid = false;
    if (fileExists(scriptPath)) {
      const checker = checkScript || (p => spawnSync(process.execPath,['--check',p],{encoding:'utf8',timeout:10000}).status===0);
      valid = checker(scriptPath);
    }
    record('qa_script_syntax',valid,valid?'QA script syntax valid':'Missing/invalid QA script');
  }
  const blocked = checks.filter(c=>c.status==='BLOCKED');
  return {
    tool: 'eldoria-closeout-preflight-v1',
    verdict: blocked.length ? 'BLOCKED_BEFORE_EXPENSIVE_QA' : 'READY_FOR_TARGETED_QA_NOT_CERTIFIED',
    checks,
    blocked:blocked.map(c=>c.name),
    release_approved:false,
    actions_performed:'read_only_no_runner_claim_no_dispatch_no_publication',
    note:'Checks readiness on THIS host only; does not certify Windows runner availability, game controls, visuals, saves, APK identity or a playable build.'
  };
}

function cliArgs(argv) {
  const result = {};
  for (let i=0;i<argv.length;i++) {
    const key=argv[i];
    if (!/^--[a-z0-9-]+$/.test(key) || i+1>=argv.length || /^--/.test(argv[i+1])) throw new Error('Each option needs a value: '+key);
    if (Object.hasOwn(result,key)) throw new Error('Repeated option: '+key);
    result[key]=argv[++i];
  }
  return result;
}

export function main(argv=process.argv.slice(2)) {
  try {
    const command=argv[0];
    if (!['triage','preflight-browser'].includes(command)) throw new Error('Usage: closeout.mjs triage|preflight-browser [--options]');
    const a=cliArgs(argv.slice(1));
    let report;
    if (command==='triage') {
      if (!a['--log'] && !a['--report']) throw new Error('triage requires --log or --report');
      const logText=a['--log']?fs.readFileSync(a['--log'],'utf8'):'';
      const json=a['--report']?JSON.parse(fs.readFileSync(a['--report'],'utf8')):null;
      report=triage({logText,report:json,change:a['--change']||'unknown'});
    } else {
      report=browserPreflight({artifact:a['--artifact'],artifactSha256:a['--sha256'],
        sourceSha:a['--source-sha'],validatedSourceSha:a['--validated-source-sha'],scriptPath:a['--script']});
    }
    process.stdout.write(JSON.stringify(report,null,2)+'\n');
    return report.verdict.startsWith('BLOCKED') ? 2 : 0;
  } catch(e) {
    process.stderr.write('ELDORIA_CLOSEOUT_ERROR '+e.message+'\n');
    return 2;
  }
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) process.exitCode=main();
