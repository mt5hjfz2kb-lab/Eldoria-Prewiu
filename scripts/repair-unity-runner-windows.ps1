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

$unity = "C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe"
if (-not (Test-Path $unity)) { throw "Unity 6000.3.23f1 not found at $unity" }
if (-not (Test-Path (Join-Path $RunnerRoot "run.cmd"))) { throw "Runner not found at $RunnerRoot" }

[Environment]::SetEnvironmentVariable("UNITY_EDITOR_PATH", $unity, "Machine")
$env:UNITY_EDITOR_PATH = $unity

# A self-hosted runner cannot accept queued jobs while Windows is asleep.
& powercfg.exe /change standby-timeout-ac 0 | Out-Null
& powercfg.exe /change hibernate-timeout-ac 0 | Out-Null

$repaired = $false
$serviceFile = Join-Path $RunnerRoot ".service"
if (Test-Path $serviceFile) {
    $serviceName = (Get-Content $serviceFile -Raw).Trim()
    $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($service) {
        & sc.exe config $serviceName start= auto | Out-Null
        & sc.exe failure $serviceName reset= 0 actions= restart/5000/restart/10000/restart/30000 | Out-Null
        & sc.exe failureflag $serviceName 1 | Out-Null
        if ($service.Status -ne "Running") {
            Start-Service -Name $serviceName
            Start-Sleep -Seconds 3
        }
        $service = Get-Service -Name $serviceName
        Write-Host "SERVICE $serviceName $($service.Status)"
        $repaired = ($service.Status -eq "Running")
    }
}

$task = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if ($task) {
    if ($task.State -ne "Running") {
        Start-ScheduledTask -TaskName $TaskName
        Start-Sleep -Seconds 3
    }
    $task = Get-ScheduledTask -TaskName $TaskName
    Write-Host "TASK $TaskName $($task.State)"
    if ($task.State -eq "Running") { $repaired = $true }
}

if (-not $repaired) {
    throw "Runner installation exists, but neither the Windows service nor the interactive watchdog task could be started. Run scripts\setup-unity-runner-windows.ps1 or scripts\switch-unity-runner-to-user-session.ps1 as appropriate."
}

$listener = Get-CimInstance Win32_Process -Filter "Name='Runner.Listener.exe'" -ErrorAction SilentlyContinue
if ($listener) {
    Write-Host "LISTENER running PID=$($listener.ProcessId -join ',')"
} else {
    Write-Warning "Runner host was started but Runner.Listener.exe was not visible yet; GitHub may need a few seconds to mark it online."
}

Write-Host "Unity runner repair completed. Existing queued GitHub Actions jobs should be picked up automatically."
