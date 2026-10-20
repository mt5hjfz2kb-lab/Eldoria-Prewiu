param([string]$Package)
$ErrorActionPreference='Stop'
$base=Join-Path $env:ProgramData 'EldoriaPlaytest'
$ts=Join-Path $env:ProgramFiles 'Tailscale\tailscale.exe'
$report=[ordered]@{state='PREPARING';installed_now=$false;network_verified=$false;public_funnel=$false}
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'PC editing processes active; refuse setup interference'}
New-Item -ItemType Directory -Force $base | Out-Null
$identity=Get-Content (Join-Path $Package 'identity.json') -Raw | ConvertFrom-Json
$version=Join-Path $base ('versions\'+$identity.candidate.revision+'-'+$identity.zip_sha256.Substring(0,12))
if(-not(Test-Path $version)){
  python tools/delivery/prepare-pc.py $Package $version
  if($LASTEXITCODE){throw 'PC package verification failed'}
}
$current=Join-Path $base 'current'
if(Test-Path $current){
  $item=Get-Item $current -Force
  if(-not($item.Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Existing current directory is not managed junction; preserve it'}
  $expectedPrefix=(Join-Path $base 'versions')+'\'
  if(-not([string]$item.Target).StartsWith($expectedPrefix,[StringComparison]::OrdinalIgnoreCase)){throw 'Existing junction has unrelated target'}
  if(([string]$item.Target) -ne $version){[IO.Directory]::Delete($current);New-Item -ItemType Junction -Path $current -Target $version | Out-Null}
}else{New-Item -ItemType Junction -Path $current -Target $version | Out-Null}
if(-not(Test-Path $ts)){
  $admin=([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
  if(-not $admin){throw 'Administrator permission needed for initial Tailscale installation'}
  if($env:PROCESSOR_ARCHITECTURE -ne 'AMD64'){throw 'Installer pinned to verified amd64 PC only'}
  $url='https://pkgs.tailscale.com/stable/tailscale-setup-1.104.1-amd64.msi'
  $msi=Join-Path $env:RUNNER_TEMP 'eldoria-tailscale-1.104.1.msi'
  Invoke-WebRequest $url -OutFile $msi -UseBasicParsing
  $expected=((Invoke-WebRequest ($url+'.sha256') -UseBasicParsing).Content.Trim() -split '\s+')[0]
  if((Get-FileHash $msi -Algorithm SHA256).Hash -ne $expected){throw 'Official installer checksum mismatch'}
  $sig=Get-AuthenticodeSignature $msi
  if($sig.Status -ne 'Valid' -or $sig.SignerCertificate.Subject -notmatch 'Tailscale'){throw 'Official installer signature invalid'}
  $p=Start-Process msiexec.exe -ArgumentList @('/i',('"{0}"' -f $msi),'/qn','/norestart') -Wait -PassThru
  if($p.ExitCode -notin @(0,3010)){throw "Tailscale MSI failed: $($p.ExitCode)"}
  $report.installed_now=$true
}
if(-not(Test-Path $ts)){throw 'Tailscale CLI unavailable after installation'}
$report.version=(& $ts version | Select-Object -First 1)
$report.revision=$identity.candidate.revision
$report.package_sha256=$identity.zip_sha256
# No login URLs, keys, identities, peer addresses or account details in CI logs.
$stateJson=& $ts status --json 2>$null
$state=$null
if($stateJson){try{$state=($stateJson -join [Environment]::NewLine)|ConvertFrom-Json}catch{}}
if(-not $state -or $state.BackendState -ne 'Running'){
  $report.state='PC_READY_OWNER_TAILSCALE_LOGIN_REQUIRED'
  $report.next='Open Tailscale on the PC and sign in; install Tailscale on iPhone with the same account. Then request connection completion.'
}else{
  $report.state='PC_READY_EXISTING_TAILSCALE_PROFILE_REQUIRES_OWNER_CONFIRMATION'
  $report.next='Confirm the existing PC Tailscale account belongs to the owner before sharing the prepared directory.'
}
$report | ConvertTo-Json -Depth 6 | Set-Content -Encoding UTF8 (Join-Path $base 'setup-status.json')
$report | ConvertTo-Json -Depth 6
