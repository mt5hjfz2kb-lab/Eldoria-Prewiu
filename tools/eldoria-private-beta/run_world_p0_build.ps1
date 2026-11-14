param([Parameter(Mandatory=$true)][string]$SourceSha)
$ErrorActionPreference='Stop'
trap { Write-Host $_.ScriptStackTrace; throw }
if($SourceSha -ne 'd0b21fd5708c5070d1bd62dc6c6a98399c549051'){throw 'Unexpected frozen source'}
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native editor occupied'}
if(-not(Get-Command git.exe -ErrorAction SilentlyContinue)){
  $git=Get-ChildItem "$env:LOCALAPPDATA/GitHubDesktop" -Directory -Filter 'app-*' |
    Sort-Object Name -Descending | ForEach-Object {Join-Path $_.FullName 'resources/app/git/cmd/git.exe'} |
    Where-Object {Test-Path $_} | Select-Object -First 1
  if(-not $git){throw 'Installed Git unavailable'}
  $env:PATH=(Split-Path $git)+';'+$env:PATH
}
# Reuse immutable objects owned by this workstream, never another artist's checkout.
$seed=Join-Path $env:GITHUB_WORKSPACE 'world-p0-build-source-5881df4e76da'
$root=Join-Path $env:GITHUB_WORKSPACE ('world-p0-build-source-'+$SourceSha.Substring(0,12))
if(Test-Path $root){
  if((Get-Item $root).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Source reparse point'}
  if((& git -C $root rev-parse --verify HEAD).Trim() -ne $SourceSha){throw 'Preserve different existing source'}
}else{
  if((Get-PSDrive C).Free/1GB -lt 36){throw 'Insufficient disk; preserve sources'}
  if(-not(Test-Path $seed) -or ((Get-Item $seed).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Owned immutable seed missing'}
  if((& git -C $seed remote get-url origin).Trim() -ne ('https://github.com/'+$env:GITHUB_REPOSITORY+'.git')){throw 'Owned seed origin mismatch'}
  & git -C $seed cat-file -e '5881df4e76da72c2897633c619449c60694b3552^{commit}'
  if($LASTEXITCODE){throw 'Owned seed provenance missing'}
  & git -C $seed fetch --no-tags --depth=1 --filter=blob:none origin $SourceSha
  if($LASTEXITCODE){throw 'Exact source fetch failed'}
  & git -C $seed worktree add --no-checkout --detach $root $SourceSha
  if($LASTEXITCODE){throw 'New owned worktree failed; preserve prior sources'}
  & git -C $root sparse-checkout set --no-cone '/Unity/' '/tools/eldoria-private-beta/'
  if($LASTEXITCODE){throw 'Sparse configuration failed'}
  & git -C $root checkout --detach $SourceSha
  if($LASTEXITCODE){throw 'Exact source checkout failed; preserve all worktrees'}
}
if((& git -C $root rev-parse HEAD).Trim() -ne $SourceSha){throw 'Frozen source mismatch'}
$project=Join-Path $root 'Unity' 
if((Get-PSDrive C).Free/1GB -lt 32){throw 'Insufficient space for WebGL; preserve existing work'}
$changed=@(& git -C $root diff --name-only HEAD -- '*.cs' '*.asmdef' '*.unity')
if($LASTEXITCODE -or $changed.Count){throw ('Frozen gameplay source drift: '+($changed -join ','))}
$audit=Get-Content (Join-Path $root 'tools/eldoria-private-beta/native/existing-input-audit.json') -Raw|ConvertFrom-Json
$art=@($audit.files|Where-Object {$_.path.StartsWith('Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Resources/BastionIII/')})
if($art.Count -ne 51){throw 'Original art inventory changed'}
$origins=@('breachwatch-native-source-43489752868-1',
            'beta-structural-preserved-inputs-43246088947-1',
            'beta-structural-preserved-inputs-43211338159-1',
            'beta-structural-preserved-inputs-43135567923-1',
            'beta-structural-preserved-inputs-43127449523-1',
            'beta-structural-preserved-inputs-42998206844-1',
            'beta-structural-preserved-inputs-42993101501-1',
            'beta-structural-preserved-inputs-42992127220-1',
            'beta-structural-preserved-inputs-42985136961-1',
            'beta-structural-preserved-inputs-42909656581-1',
            'beta-structural-preserved-inputs-42714385780-1',
            'beta-structural-preserved-inputs-42682071151-1',
            'beta-structural-preserved-inputs-42645633286-1',
            'beta-structural-preserved-inputs-42595057735-1',
            'beta-structural-preserved-inputs-42593886558-1',
            'beta-structural-preserved-inputs-42592919747-1',
            'beta-structural-preserved-inputs-42590953962-1',
            'beta-structural-preserved-inputs-42589025185-1',
            'beta-structural-preserved-inputs-42588844458-1',
            'beta-structural-preserved-inputs-42588287142-1',
            'beta-structural-preserved-inputs-42587774697-1',
            'beta-structural-preserved-inputs-42586848479-1',
            'beta-structural-preserved-inputs-42586512619-1',
            'beta-structural-preserved-inputs-42585946027-1',
            'beta-structural-preserved-inputs-42518950317-1',
            'beta-structural-preserved-inputs-42514790566-1',
            'beta-structural-preserved-inputs-42514002669-1',
            'beta-structural-preserved-inputs-42513182378-1',
            'beta-structural-preserved-inputs-42508059413-1',
            'beta-structural-preserved-inputs-42507779126-1',
            'beta-structural-preserved-inputs-42435095850-1',
            'beta-structural-preserved-inputs-42431634641-1',
            'beta-structural-preserved-inputs-42431391139-1',
            'forja-thin-native-source-42408083695',
            'beta-structural-source-42505611257-1',
            'valoria-layered-source')
foreach($item in $art){
  $dest=Join-Path $root $item.path
  if(-not(Test-Path $dest)){
    $from=$null
    foreach($name in $origins){
      $candidate=Join-Path (Join-Path $env:GITHUB_WORKSPACE $name) $item.path
      if((Test-Path $candidate) -and (Get-FileHash $candidate -Algorithm SHA256).Hash -eq $item.sha256){$from=$candidate;break}
    }
    if(-not $from){throw ('Audited original art unavailable '+$item.path)}
    New-Item -ItemType Directory -Force (Split-Path $dest)|Out-Null
    Copy-Item $from $dest
  }
  if((Get-FileHash $dest -Algorithm SHA256).Hash -ne $item.sha256){throw ('Original art changed '+$item.path)}
}
$query=Join-Path $project 'Assets/Eldoria/World4XCanon/Resources/World4X/Blender/RuntimeTerrainQueries.bytes'
$priorTerrain=Join-Path $env:GITHUB_WORKSPACE 'world-p0-verified-prior-proof/integrated-terrain-43489752868'
foreach($part in @(@{path=$query;name='RuntimeTerrainQueries.bytes';hash='013EBF8194BE6974DBCADF27E3CF2AE43DB7C7638E0E6CB22CFE838E037ED986'},@{path=($query+'.meta');name='RuntimeTerrainQueries.bytes.meta';hash='C05C2D9E21F36BA5FBCBBBD4D97CC377A1CB04F72FBDA47BC630B1D9F4A324BD'})){
  if(-not(Test-Path $part.path)){
    $from=Join-Path $priorTerrain $part.name
    if((Get-FileHash $from -Algorithm SHA256).Hash -ne $part.hash){throw 'Original terrain bytes differ'}
    New-Item -ItemType Directory -Force (Split-Path $part.path)|Out-Null
    Copy-Item $from $part.path
  }
  if((Get-FileHash $part.path -Algorithm SHA256).Hash -ne $part.hash){throw 'Terrain tape or GUID differs'}
}
$out=Join-Path $env:RUNNER_TEMP ('world-p0-build-'+$env:GITHUB_RUN_ID)
if(Test-Path $out){throw 'Preserve existing build evidence'}
New-Item -ItemType Directory -Path $out|Out-Null
Add-Content $env:GITHUB_ENV ('WORLD_P0_BUILD_OUT='+$out)
Add-Content $env:GITHUB_ENV ('WORLD_P0_BUILD_PROJECT='+$project)
$workflowSha=$env:GITHUB_SHA
# The workflow and compiled source have separate identities. Never relabel old code.
$env:GITHUB_SHA=$SourceSha
@{source=$SourceSha;workflow=$workflowSha;nativeRun=$env:GITHUB_RUN_ID;physicalIphoneTested=$false}|ConvertTo-Json|Set-Content (Join-Path $out 'provenance.json')
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
function Invoke-WorldProof {
  $xml=Join-Path $out 'test-results.xml'
  $env:ELDORIA_WORLD_P0_CAPTURE_REAL_ART='1'
  $env:ELDORIA_WORLD_P0_CAPTURE_DIR=Join-Path $out 'world-unity-views'
  for($attempt=1;$attempt -le 2;$attempt++){
    if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Editor occupied before real World test'}
    $log=Join-Path $out ('world-playmode-'+$attempt+'.log')
    $args=@('-batchmode','-force-d3d11','-buildTarget','Win64','-projectPath',('"'+$project+'"'),
      '-runTests','-testPlatform','PlayMode','-testFilter','Eldoria.BetaMemory.Tests.LazyRegionStartupTests',
      '-testResults',('"'+$xml+'"'),'-logFile',('"'+$log+'"'))
    $proc=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -PassThru
    if(-not $proc.WaitForExit(3600000)){& taskkill.exe /PID $proc.Id /T /F|Out-Null;throw 'Owned world test timed out'}
    if($proc.ExitCode -ne 0){Get-Content $log -Tail 100;throw 'World PlayMode failed'}
    if(Test-Path $xml){break}
  }
  if(-not(Test-Path $xml)){throw 'No genuine NUnit result'}
  [xml]$r=Get-Content $xml -Raw
  $cases=@($r.SelectNodes('//test-case')|Where-Object {$_.GetAttribute('fullname').EndsWith('.RealWorldStartsOnFirstEligibleEntryWithoutSecondGatewayOrNewHUD')})
  if($r.'test-run'.result -ne 'Passed' -or [int]$r.'test-run'.total -ne 1 -or [int]$r.'test-run'.passed -ne 1 -or [int]$r.'test-run'.failed -ne 0 -or [int]$r.'test-run'.skipped -ne 0 -or $cases.Count -ne 1 -or $cases[0].GetAttribute('result') -ne 'Passed'){throw 'Exact world fixture did not pass'}
  foreach($marker in @('ELDORIA_WORLD_STREAMING_VISUALS','ELDORIA_LAZY_REAL_WORLD_NATIVE_12_REENTRY_CYCLES_COMPLETE')){
    if(-not(Select-String $log -Pattern $marker -SimpleMatch -Quiet)){throw ('Missing native marker '+$marker)}
  }
  if(@(Select-String $log -Pattern 'ELDORIA_WORLD_SETTLED_VISIBLE_ENEMIES' -SimpleMatch).Count -ne 6){throw 'Missing settled viewport coverage'}
  if(@(Get-ChildItem $env:ELDORIA_WORLD_P0_CAPTURE_DIR -Filter '*-unity.png' -File).Count -ne 6){throw 'Missing genuine world views'}
  Write-Host ('WORLD_P0_RECONCILED_NATIVE_PASS '+$SourceSha)
}
$probe=Join-Path $project 'Assets/EldoriaWorldP0BuildProbe'
if((Test-Path $probe) -or (Test-Path ($probe+'.meta'))){throw 'Preserve existing build probe'}
try{
  # Preserve the established mobile build recipe: collision data is baked
  # offline rather than introduced as a second variable in the memory trial.
  New-Item -ItemType Directory -Path (Join-Path $probe 'Editor')|Out-Null
  Copy-Item (Join-Path $root 'tools/eldoria-private-beta/native/CollisionBake.cs') (Join-Path $probe 'Editor/CollisionBake.cs')
  $env:ELDORIA_COLLISION_BAKE_OUT=$out
  Invoke-Phase 'EldoriaBetaCollisionBake.Enable' 'Win64' 'collision-prebake' (Join-Path $out 'collision-bake-setting.json')
  $collision=Get-Content (Join-Path $out 'collision-bake-setting.json') -Raw|ConvertFrom-Json
  if($collision.source -ne $SourceSha -or -not $collision.after){throw 'Established collision prebake recipe not active'}
  Invoke-WorldProof
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
  if(Test-Path $probe){Move-Item $probe (Join-Path $out 'build-editor-probe')}
  if(Test-Path ($probe+'.meta')){Move-Item ($probe+'.meta') (Join-Path $out 'build-editor-probe.meta')}
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
