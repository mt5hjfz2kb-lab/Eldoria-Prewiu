param([Parameter(Mandatory=$true)][string]$Source,[Parameter(Mandatory=$true)][string]$Output,[string]$SnapshotId='')
$ErrorActionPreference='Stop'
function Hash([string]$p){(Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.ToLowerInvariant()}
$root=[IO.Path]::GetFullPath($Source).TrimEnd([IO.Path]::DirectorySeparatorChar)
$out=[IO.Path]::GetFullPath($Output)
if($out.StartsWith($root+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase) -or (Test-Path -LiteralPath $out)){throw 'Preservation archive must be new and outside the isolated checkout'}
$head=(& git -C $root rev-parse --verify HEAD).Trim();if($LASTEXITCODE){throw 'Cannot identify owned input source'}
$dirty=@(& git -C $root diff HEAD --name-only);if($LASTEXITCODE){throw 'Cannot inspect owned input source'}
if(-not $dirty.Count){Write-Host 'DAILY_NATIVE_SOURCE_ALREADY_CLEAN';exit 0}
[IO.Directory]::CreateDirectory($out)|Out-Null;$rows=@();$unowned=@()
foreach($rel in $dirty){
 if($rel.Contains('..') -or $rel.Contains(':') -or [IO.Path]::IsPathRooted($rel)){throw 'Unsafe native source path'}
 $p=Join-Path $root $rel
 if(-not(Test-Path -LiteralPath $p -PathType Leaf) -or ((Get-Item -LiteralPath $p -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Missing or redirected native source; preserve checkout'}
 $copy=Join-Path (Join-Path $out 'current') $rel;[IO.Directory]::CreateDirectory((Split-Path $copy))|Out-Null;[IO.File]::Copy($p,$copy,$false)
 $raw=Hash $p;if((Hash $copy) -ne $raw){throw 'Original input source was not archived byte-identically'}
 $original=(& git -C $root rev-parse ('HEAD:'+$rel)).Trim();if($LASTEXITCODE){throw 'Original Git blob unavailable'}
 $working=(& git -C $root hash-object ('--path='+$rel) $p).Trim();if($LASTEXITCODE){throw 'Working Git blob unavailable'}
 $rows+=@{path=$rel;rawSha256=$raw;originalGitBlob=$original;workingGitBlob=$working}
 if($rel -ne 'Unity/ProjectSettings/ProjectSettings.asset' -and (-not $rel.StartsWith('Unity/Assets/') -or -not $rel.EndsWith('.meta'))){$unowned+=$rel}
}
$record=@{status='CURRENT_BYTES_ARCHIVED_NOT_RESET';originalHead=$head;files=$rows;canonicalMainModified=$false;libraryCleaned=$false}
$report=Join-Path $out 'report.json';[IO.File]::WriteAllText($report,($record|ConvertTo-Json -Depth 8),(New-Object Text.UTF8Encoding($false)))
if($unowned.Count){throw ('Non-importer source change preserved; refuse source switch: '+($unowned -join ','))}
if(-not $SnapshotId){$SnapshotId=[guid]::NewGuid().ToString('N')}
if($SnapshotId -notmatch '^[a-zA-Z0-9-]+$'){throw 'Unsafe preservation identifier'}
$saved=(& git -C $root -c 'user.name=Eldoria Native Source Backup' -c 'user.email=qa@eldoria.invalid' stash create).Trim()
if($LASTEXITCODE -or $saved -notmatch '^[a-f0-9]{40}$'){throw 'Cannot preserve full original Git working state'}
$ref='refs/eldoria-native-preserved/'+$SnapshotId
& git -C $root update-ref $ref $saved;if($LASTEXITCODE){throw 'Cannot retain immutable original working-state ref'}
foreach($row in $rows){
 $blob=(& git -C $root rev-parse ($saved+':'+$row.path)).Trim()
 if($LASTEXITCODE -or $blob -ne $row.workingGitBlob -or (Hash (Join-Path $root $row.path)) -ne $row.rawSha256){throw 'Saved Git state or live bytes differ from the preserved source'}
}
$record.preservationRef=$ref;$record.savedWorkingCommit=$saved
[IO.File]::WriteAllText($report,($record|ConvertTo-Json -Depth 8),(New-Object Text.UTF8Encoding($false)))
$paths=Join-Path $out 'literal-paths.txt'
[IO.File]::WriteAllLines($paths,@($rows|ForEach-Object {':(literal)'+$_.path}),(New-Object Text.UTF8Encoding($false)))
& git -C $root restore --source=HEAD --worktree ('--pathspec-from-file='+$paths)
if($LASTEXITCODE){throw 'Input switch failed; all prior bytes remain preserved'}
foreach($row in $rows){
 $blob=(& git -C $root hash-object ('--path='+$row.path) (Join-Path $root $row.path)).Trim()
 if($LASTEXITCODE -or $blob -ne $row.originalGitBlob){throw 'Canonical native input Git blob not restored'}
}
& git -C $root diff HEAD --quiet;if($LASTEXITCODE){throw 'Unexpected remaining tracked source; no checkout'}
$record.status='PRESERVED_AND_RESET_ONLY_PRIVATE_IMPORTER_INPUTS';$record.originalGitBlobsVerified=$true
[IO.File]::WriteAllText($report,($record|ConvertTo-Json -Depth 8),(New-Object Text.UTF8Encoding($false)))
Write-Host "DAILY_NATIVE_IMPORTER_INPUTS_PRESERVED files=$($rows.Count) ref=$ref; NO_GIT_CLEAN_NO_CACHE_REMOVAL"
