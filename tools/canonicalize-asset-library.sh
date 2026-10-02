#!/usr/bin/env bash
set -euo pipefail

ROOT="${RUNNER_TEMP:-/tmp}/asset-library-canonicalization"
rm -rf "$ROOT"
mkdir -p "$ROOT"

recover() {
  local name="$1" artifact="$2" expected="$3" destination="$4"
  local safe zip out actual
  safe="$(echo "$name" | tr -cs 'A-Za-z0-9_-' '_')"
  zip="$ROOT/$safe.zip"
  out="$ROOT/$safe"
  mkdir -p "$out" "$(dirname "$destination")"

  curl -fL     -H "Authorization: Bearer $GH_TOKEN"     -H "Accept: application/vnd.github+json"     -H "X-GitHub-Api-Version: 2022-11-28"     "https://api.github.com/repos/$GITHUB_REPOSITORY/actions/artifacts/$artifact/zip"     -o "$zip"

  unzip -q "$zip" -d "$out"

  mapfile -d '' candidates < <(find "$out" -type f -name '*.glb' -print0)
  matches=()
  for file in "${candidates[@]}"; do
    [[ "$(sha256sum "$file" | awk '{print $1}')" == "$expected" ]] && matches+=("$file")
  done
  if [[ ${#matches[@]} -ne 1 ]]; then
    echo "$name expected exactly one SHA match; found ${#matches[@]}" >&2
    exit 1
  fi

  cp "${matches[0]}" "$destination"
  actual="$(sha256sum "$destination" | awk '{print $1}')"
  [[ "$actual" == "$expected" ]] || { echo "$name destination SHA mismatch" >&2; exit 1; }
  echo "RECOVERED $name | artifact=$artifact | sha256=$actual | bytes=$(stat -c%s "$destination") | $destination"
}

recover "TerraceStairRock" 10936719852 83fce93daeb5bb455ab89bb195c39f617b3ea69ff9bf29f938ae4744caff3e5e   "Unity/Assets/Eldoria/Resources/Valoria/Rescued/TerraceStairRock.glb"
recover "StreetLandingTransition" 10944255594 ef367d9f0f671cd29e1b02e2d36a2dfdea3acd6e789087e4da5fb5e087e81b01   "Unity/Assets/Eldoria/Resources/Valoria/Rescued/StreetLandingTransition.glb"
recover "GateStreetRiseRock MV1" 10941357361 9d1a97ea385d557b77779eef7a7b65f8027bfc79a56b26081abc6b62f25a8302   "Unity/Assets/Eldoria/Resources/Valoria/HistoricalLandmarks/GateStreetRiseRock_MV1.glb"

old="Unity/Assets/Resources/Valoria/MidTierArchitectureKit_v1"
new="Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1"
if [[ -d "$old" ]]; then
  rm -rf "$new"
  mkdir -p "$(dirname "$new")"
  mv "$old" "$new"
  [[ ! -f "$old.meta" ]] || mv "$old.meta" "$new.meta"
fi
for p in Piece01 Piece02 Piece03 Piece04; do
  test -f "$new/$p.glb"
  test -f "$new/$p.glb.meta"
done

python - <<'PY'
import hashlib, json, pathlib

roots=[
    pathlib.Path("Unity/Assets/Eldoria/Resources/Valoria"),
    pathlib.Path("Unity/Assets/Eldoria/Resources/WorldPlayerCity"),
]

template="""fileFormatVersion: 2
guid: {guid}
ScriptedImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 2
  userData:
  assetBundleName:
  assetBundleVariant:
  script: {{fileID: 11500000, guid: 715df9372183c47e389bb6e19fbc3b52, type: 3}}
  editorImportSettings:
    generateSecondaryUVSet: 0
  importSettings:
    nodeNameMethod: 1
    animationMethod: 2
    generateMipMaps: 1
    texturesReadable: 0
    defaultMinFilterMode: 9729
    defaultMagFilterMode: 9729
    anisotropicFilterLevel: 1
  instantiationSettings:
    mask: -1
    layer: 0
    skinUpdateWhenOffscreen: 1
    lightIntensityFactor: 1
    sceneObjectCreation: 2
  assetDependencies: []
  reportItems: []
"""

created=[]
assets=[]
for root in roots:
    for glb in sorted(root.rglob("*.glb")):
        meta=pathlib.Path(str(glb)+".meta")
        if not meta.exists():
            guid=hashlib.sha256(("eldoria-canonical-glb-meta-v1:"+glb.as_posix()).encode()).hexdigest()[:32]
            meta.write_text(template.format(guid=guid), encoding="utf-8")
            created.append({"path":glb.as_posix(),"guid":guid})
        b=glb.read_bytes()
        assets.append({
            "path":glb.as_posix(),
            "bytes":len(b),
            "sha256":hashlib.sha256(b).hexdigest(),
            "meta":meta.exists(),
        })

missing=[a["path"] for a in assets if not a["meta"]]
if missing:
    raise SystemExit("Canonical GLBs missing .meta: "+", ".join(missing))

checks={
  "Unity/Assets/Eldoria/Resources/Valoria/Rescued/TerraceStairRock.glb":"83fce93daeb5bb455ab89bb195c39f617b3ea69ff9bf29f938ae4744caff3e5e",
  "Unity/Assets/Eldoria/Resources/Valoria/Rescued/StreetLandingTransition.glb":"ef367d9f0f671cd29e1b02e2d36a2dfdea3acd6e789087e4da5fb5e087e81b01",
  "Unity/Assets/Eldoria/Resources/Valoria/HistoricalLandmarks/GateStreetRiseRock_MV1.glb":"9d1a97ea385d557b77779eef7a7b65f8027bfc79a56b26081abc6b62f25a8302",
}
for path,expected in checks.items():
    actual=hashlib.sha256(pathlib.Path(path).read_bytes()).hexdigest()
    if actual!=expected:
        raise SystemExit(f"{path} SHA mismatch {actual}")
    print("SHA_OK", path, actual)

old=pathlib.Path("Unity/Assets/Resources/Valoria/MidTierArchitectureKit_v1")
if old.exists():
    raise SystemExit("Old Mid-Tier path still exists")
new=pathlib.Path("Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1")
for n in ("Piece01","Piece02","Piece03","Piece04"):
    if not (new/f"{n}.glb").exists() or not (new/f"{n}.glb.meta").exists():
        raise SystemExit(f"Missing canonical Mid-Tier {n}")

audit={
    "schema_version":1,
    "glb_count":len(assets),
    "missing_meta_count":0,
    "generated_meta_count":len(created),
    "generated_meta":created,
    "assets":assets,
}
pathlib.Path("pipeline/asset-library-canonicalization-audit.json").write_text(
    json.dumps(audit,indent=2)+"\n", encoding="utf-8"
)
print(json.dumps(audit,indent=2))
PY

git config user.name "eldoria-art-runner"
git config user.email "actions@users.noreply.github.com"
git fetch origin main
remote="$(git rev-parse origin/main)"
if [[ "$remote" != "$GITHUB_SHA" ]]; then
  echo "main advanced during canonicalization ($remote != $GITHUB_SHA)" >&2
  exit 2
fi

git add -A Unity/Assets/Eldoria/Resources/Valoria
git add -A Unity/Assets/Eldoria/Resources/WorldPlayerCity
git add -A Unity/Assets/Resources/Valoria
git add pipeline/asset-library-canonicalization-audit.json

git diff --cached --stat
if git diff --cached --quiet; then
  echo "Canonical library already matches repository."
else
  git commit -m "chore: canonicalize Eldoria asset library"
  git push origin HEAD:main
fi
