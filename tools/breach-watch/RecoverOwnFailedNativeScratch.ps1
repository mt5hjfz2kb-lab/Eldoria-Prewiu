[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)]
  [ValidateSet('Snapshot','Cleanup')]
  [string]$Phase,
  [string]$ArtifactId='',
  [string]$ExpectedSnapshotSha256=''
)
$ErrorActionPreference='Stop'
$api='https://api.github.com/repos/mt5hjfz2kb-lab/Eldoria-Prewiu'
$headers=@{
  Accept='application/vnd.github+json'
  Authorization="Bearer $env:GH_TOKEN"
  'User-Agent'='Eldoria-Vigilia-OwnScratch-Preservation'
}
$workstream='eldoria-forja-oscura-visual-proof-20261107'
$runId='43590398106'
$expected='182676e6ce01e910677d1b95f9df3a9e3b57479b'
$name='breachwatch-isolated-43590398106-1'
$workspace=[IO.Path]::GetFullPath($env:GITHUB_WORKSPACE).TrimEnd([char]'\')
$source=Join-Path $workspace $name
$snapshot=Join-Path $env:RUNNER_TEMP ('vigilia-failed-diff-'+$env:GITHUB_RUN_ID+'.zip')
if(-not $env:GH_TOKEN -or -not $env:GITHUB_RUN_ID){throw 'No authorized GitHub Actions environment'}
$raw=Invoke-RestMethod -Uri ($api+'/contents/pipeline/active-workstreams.json?ref=main') -Headers $headers
$encoded=[string]$raw.content
if($raw.encoding -ne 'base64' -or [string]::IsNullOrWhiteSpace($encoded)){
  $blob=Invoke-RestMethod -Uri ($api+'/git/blobs/'+$raw.sha) -Headers $headers
  if($blob.encoding -ne 'base64' -or $blob.sha -ne $raw.sha){throw 'Registry immutable blob mismatch'}
  $encoded=[string]$blob.content
}
$state=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String(($encoded -replace '\s',''))) | ConvertFrom-Json
$holders=@($state.active | Where-Object {$_.ownership_state -eq 'claimed' -and @($_.resources) -contains 'windows-runner-heavy'})
if($holders.Count -ne 1 -or $holders[0].id -ne $workstream -or
   $holders[0].owner -ne $env:LEASE_OWNER -or
   [string]$holders[0].ownership_revision -ne $env:LEASE_REVISION){
  throw 'Windows resource lease moved: leave scratch untouched'
}
$prior=Invoke-RestMethod -Uri ($api+'/actions/runs/'+$runId) -Headers $headers
if($prior.id.ToString() -ne $runId -or $prior.head_sha -ne $expected -or
   $prior.conclusion -ne 'failure' -or $prior.status -ne 'completed'){
  throw 'Prior failed Unity run identity unverified'
}
$originalArtifact=Invoke-RestMethod -Uri ($api+'/actions/runs/'+$runId+'/artifacts') -Headers $headers
$original=@($originalArtifact.artifacts | Where-Object {
  $_.name -eq ('breachwatch-first-native-'+$expected+'-'+$runId) -and -not $_.expired
})
if($original.Count -ne 1){throw 'Old native failure diagnostic artifact must remain available'}
if(@(Get-Process Unity,blender,git -ErrorAction SilentlyContinue).Count){
  throw 'Active Unity/Blender/Git process: do not touch own failed source'
}
$path=Get-Item -LiteralPath $source -ErrorAction Stop
if($path.Attributes -band [IO.FileAttributes]::ReparsePoint -or
   $path.Name -ne $name -or
   -not $path.FullName.StartsWith($workspace+'\breachwatch-isolated-', [StringComparison]::OrdinalIgnoreCase)){
  throw 'Refuse linked or non-owned source directory'
}
$git=(Get-Command git.exe -ErrorAction SilentlyContinue).Source
if(-not $git){
  $git=Get-ChildItem "$env:LOCALAPPDATA/GitHubDesktop" -Directory -Filter 'app-*' -ErrorAction SilentlyContinue |
    Sort-Object Name -Descending |
    ForEach-Object {Join-Path $_.FullName 'resources/app/git/cmd/git.exe'} |
    Where-Object {Test-Path -LiteralPath $_} | Select-Object -First 1
}
if(-not $git){throw 'Git unavailable; source cannot be independently verified'}
$sha=(& $git -C $source rev-parse HEAD).Trim()
if($LASTEXITCODE -ne 0 -or $sha -ne $expected){throw 'Failed worktree changed HEAD'}
$url=(& $git -C $source remote get-url origin).Trim()
if($LASTEXITCODE -ne 0 -or $url -ne 'https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu.git'){
  throw 'Failed worktree Git origin untrusted'
}

