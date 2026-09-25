param(
    [string]$RepositoryUrl = "https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu",
    [string]$RunnerLabel = "unity-6000-3-23f1",
    [string]$RunnerName = $env:COMPUTERNAME
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

Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine -Force

$unity = "C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe"
if (-not (Test-Path $unity)) {
    throw "Unity 6000.3.23f1 was not found at: $unity"
}

[Environment]::SetEnvironmentVariable("UNITY_EDITOR_PATH", $unity, "Machine")
$env:UNITY_EDITOR_PATH = $unity

$runnerRoot = "C:\actions-runner-eldoria"
$searchRoots = @("$env:USERPROFILE\actions-runner", "$env:USERPROFILE\Downloads")
$zip = $null
foreach ($root in $searchRoots) {
    if (Test-Path $root) {
        $zip = Get-ChildItem $root -Filter "actions-runner-win-x64-*.zip" -File -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($zip) { break }
    }
}
if (-not $zip) {
    throw "GitHub Actions runner ZIP not found. Download the Windows x64 runner from Settings > Actions > Runners first."
}

if (Test-Path $runnerRoot) { Remove-Item $runnerRoot -Recurse -Force }
New-Item -ItemType Directory -Path $runnerRoot -Force | Out-Null
Expand-Archive -Path $zip.FullName -DestinationPath $runnerRoot -Force
Set-Location $runnerRoot

$token = Read-Host "Paste the NEW GitHub runner registration token (it will not be stored)"
if ([string]::IsNullOrWhiteSpace($token)) { throw "No registration token supplied." }

Write-Host ""
Write-Host "Configuring Eldoria runner as a Windows service..." -ForegroundColor Cyan
& .\config.cmd --unattended --url $RepositoryUrl --token $token --name $RunnerName --labels $RunnerLabel --work "_work" --runasservice --windowslogonaccount "NT AUTHORITY\NETWORK SERVICE" --replace
if ($LASTEXITCODE -ne 0) { throw "Runner configuration failed with exit code $LASTEXITCODE." }

$serviceFile = Join-Path $runnerRoot ".service"
if (-not (Test-Path $serviceFile)) { throw "Runner was registered but the Windows service file was not created." }
$serviceName = (Get-Content $serviceFile -Raw).Trim()
$service = Get-Service -Name $serviceName -ErrorAction Stop
if ($service.Status -ne "Running") { Start-Service -Name $serviceName; $service = Get-Service -Name $serviceName }

Write-Host ""
Write-Host "Eldoria Unity runner ready." -ForegroundColor Green
Write-Host "Runner:  $RunnerName"
Write-Host "Label:   $RunnerLabel"
Write-Host "Service: $serviceName ($($service.Status))"
Write-Host "Unity:   $unity"
Write-Host ""
Write-Host "Next repository step: set Actions variable UNITY_RUNNER_READY=true."
Write-Host "If Unity batch mode reports no license under the service account, run scripts\\switch-unity-runner-to-user-session.ps1 once."
