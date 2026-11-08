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
$baseline='948ffa9837168a0b68dab80be76b66350422b23a'
$overlays=@(
 'Unity/Assets/Eldoria/World4XCanon/RegionOneBlenderLandscape.cs',
 'Unity/Assets/Eldoria/World4XCanon/RegionOneCanonicalLandscape.cs',
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs',
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaIntegrationBuild.cs'
)
New-Item -ItemType Directory -Force $Evidence|Out-Null
if((git -C $Source rev-parse HEAD).Trim() -ne $baseline){throw 'Wrong frozen source'}
if(@(git -C $Source status --porcelain).Count){throw 'Isolated source is not clean'}
if(Test-Path $probe){throw 'Refuse to replace an existing probe'}
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native tools already running'}
if(-not(Test-Path $env:UNITY_EDITOR_PATH)){throw 'Unity editor unavailable'}
$env:VALORIA_GAMEPLAY_PROBE='1'
$proof=Join-Path $Source 'docs/evidence/valoria-reference-clean'
function Invoke-Native([string]$Method,[string]$Log,[string]$Target){
 if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native tools busy before phase'}
 $args=@('-batchmode','-force-d3d11','-buildTarget',$Target,'-projectPath',('"'+$project+'"'),'-executeMethod',$Method,'-logFile',('"'+$Log+'"'))
 $p=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -PassThru
 if(-not $p.WaitForExit(2400000)){throw "Owned Unity process $($p.Id) exceeded phase deadline; leave isolated project intact"}
 if($p.ExitCode -ne 0){throw "Unity phase $Method failed: $($p.ExitCode)"}
}
function Run-Probe([string]$Mode){
 $env:ELDORIA_EXPECT_STAGED=if($Mode -eq 'staged'){'1'}else{'0'}
 $env:ELDORIA_MEMORY_SOURCE=if($Mode -eq 'staged'){$env:GITHUB_SHA}else{$baseline}
 $env:ELDORIA_MEMORY_OUT=Join-Path $Evidence $Mode
 New-Item -ItemType Directory -Force $env:ELDORIA_MEMORY_OUT|Out-Null
 Invoke-Native 'EldoriaBetaStructuralStartupProbe.Run' (Join-Path $env:ELDORIA_MEMORY_OUT 'unity.log') 'Win64'
 if(-not(Test-Path (Join-Path $env:ELDORIA_MEMORY_OUT 'complete.txt'))){throw "Probe $Mode completion absent"}
}
try {
 New-Item -ItemType Directory -Force (Join-Path $probe 'Editor')|Out-Null
 Copy-Item (Join-Path $Tools 'tools/eldoria-private-beta/native/StructuralStartupProbe.cs') (Join-Path $probe 'Editor/StructuralStartupProbe.cs')
 Run-Probe 'baseline'
 $hashes=@()
 foreach($path in $overlays){
  $from=Join-Path $Tools $path;$to=Join-Path $Source $path
  if(-not(Test-Path $from) -or -not(Test-Path $to)){throw "Overlay absent: $path"}
  $before=(Get-FileHash $to -Algorithm SHA256).Hash
  Copy-Item $from $to -Force
  $hashes+=@{path=$path;baseline=$before;candidate=(Get-FileHash $to -Algorithm SHA256).Hash}
 }
 [IO.File]::WriteAllText((Join-Path $Evidence 'source-overlay.json'),(@{base=$baseline;overlay_commit=$env:GITHUB_SHA;files=$hashes}|ConvertTo-Json -Depth 8),$utf8)
 Run-Probe 'staged'
 $comparison=@()
 foreach($pass in 0,1){
  $b=Get-Content (Join-Path $Evidence "baseline/startup-$pass.json") -Raw|ConvertFrom-Json
  $s=Get-Content (Join-Path $Evidence "staged/startup-$pass.json") -Raw|ConvertFrom-Json
  foreach($key in 'geometryHash','queryHash','verifiedSites','validatedCities'){
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
 Invoke-Native 'Eldoria.ReferenceClean.Editor.ValoriaIntegrationBuild.ValidateCurrent' (Join-Path $Evidence 'journey.log') 'Win64'
 $native=Get-Content (Join-Path $proof 'integration-result.json') -Raw|ConvertFrom-Json
 if($native.status -ne 'INTEGRATION_PASS_ART_REVIEW_REQUIRED'){throw 'Exact-source journey certification failed'}
 Copy-Item $proof (Join-Path $Evidence 'native-journey') -Recurse
 # Probe is Editor-only, outside Assets/Eldoria, and is not shipped in the player.
 Invoke-Native 'Eldoria.ReferenceClean.Editor.ValoriaIntegratedWebBuild.Build' (Join-Path $Evidence 'web-build.log') 'WebGL'
 $web=Get-Content (Join-Path $proof 'web-build.json') -Raw|ConvertFrom-Json
 if($web.status -ne 'WEB_COMPILED_CURRENT_INTEGRATED_SCENE_NOT_PHYSICAL_IPHONE_TESTED'){throw 'WebGL build certificate failed'}
 Copy-Item (Join-Path $proof 'web-*') $Evidence
 if(-not(Test-Path (Join-Path $project 'Builds/ValoriaWebPlaytest/index.html'))){throw 'Built WebGL index absent'}
 [IO.File]::WriteAllText((Join-Path $Evidence 'complete.txt'),'Native parity and canonical journey passed; fresh WebGL built. Browser and physical iPhone are NOT certified.',$utf8)
} finally {
 # Only our disposable checkout is touched. Preserve files if an Editor is still using them.
 if(-not(Get-Process Unity -ErrorAction SilentlyContinue)){
  if(Test-Path $probe){Remove-Item $probe -Recurse -Force}
  if(Test-Path ($probe+'.meta')){Remove-Item ($probe+'.meta') -Force}
 }
}
