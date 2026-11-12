param([string]$Script=(Join-Path $PSScriptRoot 'Set-VigiliaProofOverlay.ps1'))
$ErrorActionPreference='Stop'
$root=Join-Path ([IO.Path]::GetTempPath()) ('eldoria-overlay-guard-'+[Guid]::NewGuid().ToString('N'))
$project=Join-Path $root 'project'
$ui=Join-Path $root 'ui'
$manifest=Join-Path $root 'proof/overlay.json'
$scriptPath=[IO.Path]::GetFullPath($Script)
$checks=0
function Need([bool]$ok,[string]$why){$script:checks++;if(-not $ok){throw $why}}
function Put([string]$path,[string]$value){
 [IO.Directory]::CreateDirectory((Split-Path $path))|Out-Null
 [IO.File]::WriteAllText($path,$value,(New-Object Text.UTF8Encoding($false)))
}
function Snapshot([string]$dir){
 $m=@{};Get-ChildItem -LiteralPath $dir -File -Recurse|ForEach-Object {
  $m[$_.FullName.Substring($dir.Length+1)]=(Get-FileHash -LiteralPath $_.FullName).Hash
 };return $m
}
function Same($left,$right){
 if($left.Count -ne $right.Count){return $false}
 foreach($k in $left.Keys){if(-not $right.ContainsKey($k)-or $right[$k] -ne $left[$k]){return $false}}
 return $true
}
function Install{
 & $scriptPath -Operation Install -Project $project -UiProject $ui -UiSource ('a'*40) -Manifest $manifest
}
function Restore{
 & $scriptPath -Operation Restore -Project $project -Manifest $manifest
}
try{
 Put (Join-Path $project 'Library/protected.txt') 'DO NOT MODIFY THE IMPORT CACHE'
 Put (Join-Path $project 'Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs') 'original HUD'
 Put (Join-Path $project 'Assets/Eldoria/BreachWatch/Runtime/Retired.cs') 'original extra file'
 Put (Join-Path $project 'Assets/Eldoria/BreachWatch.meta') 'original GUID'
 Put (Join-Path $ui 'Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs') 'pinned HUD'
 Put (Join-Path $ui 'Assets/Eldoria/BreachWatch/Runtime/BreachWatchResourceBagBridge.cs') 'pinned optional bridge'
 Put (Join-Path $ui 'Assets/Eldoria/BreachWatch/Resources/preserved-source.png') 'immutable fake fixture bytes, not a Unity art claim'
 Put (Join-Path $ui 'Assets/Eldoria/BreachWatch.meta') 'pinned GUID'
 $before=Snapshot $project
 Install
 Need (Test-Path $manifest) 'Restoration manifest was not saved'
 Need (Test-Path ($manifest+'.installed')) 'Complete installation marker missing'
 Need (([IO.File]::ReadAllText((Join-Path $project 'Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs'))) -eq 'pinned HUD') 'Exact pinned source was not installed'
 Need (-not(Test-Path (Join-Path $project 'Assets/Eldoria/BreachWatch/Runtime/Retired.cs'))) 'Obsolete local UI class leaked into pinned proof'
 $installed=Snapshot $project
 Restore
 Need (Same $before (Snapshot $project)) 'Original source, extra file, metadata or Library changed'
 Need ((Get-Content ($manifest+'.restored.json') -Raw|ConvertFrom-Json).restored) 'Restoration proof missing'
 # Real conflict must be detected before restoring ANY other path.
 Remove-Item -LiteralPath $manifest
 Remove-Item -LiteralPath ($manifest+'.installed')
 Remove-Item -LiteralPath ($manifest+'.restored.json')
 Remove-Item -LiteralPath ($manifest+'.original-files') -Recurse
 Install
 $file=Join-Path $project 'Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs'
 Put $file 'UNFAMILIAR NEW BYTES'
 $conflicted=Snapshot $project;$refused=$false
 try{Restore}catch{$refused=$true}
 Need $refused 'Unknown edit was not rejected'
 Need (Same $conflicted (Snapshot $project)) 'Conflict rejection partially overwrote source'
 # Preserve the EXACT unfamiliar bytes for review, without relaxing refusal
 # or allowing a cleanup routine to alter the project.
 $drift=Join-Path $root 'drift-evidence'
 & (Join-Path $PSScriptRoot 'Get-VigiliaProofOverlayDrift.ps1') -Project $project -Manifest $manifest -Output $drift
 $audit=Get-Content (Join-Path $drift 'report.json') -Raw|ConvertFrom-Json
 Need ($audit.status -eq 'REVIEW_REQUIRED' -and $audit.changed -eq 1) 'Unknown drift not reported explicitly'
 Need (([IO.File]::ReadAllText((Join-Path $drift 'changed-files/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs'))) -eq 'UNFAMILIAR NEW BYTES') 'Archived unknown source differs'
 Need (-not $audit.restorePerformed -and -not $audit.projectModified) 'Audit falsely claimed restoration'
 Need (Same $conflicted (Snapshot $project)) 'Audit mutated protected isolated project'
 Put $file 'pinned HUD'
 Restore
 Need (Same $before (Snapshot $project)) 'Known source could not be restored after conflict removal'
 # Interrupted installation: some files installed, others remain original.
 Remove-Item -LiteralPath $manifest
 Remove-Item -LiteralPath ($manifest+'.installed')
 Remove-Item -LiteralPath ($manifest+'.restored.json')
 Remove-Item -LiteralPath ($manifest+'.original-files') -Recurse
 Install
 Remove-Item -LiteralPath ($manifest+'.installed')
 Put $file 'original HUD'
 Remove-Item -LiteralPath (Join-Path $project 'Assets/Eldoria/BreachWatch/Runtime/BreachWatchResourceBagBridge.cs')
 Restore
 Need (Same $before (Snapshot $project)) 'Interrupted installation could not roll back exact source'
 # Path redirection/corruption never receives a restored file.
 $record=Get-Content -LiteralPath $manifest -Raw|ConvertFrom-Json
 $record.files[0].path='Assets/Eldoria/../../outside.txt'
 [IO.File]::WriteAllText($manifest,($record|ConvertTo-Json -Depth 8))
 $refused=$false;try{Restore}catch{$refused=$true}
 Need $refused 'Unowned path was accepted'
 Need (Same $before (Snapshot $project)) 'Unowned path rejection modified project'

 # Exercise exact reviewed metadata acceptance, archival and one-byte refusal.
 $p2=Join-Path $root 'reviewed-project';$u2=Join-Path $root 'reviewed-ui'
 $m2=Join-Path $root 'reviewed-proof/overlay.json'
 $rp=Join-Path $root 'exact-reviewed.json';$rel='Assets/Eldoria/BreachWatch/Resources/test.png.meta'
 Put (Join-Path $p2 $rel) 'ORIGINAL METADATA'
 Put (Join-Path $p2 'Assets/Eldoria/BreachWatch/Resources/retained.png.meta') 'ORIGINAL GUID RETAINED'
 Put (Join-Path $u2 'Assets/Eldoria/BreachWatch/Resources/retained.png') 'EXACT IMAGE FIXTURE'
 Put (Join-Path $u2 'Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs') 'pinned HUD'
 Put (Join-Path $u2 'Assets/Eldoria/BreachWatch/Runtime/BreachWatchResourceBagBridge.cs') 'pinned bridge'
 Put (Join-Path $u2 $rel) 'PINNED METADATA'
 $before2=Snapshot $p2
 & $scriptPath -Operation Install -Project $p2 -UiProject $u2 -UiSource ('a'*40) -Manifest $m2
 Need (([IO.File]::ReadAllText((Join-Path $p2 'Assets/Eldoria/BreachWatch/Resources/retained.png.meta'))) -eq 'ORIGINAL GUID RETAINED') 'Omitted corresponding-asset metadata lost its original GUID'
 $e=(Get-Content -LiteralPath $m2 -Raw|ConvertFrom-Json).files|Where-Object {$_.path -eq $rel}
 Put (Join-Path $p2 $rel) 'IMPORTED EXACT REVIEWED METADATA'
 $acceptedHash=(Get-FileHash -LiteralPath (Join-Path $p2 $rel) -Algorithm SHA256).Hash.ToLowerInvariant()
 $review=@{uiSource=('a'*40);role='EXACT_REVIEWED_TEMPORARY_UI_METADATA_RESTORE_ONLY'
   files=@(@{path=$rel;current=$acceptedHash;installed=$e.installed;original=$e.original})}
 [IO.File]::WriteAllText($rp,($review|ConvertTo-Json -Depth 8))
 Put (Join-Path $p2 $rel) 'IMPORTED EXACT REVIEWED METADATA!'
 $unfamiliar=Snapshot $p2;$refused=$false
 try{& $scriptPath -Operation Restore -Project $p2 -Manifest $m2 -ReviewedMetadata $rp}catch{$refused=$true}
 Need $refused 'A one-byte change bypassed the exact reviewed SHA'
 Need (Same $unfamiliar (Snapshot $p2)) 'Rejected reviewed drift partially changed the project'
 Put (Join-Path $p2 $rel) 'IMPORTED EXACT REVIEWED METADATA'
 & $scriptPath -Operation Restore -Project $p2 -Manifest $m2 -ReviewedMetadata $rp
 Need (Same $before2 (Snapshot $p2)) 'Reviewed exact metadata did not restore all original project bytes'
 Need (([IO.File]::ReadAllText((Join-Path ($m2+'.reviewed-importer-files') $rel))) -eq 'IMPORTED EXACT REVIEWED METADATA') 'Reviewed changed bytes were not archived before restoration'

 [IO.File]::WriteAllText((Join-Path (Get-Location).Path 'issue186-overlay-source-guards.json'),
  (@{checks=$checks;status='PASS';actual_filesystem=$true;unity_verified=$false}|ConvertTo-Json))
 Write-Host "ISSUE186_REAL_FILESYSTEM_OVERLAY_GUARDS_PASS checks=$checks; NOT UNITY OR ART"
}
finally{if(Test-Path -LiteralPath $root){Remove-Item -LiteralPath $root -Recurse}}
