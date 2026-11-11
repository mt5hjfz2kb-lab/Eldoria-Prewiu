param(
 [Parameter(Mandatory=$true)][string]$Source,
 [Parameter(Mandatory=$true)][string]$OverlaySource,
 [Parameter(Mandatory=$true)][string]$Evidence,
 [Parameter(Mandatory=$true)][string]$ExpectedHead
)
$ErrorActionPreference='Stop'
$Source=(Resolve-Path $Source).Path
$OverlaySource=(Resolve-Path $OverlaySource).Path
$Evidence=[IO.Path]::GetFullPath($Evidence)
$head=(git -C $OverlaySource rev-parse HEAD).Trim()
if($LASTEXITCODE -ne 0 -or $head -ne $ExpectedHead){throw 'Lazy-world candidate HEAD mismatch'}
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Shared native editor is busy'}
if(-not(Test-Path $env:UNITY_EDITOR_PATH -PathType Leaf)){throw 'Unity Editor not available'}
if(-not(Test-Path (Join-Path $Source 'Unity/Assets') -PathType Container)){throw 'Hot Unity project unavailable'}
if(Test-Path $Evidence){throw 'Refuse to overwrite existing native QA evidence'}

# Reuse the six already-audited engine input paths from the PR179 native pipeline.
# Test assets are an isolated Editor-only assembly. No player saves or scenes are edited.
$overlays=@(
 'Unity/Assets/Eldoria/World4XCanon/RegionOneBlenderLandscape.cs',
 'Unity/Assets/Eldoria/World4XCanon/RegionOneCanonicalLandscape.cs',
 'Unity/Assets/Eldoria/World4XCanon/RegionOneResourcePlacement.cs',
 'Unity/Assets/Eldoria/World4XCanon/RegionOneTerrainRoutes.cs',
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs',
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaIntegrationBuild.cs',
 'Unity/Assets/Eldoria/BetaMemoryProbe.meta',
 'Unity/Assets/Eldoria/BetaMemoryProbe/Tests.meta',
 'Unity/Assets/Eldoria/BetaMemoryProbe/Tests/PlayMode.meta',
 'Unity/Assets/Eldoria/BetaMemoryProbe/Tests/PlayMode/Eldoria.BetaMemory.Tests.PlayMode.asmdef',
 'Unity/Assets/Eldoria/BetaMemoryProbe/Tests/PlayMode/Eldoria.BetaMemory.Tests.PlayMode.asmdef.meta',
 'Unity/Assets/Eldoria/BetaMemoryProbe/Tests/PlayMode/LazyRegionStartupTests.cs',
 'Unity/Assets/Eldoria/BetaMemoryProbe/Tests/PlayMode/LazyRegionStartupTests.cs.meta'
)
$trackedBefore=@(git -C $Source status --porcelain -- Unity)
if($LASTEXITCODE -ne 0){throw 'Unable to audit original Unity repository state'}
$ownedDirty=@(git -C $Source status --porcelain -- $overlays)
if($LASTEXITCODE -ne 0 -or $ownedDirty.Count){throw "Foreign pending changes in owned paths; no mutation: $($ownedDirty -join ',')"}
foreach($rel in $overlays){
 if(-not(Test-Path (Join-Path $OverlaySource $rel) -PathType Leaf)){throw "Candidate source missing: $rel"}
}
$preserve=@($overlays)+@(
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/GeneratedLighting/BakedSceneURP.asset',
 'Unity/Assets/UniversalRenderPipelineGlobalSettings.asset'
)
$projectSettings=Join-Path $Source 'Unity/ProjectSettings'
if(Test-Path $projectSettings){
 $preserve+=@(Get-ChildItem $projectSettings -File -Recurse | ForEach-Object {
  $_.FullName.Substring($Source.Length+1).Replace('\','/')
 })
}
$preserve=@($preserve|Sort-Object -Unique)
$backup=Join-Path $env:GITHUB_WORKSPACE ("lazy-region-preserved-"+$env:GITHUB_RUN_ID+"-"+$env:GITHUB_RUN_ATTEMPT)
if(Test-Path $backup){throw 'Native backup slot already occupied'}
$records=@()
foreach($rel in $preserve){
 $actual=Join-Path $Source $rel
 $exists=Test-Path $actual -PathType Leaf
 $saved=Join-Path $backup $rel
 if($exists){
  New-Item -ItemType Directory -Force (Split-Path $saved)|Out-Null
  Copy-Item -LiteralPath $actual -Destination $saved
 }
 $records+=@{path=$rel;existed=$exists;sha256=$(if($exists){(Get-FileHash $actual -Algorithm SHA256).Hash}else{$null})}
}
New-Item -ItemType Directory -Force $Evidence|Out-Null
$log=Join-Path $Evidence 'unity-lazy-region-playmode.log'
$xml=Join-Path $Evidence 'unity-lazy-region-playmode.xml'
$failure=$null
$process=$null
$verified=$false
try{
 foreach($rel in $overlays){
  $to=Join-Path $Source $rel
  New-Item -ItemType Directory -Force (Split-Path $to)|Out-Null
  Copy-Item -LiteralPath (Join-Path $OverlaySource $rel) -Destination $to -Force
  if((Get-FileHash $to -Algorithm SHA256).Hash -ne (Get-FileHash (Join-Path $OverlaySource $rel) -Algorithm SHA256).Hash){
   throw "Candidate source changed in transit: $rel"
  }
 }
 $env:VALORIA_GAMEPLAY_PROBE='1'
 $args=@('-batchmode','-force-d3d11','-projectPath',('"' + (Join-Path $Source 'Unity') + '"'),
  '-runTests','-testPlatform','PlayMode',
  '-testFilter','Eldoria.BetaMemory.Tests.LazyRegionStartupTests',
  '-testResults',('"' + $xml + '"'),'-logFile',('"' + $log + '"'))
 $process=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -PassThru
 $end=(Get-Date).AddMinutes(55)
 while(-not $process.WaitForExit(15000)){
  if((Get-Date) -gt $end){
   Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
   $null=$process.WaitForExit(15000)
   throw 'Own Unity lazy-world test exceeded 55 minutes; owner process stopped'
  }
 }
 if($process.ExitCode -ne 0){throw "Unity PlayMode failed with exit code $($process.ExitCode)"}
 if(-not(Test-Path $xml -PathType Leaf)){throw 'Unity did not emit a native PlayMode result XML'}
 [xml]$results=Get-Content $xml -Raw
 $root=$results.DocumentElement
 if($root.Name -ne 'test-run'){throw 'Not a Unity NUnit result'}
 $total=[int]$root.GetAttribute('total')
 $failed=[int]$root.GetAttribute('failed')
 if($total -lt 1 -or $failed -ne 0 -or $root.GetAttribute('result') -ne 'Passed'){
  throw "Unity lazy-world PlayMode not certified: tests=$total failed=$failed"
 }
 if(-not((Get-Content $xml -Raw).Contains('RealWorldStartsOnFirstEligibleEntryWithoutSecondGatewayOrNewHUD'))){
  throw 'The actual lazy-world fixture was not executed'
 }
 $verified=$true
}catch{
 $failure=$_.Exception.Message
}finally{
 if($process -and -not $process.HasExited){
  $failure='Unity still running: project preserved for manual recovery, no unsafe restoration'
 }else{
  $issues=@()
  foreach($record in $records){
   $to=Join-Path $Source $record.path
   if($record.existed){
    $saved=Join-Path $backup $record.path
    try{Copy-Item -LiteralPath $saved -Destination $to -Force}catch{$issues+=$record.path;continue}
    if((Get-FileHash $to -Algorithm SHA256).Hash -ne $record.sha256){$issues+=$record.path}
   }elseif(Test-Path $to -PathType Leaf){
    try{Remove-Item -LiteralPath $to -Force}catch{$issues+=$record.path}
   }
  }
  $trackedAfter=@(git -C $Source status --porcelain -- Unity)
  if($LASTEXITCODE -ne 0 -or (@(Compare-Object $trackedBefore $trackedAfter).Count -gt 0)){
   $issues+='git status changed after test'
  }
  if($issues.Count){$failure="Restoration FAILED: $($issues -join ','); prior=$failure"}
  $restored=$issues.Count -eq 0
  $report=@{
   source_head=$head;unity_tests_passed=$verified;restored=$restored;
   failed_reason=$failure;owned_files=$overlays.Count;original_unity_status_count=$trackedBefore.Count;
   tests_executed='Eldoria.BetaMemory.Tests.LazyRegionStartupTests';
   physical_iphone_verified=$false;testers_authorized=$false
  }
  $report|ConvertTo-Json -Depth 5|Set-Content -Path (Join-Path $Evidence 'lazy-world-native-result.json') -Encoding UTF8
 }
}
if($failure){throw $failure}
if(-not $verified){throw 'No PlayMode certification'}
Write-Host 'ELDORIA_P0_REAL_LAZY_WORLD_PLAYMODE_PASS_AND_PROJECT_RESTORED'
