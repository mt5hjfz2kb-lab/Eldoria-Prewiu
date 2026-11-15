param(
    [Parameter(Mandatory=$true)][string]$SourceRoot,
    [Parameter(Mandatory=$true)][string]$OutputRoot,
    [Parameter(Mandatory=$true)][string]$NativeLog
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

if(-not $env:UNITY_EDITOR_PATH -or -not (Test-Path $env:UNITY_EDITOR_PATH)){
    throw 'ASHEN_UNITY_EDITOR_UNAVAILABLE: UNITY_EDITOR_PATH does not resolve'
}
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count -gt 0){
    throw 'ASHEN_WINDOWS_BUSY: refusing to compete with another Unity or Blender instance'
}
$drive=Get-PSDrive -Name C -ErrorAction Stop
$freeGiB=[math]::Round($drive.Free/1GB,2)
if($freeGiB -lt 20){throw "ASHEN_NO_DISK_SPACE: only $freeGiB GiB on C; require >=20"}
Write-Output "ASHEN_WINDOWS_DISK_PASS $freeGiB GiB"

$root=Join-Path $env:RUNNER_TEMP ("ashen-citadel-"+$env:GITHUB_RUN_ID)
$project=Join-Path $root 'UnityExperiment'
if(Test-Path $root){throw "Scratch root already exists; refuse destructive overwrite: $root"}
New-Item -ItemType Directory -Force -Path $project,$OutputRoot | Out-Null
$src=Join-Path $SourceRoot 'Unity'

# Only 3, independent domain/app/infrastructure trees and authored art. No production HUD,
# world geometry, runner caches or full multi-GiB Unity project checkout is copied.
$sourceFolders=@(
    'Assets/Eldoria/Scripts/Domain',
    'Assets/Eldoria/Scripts/Application',
    'Assets/Eldoria/Scripts/Infrastructure',
    'Assets/Eldoria/ExperimentalAshenCitadel',
    'ProjectSettings'
)
foreach($rel in $sourceFolders){
    $from=Join-Path $src ($rel.Replace('/',[IO.Path]::DirectorySeparatorChar))
    $to=Join-Path $project ($rel.Replace('/',[IO.Path]::DirectorySeparatorChar))
    if(-not (Test-Path $from)){throw "Required source tree missing: $from"}
    New-Item -ItemType Directory -Force -Path (Split-Path $to) | Out-Null
    Copy-Item -LiteralPath $from -Destination $to -Recurse -Force
}
# Share the exact tested mobile renderer and GUIDs with the original game,
# not another improvised shader/rendering pipeline.
$relPaths=@(
    'Assets/Eldoria/Content/EldoriaMobileURP.asset',
    'Assets/Eldoria/Content/EldoriaMobileURP.asset.meta',
    'Assets/Eldoria/Content/EldoriaForwardRenderer.asset',
    'Assets/Eldoria/Content/EldoriaForwardRenderer.asset.meta',
    'Assets/UniversalRenderPipelineGlobalSettings.asset',
    'Assets/UniversalRenderPipelineGlobalSettings.asset.meta',
    'Packages/manifest.json'
)
foreach($rel in $relPaths){
    $from=Join-Path $src ($rel.Replace('/',[IO.Path]::DirectorySeparatorChar))
    $to=Join-Path $project ($rel.Replace('/',[IO.Path]::DirectorySeparatorChar))
    if(-not(Test-Path $from)){throw "Missing original URP project dependency: $rel"}
    New-Item -ItemType Directory -Force -Path (Split-Path $to) | Out-Null
    Copy-Item -LiteralPath $from -Destination $to -Force
}
$manifestPath=Join-Path $project 'Packages/manifest.json'
$oldManifest=Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$dependencies=[ordered]@{}
$keep=@('com.unity.render-pipelines.universal','com.unity.ugui',
        'com.unity.inputsystem','com.unity.mathematics','com.unity.test-framework')
foreach($item in $oldManifest.dependencies.PSObject.Properties){
    if($item.Name.StartsWith('com.unity.modules.') -or $keep -contains $item.Name){
        $dependencies[$item.Name]=$item.Value
    }
}
foreach($required in $keep){
    if(-not $dependencies.Contains($required)){
        throw "Required Unity Package is absent from original manifest: $required"
    }
}
@{dependencies=$dependencies} | ConvertTo-Json -Depth 15 |
    Set-Content -LiteralPath $manifestPath -Encoding UTF8
