import fs from "node:fs";

const fail = (message) => {
  console.error("VALORIA_VISUAL_AUTHORITY_ROUTING=FAIL");
  console.error(message);
  process.exit(1);
};

const read = (path) => {
  if (!fs.existsSync(path)) fail("Missing required path: " + path);
  return fs.readFileSync(path, "utf8");
};

const manifest = JSON.parse(read("pipeline/visual-authority-routing-v1.json"));
const expectedSha = "8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689";

if (manifest?.canonical_visual_authority?.sha256 !== expectedSha)
  fail("Canonical Valoria visual authority SHA drifted.");

const productionGenerator = read(manifest.canonical_visual_authority.production_generator);
if (!productionGenerator.includes(expectedSha))
  fail("Production generator no longer fingerprints the canonical Valoria authority.");
if (!productionGenerator.includes("SHARP authoritative runtime scene"))
  fail("Production generator no longer declares the SHARP authoritative runtime scene.");
if (!productionGenerator.includes("ValoriaParcelPresentation"))
  fail("Production generator no longer constructs ValoriaParcelPresentation.");

read(manifest.canonical_visual_authority.production_workflow);

const visualWorld = read(manifest.legacy_fallback.implementation);
if (!visualWorld.includes("Original procedural study"))
  fail("VisualWorld legacy fallback lost its explicit legacy/provisional classification.");

const presenter = read("Unity/Assets/Eldoria/Scripts/Presentation/SlicePresenter.cs");
const parcelIndex = presenter.indexOf("if(productionParcels!=null) productionParcels.Apply(state)");
const fallbackIndex = presenter.indexOf("else if(city) VisualWorld.Create(true,state)");
if (parcelIndex < 0 || fallbackIndex < 0 || parcelIndex > fallbackIndex)
  fail("Canonical ValoriaParcelPresentation must remain preferred before the legacy VisualWorld fallback.");

const benchmark = read(manifest.generic_benchmark.producer);
if (!benchmark.includes("VisualWorld.Create(true,state)"))
  fail("Generic benchmark classification must be reviewed because it no longer exercises the known legacy Valoria fallback.");

const genericWorkflow = read(manifest.generic_benchmark.workflow);
if (!genericWorkflow.includes("ValoriaBenchmarkCapture.Capture"))
  fail("Generic benchmark workflow changed; authority classification must be reviewed.");
if (!manifest.current_screenshot_policy.forbidden_as_canonical.includes("eldoria-valoria-captures-*"))
  fail("Generic benchmark artifact pattern must remain forbidden as canonical Valoria evidence.");

console.log("VALORIA_VISUAL_AUTHORITY_ROUTING=PASS");
console.log("CANONICAL_VALORIA=SHARP + ValoriaParcelPresentation");
console.log("GENERIC_BENCHMARK=NONCANONICAL_MIXED_REGRESSION");
console.log("LEGACY_VISUALWORLD=FALLBACK_QA_ONLY");
if (manifest.legacy_fallback.runtime_cutover_pending)
  console.log("RUNTIME_CUTOVER=PENDING_OWNER_RELEASE_OF_SLICEPRESENTER");
