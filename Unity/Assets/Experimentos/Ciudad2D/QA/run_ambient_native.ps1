param([Parameter(Mandatory=$true)][string]$SourceRoot)
$ErrorActionPreference='Stop'
$project=Join-Path $SourceRoot 'Unity'
$log=Join-Path $env:RUNNER_TEMP ('city2d-ambient-native-'+$env:GITHUB_RUN_ID+'.log')
$args=@('-batchmode','-quit','-force-d3d11','-projectPath',$project,
 '-executeMethod','Eldoria.Experimentos.Ciudad2D.Editor.City2DAmbientNativeEvidence.Capture',
 '-logFile',$log)
$proc=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -PassThru
if(-not $proc.WaitForExit(900000)){
 & taskkill.exe /PID $proc.Id /T /F | Out-Null
 throw 'Unity isolated living-city camera proof timed out'
}
if($proc.ExitCode -ne 0){
 if(Test-Path -LiteralPath $log){Get-Content -LiteralPath $log -Tail 200}
 throw ('Unity living-city ambient method failed code '+$proc.ExitCode)
}
$folder=Join-Path $project 'Ciudad2DAmbientEvidence'
$report=Join-Path $folder 'evidence.json'
if(-not(Test-Path -LiteralPath $report)){throw 'Living city genuine Unity evidence missing'}
$j=Get-Content -LiteralPath $report -Raw | ConvertFrom-Json
if($j.engine -ne 'Unity' -or $j.mode -ne 'Editor_RenderTexture_two_times' -or
   @($j.captures).Count -ne 10 -or @($j.stage_gates).Count -ne 3 -or
   -not $j.original_bastion_I_for_1_4_and_V_for_5 -or
   -not $j.no_unique_bastion_II_III_IV_required -or
   -not $j.original_scene_read_only -or $j.gameplay_or_saves_modified -or
   $j.main_modified -or $j.visual_art_approved -or $j.playmode_proof_of_animation -or
   $j.physical_device_test -ne 'PENDING'){
 throw 'Living-city evidence truth contract invalid'
}
if(@($j.stage_gates | Where-Object {$_.stage -eq 1 -and $_.active -eq 4 -and $_.smoke -eq 0}).Count -ne 1 -or
   @($j.stage_gates | Where-Object {$_.stage -eq 3 -and $_.active -eq 8 -and $_.smoke -eq 4}).Count -ne 1 -or
   @($j.stage_gates | Where-Object {$_.stage -eq 5 -and $_.active -eq 10 -and $_.smoke -eq 6}).Count -ne 1){
 throw 'Chimney smoke rendered on locked, available or unbuilt land'
}
$seen=@{}
foreach($item in $j.captures){
 $file=$item.file
 if($seen.ContainsKey($file)){throw 'Duplicate real PNG'}
 $seen[$file]=$true
 $path=Join-Path $folder $file
 if(-not(Test-Path -LiteralPath $path) -or (Get-Item $path).Length -lt 8000){
  throw ('Missing genuine PNG '+$file)
 }
 $hash=(Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash.ToLowerInvariant()
 if($hash -ne $item.sha256){throw ('Real Unity frame SHA mismatch '+$file)}
 Add-Type -AssemblyName System.Drawing
 $img=[System.Drawing.Image]::FromFile($path)
 try {
  if($img.Width -ne [int]$item.width -or $img.Height -ne [int]$item.height){
   throw ('Real Unity ambient dimensions mismatch '+$file)
  }
 } finally {$img.Dispose()}
}
foreach($stage in @(1,3,5)){
 $views=if($stage -eq 5){@('390x844','1170x2532','844x390')}else{@('390x844')}
 foreach($wh in $views){
  $a=('bastion'+$stage+'_'+$wh+'_t1.png')
  $b=('bastion'+$stage+'_'+$wh+'_t2.png')
  if(-not $seen.ContainsKey($a) -or -not $seen.ContainsKey($b)){throw ('Time pair absent '+$stage+$wh)}
  $hashA=(Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $folder $a)).Hash
  $hashB=(Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $folder $b)).Hash
  if($hashA -eq $hashB){throw ('Effect not animated in actual Unity rendered frames '+$a)}
 }
}
Write-Host ('CITY2D_REAL_UNITY_LIVING_AMBIENT_10_SHA_PASS_SHA='+$env:GITHUB_SHA)
Write-Host 'ART_APPROVAL_PENDING PHYSICAL_IPHONE_PENDING ORIGINAL_GAMEPLAY_UNCHANGED'
