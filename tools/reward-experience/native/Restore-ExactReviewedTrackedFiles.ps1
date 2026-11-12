param([Parameter(Mandatory=$true)][string]$Source,[Parameter(Mandatory=$true)][string]$Manifest,[Parameter(Mandatory=$true)][string]$Output,[string]$ExpectedCurrentHead='')
$ErrorActionPreference='Stop'
function Hash([string]$p){(Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.ToLowerInvariant()}
$sourceRoot=[IO.Path]::GetFullPath($Source).TrimEnd([IO.Path]::DirectorySeparatorChar)
$outputRoot=[IO.Path]::GetFullPath($Output)
if($outputRoot.StartsWith($sourceRoot+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Archive must be outside reviewed Git checkout'}
$review=Get-Content -LiteralPath $Manifest -Raw|ConvertFrom-Json
if($review.role -ne 'EXACT_ARCHIVED_ISOLATED_R19_TRACKED_RESTORE'){throw 'Wrong exact restoration manifest role'}
$head=(& git -C $sourceRoot rev-parse HEAD).Trim()
if(-not $ExpectedCurrentHead){$ExpectedCurrentHead=[string]$review.source}
if($LASTEXITCODE -or $head -ne $ExpectedCurrentHead){throw 'Reviewed current Git HEAD changed'}
$dirty=@(& git -C $sourceRoot diff HEAD --name-only)
if($LASTEXITCODE){throw 'Cannot inspect actual tracked changes'}
$map=@{}
foreach($entry in @($review.files)){
 $rel=[string]$entry.path
 if(($rel -ne 'Unity/ProjectSettings/ProjectSettings.asset') -and (-not $rel.StartsWith('Unity/Assets/') -or -not $rel.EndsWith('.meta'))){throw 'Unreviewable tracked source path'}
 if($rel.Contains('..') -or $rel.Contains(':') -or $rel.Contains('\') -or $map.ContainsKey($rel)){throw 'Unsafe/duplicate exact tracked path'}
 if($entry.currentSha256 -notmatch '^[a-f0-9]{64}$' -or $entry.headGitBlob -notmatch '^[a-f0-9]{40}$'){throw 'Invalid exact tracked hashes'}
 $map[$rel]=$entry
}
if($dirty.Count -lt 1 -or $dirty.Count -gt $map.Count -or $map.Count -gt 300){throw 'Actual tracked changes do not match exact reviewed set'}
foreach($rel in $dirty){
 if(-not $map.ContainsKey($rel)){throw 'New unfamiliar tracked path; preserve everything'}
 $entry=$map[$rel];$p=Join-Path $sourceRoot $rel
 if(-not(Test-Path -LiteralPath $p -PathType Leaf)){throw 'Reviewed tracked file missing'}
 if((Get-Item -LiteralPath $p -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Redirected reviewed file'}
 if((Hash $p) -ne $entry.currentSha256){throw 'Reviewed tracked bytes changed; preserve everything'}
 $blob=(& git -C $sourceRoot rev-parse ('HEAD:'+$rel)).Trim()
 if($LASTEXITCODE -or $blob -ne $entry.headGitBlob){throw 'Reviewed original Git blob changed'}
}
# Whole set validated BEFORE writing source. Archive every changed byte first.
if(Test-Path -LiteralPath $outputRoot){throw 'Never replace an earlier reviewed source archive'}
[IO.Directory]::CreateDirectory($outputRoot)|Out-Null
foreach($rel in $dirty){
 $p=Join-Path $sourceRoot $rel;$copy=Join-Path (Join-Path $outputRoot 'current') $rel
 [IO.Directory]::CreateDirectory((Split-Path $copy))|Out-Null;[IO.File]::Copy($p,$copy,$false)
 if((Hash $copy) -ne $map[$rel].currentSha256 -or (Hash $p) -ne $map[$rel].currentSha256){throw 'Reviewed changed bytes not preserved exactly'}
}
$paths=Join-Path $outputRoot 'literal-paths.txt'
[IO.File]::WriteAllLines($paths,@($dirty|ForEach-Object {':(literal)'+$_}),(New-Object Text.UTF8Encoding($false)))
& git -C $sourceRoot restore --source=HEAD --worktree ('--pathspec-from-file='+$paths)
if($LASTEXITCODE){throw 'Exact reviewed Git restoration failed; original changed bytes remain archived'}
foreach($rel in $dirty){
 $blob=(& git -C $sourceRoot hash-object ('--path='+$rel) (Join-Path $sourceRoot $rel)).Trim()
 if($LASTEXITCODE -or $blob -ne $map[$rel].headGitBlob){throw 'Original restored Git blob does not match reviewed source'}
}
& git -C $sourceRoot diff HEAD --quiet
if($LASTEXITCODE){throw 'Unexpected remaining tracked source; no checkout'}
[IO.File]::WriteAllText((Join-Path $outputRoot 'report.json'),
 (@{status='EXACT_REVIEWED_TRACKED_SOURCE_RESTORED';files=$dirty.Count;originalHead=$head;allOriginalGitBlobsVerified=$true;changedBytesArchived=$true;mainModified=$false}|ConvertTo-Json),
 (New-Object Text.UTF8Encoding($false)))
Write-Host "ISSUE186_EXACT_REVIEWED_TRACKED_ORIGINALS_RESTORED files=$($dirty.Count); NO_GIT_CLEAN_NO_UNKNOWN_OVERWRITE"
