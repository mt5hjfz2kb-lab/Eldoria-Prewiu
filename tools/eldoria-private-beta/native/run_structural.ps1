param(
 [Parameter(Mandatory=$true)][string]$Source,
 [Parameter(Mandatory=$true)][string]$Tools,
 [Parameter(Mandatory=$true)][string]$Evidence
)
$ErrorActionPreference='Stop'
$Source=(Resolve-Path $Source).Path
$Tools=(Resolve-Path $Tools).Path
$Evidence=[IO.Path]::GetFullPath($Evidence)
$project=Join-Path $Source 'Unity'
$probe=Join-Path $project 'Assets/EldoriaBetaMemoryProbe'
$utf8=New-Object Text.UTF8Encoding($false)
$queryAsset=Join-Path $project 'Assets/Eldoria/World4XCanon/Resources/World4X/Blender/RuntimeTerrainQueries.bytes'
$baseline='948ffa9837168a0b68dab80be76b66350422b23a'
$overlays=@(
 'Unity/Assets/Eldoria/World4XCanon/RegionOneBlenderLandscape.cs',
 'Unity/Assets/Eldoria/World4XCanon/RegionOneCanonicalLandscape.cs',
 'Unity/Assets/Eldoria/World4XCanon/RegionOneResourcePlacement.cs',
 'Unity/Assets/Eldoria/World4XCanon/RegionOneTerrainRoutes.cs',
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs',
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaIntegrationBuild.cs'
)
. (Join-Path $Tools 'tools/eldoria-private-beta/native/ux/overlay.ps1')
New-Item -ItemType Directory -Force $Evidence|Out-Null
if((git -C $Source rev-parse HEAD).Trim() -ne $baseline){throw 'Wrong frozen source'}
if(@(git -C $Source status --porcelain).Count){throw 'Isolated source is not clean'}
if(Test-Path $probe){throw 'Refuse to replace an existing probe'}
if((Test-Path $queryAsset) -or (Test-Path ($queryAsset+'.meta'))){throw 'Refuse replacing an existing terrain query tape'}
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native tools already running'}
if(-not(Test-Path $env:UNITY_EDITOR_PATH)){throw 'Unity editor unavailable'}
$env:VALORIA_GAMEPLAY_PROBE='1'
$env:ELDORIA_MEMORY_PHASES='1'
$proof=Join-Path $Source 'docs/evidence/valoria-reference-clean'
function Set-NativePhase([string]$Description,[string]$State='pending'){
 if(-not $env:GH_TOKEN){return}
 $body=@{state=$State;context='beta/native-phase';description=$Description.Substring(0,[Math]::Min(140,$Description.Length));target_url="https://github.com/$env:GITHUB_REPOSITORY/actions/runs/$env:GITHUB_RUN_ID"}|ConvertTo-Json
 try {$null=Invoke-RestMethod -Method Post -Uri "https://api.github.com/repos/$env:GITHUB_REPOSITORY/statuses/$env:GITHUB_SHA" -Headers @{Authorization="Bearer $env:GH_TOKEN";Accept='application/vnd.github+json'} -ContentType 'application/json' -Body $body}catch{Write-Warning 'Native phase status publication unavailable'}
}
function Invoke-Native([string]$Method,[string]$Log,[string]$Target){
 if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native tools busy before phase'}
 $args=@('-batchmode','-force-d3d11','-buildTarget',$Target,'-projectPath',('"'+$project+'"'),'-executeMethod',$Method,'-logFile',('"'+$Log+'"'))
 Set-NativePhase "$Method (staged=$env:ELDORIA_EXPECT_STAGED; target=$Target)"
 Write-Host "Starting native phase $Method; log $Log"
 $p=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -PassThru
 $limit=if($Target -eq 'WebGL'){2400}elseif($Method -eq 'EldoriaBetaStructuralStartupProbe.Run' -and $env:ELDORIA_EXPECT_STAGED -eq '0'){3600}else{480}
 $deadline=(Get-Date).AddSeconds($limit)
 while(-not $p.WaitForExit(15000)){
  Write-Host "Native phase $Method still running; owned PID $($p.Id)"
  if(Test-Path $Log){Get-Content $Log -Tail 2|Write-Host}
  if((Get-Date) -gt $deadline){
   & taskkill.exe /PID $p.Id /T /F|Out-Null
   $null=$p.WaitForExit(15000)
   throw "Owned Unity phase $Method exceeded $limit seconds; its process tree stopped for preservation"
  }
 }
 if($p.ExitCode -ne 0){throw "Unity phase $Method failed: $($p.ExitCode)"}
}
function Run-Probe([string]$Mode){
 $env:ELDORIA_VALIDATE_TERRAIN_RAYCAST=if($Mode -eq 'staged' -or $Mode -eq 'route-baseline'){'1'}else{'0'}
 $env:ELDORIA_BAKE_TERRAIN_QUERIES=if($Mode -eq 'bake'){'1'}else{'0'}
 $env:ELDORIA_EXPECT_STAGED=if($Mode -ne 'baseline'){'1'}else{'0'}
 $env:ELDORIA_MEMORY_SOURCE=if($Mode -ne 'baseline'){$env:GITHUB_SHA}else{$baseline}
 $env:ELDORIA_MEMORY_OUT=Join-Path $Evidence $Mode
 New-Item -ItemType Directory -Force $env:ELDORIA_MEMORY_OUT|Out-Null
 Invoke-Native 'EldoriaBetaStructuralStartupProbe.Run' (Join-Path $env:ELDORIA_MEMORY_OUT 'unity.log') 'Win64'
 if(-not(Test-Path (Join-Path $env:ELDORIA_MEMORY_OUT 'complete.txt'))){throw "Probe $Mode completion absent"}
}
try {
 New-Item -ItemType Directory -Force (Join-Path $probe 'Editor')|Out-Null
 Copy-Item (Join-Path $Tools 'tools/eldoria-private-beta/native/StructuralStartupProbe.cs') (Join-Path $probe 'Editor/StructuralStartupProbe.cs')
 Copy-Item (Join-Path $Tools 'tools/eldoria-private-beta/native/CollisionBake.cs') (Join-Path $probe 'Editor/CollisionBake.cs')
 # Compile all verification code at the first native phase, not after the long journey.
 Copy-Item (Join-Path $Tools 'tools/eldoria-private-beta/native/ux/PretestUXProbe.cs') (Join-Path $probe 'Editor/PretestUXProbe.cs')
 # The unchanged frozen baseline was already executed twice with this topology
 # probe and the exact restored input audit. Reuse only its hash-pinned results.
 $previous=Join-Path $env:GITHUB_WORKSPACE 'beta-structural-evidence-42513182378-1/baseline'
 $expected=@{
  'startup-0.json'='a80062ea436957cba5cb1681440b9704a01c5e1896f9682a32408a1179833050'
  'startup-1.json'='4f02bedcaeaaf63eff5a44aeffe4275727d83f83d71ab38c1d3269d3639f3974'
  'complete.txt'='7c9a5636807e61e0aa2630d9e41b61037a9cb55ff62321b52b718a3838c974fb'
 }
 foreach($name in $expected.Keys){if(-not(Test-Path (Join-Path $previous $name)) -or (Get-FileHash (Join-Path $previous $name) -Algorithm SHA256).Hash -ne $expected[$name]){throw 'Reviewed frozen baseline evidence changed'}}
 foreach($pass in 0,1){
  $b=Get-Content (Join-Path $previous "startup-$pass.json") -Raw|ConvertFrom-Json
  if($b.source -ne $baseline -or $b.inputAudit -ne $env:ELDORIA_BASELINE_INPUT_AUDIT -or -not $b.canonicalSavesUnchanged -or -not $b.topologyHash){throw 'Frozen baseline provenance mismatch'}
 }
 Copy-Item $previous (Join-Path $Evidence 'baseline') -Recurse
 [IO.File]::WriteAllText((Join-Path $Evidence 'baseline-reuse.json'),(@{origin_run='42513182378';source=$baseline;input_audit=$env:ELDORIA_BASELINE_INPUT_AUDIT;hashes=$expected}|ConvertTo-Json -Depth 4),$utf8)
 $hashes=@()
 foreach($path in $overlays){
  $from=Join-Path $Tools $path;$to=Join-Path $Source $path
  if(($env:ELDORIA_P0_CITY_ONLY_BUILD -eq '1' -or $env:ELDORIA_P0_LAZY_WORLD_BUILD -eq '1') -and
     $path -eq 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs'){
   # The frozen beta10 UX manifest expects EXACTLY the old native-PASS
   # host as input (SHA256 19e289...). The newer P0 engine file is not
   # admissible: do not stage both and silently lose the UX.
   $from=Join-Path $Tools 'tools/eldoria-private-beta/native/iphone-hotfix/ValoriaIntegratedWorld.beta13-engine-control.cs.txt'
   $expectedHostSha='19E289FF881B483748EA627C0B570768498B798CB0CF684E8C2A45ECCD11CB88'
   if(-not(Test-Path $from -PathType Leaf)){throw 'Frozen native-PASS engine host absent'}
   $diskHostSha=(Get-FileHash $from -Algorithm SHA256).Hash
   if($diskHostSha -ne $expectedHostSha){
     # Git for Windows core.autocrlf=true can write a *.cs.txt as CRLF.
     # Only the exact LF source SHA is acceptable; no semantic modifications.
     $rawHost=[IO.File]::ReadAllBytes($from)
     $strictUtf8=New-Object Text.UTF8Encoding($false,$true)
     $hostText=$strictUtf8.GetString($rawHost)
     $lfHost=$hostText.Replace("`r`n","`n")
     if($lfHost -ceq $hostText -or $lfHost.Contains("`r")){throw "Immutable engine host differs: raw=$diskHostSha; not CRLF-only"}
     $lfBytes=[Text.Encoding]::UTF8.GetBytes($lfHost)
     $hasher=[Security.Cryptography.SHA256]::Create()
     try{$lfHash=[BitConverter]::ToString($hasher.ComputeHash($lfBytes)).Replace('-','')}finally{$hasher.Dispose()}
     Write-Host "Frozen host byte audit: raw=$diskHostSha normalized=$lfHash expected=$expectedHostSha"
     if($lfHash -ne $expectedHostSha){throw 'Immutable engine-host bytes differ after CRLF-only normalization'}
     [IO.File]::WriteAllBytes($from,$lfBytes)
     if((Get-FileHash $from -Algorithm SHA256).Hash -ne $expectedHostSha){throw 'LF engine host write does not match frozen source'}
     Write-Host 'Verified exact LF original engine host; continuing to UX overlay'
   }
  }
  if(-not(Test-Path $from) -or -not(Test-Path $to)){throw "Overlay absent: $path"}
  $before=(Get-FileHash $to -Algorithm SHA256).Hash
  Copy-Item $from $to -Force
  $hashes+=@{path=$path;baseline=$before;candidate=(Get-FileHash $to -Algorithm SHA256).Hash}
 }
 Install-UxOverlay
 # Exact archived UX19 first; then ONLY the reviewed PR179 additive lazy host.
 # World is NOT disabled: WEBGL runtime URL opt-in controls deferred entry.
 if($env:ELDORIA_P0_LAZY_WORLD_BUILD -eq '1'){
  if($env:ELDORIA_P0_CITY_ONLY_BUILD -eq '1'){throw 'Lazy world and city-only cannot share one build'}
  $hostRel='Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs'
  $hostTo=Join-Path $Source $hostRel
  $ux=Join-Path $Tools 'tools/eldoria-private-beta/native/ux/ValoriaIntegratedWorld.cs.txt'
  $candidateHost=Join-Path $Tools $hostRel
  if(-not(Test-Path $candidateHost -PathType Leaf) -or
     (Get-FileHash $hostTo -Algorithm SHA256).Hash -ne (Get-FileHash $ux -Algorithm SHA256).Hash){
   throw 'Frozen 19-UX must be completely staged before additive lazy host'
  }
  $sourceText=Get-Content $candidateHost -Raw
  foreach($name in 'P0LazyWorldProbe','EnterWorldAfterDeferredBuild','RoyalSection.Mailbox'){
   if(-not $sourceText.Contains($name)){throw "Lazy full-world host missing: $name"}
  }
  Copy-Item $candidateHost $hostTo -Force
  $lazyHash=(Get-FileHash $candidateHost -Algorithm SHA256).Hash
  if((Get-FileHash $hostTo -Algorithm SHA256).Hash -ne $lazyHash){throw 'Lazy host copy SHA mismatch'}
  $hostRows=@($hashes|Where-Object {$_.path -eq $hostRel})
  if($hostRows.Count -ne 1){throw 'Multiple or absent lazy-world hosts'}
  $hostRows[0].candidate=$lazyHash
  $hostRows[0].mode='FULL_REGION1_LAZY_OPTIN_OVER_EXACT_FROZEN_BETA10_UX19'
  Write-Host 'PR179_FULL_REGION1_LAZY_HOST_COMPOSED_WITH_FROZEN_UX19'
 }
 # P0 composition uses exact 662-line archived beta10 UX from the 19-file
 # proof. Newer dev UX adds unrelated features and must NOT enter this A/B.
 if($env:ELDORIA_P0_CITY_ONLY_BUILD -eq '1'){
  $hostRel='Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs'
  $hostTo=Join-Path $Source $hostRel
  $ux=Join-Path $Tools 'tools/eldoria-private-beta/native/ux/ValoriaIntegratedWorld.cs.txt'
  $composeTool=Join-Path $Tools 'tools/eldoria-private-beta/native/iphone-hotfix/compose_city_only.ps1'
  $composed=Join-Path $Evidence 'p0-city-only-composed-host.cs.txt'
  if(-not(Test-Path $composeTool)){throw 'Missing P0 compositor'}
  if((Get-FileHash $hostTo -Algorithm SHA256).Hash -ne (Get-FileHash $ux -Algorithm SHA256).Hash){throw 'P0 must compose after exact frozen UX stage'}
  & $composeTool -UxSource $ux -OutputPath $composed
  if(-not(Test-Path $composed)){throw 'Exact P0 composition missing'}
  $env:ELDORIA_P0_COMPOSED_FILE=$composed
  $uxHash=(Get-FileHash $ux -Algorithm SHA256).Hash
  Copy-Item $composed $hostTo -Force
  $composedHash=(Get-FileHash $composed -Algorithm SHA256).Hash
  if((Get-FileHash $hostTo -Algorithm SHA256).Hash -ne $composedHash){throw 'P0 staged composition hash mismatch'}
  $hostRows=@($hashes|Where-Object {$_.path -eq $hostRel})
  if($hostRows.Count -ne 1){throw 'P0 source overlay must contain one engine host'}
  $hostRows[0].candidate=$composedHash
  $hostRows[0].mode='OPT_IN_CITY_ONLY_OVER_EXACT_FROZEN_BETA10_UX'
 }
 [IO.File]::WriteAllText((Join-Path $Evidence 'source-overlay.json'),(@{base=$baseline;overlay_commit=$env:GITHUB_SHA;files=$hashes}|ConvertTo-Json -Depth 8),$utf8)
 # Prepare static collision structures in the build, preserving exact meshes.
 # Enable before ALL candidate certificates; warm runner restores settings.
 $env:ELDORIA_COLLISION_BAKE_OUT=$Evidence
 Invoke-Native 'EldoriaBetaCollisionBake.Enable' (Join-Path $Evidence 'collision-bake-setting.log') 'Win64'
 $collisionSetting=Get-Content (Join-Path $Evidence 'collision-bake-setting.json') -Raw|ConvertFrom-Json
 if(-not $collisionSetting.after){throw 'Static collision prebake setting absent'}
 Run-Probe 'bake'
 $queryHash=(Get-FileHash $queryAsset -Algorithm SHA256).Hash
 # A/B the exact same tape, collider validation, imported assets and saves.
 # Only the route-buffer implementation differs; no art/profile changes.
 $routePath='Unity/Assets/Eldoria/World4XCanon/RegionOneTerrainRoutes.cs'
 Copy-Item (Join-Path $Tools 'tools/eldoria-private-beta/native/TerrainRoutesWithoutBufferReuse.cs.txt') (Join-Path $Source $routePath) -Force
 Run-Probe 'route-baseline'
 Copy-Item (Join-Path $Tools $routePath) (Join-Path $Source $routePath) -Force
 Run-Probe 'staged'
 $routeComparison=@()
 foreach($pass in 0,1){
  $old=Get-Content (Join-Path $Evidence "route-baseline/startup-$pass.json") -Raw|ConvertFrom-Json
  $new=Get-Content (Join-Path $Evidence "staged/startup-$pass.json") -Raw|ConvertFrom-Json
  foreach($key in 'geometryHash','queryHash','topologyHash','populationRouteHash','verifiedSites','validatedCities'){
   if(-not $new.$key -or $new.$key -ne $old.$key){throw "Route-buffer behavior mismatch: $key"}
  }
  $routeComparison+=@{pass=$pass;parity=$true;before=$old;after=$new;managedPeakDelta=[long]$new.peakManaged-[long]$old.peakManaged}
 }
 [IO.File]::WriteAllText((Join-Path $Evidence 'route-buffer-comparison.json'),(@{status='EXACT_POPULATION_AND_ROUTE_PARITY';passes=$routeComparison}|ConvertTo-Json -Depth 12),$utf8)
 if((Get-FileHash $queryAsset -Algorithm SHA256).Hash -ne $queryHash){throw 'Terrain tape changed after validation'}
 $comparison=@()
 foreach($pass in 0,1){
  $b=Get-Content (Join-Path $Evidence "baseline/startup-$pass.json") -Raw|ConvertFrom-Json
  $s=Get-Content (Join-Path $Evidence "staged/startup-$pass.json") -Raw|ConvertFrom-Json
  foreach($key in 'geometryHash','queryHash','topologyHash','verifiedSites','validatedCities'){
   if($b.$key -ne $s.$key){throw "Parity mismatch pass=$pass field=$key"}
  }
  if(-not $s.ready -or -not $s.worldComplete -or -not $s.canonicalSavesUnchanged -or $s.retainedVerificationBuckets -ne 0 -or $s.guardedFrames -lt 2){throw "Staged contract failed pass=$pass"}
  $comparison+=@{pass=$pass;parity=$true;baseline=$b;staged=$s;managedPeakDelta=[long]$s.peakManaged-[long]$b.peakManaged;allocatedPeakDelta=[long]$s.peakAllocated-[long]$b.peakAllocated;reservedPeakDelta=[long]$s.peakReserved-[long]$b.peakReserved}
 }
 [IO.File]::WriteAllText((Join-Path $Evidence 'comparison.json'),(@{status='NATIVE_PARITY_PASS_NOT_MOBILE_CERTIFICATION';passes=$comparison}|ConvertTo-Json -Depth 12),$utf8)
 # The existing canonical journey gate must run anew on the exact four-file overlay.
 # Remove old certificates in this disposable checkout, never in the shared project.
 foreach($name in 'integration-result.json','integration-input-signature.txt','integration-input-manifest.txt','integration-failure.txt','web-build.json'){
  $file=Join-Path $proof $name;if(Test-Path $file){Remove-Item $file}
 }
 $env:ELDORIA_VALIDATE_TERRAIN_RAYCAST='0'
 $env:ELDORIA_BAKE_TERRAIN_QUERIES='0'
 New-Item -ItemType Directory -Force $proof|Out-Null
 # Exercise focused UI before the long journey; both use separate GUID probe saves.
 $env:ELDORIA_UX_OUT=Join-Path $Evidence 'pretest-ux'
 Invoke-Native 'EldoriaPretestUXProbe.Run' (Join-Path $Evidence 'pretest-ux.log') 'Win64'
 if(-not(Test-Path (Join-Path $env:ELDORIA_UX_OUT 'complete.txt'))){throw 'Pretester UX native proof incomplete'}
 # Keep the canonical journey's editor-input footprint identical to prior proofs.
 foreach($name in 'PretestUXProbe.cs','PretestUXProbe.cs.meta'){
  $file=Join-Path $probe ('Editor/'+$name);if(Test-Path $file){Remove-Item $file}
 }
 Invoke-Native 'Eldoria.ReferenceClean.Editor.ValoriaIntegrationBuild.ValidateCurrent' (Join-Path $Evidence 'journey.log') 'Win64'
 $native=Get-Content (Join-Path $proof 'integration-result.json') -Raw|ConvertFrom-Json
 if($native.status -ne 'INTEGRATION_PASS_ART_REVIEW_REQUIRED'){throw 'Exact-source journey certification failed'}
 Copy-Item $proof (Join-Path $Evidence 'native-journey') -Recurse
 # Probe is Editor-only, outside Assets/Eldoria, and is not shipped in the player.
 if((Get-FileHash $queryAsset -Algorithm SHA256).Hash -ne $queryHash){throw 'Terrain tape changed after journey'}
 Invoke-Native 'Eldoria.ReferenceClean.Editor.ValoriaIntegratedWebBuild.Build' (Join-Path $Evidence 'web-build.log') 'WebGL'
 $web=Get-Content (Join-Path $proof 'web-build.json') -Raw|ConvertFrom-Json
 if($web.status -ne 'WEB_COMPILED_CURRENT_INTEGRATED_SCENE_NOT_PHYSICAL_IPHONE_TESTED'){throw 'WebGL build certificate failed'}
 Copy-Item (Join-Path $proof 'web-*') $Evidence
 if(-not(Test-Path (Join-Path $project 'Builds/ValoriaWebPlaytest/index.html'))){throw 'Built WebGL index absent'}
 [IO.File]::WriteAllText((Join-Path $Evidence 'complete.txt'),'Native parity and canonical journey passed; fresh WebGL built. Browser and physical iPhone are NOT certified.',$utf8)
 Set-NativePhase 'Native parity, journey and WebGL completed; mobile unverified' 'success'
} catch {
 [IO.File]::WriteAllText((Join-Path $Evidence 'native-phase-failure.txt'),($_|Out-String),$utf8)
 Set-NativePhase 'Native phase failed; inspect evidence; no mobile certification' 'failure'
 throw
} finally {
 # Only our disposable checkout is touched. Preserve files if an Editor is still using them.
 if(-not(Get-Process Unity -ErrorAction SilentlyContinue)){
  if(Test-Path $queryAsset){
   $hash=(Get-FileHash $queryAsset -Algorithm SHA256).Hash
   if($queryHash -and $queryHash -ne $hash){throw 'Unexpected terrain tape mutation; preserve project'}
   Copy-Item $queryAsset (Join-Path $Evidence 'terrain-query-cache.bytes')
   [IO.File]::WriteAllText((Join-Path $Evidence 'terrain-query-cache.json'),(@{source=$env:GITHUB_SHA;sha256=$hash;bytes=(Get-Item $queryAsset).Length;resource='World4X/Blender/RuntimeTerrainQueries';input_audit=$env:ELDORIA_BASELINE_INPUT_AUDIT}|ConvertTo-Json),$utf8)
   Remove-Item $queryAsset -Force
  }
  if(Test-Path ($queryAsset+'.meta')){Remove-Item ($queryAsset+'.meta') -Force}
  if(Test-Path $probe){Remove-Item $probe -Recurse -Force}
  if(Test-Path ($probe+'.meta')){Remove-Item ($probe+'.meta') -Force}
 }
}

