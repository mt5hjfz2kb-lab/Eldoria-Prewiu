param(
 [Parameter(Mandatory=$true)][string]$InputRoot,
 [Parameter(Mandatory=$true)][string]$AssetName,
 [Parameter(Mandatory=$true)][string]$ExpectedSha256,
 [Parameter(Mandatory=$true)][string]$UnityEditor,
 [Parameter(Mandatory=$true)][string]$EditorSource,
 [Parameter(Mandatory=$true)][string]$SourceRun
)
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
if ($AssetName -notin @('Eldoria_Spawnling_Bastion2_v1','Eldoria_RiftFocus_v1')) { throw "Unsupported candidate: $AssetName" }
if ($ExpectedSha256 -notmatch '^[0-9a-fA-F]{64}$') { throw 'Exact expected GLB SHA-256 required.' }
$root = Join-Path $env:RUNNER_TEMP ('eldoria-tripo-qa-' + $AssetName)
$project = Join-Path $root 'Project'
$output = Join-Path $root 'Output'
if (Test-Path $root) { Remove-Item $root -Recurse -Force }
foreach ($d in @($project,$output,(Join-Path $project 'Assets\Editor'),(Join-Path $project 'Packages'),(Join-Path $project 'ProjectSettings'))) {
  New-Item -ItemType Directory -Path $d -Force | Out-Null
}
$files = @(Get-ChildItem $InputRoot -Filter 'Eldoria_Module_UnderReview_50K.glb' -File -Recurse -ErrorAction Stop)
if ($files.Count -ne 1) { throw "Expected exactly one optimized GLB, found $($files.Count)." }
$source = $files[0].FullName
$sha = (Get-FileHash -Algorithm SHA256 $source).Hash.ToLowerInvariant()
if ($sha -ne $ExpectedSha256.ToLowerInvariant()) { throw "GLB identity mismatch expected=$ExpectedSha256 actual=$sha" }
Copy-Item $source (Join-Path $project 'Assets\TripoSubject.glb')
Copy-Item $EditorSource (Join-Path $project 'Assets\Editor\TripoAssetMinimalReview.cs')
$manifest = '{"dependencies":{"com.unity.cloud.gltfast":"6.14.1","com.unity.modules.imageconversion":"1.0.0","com.unity.modules.unitywebrequest":"1.0.0","com.unity.modules.unitywebrequesttexture":"1.0.0"}}'
[IO.File]::WriteAllText((Join-Path $project 'Packages\manifest.json'),$manifest)
'm_EditorVersion: 6000.3.23f1' | Set-Content (Join-Path $project 'ProjectSettings\ProjectVersion.txt')
@{
  asset=$AssetName; source_run=$SourceRun; sha256=$sha;
  size_bytes=(Get-Item $source).Length;
  additional_tripo_credits=0;
  qa_kind='isolated_unity_no_gameplay_no_paid_generation';
} | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $output 'source-identity.json')
$env:ELDORIA_TRIPO_QA_ASSET = $AssetName
$env:ELDORIA_TRIPO_QA_OUTPUT = $output
if (-not (Test-Path $UnityEditor)) { throw "Unity binary not found at supplied path" }
$log = Join-Path $output 'UnityTripoAsset.log'
Write-Host "TRIPO_QA_START asset=$AssetName sha=$sha source_run=$SourceRun"
$args = @('-batchmode','-quit','-projectPath',$project,'-executeMethod','TripoAssetMinimalReview.Capture','-logFile',$log,'-refreshImportMode','InProcess','-job-worker-count','1')
$p=Start-Process -FilePath $UnityEditor -ArgumentList $args -PassThru
if (-not $p.WaitForExit(840000)) {
  & taskkill.exe /PID $p.Id /T /F | Out-Null
  if (Test-Path $log) { Get-Content $log -Tail 100 }
  throw 'Isolated source QA Unity timeout after 14 minutes.'
}
if ($p.ExitCode -ne 0) {
  if (Test-Path $log) { Get-Content $log -Tail 120 }
  throw "Isolated source QA Unity failure code $($p.ExitCode)"
}
$metrics=Join-Path $output 'tripo-source-metrics.json'
if (-not (Test-Path $metrics)) { throw 'Missing QA metrics' }
$m=Get-Content $metrics -Raw | ConvertFrom-Json
if ($m.asset -ne $AssetName -or -not $m.uv0AllMeshes -or -not $m.normalsAllMeshes -or -not $m.fullResolutionTexturePass -or -not $m.positiveSelection -or -not $m.emptySelectionMiss) { throw 'QA metrics contract failed.' }
foreach ($file in @('iso.png','side.png','front.png','strategic.png')) {
  $p=Join-Path $output $file
  if (-not (Test-Path $p) -or (Get-Item $p).Length -lt 15000) { throw "Missing or suspicious preview $file" }
}
Get-Content $metrics
Write-Host "TRIPO_QA_SUCCESS asset=$AssetName existing_sha=$sha no_tripo_credit_spend=true"
