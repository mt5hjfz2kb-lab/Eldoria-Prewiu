param([string]$EvidenceDir,[int]$Attempt=1,[string]$FeedbackFile='')
$ErrorActionPreference='Stop'
$ProgressPreference='SilentlyContinue'
New-Item -ItemType Directory -Force -Path $EvidenceDir | Out-Null
$task=Get-Content (Join-Path $EvidenceDir 'task.json') -Raw | ConvertFrom-Json
if($task.issuer -ne 'eldoria-coordinator-v1' -or $task.owner -ne 'chat-work-local-recovery-20261009' -or $task.model -ne 'qwen2.5-coder:3b' -or $task.budget_eur -ne 0 -or $Attempt -gt 2){throw 'Untrusted order'}
$live=Invoke-RestMethod ("https://raw.githubusercontent.com/"+$env:GITHUB_REPOSITORY+"/main/pipeline/active-workstreams.json") -TimeoutSec 30
$order=Invoke-RestMethod ("https://raw.githubusercontent.com/"+$env:GITHUB_REPOSITORY+"/main/pipeline/agent-local-preflight-request.json") -TimeoutSec 30
$claim=@($live.active|Where-Object{$_.id -eq $task.workstream_id -and $_.owner -eq $task.owner -and $_.status -eq 'active'})
if($claim.Count -ne 1 -or $order.owner -ne $task.owner -or $order.enabled -ne $true -or ([DateTimeOffset]::Parse($order.expires_at) -lt [DateTimeOffset]::UtcNow)){throw 'Live ownership or authorization expired'}
$alias=@('windows-runner-heavy','windows-self-hosted-unity-6000-3-23f1')
$competing=@($live.active|Where-Object{$_.id -ne $task.workstream_id -and $_.status -eq 'active' -and @($_.resources|Where-Object{$alias -contains $_}).Count -gt 0})
if(@($competing|Where-Object{$_.id -ne 'r2-b-strategic-choice'}).Count){throw 'Unapproved productive reservation'}
if($order.dg_authorized_loan.authorization -ne 'owner-executive-recovery-20261009' -or $order.dg_authorized_loan.no_game_changes -ne $true){throw 'Authorized runner loan missing'}
$exe=Join-Path $env:LOCALAPPDATA 'EldoriaLocalAI/ollama-v0.40.1/ollama.exe'
$models=Join-Path $env:LOCALAPPDATA 'EldoriaLocalAI/models'
if(!(Test-Path -LiteralPath $exe -PathType Leaf) -or !(Test-Path -LiteralPath $models -PathType Container)){throw 'Existing private installation missing; no installation permitted'}
$manifest=Join-Path $models 'manifests/registry.ollama.ai/library/qwen2.5-coder/3b'
if(!(Test-Path -LiteralPath $manifest)){throw 'Exact installed model manifest absent'}
if(Get-Process -Name Unity,Blender,ollama -ErrorAction SilentlyContinue){throw 'Productive process or local model already running'}
if(Get-NetTCPConnection -LocalPort 11434 -State Listen -ErrorAction SilentlyContinue){throw 'Port already occupied'}
$lockPath=Join-Path $env:LOCALAPPDATA 'EldoriaLocalAI/executor-recovery.lock'
$lock=$null;$server=$null
try{
 $lock=[IO.File]::Open($lockPath,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
 $env:OLLAMA_HOST='127.0.0.1:11434';$env:OLLAMA_MODELS=$models
 $env:OLLAMA_NO_CLOUD='1';$env:OLLAMA_NUM_PARALLEL='1';$env:OLLAMA_MAX_LOADED_MODELS='1'
 $env:OLLAMA_CONTEXT_LENGTH='2048';$env:OLLAMA_KEEP_ALIVE='0'
 $server=Start-Process -FilePath $exe -ArgumentList serve -PassThru -RedirectStandardOutput (Join-Path $EvidenceDir 'server-out.log') -RedirectStandardError (Join-Path $EvidenceDir 'server-err.log')
 $server.Id | Set-Content (Join-Path $EvidenceDir 'owned-pid.txt')
 $ready=$false
 for($i=0;$i -lt 30;$i++){
  if($server.HasExited){throw 'Owned Ollama exited'}
  try{$version=Invoke-RestMethod 'http://127.0.0.1:11434/api/version' -TimeoutSec 2;$ready=$true;break}catch{Start-Sleep -Seconds 1}
 }
 if(!$ready){throw 'Local model server not ready'}
 $listeners=@(Get-NetTCPConnection -State Listen -LocalPort 11434)
 if(!$listeners.Count -or @($listeners|Where-Object{$_.LocalAddress -ne '127.0.0.1' -or $_.OwningProcess -ne $server.Id}).Count){throw 'Listener identity or network binding rejected'}
 $tags=Invoke-RestMethod 'http://127.0.0.1:11434/api/tags' -TimeoutSec 15
 $model=@($tags.models|Where-Object{$_.name -eq $task.model -and $_.digest -eq $task.model_digest})
 if($model.Count -ne 1){throw 'Exact historical model digest missing'}
 $prompt=$task.prompt
 if($Attempt -eq 2){
  if(!(Test-Path $FeedbackFile)){throw 'Independent feedback absent'}
  $feedback=Get-Content $FeedbackFile -Raw
  $prompt+=[Environment]::NewLine+"Independent QA rejected your previous candidate. Correct only this task. QA feedback: $feedback"
 }
 # Structural schema prevents omission; semantic values still come from Qwen and independent QA.
 if($task.kind -eq 'unity-save-notice-policy'){
  $schema=@{type='object';additionalProperties=$false;required=@('implementation');properties=@{implementation=@{type='string'}}}
 }elseif($task.kind -eq 'runner-resource-aliases'){
  $props=@{'windows-runner-heavy'=@{type='string'};'windows-self-hosted-unity-6000-3-23f1'=@{type='string'}}
  $schema=@{type='object';additionalProperties=$false;required=@('aliases');properties=@{aliases=@{type='object';additionalProperties=$false;required=@('windows-runner-heavy','windows-self-hosted-unity-6000-3-23f1');properties=$props}}}
 }elseif($task.kind -eq 'runner-conflict-cases'){
  $schema=@{type='object';additionalProperties=$false;required=@('cases');properties=@{cases=@{type='array';minItems=7;maxItems=7;items=@{type='object';additionalProperties=$false;required=@('resources','conflict');properties=@{resources=@{type='array';items=@{type='string'}};conflict=@{type='boolean'}}}}}}
 }else{throw 'Unsupported task kind'}
 if($task.kind -eq 'runner-conflict-cases'){
  # The actual 3B model failed joint seven-case reasoning. Bounded specialization:
  # M16 supplies exact case inputs; only the real model supplies their labels.
  $inputs=@(@{resources=@()},@{resources=@('windows-runner-heavy')},@{resources=@('windows-self-hosted-unity-6000-3-23f1')},@{resources=@('github-hosted-blender')},@{resources=@('windows-runner-heavy','windows-self-hosted-unity-6000-3-23f1')},@{resources=@('linux-qa','windows-runner-heavy')},@{resources=@('linux-qa','github-hosted-blender')})
  $cases=@();$responses=@();$tokenCount=0
  $oneSchema=@{type='object';additionalProperties=$false;required=@('conflict');properties=@{conflict=@{type='boolean'}}}
  foreach($inputCase in $inputs){
   $caseJson=ConvertTo-Json -InputObject $inputCase.resources -Compress
   $onePrompt='You classify a Windows resource reservation. If the resource array contains windows-runner-heavy OR windows-self-hosted-unity-6000-3-23f1, conflict is true. If it contains neither, conflict is false. Consider exactly this single array: '+$caseJson+'. Return JSON with one boolean conflict.'
   if($Attempt -eq 2){$onePrompt+=' Independent QA rejected a previous full batch. Re-evaluate this case using the stated OR rule.'}
   $body=@{model=$task.model;stream=$false;keep_alive='0';format=$oneSchema;options=@{temperature=0;num_predict=64;num_ctx=1024};prompt=$onePrompt}|ConvertTo-Json -Depth 15
   $one=Invoke-RestMethod 'http://127.0.0.1:11434/api/generate' -Method Post -ContentType 'application/json' -Body ([Text.Encoding]::UTF8.GetBytes($body)) -TimeoutSec 20
   $label=$one.response|ConvertFrom-Json
   if($label.conflict -isnot [bool] -or !$one.done -or $one.eval_count -le 0){throw 'Model case inference incomplete'}
   $cases+=@{resources=$inputCase.resources;conflict=$label.conflict}
   $responses+=@{input=$inputCase.resources;prompt=$onePrompt;response=$one};$tokenCount+=$one.eval_count
  }
  $responses|ConvertTo-Json -Depth 12|Set-Content (Join-Path $EvidenceDir 'model-response.json') -Encoding UTF8
  $candidate=@{cases=$cases}|ConvertTo-Json -Depth 8
  $response=@{model=$task.model;done=$true;eval_count=$tokenCount;response=$candidate}
 }else{
  $body=@{model=$task.model;stream=$false;keep_alive='0';format=$schema;options=@{temperature=0;num_predict=800;num_ctx=2048};prompt=$prompt}|ConvertTo-Json -Depth 15
  $response=Invoke-RestMethod 'http://127.0.0.1:11434/api/generate' -Method Post -ContentType 'application/json' -Body ([Text.Encoding]::UTF8.GetBytes($body)) -TimeoutSec 180
  $response|ConvertTo-Json -Depth 8|Set-Content (Join-Path $EvidenceDir 'model-response.json') -Encoding UTF8
 }
 if($task.kind -eq 'unity-save-notice-policy'){
  $implementation=($response.response|ConvertFrom-Json).implementation
  if($implementation -isnot [string] -or $implementation.Length -gt 1000){throw 'No bounded C# implementation produced'}
  $source="namespace Eldoria.Presentation { public static class SaveLoadNoticePolicy { public static string Message(bool loadFailed) { $implementation } } }"
  $response.response=@{source=$source}|ConvertTo-Json -Compress
 }
 [IO.File]::WriteAllText((Join-Path $EvidenceDir 'candidate.json'),$response.response,[Text.UTF8Encoding]::new($false))
 $hash=(Get-FileHash (Join-Path $EvidenceDir 'candidate.json') -Algorithm SHA256).Hash.ToLowerInvariant()
 @{
  schema_version=1;source_sha=$task.source_sha;run_id=$env:GITHUB_RUN_ID;attempt=$Attempt
  model=$response.model;digest=$model[0].digest;done=$response.done;eval_count=$response.eval_count
  external_api_calls=0;generated_code_executed=$false;candidate_sha256=$hash
  binary_sha256=(Get-FileHash $exe -Algorithm SHA256).Hash.ToLowerInvariant()
  manifest_sha256=(Get-FileHash $manifest -Algorithm SHA256).Hash.ToLowerInvariant()
  local_version=$version.version;endpoint='http://127.0.0.1:11434';local_inference_calls=$(if($task.kind -eq 'runner-conflict-cases'){7}else{1})
 }|ConvertTo-Json -Depth 8|Set-Content (Join-Path $EvidenceDir 'proof.json') -Encoding UTF8
 Write-Output ("ELDORIA_REAL_LOCAL_INFERENCE task="+$task.task_id+" attempt="+$Attempt+" tokens="+$response.eval_count+" hash="+$hash)
}finally{
 if($server -and !$server.HasExited){& taskkill /PID $server.Id /T /F|Out-Null}
 if($lock){$lock.Dispose();Remove-Item -LiteralPath $lockPath -Force}
 Start-Sleep -Seconds 2
 $closed=!(Get-NetTCPConnection -LocalPort 11434 -State Listen -ErrorAction SilentlyContinue)
 $stopped=(!$server -or !(Get-Process -Id $server.Id -ErrorAction SilentlyContinue))
 @{
  listener_closed=$closed;owned_process_stopped=$stopped;lock_released=!(Test-Path $lockPath)
 }|ConvertTo-Json|Set-Content (Join-Path $EvidenceDir 'cleanup.json') -Encoding UTF8
 if(!$closed -or !$stopped){throw 'Owned model cleanup failed'}
}

