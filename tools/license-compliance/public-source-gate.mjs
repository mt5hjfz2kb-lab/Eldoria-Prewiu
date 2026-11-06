import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

export const PROTECTED_IMPORTED_ROOTS = Object.freeze([
  'Unity/Assets/Bublik/',
  'Unity/Assets/EmaceArt/',
  'Unity/Assets/Holotna/',
  'Unity/Assets/Mega Fantasy Props Pack/',
  'Unity/Assets/NatureStarterKit2/',
  'Unity/Assets/PolyOne/',
  'Unity/Assets/Tree_Packs/',
  'Unity/Assets/WorldPackCompare/Quaternius/',
]);

// The game and historical artifacts are deliberately NOT on this denylist.
// Never remove an existing player dependency on the strength of this check.
const FIRST_PARTY_TOP_LEVEL = new Set([
  'Eldoria', 'Plugins', 'StreamingAssets', '_Recovery', '_TerrainAutoUpgrade',
]);
const NON_ASSET_SUFFIX = /(?:\.meta|\.md|\.txt|\.license|\.notice|\.gitignore|\.gitattributes)$/i;
const ART_SOURCE_EXT = /\.(?:png|jpe?g|webp|tga|psd|exr|hdr|fbx|obj|glb|gltf|blend|wav|ogg|mp3|mp4|unitypackage|ttf|otf|woff2?|sbsar)$/i;
const POSSIBLE_EXTERNAL_ASSET_EXT = /\.(?:png|jpe?g|webp|tga|psd|exr|hdr|fbx|obj|glb|gltf|blend|prefab|asset|mat|unitypackage|shader|dll|so|aar|cs|wav|ogg|mp3|mp4)$/i;
const ALLOWED_RISK = new Set(['CRITICAL', 'HIGH', 'MEDIUM']);

export function inspectPullRequestFiles(files) {
  if (!Array.isArray(files)) throw new Error('GitHub PR files payload must be an array');
  const blocking = [], advisory = [];
  for (const item of files) {
    const name = item?.filename;
    const previous = item?.previous_filename || '';
    const status = item?.status;
    if (typeof name !== 'string' || !name || /(^\/|\\|(?:^|\/)\.\.(?:\/|$))/.test(name))
      throw new Error('PR filename is missing or unsafe');
    if (!['added', 'modified', 'renamed', 'copied', 'removed', 'changed', 'unchanged'].includes(status))
      throw new Error('Unrecognized PR status for ' + name + ': ' + status);
    // Deletions are not a rights grant; they are allowed because they can reduce exposure.
    // They do not erase historical copies, and removal still needs consumer/Unity testing.
    if (status === 'removed' || status === 'unchanged') continue;
    const candidateNames = [name, previous].filter(Boolean);
    if (candidateNames.some(n => PROTECTED_IMPORTED_ROOTS.some(prefix => n.startsWith(prefix)))) {
      if (!candidateNames.some(n => !NON_ASSET_SUFFIX.test(n) && (POSSIBLE_EXTERNAL_ASSET_EXT.test(n) || !path.posix.extname(n)))) continue;
      blocking.push({ file: name, status, reason: 'IMPORTED_VENDOR_SOURCE_REQUIRES_REDISTRIBUTION_RIGHTS_AND_OWNER_REVIEW' });
      continue;
    }
    const top = /^Unity\/Assets\/([^/]+)(?:\/|$)/.exec(name)?.[1];
    const isUnknownTopLevel = top && !FIRST_PARTY_TOP_LEVEL.has(top)
      && !PROTECTED_IMPORTED_ROOTS.some(root => root.startsWith('Unity/Assets/' + top + '/'));
    if (isUnknownTopLevel && status === 'added' && !NON_ASSET_SUFFIX.test(name)) {
      blocking.push({ file: name, status, reason: 'NEW_UNKNOWN_UNITY_ASSET_PACK_REQUIRES_ORIGIN_AND_LICENSE' });
      continue;
    }
    if ((name.startsWith('ArtSource/') || name.startsWith('art-source/')
        || name.startsWith('Unity/Assets/Eldoria/')) && ART_SOURCE_EXT.test(name)
        && (status === 'added' || status === 'renamed' || status === 'modified')) {
      advisory.push({ file: name, status, reason: 'CHECK_ORIGIN_AND_TRIPO_INPUT_RIGHTS_BEFORE_COMMERCIAL_RELEASE' });
    }
  }
  return { checked_files: files.length, blocking, advisory,
    public_source_license_cleared: false };
}

