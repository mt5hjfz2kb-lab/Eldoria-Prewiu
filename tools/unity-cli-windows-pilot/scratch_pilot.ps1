# Issue147 - Unity Editor scratch pilot. Never touch the Eldoria game checkout.
$ErrorActionPreference = 'Stop'
$root = Join-Path $env:RUNNER_TEMP ('eldoria-cli-editor-proof-' + $env:GITHUB_RUN_ID)
$project = Join-Path $root 'ScratchUnity'
New-Item -ItemType Directory -Force -Path $root | Out-Null
$report = [ordered]@{
  source_sha = $env:GITHUB_SHA
  run_id = $env:GITHUB_RUN_ID
  host = 'self-hosted Windows - only new project in RUNNER_TEMP'
  source_game_modified = $false
  source_branch_modified = $false
  no_restart = $true
  no_tripo_or_paid_calls = $true
  scratch_project = $true
  initial_processes = @()
  stages = @()
  success = $false
}
function Stage($name,$status,$note) {
  $report.stages += [ordered]@{name=$name;status=$status;note=$note}
  Write-Host ("STAGE "+$name+" => "+$status+": "+$note)
  Save-Report
}
function Save-Report {
  $report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $root 'proof.json') -Encoding utf8
}
function Run-Cli($argList,$file) {
  $prior = $ErrorActionPreference
  try {
    $ErrorActionPreference = 'Continue'
    $text = (& unity @argList 2>&1 | Out-String)
    $exit = $LASTEXITCODE
  } finally { $ErrorActionPreference = $prior }
  [IO.File]::WriteAllText((Join-Path $root $file), $text)
  if($exit -ne 0) { throw "Unity CLI exit $exit at $file. See evidence." }
  if($text -match '"success"\s*:\s*false' -or $text -match '"error"\s*:\s*"Compilation Failed"') { throw "Unity Editor command returned semantic failure: $file" }
  return $text
}
function Unity-Child($argList,$timeoutSec,$logName) {
  $p = Start-Process -FilePath $editor -ArgumentList $argList -PassThru
  $script:ownChildPids += $p.Id
  if(-not $p.WaitForExit($timeoutSec*1000)){
    Stage $logName 'TIMEOUT' 'Only the scratch Unity process will be stopped.'
    Stop-Process -Id $p.Id -ErrorAction SilentlyContinue
    throw "Unity scratch process exceeded bounded timeout"
  }
  if($p.ExitCode -ne 0) { throw "Scratch Unity exited with code $($p.ExitCode); inspect $logName" }
}
$script:ownChildPids = @()
$editor = $env:UNITY_EDITOR_PATH
try {
  if(-not $editor -or -not(Test-Path -LiteralPath $editor -PathType Leaf)) {
    throw 'UNITY_EDITOR_PATH not installed or missing'
  }
  $names=@('Unity','blender','UnityShaderCompiler','UnityPackageManager','UnityCrashHandler64')
  $active=@(foreach($n in $names){Get-Process -Name $n -ErrorAction SilentlyContinue})
  $report.initial_processes=@($active|ForEach-Object {$_.ProcessName})
  if($active.Count -gt 0){ throw 'Another native process is already active; do not disrupt it.' }
  Stage 'native-safety' 'PASS' 'No Unity/Blender/editor helper processes at start, editor exists.'
  $version=(Run-Cli @('--version') 'unity-version.txt').Trim()
  $report.unity_cli_version=$version
  Stage 'cli-existing' 'PASS' $version
  New-Item -ItemType Directory -Force $project | Out-Null
  $createLog=Join-Path $root 'unity-create-project.log'
  # Brand new project only. Do not check out Eldoria or overwrite any worktrees.
  $createArgs = @('-batchmode','-nographics','-quit','-createProject',('"' + $project + '"'),'-logFile',('"' + $createLog + '"'))
  Unity-Child $createArgs 330 'create-project'
  if(-not(Test-Path -LiteralPath (Join-Path $project 'Packages/manifest.json'))) {
    throw 'Scratch Unity project lacks a package manifest.'
  }
  Stage 'create-scratch' 'PASS' 'Created new isolated Unity project using installed Editor.'
  Push-Location $project
  try {
    $env:UNITY_NON_INTERACTIVE='1'
    $env:UNITY_CLI_CHANNEL='beta'
    $pipelineInstall=Run-Cli @('pipeline','install','--project-path',$project,'--format','json') 'pipeline-install-output.txt'
    Stage 'pipeline-install' 'PASS' 'Pipeline installed in scratch only.'
  } finally {Pop-Location}
  # Start the Editor using a unique scratch project, not the canonical Eldoria project.
  $editorLog=Join-Path $root 'unity-editor-resident.log'
  $editorArgs=@('-batchmode','-force-d3d11','-projectPath',('"' + $project + '"'),'-logFile',('"' + $editorLog + '"'))
  $resident=Start-Process -FilePath $editor -ArgumentList $editorArgs -PassThru
  $ownChildPids += $resident.Id
  $ready=$false
  for($i=0;$i -lt 70;$i++){
    if($resident.HasExited){throw ('Scratch Editor terminated early: '+$resident.ExitCode)}
    Start-Sleep -Seconds 5
    # Expected while the scratch Editor imports Pipeline; native stderr is not a fatal exception.
    $prior=$ErrorActionPreference
    $detected=''
    try {
      $ErrorActionPreference='Continue'
      $detected=((& unity command --project-path $project 2>&1) | Out-String)
    } catch {
      $detected=[string]$_
    } finally { $ErrorActionPreference=$prior }
    if($detected -match 'script eval|eval_file|editor\.playmode'){
      [IO.File]::WriteAllText((Join-Path $root 'editor-commands.txt'),$detected)
      $ready=$true;break
    }
  }
  if(-not $ready){throw 'Unity Pipeline Editor did not expose CLI commands within safe bound.'}
  Stage 'editor-connected' 'PASS' 'Unity CLI discovered live Pipeline commands on isolated scratch Editor.'
  $identity=$null
  for($retry=0;$retry -lt 8;$retry++){
    try {
      $identity=Run-Cli @('command','script','eval','return UnityEngine.Application.unityVersion;','--project-path',$project) 'unity-editor-eval-version.txt'
      break
    } catch {
      if($retry -eq 7){throw}
      Start-Sleep -Seconds 3
    }
  }
  Stage 'editor-eval-readonly' 'PASS' $identity.Trim().Substring(0,[Math]::Min(150,$identity.Trim().Length))
  # Real Editor screenshot via RenderTexture, with literal-free C# for Windows CLI quoting safety.
  function Capture-Scratch([string]$fileName) {
    $codes = ($fileName.ToCharArray() | ForEach-Object { '(char)' + [int][char]$_ }) -join ','
    $fileExpr = 'new string(new char[]{' + $codes + '})'
    $cs = @'
var camGo = new UnityEngine.GameObject();
var cam = camGo.AddComponent<UnityEngine.Camera>();
cam.orthographic=true; cam.orthographicSize=3f;
cam.clearFlags=UnityEngine.CameraClearFlags.SolidColor;
cam.backgroundColor=UnityEngine.Color.black;
cam.transform.position=new UnityEngine.Vector3(0f,0f,-10f);
cam.transform.rotation=UnityEngine.Quaternion.identity;
var rt = new UnityEngine.RenderTexture(320,180,24);
cam.targetTexture=rt;
cam.Render();
UnityEngine.RenderTexture.active=rt;
var image = new UnityEngine.Texture2D(320,180,UnityEngine.TextureFormat.RGB24,false);
image.ReadPixels(new UnityEngine.Rect(0,0,320,180),0,0);
image.Apply();
System.IO.File.WriteAllBytes(System.IO.Path.Combine(UnityEngine.Application.dataPath,__FILE_EXPR__),image.EncodeToPNG());
cam.targetTexture=null;
UnityEngine.RenderTexture.active=null;
UnityEngine.Object.DestroyImmediate(image);
UnityEngine.Object.DestroyImmediate(rt);
UnityEngine.Object.DestroyImmediate(camGo);
return 320;
'@
    $cs = $cs.Replace('__FILE_EXPR__',$fileExpr)
    $null = Run-Cli @('command','script','eval',$cs,'--project-path',$project) ('unity-capture-' + $fileName + '.txt')
    $actual = Join-Path (Join-Path $project 'Assets') $fileName
    if(-not (Test-Path -LiteralPath $actual -PathType Leaf)) { throw "Unity Editor did not create $fileName" }
    if((Get-Item $actual).Length -lt 100){throw "Unity generated invalid/empty PNG $fileName"}
    Copy-Item -LiteralPath $actual -Destination (Join-Path $root $fileName) -Force
  }
  Capture-Scratch 'before-unity-cube.png'
  Stage 'before-unity-capture' 'PASS' 'Captured empty scene in actual scratch Unity Editor.'
  # No quotes in C# expressions: Unity CLI on PowerShell legacy command line strips literal quotes.
  $createCode='var p=UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube); p.transform.position=new UnityEngine.Vector3(0,0,0); return p.GetInstanceID();'
  $created = Run-Cli @('command','script','eval',$createCode,'--project-path',$project) 'unity-editor-create-cube.txt'
  if($created -notmatch '"result"\s*:\s*-?[0-9]+'){throw 'Editor did not confirm scratch GameObject instance ID.'}
  Stage 'editor-live-edit' 'PASS' 'Created an actual primitive cube in the isolated Unity Editor through CLI.'
  $readCode='return UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>(UnityEngine.FindObjectsSortMode.None).Length;'
  $exists = Run-Cli @('command','script','eval',$readCode,'--project-path',$project) 'unity-editor-verify-cube.txt'
  if($exists -notmatch '"result"\s*:\s*[1-9]'){throw 'Editor could not detect the new MeshRenderer.'}
  Stage 'editor-live-verify' 'PASS' 'Verified actual MeshRenderer exists inside scratch Editor.'
  Capture-Scratch 'after-unity-cube.png'
  $beforeHash = (Get-FileHash (Join-Path $root 'before-unity-cube.png') -Algorithm SHA256).Hash
  $afterHash = (Get-FileHash (Join-Path $root 'after-unity-cube.png') -Algorithm SHA256).Hash
  $report.visual_before_sha256=$beforeHash
  $report.visual_after_sha256=$afterHash
  if($beforeHash -eq $afterHash){throw 'Before/after RenderTexture images identical; no visual change verified.'}
  Stage 'real-unity-ab-captures' 'PASS' 'Two different native PNG captures rendered by Unity in scratch scene.'
  $report.success=$true
  Save-Report
} catch {
  $report.failure=[string]$_
  Stage 'overall' 'BLOCKED' ([string]$_)
  throw
} finally {
  foreach($pidToStop in @($ownChildPids | Select-Object -Unique)){
    try {
      $p=Get-Process -Id $pidToStop -ErrorAction SilentlyContinue
      if($p){Stop-Process -Id $pidToStop -ErrorAction SilentlyContinue}
    }catch {}
  }
  $report.only_own_scratch_processes_stopped=$true
  Save-Report
}
