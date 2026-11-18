param(
  [Parameter(Mandatory=$true)]
  [ValidateRange(1,999999999999)]
  [long]$RunId,
  [switch]$Execute,
  [switch]$EvidenceArchived
)
$ErrorActionPreference='Stop'
# This script is *only* for a temp scratch cloned by valoria-depth-pilot.
# /MIR is destructive; fail closed unless the destination is exactly the
# one named depth temp folder under RUNNER_TEMP, with no reparse/junction.
if(-not $env:RUNNER_TEMP -or -not(Test-Path -LiteralPath $env:RUNNER_TEMP -PathType Container)){
  throw 'RUNNER_TEMP is unavailable; refusing deletion'
}
$root=[IO.Path]::GetFullPath($env:RUNNER_TEMP).TrimEnd('\')
$name='valoria-depth-'+$RunId
$target=Join-Path $root $name
$resolved=[IO.Path]::GetFullPath($target)
$parent=[IO.Directory]::GetParent($resolved).FullName.TrimEnd('\')
if($parent -ne $root -or (Split-Path -Leaf $resolved) -ne $name){
  throw 'Invalid exact scratch target; refusing deletion'
}
if(-not(Test-Path -LiteralPath $resolved -PathType Container)){
  Write-Host "No own depth scratch for run $RunId"
  exit 0
}
$item=Get-Item -LiteralPath $resolved -Force
if(($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){
  throw 'Own scratch target is a reparse/junction: refusing deletion'
}
if(@(Get-Process Unity,UnityShaderCompiler,UnityPackageManager,blender -ErrorAction SilentlyContinue).Count){
  throw 'Native PC processes are active; never interrupt or delete live scratch'
}
$evidence=Join-Path $resolved 'Unity/Ciudad2DDepthPilotEvidence/evidence.json'
if((Test-Path -LiteralPath $evidence) -and -not $EvidenceArchived){
  throw 'Genuine Unity frames exist but are not explicitly marked archived; preserve source'
}
if(-not $Execute){
  Write-Host "DRY_RUN_ONLY: target=$resolved (not deleted)"
  exit 0
}
$empty=Join-Path $root ('valoria-depth-empty-'+$RunId)
if(Test-Path -LiteralPath $empty){
  if(@(Get-ChildItem -LiteralPath $empty -Force -ErrorAction Stop).Count){
    throw 'Expected empty mirror source has content; refuse /MIR'
  }
}else{
  New-Item -ItemType Directory -Path $empty -ErrorAction Stop | Out-Null
}
$log=Join-Path $root ('valoria-depth-cleanup-'+$RunId+'.log')
Write-Host "PURGE_ONLY_EXACT_RUN_SCRATCH: $resolved"
# Robocopy /MIR supports long paths by default; /XJ excludes junctions.
# Source is empty and verified. >=8 is an official robocopy failure.
& robocopy.exe $empty $resolved /MIR /XJ /R:1 /W:1 /NFL /NDL /NJH /NJS /NP ("/LOG:"+$log)
$exitCode=$LASTEXITCODE
if($exitCode -ge 8){
  throw "Robocopy safe purge failed exit=$exitCode log=$log; do not claim disk freed"
}
Remove-Item -LiteralPath $resolved -Recurse -Force -ErrorAction Stop
if(Test-Path -LiteralPath $resolved){
  throw 'Scratch still exists after safe purge; do not claim cleanup'
}
Remove-Item -LiteralPath $empty -Recurse -Force -ErrorAction Stop
Write-Host "SAFE_DEPTH_SCRATCH_CLEANUP_PASS run=$RunId"