Write-Output ('ASHEN_SPARSE_UNITY_PACKAGES '+($dependencies.Keys -join ', '))

# The source branch builds exactly one scene in an isolated editor project.
$argsLine=@(
    '-batchmode','-quit','-force-d3d11',
    '-projectPath','"'+$project+'"',
    '-executeMethod','Eldoria.ExperimentalAshenCitadel.Editor.AshenCitadelSceneBuilder.BuildAndCapture',
    '-logFile','"'+$NativeLog+'"'
) -join ' '
Write-Output 'ASHEN_UNITY_REAL_EDITOR_BEGIN'
$process=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $argsLine -PassThru
$deadline=(Get-Date).AddMinutes(34)
while(-not $process.HasExited -and (Get-Date) -lt $deadline){
    Start-Sleep -Seconds 4
    $process.Refresh()
}
if(-not $process.HasExited){
    & taskkill.exe /PID $process.Id /T /F | Out-Null
    throw 'ASHEN_EDITOR_TIMEOUT: native render did not finish before hard gate'
}
$process.Refresh()
Copy-Item -LiteralPath $NativeLog -Destination (Join-Path $OutputRoot 'unity-editor.log') -ErrorAction SilentlyContinue
if($process.ExitCode -ne 0){
    if(Test-Path $NativeLog){Get-Content -Path $NativeLog -Tail 120}
    throw "ASHEN_EDITOR_FAILED: native Unity returned $($process.ExitCode)"
}
$scene=Join-Path $project 'Assets/Eldoria/ExperimentalAshenCitadel/Scenes/AshenCitadel.unity'
if(-not(Test-Path $scene) -or (Get-Item $scene).Length -lt 10000){
    throw 'ASHEN_NO_REAL_UNITY_SCENE: scene not saved or empty'
}
$photos=Join-Path $project 'ArtEvidence'
$hashes=@()
foreach($size in @('390x844','1600x900')){
    foreach($level in 1..5){
        $filename="Ashen_Bastion_${level}_${size}.png"
        $png=Join-Path $photos $filename
        if(-not(Test-Path $png) -or (Get-Item $png).Length -lt 10000){
            throw "ASHEN_GENUINE_UNITY_PNG_MISSING: $filename"
        }
        $bytes=[IO.File]::ReadAllBytes($png)
        if(-not($bytes[0] -eq 137 -and $bytes[1] -eq 80 -and $bytes[2] -eq 78 -and $bytes[3] -eq 71)){
            throw "ASHEN_CORRUPT_PNG: $filename"
        }
        $hash=(Get-FileHash -LiteralPath $png -Algorithm SHA256).Hash
        $hashes+=@{file=$filename;sha256=$hash;size_bytes=$bytes.Length}
    }
}
if(($hashes|Where-Object {$_.file -like '*390x844.png'} |
    Select-Object -ExpandProperty sha256 -Unique).Count -ne 5){
    throw 'ASHEN_STAGE_VARIANTS_NOT_DISTINCT: same renderer output in portrait'
}
$art=Join-Path $project 'Assets/Eldoria/ExperimentalAshenCitadel'
Copy-Item -LiteralPath $scene -Destination (Join-Path $OutputRoot 'AshenCitadel.unity') -Force
Copy-Item -LiteralPath $photos -Destination (Join-Path $OutputRoot 'UnityScreenshots') -Recurse -Force
# Save ALL Unity-generated import metadata and editable material assets so the
# generated scene can later be promoted in the same experiment without GUID loss.
Copy-Item -LiteralPath $art -Destination (Join-Path $OutputRoot 'UnityExperimentSource') -Recurse -Force
@{source_sha=$env:GITHUB_SHA;scene_saved=$true;screenshots=$hashes;
    project='Isolated URP Unity experiment';certification='editor-camera-only';
    webgl_playmode=$false;physical_iphone=$false}|
    ConvertTo-Json -Depth 8 | Set-Content -Path (Join-Path $OutputRoot 'native-proof.json') -Encoding UTF8
Write-Output "ASHEN_REAL_UNITY_SOURCE_PASS: 5 stages, 10 verified screenshots; no gameplay PlayMode claim."
