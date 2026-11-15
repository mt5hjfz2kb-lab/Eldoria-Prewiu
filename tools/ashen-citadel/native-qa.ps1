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
$manifestJson=@{dependencies=$dependencies}|ConvertTo-Json -Depth 15
# Windows PowerShell 5.1 Set-Content -Encoding UTF8 emits U+FEFF BOM.
# Unity 6000 package manager rejects that BOM as invalid JSON at character 0.
[IO.File]::WriteAllText($manifestPath,$manifestJson,
    ([System.Text.UTF8Encoding]::new($false)))
$bytes=[IO.File]::ReadAllBytes($manifestPath)
if($bytes[0] -ne 123){throw 'Unity manifest must begin with ASCII { (UTF-8 no BOM)'}
Write-Output ('ASHEN_SPARSE_UNITY_PACKAGES '+($dependencies.Keys -join ', '))

# The source branch builds exactly one scene in an isolated editor project.
# DO NOT build the command with a PowerShell array and embedded +:
# it previously inserted spaces INSIDE quoted paths, and Unity exited 127.
$quotedProject='"'+$project+'"'
$quotedLog='"'+$NativeLog+'"'
$argsLine='-batchmode -quit -force-d3d11 -projectPath '+$quotedProject+
    ' -executeMethod Eldoria.ExperimentalAshenCitadel.Editor.AshenCitadelSceneBuilder.BuildAndCapture -logFile '+$quotedLog
if($argsLine -match '"\s+[A-Za-z]:\\'){throw 'Invalid leading path whitespace in Unity command'}
if($argsLine -match '[A-Za-z]:[^"]+\s+"'){throw 'Invalid trailing path whitespace in Unity command'}
Write-Output 'ASHEN_UNITY_REAL_EDITOR_BEGIN'
Write-Output ('ASHEN_EDITOR_EXE_NAME '+[IO.Path]::GetFileName($env:UNITY_EDITOR_PATH))
Write-Output ('ASHEN_EDITOR_EXE_SIZE '+(Get-Item $env:UNITY_EDITOR_PATH).Length)
Write-Output ('ASHEN_EDITOR_CMD '+$argsLine)
$stdout=Join-Path $OutputRoot 'unity-stdout.txt'
$stderr=Join-Path $OutputRoot 'unity-stderr.txt'
$process=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $argsLine -PassThru `
    -RedirectStandardOutput $stdout -RedirectStandardError $stderr
$deadline=(Get-Date).AddMinutes(34)
while(-not $process.HasExited -and (Get-Date) -lt $deadline){
    Start-Sleep -Seconds 4
    $process.Refresh()
}
if(-not $process.HasExited){
    & taskkill.exe /PID $process.Id /T /F | Out-Null
    throw 'ASHEN_EDITOR_TIMEOUT: native render did not finish before hard gate'
}
$process.WaitForExit()
$process.Refresh()
# Some Windows PowerShell 5.1 runs show a null ExitCode even when Unity logs
# an authentic exit 0. Fail closed unless this run contains a real saved scene,
# all 10 captures, and the two exact Unity completion markers.
$nativeExit=$process.ExitCode
if($null -eq $nativeExit){
    $successLog=(Test-Path $NativeLog)
    if($successLog){
        $successLog=(Select-String -LiteralPath $NativeLog -Pattern 'Application will terminate with return code 0' -Quiet) -and
                    (Select-String -LiteralPath $NativeLog -Pattern 'ASHEN_CAPTURE_COMPLETE 10 Unity-camera PNG' -Quiet)
    }
    $sceneProof=Join-Path $project 'Assets/Eldoria/ExperimentalAshenCitadel/Scenes/AshenCitadel.unity'
    $imagesProof=Join-Path $project 'ArtEvidence'
    $allImages=Test-Path $imagesProof
    if($allImages){
        foreach($size in @('390x844','1600x900')){
            foreach($level in 1..5){
                $candidate=Join-Path $imagesProof ("Ashen_Bastion_"+$level+"_"+$size+".png")
                if(-not (Test-Path $candidate) -or (Get-Item $candidate).Length -lt 10000){$allImages=$false}
            }
        }
    }
    if($successLog -and (Test-Path $sceneProof) -and $allImages){
        $nativeExit=0
        Write-Output 'ASHEN_EXIT_RECONCILED_SAME_RUN_UNITY_ZERO_LOG_AND_10_REAL_PNG'
    }else{
        $nativeExit=-997
        Write-Output 'ASHEN_EXIT_UNKNOWN_NO_ARTIFACT_PROOF_FAIL_CLOSED'
    }
}
Write-Output "ASHEN_EDITOR_EXIT_VERIFIED $nativeExit"
if(-not(Test-Path $NativeLog)){
    # Never copy the user's previous Unity Editor.log. On this Windows PC the
    # unrelated historical file exceeded 4 GiB; it is NOT experiment evidence.
    Write-Output 'ASHEN_NATIVE_LOG_NOT_WRITTEN; previous Editor.log intentionally ignored'
}
if($nativeExit -ne 0){
    foreach($log in @($NativeLog,$stdout,$stderr)){
        if(Test-Path $log){
            Write-Output ('ASHEN_DIAGNOSTIC_LOG '+([IO.Path]::GetFileName($log)))
            Get-Content -LiteralPath $log -Tail 85
        }
    }
    @{source_sha=$env:GITHUB_SHA;status='UNITY_PROCESS_FAILED';
      exit_code=$nativeExit; editor_version='Unity6000.3.23f1';
      evidence='Only this run scratch & captured process diagnostics';
      scene_saved=$false}|ConvertTo-Json -Depth 6 |
      Set-Content -Path (Join-Path $OutputRoot 'native-failed.json') -Encoding UTF8
    throw "ASHEN_EDITOR_FAILED: native Unity returned $nativeExit"
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
# PowerShell 5.1 Select-Object -ExpandProperty does NOT expose Hashtable keys.
# Use explicit key indexing; the previous expression blocked delivery AFTER all
# ten actual Unity screenshots were created and the Editor exited successfully.
$portraitHashes=@($hashes | Where-Object {$_['file'] -like '*390x844.png'} |
    ForEach-Object {$_['sha256']} | Sort-Object -Unique)
if($portraitHashes.Count -ne 5){
    throw "ASHEN_STAGE_VARIANTS_NOT_DISTINCT: expected 5, found $($portraitHashes.Count)"
}
# EditMode's Unity Test Framework runs against the exact same imported
# isolated scene, with real LocalGateway progression and persistence.
# A successful .obj import/Editor camera photo alone never proves gameplay.
$testLog=Join-Path $OutputRoot 'unity-nunit.log'
$testResults=Join-Path $OutputRoot 'Ashen-EditMode-Results.xml'
$testArgs='-batchmode -force-d3d11 -projectPath '+$quotedProject+
    ' -runTests -testPlatform editmode -testFilter Eldoria.ExperimentalAshenCitadel.Tests.AshenCitadelProgressionTests -testResults "'+$testResults+
    '" -logFile "'+$testLog+'"'
Write-Output ('ASHEN_NUNIT_UNITY_BEGIN '+$testArgs)
$testStd=Join-Path $OutputRoot 'nunit-stdout.txt'
$testErr=Join-Path $OutputRoot 'nunit-stderr.txt'
$testProcess=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $testArgs -PassThru `
    -RedirectStandardOutput $testStd -RedirectStandardError $testErr
