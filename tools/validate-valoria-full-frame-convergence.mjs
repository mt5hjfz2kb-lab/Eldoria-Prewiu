import fs from 'node:fs';

const path = 'pipeline/valoria-full-frame-convergence-v1.json';
const cfg = JSON.parse(fs.readFileSync(path, 'utf8'));

const requiredLanes = [
  'composition_and_mass',
  'architecture_coherence',
  'terrain_and_transitions',
  'materials_and_surface',
  'lighting_and_value_hierarchy',
  'atmosphere_horizon_depth',
  'vegetation_and_environment',
  'life_props_storytelling',
  'corruption_world_threat',
  'hud_world_balance'
];

const fail = (msg) => {
  console.error('FULL_FRAME_CONVERGENCE_CONFIG_FAIL:', msg);
  process.exit(1);
};

if (!cfg.enabled) fail('configuration must stay enabled while Valoria is converging');
if (cfg.mode !== 'full_frame_batch') fail('mode must be full_frame_batch');
if (cfg.acceptance?.tech_pass_terminal !== false) fail('TECH PASS cannot be terminal');
if (cfg.acceptance?.local_visual_pass_terminal !== false) fail('LOCAL VISUAL PASS cannot be terminal');
if (cfg.acceptance?.require_full_frame_visual_pass !== true) fail('FULL-FRAME VISUAL PASS must be required');
if (cfg.batch_policy?.execute_all_currently_actionable_zero_credit_defects !== true) fail('batch execution policy must remain enabled');
if (cfg.batch_policy?.isolated_micro_pass_terminal_forbidden !== true) fail('micro-pass terminal acceptance must remain forbidden');
if (cfg.publish?.forbid_stale_owner_review !== true) fail('stale owner review must remain forbidden');

const lanes = new Set(cfg.lanes || []);
for (const lane of requiredLanes) {
  if (!lanes.has(lane)) fail(`missing required lane: ${lane}`);
}

const views = new Set(cfg.acceptance?.official_views || []);
for (const view of ['zoom19','zoom12','zoom9','mobile']) {
  if (!views.has(view)) fail(`missing official view: ${view}`);
}

console.log('FULL_FRAME_CONVERGENCE_CONFIG_PASS');
console.log(JSON.stringify({
  mode: cfg.mode,
  lane_count: cfg.lanes.length,
  official_views: cfg.acceptance.official_views,
  terminal_acceptance: 'FULL_FRAME_VISUAL_PASS'
}, null, 2));