export function validateRightsEvidence(ledger, shelf) {
  const errors = [], warnings = [];
  if (ledger?.schema_version !== 1 || !Array.isArray(ledger?.families))
    errors.push('License audit ledger schema/records missing');
  const seen = new Set();
  for (const record of ledger?.families || []) {
    if (!record?.id || seen.has(record.id)) errors.push('Missing/duplicate license record id');
    seen.add(record?.id);
    if (!ALLOWED_RISK.has(record?.risk)) errors.push('Unknown risk priority for ' + record?.id);
    if (!Array.isArray(record?.proof_required) || !record.proof_required.length)
      errors.push('Missing evidence requirements for ' + record?.id);
  }
  const items = (shelf?.groups || []).flatMap(group => group.items || []);
  if (!Array.isArray(shelf?.groups) || !items.length)
    errors.push('Production shelf is missing');
  for (const item of items) {
    if (typeof item.license_certified !== 'boolean') {
      errors.push('license_certified must be explicit for ' + item?.path); continue;
    }
    if (item.license_certified === true) {
      // A public record must contain REFERENCES to private proof, not copies of
      // the owner's receipts or any provider credentials.
      const proof = item.license_evidence;
      const mandatory = ['licensor', 'license_id', 'private_proof_reference',
        'verified_by', 'verified_at', 'allowed_embedding', 'allowed_public_sources'];
      if (!proof || mandatory.some(k => typeof proof[k] !== 'string' || !proof[k].trim()))
        errors.push('Unsubstantiated TRUE license_certified for ' + item.path);
    } else {
      warnings.push('Rights not yet certified: ' + item.path);
    }
  }
  return { pass: errors.length === 0, errors, warnings, ledger_families: seen.size,
    shelf_items: items.length, pending_rights: warnings.length };
}

async function getPullFiles(repo, number, token) {
  if (!/^[a-z\d_.-]+\/[a-z\d_.-]+$/i.test(repo) || !/^[1-9]\d*$/.test(String(number)))
    throw new Error('Invalid GitHub repository or PR number');
  if (!token) throw new Error('A read-only GitHub API token is required for PR inspection');
  const out = [];
  for (let page = 1; page <= 30; page++) {
    const url = 'https://api.github.com/repos/' + repo + '/pulls/' + number + '/files?per_page=100&page=' + page;
    const response = await fetch(url, { headers: {
      Accept: 'application/vnd.github+json',
      Authorization: 'Bearer ' + token,
      'X-GitHub-Api-Version': '2022-11-28',
    } });
    if (!response.ok) throw new Error('GitHub PR file listing HTTP ' + response.status);
    const rows = await response.json();
    if (!Array.isArray(rows)) throw new Error('Expected an array from GitHub');
    out.push(...rows);
    if (rows.length < 100) return out;
  }
  throw new Error('PR files reach GitHub 3000-file limit: fail closed; inspect manually');
}

function readJson(p) { return JSON.parse(fs.readFileSync(p, 'utf8')); }

export async function main(argv = process.argv.slice(2)) {
  const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
  if (argv.includes('--verify-ledger')) {
    const ledger = readJson(path.join(root, 'docs/project-organization/LICENSE_PROVENANCE_EVIDENCE_20261106.json'));
    const shelf = readJson(path.join(root, 'pipeline/asset-vault/clean-production-shelf-v1.json'));
    const result = validateRightsEvidence(ledger, shelf);
    console.log('License evidence integrity (NOT a rights clearance):', JSON.stringify({
      ledger_families: result.ledger_families, shelf_items: result.shelf_items,
      pending_rights: result.pending_rights, errors: result.errors,
    }));
    if (!result.pass) process.exitCode = 1;
    return result;
  }
  const prIndex = argv.indexOf('--pr');
  const fixtureIndex = argv.indexOf('--fixture');
  if (prIndex < 0 && fixtureIndex < 0) throw new Error('Use --verify-ledger, --pr NUMBER, or --fixture FILE');
  const files = fixtureIndex >= 0
    ? readJson(argv[fixtureIndex + 1])
    : await getPullFiles(process.env.GITHUB_REPOSITORY, argv[prIndex + 1],
      process.env.GH_TOKEN || process.env.GITHUB_TOKEN);
  const result = inspectPullRequestFiles(files);
  console.log('Third-party public-source PR inspection (not a legal clearance):');
  console.log(JSON.stringify(result, null, 2));
  if (result.blocking.length) {
    console.error('BLOCK: public third-party source changed. Verify original rights and explicitly reconcile the licence before merge. Do not delete or modify game assets automatically.');
    process.exitCode = 1;
  }
  return result;
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url))
  main().catch(error => { console.error('License guard failed closed:', error.message); process.exitCode = 1; });
