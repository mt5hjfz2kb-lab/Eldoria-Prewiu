param([Parameter(Mandatory=$true)][string]$Workspace,[Parameter(Mandatory=$true)][string]$Output)
$ErrorActionPreference='Stop'
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Refuse preserved-source audit while a native process runs'}
$project=[IO.Path]::GetFullPath((Join-Path $Workspace 'issue186-source/Unity')).TrimEnd([IO.Path]::DirectorySeparatorChar)
$manifest=Join-Path $Workspace 'issue186-native-43146245415/vigilia-overlay.json'
if(-not(Test-Path -LiteralPath $manifest)){Write-Host 'NO_PRIOR_R19_OVERLAY_MANIFEST';exit 0}
if(Test-Path -LiteralPath ($manifest+'.restored.json')){
 $r=Get-Content -LiteralPath ($manifest+'.restored.json') -Raw|ConvertFrom-Json
 if($r.restored -and $r.project -eq $project){Write-Host 'R19_PRIOR_EXACT_RESTORE_ALREADY_RECORDED';exit 0}
 throw 'Invalid previous restoration marker'
}
$saved=Get-Content -LiteralPath $manifest -Raw|ConvertFrom-Json
if($saved.version -ne 1 -or $saved.project -ne $project -or $saved.ui_source -ne 'af658a6cc1a02d45d7efc9c57dafef24e8512564'){throw 'Prior preserved-overlay provenance mismatch'}
function Hash([string]$p){(Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.ToLowerInvariant()}
function NoLink([string]$p){if((Test-Path -LiteralPath $p)-and ((Get-Item -LiteralPath $p -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw "Refuse redirected preserved path: $p"}}
NoLink $project;NoLink $manifest;NoLink $saved.backup
$archive=Join-Path $Output 'preserved-r19-overlay-audit'
[IO.Directory]::CreateDirectory($archive)|Out-Null
[IO.File]::Copy($manifest,(Join-Path $archive 'original-manifest.json'),$false)
$drift=@();$checked=0
foreach($e in @($saved.files)){
 if(($e.path -ne 'Assets/Eldoria/BreachWatch.meta') -and (-not $e.path.StartsWith('Assets/Eldoria/BreachWatch/') -or $e.path.Contains('..') -or $e.path.Contains(':'))){throw 'Unowned preserved overlay entry'}
 $p=Join-Path $project $e.path;NoLink $p
 $current=$null;if(Test-Path -LiteralPath $p -PathType Leaf){$current=Hash $p}
 $checked++
 if($e.original){$b=Join-Path $saved.backup $e.path;NoLink $b;if(-not(Test-Path -LiteralPath $b)-or (Hash $b) -ne $e.original){throw 'Prior original backup failed exact SHA'} }
 if($current -eq $e.installed){continue}
 $record=@{path=$e.path;original=$e.original;installed=$e.installed;current=$current;source_mutated=$false}
 if($current){
  $dest=Join-Path $archive ('current/'+$e.path);[IO.Directory]::CreateDirectory((Split-Path $dest))|Out-Null
  [IO.File]::Copy($p,$dest,$false);if((Hash $dest) -ne $current){throw 'Preserved unknown bytes were not archived exactly'}
 }
 $installed=Join-Path $Workspace ('issue186-vigilia-proof-source/Unity/'+$e.path);NoLink $installed
 if($e.installed -and (Test-Path -LiteralPath $installed) -and (Hash $installed) -eq $e.installed){
  $dest=Join-Path $archive ('installed/'+$e.path);[IO.Directory]::CreateDirectory((Split-Path $dest))|Out-Null;[IO.File]::Copy($installed,$dest,$false)
  if($e.path.EndsWith('.meta') -and (Get-Item $installed).Length -lt 20000){$record.installed_text=[IO.File]::ReadAllText($installed)}
 }
 if($current -and $e.path.EndsWith('.meta') -and (Get-Item $p).Length -lt 20000){$record.current_text=[IO.File]::ReadAllText($p)}
 $drift+= $record
}
$report=@{status='READ_ONLY_PRESERVED_OVERLAY_AUDIT';checked=$checked;drift=$drift;source_mutated=$false;restored=$false}
$json=$report|ConvertTo-Json -Depth 12
[IO.File]::WriteAllText((Join-Path $archive 'report.json'),$json,(New-Object Text.UTF8Encoding($false)))
Write-Host $json
throw 'PRESERVED_OVERLAY_AUDITED_AND_ARCHIVED; REVIEW BEFORE ANY SOURCE CHECKOUT OR RESTORATION'
