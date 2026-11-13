param(
  [Parameter(Mandatory=$true)][string]$Project,
  [Parameter(Mandatory=$true)][string]$Manifest,
  [Parameter(Mandatory=$true)][string]$Output
)
$ErrorActionPreference='Stop'
# Read-only audit of a temporary Vigilia overlay that did not restore.
# It NEVER changes project/manifest/backup files and NEVER claims cleanup.
function Full([string]$p){[IO.Path]::GetFullPath($p).TrimEnd([IO.Path]::DirectorySeparatorChar)}
function Hash([string]$p){(Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.ToLowerInvariant()}
function NoLinks([string]$p){
  if(Test-Path -LiteralPath $p){
    $item=Get-Item -LiteralPath $p -Force
    if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw "Redirected path refused: $p"}
  }
}
function Owned([string]$rel){
  if(-not $rel.StartsWith('Assets/Eldoria/BreachWatch/') -and
     $rel -ne 'Assets/Eldoria/BreachWatch.meta'){throw "Unowned drift path: $rel"}
  if($rel -match '(^|/)\.\.(/|$)|:|\\'){throw "Unsafe drift path: $rel"}
}
$projectRoot=Full $Project
$manifestPath=Full $Manifest
$outputRoot=Full $Output
if(-not(Test-Path -LiteralPath $projectRoot -PathType Container)){throw 'Actual isolated project missing'}
if(-not(Test-Path -LiteralPath $manifestPath -PathType Leaf)){throw 'Recorded exact overlay manifest missing'}
if($outputRoot -eq $projectRoot -or $outputRoot.StartsWith(
   $projectRoot+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){
  throw 'Drift evidence must be outside the Unity project'
}
if(Test-Path -LiteralPath $outputRoot){throw 'Do not replace existing drift evidence'}
NoLinks $projectRoot
$saved=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json
if($saved.version -ne 1 -or $saved.project -ne $projectRoot -or
   -not $saved.files -or @($saved.files).Count -lt 1){throw 'Invalid overlay manifest provenance'}
$seen=@{};$changes=@();$unchanged=0
foreach($entry in @($saved.files)){
  $rel=[string]$entry.path
  Owned $rel
  if($seen.ContainsKey($rel)){throw "Duplicate overlay path: $rel"}
  $seen[$rel]=$true
  $absolute=Join-Path $projectRoot $rel
  $parent=Split-Path $absolute
  while($parent -and $parent.Length -ge $projectRoot.Length){
    NoLinks $parent
    if($parent -eq $projectRoot){break}
    $parent=Split-Path $parent
  }
  NoLinks $absolute
  if(Test-Path -LiteralPath $absolute){
    if(-not(Test-Path -LiteralPath $absolute -PathType Leaf)){throw "Unexpected directory: $rel"}
    $actual=Hash $absolute
    $bytes=(Get-Item -LiteralPath $absolute).Length
  }else{$actual=$null;$bytes=0}
  $expected=[string]$entry.installed
  if($expected -and $expected -notmatch '^[a-f0-9]{64}$'){throw "Invalid installed hash: $rel"}
  $isKnown=($expected -and $actual -eq $expected) -or (-not $expected -and -not $actual)
  if(-not $isKnown -and -not(Test-Path -LiteralPath ($manifestPath+'.installed'))){
    $isKnown=($entry.original -and $actual -eq $entry.original) -or
             (-not $entry.original -and -not $actual)
  }
  if($isKnown){$unchanged++;continue}
  $changes+=@{path=$rel;currentSha256=$actual;installedSha256=$expected
              originalSha256=$entry.original;bytes=$bytes;present=[bool]$actual}
}
[IO.Directory]::CreateDirectory($outputRoot)|Out-Null
$copied=@()
foreach($change in $changes){
  if(-not $change.present){continue}
  $src=Join-Path $projectRoot $change.path
  $dest=Join-Path (Join-Path $outputRoot 'changed-files') $change.path
  [IO.Directory]::CreateDirectory((Split-Path $dest))|Out-Null
  [IO.File]::Copy($src,$dest,$false)
  if((Hash $dest) -ne $change.currentSha256 -or (Hash $src) -ne $change.currentSha256){
    throw "Unknown bytes changed during preservation: $($change.path)"
  }
  $copied+=$change.path
}
$status=if($changes.Count){'REVIEW_REQUIRED'}else{'NO_DRIFT_DETECTED'}
$report=@{status=$status;project=$projectRoot;manifest=$manifestPath
  pinnedUiSource=$saved.ui_source;checked=@($saved.files).Count
  unchanged=$unchanged;changed=@($changes).Count;changedPaths=$changes;archived=$copied
  projectModified=$false;restorePerformed=$false;safeToAssumeUnityImporter=$false}
[IO.File]::WriteAllText((Join-Path $outputRoot 'report.json'),
  ($report|ConvertTo-Json -Depth 9),(New-Object Text.UTF8Encoding($false)))
Write-Host "ISSUE186_OVERLAY_DRIFT_ARCHIVED_FOR_REVIEW changed=$($changes.Count) no_project_writes=true"
