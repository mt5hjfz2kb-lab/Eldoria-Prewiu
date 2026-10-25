$ErrorActionPreference='Stop'
$branch='feature/world4x-region1-canon-20261022'
if($env:GITHUB_EVENT_NAME -ne 'push' -or $env:GITHUB_REF_NAME -ne $branch){throw 'Candidate branch only'}
$headers=@{Authorization="Bearer $env:GH_TOKEN";Accept='application/vnd.github+json'}
$repo='https://api.github.com/repos/mt5hjfz2kb-lab/Eldoria-Prewiu'
$raw=Invoke-RestMethod -Headers $headers -Uri "$repo/contents/pipeline/active-workstreams.json?ref=main"
$registry=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($raw.content))|ConvertFrom-Json
$lease=@($registry.active|Where-Object {$_.id -eq 'eldoria-world-region-1-canon-20261022'})
$sourceRegistry=Get-Content (Join-Path $env:GITHUB_WORKSPACE 'world4x-source/pipeline/active-workstreams.json') -Raw | ConvertFrom-Json
$sourceLease=@($sourceRegistry.active|Where-Object {$_.id -eq 'eldoria-world-region-1-canon-20261022'})
if($lease.Count -ne 1 -or $sourceLease.Count -ne 1 -or $lease[0].owner -ne $sourceLease[0].owner -or $lease[0].ownership_revision -ne $sourceLease[0].ownership_revision -or $lease[0].write_scope_released -or $lease[0].resources -notcontains 'windows-runner-heavy'){throw 'World ownership changed'}
[xml]$tests=Get-Content (Join-Path $env:RUNNER_TEMP 'eldoria-region1-canon-tests.xml')
$cases=@($tests.SelectNodes('//test-case'))
if($cases.Count -lt 62 -or @($cases|Where-Object {$_.result -ne 'Passed'}).Count){throw 'Actual candidate tests must pass before materialization'}
$remote=Invoke-RestMethod -Headers $headers -Uri "$repo/git/ref/heads/$branch"
if($remote.object.sha -ne $env:GITHUB_SHA){throw 'Branch advanced: do not commit stale candidate'}
$root=Join-Path $env:GITHUB_WORKSPACE 'world4x-source'
Set-Location $root
if((git rev-parse HEAD).Trim() -ne $env:GITHUB_SHA){throw 'Checkout identity mismatch'}
$generated=Join-Path $env:RUNNER_TEMP "region1-authoring-$env:GITHUB_RUN_ID"
$assets='Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Blender'
$sources="docs/evidence/world4x-canon-20261022/sources/native-$env:GITHUB_RUN_ID"
New-Item -ItemType Directory -Force $sources|Out-Null
$paths=@()
foreach($file in @('RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes','production-region.json','RegionOneResources.glb','RegionOneResourceImpostors.png','RegionOneGrassDetail.png')){
 $path="$assets/$file"
 if((Get-FileHash $path).Hash -ne (Get-FileHash (Join-Path $generated $file)).Hash){throw "Tested asset differs: $file"}
 $paths+=$path
 if(Test-Path "$path.meta"){$paths+="$path.meta"}
}
# The pre-existing Tripo boar and finished Spawnling sources are immutable.
# Preserve exact imports and the Blender-authored boar far LOD in the same PR.
$creatures='Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Creatures'
foreach($name in @('RockBoarNear.glb','RockBoarFar.glb','SpawnlingNear.glb','SpawnlingFar.glb')){
 $path="$creatures/$name"
 if(-not(Test-Path $path) -or (Get-Item $path).Length -lt 100000){throw "Missing source-backed creature in native candidate: $path"}
 $paths+=$path
 if(Test-Path "$path.meta"){$paths+="$path.meta"}
}
if(Test-Path "$creatures.meta"){$paths+="$creatures.meta"}
foreach($file in @('Eldoria_Region1_Production.blend','Eldoria_Resource_Library.blend','RockBoar_Far_LOD.blend')){
 $path="$sources/$file";Copy-Item (Join-Path $generated $file) $path;$paths+=$path
}
# Keep the required genuine review views beside their exact editable source;
# artifact retention must not erase the six-scale/orientation delivery evidence.
$captures=Join-Path $env:RUNNER_TEMP "region1-canon-captures-$env:GITHUB_RUN_ID"
$review="$sources/review"
New-Item -ItemType Directory -Force $review|Out-Null
foreach($label in @('cercana-horizontal','cercana-vertical','regional-horizontal','regional-vertical','panoramica-horizontal','panoramica-vertical')){
 $metadata=Get-Content (Join-Path $captures "$label-source.json") -Raw|ConvertFrom-Json
 $png=Join-Path $captures "$label-unity.png"
 if($metadata.sha256 -ne (Get-FileHash $png -Algorithm SHA256).Hash.ToLower()){throw "Capture identity mismatch: $label"}
 foreach($suffix in @('-unity.png','-source.json')){
  $path="$review/$label$suffix";Copy-Item (Join-Path $captures "$label$suffix") $path;$paths+=$path
 }
}
foreach($file in @('eldoria-region1-canon-tests.xml')){
 $source=Join-Path $env:RUNNER_TEMP $file
 if(Test-Path $source){$path="$review/$file";Copy-Item $source $path;$paths+=$path}
}
$path="$review/retained-core-integrity.json";Copy-Item (Join-Path $generated 'retained-core-integrity.json') $path;$paths+=$path
foreach($file in Get-ChildItem (Join-Path $captures 'retained-sector') -File | Where-Object {$_.Extension -in @('.png','.json')}){
 $path="$review/sector-$($file.Name)";Copy-Item $file.FullName $path;$paths+=$path
}
foreach($file in Get-ChildItem (Join-Path $captures 'retained-sector/continental-details') -File){
 $path="$review/continent-$($file.Name)";Copy-Item $file.FullName $path;$paths+=$path
}
$creatureCaptures=Join-Path $captures 'retained-sector/creature-details'
if(-not(Test-Path $creatureCaptures)){throw 'Actual Tripo creature screenshots missing'}
foreach($file in Get-ChildItem $creatureCaptures -File){
 $path="$review/creature-$($file.Name)";Copy-Item $file.FullName $path;$paths+=$path
}
$breachCaptures=Join-Path $captures 'retained-sector/breach-details'
if(@(Get-ChildItem $breachCaptures -Filter '*.png').Count -ne 4){throw 'Real close and regional Breach captures required in both orientations'}
foreach($metadata in Get-ChildItem $breachCaptures -Filter '*.json'){
 $identity=Get-Content $metadata.FullName -Raw|ConvertFrom-Json
 $png=Join-Path $breachCaptures ($metadata.BaseName+'.png')
 if(-not $identity.actualUnityCamera -or -not $identity.playMode -or $identity.sha256 -ne (Get-FileHash $png -Algorithm SHA256).Hash.ToLower()){throw 'Breach evidence identity mismatch'}
}
foreach($file in Get-ChildItem $breachCaptures -File){$path="$review/breach-$($file.Name)";Copy-Item $file.FullName $path;$paths+=$path}
foreach($name in @('approved-node-authoring.json','ready.json','request.json','response.json','session-closed.json')){
 $file=Join-Path $generated $name
 if(-not(Test-Path $file)){throw "Actual BlenderMCP asset authoring trace missing: $name"}
 $path="$review/mcp-$name";Copy-Item $file $path;$paths+=$path
}
# General proof must be fresh and belong to this exact native source.
$general=Join-Path $env:RUNNER_TEMP "region1-general-proof-$env:GITHUB_RUN_ID"
$proof=Get-Content (Join-Path $general 'general-proof.json') -Raw|ConvertFrom-Json
if($proof.source -ne $env:GITHUB_SHA -or $proof.run -ne $env:GITHUB_RUN_ID -or $proof.editmode_passed -lt 26 -or $proof.gpu_playmode_passed -lt 62 -or $proof.probe_checks -lt 44 -or $proof.fresh_captures -lt 34){throw 'General integrated evidence is incomplete or stale'}
foreach($item in $proof.images){if($item.sha256 -ne (Get-FileHash (Join-Path $general $item.file) -Algorithm SHA256).Hash.ToLower()){throw 'General screenshot hash mismatch'}}
New-Item -ItemType Directory -Force "$review/general"|Out-Null
foreach($file in Get-ChildItem $general -File){$path="$review/general/$($file.Name)";Copy-Item $file.FullName $path;$paths+=$path}
$manifest=@{candidate_commit=$env:GITHUB_SHA;native_run=$env:GITHUB_RUN_ID;art_approved=$false;mobile_profiled=$false;files=@()}
foreach($path in $paths){
 if((Get-Item $path).Length -ge 100MB){throw "File exceeds repository object limit: $path"}
 $manifest.files+=@{path=$path;sha256=(Get-FileHash $path -Algorithm SHA256).Hash.ToLower();bytes=(Get-Item $path).Length}
}
$manifest|ConvertTo-Json -Depth 6|Set-Content "$sources/manifest.json"
$paths+="$sources/manifest.json"
# Stage an explicit allowlist only; runner-side city/material changes are excluded.
git add --sparse -- $paths
if($LASTEXITCODE){throw 'Could not stage verified candidate'}
$staged=@(git diff --cached --name-only)
foreach($path in $staged){if($paths -notcontains $path){throw "Unexpected staged path: $path"}}
git -c user.name='Eldoria World Production' -c user.email='41898282+github-actions[bot]@users.noreply.github.com' commit -m "world4x: preserve native $env:GITHUB_RUN_ID playable candidate assets and editable sources (art pending)"
if($LASTEXITCODE){throw 'Candidate commit failed'}
try {
 $env:GIT_CONFIG_COUNT='1';$env:GIT_CONFIG_KEY_0='http.https://github.com/.extraheader'
 $env:GIT_CONFIG_VALUE_0='AUTHORIZATION: basic '+[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("x-access-token:$env:GH_TOKEN"))
 git push origin "HEAD:refs/heads/$branch"
 if($LASTEXITCODE){throw 'Candidate push rejected; no force push attempted'}
} finally {
 Remove-Item Env:GIT_CONFIG_COUNT,Env:GIT_CONFIG_KEY_0,Env:GIT_CONFIG_VALUE_0 -ErrorAction SilentlyContinue
}
@{candidate_source=$env:GITHUB_SHA;assets_commit=(git rev-parse HEAD).Trim();run=$env:GITHUB_RUN_ID;merged=$false;published=$false}|ConvertTo-Json|Set-Content (Join-Path $env:RUNNER_TEMP 'region1-materialized-candidate.json')
