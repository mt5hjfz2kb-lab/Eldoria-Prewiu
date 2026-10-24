# Reuse the existing UI certification's native probe, framing assertions and
# reversible Search workaround (ui-progression-cert.yml at 611ca8df70431f58c5db7a75611670a8228a8a2c).
# This owned World4X entry point adds the full GPU suite; it never edits UI workflow/product files.
$ErrorActionPreference='Stop'
$sourceRoot=Join-Path $env:GITHUB_WORKSPACE 'world4x-source'
Set-Location $sourceRoot
$env:VALORIA_GAMEPLAY_PROBE='1'
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native process already active before general certification'}
$proof=Join-Path $env:RUNNER_TEMP "region1-general-proof-$env:GITHUB_RUN_ID"
if(Test-Path $proof){throw 'Do not overwrite same-run general evidence'}
New-Item -ItemType Directory -Force $proof|Out-Null
$nativeStarted=(Get-Date).ToUniversalTime()
try {
# Reuse the proven valoria-reference-clean native-editor recovery:
# UnityEditor.Search.SearchDatabase.IndexationOnStartup threw
# ArgumentOutOfRangeException on the exact V3 candidate in run 39564761151.
# This alters only temporary editor preferences/cache, never assets,
# gameplay, scene bytes, source, progression, or owner saves.
$project=Join-Path $sourceRoot 'Unity'
$settings=Join-Path $project 'UserSettings/Search.index'
$backup=Join-Path $env:RUNNER_TEMP "eldoria-world-general-search-original-$env:GITHUB_RUN_ID.json"
$created=Join-Path $env:RUNNER_TEMP "eldoria-world-general-search-created-$env:GITHUB_RUN_ID.flag"
if((Test-Path $backup) -or (Test-Path $created)){
  throw 'Existing current-run Search-index backup must be reconciled before rerun'
}
if(Test-Path $settings){
  Copy-Item -LiteralPath $settings -Destination $backup
  try{$config=Get-Content -LiteralPath $settings -Raw -Encoding UTF8|ConvertFrom-Json -ErrorAction Stop}
  catch{throw 'Original Unity Editor Search.index malformed; preserve original and refuse blind replacement'}
  if($config.PSObject.Properties['roots']){
    $config.roots=@()
    $config|ConvertTo-Json -Depth 32|Set-Content $settings -Encoding UTF8
  }
}else{
  New-Item -ItemType Directory -Force (Split-Path $settings)|Out-Null
  $config=[ordered]@{
    name='Assets';type='asset';roots=@();includes=@();
    excludes=@('Assets/Temp/','Assets/External/','Temp/')
    options=@{disabled=$false;types=$true;properties=$true;extended=$true;dependencies=$true}
    baseScore=999
  }
  $config|ConvertTo-Json -Depth 16|Set-Content $settings -Encoding UTF8
  'created only for V3 CI'|Set-Content $created -Encoding ASCII
}
$current=Get-Content $settings -Raw -Encoding UTF8|ConvertFrom-Json
if($current.PSObject.Properties['roots'] -and @($current.roots).Count -ne 0){
  throw 'Editor-only search index roots must be empty during V3 validation'
}
$cache=Join-Path $project 'Library/Search'
$cacheBackup=Join-Path $env:RUNNER_TEMP "eldoria-world-general-search-cache-$env:GITHUB_RUN_ID"
if(Test-Path $cacheBackup){throw 'Existing same-run Search cache backup must not be overwritten'}
if(Test-Path $cache){
  Move-Item -LiteralPath $cache -Destination $cacheBackup -ErrorAction Stop
  Write-Host "ELDORIA_V3_SEARCH_CACHE_ISOLATED $cacheBackup"
}
Write-Host "ELDORIA_V3_PROVEN_SEARCH_INDEX_RECOVERY_APPLIED"
# The existing ValoriaIntegrationBuild.ValidateCurrent test plays the
# real scene with a probe-only disposable save and checks real pointer
# raycasts, both orientations, story, construction, World and I-II.
# Do not modify any art, prefab, saved game or canonical publication.
# Preserve the ORIGINAL V2 native framing metrics, without
# changing the checked-in baseline or taking credit for old runs.
$e=Join-Path $sourceRoot 'docs/evidence/valoria-reference-clean'
$baselineFile=Join-Path $env:RUNNER_TEMP "Region1-General-Framing-Baseline-$env:GITHUB_RUN_ID.json"
# On the shared Windows runner, a previous task can leave a sparse
# checkout that omits docs/evidence even though the blob is tracked.
# Read the original baseline directly from THIS PR merge commit's
# Git tree. Do not overwrite another workstream's evidence on disk.
$blobRef="$($env:GITHUB_SHA):docs/evidence/valoria-reference-clean/integration-result.json"
$baselineLines=@(& git show $blobRef)
if($LASTEXITCODE -ne 0 -or $baselineLines.Count -lt 8){
  throw 'Original native V2 baseline blob missing from exact Git commit'
}
$baselineText=$baselineLines -join "`n"
try{$original=$baselineText|ConvertFrom-Json -ErrorAction Stop}
catch{throw 'Original exact-Git V2 baseline JSON could not be parsed'}
if($original.status -ne 'INTEGRATION_PASS_ART_REVIEW_REQUIRED' -or @($original.framing).Count -lt 10){
  throw 'Original exact-Git V2 baseline is not an acceptable full I-II capture'
}
[IO.File]::WriteAllText($baselineFile,$baselineText,([System.Text.UTF8Encoding]::new($false)))
Write-Host "ELDORIA_V2_SOURCE_BASELINE_FROM_EXACT_GIT_TREE $($env:GITHUB_SHA)"
$log=Join-Path $env:RUNNER_TEMP 'eldoria-region1-general-integration.log'
Remove-Item $log -Force -ErrorAction SilentlyContinue
$args=@('-batchmode','-force-d3d11','-projectPath','Unity',
  '-executeMethod','Eldoria.ReferenceClean.Editor.ValoriaIntegrationBuild.ValidateCurrent',
  '-logFile',$log)
$process=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -WorkingDirectory $sourceRoot -PassThru
# A known editor-only SearchDatabase crash previously left this
# exact test running until its 30-minute timeout (run 39564761151).
# Detect it early AFTER the reversible Search-index recovery, so a
# genuine remaining infrastructure problem does not hoard the PC.
$deadline=(Get-Date).AddMinutes(30)
while(-not $process.WaitForExit(10000)){
  if(Test-Path $log){
    $tail=Get-Content -LiteralPath $log -Tail 700 -ErrorAction SilentlyContinue|Out-String
    if($tail.Contains('ArgumentOutOfRangeException') -and
       $tail.Contains('UnityEditor.Search.SearchDatabase')){
      & taskkill.exe /PID $process.Id /T /F | Out-Null
      throw 'Unity SearchDatabase indexing fault persisted despite the exact reversible workaround'
    }
  }
  if((Get-Date) -ge $deadline){
    if(Test-Path $log){Get-Content $log -Tail 180}
    & taskkill.exe /PID $process.Id /T /F | Out-Null
    throw 'Native I-II viewport/capture probe exceeded 30 minutes'
  }
}
if($process.ExitCode -ne 0){
  if(Test-Path $log){Get-Content $log -Tail 180}
  throw "Native I-II visual probe failed: $($process.ExitCode)"
}
$e=Join-Path $sourceRoot 'docs/evidence/valoria-reference-clean'
foreach($file in @(
  'integration-story-aldric.png',
  'integration-city-16x9.png',
  'integration-construction-landscape.png',
  'integration-construction-portrait.png',
  'integration-world-16x9.png',
  'integration-world-portrait.png',
  'integration-bastion-upgrade.png',
  'integration-training-portrait.png',
  'integration-march-landscape.png',
  'integration-march-portrait.png',
  'integration-chapter2-complete.png',
  'integration-result.json'
)){
  if(-not(Test-Path (Join-Path $e $file))){throw "Missing real Unity evidence: $file"}
}
$result=Get-Content (Join-Path $e 'integration-result.json') -Raw|ConvertFrom-Json
if($result.status -ne 'INTEGRATION_PASS_ART_REVIEW_REQUIRED'){
  throw "Existing I-II probe not accepted: $($result.status) $($result.error)"
}
# Measure actually-free camera area in BOTH mobile orientations,
# using the existing certified native framing rectangles, and
# compare like-for-like against V2 for each same gameplay state.
$before=Get-Content $baselineFile -Raw|ConvertFrom-Json
$v3Targets=@(
  'integration-construction-landscape.png',
  'integration-construction-portrait.png',
  'integration-bastion-upgrade.png',
  # Training deliberately leaves the city for an opaque management
  # destination. Do NOT test the city viewport behind that screen:
  # the correct acceptance gate is native PlayMode full-screen,
  # topmost raycast, camera restoration and screenshot presence.
  'integration-art-forest-valoria-landscape.png',
  'integration-art-forest-valoria-portrait.png'
)
foreach($capture in $v3Targets){
  $old=@($before.framing | Where-Object {$_.capture -eq $capture})
  $new=@($result.framing | Where-Object {$_.capture -eq $capture})
  if($old.Count -ne 1 -or $new.Count -ne 1 -or -not $new[0].pass){
    throw "HUD V3 missing successful like-for-like native camera framing: $capture"
  }
  $oldRect=@($old[0].visible)
  $newRect=@($new[0].visible)
  if($oldRect.Count -ne 4 -or $newRect.Count -ne 4){
    throw "Invalid native viewport coordinates: $capture"
  }
  # Math.Max(0, Double) resolves the Integer overload under
  # Windows PowerShell 5.1: 0.46 became 0 and 0.78 became 1.
  # The old gate falsely reported V2=0% / V3=100% for EVERY frame.
  # Preserve fractional coordinates and reject non-finite geometry.
  $coords=@($oldRect)+@($newRect)
  foreach($v in $coords){
    $number=[double]$v
    if([double]::IsNaN($number) -or [double]::IsInfinity($number) -or
       $number -lt 0.0 -or $number -gt 1.0){
      throw "Invalid native normalized framing coordinate: $v ($capture)"
    }
  }
  $oldW=[double]$oldRect[2]-[double]$oldRect[0]
  $oldH=[double]$oldRect[3]-[double]$oldRect[1]
  $newW=[double]$newRect[2]-[double]$newRect[0]
  $newH=[double]$newRect[3]-[double]$newRect[1]
  if($oldW -le 0.0 -or $oldH -le 0.0 -or
     $newW -le 0.0 -or $newH -le 0.0){
    throw "Degenerate or inverted real Unity viewport: $capture"
  }
  [double]$oldArea=$oldW*$oldH
  [double]$newArea=$newW*$newH
  [double]$gain=$newArea-$oldArea
  Write-Host ("ELDORIA_V3_FREE_VIEWPORT capture={0} V2={1:N1}% V3={2:N1}% gain={3:N1}pp" -f $capture,($oldArea*100),($newArea*100),($gain*100))
  # An eight-percentage-point increase is meaningful; leave V2
  # untouched if the contextual cards merely moved the occlusion.
  if($gain -lt .08){
    throw "V3 did not reclaim at least 8 percentage points of real free scene: $capture"
  }
}
# Independent destinations have no visible city viewport by design;
# verify actual native screenshots instead of pretending camera space
# is visible behind their opaque artwork.
foreach($capture in @('integration-training.png','integration-training-portrait.png','integration-march-landscape.png','integration-march-portrait.png')){
  $path=Join-Path $e $capture
  if(-not(Test-Path $path)){throw "Missing genuine full-screen management capture: $capture"}
  if((Get-Item $path).Length -lt 4096){throw "Management capture too small: $capture"}
  if(@($result.framing | Where-Object {$_.capture -eq $capture}).Count -ne 0){
    throw "Management screen was wrongly certified as exposed-city camera: $capture"
  }
  Write-Host "ELDORIA_V3_NATIVE_MANAGEMENT_SCREEN=$capture"
}
Write-Host "ELDORIA_V3_REAL_NATIVE_I_II_PROBE=$($result.status)"
Write-Host "ELDORIA_V3_CHECKS=$(@($result.checks).Count) CAPTURES=$(@(Get-ChildItem $e -Filter 'integration*.png').Count)"
Write-Host "Native visual evidence still requires owner inspection before merge/publish."
$xml = Join-Path $env:RUNNER_TEMP 'eldoria-region1-editmode.xml'
$log = Join-Path $env:RUNNER_TEMP 'eldoria-region1-editmode.log'
Remove-Item $xml,$log -Force -ErrorAction SilentlyContinue
$project=Join-Path $sourceRoot 'Unity'
if(-not(Test-Path $project)){throw 'Unity source project is missing from current main checkout'}
# Unity 6 on this Windows runner misreads mixed C:\\.../Unity as a
# relative path and prepends the workspace again (run 39174538409).
# Use a pure relative project name with explicit working directory.
$args = @("-batchmode","-nographics","-projectPath","Unity","-runTests","-testPlatform","EditMode","-testResults",$xml,"-logFile",$log)
$process = Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -WorkingDirectory $sourceRoot -PassThru
if (-not $process.WaitForExit(600000)) {
  & taskkill.exe /PID $process.Id /T /F | Out-Null
  if (Test-Path $log) { Get-Content $log -Tail 240 }
  throw "Unity EditMode timed out after 10 minutes."
}
if ($process.ExitCode -ne 0) { if (Test-Path $log) { Get-Content $log -Tail 200 }; throw "Unity EditMode failed with exit code $($process.ExitCode)." }
[xml]$results = Get-Content $xml
if ($results.'test-run'.result -ne "Passed") { throw "EditMode tests did not pass: $($results.'test-run'.result)" }
[xml]$edit=Get-Content (Join-Path $env:RUNNER_TEMP 'eldoria-region1-editmode.xml') -Raw
$editCases=@($edit.SelectNodes('//test-case'))
if($editCases.Count -lt 26 -or @($editCases|Where-Object {$_.result -ne 'Passed'}).Count){throw 'All EditMode cases must pass without omissions'}
[xml]$play=Get-Content (Join-Path $env:RUNNER_TEMP 'eldoria-region1-canon-tests.xml') -Raw
$playCases=@($play.SelectNodes('//test-case'))
if($playCases.Count -lt 57 -or @($playCases|Where-Object {$_.result -ne 'Passed'}).Count){throw 'All GPU PlayMode cases including genuine capture must pass'}
$native=Get-Content (Join-Path $e 'integration-result.json') -Raw|ConvertFrom-Json
if($native.source -ne $env:GITHUB_SHA -or $native.status -ne 'INTEGRATION_PASS_ART_REVIEW_REQUIRED' -or @($native.checks).Count -lt 44){throw 'General probe identity or coverage incomplete'}
$fresh=@(Get-ChildItem $e -Filter 'integration-*.png'|Where-Object {$_.LastWriteTimeUtc -ge $nativeStarted})
if($fresh.Count -lt 34){throw "Expected at least 34 freshly rendered general captures; got $($fresh.Count)"}
$manifest=@{source=$env:GITHUB_SHA;run=$env:GITHUB_RUN_ID;editmode_passed=$editCases.Count;gpu_playmode_passed=$playCases.Count;probe_checks=@($native.checks).Count;fresh_captures=$fresh.Count;art_approved=$false;physical_mobile_tested=$false;images=@()}
foreach($png in $fresh){Copy-Item $png.FullName (Join-Path $proof $png.Name);$manifest.images+=@{file=$png.Name;sha256=(Get-FileHash $png.FullName -Algorithm SHA256).Hash.ToLower();bytes=$png.Length}}
Copy-Item (Join-Path $e 'integration-result.json') (Join-Path $proof 'integration-result.json')
Copy-Item (Join-Path $env:RUNNER_TEMP 'eldoria-region1-editmode.xml') (Join-Path $proof 'editmode.xml')
Copy-Item (Join-Path $env:RUNNER_TEMP 'eldoria-region1-canon-tests.xml') (Join-Path $proof 'playmode.xml')
$manifest|ConvertTo-Json -Depth 8|Set-Content (Join-Path $proof 'general-proof.json') -Encoding UTF8
Write-Host "REGION1_FULL_NATIVE_CERT edit=$($editCases.Count) play=$($playCases.Count) checks=$(@($native.checks).Count) fresh_png=$($fresh.Count)"
} finally {
$project=Join-Path $sourceRoot 'Unity'
$settings=Join-Path $project 'UserSettings/Search.index'
$backup=Join-Path $env:RUNNER_TEMP "eldoria-world-general-search-original-$env:GITHUB_RUN_ID.json"
$created=Join-Path $env:RUNNER_TEMP "eldoria-world-general-search-created-$env:GITHUB_RUN_ID.flag"
if(Test-Path $backup){
  Copy-Item -LiteralPath $backup -Destination $settings -Force
  Write-Host 'Original Unity Search.index restored without rewriting source'
}elseif(Test-Path $created){
  if(Test-Path $settings){Remove-Item -LiteralPath $settings -Force}
  Write-Host 'Temporary Unity Search.index removed; original did not exist'
}
$cache=Join-Path $project 'Library/Search'
$cacheBackup=Join-Path $env:RUNNER_TEMP "eldoria-world-general-search-cache-$env:GITHUB_RUN_ID"
if(Test-Path $cacheBackup){
  if(Test-Path $cache){
    $post=Join-Path $env:RUNNER_TEMP "eldoria-world-general-search-new-$env:GITHUB_RUN_ID"
    if(Test-Path $post){throw 'Refuse overwriting an existing post-validation Search cache'}
    Move-Item -LiteralPath $cache -Destination $post -ErrorAction Stop
  }
  Move-Item -LiteralPath $cacheBackup -Destination $cache -ErrorAction Stop
  Write-Host 'Existing Editor Search cache restored; temporary new index isolated'
}
Write-Host 'ELDORIA_V3_EDITOR_SEARCH_RECOVERY_RESTORED'
}