$testDeadline=(Get-Date).AddMinutes(19)
while(-not $testProcess.HasExited -and (Get-Date) -lt $testDeadline){
    Start-Sleep -Seconds 4
    $testProcess.Refresh()
}
if(-not $testProcess.HasExited){
    & taskkill.exe /PID $testProcess.Id /T /F | Out-Null
    throw 'ASHEN_NUNIT_TIMEOUT: actual Unity EditMode suite failed to complete'
}
$testProcess.WaitForExit()
$testProcess.Refresh()
if(-not(Test-Path $testResults)){
    if(Test-Path $testLog){Get-Content $testLog -Tail 110}
    throw 'ASHEN_NUNIT_RESULTS_MISSING: no real Unity test XML'
}
[xml]$xml=Get-Content -LiteralPath $testResults -Raw
$rootNode=$xml.DocumentElement
if($null -eq $rootNode -or $rootNode.Name -ne 'test-run'){
    throw 'ASHEN_NUNIT_INVALID_RESULTS: XML has no test-run root'
}
$completed=[int]$rootNode.GetAttribute('total')
$passed=[int]$rootNode.GetAttribute('passed')
$failed=[int]$rootNode.GetAttribute('failed')
$skipped=[int]$rootNode.GetAttribute('skipped')
if($null -ne $testProcess.ExitCode -and $testProcess.ExitCode -ne 0){
    throw "ASHEN_NUNIT_EDITOR_ERROR: Unity returned $($testProcess.ExitCode)"
}
if($completed -lt 7 -or $failed -ne 0 -or $skipped -ne 0 -or
   $passed -ne $completed -or $rootNode.GetAttribute('result') -ne 'Passed'){
    Write-Output "ASHEN_NUNIT_GATE_FAIL total=$completed passed=$passed failed=$failed skipped=$skipped"
    if(Test-Path $testLog){Get-Content $testLog -Tail 110}
    throw 'ASHEN_NUNIT_GAMEPLAY_REGRESSION_OR_MISSING_TESTS'
}
Write-Output "ASHEN_NUNIT_REAL_PASS total=$completed passed=$passed failed=$failed skipped=$skipped"

