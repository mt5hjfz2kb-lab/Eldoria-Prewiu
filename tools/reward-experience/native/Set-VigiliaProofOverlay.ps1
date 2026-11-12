param(
 [Parameter(Mandatory=$true)][ValidateSet('Install','Restore')][string]$Operation,
 [Parameter(Mandatory=$true)][string]$Project,
 [Parameter(Mandatory=$true)][string]$Manifest,
 [string]$UiProject='',
 [string]$UiSource='',
 [string]$ReviewedMetadata=''
)
$ErrorActionPreference='Stop'
function Hash([string]$p){(Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.ToLowerInvariant()}
function Full([string]$p){[IO.Path]::GetFullPath($p).TrimEnd([IO.Path]::DirectorySeparatorChar)}
function Owned([string]$rel){
 if($rel -eq 'Assets/Eldoria/BreachWatch.meta'){return}
 if(-not $rel.StartsWith('Assets/Eldoria/BreachWatch/') -or
    $rel.Contains('..') -or $rel.Contains(':')){throw "Unowned UI overlay path: $rel"}
}
function NoLinks([string]$p){
 if(Test-Path -LiteralPath $p){
  $item=Get-Item -LiteralPath $p -Force
  if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw "Refuse redirected path: $p"}
 }
}
$projectRoot=Full $Project
$manifestPath=[IO.Path]::GetFullPath($Manifest)
if($manifestPath.StartsWith($projectRoot+[IO.Path]::DirectorySeparatorChar,
 [StringComparison]::OrdinalIgnoreCase)){throw 'Manifest/backup must remain outside Unity project'}
