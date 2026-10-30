# Archive identity only: reuse never grants artistic approval or changes Unity inputs.
# Compatible with Windows PowerShell 5.1; no modules, installs, symlinks or deletions.
function Save-EldoriaEditableSource {
 param(
  [Parameter(Mandatory=$true)][string]$GeneratedFile,
  [Parameter(Mandatory=$true)][string]$DestinationDirectory
 )
 $ErrorActionPreference='Stop'
 $prefix='docs/evidence/world4x-canon-20261022/sources/'
 $name=[IO.Path]::GetFileName($GeneratedFile)
 if($name -notin @('Eldoria_Region1_Production.blend','Eldoria_Resource_Library.blend','RockBoar_Far_LOD.blend')){throw 'Unexpected editable source name'}
 if($DestinationDirectory -notmatch '^docs/evidence/world4x-canon-20261022/sources/native-[0-9]+$'){throw 'Invalid native source destination'}
 if(-not(Test-Path -LiteralPath $GeneratedFile -PathType Leaf)){throw 'Generated editable source missing'}
 $sha256=(Get-FileHash -LiteralPath $GeneratedFile -Algorithm SHA256).Hash.ToLowerInvariant()
 $bytes=(Get-Item -LiteralPath $GeneratedFile).Length
 $blob=(& git hash-object --no-filters -- $GeneratedFile).Trim()
 if($LASTEXITCODE -or $blob -notmatch '^[0-9a-f]{40}$'){throw 'Cannot identify generated Git blob'}
 # Read tree metadata only, not every historical BLEND body. Reuse only an
 # exact already-tracked blob with the same filename in the existing archive.
 $entries=@(& git -c core.quotepath=false ls-tree -r --full-tree HEAD -- $prefix)
 if($LASTEXITCODE){throw 'Cannot read source archive Git tree'}
 $candidates=@()
 foreach($line in $entries){
  if($line -match '^100644 blob ([0-9a-f]{40})\t(.+)$'){
   $entryBlob=$Matches[1];$entryPath=$Matches[2]
   if($entryBlob -eq $blob -and [IO.Path]::GetFileName($entryPath) -eq $name){$candidates+=$entryPath}
  }
 }
 $path="$DestinationDirectory/$name"
 $reused=$candidates.Count -gt 0
 if($reused){
  $path=@($candidates | Sort-Object)[0]
  if(-not(Test-Path -LiteralPath $path)){
   # Recover just this immutable tracked file if checkout is sparse. Never
   # reset an existing local file, clean the workspace or touch Unity caches.
   & git checkout --ignore-skip-worktree-bits HEAD -- $path
   if($LASTEXITCODE){throw "Cannot restore referenced editable source: $path"}
  }
 }else{
  if(-not(Test-Path -LiteralPath $path)){
   New-Item -ItemType Directory -Force $DestinationDirectory | Out-Null
   [IO.File]::Copy((Get-Item -LiteralPath $GeneratedFile).FullName,(Join-Path (Get-Location).Path $path),$false)
  }
 }
 # A same-SHA1 tree entry alone is insufficient: verify actual source bytes
 # with SHA256 too. Dirty or conflicting destinations fail without overwrite.
 if((Get-Item -LiteralPath $path).Length -ne $bytes -or
    (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $sha256){
  throw "Editable source identity conflict; preserved existing file: $path"
 }
 return [PSCustomObject]@{path=$path;sha256=$sha256;bytes=$bytes;reused=$reused;avoided_checkout_bytes=$(if($reused){$bytes}else{0})}
}