# The second phase runs the ACTUAL Unity scene in Editor PlayMode, not a
# camera-only render. It presses real Unity UI Button.onClick callbacks,
# verifies the starter kingdom hides unbuilt buildings, and switches World/City.
$uiLog=Join-Path $OutputRoot 'unity-playmode.log'
$uiResults=Join-Path $OutputRoot 'Ashen-PlayMode-Results.xml'
$uiArgs='-batchmode -force-d3d11 -projectPath '+$quotedProject+
    ' -runTests -testPlatform playmode -testFilter Eldoria.ExperimentalAshenCitadel.Tests.AshenCitadelPlayableTests -testResults "'+$uiResults+
    '" -logFile "'+$uiLog+'"'
Write-Output ('ASHEN_PLAYMODE_UNITY_BEGIN '+$uiArgs)
$uiStd=Join-Path $OutputRoot 'playmode-stdout.txt'
$uiErr=Join-Path $OutputRoot 'playmode-stderr.txt'
$uiProc=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $uiArgs -PassThru `
    -RedirectStandardOutput $uiStd -RedirectStandardError $uiErr
$uiDeadline=(Get-Date).AddMinutes(17)
while(-not $uiProc.HasExited -and (Get-Date) -lt $uiDeadline){
    Start-Sleep -Seconds 4
    $uiProc.Refresh()
}
if(-not $uiProc.HasExited){
    & taskkill.exe /PID $uiProc.Id /T /F | Out-Null
    throw 'ASHEN_PLAYMODE_TIMEOUT'
}
$uiProc.WaitForExit()
$uiProc.Refresh()
if(-not(Test-Path $uiResults)){
    if(Test-Path $uiLog){Get-Content $uiLog -Tail 100}
    throw 'ASHEN_PLAYMODE_XML_MISSING: no real Unity scene test result'
}
[xml]$uiXml=Get-Content -LiteralPath $uiResults -Raw
$uiRoot=$uiXml.DocumentElement
if($null -eq $uiRoot -or $uiRoot.Name -ne 'test-run'){
    throw 'ASHEN_PLAYMODE_INVALID_XML'
}
$uiTotal=[int]$uiRoot.GetAttribute('total')
$uiPassed=[int]$uiRoot.GetAttribute('passed')
$uiFailed=[int]$uiRoot.GetAttribute('failed')
$uiSkipped=[int]$uiRoot.GetAttribute('skipped')
if($null -ne $uiProc.ExitCode -and $uiProc.ExitCode -ne 0){
    if(Test-Path $uiLog){Get-Content $uiLog -Tail 100}
    throw "ASHEN_PLAYMODE_EDITOR_FAILURE: Unity returned $($uiProc.ExitCode)"
}
if($uiTotal -lt 3 -or $uiPassed -ne $uiTotal -or $uiFailed -ne 0 -or
   $uiSkipped -ne 0 -or $uiRoot.GetAttribute('result') -ne 'Passed'){
    Write-Output "ASHEN_PLAYMODE_GATE_FAIL total=$uiTotal passed=$uiPassed failed=$uiFailed skipped=$uiSkipped"
    if(Test-Path $uiLog){Get-Content $uiLog -Tail 100}
    throw 'ASHEN_PLAYMODE_SCENE_NAVIGATION_REGRESSION'
}
Write-Output "ASHEN_PLAYMODE_REAL_PASS total=$uiTotal passed=$uiPassed failed=$uiFailed skipped=$uiSkipped"

$art=Join-Path $project 'Assets/Eldoria/ExperimentalAshenCitadel'
Copy-Item -LiteralPath $scene -Destination (Join-Path $OutputRoot 'AshenCitadel.unity') -Force
Copy-Item -LiteralPath $photos -Destination (Join-Path $OutputRoot 'UnityScreenshots') -Recurse -Force
# Save ALL Unity-generated import metadata and editable material assets so the
# generated scene can later be promoted in the same experiment without GUID loss.
Copy-Item -LiteralPath $art -Destination (Join-Path $OutputRoot 'UnityExperimentSource') -Recurse -Force
@{source_sha=$env:GITHUB_SHA;scene_saved=$true;screenshots=$hashes;
    project='Isolated URP Unity experiment';certification='editor-camera-plus-real-unity-editmode-and-playmode';
    unity_editmode_tests=$passed;unity_playmode_tests=$uiPassed;
    webgl_playmode=$false;editor_playmode=$true;physical_iphone=$false}|
    ConvertTo-Json -Depth 8 | Set-Content -Path (Join-Path $OutputRoot 'native-proof.json') -Encoding UTF8
Write-Output "ASHEN_REAL_UNITY_SOURCE_PASS: 5 stages, 10 camera PNGs, 7 EditMode assertions, 3 live PlayMode UI tests; mobile/WebGL not yet certified."
