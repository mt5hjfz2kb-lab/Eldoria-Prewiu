# Verified, private, fail-closed SLM Mode A installer. Windows PowerShell 5.1.
# No Windows/Unity restart, no OS service, no remote OAuth or Codex profile changes.
[CmdletBinding()]
param([ValidateSet('check','install')][string]$Mode='check')
$ErrorActionPreference='Stop'
$ProgressPreference='SilentlyContinue'
$repo='mt5hjfz2kb-lab/Eldoria-Prewiu'
try {
 $uri="https://api.github.com/repos/$repo/contents/pipeline/active-workstreams.json?ref=main"
 $r=Invoke-RestMethod -Uri $uri -Headers @{Accept='application/vnd.github+json';'Cache-Control'='no-cache'} -TimeoutSec 25
 if(-not $r.content -or -not $r.sha){throw 'Unverified registry'}
 $reg=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String(($r.content -replace '\s',''))) | ConvertFrom-Json
 if(-not $reg.active -or -not $reg.updated_at){throw 'Invalid registry'}
 $leases=@($reg.active | Where-Object { @($_.resources) -contains 'windows-runner-heavy' })
 $exclusive=($leases.Count -eq 1 -and $leases[0].id -eq 'eldoria-slm-permanent-bootstrap-20261104' -and $leases[0].owner -eq 'chatgpt-eldoria-slm-bootstrap-20261104' -and $leases[0].ownership_state -eq 'claimed')
 $busy=@(Get-Process -Name @('Unity','blender','UnityShaderCompiler','UnityPackageManager','UnityCrashHandler64') -ErrorAction SilentlyContinue)
 # Ollama can be installed and idle without running an inference job.
 # Official localhost GET /api/ps lists models currently loaded, not whether the server process exists.
 $ollamaProc=@(Get-Process -Name 'ollama*' -ErrorAction SilentlyContinue)
 $ollamaIdle=($ollamaProc.Count -eq 0)
 if($ollamaProc.Count -gt 0){
  $ollamaIdle=$true
  foreach($attempt in 1..2){
   try {
    $ps=Invoke-RestMethod -Uri 'http://127.0.0.1:11434/api/ps' -TimeoutSec 4
    if($null -eq $ps -or $null -eq $ps.models -or @($ps.models).Count -ne 0){$ollamaIdle=$false;break}
   } catch {$ollamaIdle=$false;break}
   Start-Sleep -Milliseconds 650
  }
 }
 $os=Get-CimInstance Win32_OperatingSystem
 $ram=[math]::Round(($os.FreePhysicalMemory*1KB)/1GB,2)
 $profile=$env:LOCALAPPDATA
 if(-not $profile){throw 'No stable profile'}
 $drive=[System.IO.Path]::GetPathRoot($profile)
 $disk=Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='$($drive.TrimEnd([char]92))'"
 if(-not $disk){throw 'Cannot measure disk'}
 $diskGB=[math]::Round(($disk.FreeSpace/1GB),2)
 $userOK=(-not ($env:USERNAME -ieq 'SYSTEM' -or $env:USERPROFILE -match 'systemprofile' -or $profile -match '(?i)[\\/]_temp[\\/]'))
 $py=Get-Command py.exe -ErrorAction SilentlyContinue
 $hasPython=$false
 if($py){$v=& $py.Source -3.12 --version 2>&1;$hasPython=($LASTEXITCODE -eq 0 -and "$v" -match '^Python 3\.12\.')}
 $safe=$exclusive -and $busy.Count -eq 0 -and $ollamaIdle -and $ram -ge 6 -and $diskGB -ge 10 -and $userOK -and $hasPython
 [ordered]@{safe_to_install=$safe;exclusive_lease=$exclusive;busy_process_count=$busy.Count;ollama_daemon_present=($ollamaProc.Count -gt 0);ollama_inference_idle=$ollamaIdle;free_ram_gb=$ram;free_disk_gb=$diskGB;python_312=$hasPython;stable_user=$userOK} | ConvertTo-Json -Compress | Write-Output
 if(-not $safe){exit 3}
 if($Mode -eq 'check'){'PREFLIGHT_PASS no changes' | Write-Output;exit 0}
 $root=Join-Path $profile 'EldoriaSLM'
 $db=Join-Path $root 'data'
 $venv=Join-Path $root 'venv-4.2.12'
 $marker=Join-Path $root 'eldoria-owned-marker.json'
 if((Test-Path $root) -and -not (Test-Path $marker)){throw 'Existing unmanaged directory'}
 New-Item -ItemType Directory -Force -Path $root,$db | Out-Null
 if(-not (Test-Path $marker)){'{"owner":"Eldoria","version":"4.2.12","autostart":false}' | Set-Content -LiteralPath $marker -Encoding UTF8}
 $python=Join-Path $venv 'Scripts\python.exe'
 if(-not (Test-Path $python)){& py -3.12 -m venv $venv;if($LASTEXITCODE -ne 0){throw 'venv failed'}}
 & $python -m pip install --no-input --disable-pip-version-check 'superlocalmemory==4.2.12'
 if($LASTEXITCODE -ne 0){throw 'install failed'}
 $slm=Join-Path $venv 'Scripts\slm.exe'
 $env:SLM_HOME=$db;$env:SLM_SKIP_LAYA='1';$env:SLM_MCP_PROFILE='core'
 $env:PATH="$(Join-Path $venv 'Scripts');$env:PATH"
 $started=$false
 try {
  & $slm setup --auto;if($LASTEXITCODE -ne 0){throw 'setup failed'}
  & $slm mode a;if($LASTEXITCODE -ne 0){throw 'Mode A failed'}
  & $slm serve start;if($LASTEXITCODE -ne 0){throw 'daemon start failed'}
  $started=$true
  & $slm warmup;if($LASTEXITCODE -ne 0){throw 'warmup failed'}
  & $slm doctor --quick;if($LASTEXITCODE -ne 0){throw 'doctor failed'}
  $seed=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'verified-facts.json') -Raw | ConvertFrom-Json
  if($seed.repository -ne $repo -or $seed.schema_version -ne 1){throw 'Invalid seed'}
  $head=(Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/branches/main" -TimeoutSec 25).commit.sha
  foreach($s in @($seed.entries | Select-Object -Unique source_path,source_blob_sha)){
   if($s.source_path.Contains('..') -or $s.source_path -notmatch '^[A-Za-z0-9_./-]+$'){throw 'Unsafe source path'}
   $remote=Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/contents/$($s.source_path)?ref=$head" -TimeoutSec 25
   if($remote.sha -ne $s.source_blob_sha){throw 'Source changed: re-review required'}
  }
  $after=(Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/branches/main" -TimeoutSec 25).commit.sha
  if($head -ne $after){throw 'main changed while verifying'}
  $ledgerPath=Join-Path $db 'eldoria-reviewed-seed-ledger.json'
  $ledger=@{}
  if(Test-Path $ledgerPath){$old=Get-Content -LiteralPath $ledgerPath -Raw | ConvertFrom-Json;foreach($p in $old.PSObject.Properties){$ledger[$p.Name]=$p.Value}}
  $saved=0
  foreach($s in $seed.entries){
   if($s.id -notmatch '^[a-z0-9-]+$'){throw 'Invalid id'}
   $entry="ELDORIA [$($s.id)] $($s.text) SOURCE: $($s.source_path) blob=$($s.source_blob_sha). Verify latest HEAD before action. NOT a live owner lease or art approval."
   $hash=[Security.Cryptography.SHA256]::Create().ComputeHash([Text.Encoding]::UTF8.GetBytes($entry))
   $fp=([BitConverter]::ToString($hash)).Replace('-','').ToLowerInvariant()
   if($ledger.ContainsKey($s.id)){if($ledger[$s.id] -ne $fp){throw 'Changed memory requires review'};continue}
   $response=& $slm remember $entry --kind $s.kind --tags 'eldoria,source-reviewed' --sync --json
   if($LASTEXITCODE -ne 0){throw 'Memory save error'}
   $receipt=($response | Out-String) | ConvertFrom-Json
   if(-not $receipt.success -and -not $receipt.accepted -and -not $receipt.fact_ids){throw 'No save receipt'}
   $ledger[$s.id]=$fp
   $ledger | ConvertTo-Json | Set-Content -LiteralPath $ledgerPath -Encoding UTF8
   $saved++
  }
  Write-Output "SLM_LOCAL_INSTALL_PASS memories_saved=$saved; no autostart or remote integration"
 } finally { if($started){& $slm serve stop | Out-Null}; 'SLM_DAEMON_STOPPED' | Write-Output }
} catch {Write-Output ('FAIL_CLOSED: '+$_.Exception.Message);exit 4}
