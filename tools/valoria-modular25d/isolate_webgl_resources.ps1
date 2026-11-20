param([Parameter(Mandatory=$true)][string]$SourceRoot)
$ErrorActionPreference='Stop'
# Isolation applies only to this run's own SHA-pinned Windows scratch.
# Unity's official build rules include EVERY Resource-folder asset even if
# an isolated scene does not reference it. Preserve only the experiment's
# original modular assets; archive unrelated Eldoria Resources outside Assets.
$SourceRoot=[IO.Path]::GetFullPath($SourceRoot)
$temp=[IO.Path]::GetFullPath($env:RUNNER_TEMP)
if(-not $env:GITHUB_SHA -or -not $env:GITHUB_RUN_ID){throw 'Missing run provenance'}
if(-not $SourceRoot.StartsWith(($temp.TrimEnd([IO.Path]::DirectorySeparatorChar)+[IO.Path]::DirectorySeparatorChar),[StringComparison]::OrdinalIgnoreCase) -or
   -not([IO.Path]::GetFileName($SourceRoot) -eq ('eldoria-mod25d-'+$env:GITHUB_RUN_ID))){
  throw 'Refuse touching other checkouts or any canonical game project'
}
$head=(git -C $SourceRoot rev-parse HEAD).Trim()
if($LASTEXITCODE -ne 0 -or $head -ne $env:GITHUB_SHA){throw 'Source not pinned to run commit'}
$assets=Join-Path $SourceRoot 'Unity/Assets'
$canonical=Join-Path $assets 'Eldoria'
$experiment=Join-Path $assets 'Experimentos/ValoriaModular25D/Resources/ValoriaModular25D'
if(-not(Test-Path -LiteralPath $canonical -PathType Container)){throw 'Canonical source absent'}
foreach($asset in 'bastion_i.png','sawmill_i.png','OrganicMeadowTile.png','OrganicCobbleRoad.png'){
  if(-not(Test-Path -LiteralPath (Join-Path $experiment $asset) -PathType Leaf)){
    throw ('Required original 2.5D art missing '+$asset)
  }
}
$destination=Join-Path $temp ('eldoria-mod25d-external-resources-'+$env:GITHUB_RUN_ID)
if(Test-Path -LiteralPath $destination){throw 'Refuse replacing archive from another phase'}
New-Item -ItemType Directory -Path $destination|Out-Null
$found=@(Get-ChildItem -LiteralPath $canonical -Directory -Recurse -Force | Where-Object{$_.Name -ceq 'Resources'})
# Do not move nested Resources twice; root ancestor includes its children.
$roots=@($found|Where-Object{
  $current=$_.FullName
  -not @($found|Where-Object{
    $_.FullName.Length -lt $current.Length -and
    $current.StartsWith(($_.FullName+[IO.Path]::DirectorySeparatorChar),[StringComparison]::OrdinalIgnoreCase)
  }).Count
})
$manifest=@();[long]$total=0
foreach($root in $roots){
  if(($root.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'Symlink resource forbidden'}
  $relative=$root.FullName.Substring($assets.Length).TrimStart([IO.Path]::DirectorySeparatorChar)
  if(-not $relative.StartsWith(('Eldoria'+[IO.Path]::DirectorySeparatorChar),[StringComparison]::OrdinalIgnoreCase)){
    throw ('Outside canonical assets '+$relative)
  }
  $files=@(Get-ChildItem -LiteralPath $root.FullName -File -Recurse -Force)
  if(@($files|Where-Object{($_.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0}).Count){
    throw ('File link inside Resources '+$relative)
  }
  [long]$size=($files|Measure-Object -Property Length -Sum).Sum
  $to=Join-Path $destination $relative
  New-Item -ItemType Directory -Force -Path (Split-Path -Parent $to)|Out-Null
  Move-Item -LiteralPath $root.FullName -Destination $to
  $meta=($root.FullName+'.meta')
  if(Test-Path -LiteralPath $meta){Move-Item -LiteralPath $meta -Destination ($to+'.meta')}
  if(Test-Path -LiteralPath $root.FullName){throw 'Resource was not isolated'}
  $manifest+=@{path=$relative;files=$files.Count;sourceBytes=$size}
  $total+=$size
}
foreach($asset in 'bastion_i.png','sawmill_i.png','OrganicMeadowTile.png','OrganicCobbleRoad.png'){
  if(-not(Test-Path -LiteralPath (Join-Path $experiment $asset))){
    throw ('Sawmill or Bastion original sprite was affected: '+$asset)
  }
}
$report=Join-Path $SourceRoot 'Unity/Artifacts/ValoriaModular25D/WebGLResourceIsolation.json'
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $report)|Out-Null
(@{sourceSha=$head;scope='ONLY_UNRELATED_ELDORIA_RESOURCES_IN_OWN_DISPOSABLE_WEBGL_SCRATCH';
  excludedDirectoryCount=$manifest.Count;excludedSourceBytes=$total;dirs=$manifest;
  originalModularResourcesUnchanged=$true;mainGitSourcesUnchanged=$true;
  browserAcceptance='PENDING';deviceAcceptance='NOT_TESTED'
 }|ConvertTo-Json -Depth 8)|Set-Content -LiteralPath $report -Encoding UTF8
Write-Host "MOD25D_WEBGL_RESOURCES_ISOLATED dirs=$($manifest.Count) bytes=$total; source art intact."
