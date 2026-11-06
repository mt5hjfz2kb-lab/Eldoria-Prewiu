# Link installed SuperLocalMemory to the existing Codex user profile safely.
# Only touch Codex TOML/SLM-owned hooks; no Unity, no system restart, no OAuth.
[CmdletBinding()]
param([ValidateSet('check','connect')][string]$Mode='check')
$ErrorActionPreference='Stop'
$ProgressPreference='SilentlyContinue'
try {
 $root=Join-Path $env:LOCALAPPDATA 'EldoriaSLM'
 $marker=Join-Path $root 'eldoria-owned-marker.json'
 $slm=Join-Path $root 'venv-4.2.12\Scripts\slm.exe'
 $py=Join-Path $root 'venv-4.2.12\Scripts\python.exe'
 $data=Join-Path $root 'data'
 if(-not (Test-Path $marker) -or -not (Test-Path $slm) -or -not (Test-Path $py) -or -not (Test-Path $data)) {
    throw 'Owned SuperLocalMemory installation not found; no user changes'
 }
 $meta=Get-Content -LiteralPath $marker -Raw|ConvertFrom-Json
 if($meta.owner -ne 'Eldoria' -or $meta.version -ne '4.2.12'){throw 'Unknown install marker'}
 if(-not $env:USERPROFILE -or $env:USERNAME -ieq 'SYSTEM' -or $env:USERPROFILE -match 'systemprofile'){
   throw 'Unstable Windows user; no Codex changes'
 }
 $config=Join-Path $env:USERPROFILE '.codex\config.toml'
 $backup=Join-Path $root 'backups'
 $helper=Join-Path $PSScriptRoot 'Connect-SlmCodex.py'
 if(-not (Test-Path $helper)){throw 'Helper missing'}
 $codex=Get-Command codex.exe -ErrorAction SilentlyContinue
 if(-not $codex){$codex=Get-Command codex -ErrorAction SilentlyContinue}
 [ordered]@{
   profile_stable=$true
   installed_slm=$true
   config_exists=(Test-Path $config)
   codex_cli_detected=[bool]$codex
   oauth_not_requested=$true
   network_disabled=$true
   requested_mode=$Mode
 }|ConvertTo-Json -Compress|Write-Host
 if($Mode -eq 'check'){
   & $py $helper --help | Out-Null
   if($LASTEXITCODE -ne 0){throw 'Configuration helper unavailable'}
   Write-Host 'SLM_CODEX_SAFE_PREFLIGHT_PASS'
   exit 0
 }
 # Changes may only begin with exactly our own exclusive current native lease.
 $repo='mt5hjfz2kb-lab/Eldoria-Prewiu'
 $response=Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/contents/pipeline/active-workstreams.json?ref=main" -TimeoutSec 25
 if(-not $response.content){throw 'Missing live lease data'}
 $registry=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String(($response.content -replace '\s','')))|ConvertFrom-Json
 $claims=@($registry.active|Where-Object {@($_.resources) -contains 'windows-runner-heavy'})
 if($claims.Count -ne 1 -or $claims[0].id -ne 'eldoria-slm-permanent-bootstrap-20261104' -or
    $claims[0].owner -ne 'chatgpt-eldoria-slm-bootstrap-20261104' -or
    $claims[0].ownership_state -ne 'claimed'){
   throw 'Exclusive native resource held by someone else; no Codex change'
 }
 if(@(Get-Process Unity,blender,UnityShaderCompiler,UnityPackageManager,UnityCrashHandler64 -ErrorAction SilentlyContinue).Count){
   throw 'Active native graphics process; no Codex change'
 }
 $env:SLM_HOME=$data
 $env:SLM_SKIP_LAYA='1'
 $env:SLM_MCP_PROFILE='core'
 $env:PATH="$(Join-Path $root 'venv-4.2.12\Scripts');$env:PATH"
 & $py $helper --config $config --slm-executable $slm --slm-home $data --backup-dir $backup
 if($LASTEXITCODE -ne 0){throw 'Safe Codex MCP config merge failed'}
 $cmd=Join-Path $root 'venv-4.2.12\Scripts\slm.exe'
 & $cmd codex install --dry-run | Out-Null
 if($LASTEXITCODE -ne 0){throw 'Codex skills/hook preview failed; config remains backed up'}
 & $cmd codex install | Out-Null
 if($LASTEXITCODE -ne 0){throw 'SLM-owned Codex skills installation failed'}
 & $cmd codex status | Out-Null
 if($LASTEXITCODE -ne 0){throw 'Installed Codex assets not verified'}
 $probe=Join-Path $PSScriptRoot 'Validate-SlmMcp.py'
 & $py $probe
 if($LASTEXITCODE -ne 0){throw 'Independent MCP client proof failed'}
 Write-Host 'SLM_CODEX_REGISTERED_PASS: private MCP config and SLM skills verified; no authenticated model session claimed'
 if($codex){
   $output=(& $codex.Source mcp list 2>&1 | Out-String)
   if($LASTEXITCODE -eq 0 -and $output -match 'superlocalmemory'){
     Write-Host 'SLM_CODEX_CLI_LIST_PASS: config recognized by installed Codex CLI'
   } else {
     Write-Host 'SLM_CODEX_CLI_LIST_UNVERIFIED: Codex CLI not ready to list MCP; no OAuth attempted'
   }
 } else {
   Write-Host 'SLM_CODEX_CLI_ABSENT: config and SLM skills prepared, no Codex login attempted'
 }
} catch {
 Write-Host ('SLM_CODEX_FAIL_CLOSED: '+$_.Exception.Message)
 exit 4
}
