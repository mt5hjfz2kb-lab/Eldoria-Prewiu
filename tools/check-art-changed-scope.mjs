#!/usr/bin/env node
// Eldoria art production scope guard: a new source/scene cannot bypass the AI authoring request.
import fs from 'node:fs';
import path from 'node:path';

const grandfathered = new Set([
  'world-region-1-visual-convergence-v3-20261010',
  'valoria-bastion1-player-scene-integration-20261012'
]);
const globs = [
  /^art-source\//,
  /^Unity\/Assets\/Eldoria\/ArtTests\//,
  /^Unity\/Assets\/Eldoria\/Resources\/Valoria\//,
  /^Unity\/Assets\/Eldoria\/ProductionSlice\/Runtime\/Valoria\.unity$/,
  /^Unity\/Assets\/Eldoria\/.*\.(?:blend|glb|gltf|fbx|obj|prefab|mat|shader|shadergraph|terrainlayer|terrain|mesh|unity|png|jpe?g|tga|psd|exr|hdr|tiff?|asset)$/i,
  /^tools\/valoria-bastion1-assembly\//
];
export const isArtChange = file => globs.some(re => re.test(file));

function matchScope(pattern, filename) {
  const parts = String(pattern).split('/').map(part => {
    if (part === '**') return '.*';
    return part.split('*').map(s => s.replace(/[.*+?^\$\{\}()|[\]\\]/g, '\\$&')).join('[^/]*');
  });
  return new RegExp('^' + parts.join('/') + '$').test(filename);
}

export function checkArtChangedFiles(files, registry, requestFor) {
  const errors = [];
  for (const file of [...new Set(files.filter(isArtChange))]) {
    const relevant = (registry.active || []).filter(w =>
      (w.scope || []).some(scope => matchScope(scope, file)));
    if (relevant.length === 0) {
      errors.push(file + ': art asset changed without matching ACTIVE workstream ownership');
      continue;
    }
    const permitted = relevant.some(w => {
      if (w.art_production_routing !== 'required') return false;
      let req;
      try { req = requestFor(w); } catch { return false; }
      const correct = req && req.enabled === true && String(req.status).toUpperCase() === 'ACTIVE'
        && req.workstream_id === w.id && req.request_id === w.id;
      return correct && (req.ai_authorship_policy === 'required' || grandfathered.has(w.id));
    });
    if (!permitted) errors.push(file + ': no valid mandatory AI-authoring production request covers this art change');
  }
  return errors;
}

if (process.argv[1] && path.resolve(process.argv[1]) === path.resolve(new URL(import.meta.url).pathname)) {
  const at = process.argv.indexOf('--changed-files');
  if (at < 0 || !process.argv[at + 1]) {
    console.error('Usage: node tools/check-art-changed-scope.mjs --changed-files <git-diff-name-list.txt>');
    process.exit(2);
  }
  const files = fs.readFileSync(process.argv[at + 1], 'utf8').split(/\r?\n/).filter(Boolean);
  const registry = JSON.parse(fs.readFileSync('pipeline/active-workstreams.json', 'utf8'));
  const problems = checkArtChangedFiles(files, registry, w => {
    const file = w.art_production_request || 'pipeline/art-production-requests/' + w.id + '.json';
    return JSON.parse(fs.readFileSync(file, 'utf8'));
  });
  if (problems.length) { problems.forEach(x => console.error('ART_SCOPE_FAIL: ' + x)); process.exit(1); }
  console.log('ART_SCOPE_PASS: ' + files.filter(isArtChange).length + ' art asset/source changes are scoped to a mandatory AI authoring workstream');
}
