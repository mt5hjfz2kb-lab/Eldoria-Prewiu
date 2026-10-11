import fs from 'node:fs';

const manifestPath = process.argv[2] || 'Unity/Packages/manifest.json';
const lockPath = process.argv[3] || 'Unity/Packages/packages-lock.json';
const outPath = process.argv[4] || 'unity-environment-capabilities-static.json';

const manifest = JSON.parse(fs.readFileSync(manifestPath,'utf8'));
const lock = JSON.parse(fs.readFileSync(lockPath,'utf8'));
const deps = manifest.dependencies || {};
const locked = lock.dependencies || {};

function version(name){ return deps[name] || locked[name]?.version || null; }
function available(name){ return Boolean(version(name)); }

const urp = version('com.unity.render-pipelines.universal');
const shadergraph = version('com.unity.shadergraph');
const terrain = version('com.unity.modules.terrain');

const checks = [
  {name:'URP', available:!!urp, detail:urp},
  {name:'ShaderGraph', available:!!shadergraph, detail:shadergraph},
  {name:'TerrainModule', available:!!terrain, detail:terrain},
  {name:'DecalProjector_API', available:!!urp, detail:urp ? 'URP package present; runtime/API proof still required before production certification.' : 'URP missing'},
  {name:'MaterialPropertyBlock_API', available:true, detail:'UnityEngine core API; runtime use still needs Eldoria proof.'},
  {name:'LightProbeGroup_API', available:true, detail:'UnityEngine core API; placement strategy not yet certified.'},
  {name:'ReflectionProbe_API', available:true, detail:'UnityEngine core API; strategy not yet certified.'},
  {name:'LODGroup_API', available:true, detail:'UnityEngine core API; Blender LOD family proof already validated separately.'},
  {name:'GPUInstancing_material_flag', available:!!urp, detail:urp ? 'URP present; material/shader-specific proof required.' : 'URP missing'},
  {name:'VolumeFramework', available:available('com.unity.render-pipelines.core'), detail:version('com.unity.render-pipelines.core')}
];

const report = {
  schema_version:1,
  mode:'static_package_capability_audit',
  zero_spend:true,
  manifest:manifestPath,
  packages:{urp,shadergraph,terrain},
  checks,
  note:'This proves package/API availability only. It does not promote any visual technique to Eldoria-certified automation.'
};

fs.writeFileSync(outPath,JSON.stringify(report,null,2)+'\n');
console.log(JSON.stringify(report,null,2));
