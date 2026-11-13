param([Parameter(Mandatory=$true)][string]$Evidence)
$ErrorActionPreference='Stop'
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native editor occupied; do not interfere'}
if($env:GITHUB_SHA -notmatch '^[a-f0-9]{40}$'){throw 'Exact certificate source required'}
$env:ELDORIA_NEW_UI_ONLY='1'
$env:VIGILIA_TYPOGRAPHY_AB='1'
$project=(Resolve-Path 'Unity').Path
$cases=@(
  @{Name='BreachWatchNativeEvidence'; Marker='BREACH_WATCH_NATIVE_CAPTURE_PASS'; Count=8; Array='shots'},
  @{Name='BreachWatchDestinationNativeEvidence'; Folder='BreachWatchDestinationEvidence'; Marker='VIGILIA_REAL_DESTINATIONS_NATIVE_CAPTURED_ART_REVIEW_PENDING'; Count=16; Array='screens'}
)
foreach($case in $cases){
  $name=$case.Name
  $folderName=if($case.Folder){$case.Folder}else{$name}
  $folder=Join-Path $project $folderName
  # Delete only this disposable capture folder. Its report must be generated
  # afresh by the exact process, never accepted from a previous certificate.
  if(Test-Path $folder){Remove-Item -LiteralPath $folder -Recurse -Force}
  $log=Join-Path $Evidence ($name+'.log')
  if(Test-Path $log){Remove-Item -LiteralPath $log -Force}
  $started=[DateTime]::UtcNow
  $params=@('-batchmode','-force-d3d11','-projectPath',('"'+$project+'"'),
    '-executeMethod',('Eldoria.BreachWatch.Editor.'+$name+'.CaptureBatch'),'-logFile',('"'+$log+'"'))
  $process=Start-Process $env:UNITY_EDITOR_PATH -ArgumentList $params -PassThru
  if(-not $process.WaitForExit(2700000)){
    throw 'Owned native proof exceeded 45 minutes; process left intact, never certify stale output'
  }
  if($process.ExitCode -ne 0 -or -not(Test-Path $log) -or
     -not(Select-String -Path $log -Pattern $case.Marker -SimpleMatch -Quiet)){
    if(Test-Path $log){Get-Content $log -Tail 100}
    throw ('Exact production native probe failed: '+$name)
  }
  $reportPath=Join-Path $folder ($folderName+'.json')
  if(-not(Test-Path $reportPath) -or (Get-Item $reportPath).LastWriteTimeUtc -lt $started.AddSeconds(-3)){
    throw 'Native report missing or stale'
  }
  $report=Get-Content $reportPath -Raw|ConvertFrom-Json
  if($report.source -ne $env:GITHUB_SHA -or $report.artworkApproved -or $report.screenshotCount -ne $case.Count){
    throw 'Wrong source, incomplete native capture or false automatic art approval'
  }
  $screens=@($report.($case.Array))
  if($screens.Count -ne $case.Count){throw 'Native screenshot list incomplete'}
  foreach($screen in $screens){
    $image=Join-Path $folder $screen.file
    if(-not(Test-Path $image) -or
       (Get-FileHash $image -Algorithm SHA256).Hash.ToLowerInvariant() -ne $screen.sha256){throw 'Native pixels missing or altered'}
    if(-not $screen.oldCanvasOff){throw 'Retired Canvas rendered during production proof'}
  }
  if($name -eq 'BreachWatchNativeEvidence'){
    if((Get-Content (Join-Path $folder 'FirstEntry/complete.txt') -Raw) -ne 'PRODUCTION_FIRST_ENTRY_TWO_REAL_TOUCHES_PASS' -or
       (Get-Content (Join-Path $folder 'FirstEntry/source.txt') -Raw) -ne $env:GITHUB_SHA){throw 'Canonical nickname touch/save proof missing'}
    $touches=@(Get-Content (Join-Path $folder 'FirstEntry/visible-narrative-touches.txt'))
    if($touches.Count -lt 1 -or @($touches|Where-Object {$_ -notmatch '^PRODUCTION_VISIBLE_NARRATIVE_TOUCH '}).Count){throw 'Visible narrative continuation not proven'}
  }else{
    if(-not $report.probeOnly -or -not $report.saveUnchanged){throw 'Destination fixture changed the player save'}
    $retention=Get-Content (Join-Path $folder 'BreachWatchRetentionEvidence.json') -Raw|ConvertFrom-Json
    if($retention.source -ne $env:GITHUB_SHA -or $retention.screenshotCount -ne 8 -or -not $retention.saveUnchanged){throw 'Recovered retention proof missing'}
  }
  Copy-Item -LiteralPath $folder -Destination $Evidence -Recurse -Force
}
Write-Host 'VIGILIA_PRODUCTION_NATIVE_CERTIFICATE_PASS_ART_REVIEW_SEPARATE'
