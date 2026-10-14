#!/usr/bin/env node
/**
 * Read-only capability discovery on whichever machine executes this command.
 * Never mistakes a workflow declaration for a live Windows connection.
 * No installations, process starts, runner leases, writes, or paid API calls.
 */
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { execFileSync, spawnSync } from 'node:child_process';
import http from 'node:http';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const defaultRoot = path.resolve(__dirname, '..');
const args = process.argv.slice(2);
const valueAfter = (flag) => {
  const i = args.indexOf(flag);
  return i >= 0 ? args[i + 1] : undefined;
};
const root = path.resolve(valueAfter('--root') || defaultRoot);
const probe = !args.includes('--no-probe');

function readFile(relative) {
  try { return fs.readFileSync(path.join(root, relative), 'utf8'); }
  catch { return null; }
}
function localCommand(command, argv = []) {
  try { return execFileSync(command, argv, {cwd: root, encoding: 'utf8', timeout: 1500, stdio: ['ignore', 'pipe', 'ignore']}).trim(); }
  catch { return null; }
}
function detectedBinary(envNames, commands) {
  if (!probe) return {status: 'not_tested', path: null};
  for (const name of envNames) {
    const candidate = process.env[name];
    if (candidate && fs.existsSync(candidate) && fs.statSync(candidate).isFile()) {
      return {status: 'present_on_this_host', path: candidate, source: name};
    }
  }
  const finder = process.platform === 'win32' ? 'where.exe' : 'which';
  for (const command of commands) {
    const found = spawnSync(finder, [command], {encoding: 'utf8', timeout: 1500, windowsHide: true});
    if (found.status === 0 && found.stdout?.trim()) {
      return {status: 'present_on_this_host', path: found.stdout.trim().split(/\r?\n/)[0], source: 'PATH'};
    }
  }
  return {status: 'not_found_on_this_host', path: null};
}
function ollamaLocalVersion() {
  return new Promise(resolve => {
    if (!probe) return resolve({status: 'not_tested'});
    const req = http.get({hostname: '127.0.0.1', port: 11434, path: '/api/version', timeout: 1000}, res => {
      let output = '';
      res.on('data', part => { if (output.length < 1024) output += part.toString(); });
      res.on('end', () => {
        try {
          const data = JSON.parse(output);
          resolve(res.statusCode === 200 && typeof data.version === 'string'
            ? {status: 'listening_on_this_host', version: data.version}
            : {status: 'not_listening_on_this_host'});
        } catch { resolve({status: 'not_listening_on_this_host'}); }
      });
    });
    req.on('timeout', () => req.destroy());
    req.on('error', () => resolve({status: 'not_listening_on_this_host'}));
  });
}
function declaredRoutes() {
  const dir = path.join(root, '.github', 'workflows');
  let workflows = [];
  try { workflows = fs.readdirSync(dir).filter(name => /\.ya?ml$/i.test(name)); } catch {}
  const specs = workflows.map(name => ({name, content: readFile('.github/workflows/' + name) || ''}));
  const windowsUnity = specs.filter(x => /self-hosted/i.test(x.content) && /windows/i.test(x.content) && /unity/i.test(x.content));
  const artist = specs.filter(x => /qwen|ollama|blender|artist/i.test(x.name) || (/ollama/i.test(x.content) && /blender/i.test(x.content)));
  return {
    windows_unity: {status: windowsUnity.length ? 'declared_not_verified_live' : 'not_found', workflows: windowsUnity.map(x => x.name).slice(0, 12)},
    ai_artist_blender: {status: artist.length ? 'declared_not_verified_live' : 'not_found', workflows: artist.map(x => x.name).slice(0, 20)}
  };
}
function productionPause() {
  try {
    const state = JSON.parse(readFile('pipeline/active-workstreams.json'));
    return state.owner_production_pause?.active === true ? 'active' :
      state.owner_production_pause?.active === false ? 'inactive' : 'unknown';
  } catch { return 'unknown'; }
}
const report = {
  schema_version: 1,
  purpose: 'Eldoria agent start: capability discovery without execution',
  inspected_at: new Date().toISOString(),
  repository: 'mt5hjfz2kb-lab/Eldoria-Prewiu',
  checkout: {path: root, agents_md_present: Boolean(readFile('AGENTS.md')),
    head: probe ? localCommand('git', ['rev-parse', 'HEAD']) : null},
  machine: {platform: process.platform, hostname: os.hostname(),
    github_actions: process.env.GITHUB_ACTIONS === 'true',
    runner_os: process.env.RUNNER_OS || null,
    self_hosted_windows_confirmed_here: process.env.GITHUB_ACTIONS === 'true' &&
      /^windows$/i.test(process.env.RUNNER_OS || '') && Boolean(process.env.RUNNER_NAME)},
  routes: declaredRoutes(),
  local_tools: {
    unity: detectedBinary(['UNITY_EDITOR_PATH'], ['Unity.exe', 'Unity']),
    blender: detectedBinary(['BLENDER_PATH', 'BLENDER_BIN'], ['blender.exe', 'blender']),
    ollama: detectedBinary(['OLLAMA_PATH'], ['ollama.exe', 'ollama']),
    git: detectedBinary([], ['git.exe', 'git']),
    ollama_http: await ollamaLocalVersion()
  },
  owner_production_pause: productionPause(),
  required_follow_up: [
    'Read live main/AGENTS.md, HEAD, and active workstreams before doing anything else.',
    'In a ChatGPT session: separately verify its connected GitHub tools; this process cannot see ChatGPT connectors.',
    'Check Actions runs/jobs and exact workflow/ref/runner rather than assuming a declared Windows route is online.',
    'Ollama not listening on this machine does not mean it is absent from the Windows PC. Inspect actual Windows runner evidence before concluding.',
    'Respect the current production pause and resource owners; this discovery never authorizes execution.'
  ],
  conclusion: 'Machine-local facts and repository route declarations only; NOT proof of remote Windows availability.'
};
process.stdout.write(JSON.stringify(report, null, 2) + '\n');
