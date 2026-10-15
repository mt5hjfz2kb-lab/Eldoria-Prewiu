# Eldoria readonly visual reasoning diagnostic. Native PC, zero cost and zero art edits.
param(
  [string]$Source='Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/CoherentFamilyV1/Valoria_Tower_CohV1.glb',
  [string]$Reference='references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg',
  [string]$Out=''
)
$ErrorActionPreference='Stop'
if(-not $Out){$Out=Join-Path $env:TEMP ('eldoria-eyes-brain-'+[DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss'))}
if(-not (Test-Path $Source) -or -not (Test-Path $Reference)){throw 'Original source or reference missing'}
$expected='e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869'
$sourceSha=(Get-FileHash $Source -Algorithm SHA256).Hash.ToLowerInvariant()
if($sourceSha -ne $expected){throw 'Wrong original frozen Valoria source; abort'}
if(Test-Path $Out){if(@(Get-ChildItem $Out -Force).Count -gt 0){throw 'Evidence directory must be empty'}}
New-Item -ItemType Directory -Path $Out -Force | Out-Null
$cmd=Get-Command blender.exe -ErrorAction SilentlyContinue
$blender=if($cmd){$cmd.Source}else{Get-ChildItem (Join-Path $env:ProgramFiles 'Blender Foundation') -Filter blender.exe -Recurse -File -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName}
if(-not $blender){throw 'Native Blender not found; no installation allowed'}
$ollama=Join-Path $env:LOCALAPPDATA 'Programs\Ollama\ollama.exe'
if(-not (Test-Path $ollama)){
  $ollama=Get-ChildItem (Join-Path $env:SystemDrive 'Users') -Directory -ErrorAction SilentlyContinue |
    ForEach-Object {Join-Path $_.FullName 'AppData\Local\Programs\Ollama\ollama.exe'} |
    Where-Object {Test-Path $_ } | Select-Object -First 1
}
if(-not $ollama){throw 'Previously installed local Ollama not found'}
python -c "from PIL import Image;print('Pillow available')"
if($LASTEXITCODE -ne 0){throw 'Existing Pillow required; no download'}
$models=Join-Path $env:USERPROFILE '.ollama\models'
if(-not (Test-Path $models)){
  $models=Get-ChildItem (Join-Path $env:SystemDrive 'Users') -Directory -ErrorAction SilentlyContinue |
    ForEach-Object {Join-Path $_.FullName '.ollama\models' } |
    Where-Object {Test-Path $_} | Select-Object -First 1
}
if($models -and (Test-Path $models)){$env:OLLAMA_MODELS=$models}
$env:OLLAMA_HOST='127.0.0.1:11434'
$env:OLLAMA_NUM_PARALLEL='1'
$env:OLLAMA_MAX_LOADED_MODELS='1'
$env:ELDORIA_ARTIST_NUM_CTX='16384'
$server=$null
try {
  & $blender -b --factory-startup --python-exit-code 1 --python 'tools/art-direction/vision-diagnostic/render_source_views.py' -- --source $Source --out $Out
  if($LASTEXITCODE -ne 0){throw 'Original multi-view render failed'}
  $ready=$false
  try{$null=Invoke-RestMethod 'http://127.0.0.1:11434/api/version' -TimeoutSec 3;$ready=$true}catch{}
  if(-not $ready){
    $server=Start-Process -FilePath $ollama -ArgumentList 'serve' -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $Out 'ollama-stdout.log') -RedirectStandardError (Join-Path $Out 'ollama-stderr.log')
    for($i=0;$i -lt 40;$i++){
      try{$null=Invoke-RestMethod 'http://127.0.0.1:11434/api/version' -TimeoutSec 2;$ready=$true;break}
      catch{Start-Sleep -Seconds 1}
    }
  }
  if(-not $ready){throw 'Native local Ollama did not respond'}
  python 'tools/art-direction/vision-diagnostic/eyes_brain_exam.py' --views $Out --reference $Reference --out $Out --model 'qwen3-vl:4b-instruct' --model 'qwen3-vl:8b-instruct-q4_K_M'
  if($LASTEXITCODE -ne 0){throw 'No complete Qwen eyes/brain diagnosis; evidence preserved'}
  Write-Host ('ELDORIA_EYES_BRAIN_NEEDS_INDEPENDENT_REVIEW '+$Out)
}
finally {
  if($server){try{Stop-Process -Id $server.Id -ErrorAction SilentlyContinue}catch{}}
  if((Get-FileHash $Source -Algorithm SHA256).Hash.ToLowerInvariant() -ne $sourceSha){throw 'Original source changed!'}
}
