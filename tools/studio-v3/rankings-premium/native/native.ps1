# Isolated native PC; only our scratch process can be stopped.
param([Parameter(Mandatory=$true)][string]$InputRoot,[Parameter(Mandatory=$true)][string]$OutRoot)
$ErrorActionPreference='Stop'
$root=$OutRoot
New-Item -ItemType Directory -Path $root | Out-Null
$project=Join-Path $env:RUNNER_TEMP ("pr198-scratch-unity-"+$env:GITHUB_RUN_ID)
$editor=$env:UNITY_EDITOR_PATH
if(-not $editor -or -not(Test-Path -LiteralPath $editor -PathType Leaf)){throw 'Unity Editor missing'}
if(@(Get-Process Unity,blender,UnityShaderCompiler,UnityPackageManager,UnityCrashHandler64 -ErrorAction SilentlyContinue).Count){throw 'Native process active; do not interrupt'}
$bc=Get-Command blender.exe -ErrorAction SilentlyContinue
if($bc){$blender=$bc.Source}else{$blender=Get-ChildItem (Join-Path $env:ProgramFiles 'Blender Foundation') -Directory | ForEach-Object {Join-Path $_.FullName 'blender.exe'} | Where-Object {Test-Path -LiteralPath $_} | Select-Object -First 1}
$addon=Join-Path $env:APPDATA 'Blender Foundation\Blender\5.2\scripts\addons\blender_mcp.py'
if(-not(Test-Path -LiteralPath $addon) -or (Get-FileHash -Algorithm SHA256 -LiteralPath $addon).Hash.ToLower() -ne 'eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5'){throw 'Installed Blender MCP hash changed'}
function Run-Child($exe,$args,$seconds,$label){
 $p=Start-Process -FilePath $exe -ArgumentList $args -PassThru -RedirectStandardOutput (Join-Path $root ($label+'.stdout.txt')) -RedirectStandardError (Join-Path $root ($label+'.stderr.txt'))
 if(-not $p.WaitForExit($seconds*1000)){Stop-Process -Id $p.Id -ErrorAction SilentlyContinue;throw ($label+' timed out (only own process stopped)')}
 if($p.ExitCode -ne 0){Get-Content (Join-Path $root ($label+'.stderr.txt')) -Tail 30;throw ($label+' exit '+$p.ExitCode)}
}
function Q([string]$v){return ('"'+$v+'"')}
$blenderOut=Join-Path $root 'blender'
Run-Child $blender @('-b','-t','4','--python-exit-code','1','--python',(Q (Join-Path $InputRoot 'blender_author.py')),'--',(Q (Join-Path $InputRoot 'native-design.json')),(Q $blenderOut),(Q $addon)) 480 'blender'
if(-not(Test-Path (Join-Path $blenderOut 'blender-proof.json'))){throw 'Blender MCP proof missing'}
Get-Content (Join-Path $blenderOut 'blender-proof.json')
# Create a new minimal UGUI project. No game worktree is opened.
New-Item -ItemType Directory -Path $project | Out-Null
foreach($dir in @('Assets\Editor','Assets\Domain','Assets\Fonts','Assets\Resources\PR198Art','Packages','ProjectSettings')){New-Item -ItemType Directory -Force (Join-Path $project $dir)|Out-Null}
$assets=Join-Path $project 'Assets\Resources\PR198Art'
Copy-Item -LiteralPath (Join-Path $InputRoot 'native-design.json') -Destination (Join-Path $project 'Assets\native-design.json')
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
& python (Join-Path $root 'medal-variants.py') $assets
if($LASTEXITCODE -ne 0){foreach($i in 0..3){Copy-Item (Join-Path $assets 'medallion.png') (Join-Path $assets ("medallion-"+$i+".png"))}}
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
 & python (Join-Path $InputRoot 'unity_mcp_probe.py') (Join-Path $root 'unity-mcp-proof.json') $project
 if($LASTEXITCODE -ne 0){throw 'Unity MCP status probe failed; no claim of MCP success'}
 $prior=$ErrorActionPreference;$ErrorActionPreference='Continue'
 try{$result=(& unity command script eval 'return NativeCapture.Run();' --project-path $project 2>&1|Out-String);$code=$LASTEXITCODE}finally{$ErrorActionPreference=$prior}
 [IO.File]::WriteAllText((Join-Path $root 'unity-native-command.txt'),$result)
 if($code -ne 0 -or -not(Test-Path (Join-Path $root 'unity-proof.json'))){Write-Host $result;throw 'Native capture did not complete'}
 Get-Content (Join-Path $root 'unity-proof.json')
}finally{
 if($resident -and -not $resident.HasExited){Stop-Process -Id $resident.Id -ErrorAction SilentlyContinue}
 Pop-Location
}
