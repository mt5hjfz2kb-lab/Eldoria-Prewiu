param([Parameter(Mandatory=$true)][string]$SourceRoot)
$ErrorActionPreference='Stop'
$project=Join-Path $SourceRoot 'Unity'
function Invoke-Unity([string]$name,[string[]]$arguments,[int]$timeoutMinutes=20) {
    $log=Join-Path $env:RUNNER_TEMP ('valoria25d-'+$name+'-'+$env:GITHUB_RUN_ID+'.log')
    $arguments+=@('-logFile',$log)
    Write-Host ('VALORIA25D_BEGIN '+$name)
    $proc=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $arguments -PassThru
    if(-not $proc.WaitForExit($timeoutMinutes*60000)){
        & taskkill.exe /PID $proc.Id /T /F | Out-Null
        throw ('Unity '+$name+' timed out')
    }
    if($proc.ExitCode -ne 0){
        if(Test-Path $log){Get-Content -LiteralPath $log -Tail 200}
        throw ('Unity '+$name+' failed with exit '+$proc.ExitCode)
    }
    Write-Host ('VALORIA25D_NATIVE_STEP_PASS '+$name)
}
function Assert-Xml([string]$file,[int]$minTotal,[string]$label) {
    if(-not(Test-Path -LiteralPath $file)){
        throw ('Missing test report '+$label)
    }
    [xml]$x=Get-Content -LiteralPath $file
    $total=[int]$x.DocumentElement.GetAttribute('total')
    $passed=[int]$x.DocumentElement.GetAttribute('passed')
    $failed=[int]$x.DocumentElement.GetAttribute('failed')
    if($total -lt $minTotal -or $failed -ne 0 -or $total -ne $passed) {
        throw ("Native $label tests failed total=$total passed=$passed failed=$failed")
    }
    Write-Host ("VALORIA25D_NATIVE_NUNIT_PASS $label total=$total")
}
if(-not(Test-Path (Join-Path $project 'Assets/Valoria25D/Scenes/Valoria25D_PremiumV6_Source.unity'))){
    throw 'No selectively migrated V6 Unity scene'
}
$sourceMeta=Join-Path $project 'Assets/Valoria25D/Scenes/Valoria25D_PremiumV6_Source.unity.meta'
if(-not(Test-Path $sourceMeta)){throw 'Unity premium V6 scene .meta missing'}
$metaText=Get-Content -LiteralPath $sourceMeta -Raw
$metaLines=@(Get-Content -LiteralPath $sourceMeta | ForEach-Object { $_.Trim() })
# Git's core.autocrlf on Windows may preserve CRLF; matching raw YAML lines
# with $ anchors fails on CRLF. Validate normalized records instead.
$validGuid=$metaLines -contains 'guid: ec837f59b040445da407785c6f670ecd'
$validImporter=$metaLines -contains 'DefaultImporter:'
$validHeader=$metaLines.Count -ge 7 -and $metaLines[0] -eq 'fileFormatVersion: 2'
$literalBackslashN=$metaText.Contains([char]92 + 'n')
if(-not $validGuid -or -not $validImporter -or -not $validHeader -or $literalBackslashN){
    Write-Host ('VALORIA25D_META_DIAG lines='+$metaLines.Count+
      ' guid='+$validGuid+' importer='+$validImporter+
      ' header='+$validHeader+' literalEscapes='+$literalBackslashN)
    throw 'Malformed scene GUID YAML .meta; abort before long Unity import'
}
Write-Host 'VALORIA25D_V6_SCENE_META_VALID_GUID_YAML_PASS'
Invoke-Unity 'scene' @('-batchmode','-quit','-force-d3d11','-projectPath',$project,
    '-executeMethod','Eldoria.Valoria25D.Editor.City25DGameplaySceneBuilder.Build') 18
if(-not(Test-Path (Join-Path $project 'Assets/Valoria25D/Scenes/Valoria25D_Gameplay.unity'))){
    throw 'No authentic Unity gameplay scene authored'
}
$gatewayXml=Join-Path $env:RUNNER_TEMP ('valoria25d-gateway-'+$env:GITHUB_RUN_ID+'.xml')
Invoke-Unity 'gateway' @('-batchmode','-force-d3d11','-projectPath',$project,
    '-runTests','-testPlatform','EditMode',
    '-testFilter','Eldoria.Valoria25D.Tests.City25DGatewayIntegrationTests',
    '-testResults',$gatewayXml) 15
Assert-Xml $gatewayXml 6 'canonical-gateway'
$env:VALORIA25D_QA_SAVE_PATH=Join-Path $env:RUNNER_TEMP ('valoria25d-qa-'+$env:GITHUB_RUN_ID+'.json')
if(Test-Path $env:VALORIA25D_QA_SAVE_PATH){throw 'Previous QA save exists'}
$playXml=Join-Path $env:RUNNER_TEMP ('valoria25d-playmode-'+$env:GITHUB_RUN_ID+'.xml')
Invoke-Unity 'playmode' @('-batchmode','-force-d3d11','-projectPath',$project,
    '-runTests','-testPlatform','EditMode',
    '-testFilter','Eldoria.Valoria25D.Tests.City25DScenePlayModeProof',
    '-testResults',$playXml) 15
Assert-Xml $playXml 2 'city-plus-existing-world-playmode'
$proof=Join-Path $project 'Valoria25DGameplayEvidence/evidence.json'
if(-not(Test-Path $proof)){throw 'Native Unity screenshot manifest missing'}
$captures=Get-Content -LiteralPath $proof -Raw | ConvertFrom-Json
if($captures.engine -ne 'Unity' -or -not $captures.real_gateway_and_save -or
   @($captures.captures).Count -lt 3){throw 'Unity screenshot proof invalid'}
foreach($item in $captures.captures){
    $png=Join-Path (Split-Path $proof -Parent) $item.file
    if(-not(Test-Path $png)){throw ('Missing native Unity frame '+$item.file)}
    $sha=(Get-FileHash $png -Algorithm SHA256).Hash.ToLowerInvariant()
    if($sha -ne $item.sha256){throw ('Frame SHA mismatch '+$item.file)}
}
Write-Host 'VALORIA25D_NATIVE_SCENE_SCREENSHOT_SHA_PASS'
Invoke-Unity 'webgl' @('-batchmode','-quit','-buildTarget','WebGL',
    '-projectPath',$project,
    '-executeMethod','Eldoria.Valoria25D.Editor.City25DCandidateBuild.WebGL') 24
$built=Join-Path $project 'Valoria25D_CandidateWebGL'
if(-not(Test-Path (Join-Path $built 'index.html')) -or
   -not(Test-Path (Join-Path $built 'integration_manifest.json'))){
    throw 'No real WebGL candidate'
}
Write-Host 'VALORIA25D_REAL_GAMEPLAY_SCENE_PLAYMODE_WEBGL_ALL_PASS'
