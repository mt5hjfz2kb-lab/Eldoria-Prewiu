$ErrorActionPreference='Stop'
$originRun='42435095850';$originSource='91734045e8d32cc90cc4843b4b7e08e57b1a0458';$base='948ffa9837168a0b68dab80be76b66350422b23a'
$source=Join-Path $env:GITHUB_WORKSPACE 'valoria-layered-source'
$old=Join-Path $env:GITHUB_WORKSPACE "beta-structural-evidence-$originRun-1"
$web=Join-Path $env:GITHUB_WORKSPACE "beta-structural-webgl-$originRun-1"
$out=Join-Path $env:GITHUB_WORKSPACE "beta-structural-evidence-$env:GITHUB_RUN_ID-$env:GITHUB_RUN_ATTEMPT"
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native tools busy; do not restore or export'}
if((git -C $source rev-parse HEAD).Trim() -ne $base){throw 'Source base changed'}
$before=Get-Content (Join-Path $old 'warm-cache-before.json') -Raw|ConvertFrom-Json
$audit=Get-Content (Join-Path $old 'existing-input-audit.json') -Raw|ConvertFrom-Json
$restoration=Get-Content (Join-Path $old 'warm-cache-restoration.json') -Raw|ConvertFrom-Json
$comparison=Get-Content (Join-Path $old 'comparison.json') -Raw|ConvertFrom-Json
$journey=Get-Content (Join-Path $old 'native-journey/integration-result.json') -Raw|ConvertFrom-Json
$build=Get-Content (Join-Path $old 'web-build.json') -Raw|ConvertFrom-Json
if($comparison.status -ne 'NATIVE_PARITY_PASS_NOT_MOBILE_CERTIFICATION' -or $journey.status -ne 'INTEGRATION_PASS_ART_REVIEW_REQUIRED' -or $journey.source -ne $originSource -or $build.source_sha -ne $originSource -or $build.status -ne 'WEB_COMPILED_CURRENT_INTEGRATED_SCENE_NOT_PHYSICAL_IPHONE_TESTED'){throw 'Exact compiled native certificates absent'}
if(-not(Test-Path (Join-Path $old 'complete.txt')) -or -not(Test-Path (Join-Path $web 'index.html'))){throw 'Completed native build absent'}
if(@($restoration.conflicts).Count -or @($restoration.existing_input_changes).Count){throw 'Prior input restoration conflicts require separate review'}
$paths=@('Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/GeneratedLighting/BakedSceneURP.asset','Unity/Assets/UniversalRenderPipelineGlobalSettings.asset')
$expected=@($paths|ForEach-Object {' M '+$_})
$actual=@(git -C $source status --porcelain -- Unity)
$delta=@(Compare-Object @($audit.status) $actual)
if($delta.Count -ne 2 -or @($delta|Where-Object {$_.SideIndicator -ne '=>' -or $expected -notcontains $_.InputObject}).Count){throw 'Unexpected current source differences; preserve without overwrite'}
foreach($r in $before.files){if((Get-FileHash (Join-Path $source $r.path) -Algorithm SHA256).Hash -ne $r.sha256){throw "Previously restored input changed: $($r.path)"}}
New-Item -ItemType Directory -Force $out|Out-Null
$archive=Join-Path $env:GITHUB_WORKSPACE "beta-build-settings-preserved-$env:GITHUB_RUN_ID-$env:GITHUB_RUN_ATTEMPT"
if(Test-Path $archive){throw 'Existing preservation directory must remain untouched'}
New-Item -ItemType Directory $archive|Out-Null
$diff=git -C $source diff -- $paths
$utf8=New-Object Text.UTF8Encoding($false)
[IO.File]::WriteAllLines((Join-Path $out 'post-build-settings.diff'),[string[]]$diff,$utf8)
$settings=@()
foreach($path in $paths){
 $file=Join-Path $source $path;$preserved=Join-Path $archive $path
 New-Item -ItemType Directory -Force (Split-Path $preserved)|Out-Null;Copy-Item $file $preserved
 $beforeHash=(Get-FileHash $file -Algorithm SHA256).Hash
 git -C $source restore --source=$base -- $path
 if($LASTEXITCODE -ne 0){throw 'Failed to restore exact baseline configuration'}
 $settings+=@{path=$path;post_build_sha256=$beforeHash;restored_sha256=(Get-FileHash $file -Algorithm SHA256).Hash;preserved=$preserved}
}
$terrainFile=Join-Path $source $paths[0]
$terrainHash=(Get-FileHash $terrainFile -Algorithm SHA256).Hash
if($terrainHash -ne '330F9E879D49A336971FF9CC8DCFE44E0409DAD96C604CE1B2309D2B9AA6A314'){
 # Git normalizes text EOLs. Accept only a byte sequence matching the actual native input certificate.
 $lf=[IO.File]::ReadAllText($terrainFile).Replace("`r`n","`n")
 foreach($text in @($lf,$lf.Replace("`n","`r`n"))){
  $bytes=$utf8.GetBytes($text);$h=[Security.Cryptography.SHA256]::Create()
  try {$hash=([BitConverter]::ToString($h.ComputeHash($bytes))).Replace('-','')}finally{$h.Dispose()}
  if($hash -eq '330F9E879D49A336971FF9CC8DCFE44E0409DAD96C604CE1B2309D2B9AA6A314'){[IO.File]::WriteAllBytes($terrainFile,$bytes);$terrainHash=$hash;break}
 }
 $settings[0].restored_sha256=$terrainHash
}
if($terrainHash -ne '330F9E879D49A336971FF9CC8DCFE44E0409DAD96C604CE1B2309D2B9AA6A314'){throw 'Restored URP asset differs from canonical native input signature'}
$after=@(git -C $source status --porcelain -- Unity)
if(@(Compare-Object @($audit.status) $after).Count){throw 'Source inventory not restored'}
foreach($r in $before.files){if((Get-FileHash (Join-Path $source $r.path) -Algorithm SHA256).Hash -ne $r.sha256){throw 'Final original input hash mismatch'}}
# Export immutable prior certificates, preserving the original failed restoration report.
foreach($name in 'comparison.json','source-overlay.json','existing-input-audit.json','warm-cache-before.json','complete.txt','web-build.json','web-input-current-manifest.txt','web-input-current-signature.txt','web-authority-platform-normalization.txt'){
 if(Test-Path (Join-Path $old $name)){Copy-Item (Join-Path $old $name) (Join-Path $out $name)}
}
Copy-Item (Join-Path $old 'warm-cache-restoration.json') (Join-Path $out 'warm-cache-restoration-before-export.json')
New-Item -ItemType Directory (Join-Path $out 'native-journey')|Out-Null
foreach($name in 'integration-result.json','integration-input-manifest.txt','integration-input-signature.txt'){Copy-Item (Join-Path $old ('native-journey/'+$name)) (Join-Path $out ('native-journey/'+$name))}
$files=@(Get-ChildItem $web -File -Recurse|ForEach-Object {@{path=$_.FullName.Substring($web.Length+1).Replace('\','/');bytes=$_.Length;sha256=(Get-FileHash $_.FullName -Algorithm SHA256).Hash}})
if($files.Count -ne $build.files.Count){throw 'Fresh WebGL file inventory differs from build certificate'}
$restoreDoc=@{restored=$true;original_run=$originRun;export_run=$env:GITHUB_RUN_ID;settings=$settings;existing_input_changes=@();conflicts=@();status_changes=@()}
[IO.File]::WriteAllText((Join-Path $out 'warm-cache-restoration.json'),($restoreDoc|ConvertTo-Json -Depth 8),$utf8)
$exportDoc=@{status='CERTIFIED_BUILD_EXPORTED_SOURCE_RESTORED_POST_BUILD_DIFF_REVIEW_REQUIRED';origin_native_run=$originRun;origin_native_source=$originSource;origin_evidence_artifact=13598859327;origin_evidence_zip_sha256='52a39d2be58704088348b944b3dfb31469e27171972c60d45c4d81f34040694a';export_run=$env:GITHUB_RUN_ID;export_source=$env:GITHUB_SHA;native_journey_checks=$journey.checks.Count;files=$files;settings=$settings;physical_iphone_tested=$false;post_build_diff_sha256=(Get-FileHash (Join-Path $out 'post-build-settings.diff') -Algorithm SHA256).Hash}
[IO.File]::WriteAllText((Join-Path $out 'certified-export.json'),($exportDoc|ConvertTo-Json -Depth 10),$utf8)
Add-Content $env:GITHUB_ENV "BETA_CERTIFIED_WEB=$web"
Write-Host 'Certified WebGL preserved and exported without rebuilding; exact original source restored'
