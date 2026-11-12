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
 [IO.File]::WriteAllText((Join-Path (Get-Location).Path 'issue186-overlay-source-guards.json'),
  (@{checks=$checks;status='PASS';actual_filesystem=$true;unity_verified=$false}|ConvertTo-Json))
 Write-Host "ISSUE186_REAL_FILESYSTEM_OVERLAY_GUARDS_PASS checks=$checks; NOT UNITY OR ART"
}
finally{if(Test-Path -LiteralPath $root){Remove-Item -LiteralPath $root -Recurse}}
