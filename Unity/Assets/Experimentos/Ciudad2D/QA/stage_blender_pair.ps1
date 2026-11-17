param([Parameter(Mandatory=$true)][string]$SourceRoot)
$ErrorActionPreference='Stop'
$req=Get-Content (Join-Path $SourceRoot 'tools/ciudad2d-native/request.json') -Raw | ConvertFrom-Json
if($req.blender_artifact_id -lt 1 -or $req.blender_artifact_sha -notmatch '^[a-f0-9]{40}$'){throw 'Blender artifact/run SHA must be pinned'}
$api="https://api.github.com/repos/$env:GITHUB_REPOSITORY/actions/artifacts/$($req.blender_artifact_id)"
$headers=@{Authorization="Bearer $env:GH_TOKEN";Accept='application/vnd.github+json'}
$art=Invoke-RestMethod -Uri $api -Headers $headers
if($art.expired -or $art.workflow_run.head_sha -ne $req.blender_artifact_sha){throw 'Artifact provenance does not match pinned Blender source'}
$zip=Join-Path $env:RUNNER_TEMP ('city2d-blender-'+$env:GITHUB_RUN_ID+'.zip')
$unpack=Join-Path $env:RUNNER_TEMP ('city2d-blender-'+$env:GITHUB_RUN_ID)
Invoke-WebRequest -Uri ($api+'/zip') -Headers $headers -MaximumRedirection 8 -OutFile $zip
Expand-Archive -LiteralPath $zip -DestinationPath $unpack
$src=Join-Path $unpack 'city2d-blender'
if(-not(Test-Path $src)){throw 'Missing source assets from official Blender artifact'}
$manifest=Get-Content (Join-Path $src 'blender-pilot-manifest.json') -Raw | ConvertFrom-Json
$contract=Get-Content (Join-Path $src 'unity-sprite-contract.json') -Raw | ConvertFrom-Json
if($manifest.kit_ref -ne 'art/visual-reboot@84bb8e7a83810f4b722c764938a5d36e20b906d5'){throw 'Kit SHA drift'}
if((Get-FileHash (Join-Path $src 'Valoria_Buildings_Master.blend') -Algorithm SHA256).Hash.ToLowerInvariant() -ne $contract.blender_master_sha256){throw 'Blender master hash mismatch'}
$dst=Join-Path $SourceRoot 'Unity/Assets/Experimentos/Ciudad2D/Sprites/BlenderPilot'
New-Item -ItemType Directory -Path $dst -Force | Out-Null
foreach($name in @('Cuartel','Aserradero')){
  $line=$contract.buildings.$name
  foreach($entry in @(@{file="$($name)_Ready.png";sha=$line.ready_sha256},@{file="$($name)_ShadowReady.png";sha=$line.shadow_ready_sha256})){
    $from=Join-Path $src $entry.file
    if(-not(Test-Path $from) -or (Get-FileHash $from -Algorithm SHA256).Hash.ToLowerInvariant() -ne $entry.sha){throw ('Sprite SHA mismatch '+$entry.file)}
    Copy-Item -LiteralPath $from -Destination (Join-Path $dst $entry.file)
  }
}
Copy-Item -LiteralPath (Join-Path $src 'unity-sprite-contract.json') -Destination (Join-Path $dst 'origin-contract.json')
Write-Host ('CITY2D_BLENDER_SOURCE_STAGE_PASS artifact='+$req.blender_artifact_id)
