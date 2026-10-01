import fs from 'node:fs';

const path = process.argv[2] || 'pipeline/art-production-request.json';
const out = process.argv[3] || '';
const req = JSON.parse(fs.readFileSync(path, 'utf8'));

const profiles = new Set([
  'environment_composition',
  'environment_surface',
  'environment_new_geometry',
  'animated_asset'
]);

function fail(message) {
  console.error('ART_PRODUCTION_PLAN=FAIL');
  console.error(message);
  process.exit(2);
}

if (!profiles.has(req.profile)) fail('Unsupported profile: ' + req.profile);

const approved = Boolean(req.credit_authorization && req.credit_authorization.approved);
const cost = Number((req.credit_authorization && req.credit_authorization.authorized_credit_cost) || 0);
const allowTripo = Boolean(req.allow_tripo);
const gap = Boolean(req.geometry_gap_proven);
const cap = req.capabilities || {};

if (approved && cost <= 0) fail('Credit authorization approved=true requires a positive authorized_credit_cost.');
if (allowTripo && req.profile !== 'environment_new_geometry' && req.profile !== 'animated_asset') {
  fail('Tripo is not allowed for composition/surface profiles.');
}
if (req.profile === 'environment_new_geometry' && !gap) {
  fail('New geometry route requires geometry_gap_proven=true. Run modular/composition proof first.');
}
if (allowTripo && !approved && cost !== 0) {
  fail('Unapproved Tripo route must keep authorized_credit_cost=0.');
}

const stages = [];
let stopBeforeSpend = false;

if (req.profile === 'environment_composition') {
  stages.push('unity_modular_assembly');
  stages.push('unity_environment_art');
  stages.push('official_camera_validation');
}
if (req.profile === 'environment_surface') {
  stages.push('unity_surface_diagnosis');
  if (cap.blender_bake || cap.blender_material_consolidation) {
    stages.push('blender_surface_processing');
  }
  stages.push('unity_environment_art');
  stages.push('official_camera_validation');
}
if (req.profile === 'environment_new_geometry') {
  stages.push('geometry_gap_evidence');
  stages.push('tripo_exact_input_or_source_preflight');
  stages.push('tripo_cost_probe');
  if (!allowTripo || !approved) {
    stopBeforeSpend = true;
  } else {
    if (cap.tripo_parts) stages.push('tripo_parts_or_segmentation');
    if (cap.tripo_retopology) stages.push('tripo_retopology');
    stages.push('tripo_generate_or_transform');
    stages.push('blender_production_processing');
    stages.push('unity_environment_art');
    stages.push('official_camera_validation');
  }
}
if (req.profile === 'animated_asset') {
  stages.push('character_pipeline_preflight');
  if (!allowTripo || !approved) stopBeforeSpend = true;
  else stages.push('tripo_character_generation_or_rigging');
  stages.push('character_validation');
}

const plan = {
  schema_version: 1,
  enabled: Boolean(req.enabled),
  request_id: req.request_id,
  asset_or_sector: req.asset_or_sector,
  profile: req.profile,
  zero_spend_default: true,
  geometry_gap_proven: gap,
  tripo_allowed: allowTripo,
  credit_authorized: approved,
  authorized_credit_cost: cost,
  stop_before_credit_spend: stopBeforeSpend,
  stages,
  validation: req.validation || {},
  capabilities_requested: cap
};

const json = JSON.stringify(plan, null, 2);
console.log('ART_PRODUCTION_PLAN=PASS');
console.log(json);
if (out) fs.writeFileSync(out, json + '\n');
