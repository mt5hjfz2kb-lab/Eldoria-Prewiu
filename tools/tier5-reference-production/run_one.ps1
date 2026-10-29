param([Parameter(Mandatory=$true)][string]$RequestPath)
$ErrorActionPreference='Stop'
$env:TRIPO_CLIENT_APP='codex'
$env:TRIPO_DISTRIBUTION_CHANNEL='codex_plugin'
$req=Get-Content $RequestPath -Raw | ConvertFrom-Json
$allowed=@('lyra','bastion-v','aserradero-v','cuartel-v','granero-v','cantera-v')
if ($allowed -notcontains [string]$req.asset) { throw 'Unknown asset' }
if ([int]$req.iteration -lt 1 -or [int]$req.iteration -gt 3) { throw 'Invalid attempt' }
if ([int]$req.credit_limit -ne 30) { throw 'Only standard image generation at 30 credits allowed' }
$name=([string]$req.asset)+'-image-r'+[string]$req.iteration
$out=Join-Path $env:USERPROFILE ('EldoriaArtSource\Tier5ReferenceProduction\'+$name)
New-Item -ItemType Directory -Force $out | Out-Null
('source_dir='+$out) >> $env:GITHUB_OUTPUT
('asset_name='+$name) >> $env:GITHUB_OUTPUT
$tripo=Get-Command tripo.cmd -ErrorAction SilentlyContinue
if (-not $tripo) { $tripo=Get-Command tripo -ErrorAction Stop }
$reference=Join-Path $out 'reference.jpeg'
Invoke-WebRequest -UseBasicParsing -Uri $req.reference_url -OutFile $reference -TimeoutSec 60
if ((Get-FileHash $reference -Algorithm SHA256).Hash.ToLowerInvariant() -ne [string]$req.reference_sha256) { throw 'Exact reference hash mismatch' }
$submission=Join-Path $out 'submission.json'
if (-not (Test-Path $submission)) {
    if (-not [bool]$req.submit_authorized) { throw 'No existing submission and submission not authorized' }
    $lock=Join-Path $out 'SUBMISSION_STARTED.lock'
    if (Test-Path $lock) { throw 'Ambiguous prior submission: recover from history, no resubmission' }
    $doctor=(& $tripo.Source doctor --json | Out-String) | ConvertFrom-Json
    if (-not $doctor.ok) { throw 'CLI authentication unavailable' }
    $balance=(& $tripo.Source balance --json | Out-String) | ConvertFrom-Json
    if ([double]$balance.balance -lt 30) { throw 'Insufficient prepaid balance; no topup' }
    $balance | ConvertTo-Json | Set-Content (Join-Path $out 'balance-before.json')
    $command=@('make',$reference,'--model','v3.1-20260211','-p','face_limit=95000','-p','texture=true','-p','pbr=true','-p','texture_quality=standard','-p','geometry_quality=standard','-p','texture_alignment=original_image','-p','orientation=align_image','--name',('eldoria-'+$name+'-20261029'),'--no-wait','--json','--yes','--quiet','--no-open','-o',$out)
    $command | ConvertTo-Json | Set-Content (Join-Path $out 'configuration.json')
    Copy-Item $RequestPath (Join-Path $out 'authorized-request.json')
    [IO.File]::WriteAllText($lock,[DateTime]::UtcNow.ToString('o'))
    $text=(& $tripo.Source @command | Out-String).Trim()
    $code=$LASTEXITCODE
    $text | Set-Content $submission
    if ($code -ne 0) { throw 'Submission uncertain: no automatic retry' }
}
$submitted=Get-Content $submission -Raw | ConvertFrom-Json
$task=[string]$submitted.task_id
if ($task -notmatch '^[a-f0-9-]{36}$') { throw 'Missing task ID: recover history only' }
Write-Host ('RECOVERING_EXISTING_TASK='+$task)
for ($i=0;$i -lt 180;$i++) {
    $text=(& $tripo.Source task get $task --json | Out-String).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Task query failed: query can be retried, generation cannot' }
    $text | Set-Content (Join-Path $out 'task-result.json')
    $result=$text | ConvertFrom-Json
    Write-Host ('TASK_STATUS='+$result.status+' PROGRESS='+$result.progress)
    if ($result.status -eq 'success') { break }
    if ($result.status -in @('failed','cancelled','banned')) { throw ('Task terminal status: '+$result.status) }
    Start-Sleep -Seconds 5
}
if ($result.status -ne 'success') { throw 'Task pending: recover same ID later' }
function Get-GlbUrls($o) {
    if ($null -eq $o) { return }
    if ($o -is [string]) { if ($o -match '^https://.*\.glb(\?|$)') { $o }; return }
    if ($o -is [array]) { foreach ($v in $o) { Get-GlbUrls $v }; return }
    if ($o -is [pscustomobject]) { foreach ($p in $o.PSObject.Properties) { Get-GlbUrls $p.Value } }
}
$urls=@(Get-GlbUrls $result | Select-Object -Unique)
if ($urls.Count -lt 1) { throw 'No GLB URL: inspect saved task result' }
for ($i=0;$i -lt $urls.Count;$i++) {
    $filename=if ($i -eq 0) { 'raw.glb' } else { 'raw-variant-'+$i+'.glb' }
    $dest=Join-Path $out $filename
    if (-not (Test-Path $dest)) { Invoke-WebRequest -UseBasicParsing -Uri $urls[$i] -OutFile $dest -TimeoutSec 180 }
}
& $tripo.Source balance --json | Set-Content (Join-Path $out 'balance-after.json')
$credits=$result.credits_consumed
if ($null -eq $credits) { $credits=$result.consumed_credit }
@{task_id=$task;credits_consumed=$credits;reference_sha256=$req.reference_sha256;raw_sha256=(Get-FileHash (Join-Path $out 'raw.glb') -Algorithm SHA256).Hash.ToLowerInvariant();status='SOURCE_DOWNLOADED_REVIEW_PENDING';runtime_integration=$false} | ConvertTo-Json | Set-Content (Join-Path $out 'identity.json')
Get-Content (Join-Path $out 'identity.json')
if ($null -ne $credits -and [double]$credits -gt 30) { throw 'Unexpected cost: stop for review, do not submit another task' }
