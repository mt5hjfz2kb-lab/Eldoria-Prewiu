# Fetch exactly the official CC0 assets in the checked-in manifest before opening Unity.
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$manifest = Get-Content (Join-Path $PSScriptRoot 'valoria-art-gate-assets.json') -Raw | ConvertFrom-Json
foreach ($asset in $manifest.assets) {
    $destination = Join-Path $repoRoot $asset.path
    New-Item -ItemType Directory -Force -Path (Split-Path $destination) | Out-Null
    if ((Test-Path $destination) -and (Get-FileHash $destination -Algorithm MD5).Hash.ToLowerInvariant() -eq $asset.md5) {
        continue
    }
    $temporary = "$destination.download"
    try {
        Invoke-WebRequest -Uri $asset.url -UseBasicParsing -OutFile $temporary
        $actual = (Get-FileHash $temporary -Algorithm MD5).Hash.ToLowerInvariant()
        if ($actual -ne $asset.md5) { throw "Asset checksum mismatch: $($asset.path)" }
        Move-Item -Force $temporary $destination
    } finally {
        Remove-Item $temporary -Force -ErrorAction SilentlyContinue
    }
}
Write-Host "Valoria asset gate: $($manifest.assets.Count) official CC0 files verified."
