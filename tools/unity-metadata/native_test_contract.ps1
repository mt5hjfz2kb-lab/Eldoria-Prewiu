function Set-MetadataPlayModeFixture([string]$EvidenceRoot) {
    $captureDirectory=Join-Path $EvidenceRoot 'region1-captures'
    if(Test-Path -LiteralPath $captureDirectory){throw 'Refuse to mix prior native captures'}
    New-Item -ItemType Directory -Path $captureDirectory | Out-Null
    $env:VALORIA_GAMEPLAY_PROBE='1'
    $env:ELDORIA_REGION1_CAPTURE_OUT=$captureDirectory
    return @{probe_enabled=$true;capture_directory=$captureDirectory}
}

function Assert-MetadataTestResult([string]$ResultPath,[string]$Platform,[string]$ProjectRoot) {
    if($Platform -notin @('EditMode','PlayMode')){throw 'Unknown native test platform'}
    [xml]$xml=Get-Content -LiteralPath $ResultPath -Raw
    $run=$xml.'test-run'
    if($run.result -ne 'Passed' -or [int]$run.failed -ne 0 -or [int]$run.passed -lt 1){
        throw "$Platform NUnit result is not passing"
    }
    $expectedRoot=([IO.Path]::GetFullPath((Join-Path $ProjectRoot 'Library/ScriptAssemblies'))).Replace([char]92,[char]47).TrimEnd('/')+'/'
    $assemblies=@($xml.SelectNodes("//test-suite[@type='Assembly']"))
    if(-not $assemblies.Count){throw 'No native test assembly source identity'}
    foreach($assembly in $assemblies){
        $actual=([string]$assembly.fullname).Replace([char]92,[char]47)
        if(-not $actual.StartsWith($expectedRoot,[StringComparison]::OrdinalIgnoreCase)){
            throw "Tests came from another project: $actual"
        }
    }
    $cases=@($xml.SelectNodes('//test-case'))
    if(-not $cases.Count -or @($cases|Where-Object {$_.result -ne 'Passed'}).Count){
        throw 'Native cases must all be Passed; skips are not certification'
    }
    if($Platform -eq 'PlayMode' -and $cases.Count -lt 61){
        throw 'Expected complete canonical 61+ PlayMode cases'
    }
    return @{platform=$Platform;passed=[int]$run.passed;failed=[int]$run.failed;
        skipped=[int]$run.skipped;cases=$cases.Count;assemblies=$assemblies.Count;source_verified=$true}
}
