# Eldoria owner-authorized isolated local Windows Qwen -> real Blender artistic proof.
# Never edits source, never installs models, never self-certifies visual quality.
param(
  [Parameter(Mandatory=$true)][string]$Source,
  [Parameter(Mandatory=$true)][string]$Reference,
  [Parameter(Mandatory=$true)][string]$Out,
  [string]$Model='qwen3-vl:4b-instruct'
)
$ErrorActionPreference='Stop'
if (Test-Path $Out) { if (@(Get-ChildItem $Out -Force).Count -gt 0) { throw 'Output must be new and empty' } }
New-Item -ItemType Directory -Path $Out -Force | Out-Null
$utf8=New-Object System.Text.UTF8Encoding($false)
function Save-Utf8([string]$p,[string]$t) { [IO.File]::WriteAllText($p,$t,$utf8) }
if (-not (Test-Path $Source) -or -not (Test-Path $Reference)) { throw 'Original source/reference missing' }
$sourceSHA=(Get-FileHash $Source -Algorithm SHA256).Hash.ToLowerInvariant()
$bin=Get-Command blender.exe -ErrorAction SilentlyContinue
$blender=$null
if ($bin) { $blender=$bin.Source }
if (-not $blender) {
  $folder=Join-Path $env:ProgramFiles 'Blender Foundation'
  $blender=Get-ChildItem $folder -Filter blender.exe -File -Recurse -ErrorAction SilentlyContinue |
    Select-Object -First 1 -ExpandProperty FullName
}
if (-not $blender) { throw 'Native Windows Blender executable missing' }
$ollama=Join-Path $env:LOCALAPPDATA 'Programs\Ollama\ollama.exe'
if (-not (Test-Path $ollama)) {
  $ollama=Get-ChildItem (Join-Path $env:SystemDrive 'Users') -Directory -ErrorAction SilentlyContinue |
    ForEach-Object { Join-Path $_.FullName 'AppData\Local\Programs\Ollama\ollama.exe' } |
    Where-Object { Test-Path $_ } | Select-Object -First 1
}
if (-not $ollama) { throw 'Ollama native binary missing' }
$env:OLLAMA_HOST='127.0.0.1:11434'
$models=Join-Path $env:USERPROFILE '.ollama\models'
if (-not (Test-Path $models)) {
  $models=Get-ChildItem (Join-Path $env:SystemDrive 'Users') -Directory -ErrorAction SilentlyContinue |
    ForEach-Object { Join-Path $_.FullName '.ollama\models' } |
    Where-Object { Test-Path $_ } | Select-Object -First 1
}
if ($models -and (Test-Path $models)) { $env:OLLAMA_MODELS=$models }
$serve=$null
$preexisting=$false
try {
  try { $null=Invoke-RestMethod 'http://127.0.0.1:11434/api/version' -TimeoutSec 3; $preexisting=$true } catch {}
  if (-not $preexisting) {
    $serve=Start-Process -FilePath $ollama -ArgumentList 'serve' -WindowStyle Hidden -PassThru `
      -RedirectStandardOutput (Join-Path $Out 'ollama-stdout.log') `
      -RedirectStandardError (Join-Path $Out 'ollama-stderr.log')
  }
  $ready=$false
  for($i=0;$i -lt 50;$i++) {
    try { $null=Invoke-RestMethod 'http://127.0.0.1:11434/api/version' -TimeoutSec 3; $ready=$true;break }
    catch { Start-Sleep -Seconds 1 }
  }
  if (-not $ready) { throw 'Existing local Ollama did not start' }
  $models=Invoke-RestMethod 'http://127.0.0.1:11434/api/tags' -TimeoutSec 10
  $names=@($models.models | ForEach-Object {$_.name})
  if ($Model -notin $names) { throw "Requested Qwen model not already cached: $Model" }
  $inspect=Join-Path $Out 'inspected'
  & $blender -b --factory-startup --python-exit-code 1 --python tools/art-direction/full-studio/inspect_source.py -- --source $Source --out $inspect
  if ($LASTEXITCODE -ne 0) { throw 'Native Blender source inspection failed' }
  $sourceInfo=Get-Content (Join-Path $inspect 'source-inspection.json') -Raw | ConvertFrom-Json
  if (-not $sourceInfo.source_unchanged -or $sourceInfo.source_sha256 -ne $sourceSHA) { throw 'Frozen source or identity failed inspection' }
  $target=(@($sourceInfo.objects | Sort-Object vertices -Descending)[0]).name
  if (-not $target -or $sourceInfo.asset_count -lt 1) { throw 'No verifiable original source mesh' }
  $srcInfo=(@($sourceInfo.objects | Where-Object name -eq $target)[0])
  $schema=@'
{
  "type":"object","additionalProperties":false,
  "properties":{
    "schema_version":{"const":2},
    "asset_id":{"type":"string","minLength":3,"maxLength":70},
    "observed_defect":{"type":"string","minLength":30,"maxLength":180},
    "design_intent":{"type":"string","minLength":30,"maxLength":200},
    "asset_decision":{"const":"MODIFY"},
    "expected_visual_difference":{"type":"string","minLength":30,"maxLength":200},
    "mobile_risks":{"type":"string","maxLength":180},
    "actions":{"type":"array","minItems":2,"maxItems":2,"prefixItems":[
      {"type":"object","additionalProperties":false,"properties":{
        "id":{"type":"string","minLength":3,"maxLength":30},
        "op":{"const":"sculpt_anchor"},
        "target":{"type":"string"},
        "reason":{"type":"string","minLength":20,"maxLength":150},
        "params":{"type":"object","additionalProperties":false,"properties":{
          "anchor":{"type":"string","enum":["x_min","x_max","y_min","y_max","z_min","z_max"]},
          "direction":{"type":"string","enum":["+X","-X","+Y","-Y","+Z","-Z"]},
          "distance":{"type":"number","minimum":0.8,"maximum":1.8},
          "radius":{"type":"number","minimum":1.5,"maximum":6}
        },"required":["anchor","direction","distance","radius"]}
      },"required":["id","op","target","reason","params"]},
      {"type":"object","additionalProperties":false,"properties":{
        "id":{"type":"string","minLength":3,"maxLength":30},
        "op":{"const":"material"},
        "target":{"type":"string"},
        "reason":{"type":"string","minLength":20,"maxLength":150},
        "params":{"type":"object","additionalProperties":false,"properties":{
          "base_color":{"type":"array","minItems":4,"maxItems":4,
                        "items":{"type":"number","minimum":0,"maximum":1}},
          "roughness":{"type":"number","minimum":0.45,"maximum":1},
          "metallic":{"type":"number","minimum":0,"maximum":0.15}
        },"required":["base_color","roughness","metallic"]}
      },"required":["id","op","target","reason","params"]}
    ]}
  },"required":["schema_version","asset_id","observed_defect","design_intent",
                  "asset_decision","expected_visual_difference","mobile_risks","actions"]
}
'@ | ConvertFrom-Json
  foreach($a in $schema.properties.actions.prefixItems) {
    $a.properties.target | Add-Member -MemberType NoteProperty -Name const -Value $target -Force
  }
  $sourceData=($srcInfo | ConvertTo-Json -Depth 15 -Compress)
  $prompt='ELDORIA genuine artistic authoring on a frozen Valoria tower. You are a visual artist using REAL Blender locally on Windows. Owner target image: first, inspected BEFORE frame: second. The camera and original object are immutable. SOURCE MESH DATA '+$sourceData+' . Choose exactly TWO original and distinct artistic actions: FIRST a meaningful sculpt_anchor to correct a visibly defective silhouette (you choose landmark, direction, distance and radius); SECOND a restrained plausible aged medieval stone PBR material on exactly the named original source mesh (you choose 4 RGBA numeric values, roughness, metallic). Match the medieval stone/Valoria reference. The actual Blender execution will reject invented names, invalid values or no inherited vertex edits; never return code. Strong improvement without destruction. Fill EVERY schema field as substantive text. This is one experiment; you cannot approve yourself.'
  $images=@(
    [Convert]::ToBase64String([IO.File]::ReadAllBytes($Reference)),
    [Convert]::ToBase64String([IO.File]::ReadAllBytes((Join-Path $inspect 'source-preview-blender.png')))
  )
  $payload=@{
    model=$Model; stream=$false; think=$false; format=$schema;
    options=@{temperature=0.38;num_ctx=12288;num_predict=1300};
    messages=@(
      @{role='system';content='You are a senior 3D environment artist creating actual decisions. Observe two images and verified Blender source mesh. Do not invent scene objects. Your two authored operations will be executed verbatim in real Blender. Respond JSON strictly.'},
      @{role='user';content=$prompt;images=$images}
    )
  }
  $body=$payload | ConvertTo-Json -Depth 60 -Compress
  Save-Utf8 (Join-Path $Out 'request-sha256.txt') ([Convert]::ToBase64String([Security.Cryptography.SHA256]::Create().ComputeHash([Text.Encoding]::UTF8.GetBytes($body))).Substring(0,32))
  Write-Host ('ELDORIA_PC_REAL_QWEN_INFERENCE_STARTED model='+$Model)
  $response=Invoke-RestMethod -Uri 'http://127.0.0.1:11434/api/chat' -Method Post -ContentType 'application/json' -Body ([Text.Encoding]::UTF8.GetBytes($body)) -TimeoutSec 1100
  $raw=$response.message.content
  if (-not $raw -or $raw.Length -lt 300) { throw 'Local Qwen output missing or incomplete' }
  Save-Utf8 (Join-Path $Out 'qwen-raw.json') $raw
  $plan=$raw | ConvertFrom-Json
  if ($plan.actions.Count -ne 2 -or $plan.actions[0].op -ne 'sculpt_anchor' -or $plan.actions[1].op -ne 'material') { throw 'Actual Qwen two-stage action contract rejected' }
  if ($plan.actions[0].target -ne $target -or $plan.actions[1].target -ne $target) { throw 'Qwen referenced nonexistent source mesh' }
  if ($plan.actions[0].id -eq $plan.actions[1].id) { throw 'Duplicate model action IDs' }
  $planFile=Join-Path $Out 'artist-plan.json'
  # An identical byte copy: Blender does NOT see any rewritten model numbers.
  [IO.File]::Copy((Join-Path $Out 'qwen-raw.json'),$planFile,$false)
  $output=Join-Path $Out 'native-blender-output'
  & $blender -b --factory-startup --python-exit-code 1 --python tools/art-direction/full-studio/blender_toolbox.py -- --source $Source --plan $planFile --out $output --texture-root $env:GITHUB_WORKSPACE --review tools/art-direction/full-studio/tests/valoria_tower_hero_review.json
  if ($LASTEXITCODE -ne 0) { throw 'Original PC Qwen authored art choices rejected by Blender' }
  $proof=Get-Content (Join-Path $output 'execution-proof.json') -Raw | ConvertFrom-Json
  if (-not $proof.blender_execution -or $proof.action_count -ne 2 -or -not $proof.inherited_mesh_data_changed -or -not $proof.matched_review_camera) { throw 'PC Blender did not modify actual original inherited mesh in fixed A/B camera' }
  if ($proof.art_approved -or $proof.unity_tested) { throw 'Technical pass cannot approve quality' }
  $verifiedSourceSHA=(Get-FileHash $Source -Algorithm SHA256).Hash.ToLowerInvariant()
  if ($sourceSHA -ne $verifiedSourceSHA) { throw 'Original game resource was modified' }
  $originalRawSHA=(Get-FileHash (Join-Path $Out 'qwen-raw.json') -Algorithm SHA256).Hash.ToLowerInvariant()
  $executedPlanSHA=(Get-FileHash $planFile -Algorithm SHA256).Hash.ToLowerInvariant()
  if ($originalRawSHA -ne $executedPlanSHA) { throw 'Original model values not preserved in Blender handoff' }
  $final=@{
    state='PC_LOCAL_QWEN_AND_BLENDER_REAL_SOURCE_ART_CYCLE_TECHNICAL_PASS'
    artist_model=$Model
    inference_location='SAME_WINDOWS_PC'
    blender_location='SAME_WINDOWS_PC'
    qwen_original_SHA256=$originalRawSHA
    executed_plan_SHA256=$executedPlanSHA
    frozen_source_SHA256=$sourceSHA
    action_types=@($plan.actions | ForEach-Object {$_.op})
    inherited_mesh_data_changed=$true
    camera_equivalent=$true
    local_pbr_material_changed=$true
    model_downloads=0
    art_approved=$false
    unity_tested=$false
    gallery_visual_review_required=$true
  }
  Save-Utf8 (Join-Path $Out 'same-pc-authenticity-proof.json') ($final | ConvertTo-Json -Depth 7)
  Write-Host ('ELDORIA_SAME_WINDOWS_PC_NATIVE_QWEN_TO_BLENDER_TECHNICAL_PASS action_types='+($final.action_types -join ','))
} finally {
  if ($serve) { try { Stop-Process -Id $serve.Id -ErrorAction SilentlyContinue } catch {} }
}
