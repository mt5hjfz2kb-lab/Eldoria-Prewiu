$ErrorActionPreference='Stop'
$project=Join-Path $env:GITHUB_WORKSPACE 'world4x-source/Unity'
$log=Join-Path $env:RUNNER_TEMP 'eldoria-region1-canon-unity-source.log'
$env:ELDORIA_REGION1_SOURCE_EVIDENCE=Join-Path $env:RUNNER_TEMP 'eldoria-region1-source-gate.json'
$arguments=@('-batchmode','-quit','-projectPath',('"'+$project+'"'),'-executeMethod','Eldoria.World4XCanon.Editor.RegionOneCanonicalSourceGate.Validate','-logFile',('"'+$log+'"'))
$process=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $arguments -PassThru
$start=Get-Date
$nextTrace=0
try{
 while(-not $process.HasExited){
  $elapsed=((Get-Date)-$start).TotalSeconds
  if($elapsed -ge $nextTrace){
   Write-Host ("UNITY_SOURCE_IMPORT_PROGRESS pid="+$process.Id+" elapsed_seconds="+[int]$elapsed)
   if(Test-Path $log){Get-Content $log -Tail 8}
   $nextTrace=$elapsed+20
  }
  if($elapsed -gt 900){
   & taskkill.exe /PID $process.Id /T /F | Out-Null
   throw 'Unity import/source gate exceeded fifteen minutes; exact process stopped and logs retained for diagnosis'
  }
  Start-Sleep -Seconds 2
  $process.Refresh()
 }
 $process.WaitForExit()
 if(Test-Path $log){Get-Content $log -Tail 120}
 if($process.ExitCode -ne 0){throw "Unity import/source gate failed: $($process.ExitCode)"}
 if(-not(Test-Path $env:ELDORIA_REGION1_SOURCE_EVIDENCE)){throw 'Source gate did not execute'}
}finally{
 if(Test-Path $log){Write-Host 'UNITY_IMPORT_DIAGNOSTIC_LOG_RETAINED';Get-Content $log -Tail 30}
}
