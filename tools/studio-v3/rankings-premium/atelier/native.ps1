# Isolated native PC; only our scratch process can be stopped.
param([Parameter(Mandatory=$true)][string]$InputRoot,[Parameter(Mandatory=$true)][string]$OutRoot,[string]$ExistingBlender='')
$ErrorActionPreference='Stop'
$root=$OutRoot
New-Item -ItemType Directory -Path $root | Out-Null
$project=Join-Path $env:RUNNER_TEMP ("pr198-scratch-unity-"+$env:GITHUB_RUN_ID)
$editor=$env:UNITY_EDITOR_PATH
if(-not $editor -or -not(Test-Path -LiteralPath $editor -PathType Leaf)){throw 'Unity Editor missing'}
if(@(Get-Process Unity,blender,UnityShaderCompiler,UnityPackageManager,UnityCrashHandler64 -ErrorAction SilentlyContinue).Count){throw 'Native process active; do not interrupt'}
$bc=Get-Command blender.exe -ErrorAction SilentlyContinue
if($bc){$blender=$bc.Source}else{$blender=Get-ChildItem (Join-Path $env:ProgramFiles 'Blender Foundation') -Directory | ForEach-Object {Join-Path $_.FullName 'blender.exe'} | Where-Object {Test-Path -LiteralPath $_} | Select-Object -First 1}
# Windows Store aliases are not interpreters. Prefer Blender's installed Python.
$br=Split-Path -Parent $blender
$py=Get-ChildItem -LiteralPath $br -Directory | ForEach-Object {Join-Path $_.FullName 'python\bin\python.exe'} | Where-Object {Test-Path -LiteralPath $_ -PathType Leaf} | Select-Object -First 1
if($py){$pv=(& $py -c 'import sys; print(sys.version)' 2>&1|Out-String);if($LASTEXITCODE -ne 0){$py=$null}else{Write-Host ('BUNDLED_BLENDER_PYTHON '+$pv.Trim())}}
if(-not $py){Write-Warning 'No callable bundled Python; native editor route remains independent'}
$addon=Join-Path $env:APPDATA 'Blender Foundation\Blender\5.2\scripts\addons\blender_mcp.py'
if(-not(Test-Path -LiteralPath $addon) -or (Get-FileHash -Algorithm SHA256 -LiteralPath $addon).Hash.ToLower() -ne 'eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5'){throw 'Installed Blender MCP hash changed'}
function Run-Child([string]$exe,[string[]]$argList,[int]$seconds,[string]$label){
 $p=Start-Process -FilePath $exe -ArgumentList $argList -PassThru -RedirectStandardOutput (Join-Path $root ($label+'.stdout.txt')) -RedirectStandardError (Join-Path $root ($label+'.stderr.txt'))
 if(-not $p.WaitForExit($seconds*1000)){Stop-Process -Id $p.Id -ErrorAction SilentlyContinue;throw ($label+' timed out (only own process stopped)')}
 if($p.ExitCode -ne 0){Get-Content (Join-Path $root ($label+'.stderr.txt')) -Tail 30;throw ($label+' exit '+$p.ExitCode)}
}
function Q([string]$v){return ('"'+$v+'"')}
$blenderOut=Join-Path $root 'blender'
if($ExistingBlender){
 $src=Join-Path $ExistingBlender 'blender'
 $proof=Get-Content -LiteralPath (Join-Path $src 'blender-proof.json') -Raw | ConvertFrom-Json
 if($proof.actual_blender -ne '5.2.2 LTS' -or $proof.mesh_objects -ne 33 -or @($proof.actions|Where-Object {$_.status -eq 'success'}).Count -ne 2){throw 'Invalid existing Blender proof'}
 if((Get-FileHash -Algorithm SHA256 (Join-Path $src 'medallion.png')).Hash.ToLower() -ne '3999b214a387cd92cfd53880d08e92631f83a5fb177ea14e7d007b80cb3dec8f' -or (Get-FileHash -Algorithm SHA256 (Join-Path $src 'medallion.blend')).Hash.ToLower() -ne 'f84847c12cdeceb1eda11880b0d05e0025051c2689fae21baa4a43113f41e8ea'){throw 'Existing Blender bytes changed'}
 Copy-Item -LiteralPath $src -Destination $blenderOut -Recurse
 Write-Host 'PR198_BLENDER_VERIFIED_REUSED_NO_NEW_SPEND'
}else{
Run-Child -exe $blender -argList @('-b','-t','4','--python-exit-code','1','--python',(Q (Join-Path $InputRoot 'blender_author.py')),'--',(Q (Join-Path $InputRoot 'native-design.json')),(Q $blenderOut),(Q $addon)) -seconds 480 -label 'blender'
if(-not(Test-Path (Join-Path $blenderOut 'blender-proof.json'))){throw 'Blender MCP proof missing'}
}
Get-Content (Join-Path $blenderOut 'blender-proof.json')
# Create a new minimal UGUI project. No game worktree is opened.
New-Item -ItemType Directory -Path $project | Out-Null
foreach($dir in @('Assets\Editor','Assets\Domain','Assets\Fonts','Assets\Resources\PR198Art','Packages','ProjectSettings')){New-Item -ItemType Directory -Force (Join-Path $project $dir)|Out-Null}
$assets=Join-Path $project 'Assets\Resources\PR198Art'
Copy-Item -LiteralPath (Join-Path $InputRoot 'native-design.json') -Destination (Join-Path $project 'Assets\native-design.json')
Copy-Item -LiteralPath (Join-Path $InputRoot 'palette.json') -Destination (Join-Path $project 'Assets\palette.json')
Copy-Item (Join-Path $InputRoot 'domain\*.cs') (Join-Path $project 'Assets\Domain')
Copy-Item (Join-Path $InputRoot 'NativeProfile.cs') (Join-Path $project 'Assets')
Copy-Item (Join-Path $InputRoot 'NativeCapture.cs') (Join-Path $project 'Assets\Editor')
Copy-Item (Join-Path $InputRoot 'Cormorant.ttf') (Join-Path $project 'Assets\Fonts')
Copy-Item (Join-Path $InputRoot 'SourceSans.ttf') (Join-Path $project 'Assets\Fonts')
Copy-Item (Join-Path $InputRoot 'assets\*') $assets
Copy-Item (Join-Path $blenderOut 'medallion.png') (Join-Path $assets 'medallion.png')
# Same actual relief family, distinct gold/stone/enamel art variants prepared via deterministic color transforms (no new gameplay rarity).
@'
import sys,pathlib
from PIL import Image,ImageEnhance
p=pathlib.Path(sys.argv[1]); im=Image.open(p/"medallion.png").convert("RGBA")
for i,(sat,b) in enumerate([(1.0,1.0),(.55,.82),(.85,.7),(1.2,1.18)]):
 rgb=ImageEnhance.Brightness(ImageEnhance.Color(im.convert("RGB")).enhance(sat)).enhance(b);rgb.putalpha(im.getchannel("A"));rgb.save(p/("medallion-"+str(i)+".png"))