if($Phase -eq 'Snapshot'){
  if(Test-Path -LiteralPath $snapshot){throw 'Snapshot path already exists'}
  $psi=New-Object Diagnostics.ProcessStartInfo
  $psi.FileName=$git
  $psi.Arguments='-C "'+$source+'" -c core.quotePath=false status --porcelain=v1 --untracked-files=all -z'
  $psi.UseShellExecute=$false
  $psi.CreateNoWindow=$true
  $psi.RedirectStandardOutput=$true
  $psi.RedirectStandardError=$true
  $psi.StandardOutputEncoding=New-Object Text.UTF8Encoding($false)
  $proc=[Diagnostics.Process]::Start($psi)
  $rawStatus=$proc.StandardOutput.ReadToEnd()
  $stderr=$proc.StandardError.ReadToEnd()
  $proc.WaitForExit()
  if($proc.ExitCode -ne 0){throw "Git status could not be verified: $stderr"}
  $records=@($rawStatus.Split([char]0) | Where-Object { $_.Length -gt 0 })
  Add-Type -AssemblyName System.IO.Compression
  Add-Type -AssemblyName System.IO.Compression.FileSystem
  $zip=[IO.Compression.ZipFile]::Open($snapshot,[IO.Compression.ZipArchiveMode]::Create)
  $files=New-Object System.Collections.Generic.List[object]
  $total=[long]0
  try {
    foreach($record in $records){
      if($record.Length -lt 4){throw 'Malformed modified/untracked Git record'}
      $status=$record.Substring(0,2)
      if($status -ne ' M' -and $status -ne '??'){
        throw "Unknown staged, renamed or deleted record: $record"
      }
      $rel=$record.Substring(3).Replace('\','/')
      if(-not $rel.StartsWith('Unity/',[StringComparison]::Ordinal) -or
         $rel.StartsWith('Unity/Library/',[StringComparison]::Ordinal) -or
         $rel.Contains('../') -or $rel.StartsWith('/')){
        throw "Unexpected changed path: $rel"
      }
      $actual=[IO.Path]::GetFullPath((Join-Path $source ($rel.Replace('/','\'))))
      if(-not $actual.StartsWith($path.FullName+'\', [StringComparison]::OrdinalIgnoreCase) -or
         -not(Test-Path -LiteralPath $actual)){
        throw "Modified file missing/outside dedicated checkout: $rel"
      }
      $item=Get-Item -LiteralPath $actual -ErrorAction Stop
      if($item.PSIsContainer -or $item.Attributes -band [IO.FileAttributes]::ReparsePoint){
        throw "Unsafe changed file or link: $rel"
      }
      $walk=Split-Path -Parent $actual
      while($walk.Length -gt $path.FullName.Length){
        $ancestor=Get-Item -LiteralPath $walk -ErrorAction Stop
        if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){
          throw "Symlinked ancestor in changed source: $rel"
        }
        $walk=Split-Path -Parent $walk
      }
      $total+=$item.Length
      if($total -gt 512MB){throw 'Too many unknown modified source bytes for bounded backup'}
      $hash=(Get-FileHash -LiteralPath $actual -Algorithm SHA256).Hash.ToLowerInvariant()
      [IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
        $zip,$actual,('source-files/'+$rel),[IO.Compression.CompressionLevel]::Optimal) | Out-Null
      $files.Add(@{path=$rel;gitStatus=$status;size=$item.Length;sha256=$hash})
    }
    $manifest=@{
      workstream=$workstream
      sourceSha=$expected
      previousFailedRun=[int]$runId
      recordCount=$records.Count
      totalBytes=$total
      allChangesStoredExactly=$true
      immutableOriginalCommitOnGitHub=$true
      oldNativeErrorArtifactId=$original[0].id
      files=@($files.ToArray())
    } | ConvertTo-Json -Depth 8
    $entry=$zip.CreateEntry('manifest.json',[IO.Compression.CompressionLevel]::Optimal)
    $writer=New-Object IO.StreamWriter($entry.Open(),(New-Object Text.UTF8Encoding($false)))
    try{$writer.Write($manifest)}finally{$writer.Dispose()}
  } finally {
    $zip.Dispose()
  }
  $size=(Get-Item -LiteralPath $snapshot).Length
  if($size -lt 100 -or $size -gt 600MB){throw 'Native snapshot invalid archive size'}
  $snapsha=(Get-FileHash -LiteralPath $snapshot -Algorithm SHA256).Hash.ToLowerInvariant()
  Add-Content $env:GITHUB_OUTPUT ('snapshot_sha='+$snapsha)
  Write-Host ('VIGILIA_SNAPSHOT_OWN_CRASHED_SOURCE_READY files='+$records.Count+
    ' changedBytes='+$total+' archiveBytes='+$size+' SHA256='+$snapsha)
  exit 0
}

if($ArtifactId -notmatch '^[1-9][0-9]+$' -or
   $ExpectedSnapshotSha256 -notmatch '^[a-f0-9]{64}$'){
  throw 'Snapshot artifact identity or SHA unverified; preserve all files'
}
$artifact=Invoke-RestMethod -Uri ($api+'/actions/artifacts/'+$ArtifactId) -Headers $headers
if($artifact.id.ToString() -ne $ArtifactId -or $artifact.expired -or
   $artifact.name -ne ('vigilia-own-failed-source-'+$env:GITHUB_RUN_ID)){
  throw 'Immutable uploaded source-difference backup missing'
}
if((Get-FileHash -LiteralPath $snapshot -Algorithm SHA256).Hash.ToLowerInvariant() -ne $ExpectedSnapshotSha256){
  throw 'Source-difference snapshot hash changed'
}
if(@(Get-Process Unity,blender,git -ErrorAction SilentlyContinue).Count){
  throw 'Concurrent Unity/Blender/Git: no cleanup'
}
# The full source at the exact Git SHA is remote. Every local difference and
# untracked byte is preserved in the successfully uploaded ZIP. Only this
# unique, verified, failed-run checkout can be reclaimed.
$before=(Get-PSDrive C).Free
$empty=Join-Path $env:RUNNER_TEMP ('vigilia-empty-own-failed-'+$env:GITHUB_RUN_ID)
if(Test-Path -LiteralPath $empty){throw 'Unexpected existing empty mirror folder'}
New-Item -ItemType Directory -Path $empty -ErrorAction Stop | Out-Null
& robocopy.exe $empty $source /MIR /XJ /R:0 /W:0 /NFL /NDL /NJH /NJS /NP | Out-Null
$result=$LASTEXITCODE
if($result -gt 7){throw "Isolated failed directory long path cleanup failed $result"}
$remaining=@(Get-ChildItem -LiteralPath $source -Force -ErrorAction Stop | Select-Object -First 1)
if($remaining.Count){throw 'Unknown remaining files/junctions; refusing final removal'}
Remove-Item -LiteralPath $source -Force -ErrorAction Stop
Remove-Item -LiteralPath $empty -Force -ErrorAction Stop
$after=(Get-PSDrive C).Free
Write-Host ('VIGILIA_OWN_FAILED_SOURCE_PRESERVED_IN_ACTIONS_ARTIFACT '+$ArtifactId)
Write-Host ('VIGILIA_OWN_FAILED_SOURCE_RECLAIMED_GIB '+[math]::Round(($after-$before)/1GB,2))
Write-Host ('VIGILIA_PC_SAFE_FREE_GIB '+[math]::Round($after/1GB,2))
if($after -lt 16GB){throw 'No room for new native Unity even after safe own-file backup cleanup'}
