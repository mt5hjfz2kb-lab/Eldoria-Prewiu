$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'native_test_contract.ps1')
$contractScratch=Join-Path ([IO.Path]::GetTempPath()) ('metadata-test-contract-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $contractScratch | Out-Null
$project=Join-Path $contractScratch 'Unity'
$assembly=Join-Path $project 'Library/ScriptAssemblies/Eldoria.Tests.dll'
$oldProbe=$env:VALORIA_GAMEPLAY_PROBE
$oldCapture=$env:ELDORIA_REGION1_CAPTURE_OUT

function Write-TestXml([string]$Name,[int]$Count,[string]$AssemblyPath,[bool]$SkipFirst=$false) {
    $nodes=@(for($i=0;$i -lt $Count;$i++){
        $state=if($SkipFirst -and $i -eq 0){'Skipped'}else{'Passed'}
        '<test-case name="case'+$i+'" result="'+$state+'" />'
    }) -join ''
    $passed=if($SkipFirst){$Count-1}else{$Count}
    $skipped=if($SkipFirst){1}else{0}
    $escaped=[Security.SecurityElement]::Escape($AssemblyPath)
    $content='<test-run result="Passed" passed="'+$passed+'" failed="0" skipped="'+$skipped+'"><test-suite type="Assembly" fullname="'+$escaped+'">'+$nodes+'</test-suite></test-run>'
    $file=Join-Path $contractScratch ($Name+'.xml')
    $content | Set-Content -LiteralPath $file -Encoding UTF8
    return $file
}
function Must-Reject([scriptblock]$Action,[string]$Label) {
    $rejected=$false
    try { & $Action | Out-Null } catch { $rejected=$true }
    if(-not $rejected){throw "Incorrect native acceptance: $Label"}
}
try {
    $edit=Write-TestXml 'edit' 1 $assembly
    $result=Assert-MetadataTestResult $edit 'EditMode' $project
    if($result.passed -ne 1 -or -not $result.source_verified){throw 'EditMode identity/count missing'}
    $play=Write-TestXml 'play' 61 $assembly
    $result=Assert-MetadataTestResult $play 'PlayMode' $project
    if($result.cases -ne 61 -or $result.skipped -ne 0){throw 'PlayMode completeness missing'}
    $skip=Write-TestXml 'skipped' 61 $assembly $true
    Must-Reject {Assert-MetadataTestResult $skip 'PlayMode' $project} 'ignored test hidden by Passed root'
    Must-Reject {Assert-MetadataTestResult $edit 'PlayMode' $project} 'incomplete PlayMode suite'
    $foreign=Write-TestXml 'foreign' 61 (Join-Path $contractScratch 'other-project/Library/ScriptAssemblies/Eldoria.Tests.dll')
    Must-Reject {Assert-MetadataTestResult $foreign 'PlayMode' $project} 'another project assembly'
    $failure=Join-Path $contractScratch 'failure.xml'
    (Get-Content -LiteralPath $play -Raw).Replace('result="Passed" passed="61" failed="0"','result="Failed" passed="60" failed="1"') | Set-Content -LiteralPath $failure
    Must-Reject {Assert-MetadataTestResult $failure 'PlayMode' $project} 'failed NUnit root'
    $empty=Join-Path $contractScratch 'empty.xml'
    '<test-run result="Passed" passed="1" failed="0" />' | Set-Content -LiteralPath $empty
    Must-Reject {Assert-MetadataTestResult $empty 'EditMode' $project} 'missing assembly evidence'
    $fixture=Set-MetadataPlayModeFixture $contractScratch
    if($env:VALORIA_GAMEPLAY_PROBE -ne '1' -or $env:ELDORIA_REGION1_CAPTURE_OUT -ne $fixture.capture_directory -or -not(Test-Path -LiteralPath $fixture.capture_directory)){
        throw 'Disposable native gameplay/capture fixture missing'
    }
    Must-Reject {Set-MetadataPlayModeFixture $contractScratch} 'mixing prior captures'
    Write-Host 'Native test contract: valid EditMode/61-case PlayMode, skipped/incomplete/foreign/failed/missing rejection and disposable fixture PASS'
} finally {
    $env:VALORIA_GAMEPLAY_PROBE=$oldProbe
    $env:ELDORIA_REGION1_CAPTURE_OUT=$oldCapture
    Remove-Item -LiteralPath $contractScratch -Recurse -Force
}
