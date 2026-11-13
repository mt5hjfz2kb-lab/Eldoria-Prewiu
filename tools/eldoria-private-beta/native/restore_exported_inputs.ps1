param([string]$Evidence)
$ErrorActionPreference='Stop'
$source=Join-Path $env:GITHUB_WORKSPACE 'beta-structural-source-42505611257-1'
$origin=Join-Path $env:GITHUB_WORKSPACE 'beta-structural-evidence-42508059413-1'
$frozen='948ffa9837168a0b68dab80be76b66350422b23a'
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native tools busy'}
if((git -C $source rev-parse HEAD).Trim() -ne $frozen){throw 'Not the owned frozen isolated source'}
$auditPath=Join-Path $origin 'existing-input-audit.json'
if((Get-FileHash $auditPath -Algorithm SHA256).Hash -ne 'DB0DC33D00DFA620AF5EA2F8B26557C999731F871F85A67A5985E4ECCE866AF0'){throw 'Original reviewed audit changed'}
$audit=Get-Content $auditPath -Raw|ConvertFrom-Json
$paths=@('Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/GeneratedLighting/BakedSceneURP.asset','Unity/Assets/UniversalRenderPipelineGlobalSettings.asset')
$diff=(git -C $source diff -- $paths)-join "`n"
$utf8=New-Object Text.UTF8Encoding($false)
$hasher=[Security.Cryptography.SHA256]::Create()
$hash=([BitConverter]::ToString($hasher.ComputeHash($utf8.GetBytes($diff)))).Replace('-','').ToLowerInvariant()
$hasher.Dispose()
if($hash -ne 'b2fd708762e1abf446adcd16d8f3fc91bab3f54502710d623ba417e434644ec4'){throw 'Post-build URP changes differ from reviewed exact diff'}
if(Test-Path $Evidence){throw 'Restoration evidence already exists'}
New-Item -ItemType Directory $Evidence|Out-Null
[IO.File]::WriteAllText((Join-Path $Evidence 'reviewed-urp.diff'),$diff,$utf8)
foreach($path in $paths){
 $to=Join-Path $Evidence ('before/'+$path)
 New-Item -ItemType Directory -Force (Split-Path $to)|Out-Null
 Copy-Item (Join-Path $source $path) $to
}
# Only two known generated changes in our disposable checkout. Shared warm
# project and compiled/exported build are never written by this restoration.
& git -C $source restore --source=$frozen -- $paths
if($LASTEXITCODE -ne 0){throw 'Exact input restoration failed; preserved bytes retained'}
$status=@(git -C $source status --porcelain -- Unity)
if($LASTEXITCODE -ne 0 -or $status.Count -ne @($audit.status).Count -or @(Compare-Object $status @($audit.status)).Count){throw 'Restored source status differs from original input audit'}
$actual=@(git -C $source ls-files --modified --others --exclude-standard -- Unity)
$expected=@($audit.files|ForEach-Object {$_.path})
if($actual.Count -ne $expected.Count -or @(Compare-Object $actual $expected).Count){throw 'Restored input membership differs'}
foreach($item in $audit.files){if((Get-FileHash (Join-Path $source $item.path) -Algorithm SHA256).Hash -ne $item.sha256){throw "Restored input hash differs: $($item.path)"}}
[IO.File]::WriteAllText((Join-Path $Evidence 'restoration.json'),(@{status='EXACT_ORIGINAL_ISOLATED_INPUT_RESTORED';compiled_origin_run='42508059413';export_run='42511115716';input_audit_sha256='DB0DC33D00DFA620AF5EA2F8B26557C999731F871F85A67A5985E4ECCE866AF0';verified_files=$actual.Count;shared_project_modified=$false;compiled_output_modified=$false;restored_paths=$paths}|ConvertTo-Json -Depth 5),$utf8)
Write-Host 'EXACT_ORIGINAL_ISOLATED_INPUT_RESTORED; shared project and compiled output untouched'
