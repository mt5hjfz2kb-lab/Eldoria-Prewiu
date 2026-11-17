param([Parameter(Mandatory=$true)][string]$SourceRoot)
$ErrorActionPreference='Stop'
$project=Join-Path $SourceRoot 'Unity'
$log=Join-Path $env:RUNNER_TEMP ('city2d-blender-ab-'+$env:GITHUB_RUN_ID+'.log')
$args=@('-batchmode','-quit','-force-d3d11','-projectPath',$project,'-executeMethod','Eldoria.Experimentos.Ciudad2D.Editor.City2DBlenderPilotEvidence.Capture','-logFile',$log)
$proc=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -PassThru
if(-not $proc.WaitForExit(1200000)){& taskkill.exe /PID $proc.Id /T /F | Out-Null;throw 'Unity Blender A/B timed out'}
if($proc.ExitCode -ne 0){if(Test-Path $log){Get-Content $log -Tail 160};throw ('Unity Blender A/B failed '+$proc.ExitCode)}
$folder=Join-Path $project 'Ciudad2DBlenderPilotEvidence'
$evidence=Join-Path $folder 'evidence.json'
if(-not(Test-Path $evidence)){throw 'Blender Unity A/B evidence missing'}
$j=Get-Content $evidence -Raw | ConvertFrom-Json
if(@($j.captures).Count -ne 16 -or -not $j.comparison_camera_locked -or -not $j.two_building_layers -or -not $j.unchanged_unity_hotspots -or -not $j.progression_real_read_only -or $j.gameplay_or_save_modified -or $j.art_approved -or $j.device_qa -ne 'PENDING'){throw 'A/B evidence contract violated'}
foreach($entry in $j.captures){
  $p=Join-Path $folder $entry.file
  if(-not(Test-Path $p) -or (Get-Item $p).Length -lt 8000){throw ('A/B Unity PNG missing '+$entry.file)}
  if((Get-FileHash -Algorithm SHA256 $p).Hash.ToLowerInvariant() -ne $entry.sha256){throw ('A/B SHA mismatch '+$entry.file)}
}
Write-Host ('CITY2D_BLENDER_UNITY_AB_16_SOURCE_VERIFIED captures='+@($j.captures).Count)
