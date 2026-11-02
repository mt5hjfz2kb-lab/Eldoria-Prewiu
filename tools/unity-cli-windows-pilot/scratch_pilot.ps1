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
  $text = (& unity @argList 2>&1 | Out-String)
  $exit = $LASTEXITCODE
  [IO.File]::WriteAllText((Join-Path $root $file), $text)
  if($exit -ne 0) { throw "Unity CLI exit $exit at $file. See evidence." }
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
    $detected=((& unity command --project-path $project 2>&1) | Out-String)
    if($detected -match 'script eval|eval_file|editor\.playmode'){
      [IO.File]::WriteAllText((Join-Path $root 'editor-commands.txt'),$detected)
      $ready=$true;break
    }
  }
  if(-not $ready){throw 'Unity Pipeline Editor did not expose CLI commands within safe bound.'}
  Stage 'editor-connected' 'PASS' 'Unity CLI discovered live Pipeline commands on isolated scratch Editor.'
  $identity=Run-Cli @('command','script','eval','return UnityEngine.Application.unityVersion;','--project-path',$project) 'unity-editor-eval-version.txt'
  Stage 'editor-eval-readonly' 'PASS' $identity.Trim().Substring(0,[Math]::Min(150,$identity.Trim().Length))
  # Exercise a real reversible edit through Unity CLI, not through project file patching.
  $createCode='var p=UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube); p.name="ELDORIA_CLI_TEST_CUBE"; p.transform.position=new UnityEngine.Vector3(0,0,0); return p.name;'
  $created=Run-Cli @('command','script','eval',$createCode,'--project-path',$project) 'unity-editor-create-cube.txt'
  if($created -notmatch 'ELDORIA_CLI_TEST_CUBE'){throw 'Editor eval did not report scratch GameObject created.'}
  Stage 'editor-live-edit' 'PASS' 'Created one cube in isolated Unity Editor through CLI.'
  $readCode='return UnityEngine.GameObject.Find("ELDORIA_CLI_TEST_CUBE") != null;'
  $exists=Run-Cli @('command','script','eval',$readCode,'--project-path',$project) 'unity-editor-verify-cube.txt'
  if($exists -notmatch 'true'){throw 'Live Editor could not find object created via CLI.'}
  Stage 'editor-live-verify' 'PASS' 'Verified object exists in editor memory.'
  # Capture deterministic images through the live Unity camera, not a fabricated mock.
  $env:ELDORIA_CLI_EVIDENCE = $root
  # The resident Editor inherits this variable only if set before launch, so pass absolute literal escaped path.
  $forCSharp=$root.Replace('\','/').Replace('"','\"')
  $captureCode=@'
var camGo=new UnityEngine.GameObject("ELDORIA_SCRATCH_CAPTURE_CAMERA");
var cam=camGo.AddComponent<UnityEngine.Camera>();
cam.orthographic=true; cam.orthographicSize=3f;
cam.clearFlags=UnityEngine.CameraClearFlags.SolidColor;
cam.backgroundColor=UnityEngine.Color.black;
cam.transform.position=new UnityEngine.Vector3(0f,0f,-10f);
cam.transform.rotation=UnityEngine.Quaternion.identity;
var rt=new UnityEngine.RenderTexture(320,180,24);
cam.targetTexture=rt;
cam.Render();
UnityEngine.RenderTexture.active=rt;
var image=new UnityEngine.Texture2D(320,180,UnityEngine.TextureFormat.RGB24,false);
image.ReadPixels(new UnityEngine.Rect(0,0,320,180),0,0);
image.Apply();
System.IO.File.WriteAllBytes("__OUTPUT_PATH__",image.EncodeToPNG());
cam.targetTexture=null;
UnityEngine.RenderTexture.active=null;
UnityEngine.Object.DestroyImmediate(image);
UnityEngine.Object.DestroyImmediate(rt);
UnityEngine.Object.DestroyImmediate(camGo);
return "SCRATCH_PNG_CAPTURE_OK";
'@
  $captureCode=$captureCode.Replace('__OUTPUT_PATH__',($forCSharp + '/after-unity-cube.png'))
  $capture=Run-Cli @('command','script','eval',$captureCode,'--project-path',$project) 'unity-editor-capture-output.txt'
  if(-not(Test-Path -LiteralPath (Join-Path $root 'after-unity-cube.png'))) { throw 'No real Unity PNG from editor render.' }
  Stage 'real-unity-capture' 'PASS' 'Unity RenderTexture PNG captured in scratch scene.'
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
