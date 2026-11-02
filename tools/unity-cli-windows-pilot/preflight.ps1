$ErrorActionPreference = 'Stop'
$root = Join-Path $env:RUNNER_TEMP ('eldoria-cli-preflight-' + $env:GITHUB_RUN_ID)
New-Item -ItemType Directory -Force -Path $root | Out-Null
$processNames = @('Unity', 'Unity Hub', 'blender', 'UnityShaderCompiler', 'UnityPackageManager', 'UnityCrashHandler64')
$found = @()
foreach ($name in $processNames) {
  $processes = @(Get-Process -Name $name -ErrorAction SilentlyContinue)
  $found += [pscustomobject]@{ name=$name; count=$processes.Count }
}
$edit = $env:UNITY_EDITOR_PATH
$editorExists = ($edit -and (Test-Path -LiteralPath $edit -PathType Leaf))
$versionExists = Test-Path -LiteralPath (Join-Path $env:GITHUB_WORKSPACE 'valoria-layered-source/Unity/ProjectSettings/ProjectVersion.txt')
$cliCandidates = @('unity','codex')
$installed = foreach ($cmd in $cliCandidates) {
  $foundCommand = Get-Command $cmd -ErrorAction SilentlyContinue
  [pscustomobject]@{ name=$cmd; detected=[bool]$foundCommand }
}
$relevant = @($found | Where-Object { $_.count -gt 0 })
$free = ($relevant.Count -eq 0)
$result = [ordered]@{
  source = 'self-hosted windows runner read-only process and tool preflight'
  workflow_sha = $env:GITHUB_SHA
  run_id = $env:GITHUB_RUN_ID
  checked_utc = [DateTime]::UtcNow.ToString('o')
  os = (Get-CimInstance -ClassName Win32_OperatingSystem).Caption
  os_build = [Environment]::OSVersion.Version.ToString()
  unity_editor_path_configured = [bool]$edit
  unity_editor_installed_at_configured_path = [bool]$editorExists
  unity_project_version_file_present_at_shared_checkout = [bool]$versionExists
  tools = @($installed)
  processes = @($found)
  safe_to_attempt_isolated_test = $free
  did_install = $false
  did_launch_unity = $false
  did_kill_process = $false
  did_modify_shared_project = $false
  did_restart = $false
}
$result | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $root 'preflight.json') -Encoding utf8
Write-Host "WINDOWS_READONLY_PREFLIGHT_JSON=$root/preflight.json"
Write-Host ("ELDORIA_WINDOWS_SAFE=" + $free)
Write-Host ("UNITY_EDITOR_INSTALLED=" + [bool]$editorExists)
if (-not $free) {
  Write-Warning 'Unity/Blender/shared processes are active. Do not claim or run Editor until they are gone.'
}
