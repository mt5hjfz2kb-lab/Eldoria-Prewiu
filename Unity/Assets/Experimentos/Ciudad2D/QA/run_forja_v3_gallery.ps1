param([Parameter(Mandatory=$true)][string]$SourceRoot)
$ErrorActionPreference='Stop'
$project=Join-Path $SourceRoot 'Unity'
$log=Join-Path $env:RUNNER_TEMP ('city2d-forja-v3-gallery-'+$env:GITHUB_RUN_ID+'.log')
$args=@('-batchmode','-quit','-force-d3d11','-projectPath',$project,
  '-executeMethod','Eldoria.Experimentos.Ciudad2D.Editor.City2DAIPilotGallery.CaptureForjaV3',
  '-logFile',$log)
$proc=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -PassThru
if(-not $proc.WaitForExit(1200000)){
  & taskkill.exe /PID $proc.Id /T /F | Out-Null
  throw 'Isolated Forja V3 Unity gallery timed out'
}
if($proc.ExitCode -ne 0){
  if(Test-Path $log){Get-Content $log -Tail 160}
  throw ('Three-building Unity gallery failed '+$proc.ExitCode)
}
$folder=Join-Path $project 'Ciudad2DForjaV3Evidence'
$report=Join-Path $folder 'evidence.json'
if(-not(Test-Path -LiteralPath $report)){throw 'Native Forja V3 gallery evidence absent'}
$j=Get-Content -LiteralPath $report -Raw | ConvertFrom-Json
if($j.engine -ne 'Unity' -or $j.mode -ne 'Editor_RenderTexture' -or
   -not $j.gallery_only -or -not $j.three_new_buildings_present -or
   -not $j.camera_and_terrain_locked -or -not $j.visible_alpha_width_aligned -or
   -not $j.alpha_ground_foot_aligned_to_original -or
   $j.forja_variant -ne 'V3' -or -not $j.master_art_unchanged -or
   $j.art_approved -or $j.main_modified -or $j.gameplay_or_save_modified -or
   -not $j.playmode_interaction_not_certified_in_gallery -or
   @($j.captures).Count -ne 8){throw 'Gallery native evidence contract invalid'}
$files=@{}
foreach($entry in $j.captures){
  $file=Join-Path $folder $entry.file
  if($files.ContainsKey($entry.file)){throw 'Duplicate evidence filename'}
  $files[$entry.file]=$true
  if(-not(Test-Path -LiteralPath $file) -or (Get-Item $file).Length -lt 8000){
    throw ('Missing/empty native PNG '+$entry.file)
  }
  $hash=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash.ToLowerInvariant()
  if($hash -ne $entry.sha256){throw ('Native PNG SHA mismatch '+$entry.file)}
  Add-Type -AssemblyName System.Drawing
  $image=[System.Drawing.Image]::FromFile($file)
  try{
    if($image.Width -ne [int]$entry.width -or $image.Height -ne [int]$entry.height){
      throw ('Native PNG dimensions incorrect '+$entry.file)
    }
  }finally{$image.Dispose()}
}
foreach($mode in @('A_original','B_new_ai')){
  foreach($suffix in @('390x844.png','1170x2532.png','844x390.png','1170x2532_zoom.png')){
    $fileName=$mode+'_'+$suffix
    if(-not $files.ContainsKey($fileName)){throw ('Required A/B PNG missing '+$fileName)}
  }
}
Write-Host ('UNITY_REAL_EDITOR_GALLERY_8_IMAGES_SHA_VERIFIED_ART_NOT_APPROVED sha='+$env:GITHUB_SHA)
