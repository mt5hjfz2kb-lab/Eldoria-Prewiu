param([string]$Script=(Join-Path $PSScriptRoot 'Preserve-OwnedImporterState.ps1'))
$ErrorActionPreference='Stop';$root=Join-Path ([IO.Path]::GetTempPath()) ('daily-source-preserve-'+[guid]::NewGuid().ToString('N'));$repo=Join-Path $root 'daily-source'
function Put([string]$p,[string]$v){[IO.Directory]::CreateDirectory((Split-Path $p))|Out-Null;[IO.File]::WriteAllText($p,$v,(New-Object Text.UTF8Encoding($false)))}
function Need([bool]$ok,[string]$why){if(-not $ok){throw $why};$script:checks++}
$checks=0
try{
 [IO.Directory]::CreateDirectory($repo)|Out-Null;& git -C $repo init --quiet
 & git -C $repo config user.name 'Daily isolated QA';& git -C $repo config user.email 'qa@eldoria.invalid'
 $meta='Unity/Assets/source.png.meta';$settings='Unity/ProjectSettings/ProjectSettings.asset';$code='Unity/Assets/Original.cs'
 Put (Join-Path $repo '.gitignore') 'Unity/Library/'
 Put (Join-Path $repo $meta) 'ORIGINAL GUID';Put (Join-Path $repo $settings) 'ORIGINAL ICONS';Put (Join-Path $repo $code) 'ORIGINAL SOURCE'
 & git -C $repo add --all;& git -C $repo commit --quiet -m 'Actual original Git fixture';if($LASTEXITCODE){throw 'Fixture commit failed'}
 Put (Join-Path $repo $meta) 'IMPORTER DIFFERENCE PRESERVED';Put (Join-Path $repo $settings) 'EMPTY IMPORTER ICON SLOTS'
 Put (Join-Path $repo 'Unity/Library/preserved.txt') 'RETAIN WARM CACHE';Put (Join-Path $repo 'untracked-note.txt') 'RETAIN UNTRACKED NOTE'
 $out=Join-Path $root 'preserved'; & $Script -Source $repo -Output $out -SnapshotId 'fixture'
 $p=Get-Content (Join-Path $out 'report.json') -Raw|ConvertFrom-Json
 Need ($p.originalGitBlobsVerified -and $p.files.Count -eq 2) 'Actual original Git inputs not verified'
 Need (([IO.File]::ReadAllText((Join-Path $repo $meta))) -eq 'ORIGINAL GUID') 'Private canonical metadata not restored'
 Need (((& git -C $repo show ($p.preservationRef+':'+$meta)) -join "`n") -eq 'IMPORTER DIFFERENCE PRESERVED') 'Preserved Git ref did not retain original working source'
 Need (([IO.File]::ReadAllText((Join-Path (Join-Path $out 'current') $settings))) -eq 'EMPTY IMPORTER ICON SLOTS') 'Exact current settings not archived'
 Need ((Test-Path (Join-Path $repo 'Unity/Library/preserved.txt')) -and (Test-Path (Join-Path $repo 'untracked-note.txt'))) 'Cache or untracked source was removed'
 Put (Join-Path $repo $code) 'UNFAMILIAR CODE MUST REMAIN';$refused=$false
 try{& $Script -Source $repo -Output (Join-Path $root 'refused') -SnapshotId 'refused'}catch{$refused=$true}
 Need ($refused -and (([IO.File]::ReadAllText((Join-Path $repo $code))) -eq 'UNFAMILIAR CODE MUST REMAIN')) 'Unknown source code was reset or not refused'
 Write-Host "DAILY_REAL_GIT_SOURCE_PRESERVATION_PASS checks=$checks; NO UNITY OR ART CLAIM"
}finally{if(Test-Path -LiteralPath $root){Remove-Item -LiteralPath $root -Recurse -Force}}