NoLinks $projectRoot
if(-not(Test-Path -LiteralPath (Join-Path $projectRoot 'Assets/Eldoria') -PathType Container)){
 throw 'Not the isolated Unity project'
}
$targetRoot=Join-Path $projectRoot 'Assets/Eldoria/BreachWatch'
NoLinks $targetRoot
if(Test-Path -LiteralPath $targetRoot){
 Get-ChildItem -LiteralPath $targetRoot -Recurse -Force | ForEach-Object {NoLinks $_.FullName}
}
if($Operation -eq 'Restore'){
 $saved=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json
 if($saved.project -ne $projectRoot -or $saved.version -ne 1){throw 'Overlay restoration provenance mismatch'}
 $reviewed=@{};$accepted=@()
 if(-not $ReviewedMetadata){$ReviewedMetadata=Join-Path $PSScriptRoot 'reviewed-vigilia-imported-metadata.json'}
 if(Test-Path -LiteralPath $ReviewedMetadata){
  $review=Get-Content -LiteralPath $ReviewedMetadata -Raw|ConvertFrom-Json
  if($review.uiSource -eq $saved.ui_source -and $review.role -eq 'EXACT_REVIEWED_TEMPORARY_UI_METADATA_RESTORE_ONLY'){
   foreach($known in @($review.files)){
    Owned $known.path
    if(-not $known.path.EndsWith('.meta') -or $known.current -notmatch '^[a-f0-9]{64} Unknown edits are never
 # overwritten, even if earlier paths would otherwise be safe to restore.
 foreach($entry in @($saved.files)){
  Owned $entry.path
  $p=Join-Path $projectRoot $entry.path
  NoLinks $p
  $exists=Test-Path -LiteralPath $p -PathType Leaf
  if((Test-Path -LiteralPath $p) -and -not $exists){throw "Overlay path became a directory: $($entry.path)"}
  $current=$null
  if($exists){$current=Hash $p}
  $complete=Test-Path -LiteralPath ($manifestPath+'.installed')
  $valid=($entry.installed -and $current -eq $entry.installed) -or
         (-not $entry.installed -and -not $exists)
  if(-not $complete){
   # If installation stopped part-way, unchanged originals and not-yet-
   # created new paths are also safe. Any unfamiliar bytes remain refused.
   $valid=$valid -or ($entry.original -and $current -eq $entry.original) -or
          (-not $entry.original -and -not $exists)
  }
  if(-not $valid -and $reviewed.ContainsKey($entry.path)){
   $known=$reviewed[$entry.path]
   if($current -eq $known.current -and $entry.installed -eq $known.installed -and
      $entry.original -eq $known.original){
    $valid=$true;$accepted+=@{path=$entry.path;current=$current}
   }
  }
  if(-not $valid){throw "Overlay file changed after install; preserve it: $($entry.path)"}
  if($entry.original){
   $backup=Join-Path $saved.backup $entry.path
   if(-not(Test-Path -LiteralPath $backup -PathType Leaf) -or (Hash $backup) -ne $entry.original){
    throw "Original overlay backup changed: $($entry.path)"
   }
  }
 }
 # Archive every EXACT reviewed changed metadata byte before any restoration.
 # This never recognizes arbitrary importer changes or edits to source/art.
 foreach($known in $accepted){
  $from=Join-Path $projectRoot $known.path
  $copy=Join-Path ($manifestPath+'.reviewed-importer-files') $known.path
  [IO.Directory]::CreateDirectory((Split-Path $copy))|Out-Null
  if(Test-Path -LiteralPath $copy){if((Hash $copy) -ne $known.current){throw 'Different prior metadata archive'}}
  else{[IO.File]::Copy($from,$copy,$false)}
  if((Hash $copy) -ne $known.current -or (Hash $from) -ne $known.current){throw 'Reviewed metadata changed before restoration'}
 }
 Write-Host "ISSUE186_EXACT_REVIEWED_METADATA_ARCHIVED $($accepted.Count); NO_GENERIC_NORMALIZATION_ALLOWANCE"
 foreach($entry in @($saved.files)){
  $p=Join-Path $projectRoot $entry.path
  if($entry.original){
   [IO.Directory]::CreateDirectory((Split-Path $p))|Out-Null
   [IO.File]::Copy((Join-Path $saved.backup $entry.path),$p,$true)
  }elseif(Test-Path -LiteralPath $p){Remove-Item -LiteralPath $p}
 }
 foreach($entry in @($saved.files)){
  $p=Join-Path $projectRoot $entry.path
  if($entry.original){
   if((Hash $p) -ne $entry.original){throw "Original bytes not restored: $($entry.path)"}
  }elseif(Test-Path -LiteralPath $p){throw "New overlay file remained: $($entry.path)"}
 }
 [IO.File]::WriteAllText($manifestPath+'.restored.json',
  (@{restored=$true;files=@($saved.files).Count;project=$projectRoot;ui_source=$saved.ui_source}|ConvertTo-Json),
  (New-Object Text.UTF8Encoding($false)))
 Write-Host "ISSUE186_EXACT_VIGILIA_OVERLAY_RESTORED $(@($saved.files).Count)"
 exit 0
}
if($UiSource -notmatch '^[a-f0-9]{40}$'){throw 'Pinned original UI source SHA required'}
if(Test-Path -LiteralPath $manifestPath){throw 'Never replace an existing restoration manifest'}
$uiRoot=Full $UiProject
$sourceRoot=Join-Path $uiRoot 'Assets/Eldoria/BreachWatch'
NoLinks $uiRoot;NoLinks $sourceRoot
Get-ChildItem -LiteralPath $sourceRoot -Recurse -Force | ForEach-Object {NoLinks $_.FullName}
foreach($required in @('Runtime/BreachWatchHud.cs','Runtime/BreachWatchResourceBagBridge.cs')){
 if(-not(Test-Path -LiteralPath (Join-Path $sourceRoot $required) -PathType Leaf)){
  throw "Pinned real UI source missing: $required"
 }
}
$all=@{}
foreach($root in @($sourceRoot,$targetRoot)){
 if(Test-Path -LiteralPath $root){
  Get-ChildItem -LiteralPath $root -File -Recurse -Force | ForEach-Object {
   NoLinks $_.FullName
   $relative='Assets/Eldoria/BreachWatch/'+$_.FullName.Substring($root.Length+1).Replace('\','/')
   Owned $relative;$all[$relative]=$true
  }
 }
}
foreach($base in @($uiRoot,$projectRoot)){
 $meta=Join-Path $base 'Assets/Eldoria/BreachWatch.meta'
 if(Test-Path -LiteralPath $meta){NoLinks $meta;$all['Assets/Eldoria/BreachWatch.meta']=$true}
}
$backup=$manifestPath+'.original-files'
if(Test-Path -LiteralPath $backup){throw 'Original backup directory already exists'}
[IO.Directory]::CreateDirectory($backup)|Out-Null
$entries=@()
foreach($relative in @($all.Keys|Sort-Object)){
 $from=Join-Path $uiRoot $relative;$to=Join-Path $projectRoot $relative
 if((Test-Path -LiteralPath $to) -and -not(Test-Path -LiteralPath $to -PathType Leaf)){
  throw "Target file path is a directory: $relative"
 }
 $old=$null;$installed=$null;$preserveMeta=$false
 if(Test-Path -LiteralPath $to -PathType Leaf){
  $old=Hash $to;$copy=Join-Path $backup $relative
  [IO.Directory]::CreateDirectory((Split-Path $copy))|Out-Null
  [IO.File]::Copy($to,$copy,$false)
  if((Hash $copy) -ne $old){throw "Backup mismatch: $relative"}
 }
 if(Test-Path -LiteralPath $from -PathType Leaf){$installed=Hash $from}
 elseif($old -and $relative.EndsWith('.meta') -and
   (Test-Path -LiteralPath ($from.Substring(0,$from.Length-5)))){
  # The pinned UI omits these non-runtime folder/preview/manifest metadata.
  # Keep the existing original GUID instead of forcing Unity to invent one.
  $installed=$old;$preserveMeta=$true
 }
 $entries+=@{path=$relative;original=$old;installed=$installed;preserved_metadata=$preserveMeta}
}
[IO.Directory]::CreateDirectory((Split-Path $manifestPath))|Out-Null
$record=@{version=1;project=$projectRoot;ui_source=$UiSource;backup=$backup;files=$entries}
[IO.File]::WriteAllText($manifestPath,($record|ConvertTo-Json -Depth 8),
 (New-Object Text.UTF8Encoding($false)))
foreach($entry in $entries){
 $to=Join-Path $projectRoot $entry.path;$from=Join-Path $uiRoot $entry.path
 if($entry.preserved_metadata){$from=Join-Path $backup $entry.path}
 if($entry.installed){
  [IO.Directory]::CreateDirectory((Split-Path $to))|Out-Null
  [IO.File]::Copy($from,$to,$true)
  if((Hash $to) -ne $entry.installed){throw "Installed source differs: $($entry.path)"}
 }elseif(Test-Path -LiteralPath $to){Remove-Item -LiteralPath $to}
}
[IO.File]::WriteAllText($manifestPath+'.installed','EXACT_SOURCE_FILES_INSTALLED',
 (New-Object Text.UTF8Encoding($false)))
Write-Host "ISSUE186_PINNED_VIGILIA_OVERLAY_INSTALLED $($entries.Count); RESTORE_MANIFEST_RETAINED"
 -or $reviewed.ContainsKey($known.path)){throw 'Invalid exact reviewed metadata entry'}
    $reviewed[$known.path]=$known
   }
  }
 }
 # Validate the WHOLE set before modifying anything. Unknown edits are never
 # overwritten, even if earlier paths would otherwise be safe to restore.
 foreach($entry in @($saved.files)){
  Owned $entry.path
  $p=Join-Path $projectRoot $entry.path
  NoLinks $p
  $exists=Test-Path -LiteralPath $p -PathType Leaf
  if((Test-Path -LiteralPath $p) -and -not $exists){throw "Overlay path became a directory: $($entry.path)"}
  $current=$null
  if($exists){$current=Hash $p}
  $complete=Test-Path -LiteralPath ($manifestPath+'.installed')
  $valid=($entry.installed -and $current -eq $entry.installed) -or
         (-not $entry.installed -and -not $exists)
  if(-not $complete){
   # If installation stopped part-way, unchanged originals and not-yet-
   # created new paths are also safe. Any unfamiliar bytes remain refused.
   $valid=$valid -or ($entry.original -and $current -eq $entry.original) -or
          (-not $entry.original -and -not $exists)
  }
  if(-not $valid){throw "Overlay file changed after install; preserve it: $($entry.path)"}
  if($entry.original){
   $backup=Join-Path $saved.backup $entry.path
   if(-not(Test-Path -LiteralPath $backup -PathType Leaf) -or (Hash $backup) -ne $entry.original){
    throw "Original overlay backup changed: $($entry.path)"
   }
  }
 }
 foreach($entry in @($saved.files)){
  $p=Join-Path $projectRoot $entry.path
  if($entry.original){
   [IO.Directory]::CreateDirectory((Split-Path $p))|Out-Null
   [IO.File]::Copy((Join-Path $saved.backup $entry.path),$p,$true)
  }elseif(Test-Path -LiteralPath $p){Remove-Item -LiteralPath $p}
 }
 foreach($entry in @($saved.files)){
  $p=Join-Path $projectRoot $entry.path
  if($entry.original){
   if((Hash $p) -ne $entry.original){throw "Original bytes not restored: $($entry.path)"}
  }elseif(Test-Path -LiteralPath $p){throw "New overlay file remained: $($entry.path)"}
 }
 [IO.File]::WriteAllText($manifestPath+'.restored.json',
  (@{restored=$true;files=@($saved.files).Count;project=$projectRoot;ui_source=$saved.ui_source}|ConvertTo-Json),
  (New-Object Text.UTF8Encoding($false)))
 Write-Host "ISSUE186_EXACT_VIGILIA_OVERLAY_RESTORED $(@($saved.files).Count)"
 exit 0
}
if($UiSource -notmatch '^[a-f0-9]{40}$'){throw 'Pinned original UI source SHA required'}
if(Test-Path -LiteralPath $manifestPath){throw 'Never replace an existing restoration manifest'}
$uiRoot=Full $UiProject
$sourceRoot=Join-Path $uiRoot 'Assets/Eldoria/BreachWatch'
NoLinks $uiRoot;NoLinks $sourceRoot
Get-ChildItem -LiteralPath $sourceRoot -Recurse -Force | ForEach-Object {NoLinks $_.FullName}
foreach($required in @('Runtime/BreachWatchHud.cs','Runtime/BreachWatchResourceBagBridge.cs')){
 if(-not(Test-Path -LiteralPath (Join-Path $sourceRoot $required) -PathType Leaf)){
  throw "Pinned real UI source missing: $required"
 }
}
$all=@{}
foreach($root in @($sourceRoot,$targetRoot)){
 if(Test-Path -LiteralPath $root){
  Get-ChildItem -LiteralPath $root -File -Recurse -Force | ForEach-Object {
   NoLinks $_.FullName
   $relative='Assets/Eldoria/BreachWatch/'+$_.FullName.Substring($root.Length+1).Replace('\','/')
   Owned $relative;$all[$relative]=$true
  }
 }
}
foreach($base in @($uiRoot,$projectRoot)){
 $meta=Join-Path $base 'Assets/Eldoria/BreachWatch.meta'
 if(Test-Path -LiteralPath $meta){NoLinks $meta;$all['Assets/Eldoria/BreachWatch.meta']=$true}
}
$backup=$manifestPath+'.original-files'
if(Test-Path -LiteralPath $backup){throw 'Original backup directory already exists'}
[IO.Directory]::CreateDirectory($backup)|Out-Null
$entries=@()
foreach($relative in @($all.Keys|Sort-Object)){
 $from=Join-Path $uiRoot $relative;$to=Join-Path $projectRoot $relative
 if((Test-Path -LiteralPath $to) -and -not(Test-Path -LiteralPath $to -PathType Leaf)){
  throw "Target file path is a directory: $relative"
 }
 $old=$null;$installed=$null
 if(Test-Path -LiteralPath $to -PathType Leaf){
  $old=Hash $to;$copy=Join-Path $backup $relative
  [IO.Directory]::CreateDirectory((Split-Path $copy))|Out-Null
  [IO.File]::Copy($to,$copy,$false)
  if((Hash $copy) -ne $old){throw "Backup mismatch: $relative"}
 }
 if(Test-Path -LiteralPath $from -PathType Leaf){$installed=Hash $from}
 $entries+=@{path=$relative;original=$old;installed=$installed}
}
[IO.Directory]::CreateDirectory((Split-Path $manifestPath))|Out-Null
$record=@{version=1;project=$projectRoot;ui_source=$UiSource;backup=$backup;files=$entries}
[IO.File]::WriteAllText($manifestPath,($record|ConvertTo-Json -Depth 8),
 (New-Object Text.UTF8Encoding($false)))
foreach($entry in $entries){
 $to=Join-Path $projectRoot $entry.path;$from=Join-Path $uiRoot $entry.path
 if($entry.installed){
  [IO.Directory]::CreateDirectory((Split-Path $to))|Out-Null
  [IO.File]::Copy($from,$to,$true)
  if((Hash $to) -ne $entry.installed){throw "Installed source differs: $($entry.path)"}
 }elseif(Test-Path -LiteralPath $to){Remove-Item -LiteralPath $to}
}
[IO.File]::WriteAllText($manifestPath+'.installed','EXACT_SOURCE_FILES_INSTALLED',
 (New-Object Text.UTF8Encoding($false)))
Write-Host "ISSUE186_PINNED_VIGILIA_OVERLAY_INSTALLED $($entries.Count); RESTORE_MANIFEST_RETAINED"
