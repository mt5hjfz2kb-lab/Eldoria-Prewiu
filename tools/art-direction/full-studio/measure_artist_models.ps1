# Owner-authorized final convergence check: world-space source-safe Qwen brushes, frozen original and cached models.
param([Parameter(Mandatory=$true)][string]$Source,
      [Parameter(Mandatory=$true)][string]$Reference,
      [Parameter(Mandatory=$true)][string]$Review,
      [Parameter(Mandatory=$true)][string]$Out)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $Out | Out-Null
if(-not (Test-Path $Source) -or -not (Test-Path $Reference) -or -not (Test-Path $Review)){
  throw 'Frozen GLB / real reference / camera JSON unavailable'
}
$sourceHash=(Get-FileHash $Source -Algorithm SHA256).Hash.ToLowerInvariant()
$bincmd=Get-Command blender.exe -ErrorAction SilentlyContinue
$blender=if($bincmd){$bincmd.Source}else{Get-ChildItem (Join-Path $env:ProgramFiles 'Blender Foundation') -Filter blender.exe -Recurse -File -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName}
if(-not $blender){throw 'Installed Blender unavailable'}
$reported=& $blender -b --factory-startup --python-exit-code 1 --python-expr "import addon_utils;m=next(x for x in addon_utils.modules(refresh=True) if x.__name__=='blender_mcp');print('ELDORIA_MCP='+m.__file__)"
if($LASTEXITCODE -ne 0){throw 'Installed BlenderMCP unavailable'}
$mcpLine=@($reported | Where-Object {$_ -like 'ELDORIA_MCP=*'}) | Select-Object -First 1
if(-not $mcpLine){throw 'BlenderMCP module path not returned'}
$mcp=$mcpLine.Substring('ELDORIA_MCP='.Length).Trim()
$mcpHash=(Get-FileHash $mcp -Algorithm SHA256).Hash.ToLowerInvariant()
if($mcpHash -ne 'eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5'){throw 'Installed BlenderMCP SHA changed'}
$ollama=Join-Path $env:LOCALAPPDATA 'Programs\Ollama\ollama.exe'
if(-not (Test-Path $ollama)){
  $ollama=Get-ChildItem (Join-Path $env:SystemDrive 'Users') -Directory -ErrorAction SilentlyContinue |
    ForEach-Object {Join-Path $_.FullName 'AppData\Local\Programs\Ollama\ollama.exe'} |
    Where-Object {Test-Path $_} | Select-Object -First 1
}
if(-not $ollama){throw 'Existing Ollama missing'}
$modelsDir=Join-Path $env:USERPROFILE '.ollama\models'
if(-not (Test-Path $modelsDir)){
  $modelsDir=Get-ChildItem (Join-Path $env:SystemDrive 'Users') -Directory -ErrorAction SilentlyContinue |
    ForEach-Object {Join-Path $_.FullName '.ollama\models'} |
    Where-Object {Test-Path $_} | Select-Object -First 1
}
if($modelsDir -and (Test-Path $modelsDir)){$env:OLLAMA_MODELS=$modelsDir}
$env:OLLAMA_HOST='127.0.0.1:11434'
$env:OLLAMA_NUM_PARALLEL='1'
$env:OLLAMA_MAX_LOADED_MODELS='1'
$env:ELDORIA_ARTIST_NUM_CTX='16384'
$server=$null;$wasActive=$false
try{
  try{$null=Invoke-RestMethod 'http://127.0.0.1:11434/api/version' -TimeoutSec 3;$wasActive=$true}catch{}
  if(-not $wasActive){
    $server=Start-Process -FilePath $ollama -ArgumentList 'serve' -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $Out 'ollama-stdout.log') -RedirectStandardError (Join-Path $Out 'ollama-stderr.log')
  }
  $ready=$false
  for($i=0;$i -lt 45;$i++){
    try{$null=Invoke-RestMethod 'http://127.0.0.1:11434/api/version' -TimeoutSec 3;$ready=$true;break}
    catch{Start-Sleep -Seconds 1}
  }
  if(-not $ready){throw 'Ollama local service failed to respond'}
  $tags=Invoke-RestMethod 'http://127.0.0.1:11434/api/tags' -TimeoutSec 10
  $installed=@($tags.models | ForEach-Object {$_.name})
  $cases=@(
    @{label='4b-sculpt';model='qwen3-vl:4b-instruct';kind='mesh_edit'},
    @{label='4b-assembly';model='qwen3-vl:4b-instruct';kind='assembly'},
    @{label='8b-sculpt';model='qwen3-vl:8b-instruct-q4_K_M';kind='mesh_edit'},
    @{label='8b-professional';model='qwen3-vl:8b-instruct-q4_K_M';kind='professional'}
  )
  # Owner-authorized single real source/real camera experiment. Previous
  # 4B vs 8B and mandatory primitive-assembly tests are already evidenced.
  $cases=@($cases | Where-Object {$_.label -eq '4b-assembly'})
  foreach($case in $cases){
    if($case.model -notin $installed){throw ('No previously cached model; no download authorized: '+$case.model)}
  }
  $smi=Get-Command nvidia-smi.exe -ErrorAction SilentlyContinue
  $gpuBefore=if($smi){@(& $smi.Source --query-gpu=name,memory.total,memory.free --format=csv,noheader,nounits)}else{@()}
  $checks=@()
  foreach($case in $cases){
    $outCase=Join-Path $Out $case.label
    $log=Join-Path $Out ($case.label+'.log')
    $sw=[System.Diagnostics.Stopwatch]::StartNew()
    $params=@('tools/art-direction/full-studio/artist_loop.py',
      '--brief','ELDORIA OWNER ART QUALITY CONTRACT V1 — NEW ARTISTIC HYPOTHESIS AFTER 8B SOURCE OPERATIONS FAILED. On a COPY of the SAME frozen Valoria 2.65m wooden village watchtower, Qwen4B must design an ORIGINAL modest three-dimensional TIMBER SUPPORT BRACKET or ROOF EAVE structural piece as a SEPARATE MESH/CUBOID attached to the outer roof and wall with convincing contact, medieval construction logic, and actual camera-visible silhouette. Do NOT tweak existing face 324/325, replace the inherited texture atlas or add any centered cylinder on the roof apex. Independent A/B found repeated extrusions, source bevel and sculpt brushes unchanged at the player view. Choose real source measured Blender WORLD x/y/z (Z UP), check actual eave perimeter, prefer wall-side bracket supporting an outer roof edge, not an arbitrary rooftop ornament. Choose YOUR OWN suitable primitive or closed noncoplanar mesh, geometry dimensions, exact transform and finishing — no assistant-generated coordinates or recipe. Preserve correct roof, stairs, front entrance, original timber logs, UV and circulation. No pass is ART_APPROVED without independent review. STOP after first genuine visible candidate and hold it for its external A/B reviewer.'
      '--reference',$Reference,'--source',$Source,'--review',$Review,
      '--out',$outCase,'--model',$case.model,'--blender',$blender,
      '--transport','blendermcp','--mcp-addon',$mcp,'--mcp-sha256',$mcpHash,
      '--max-cycles','4','--min-successful-cycles','2',
      '--max-plan-attempts','5','--max-actions','3','--max-tokens','2048',
      '--vision-max-edge','768','--asset-category','architecture')
    if($case.kind -eq 'assembly'){$params+=@('--source-build-phase','--source-tool-sequencing')}
    elseif($case.kind -eq 'professional'){$params+=@('--phase','full','--source-tool-sequencing')}
    else{$params+=@('--phase','mesh_edit')}
    $code=999
    try{
      $ErrorActionPreference='Continue'
      & python @params *> $log
      $code=$LASTEXITCODE
    }catch{
      Add-Content -Path $log -Value ('RUNNER_ERROR: '+$_)
      $code=997
    }finally{
      $ErrorActionPreference='Stop'
      $sw.Stop()
    }
    $resultFile=Join-Path $outCase 'full-studio-result.json'
    $cycles=@()
    $qaCount=0
    if(Test-Path $resultFile){
      $result=Get-Content $resultFile -Raw | ConvertFrom-Json
      if($result.art_approved){throw 'False self-approval forbidden'}
      $qaCount=[int]$result.ai_authored_action_plans
      $cycles=@($result.cycles | ForEach-Object {@{cycle=$_.cycle;status=$_.status;screen_fraction=$_.screen_impact.changed_fraction}})
    }
    $visibilityReport=Join-Path $outCase 'source-inspection/source-inspection.json'
    $visibleFaces=0
    if(Test-Path $visibilityReport){
      $observations=Get-Content $visibilityReport -Raw | ConvertFrom-Json
      foreach($o in @($observations.objects)){
        $visibleFaces+=@($o.camera_visible_face_landmarks).Count
      }
    }
    $gpuAfter=if($smi){@(& $smi.Source --query-gpu=name,memory.total,memory.free --format=csv,noheader,nounits)}else{@()}
    $checks+=@{label=$case.label;model=$case.model;mode=$case.kind;
      elapsed_seconds=[math]::Round($sw.Elapsed.TotalSeconds,2);
      process_exit_code=$code;cycles=$cycles;result_present=(Test-Path $resultFile);
      real_camera_ray_visible_faces=$visibleFaces;qa_review_candidates=$qaCount;
      gpu_after=$gpuAfter;art_approved=$false}
    Write-Host ('ELDORIA_QWEN_LOCAL_ART_TRIAL '+$case.label+' code='+$code+' seconds='+[math]::Round($sw.Elapsed.TotalSeconds,1))
  }
  if((Get-FileHash $Source -Algorithm SHA256).Hash.ToLowerInvariant() -ne $sourceHash){throw 'FROZEN SOURCE MUTATED'}
  $report=@{schema_version=1;state='REAL_LOCAL_QWEN_ARTISTIC_BENCHMARK_INDEPENDENT_REVIEW_PENDING';
    original_source_sha256=$sourceHash;source_unchanged=$true;
    comparison='SAME_TOWER_SAME_REFERENCE_SAME_CAMERA';art_approved=$false;
    model_weight_training=$false;model_downloads=0;
    gpu_before=$gpuBefore;vision_max_edge=768;num_ctx=16384;max_tokens=2048;
    results=$checks}
  $report | ConvertTo-Json -Depth 15 | Set-Content -Encoding UTF8 (Join-Path $Out 'benchmark-results.json')
  Write-Host ('ELDORIA_QWEN_4B_8B_REAL_BLENDER_BENCHMARK_COMPLETE trials='+$checks.Count)
  # GitHub SUCCESS previously masked Python code=1 and zero acceptable
  # candidates. Make technical QA failure explicit, with artifacts uploaded.
  $unusable=@($checks | Where-Object {
    $_.process_exit_code -ne 0 -or $_.qa_review_candidates -lt 1 -or
    $_.real_camera_ray_visible_faces -lt 1
  })
  if($unusable.Count -gt 0){
    throw 'REAL_ARTIST_BENCHMARK_FAILED: no verified source-camera-ray-visible and QA reviewable candidate. Examine artifact, do not claim artistic success.'
  }
}finally{
  if($server){try{Stop-Process -Id $server.Id -ErrorAction SilentlyContinue}catch{}}
}
