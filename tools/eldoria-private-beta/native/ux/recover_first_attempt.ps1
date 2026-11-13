param([Parameter(Mandatory=$true)][string]$Source,[Parameter(Mandatory=$true)][string]$Evidence)
$ErrorActionPreference='Stop'
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native tools busy; preserve inputs'}
$original=@{
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaIntegrationBuild.cs'='4D84DC082FA107BCBD9B0DE03E1AD24D6CF0154F01A73150B9B9815846693478'
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs'='85533B5441B2F63D6E89A41C0561DC3689793A76A1D71375970241E4361E444C'
}
$manifest=Get-Content (Join-Path $PSScriptRoot 'manifest.json') -Raw|ConvertFrom-Json
$backupRoot=Join-Path $env:GITHUB_WORKSPACE 'beta-structural-preserved-inputs-42585946027-1'
$pending=@()
foreach($path in $original.Keys){
 $target=Join-Path $Source $path
 $current=(Get-FileHash $target -Algorithm SHA256).Hash
 if($current -eq $original[$path]){continue}
 $entry=@($manifest.files|Where-Object {$_.path -eq $path})
 if($entry.Count -ne 1 -or $current -ne $entry[0].baseline_sha256){throw "Not the known intermediate overlay: $path"}
 $backup=Join-Path $backupRoot $path
 if(-not(Test-Path $backup) -or (Get-FileHash $backup -Algorithm SHA256).Hash -ne $original[$path]){throw "Original backup changed: $path"}
 $pending+=@{path=$path;target=$target;backup=$backup;before=$current;original=$original[$path]}
}
foreach($item in $pending){Copy-Item $item.backup $item.target -Force}
New-Item -ItemType Directory -Force $Evidence|Out-Null
$pending|ConvertTo-Json -Depth 5|Set-Content (Join-Path $Evidence 'known-intermediate-recovery.json') -Encoding UTF8
Write-Host "Recovered known intermediate source files: $($pending.Count); all other inputs untouched"
