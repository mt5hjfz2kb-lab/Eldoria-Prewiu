param([Parameter(Mandatory=$true)][string]$SourceSha)
$ErrorActionPreference='Stop'
if($SourceSha -ne '5881df4e76da72c2897633c619449c60694b3552'){throw 'Unexpected frozen source'}
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native editor occupied'}
if(-not(Get-Command git.exe -ErrorAction SilentlyContinue)){
  $git=Get-ChildItem "$env:LOCALAPPDATA/GitHubDesktop" -Directory -Filter 'app-*' |
    Sort-Object Name -Descending | ForEach-Object {Join-Path $_.FullName 'resources/app/git/cmd/git.exe'} |
    Where-Object {Test-Path $_} | Select-Object -First 1
  if(-not $git){throw 'Installed Git unavailable'}
  $env:PATH=(Split-Path $git)+';'+$env:PATH
}
$matches=@()
foreach($name in @('eldoria-world-p0-43641168362','eldoria-world-p0-43638950225')){
  $candidate=Join-Path $env:RUNNER_TEMP $name
  if((Test-Path $candidate) -and -not((Get-Item $candidate).Attributes -band [IO.FileAttributes]::ReparsePoint)){
    if((& git -C $candidate rev-parse HEAD).Trim() -eq $SourceSha){$matches+=,$candidate}
  }
}
if($matches.Count -ne 1){throw 'Expected exactly one owned native-tested checkout'}
$root=$matches[0];$project=Join-Path $root 'Unity'
if((Get-PSDrive C).Free/1GB -lt 32){throw 'Insufficient space for WebGL; preserve existing work'}
$changed=@(& git -C $root diff --name-only HEAD -- '*.cs' '*.asmdef' '*.unity')
if($LASTEXITCODE -or $changed.Count){throw ('Frozen gameplay source drift: '+($changed -join ','))}
$audit=Get-Content (Join-Path $root 'tools/eldoria-private-beta/native/existing-input-audit.json') -Raw|ConvertFrom-Json
$art=@($audit.files|Where-Object {$_.path.StartsWith('Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Resources/BastionIII/')})
if($art.Count -ne 51){throw 'Original art inventory changed'}
foreach($item in $art){if((Get-FileHash (Join-Path $root $item.path) -Algorithm SHA256).Hash -ne $item.sha256){throw ('Original art changed '+$item.path)}}
$query=Join-Path $project 'Assets/Eldoria/World4XCanon/Resources/World4X/Blender/RuntimeTerrainQueries.bytes'
if((Get-FileHash $query -Algorithm SHA256).Hash -ne '013EBF8194BE6974DBCADF27E3CF2AE43DB7C7638E0E6CB22CFE838E037ED986'){throw 'Terrain tape differs'}
$out=Join-Path $env:RUNNER_TEMP ('world-p0-build-'+$env:GITHUB_RUN_ID)
if(Test-Path $out){throw 'Preserve existing build evidence'}
New-Item -ItemType Directory -Path $out|Out-Null
Add-Content $env:GITHUB_ENV ('WORLD_P0_BUILD_OUT='+$out)
Add-Content $env:GITHUB_ENV ('WORLD_P0_BUILD_PROJECT='+$project)
$workflowSha=$env:GITHUB_SHA
# The workflow and compiled source have separate identities. Never relabel old code.
$env:GITHUB_SHA=$SourceSha
@{source=$SourceSha;workflow=$workflowSha;nativeRun=43641168362;physicalIphoneTested=$false}|ConvertTo-Json|Set-Content (Join-Path $out 'provenance.json')
$backup=Join-Path $out 'original-inputs';New-Item -ItemType Directory -Path $backup|Out-Null
Copy-Item (Join-Path $project 'ProjectSettings') (Join-Path $backup 'ProjectSettings') -Recurse
$render=@('Assets/UniversalRenderPipelineGlobalSettings.asset','Assets/Eldoria/ArtTests/ValoriaReferenceClean/GeneratedLighting/BakedSceneURP.asset')
foreach($rel in $render){$dest=Join-Path $backup $rel;New-Item -ItemType Directory -Force (Split-Path $dest)|Out-Null;Copy-Item (Join-Path $project $rel) $dest}
$proof=Join-Path $root 'docs/evidence/valoria-reference-clean'
$web=Join-Path $project 'Builds/ValoriaWebPlaytest'
$ui=Join-Path $project 'BreachWatchDestinationEvidence'
$prior=@{}
foreach($entry in @(@{path=$proof;name='prior-proof'},@{path=$web;name='prior-web'},@{path=$ui;name='prior-ui'})){
  $prior[$entry.name]=Test-Path $entry.path
  if($prior[$entry.name]){Move-Item $entry.path (Join-Path $backup $entry.name)}
}
function Invoke-Phase([string]$method,[string]$target,[string]$label,[string]$result){
  for($attempt=1;$attempt -le 2;$attempt++){
    if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native editor occupied'}
    $log=Join-Path $out ($label+'-'+$attempt+'.log')
    $args=@('-batchmode','-force-d3d11','-buildTarget',$target,'-projectPath',('"'+$project+'"'),'-executeMethod',$method,'-logFile',('"'+$log+'"'))
    $proc=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -PassThru
    if(-not $proc.WaitForExit(3600000)){& taskkill.exe /PID $proc.Id /T /F|Out-Null;throw 'Owned Unity phase timed out'}
    if($proc.ExitCode -ne 0){Get-Content $log -Tail 120;throw ('Unity phase failed: '+$label)}
    if(Test-Path $result){return}
    Write-Host ('COLD_IMPORT_NO_RESULT_RETRY '+$label)
  }
  throw ('No genuine result after warm retry: '+$label)
}
try{
  Invoke-Phase 'Eldoria.BreachWatch.Editor.BreachWatchDestinationNativeEvidence.CaptureBatch' 'Win64' 'destinations' (Join-Path $ui 'BreachWatchDestinationEvidence.json')
  foreach($name in @('BreachWatchDestinationEvidence.json','BreachWatchIntegratedTreasureEvidence.json')){
    $j=Get-Content (Join-Path $ui $name) -Raw|ConvertFrom-Json
    if($j.source -ne $SourceSha -or -not $j.saveUnchanged){throw 'Exact UI source/save proof failed'}
    foreach($screen in $j.screens){if((Get-FileHash (Join-Path $ui $screen.file) -Algorithm SHA256).Hash.ToLowerInvariant() -ne $screen.sha256){throw 'UI image hash differs'}}
  }
  Invoke-Phase 'Eldoria.ReferenceClean.Editor.ValoriaIntegrationBuild.ValidateCurrent' 'Win64' 'campaign' (Join-Path $proof 'integration-result.json')
  $j=Get-Content (Join-Path $proof 'integration-result.json') -Raw|ConvertFrom-Json
  if($j.source -ne $SourceSha -or $j.status -ne 'INTEGRATION_PASS_ART_REVIEW_REQUIRED' -or $j.checks -notcontains 'continuous native I-V authored objectives complete' -or $j.checks -notcontains 'continuous I-V final saved state exact'){throw 'Native campaign I-V proof failed'}
  Invoke-Phase 'Eldoria.ReferenceClean.Editor.ValoriaIntegratedWebBuild.Build' 'WebGL' 'webgl' (Join-Path $proof 'web-build.json')
  $j=Get-Content (Join-Path $proof 'web-build.json') -Raw|ConvertFrom-Json
  if($j.source_sha -ne $SourceSha -or $j.status -ne 'WEB_COMPILED_CURRENT_INTEGRATED_SCENE_NOT_PHYSICAL_IPHONE_TESTED' -or -not(Test-Path (Join-Path $web 'index.html'))){throw 'Exact WebGL compile proof failed'}
  Write-Host ('WORLD_P0_EXACT_WEBGL_COMPILED_NOT_IPHONE_PASS '+$SourceSha)
}finally{
  if(@(Get-Process Unity -ErrorAction SilentlyContinue).Count){throw 'Editor still live; retain backups'}
  foreach($entry in @(@{path=$proof;name='prior-proof';output='integrated-build-proof'},@{path=$web;name='prior-web';output='compiled-webgl'},@{path=$ui;name='prior-ui';output='native-ui'})){
    if(Test-Path $entry.path){Move-Item $entry.path (Join-Path $out $entry.output)}
    if($prior[$entry.name]){Move-Item (Join-Path $backup $entry.name) $entry.path}
  }
  foreach($rel in $render){Copy-Item (Join-Path $backup $rel) (Join-Path $project $rel) -Force}
  Get-ChildItem (Join-Path $backup 'ProjectSettings') -Recurse -File|ForEach-Object{
    $rel=$_.FullName.Substring((Join-Path $backup 'ProjectSettings').Length+1)
    $dest=Join-Path (Join-Path $project 'ProjectSettings') $rel
    Copy-Item $_.FullName $dest -Force
    if((Get-FileHash $_.FullName).Hash -ne (Get-FileHash $dest).Hash){throw 'Project settings restoration failed'}
  }
  $env:GITHUB_SHA=$workflowSha
}
