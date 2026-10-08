param([ValidateSet('InstallAndTest','Serve')][string]$Mode='InstallAndTest', [string]$EvidenceDir)
$ErrorActionPreference='Stop'
$root=Join-Path $env:LOCALAPPDATA 'EldoriaLocalAI'
$bin=Join-Path $root 'ollama-v0.40.1'
$exe=Join-Path $bin 'ollama.exe'
$env:OLLAMA_HOST='127.0.0.1:11434'
$env:OLLAMA_NO_CLOUD='1'
$env:OLLAMA_MODELS=Join-Path $root 'models'
$env:OLLAMA_NUM_PARALLEL='1'
$env:OLLAMA_MAX_LOADED_MODELS='1'
$env:OLLAMA_CONTEXT_LENGTH='2048'
$env:OLLAMA_KEEP_ALIVE='0'
function AssertIdle {
  if (Get-Process -Name Unity,Blender -ErrorAction SilentlyContinue) { throw 'BLOCKED: Unity/Blender running; no processes were stopped.' }
  $mem=Get-CimInstance Win32_OperatingSystem
  if ($mem.FreePhysicalMemory / 1MB -lt 6) { throw 'BLOCKED: less than 6 GB free RAM.' }
  $gpu=& nvidia-smi --query-gpu=memory.free --format=csv,noheader,nounits
  if ($LASTEXITCODE -ne 0 -or [int]($gpu | Select-Object -First 1) -lt 6000) { throw 'BLOCKED: less than 6000 MiB free VRAM.' }
}
AssertIdle
if (Get-NetTCPConnection -State Listen -LocalPort 11434 -ErrorAction SilentlyContinue) { throw 'BLOCKED: port 11434 already occupied; existing server not modified.' }
if (Get-Process -Name ollama -ErrorAction SilentlyContinue) { throw 'BLOCKED: existing Ollama process; no replacement attempted.' }
if ($Mode -eq 'Serve') {
  if (!(Test-Path $exe)) { throw 'Run the installation workflow first.' }
  & $exe serve
  exit $LASTEXITCODE
}
if (!$EvidenceDir) { throw 'EvidenceDir required.' }
New-Item -ItemType Directory -Force -Path $root,$EvidenceDir | Out-Null
$os=Get-CimInstance Win32_OperatingSystem
if ([int]$os.BuildNumber -lt 19045) { throw 'BLOCKED: Windows 10 22H2 or newer required.' }
$drive=Get-Item $root
if ((Get-PSDrive $drive.PSDrive.Name).Free / 1GB -lt 12) { throw 'BLOCKED: less than 12 GB free disk space.' }
$before=@(Get-Process -Name Unity,Blender -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,StartTime)
& nvidia-smi --query-gpu=name,driver_version,memory.total,memory.free,utilization.gpu --format=csv | Set-Content (Join-Path $EvidenceDir 'gpu-before.txt')
if (!(Test-Path $exe)) {
  $zip=Join-Path $root 'ollama-v0.40.1.zip'
  [Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
  Invoke-WebRequest -UseBasicParsing -Uri 'https://github.com/ollama/ollama/releases/download/v0.40.1/ollama-windows-amd64.zip' -OutFile $zip
  $hash=(Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant()
  if ($hash -ne 'b394d14436d38032f23190e3f14eb2c6dad5ebbe4e192414f74c8fdca01703ab') { throw 'STOP: official archive checksum mismatch.' }
  Expand-Archive -LiteralPath $zip -DestinationPath $bin -Force
  Remove-Item -LiteralPath $zip
}
AssertIdle
$server=$null
try {
  $server=Start-Process -FilePath $exe -ArgumentList 'serve' -PassThru -RedirectStandardOutput (Join-Path $EvidenceDir 'server-stdout.log') -RedirectStandardError (Join-Path $EvidenceDir 'server-stderr.log')
  $ready=$false
  for ($i=0; $i -lt 30; $i++) {
    if ($server.HasExited) { throw 'Ollama exited before readiness.' }
    try { $version=Invoke-RestMethod -Uri 'http://127.0.0.1:11434/api/version' -TimeoutSec 2; $ready=$true; break } catch { Start-Sleep -Seconds 1 }
  }
  if (!$ready) { throw 'Ollama readiness timeout.' }
  $listeners=@(Get-NetTCPConnection -State Listen -LocalPort 11434)
  if (!$listeners.Count -or @($listeners | Where-Object {$_.LocalAddress -ne '127.0.0.1' -or $_.OwningProcess -ne $server.Id}).Count) { throw 'STOP: unexpected listener ownership/address.' }
  & $exe pull qwen2.5-coder:3b
  if ($LASTEXITCODE -ne 0) { throw 'Model download failed.' }
  AssertIdle
  & python (Join-Path $PSScriptRoot 'local-ai-proof-v1.py') $EvidenceDir
  if ($LASTEXITCODE -ne 0) { throw 'Isolated programming proof failed.' }
  $proof=Get-Content (Join-Path $EvidenceDir 'proof.json') -Raw | ConvertFrom-Json
  if ($proof.status -ne 'PASS') { throw 'Proof did not pass.' }
  @{status='PROOF_PASS';ollama_version=$version.version;archive_sha256='b394d14436d38032f23190e3f14eb2c6dad5ebbe4e192414f74c8fdca01703ab';bind='127.0.0.1:11434';cloud_disabled=$true;services_installed=0;global_env_changes=0;source_sha=$env:GITHUB_SHA;run_id=$env:GITHUB_RUN_ID;unity_blender_processes_before=$before;paid_api_calls=0;installation_path=$bin} | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $EvidenceDir 'installation.json')
} finally {
  if ($server -and !$server.HasExited) { & taskkill /PID $server.Id /T /F | Out-Null }
  & nvidia-smi --query-gpu=name,memory.free,utilization.gpu --format=csv | Set-Content (Join-Path $EvidenceDir 'gpu-after.txt')
}
Start-Sleep -Seconds 2
if (Get-NetTCPConnection -State Listen -LocalPort 11434 -ErrorAction SilentlyContinue) { throw 'Server listener remains after stop; inspect evidence.' }
if (Get-Process -Name Unity,Blender -ErrorAction SilentlyContinue) { throw 'Concurrent application appeared; non-interference cannot be certified.' }
$log=Get-Content (Join-Path $EvidenceDir 'server-stderr.log') -Raw
if ($log -notmatch 'Ollama cloud disabled: true') { throw 'Cannot verify local-only mode from server logs.' }
@{status='PASS';listener_closed=$true;model_unloaded=$true;unity_blender_not_running_before_or_after=$true;checked_at=(Get-Date).ToUniversalTime().ToString('o')} | ConvertTo-Json | Set-Content (Join-Path $EvidenceDir 'final-check.json')
Write-Output 'ELDORIA_LOCAL_AI_INSTALL_AND_REAL_PROOF_PASS'
