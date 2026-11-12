param([string]$Script=(Join-Path $PSScriptRoot 'Restore-ExactReviewedTrackedFiles.ps1'))
$ErrorActionPreference='Stop';$root=Join-Path ([IO.Path]::GetTempPath()) ('eldoria-git-recovery-'+[guid]::NewGuid().ToString('N'))
$repo=Join-Path $root 'repo';$manifest=Join-Path $root 'review.json';$out=Join-Path $root 'archive';$checks=0
function Need([bool]$ok,[string]$why){$script:checks++;if(-not $ok){throw $why}}
function Put([string]$p,[string]$text){[IO.Directory]::CreateDirectory((Split-Path $p))|Out-Null;[IO.File]::WriteAllText($p,$text,(New-Object Text.UTF8Encoding($false)))}
function Hash([string]$p){(Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.ToLowerInvariant()}
try{
 [IO.Directory]::CreateDirectory($repo)|Out-Null
 & git -C $repo init --quiet
 & git -C $repo config user.name 'Isolated Eldoria QA'
 & git -C $repo config user.email 'qa@eldoria.invalid'
 $meta='Unity/Assets/fixture.png.meta';$settings='Unity/ProjectSettings/ProjectSettings.asset'
 Put (Join-Path $repo $meta) 'ORIGINAL GUID AND SETTINGS'
 Put (Join-Path $repo $settings) 'ORIGINAL PLAYER SETTINGS'
 & git -C $repo add --all
 & git -C $repo commit --quiet -m 'Isolated original fixture'
 if($LASTEXITCODE){throw 'Cannot create genuine isolated Git fixture'}
 $head=(& git -C $repo rev-parse HEAD).Trim();$files=@()
 foreach($rel in @($meta,$settings)){
  Put (Join-Path $repo $rel) ('IMPORTED REVIEWED '+$rel)
  $files+=@{path=$rel;currentSha256=(Hash (Join-Path $repo $rel));headGitBlob=((& git -C $repo rev-parse ('HEAD:'+$rel)).Trim())}
 }
 Put (Join-Path $repo 'Unity/Library/preserved.txt') 'DO NOT CLEAN THE IMPORT CACHE'
 [IO.File]::WriteAllText($manifest,(@{source=$head;role='EXACT_ARCHIVED_ISOLATED_R19_TRACKED_RESTORE';files=$files}|ConvertTo-Json -Depth 8))
 Put (Join-Path $repo $meta) 'IMPORTED UNKNOWN ONE BYTE CHANGE'
 $beforeMeta=Hash (Join-Path $repo $meta);$beforeSettings=Hash (Join-Path $repo $settings);$rejected=$false
 try{& $Script -Source $repo -Manifest $manifest -Output $out}catch{$rejected=$true}
 Need $rejected 'Unknown changed metadata was not refused'
 Need (((Hash (Join-Path $repo $meta)) -eq $beforeMeta) -and ((Hash (Join-Path $repo $settings)) -eq $beforeSettings)) 'Rejected recovery partially overwrote tracked source'
 Put (Join-Path $repo $meta) ('IMPORTED REVIEWED '+$meta)
 & $Script -Source $repo -Manifest $manifest -Output $out
 Need (([IO.File]::ReadAllText((Join-Path $repo $meta))) -eq 'ORIGINAL GUID AND SETTINGS') 'Original metadata not recovered exactly from actual Git'
 Need (([IO.File]::ReadAllText((Join-Path $repo $settings))) -eq 'ORIGINAL PLAYER SETTINGS') 'Original PlayerSettings not recovered exactly'
 Need ((([IO.File]::ReadAllText((Join-Path $repo 'Unity/Library/preserved.txt'))) -eq 'DO NOT CLEAN THE IMPORT CACHE') -and (Test-Path -LiteralPath (Join-Path (Join-Path $out 'current') $meta))) 'Cache or archived unfamiliar bytes lost'
 Write-Host "ISSUE186_REAL_GIT_TRACKED_RECOVERY_PASS checks=$checks; NOT UNITY OR ART"
}finally{if(Test-Path -LiteralPath $root){Remove-Item -LiteralPath $root -Recurse}}
