param(
  [Parameter(Mandatory=$true)][string]$InputRoot,
  [Parameter(Mandatory=$true)][string]$EditorScript,
  [Parameter(Mandatory=$true)][string]$UnityEditor
)
$ErrorActionPreference = "Stop"
$ProgressPreference = 'SilentlyContinue'
$root = Join-Path $env:RUNNER_TEMP 'eldoria-boar-unity-isolated-20261020'
$project = Join-Path $root 'Project'
$output = Join-Path $root 'Output'
if (Test-Path $root) { Remove-Item $root -Recurse -Force }
New-Item -ItemType Directory -Force $project, $output, (Join-Path $project 'Assets\Editor'), (Join-Path $project 'Packages'), (Join-Path $project 'ProjectSettings') | Out-Null

$models = @(Get-ChildItem $InputRoot -Recurse -File -Filter 'Eldoria_Module_UnderReview_50K.glb' -ErrorAction Stop)
if ($models.Count -ne 1) { throw "Expected exactly one previously-generated optimized boar GLB in artifact; found $($models.Count)." }
$source = $models[0].FullName
$sha = (Get-FileHash $source -Algorithm SHA256).Hash.ToLowerInvariant()
if ($sha -ne '897c8d02a445c6c760376031c38618698740e9593346099fafb4c3a0b93c1dc3') {
  throw "Existing boar optimized SHA mismatch: $sha. Refusing wrong model."
}
Copy-Item $source (Join-Path $project 'Assets\RockBoar.glb') -Force
Copy-Item $EditorScript (Join-Path $project 'Assets\Editor\RockBoarMinimalReview.cs') -Force
$manifest = '{"dependencies":{"com.unity.cloud.gltfast":"6.14.1","com.unity.modules.imageconversion":"1.0.0","com.unity.modules.unitywebrequest":"1.0.0","com.unity.modules.unitywebrequesttexture":"1.0.0"}}'
[IO.File]::WriteAllText((Join-Path $project 'Packages\manifest.json'),$manifest)
@('m_EditorVersion: 6000.3.23f1','m_EditorVersionWithRevision: 6000.3.23f1 (09d2ecc7fb28)') | Set-Content (Join-Path $project 'ProjectSettings\ProjectVersion.txt')
@{
  model = "Eldoria_RockBoar_FirstHunt_v1"
  sourceSha256 = $sha
  sourceBytes = (Get-Item $source).Length
  sourceRun = 39495619494
  sourceArtifact = 12297913548
  tripoTask = "28f57ff9-b978-468c-8c2a-6136e0a65ea1"
  additionalTripoCreditsSpent = 0
  unity = "6000.3.23f1"
  mode = "minimal_isolated_no_project_import_no_paid_generation"
} | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $output 'source-identity.json')
$env:ELDORIA_BOAR_QA_OUTPUT = $output
$log = Join-Path $output 'UnityRockBoar.log'
if (-not (Test-Path $UnityEditor)) { throw "Configured Unity editor not found: $UnityEditor" }
Write-Host "BOAR_ISOLATED_UNITY_BEGIN sha256=$sha project=$project source=$source"
$arguments = @('-batchmode','-quit','-projectPath',$project,'-executeMethod','RockBoarMinimalReview.Capture','-logFile',$log,'-refreshImportMode','InProcess','-job-worker-count','1')
$process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -PassThru
if (-not $process.WaitForExit(840000)) {
  Write-Host 'Unity did not complete isolated test within 14 minutes.'
  & taskkill.exe /PID $process.Id /T /F | Out-Null
  if (Test-Path $log) { Get-Content $log -Tail 80 }
  throw 'Rock Boar isolated Unity gate timed out after 14 minutes.'
}
if ($process.ExitCode -ne 0) {
  if (Test-Path $log) { Get-Content $log -Tail 180 }
  throw "Rock Boar isolated Unity gate failed with code $($process.ExitCode)"
}
$metrics = Join-Path $output 'boar-unity-metrics.json'
if (-not (Test-Path $metrics)) { if (Test-Path $log) { Get-Content $log -Tail 120 }; throw 'No Unity model metrics were produced.' }
$expected = @('boar-unity-isometric-textured.png','boar-unity-side-textured.png','boar-unity-front-textured.png','boar-unity-strategic-textured.png')
foreach ($name in $expected) {
  $png = Join-Path $output $name
  if (-not (Test-Path $png) -or (Get-Item $png).Length -lt 20000) { throw "Missing genuine Unity render: $name" }
}
Get-Content $metrics
Write-Host 'BOAR_ISOLATED_UNITY_PASS: real model imported and captured in Unity 6000.3.23f1. NO Tripo credits spent.'
