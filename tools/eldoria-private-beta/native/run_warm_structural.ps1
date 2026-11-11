param(
 [Parameter(Mandatory=$true)][string]$Source,
 [Parameter(Mandatory=$true)][string]$Tools,
 [Parameter(Mandatory=$true)][string]$Evidence,
 [Parameter(Mandatory=$true)][string]$WebOutput,
 [string]$InputAuditPath
)
$ErrorActionPreference='Stop'
$Source=(Resolve-Path $Source).Path;$Tools=(Resolve-Path $Tools).Path
$Evidence=[IO.Path]::GetFullPath($Evidence);$WebOutput=[IO.Path]::GetFullPath($WebOutput)
$baseline='948ffa9837168a0b68dab80be76b66350422b23a'
if((git -C $Source rev-parse HEAD).Trim() -ne $baseline){throw 'Warm project is not the exact frozen commit'}
$pending=@(git -C $Source status --porcelain -- Unity)
if($LASTEXITCODE -ne 0){throw 'Cannot inspect warm source'}
$auditPath=if($InputAuditPath){(Resolve-Path $InputAuditPath).Path}else{Join-Path $Tools 'tools/eldoria-private-beta/native/existing-input-audit.json'}
$audit=Get-Content $auditPath -Raw|ConvertFrom-Json
if($audit.base -ne $baseline){throw 'Audited base differs'}
if(@($audit.status).Count -ne $pending.Count -or ($pending.Count -gt 0 -and @(Compare-Object @($audit.status) $pending).Count)){throw 'Pending source differs from reviewed inventory'}
$actualPaths=@(git -C $Source ls-files --modified --others --exclude-standard -- Unity)
$expectedPaths=@($audit.files|ForEach-Object {$_.path})
if($LASTEXITCODE -ne 0 -or $expectedPaths.Count -ne $actualPaths.Count -or ($actualPaths.Count -gt 0 -and @(Compare-Object $expectedPaths $actualPaths).Count)){throw 'Pending input membership changed'}
foreach($item in $audit.files){
 $file=Join-Path $Source $item.path
 if(-not(Test-Path $file -PathType Leaf) -or (Get-FileHash $file -Algorithm SHA256).Hash -ne $item.sha256){throw "Reviewed existing input changed: $($item.path)"}
}
$env:ELDORIA_BASELINE_INPUT_AUDIT=(Get-FileHash $auditPath -Algorithm SHA256).Hash
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native tools busy'}
if(Test-Path $WebOutput){throw 'Refuse replacing a previous WebGL output'}
New-Item -ItemType Directory -Force $Evidence|Out-Null
$utf8=New-Object Text.UTF8Encoding($false)
$overlays=@(
 'Unity/Assets/Eldoria/World4XCanon/RegionOneBlenderLandscape.cs',
 'Unity/Assets/Eldoria/World4XCanon/RegionOneCanonicalLandscape.cs',
 'Unity/Assets/Eldoria/World4XCanon/RegionOneResourcePlacement.cs',
 'Unity/Assets/Eldoria/World4XCanon/RegionOneTerrainRoutes.cs',
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs',
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaIntegrationBuild.cs'
)
 . (Join-Path $Tools 'tools/eldoria-private-beta/native/ux/overlay.ps1')
