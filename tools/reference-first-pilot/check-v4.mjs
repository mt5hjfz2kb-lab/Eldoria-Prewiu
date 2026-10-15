import fs from "node:fs";
import assert from "node:assert/strict";

const root = "Unity/Assets/Eldoria/ArtTests/ReferenceFirstPilot/Editor/";
const v3 = fs.readFileSync(root+"ReferenceFirstCompositionV3Capture.cs","utf8");
const v4 = fs.readFileSync(root+"ReferenceFirstCompositionV4Capture.cs","utf8");
const workflow = fs.readFileSync(".github/workflows/eldoria-reference-first-zero-budget-pilot.yml","utf8");
const truth = fs.readFileSync("docs/evidence/reference-first-zero-budget-visual-pilot-20261015/CANONICAL_USER_REFERENCE_V3.md","utf8");

assert.match(truth,/de476e0516f744f9cd899419404971fa5e8d42bd49f2f7db7670d1cb325f8a4f/,"Incorrect user reference");
assert.match(v4,/class ReferenceFirstCompositionV4Capture/);
assert.match(v4,/USER CANONICAL 1513x1039 SHA256 de476e0516f744f9cd899419404971fa5e8d42bd49f2f7db7670d1cb325f8a4f/);
assert.match(v4,/OrganicGroundTexture/);
assert.match(v4,/Texture2D\(res,res,TextureFormat.RGB24,true\)/);
assert.match(v4,/OrganicTerrainBase\.asset/);
assert.match(v4,/Mathf.PerlinNoise/);
assert.match(v4,/Continuous low flank wall/);
assert.match(v4,/LOWER BRIDGE GATE/);
assert.match(v4,/UPPER DEFENSIVE GATE/);
assert.match(v4,/CENTRAL STAIR step/);
assert.match(v4,/BRIDGE stone deck/);
assert.match(v4,/MAIN AXIS/);
assert.match(v4,/UnityEngine\.Application\.isBatchMode/);
assert.match(v4,/UnityEngine\.Application\.dataPath/);
assert.doesNotMatch(v4,/UnityEngine\\.UnityEngine\\.Application|(?<!UnityEngine\\.)Application\\.dataPath/,"invalid Application namespace");
assert.doesNotMatch(v4,/Repeatable earth variation|new Vector3\(54f,4\.8f,3\.2f\)/,"V3 rejected decal/floating masonry still present");
assert.match(v4,/artApproved = false/);
assert.match(v4,/productionModified = false/);
assert.match(v4,/licenseVerified = false/);
for(const item of ["ReferenceFirstCompositionV4Capture.Capture","V4/Imported","reference-first-composition-v4",
    "render-manifest-v4.json","07-reference-materials-overview.png","08-reference-materials-close.png"]){
   assert.ok(workflow.includes(item),"missing workflow reference "+item);
}
assert.ok(v3.includes("ReferenceFirstCompositionV3Capture"),"Prior genuine capture version was overwritten");
for(const p of ["Unity/Assets/Eldoria/Resources/Valoria/Stone_Wall.prefab",
                "Unity/Assets/Eldoria/Resources/Valoria/Stone_Tower.prefab",
                "Unity/Assets/Eldoria/Resources/WorldInventory/Tree01A.prefab",
                "Unity/Assets/Eldoria/Resources/WorldInventory/Rock01.prefab",
                "Unity/Assets/Eldoria/ArtTests/OriginalHero/Textures/rock.png"]){
    assert.ok(fs.existsSync(p), "Missing actual source "+p);
}
console.log("V4_STATIC_REFERENCE_FIRST_PASS: owner exact image identified, authentic source assets exist, isolated camera workflow targets V4; art approval remains FALSE");
