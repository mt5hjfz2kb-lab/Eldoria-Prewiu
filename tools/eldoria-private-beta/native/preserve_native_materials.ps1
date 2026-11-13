function Get-PR179NativeMaterialPaths {
 @(
  'Unity/Assets/Holotna/Mountain/Materials/Bush01.mat'
  'Unity/Assets/Holotna/Mountain/Materials/Leaf01A.mat'
  'Unity/Assets/Holotna/Mountain/Materials/Leaf01B.mat'
  'Unity/Assets/Mega Fantasy Props Pack/Materials/bark_2_d.mat'
  'Unity/Assets/Mega Fantasy Props Pack/Materials/cobblestone_wall_1_fixed_d.mat'
  'Unity/Assets/Mega Fantasy Props Pack/Materials/granite_1_d.mat'
  'Unity/Assets/Mega Fantasy Props Pack/Materials/stone_wall_3_d.mat'
  'Unity/Assets/Mega Fantasy Props Pack/Materials/wood_1_d.mat'
  'Unity/Assets/Mega Fantasy Props Pack/Materials/wooden-boards-texture_d.mat'
 )
}

function Repair-PR179KnownMaterialDrift {
 param([string]$Source,[object]$Audit,[string]$Evidence,[string]$ExpectedHead)
 $ErrorActionPreference='Stop'
 if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native tools active: no material restoration'}
 if($ExpectedHead -notmatch '^[a-f0-9]{40}$' -or
    (git -C $Source rev-parse HEAD).Trim() -ne $ExpectedHead -or $LASTEXITCODE){
  throw 'Material restoration source identity differs'
 }
 # Existing dirty production inputs retain their ORIGINAL byte hashes. A
 # material repair can never authorize a new or altered production baseline.
 foreach($item in $Audit.files){
  $file=Join-Path $Source $item.path
  if(-not(Test-Path -LiteralPath $file -PathType Leaf) -or
     (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $item.sha256){
   throw "Reviewed dirty input changed: $($item.path)"
  }
 }
 $current=@(git -C $Source status --porcelain -- Unity)
 if($LASTEXITCODE){throw 'Cannot inspect exact material delta'}
 $delta=@(Compare-Object @($Audit.status) $current)
 if(-not $delta.Count){return 0}
 $known=@(Get-PR179NativeMaterialPaths)
 $paths=@()
 foreach($entry in $delta){
  $line=[string]$entry.InputObject
  if($entry.SideIndicator -ne '=>' -or -not $line.StartsWith(' M ')){
   throw 'Material recovery refuses missing, staged or non-material changes'
  }
  $path=$line.Substring(3).Trim([char]34)
  if($known -cnotcontains $path -or @($Audit.files|Where-Object {$_.path -ceq $path}).Count){
   throw "Material recovery refuses unapproved path: $path"
  }
  $paths+=@($path)
 }
 $paths=@($paths|Sort-Object -Unique)
 # Refuse a changed index as well: restore only paths that were clean in
 # the immutable original audit and still have the frozen tracked source.
 foreach($path in $paths){
  $blob=(& git -C $Source rev-parse "${ExpectedHead}:$path").Trim()
  if($LASTEXITCODE){throw 'Frozen material blob unavailable'}
  $indexed=(& git -C $Source rev-parse ":$path").Trim()
  if($LASTEXITCODE -or $indexed -ne $blob){throw 'Material index differs from frozen source'}
 }
 $backup=Join-Path $Evidence 'known-material-before-restore'
 if(Test-Path -LiteralPath $backup){throw 'Refuse overwriting material recovery evidence'}
 New-Item -ItemType Directory -Path $backup -Force|Out-Null
 $records=@()
 foreach($path in $paths){
  $file=Join-Path $Source $path
  $saved=Join-Path $backup $path
  New-Item -ItemType Directory -Path (Split-Path $saved) -Force|Out-Null
  Copy-Item -LiteralPath $file -Destination $saved
  $records+=@{path=$path;before_sha256=(Get-FileHash -LiteralPath $saved -Algorithm SHA256).Hash}
 }
 & git -C $Source diff --binary -- @paths | Set-Content -LiteralPath (Join-Path $Evidence 'known-material-original-drift.diff') -Encoding UTF8
 & git -C $Source restore --source=$ExpectedHead --worktree -- @paths
 if($LASTEXITCODE){throw 'Targeted material restoration failed; evidence retained'}
 foreach($record in $records){
  $path=[string]$record.path
  $blob=(& git -C $Source rev-parse "${ExpectedHead}:$path").Trim()
  $restored=(& git -C $Source hash-object "--path=$path" $path).Trim()
  if($LASTEXITCODE -or $restored -ne $blob){throw "Restored material differs from frozen Git blob: $path"}
  $record.after_sha256=(Get-FileHash -LiteralPath (Join-Path $Source $path) -Algorithm SHA256).Hash
 }
 $after=@(git -C $Source status --porcelain -- Unity)
 if($LASTEXITCODE -or @(Compare-Object @($Audit.status) $after).Count){throw 'Restoration did not recover exact original status'}
 foreach($item in $Audit.files){
  if((Get-FileHash -LiteralPath (Join-Path $Source $item.path) -Algorithm SHA256).Hash -ne $item.sha256){
   throw 'Initially dirty production bytes changed during material restoration'
  }
 }
 @{expected_head=$ExpectedHead;restored_count=$paths.Count;original_dirty_hashes_preserved=$true;files=$records}|
  ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $Evidence 'known-material-restoration.json') -Encoding UTF8
 Write-Host "PR179_KNOWN_MATERIALS_RESTORED count=$($paths.Count); FROZEN_TRACKED_BLOBS_AND_ORIGINAL_DIRTY_BYTES_VERIFIED"
 return $paths.Count
}
