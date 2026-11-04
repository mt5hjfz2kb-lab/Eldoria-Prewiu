$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'native_process.ps1')
$processScratch=Join-Path ([IO.Path]::GetTempPath()) ('metadata-process-contract-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $processScratch | Out-Null
$processHost=(Get-Process -Id $PID).Path
$observed=@()
try {
    foreach($expected in @(0,7)){
        $child="Write-Output 'metadata-probe-out'; [Console]::Error.WriteLine('metadata-probe-err'); Start-Sleep -Milliseconds 500; exit $expected"
        $encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($child))
        $stdout=Join-Path $processScratch ("stdout-$expected.txt")
        $stderr=Join-Path $processScratch ("stderr-$expected.txt")
        $result=Invoke-MetadataLoggedProcess $processHost @('-NoLogo','-NoProfile','-NonInteractive','-OutputFormat','Text','-EncodedCommand',$encoded) $processScratch $stdout $stderr 30000
        if($result.exit_code -isnot [int] -or $result.exit_code -ne $expected -or ($result.windows -and -not $result.handle_retained)){throw "Incorrect actual process exit code: expected $expected actual=$($result.exit_code)"}
        if(-not (Get-Content -LiteralPath $stdout -Raw).Contains('metadata-probe-out') -or -not (Get-Content -LiteralPath $stderr -Raw).Contains('metadata-probe-err')){throw 'Redirected process streams missing'}
        $observed+=@{expected=$expected;actual=$result.exit_code;windows=$result.windows;handle_retained=$result.handle_retained}
    }
    Write-Host 'Logged process contract: real exit0/exit7 and both redirected streams PASS'
    return @{state='PASS';shell_version=$PSVersionTable.PSVersion.ToString();cases=$observed}
} finally {Remove-Item -LiteralPath $processScratch -Recurse -Force}