'@ | Set-Content -LiteralPath (Join-Path $root 'medal-variants.py') -Encoding utf8
# Pillow is optional on this PC; if absent, use same authored physical emblem without fabricated distinction.
$prior=$ErrorActionPreference;$ErrorActionPreference='Continue'
$variantExit=1
if($py){& $py (Join-Path $root 'medal-variants.py') $assets;$variantExit=$LASTEXITCODE}
$ErrorActionPreference=$prior
if($variantExit -ne 0){foreach($i in 0..3){Copy-Item (Join-Path $assets 'medallion.png') (Join-Path $assets ("medallion-"+$i+".png"))}}
[IO.File]::WriteAllText((Join-Path $project 'Packages\manifest.json'),'{"dependencies":{"com.unity.ugui":"2.0.0","com.unity.modules.imageconversion":"1.0.0","com.unity.modules.ui":"1.0.0","com.unity.modules.uielements":"1.0.0"}}')
[IO.File]::WriteAllText((Join-Path $project 'ProjectSettings\ProjectVersion.txt'),"m_EditorVersion: 6000.3.23f1"+[Environment]::NewLine)
$env:PR198_NATIVE_OUT=$root
$env:UNITY_NON_INTERACTIVE='1';$env:UNITY_CLI_CHANNEL='beta'
Push-Location $project
$resident=$null
try{
 $prior=$ErrorActionPreference;$ErrorActionPreference='Continue'
 $install=(& unity pipeline install --project-path $project --format json 2>&1|Out-String);$code=$LASTEXITCODE;$ErrorActionPreference=$prior
 [IO.File]::WriteAllText((Join-Path $root 'pipeline-install.txt'),$install)
 if($code -ne 0 -or $install -match '"success"\s*:\s*false'){throw 'Unity Pipeline installation failed; inspect scratch only'}
 $resident=Start-Process -FilePath $editor -ArgumentList @('-batchmode','-force-d3d11','-projectPath',(Q $project),'-logFile',(Q (Join-Path $root 'unity-editor.log'))) -PassThru
 $ready=$false
 for($i=0;$i -lt 66;$i++){
  if($resident.HasExited){throw ('Scratch Editor exited '+$resident.ExitCode)}
  Start-Sleep -Seconds 5
  $prior=$ErrorActionPreference;$ErrorActionPreference='Continue'
  try{$detected=(& unity command --project-path $project 2>&1|Out-String)}finally{$ErrorActionPreference=$prior}
  if($detected -match 'script eval|eval_file|editor\.playmode'){[IO.File]::WriteAllText((Join-Path $root 'editor-commands.txt'),$detected);$ready=$true;break}
 }
 if(-not $ready){throw 'Scratch editor Pipeline did not become ready'}
 # Probe transport independently; a failed probe must not discard native rendering.
 if($py){
  $prior=$ErrorActionPreference;$ErrorActionPreference='Continue'
  try{$probeText=(& $py (Join-Path $InputRoot 'unity_mcp_probe.py') (Join-Path $root 'unity-mcp-proof.json') $project 2>&1|Out-String);$probeExit=$LASTEXITCODE}finally{$ErrorActionPreference=$prior}
  [IO.File]::WriteAllText((Join-Path $root 'unity-mcp-probe-log.txt'),$probeText)
  Write-Host $probeText
  if($probeExit -ne 0){Write-Warning 'Unity MCP not confirmed; proceed with actual native editor CLI'}
 }else{Write-Warning 'Python unavailable; Unity MCP not tested in this run'}

 $prior=$ErrorActionPreference;$ErrorActionPreference='Continue'
 try{$result=(& unity command script eval 'return NativeCapture.Run();' --project-path $project 2>&1|Out-String);$code=$LASTEXITCODE}finally{$ErrorActionPreference=$prior}
 [IO.File]::WriteAllText((Join-Path $root 'unity-native-command.txt'),$result)
 if($code -ne 0 -or -not(Test-Path (Join-Path $root 'unity-proof.json'))){
  Write-Host $result
  Write-Warning 'CLI eval did not finish capture; retry with the same native editor and trusted batch method'
  if($resident -and -not $resident.HasExited){Stop-Process -Id $resident.Id -ErrorAction Stop;[void]$resident.WaitForExit(30000)}
  $resident=$null
  $resident=Start-Process -FilePath $editor -ArgumentList @('-batchmode','-force-d3d11','-projectPath',(Q $project),'-executeMethod','NativeCapture.Batch','-quit','-logFile',(Q (Join-Path $root 'unity-batch.log'))) -PassThru
  if(-not $resident.WaitForExit(420000)){throw 'Owned native batch capture timed out'}
  if(-not(Test-Path (Join-Path $root 'unity-proof.json'))){Get-Content (Join-Path $root 'unity-batch.log') -Tail 50;throw 'Native batch capture did not produce proof'}
  [IO.File]::WriteAllText((Join-Path $root 'capture-transport.txt'),'Unity Editor batch executeMethod')
 }else{[IO.File]::WriteAllText((Join-Path $root 'capture-transport.txt'),'Unity CLI live editor script eval')}

 Get-Content (Join-Path $root 'unity-proof.json')
}finally{
 if($resident -and -not $resident.HasExited){Stop-Process -Id $resident.Id -ErrorAction SilentlyContinue}
 Pop-Location
}
