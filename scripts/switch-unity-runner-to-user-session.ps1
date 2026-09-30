param(
    [string]$RunnerRoot = "C:\actions-runner-eldoria",
    [string]$TaskName = "EldoriaUnityRunner"
)

$ErrorActionPreference = "Stop"

function Assert-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Open Windows PowerShell with Run as administrator and run this script again."
    }
}

Assert-Administrator

if (-not (Test-Path (Join-Path $RunnerRoot "run.cmd"))) {
    throw "Runner not found at $RunnerRoot"
}

$serviceFile = Join-Path $RunnerRoot ".service"
if (Test-Path $serviceFile) {
    $serviceName = (Get-Content $serviceFile -Raw).Trim()
    $svc = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($svc) {
        if ($svc.Status -ne "Stopped") { Stop-Service -Name $serviceName -Force }
        & sc.exe delete $serviceName | Out-Null
        Start-Sleep -Seconds 2
    }
}

$userId = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
$watchdogPath = Join-Path $RunnerRoot "eldoria-runner-watchdog.ps1"
$watchdog = @'
$ErrorActionPreference = "Continue"
$runnerRoot = '__RUNNER_ROOT__'
while ($true) {
    try {
        Push-Location $runnerRoot
        & (Join-Path $runnerRoot "run.cmd")
    }
    catch {
        Write-Warning "Eldoria runner stopped: $_"
    }
    finally {
        if ((Get-Location).Path -eq $runnerRoot) { Pop-Location }
    }
    Start-Sleep -Seconds 5
}
'@
$watchdog = $watchdog.Replace('__RUNNER_ROOT__', $RunnerRoot.Replace("'", "''"))
Set-Content -Path $watchdogPath -Value $watchdog -Encoding UTF8

$action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$watchdogPath`"" -WorkingDirectory $RunnerRoot
$logonTrigger = New-ScheduledTaskTrigger -AtLogOn -User $userId
# A stopped interactive task otherwise stays down until the next sign-in.
# IgnoreNew prevents the periodic trigger from starting a second listener.
$recoveryTrigger = New-ScheduledTaskTrigger -Once -At (Get-Date).AddMinutes(1) -RepetitionInterval (New-TimeSpan -Minutes 5) -RepetitionDuration (New-TimeSpan -Days 365)
$principal = New-ScheduledTaskPrincipal -UserId $userId -LogonType Interactive -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -RestartCount 999 -RestartInterval (New-TimeSpan -Minutes 1) -ExecutionTimeLimit (New-TimeSpan -Seconds 0) -MultipleInstances IgnoreNew
Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger @($logonTrigger, $recoveryTrigger) -Principal $principal -Settings $settings -Force | Out-Null
Start-ScheduledTask -TaskName $TaskName
Start-Sleep -Seconds 3

$task = Get-ScheduledTask -TaskName $TaskName
Write-Host ""
Write-Host "Eldoria runner switched to the logged-in Windows user." -ForegroundColor Green
Write-Host "User: $userId"
Write-Host "Task: $TaskName"
Write-Host "State: $($task.State)"
Write-Host ""
Write-Host "Unity batch jobs will now use the same Windows profile/license as the interactive Unity Editor."
Write-Host "A watchdog will restart the runner automatically if run.cmd exits unexpectedly."
Write-Host "The scheduled task will also retry every five minutes if the watchdog itself stops."
