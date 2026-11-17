param([Parameter(Mandatory=$true)][string]$SourceRoot)
$ErrorActionPreference='Stop'
$project=Join-Path $SourceRoot 'Unity'
$log=Join-Path $env:RUNNER_TEMP ('city2d-premium-scene-'+$env:GITHUB_RUN_ID+'.log')
$program='Eldoria.Experimentos.Ciudad2D.Editor.City2DPremiumPreview.Capture'
$args=@('-batchmode','-quit','-force-d3d11','-projectPath',$project,
 '-executeMethod',$program,'-logFile',$log)
$proc=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -PassThru
if(-not $proc.WaitForExit(900000)){
 & taskkill.exe /PID $proc.Id /T /F | Out-Null
 throw 'Premium V1 Unity scene builder timed out'
}
if($proc.ExitCode -ne 0){
 if(Test-Path $log){Get-Content $log -Tail 180}
 throw ('Premium V1 scene build failed code '+$proc.ExitCode)
}
$scene=Join-Path $project 'Assets/Experimentos/Ciudad2D/QA/Experimento_Ciudad2D_PremiumV1.unity'
if(-not(Test-Path -LiteralPath $scene)){
 throw 'No independent playable premium scene authored by Unity'
}
$proof=Join-Path $project 'Ciudad2DPremiumPlayableEvidence'
$manifest=Join-Path $proof 'evidence.json'
if(-not(Test-Path $manifest)){throw 'Premium actual Unity captures missing'}
$j=Get-Content -LiteralPath $manifest -Raw | ConvertFrom-Json
if($j.engine -ne 'Unity' -or $j.mode -ne 'Editor_RenderTexture' -or
   @($j.captures).Count -ne 5 -or
   @($j.three_premium_candidates).Count -ne 3 -or
   -not $j.shadows_independent_built_gated -or
   -not $j.bastion_I_1_to_4_V_at_5 -or
   $j.cantera_premium -or
   $j.gameplay_modified -or $j.main_modified -or $j.art_approved -or
   $j.iPhone_physical_test -ne 'PENDING'){
 throw 'Premium scene independent proof truth contract incorrect'
}
$hashes=@{}
foreach($frame in $j.captures){
 $path=Join-Path $proof $frame.file
 if(-not(Test-Path -LiteralPath $path) -or (Get-Item $path).Length -lt 8000){
  throw ('Genuine Unity premium screenshot missing '+$frame.file)
 }
 $hash=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
 if($hash -ne $frame.sha256){throw ('Wrong premium Unity image SHA '+$frame.file)}
 Add-Type -AssemblyName System.Drawing
 $im=[System.Drawing.Image]::FromFile($path)
 try{
  if($im.Width -ne [int]$frame.width -or $im.Height -ne [int]$frame.height){
   throw ('Premium native screenshot dimensions incorrect '+$frame.file)
  }
 }finally{$im.Dispose()}
 $hashes[$frame.file]=$true
}
foreach($item in @('premium_bastion1_390x844.png',
                   'premium_bastion3_390x844.png',
                   'premium_bastion5_390x844.png',
                   'premium_bastion5_1170x2532.png',
                   'premium_bastion5_844x390.png')){
 if(-not $hashes.ContainsKey($item)){throw ('Missing premium stage/resolution '+$item)}
}
Write-Host 'PREMIUM_V1_REAL_UNITY_SCENE_COPY_AND_5_SOURCE_SHA_PASS_ART_UNAPPROVED'

# Run a completely different Unity invocation that enters actual Editor PlayMode
# and tests pointer dispatch, HUD, read-only states and shadow built-gates.
$xml=Join-Path $env:RUNNER_TEMP ('city2d-premium-playmode-'+$env:GITHUB_RUN_ID+'.xml')
$testlog=Join-Path $env:RUNNER_TEMP ('city2d-premium-playmode-'+$env:GITHUB_RUN_ID+'.log')
$filter='Eldoria.Experimentos.Ciudad2D.PlayModeProof.PremiumVisualSceneProof.PremiumScene_GenuinePlayMode_StagesSpritesShadowsPointer'
# Unity 6000.3: -quit is NOT supported while -runTests executes.
# Unity Test Framework exits the Editor itself after it writes its NUnit XML.
$args=@('-batchmode','-force-d3d11','-projectPath',$project,
  '-runTests','-testPlatform','PlayMode','-testFilter',$filter,
  '-testResults',$xml,'-logFile',$testlog)
$job=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -PassThru
if(-not $job.WaitForExit(900000)){
 & taskkill.exe /PID $job.Id /T /F | Out-Null
 throw 'Premium actual Editor PlayMode test timed out'
}
if(-not(Test-Path $xml)){
 if(Test-Path $testlog){Get-Content $testlog -Tail 200}
 throw ('Premium PlayMode XML missing, Unity exit '+$job.ExitCode)
}
[xml]$results=Get-Content -LiteralPath $xml
$total=[int]$results.DocumentElement.GetAttribute('total')
$passed=[int]$results.DocumentElement.GetAttribute('passed')
$failed=[int]$results.DocumentElement.GetAttribute('failed')
if($total -lt 1 -or $passed -lt 1 -or $failed -ne 0 -or $job.ExitCode -ne 0){
 if(Test-Path $testlog){Get-Content $testlog -Tail 180}
 throw ("Premium PlayMode failed: total=$total passed=$passed failed=$failed exit="+$job.ExitCode)
}
Write-Host 'PREMIUM_V1_REAL_PLAYMODE_SCENE_SHADOW_PLOT_TOUCH_HUD_PASS_NO_IPHONE_CLAIM'
