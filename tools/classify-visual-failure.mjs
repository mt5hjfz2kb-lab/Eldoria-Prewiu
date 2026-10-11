import fs from 'node:fs';

const inputPath = process.argv[2] || '';
const out = process.argv[3] || '';
if (!inputPath) {
  console.error('VISUAL_FAILURE_CLASSIFIER=FAIL');
  console.error('Provide a diagnostic JSON path.');
  process.exit(2);
}
const d = JSON.parse(fs.readFileSync(inputPath, 'utf8'));
const allowed = new Set([
  'runtime_density_or_gpu',
  'source_contamination_or_framing',
  'geometry_identity_or_silhouette',
  'interaction_clickability_or_collision',
  'depth_or_occlusion_alignment',
  'surface_material_or_relief',
  'semantic_isolation',
  'camera_disocclusion_or_parallax',
  'unknown'
]);

let failureClass = 'unknown';
let reason = 'No single recognized signal.';
let ambiguous = false;

if (d.failure_class && allowed.has(d.failure_class)) {
  failureClass = d.failure_class;
  reason = 'Explicit validated failure_class supplied by upstream gate.';
} else {
  const signals = new Set(Array.isArray(d.signals) ? d.signals : []);
  const mapping = [
    ['runtime_density_or_gpu', ['gpu_oom','memory_failure','density_instability','renderer_crash_at_density']],
    ['source_contamination_or_framing', ['hud_baked_in','source_contamination','bad_framing','dirty_source']],
    ['interaction_clickability_or_collision', ['raycast_fail','selection_fail','clickability_fail','collision_fail','hotspot_fail']],
    ['geometry_identity_or_silhouette', ['silhouette_fail','geometry_identity_fail','architectural_identity_fail','deformed_mesh']],
    ['camera_disocclusion_or_parallax', ['disocclusion_fail','parallax_fail','camera_envelope_fail','projection_seam']],
    ['depth_or_occlusion_alignment', ['occlusion_fail','depth_alignment_fail','depth_order_fail']],
    ['surface_material_or_relief', ['surface_fail','material_fail','pbr_fail','relief_fail','lighting_material_mismatch']],
    ['semantic_isolation', ['semantic_isolation_fail','segmentation_fail']]
  ];
  const hits = mapping.filter(([,keys]) => keys.some(k => signals.has(k))).map(([cls]) => cls);
  if (hits.length === 1) {
    failureClass = hits[0];
    reason = 'Exactly one known failure class matched diagnostic signals.';
  } else if (hits.length > 1) {
    ambiguous = true;
    reason = 'Multiple failure classes matched; automatic routing is intentionally blocked.';
  }
}

const result = {
  schema_version: 1,
  failure_class: failureClass,
  ambiguous,
  auto_route_allowed: !ambiguous && failureClass !== 'unknown',
  reason
};
console.log('VISUAL_FAILURE_CLASSIFIER=PASS');
console.log(JSON.stringify(result, null, 2));
if (out) fs.writeFileSync(out, JSON.stringify(result, null, 2) + '\n');
