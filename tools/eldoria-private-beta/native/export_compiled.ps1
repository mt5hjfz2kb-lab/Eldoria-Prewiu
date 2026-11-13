param([string]$Evidence)
$ErrorActionPreference='Stop'
$origin='42508059413';$compiled='6cf82e7bfb07a8bca7085ea5c3518fbb3c0f8a80'
$originEvidence=Join-Path $env:GITHUB_WORKSPACE "beta-structural-evidence-$origin-1"
$web=Join-Path $env:GITHUB_WORKSPACE "beta-structural-webgl-$origin-1"
$source=Join-Path $env:GITHUB_WORKSPACE 'beta-structural-source-42505611257-1'
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native tools busy; preserve all outputs'}
if(-not(Test-Path (Join-Path $originEvidence 'complete.txt'))){throw 'Native completion absent'}
$c=Get-Content (Join-Path $originEvidence 'comparison.json') -Raw|ConvertFrom-Json
$j=Get-Content (Join-Path $originEvidence 'native-journey/integration-result.json') -Raw|ConvertFrom-Json
$w=Get-Content (Join-Path $originEvidence 'web-build.json') -Raw|ConvertFrom-Json
$r=Get-Content (Join-Path $originEvidence 'warm-cache-restoration.json') -Raw|ConvertFrom-Json
if($c.status -ne 'NATIVE_PARITY_PASS_NOT_MOBILE_CERTIFICATION' -or @($c.passes).Count -ne 2){throw 'Parity proof absent'}
foreach($p in $c.passes){if(-not $p.parity -or $p.staged.source -ne $compiled -or -not $p.staged.canonicalSavesUnchanged){throw 'Parity source mismatch'}}
if($j.status -ne 'INTEGRATION_PASS_ART_REVIEW_REQUIRED' -or $j.source -ne $compiled -or @($j.checks).Count -ne 225){throw 'Journey certificate mismatch'}
if($w.status -ne 'WEB_COMPILED_CURRENT_INTEGRATED_SCENE_NOT_PHYSICAL_IPHONE_TESTED' -or $w.source_sha -ne $compiled -or $w.error){throw 'Build certificate mismatch'}
$allowed=@(' M Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/GeneratedLighting/BakedSceneURP.asset',' M Unity/Assets/UniversalRenderPipelineGlobalSettings.asset')
$reported=@($r.status_changes|ForEach-Object {$_.InputObject})
if(@($r.conflicts).Count -or @($r.existing_input_changes).Count -or $reported.Count -ne 2 -or @(Compare-Object $allowed $reported).Count){throw 'Restoration failure differs from inspected evidence'}
$current=@(git -C $source status --porcelain -- Unity)
if($LASTEXITCODE -ne 0 -or $current.Count -ne @($r.unity_status).Count -or @(Compare-Object $current @($r.unity_status)).Count){throw 'Source state changed since preserved build'}
if(Test-Path $Evidence){throw 'Export evidence already exists; preserve prior snapshot'}
New-Item -ItemType Directory $Evidence|Out-Null
Copy-Item (Join-Path $originEvidence '*') $Evidence -Recurse
$utf8=New-Object Text.UTF8Encoding($false)
$diff=git -C $source diff -- 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/GeneratedLighting/BakedSceneURP.asset' 'Unity/Assets/UniversalRenderPipelineGlobalSettings.asset'
[IO.File]::WriteAllText((Join-Path $Evidence 'post-build-urp.diff'),($diff -join "`n"),$utf8)
$files=@(Get-ChildItem $web -File -Recurse|ForEach-Object {@{path=$_.FullName.Substring($web.Length+1).Replace('\','/');bytes=$_.Length;sha256=(Get-FileHash $_.FullName -Algorithm SHA256).Hash}})
$expected=@($w.files|ForEach-Object {($_ -replace '^Builds/ValoriaWebPlaytest[\\/]','').Replace('\','/')})
$actual=@($files|ForEach-Object {$_.path})
if($actual.Count -ne $expected.Count -or @(Compare-Object $expected $actual).Count){throw 'Preserved build inventory differs from build certificate'}
$proof=@{origin_run=$origin;origin_source=$compiled;export_run=$env:GITHUB_RUN_ID;export_source=$env:GITHUB_SHA;status='COMPILED_AND_NATIVE_VERIFIED_OUTPUT_RECOVERED_WITHOUT_REBUILD';source_modified_by_recovery=$false;restoration_pending=$allowed;files=$files}
[IO.File]::WriteAllText((Join-Path $Evidence 'recovered-output-inventory.json'),($proof|ConvertTo-Json -Depth 8),$utf8)
Add-Content $env:GITHUB_ENV "RECOVERED_WEB=$web"
Write-Host "RECOVERED_COMPILED_OUTPUT files=$($files.Count) origin=$origin; isolated URP post-build differences retained for review"
