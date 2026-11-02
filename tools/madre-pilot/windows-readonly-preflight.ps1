#requires -Version 5.1
<#
Eldoria / MADRE pilot — READ-ONLY Windows preflight.
No admin elevation; no installation; no network; no file writes; no secrets.
Do not execute on the shared runner until its lease is released.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Count-Running([string]$Name) {
    return @((Get-Process -Name $Name -ErrorAction SilentlyContinue)).Count
}

function Get-AvailableVersion([string]$Exe) {
    $found = Get-Command $Exe -ErrorAction SilentlyContinue
    if (-not $found) { return 'not-found' }
    try { return ((& $found.Source --version 2>&1) | Out-String).Trim() }
    catch { return 'detected-version-unavailable' }
}

$os = Get-CimInstance -ClassName Win32_OperatingSystem
$computer = Get-CimInstance -ClassName Win32_ComputerSystem
$cpu = Get-CimInstance -ClassName Win32_Processor | Select-Object -First 1
$disks = @(Get-CimInstance -ClassName Win32_LogicalDisk -Filter 'DriveType=3' | ForEach-Object {
    [ordered]@{
        drive = $_.DeviceID
        total_gb = [math]::Round([double]$_.Size / 1GB, 1)
        free_gb = [math]::Round([double]$_.FreeSpace / 1GB, 1)
    }
})
$unityCount = Count-Running 'Unity'
$blenderCount = Count-Running 'blender'
$runnerServices = @(Get-Service -Name 'actions.runner.*' -ErrorAction SilentlyContinue)
$runnerRunning = @($runnerServices | Where-Object { $_.Status -eq 'Running' }).Count

# Checking whether wsl.exe exists does not install, launch or modify WSL.
$wslBinaryPresent = (Test-Path (Join-Path $env:WINDIR 'System32\wsl.exe'))
$virtualization = 'unknown'
if ($null -ne $cpu -and $null -ne $cpu.VirtualizationFirmwareEnabled) {
    $virtualization = [string]$cpu.VirtualizationFirmwareEnabled
}

$report = [ordered]@{
    purpose = 'MADRE_READONLY_PREFLIGHT_NO_CHANGES'
    when_local = (Get-Date).ToString('yyyy-MM-ddTHH:mm:ssK')
    os_name = $os.Caption
    windows_build = $os.BuildNumber
    os_is_64_bit = ($os.OSArchitecture -like '*64*')
    cpu_name = $cpu.Name
    logical_processors = $computer.NumberOfLogicalProcessors
    virtualization_enabled_in_firmware = $virtualization
    ram_total_gb = [math]::Round([double]$computer.TotalPhysicalMemory / 1GB, 1)
    ram_free_gb = [math]::Round(([double]$os.FreePhysicalMemory * 1KB) / 1GB, 1)
    disks = $disks
    wsl_binary_exists = $wslBinaryPresent
    node_version_windows = (Get-AvailableVersion 'node.exe')
    git_version_windows = (Get-AvailableVersion 'git.exe')
    unity_processes = $unityCount
    blender_processes = $blenderCount
    github_runner_services_running = $runnerRunning
    install_attempted = $false
    requires_restart = 'not-evaluated'
    pilot_can_start = $false
    reason = 'Read-only diagnostics do not grant permission to install; verify GitHub runner lease and owner approval separately.'
}
$report | ConvertTo-Json -Depth 6

if ($unityCount -gt 0 -or $blenderCount -gt 0) {
    Write-Warning 'Unity or Blender is running. Do not attempt MADRE/WSL installation.'
}
Write-Output 'READ_ONLY_PREFLIGHT_FINISHED_NO_SYSTEM_CHANGES'
