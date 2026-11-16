param([Parameter(Mandatory=$true)][string]$SourceRoot,
      [Parameter(Mandatory=$true)][string]$OutputRoot)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
if(-not $env:UNITY_EDITOR_PATH -or -not(Test-Path $env:UNITY_EDITOR_PATH)){throw 'Unity editor unavailable'}
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Shared PC busy'}
if((Get-PSDrive C).Free -lt 20GB){throw 'Require 20 GiB free before isolated import'}
$root=Join-Path $env:RUNNER_TEMP ('visual-reboot-'+$env:GITHUB_RUN_ID)
if(Test-Path $root){throw 'Refuse to overwrite earlier evidence or project'}
$project=Join-Path $root 'Unity'
New-Item -ItemType Directory -Force -Path $project,$OutputRoot | Out-Null
$src=Join-Path $SourceRoot 'Unity'
$folders=@('Assets/Art_Reboot','ProjectSettings')
foreach($rel in $folders){
 $to=Join-Path $project $rel
 New-Item -ItemType Directory -Force -Path (Split-Path $to) | Out-Null
 Copy-Item -LiteralPath (Join-Path $src $rel) -Destination $to -Recurse
}
# Preserve the source scene bytes for comparison; never enter its Play Mode.
$files=@('Assets/Art_Reboot.meta',
 'Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity',
 'Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity.meta',
 'Assets/Eldoria/Content/EldoriaMobileURP.asset','Assets/Eldoria/Content/EldoriaMobileURP.asset.meta',
 'Assets/Eldoria/Content/EldoriaForwardRenderer.asset','Assets/Eldoria/Content/EldoriaForwardRenderer.asset.meta',
 'Assets/UniversalRenderPipelineGlobalSettings.asset','Assets/UniversalRenderPipelineGlobalSettings.asset.meta')
foreach($rel in $files){
 $to=Join-Path $project $rel
 New-Item -ItemType Directory -Force -Path (Split-Path $to) | Out-Null
 Copy-Item -LiteralPath (Join-Path $src $rel) -Destination $to
}
# This is a visual import lab, not a rebuilt game. No domain/runtime scripts are copied.
# Record the deliberate absence of original scene dependencies instead of claiming integration.
$manifest=Get-Content (Join-Path $src 'Packages/manifest.json') -Raw | ConvertFrom-Json
$urp=$manifest.dependencies.'com.unity.render-pipelines.universal'
if($urp -ne '17.3.0'){throw "Unexpected URP version: $urp"}
New-Item -ItemType Directory -Force -Path (Join-Path $project 'Packages') | Out-Null
$deps=@{'com.unity.render-pipelines.universal'=$urp;'com.unity.modules.imageconversion'='1.0.0'}
[IO.File]::WriteAllText((Join-Path $project 'Packages/manifest.json'),
 (@{dependencies=$deps}|ConvertTo-Json -Depth 5),([Text.UTF8Encoding]::new($false)))
$env:REBOOT_CAPTURE_OUTPUT=$OutputRoot
$log=Join-Path $OutputRoot 'unity-editor.log'
$arguments='-batchmode -quit -force-d3d11 -projectPath "'+$project+'" -executeMethod Eldoria.ArtReboot.Editor.RebootPilotCapture.BuildAndCapture -logFile "'+$log+'"'
$p=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $arguments -PassThru
$deadline=(Get-Date).AddMinutes(35)
while(-not $p.HasExited -and (Get-Date) -lt $deadline){Start-Sleep -Seconds 4;$p.Refresh()}
if(-not $p.HasExited){
 # Only the process started by THIS script; never kill by process name.
 & taskkill.exe /PID $p.Id /T /F | Out-Null
 throw 'Own Unity process timed out; logs retained'
}
$p.WaitForExit();$p.Refresh()
if($null -ne $p.ExitCode -and $p.ExitCode -ne 0){
 if(Test-Path $log){Get-Content -LiteralPath $log -Tail 100}
 throw "Unity exited $($p.ExitCode)"
}
if(-not(Test-Path $log)){throw 'No run-specific Unity log'}
if(-not(Select-String -LiteralPath $log -Pattern 'REBOOT_CAPTURE_OUTPUT' -Quiet)){throw 'Capture completion marker missing'}
if(-not(Select-String -LiteralPath $log -Pattern 'Application will terminate with return code 0' -Quiet)){throw 'Unity clean exit not proven'}
$hashes=@()
foreach($size in @('1100x1250','390x844','1600x900','844x390')){
 $name='unity-'+$size+'.png';$path=Join-Path $OutputRoot $name
 if(-not(Test-Path $path) -or (Get-Item $path).Length -lt 10000){throw "Missing/empty PNG: $name"}
 $hashes+=@{file=$name;sha256=(Get-FileHash $path -Algorithm SHA256).Hash}
}
Copy-Item (Join-Path $project 'Assets/Art_Reboot') (Join-Path $OutputRoot 'ImportedArtReboot') -Recurse
@{source_sha=$env:GITHUB_SHA;screenshots=$hashes;scope='Isolated visual import lab with copied inactive source hierarchy';
  unity_compiled=$true;unity_rendered=$true;gameplay_tested=$false;full_project_integration=$false;
  mobile_tested=$false;art_approved=$false}|ConvertTo-Json -Depth 8 | Set-Content (Join-Path $OutputRoot 'native-proof.json')
Write-Output 'REBOOT_NATIVE_IMPORT_AND_CAPTURE_COMPLETE_REQUIRES_VISUAL_REVIEW'
# Scratch is preserved for a failed run. Only delete own scratch after evidence copy succeeds.
if($root.StartsWith($env:RUNNER_TEMP) -and (Split-Path $root -Leaf) -eq ('visual-reboot-'+$env:GITHUB_RUN_ID)){
 # Windows PowerShell 5 Remove-Item fails on long Unity PackageCache paths.
 # Extended-length cmd removal is confined to this run's known scratch root.
 $extended='\\?\'+[IO.Path]::GetFullPath($root)
 & $env:ComSpec /d /c ('rd /s /q "'+$extended+'"')
 if(Test-Path -LiteralPath $root){throw 'Own scratch cleanup incomplete; evidence retained'}
 Write-Output 'REBOOT_OWN_SCRATCH_REMOVED' 
}