$overlays=@($overlays)+@($uxFiles|ForEach-Object {$_.path})
$overlays=@($overlays|Sort-Object -Unique)
$paths=@($overlays)+@($audit.files|ForEach-Object {$_.path})
# Unity populates URP runtime resource lists during WebGL compilation. Preserve
# these tracked inputs before any phase, just like ProjectSettings; do not
# mistake the post-build metadata for an authored runtime-source change.
$paths+=@(
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/GeneratedLighting/BakedSceneURP.asset',
 'Unity/Assets/UniversalRenderPipelineGlobalSettings.asset'
)
$paths+=@(Get-ChildItem (Join-Path $Source 'Unity/ProjectSettings') -File -Recurse|ForEach-Object {$_.FullName.Substring($Source.Length+1).Replace('\','/')})
$paths=@($paths|Sort-Object -Unique)
$backupRoot=Join-Path $env:GITHUB_WORKSPACE "beta-structural-preserved-inputs-$env:GITHUB_RUN_ID-$env:GITHUB_RUN_ATTEMPT"
if(Test-Path $backupRoot){throw 'Preserved input backup already exists'}
$savedAudit=Join-Path $Evidence 'existing-input-audit.json'
if([IO.Path]::GetFullPath($auditPath) -ne [IO.Path]::GetFullPath($savedAudit)){Copy-Item $auditPath $savedAudit}
$records=@()
foreach($path in $paths){
 $original=Join-Path $Source $path;$backup=Join-Path $backupRoot $path
 New-Item -ItemType Directory -Force (Split-Path $backup)|Out-Null
 Copy-Item $original $backup
 $records+=@{path=$path;backup=$backup;sha256=(Get-FileHash $original -Algorithm SHA256).Hash;overlay=($overlays -contains $path)}
}
[IO.File]::WriteAllText((Join-Path $Evidence 'warm-cache-before.json'),(@{base=$baseline;input_audit_sha256=$env:ELDORIA_BASELINE_INPUT_AUDIT;source_kind="frozen code plus reviewed existing generated assets and normalized metadata";files=$records}|ConvertTo-Json -Depth 6),$utf8)
$proof=Join-Path $Source 'docs/evidence/valoria-reference-clean'
$build=Join-Path $Source 'Unity/Builds/ValoriaWebPlaytest'
$suffix=".beta-preserved-$env:GITHUB_RUN_ID-$env:GITHUB_RUN_ATTEMPT"
$proofBackup=$proof+$suffix;$buildBackup=$build+$suffix
if((Test-Path $proofBackup) -or (Test-Path $buildBackup)){throw 'A prior preserved output exists; do not replace it'}
$proofMoved=$false;$buildMoved=$false;$proofSlotCleared=$false;$buildSlotCleared=$false
try {
 if(Test-Path $proof){Move-Item $proof $proofBackup;$proofMoved=$true};$proofSlotCleared=$true
 if(Test-Path $build){Move-Item $build $buildBackup;$buildMoved=$true};$buildSlotCleared=$true
 # All pending inputs have been reviewed and hash-pinned above, and are preserved.
 # The core additionally verifies that its four owned source files match HEAD
 # before applying the candidate; quarantined historical evidence is not an input.
 $core=Get-Content (Join-Path $Tools 'tools/eldoria-private-beta/native/run_structural.ps1') -Raw
 $needle='git -C $Source status --porcelain'
 if(([regex]::Matches($core,[regex]::Escape($needle))).Count -ne 1){throw 'Core clean-source gate changed'}
 $core=$core.Replace($needle,('git -C $Source diff --name-only -- '+($overlays -join ' ')))
 $corePath=Join-Path $Evidence 'executed-structural-core.ps1'
 [IO.File]::WriteAllText($corePath,$core,$utf8)
 & $corePath -Source $Source -Tools $Tools -Evidence $Evidence
} finally {
 if(@(Get-Process Unity -ErrorAction SilentlyContinue).Count){
  throw 'Unity is still active: preserve backups and project; do not restore under a live Editor'
 }
 if($proofSlotCleared -and (Test-Path $proof)){Move-Item $proof (Join-Path $Evidence 'native-output-at-exit')}
 if($proofMoved){Move-Item $proofBackup $proof}
 if($buildSlotCleared -and (Test-Path $build)){Move-Item $build $WebOutput}
 if($buildMoved){Move-Item $buildBackup $build}
 $conflicts=@()
 foreach($record in $records){
  $file=Join-Path $Source $record.path
  if(-not(Test-Path $file)){$conflicts+=$record.path;continue}
  $current=(Get-FileHash $file -Algorithm SHA256).Hash
  if($record.overlay){
   $candidate=(Get-FileHash (Get-UxCandidate $record.path) -Algorithm SHA256).Hash
   $uxBase=@($uxFiles|Where-Object {$_.path -eq $record.path}|ForEach-Object {$_.baseline_sha256})
   $allowedHashes=@($candidate,$record.sha256)+@($uxBase)
   if($env:ELDORIA_P0_LAZY_WORLD_BUILD -eq '1' -and
      $record.path -eq 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs'){
     $lazyCandidate=Join-Path $Tools $record.path
     if(-not(Test-Path $lazyCandidate -PathType Leaf)){throw 'Pinned full-world lazy host absent'}
     $allowedHashes+=@((Get-FileHash $lazyCandidate -Algorithm SHA256).Hash)
   }
   if($env:ELDORIA_P0_CITY_ONLY_BUILD -eq '1' -and
      $record.path -eq 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs'){
     # The exact composed file is emitted inside this run's evidence only
     # AFTER its frozen 662-line UX source and all three changes are verified.
     $composed=$env:ELDORIA_P0_COMPOSED_FILE
     if($composed -and (Test-Path $composed -PathType Leaf)){
       $allowedHashes+=@((Get-FileHash $composed -Algorithm SHA256).Hash)
     }
   }
   if($allowedHashes -notcontains $current){$conflicts+=$record.path;continue}
  }
  if($current -ne $record.sha256){Copy-Item $record.backup $file -Force}
 }
 $after=@(git -C $Source status --porcelain -- Unity)
 $statusChanges=@()
 if(@($audit.status).Count -eq 0){$statusChanges=@($after)}
 elseif($after.Count -eq 0){$statusChanges=@($audit.status)}
 else{$statusChanges=@(Compare-Object @($audit.status) $after)}
 $inputChanges=@()
 foreach($item in $audit.files){
  $file=Join-Path $Source $item.path
  if(-not(Test-Path $file) -or (Get-FileHash $file -Algorithm SHA256).Hash -ne $item.sha256){$inputChanges+=$item.path}
 }
 $restored=$conflicts.Count -eq 0 -and $statusChanges.Count -eq 0 -and $inputChanges.Count -eq 0
 [IO.File]::WriteAllText((Join-Path $Evidence 'warm-cache-restoration.json'),(@{conflicts=$conflicts;unity_status=$after;status_changes=$statusChanges;existing_input_changes=$inputChanges;restored=$restored}|ConvertTo-Json -Depth 6),$utf8)
 if(-not $restored){throw 'Warm source has unexpected differences; evidence saved, no automatic overwrite'}
}
