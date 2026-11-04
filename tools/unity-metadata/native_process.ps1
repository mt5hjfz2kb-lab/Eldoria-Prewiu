function Invoke-MetadataLoggedProcess([string]$FilePath,[string[]]$Arguments,[string]$WorkingDirectory,[string]$StdoutPath,[string]$StderrPath,[int]$TimeoutMilliseconds) {
    $quoted=@($Arguments | ForEach-Object {
        if($_.Contains('"')){throw 'Unexpected embedded quote in fixed native argument'}
        if($_ -match '\s' -or $_.Length -eq 0){'"'+$_+'"'}else{$_}
    })
    $process=Start-Process -FilePath $FilePath -ArgumentList $quoted -WorkingDirectory $WorkingDirectory -PassThru -RedirectStandardOutput $StdoutPath -RedirectStandardError $StderrPath
    # Windows PowerShell 5.1 / PowerShell issue5421: redirected Start-Process
    # can lose ExitCode unless its handle is cached while the process is alive.
    $windowsProcess=$env:OS -eq 'Windows_NT'
    $retainedHandle=if($windowsProcess){$process.Handle}else{[IntPtr]::Zero}
    if(-not $process.WaitForExit($TimeoutMilliseconds)){
        if($env:OS -eq 'Windows_NT'){& taskkill.exe /PID $process.Id /T /F | Out-Null}else{$process.Kill()}
        throw 'Only this logged native process timed out'
    }
    $process.WaitForExit()
    $process.Refresh()
    $exitCode=$process.ExitCode
    if($null -eq $exitCode){throw 'OS process exit code unavailable; refuse inferred success'}
    $result=@{exit_code=$exitCode;process_id=$process.Id;windows=$windowsProcess;handle_retained=($retainedHandle -ne [IntPtr]::Zero)}
    $process.Dispose()
    return $result
}
