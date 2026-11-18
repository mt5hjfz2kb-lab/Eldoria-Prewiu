param([Parameter(Mandatory=$true)][long]$RunId)
$ErrorActionPreference='Stop'
$owned=Join-Path $env:RUNNER_TEMP ('valoria-spatial-'+$RunId)
$expected=Join-Path $env:RUNNER_TEMP ('valoria-spatial-'+$env:GITHUB_RUN_ID)
if([IO.Path]::GetFullPath($owned) -ne [IO.Path]::GetFullPath($expected)){throw 'Do not delete an unowned run'}
if(-not(Test-Path -LiteralPath $owned)){exit 0}
if(@(Get-Process Unity,UnityShaderCompiler,UnityPackageManager,blender -ErrorAction SilentlyContinue).Count){
  throw 'Unity/Blender still running: preserve scratch'
}
# Windows long paths can make Remove-Item -Recurse fail.
# Only mirror an empty uniquely named directory to the EXACT owned run.
$empty=Join-Path $env:RUNNER_TEMP ('valoria-spatial-empty-'+$RunId)
New-Item -ItemType Directory -Path $empty -Force | Out-Null
try {
  & robocopy.exe $empty $owned /MIR /R:1 /W:1 /NFL /NDL /NP /XJ | Out-Null
  if($LASTEXITCODE -gt 7){throw 'Robocopy mirror failed'}
} finally {
  Remove-Item -LiteralPath $empty -Recurse -Force -ErrorAction SilentlyContinue
}
if(Test-Path -LiteralPath $owned){
  Remove-Item -LiteralPath $owned -Force -ErrorAction SilentlyContinue
}
Write-Host 'SPATIAL_SCRATCH_CLEANED_OWNED_ONLY'
