param([Parameter(Mandatory=$true)][string]$SourceRoot)
$ErrorActionPreference='Stop'
$project=Join-Path $SourceRoot 'Unity'
$log=Join-Path $env:RUNNER_TEMP ('city2d-quality-v2-'+$env:GITHUB_RUN_ID+'.log')
$args=@('-batchmode','-quit','-force-d3d11','-projectPath',$project,
 '-executeMethod','Eldoria.Experimentos.Ciudad2D.Editor.City2DQualityV2Gallery.Capture',
 '-logFile',$log)
$proc=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -PassThru
if(-not $proc.WaitForExit(950000)){
 & taskkill.exe /PID $proc.Id /T /F | Out-Null
 throw 'Cuartel visual quality A/B/C timed out'
}
if($proc.ExitCode -ne 0){
 if(Test-Path -LiteralPath $log){Get-Content -LiteralPath $log -Tail 180}
 throw ('Visual quality V2 Unity invocation failed: '+$proc.ExitCode)
}
$folder=Join-Path $project 'Ciudad2DQualityV2Evidence'
$manifest=Join-Path $folder 'evidence.json'
if(-not(Test-Path -LiteralPath $manifest)){throw 'Real Unity art quality A/B/C proof missing'}
$j=Get-Content -LiteralPath $manifest -Raw | ConvertFrom-Json
if($j.engine -ne 'Unity' -or $j.mode -ne 'Editor_RenderTexture' -or
   @($j.captures).Count -ne 18 -or @($j.modes).Count -ne 3 -or
   -not $j.art_comparison_only -or -not $j.shadow_separate -or
   $j.removed_duplicate_background_props -ne 9 -or
   $j.legacy_original_props -ne 32 -or $j.quieter_props -ne 23 -or
   -not $j.bastion_I_1_to_4_V_5_unchanged -or
   $j.main_modified -or $j.gameplay_modified -or
   $j.playmode_candidate_pointer_qa -or $j.art_approved -or
   $j.physical_iphone_test -ne 'PENDING'){
 throw 'Visual quality V2 evidence truth contract violation'
}
$seen=@{}
foreach($entry in $j.captures){
 if($seen.ContainsKey($entry.file)){throw 'Duplicate Unity visual capture'}
 $seen[$entry.file]=$true
 $path=Join-Path $folder $entry.file
 if(-not(Test-Path -LiteralPath $path) -or (Get-Item $path).Length -lt 8000){
  throw ('Visual quality PNG missing '+$entry.file)
 }
 $hash=(Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash.ToLowerInvariant()
 if($hash -ne $entry.sha256){throw ('Screenshot SHA mismatch '+$entry.file)}
 Add-Type -AssemblyName System.Drawing
 $image=[System.Drawing.Image]::FromFile($path)
 try {
  if($image.Width -ne [int]$entry.width -or $image.Height -ne [int]$entry.height){
   throw ('Non-native screenshot dimensions '+$entry.file)
  }
 } finally {$image.Dispose()}
}
foreach($mode in @('A_32_original','B_23_quiet','C_23_premium')){
 foreach($stage in @(3,5)){
  foreach($resolution in @('390x844','1170x2532')){
   $filename=($mode+'_bastion'+$stage+'_'+$resolution+'_standard.png')
   if(-not $seen.ContainsKey($filename)){throw ('Missing matched A/B/C pair '+$filename)}
  }
 }
 foreach($suffix in @('844x390_standard.png','1170x2532_cuartel_close.png')){
  $filename=$mode+'_bastion5_'+$suffix
  if(-not $seen.ContainsKey($filename)){throw ('Missing landscape or Cuartel detail '+$filename)}
 }
}
Write-Host ('CITY2D_REAL_UNITY_QUALITY_V2_18_SCREENSHOTS_SHA_PASS_COMMIT='+$env:GITHUB_SHA)
Write-Host 'ALL ART CANDIDATES UNAPPROVED, IPHONE REAL NOT TESTED; original City2D controls preserved'
