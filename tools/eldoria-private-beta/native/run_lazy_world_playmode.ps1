param(
 [Parameter(Mandatory=$true)][string]$Source,
 [Parameter(Mandatory=$true)][string]$OverlaySource,
 [Parameter(Mandatory=$true)][string]$Evidence,
 [Parameter(Mandatory=$true)][string]$ExpectedHead,
 [Parameter(Mandatory=$true)][string]$InputAuditPath
)
$ErrorActionPreference='Stop'
$Source=(Resolve-Path $Source).Path
$OverlaySource=(Resolve-Path $OverlaySource).Path
$Evidence=[IO.Path]::GetFullPath($Evidence)
$head=(git -C $OverlaySource rev-parse HEAD).Trim()
if($LASTEXITCODE -ne 0 -or $head -ne $ExpectedHead){throw 'Lazy-world candidate HEAD mismatch'}
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Shared native editor is busy'}
if(-not(Test-Path $env:UNITY_EDITOR_PATH -PathType Leaf)){throw 'Unity Editor not available'}
if(-not(Test-Path (Join-Path $Source 'Unity/Assets') -PathType Container)){throw 'Hot Unity project unavailable'}
if(Test-Path $Evidence){throw 'Refuse to overwrite existing native QA evidence'}

# Reuse the six already-audited engine input paths from the PR179 native pipeline.
# Test assets are an isolated PlayMode test assembly. No player saves or scenes are edited.
$overlays=@(
 'Unity/Assets/Eldoria/World4XCanon/RegionOneBlenderLandscape.cs',
 'Unity/Assets/Eldoria/World4XCanon/RegionOneCanonicalLandscape.cs',
 'Unity/Assets/Eldoria/World4XCanon/RegionOneResourcePlacement.cs',
 'Unity/Assets/Eldoria/World4XCanon/RegionOneTerrainRoutes.cs',
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs',
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaIntegrationBuild.cs',
 'Unity/Assets/Eldoria/BetaMemoryProbe.meta',
 'Unity/Assets/Eldoria/BetaMemoryProbe/Tests.meta',
 'Unity/Assets/Eldoria/BetaMemoryProbe/Tests/PlayMode.meta',
 'Unity/Assets/Eldoria/BetaMemoryProbe/Tests/PlayMode/Eldoria.BetaMemory.Tests.PlayMode.asmdef',
 'Unity/Assets/Eldoria/BetaMemoryProbe/Tests/PlayMode/Eldoria.BetaMemory.Tests.PlayMode.asmdef.meta',
 'Unity/Assets/Eldoria/BetaMemoryProbe/Tests/PlayMode/LazyRegionStartupTests.cs',
 'Unity/Assets/Eldoria/BetaMemoryProbe/Tests/PlayMode/LazyRegionStartupTests.cs.meta'
)

# The canonical beta10 pretester UX consists of 19 interdependent files.
# Without the whole set (notably RoyalContracts.Mailbox), the additive lazy
# engine host cannot compile and the existing HUD, mail and gameplay regress.
# Verify EVERY archived source and hot-project baseline BEFORE any mutation.
$uxRoot=Join-Path $OverlaySource 'tools/eldoria-private-beta/native/ux'
$uxManifestPath=Join-Path $uxRoot 'manifest.json'
if(-not(Test-Path $uxManifestPath -PathType Leaf)){throw 'Frozen 19-file UX manifest unavailable'}
$uxManifest=Get-Content $uxManifestPath -Raw|ConvertFrom-Json
if(@($uxManifest.files).Count -ne 19){throw 'Frozen pretester UX must contain exactly 19 files'}
$uxCandidateBytes=@{}
$uxPaths=@()
# recover_first_attempt.ps1 certifies the exact originally warmed source.
# The old native core applied the six engine overlays BEFORE its UX manifest,
# so two manifest baselines are staged intermediate files, not hot-source HEAD.
# Accept ONLY these frozen original hashes for these two files; every other
# UX baseline must match the immutable manifest, before any mutation.
$warmFrozenBaselines=@{
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs'='85533B5441B2F63D6E89A41C0561DC3689793A76A1D71375970241E4361E444C'
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaIntegrationBuild.cs'='4D84DC082FA107BCBD9B0DE03E1AD24D6CF0154F01A73150B9B9815846693478'
}
function Get-Sha256Bytes([byte[]]$data) {
 $hasher=[Security.Cryptography.SHA256]::Create()
 try {return [BitConverter]::ToString($hasher.ComputeHash($data)).Replace('-','').ToLowerInvariant()}
 finally {$hasher.Dispose()}
}
$strictUtf8=New-Object Text.UTF8Encoding($false,$true)
foreach($entry in $uxManifest.files){
 $rel=[string]$entry.path
 if(-not $rel.StartsWith('Unity/Assets/Eldoria/',[StringComparison]::Ordinal) -or
    $rel.Contains('..') -or $rel.Contains([char]92) -or $uxCandidateBytes.ContainsKey($rel)){
  throw "Unsafe or duplicate UX overlay: $rel"
 }
 $from=Join-Path $uxRoot ([string]$entry.overlay)
 $to=Join-Path $Source $rel
 if(-not(Test-Path $from -PathType Leaf) -or -not(Test-Path $to -PathType Leaf)){
  throw "Frozen UX source/baseline missing: $rel"
 }
 $baseline=(Get-FileHash -LiteralPath $to -Algorithm SHA256).Hash.ToLowerInvariant()
 $expectedBaseline=if($warmFrozenBaselines.ContainsKey($rel)){
  $warmFrozenBaselines[$rel].ToLowerInvariant()
 }else{([string]$entry.baseline_sha256).ToLowerInvariant()}
 if($baseline -ne $expectedBaseline){
  throw "Hot Unity base differs from the recovered original or UX manifest: $rel"
 }
 $bytes=[IO.File]::ReadAllBytes($from)
 if((Get-Sha256Bytes $bytes) -ne ([string]$entry.candidate_sha256).ToLowerInvariant()){
  # Windows git checkouts may translate LF to CRLF. Only exact UTF-8
  # CRLF-only normalization to the pinned, original byte hash is permitted.
  $decoded=$strictUtf8.GetString($bytes)
  $normalized=$decoded.Replace(([string][char]13+[string][char]10),[string][char]10)
  if($normalized.Contains([char]13)){throw "Noncanonical CR in UX overlay: $rel"}
  $bytes=[Text.Encoding]::UTF8.GetBytes($normalized)
  if((Get-Sha256Bytes $bytes) -ne ([string]$entry.candidate_sha256).ToLowerInvariant()){
   throw "Frozen UX overlay source SHA mismatch: $rel"
  }
 }
 $uxPaths+=@($rel)
 $uxCandidateBytes[$rel]=$bytes
}
if(@($uxPaths|Sort-Object -Unique).Count -ne 19){throw 'Frozen UX file membership not unique'}
Write-Host 'PR179_FROZEN_PRETESTER_UX19_VERIFIED_BEFORE_HOT_PROJECT_MUTATION'


# The warmed project has authored scene and imported art that MUST be an
# exact, previously reviewed Unity source. PR173 may have used this machine;
# no source SHA or unreviewed dirty asset is silently adopted by PR179.
$baseline='948ffa9837168a0b68dab80be76b66350422b23a'
$hotHead=(git -C $Source rev-parse HEAD).Trim()
if($LASTEXITCODE -ne 0 -or $hotHead -ne $baseline){
 throw 'Hot Unity source differs from exact reviewed frozen base; no mutation'
}
$InputAuditPath=(Resolve-Path $InputAuditPath).Path
$audit=Get-Content $InputAuditPath -Raw|ConvertFrom-Json
if($audit.base -ne $baseline){throw 'Hot native input audit base differs'}
$actualStatus=@(git -C $Source status --porcelain -- Unity)
if($LASTEXITCODE -ne 0 -or
  @($actualStatus).Count -ne @($audit.status).Count -or
  @((Compare-Object @($actualStatus) @($audit.status))).Count){
 throw 'Hot Unity pending input inventory differs from approved source, no mutation'
}
$actualDirty=@(git -C $Source -c core.quotepath=false ls-files --modified --others --exclude-standard -- Unity)
$expectedDirty=@($audit.files|ForEach-Object {$_.path})
if($LASTEXITCODE -ne 0 -or $actualDirty.Count -ne $expectedDirty.Count -or
   @((Compare-Object $actualDirty $expectedDirty)).Count){
 throw 'Hot Unity dirty input membership diverged from audited production art'
}
foreach($item in $audit.files){
 $file=Join-Path $Source $item.path
 if(-not(Test-Path $file -PathType Leaf) -or
    (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $item.sha256){
  throw "Hot Unity audited input SHA mismatch: $($item.path)"
 }
}
Write-Host 'PR179_HOT_UNITY_EXACT_FROZEN_BASE_AND_ALL_DIRTY_ASSETS_VERIFIED'

$trackedBefore=@(git -C $Source status --porcelain -- Unity)
if($LASTEXITCODE -ne 0){throw 'Unable to audit original Unity repository state'}
$ownedDirty=@(git -C $Source status --porcelain -- $overlays)
if($LASTEXITCODE -ne 0 -or $ownedDirty.Count){throw "Foreign pending changes in owned paths; no mutation: $($ownedDirty -join ',')"}
foreach($rel in $overlays){
 if(-not(Test-Path (Join-Path $OverlaySource $rel) -PathType Leaf)){throw "Candidate source missing: $rel"}
}
$preserve=@($overlays)+@($uxPaths)+@(
 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/GeneratedLighting/BakedSceneURP.asset',
 'Unity/Assets/UniversalRenderPipelineGlobalSettings.asset'
)
$projectSettings=Join-Path $Source 'Unity/ProjectSettings'
if(Test-Path $projectSettings){
 $preserve+=@(Get-ChildItem $projectSettings -File -Recurse | ForEach-Object {
  $_.FullName.Substring($Source.Length+1).Replace('\','/')
 })
}
$preserve=@($preserve|Sort-Object -Unique)
$backup=Join-Path $env:GITHUB_WORKSPACE ("lazy-region-preserved-"+$env:GITHUB_RUN_ID+"-"+$env:GITHUB_RUN_ATTEMPT)
if(Test-Path $backup){throw 'Native backup slot already occupied'}
$records=@()
foreach($rel in $preserve){
 $actual=Join-Path $Source $rel
 $exists=Test-Path $actual -PathType Leaf
 $saved=Join-Path $backup $rel
 if($exists){
  New-Item -ItemType Directory -Force (Split-Path $saved)|Out-Null
  Copy-Item -LiteralPath $actual -Destination $saved
 }
 $records+=@{path=$rel;existed=$exists;sha256=$(if($exists){(Get-FileHash $actual -Algorithm SHA256).Hash}else{$null})}
}
New-Item -ItemType Directory -Force $Evidence|Out-Null
$log=Join-Path $Evidence 'unity-lazy-region-playmode.log'
$xml=Join-Path $Evidence 'unity-lazy-region-playmode.xml'
$failure=$null
$process=$null
$verified=$false
try{
 # Stage the exact 19-file frozen UX first, then the six engine files with
 # the additive lazy-world host. This keeps mail, nickname, bastion, timers
 # and the Mailbox enum compatible with the one real gameplay gateway.
 foreach($entry in $uxManifest.files){
  $rel=[string]$entry.path
  $to=Join-Path $Source $rel
  [IO.File]::WriteAllBytes($to,[byte[]]$uxCandidateBytes[$rel])
  if((Get-FileHash -LiteralPath $to -Algorithm SHA256).Hash.ToLowerInvariant() -ne
     ([string]$entry.candidate_sha256).ToLowerInvariant()){
   throw "Frozen UX overlay failed in transit: $rel"
  }
 }
 Write-Host 'PR179_FROZEN_PRETESTER_UX19_STAGED_BEFORE_LAZY_ENGINE'
 foreach($rel in $overlays){
  $to=Join-Path $Source $rel
  New-Item -ItemType Directory -Force (Split-Path $to)|Out-Null
  Copy-Item -LiteralPath (Join-Path $OverlaySource $rel) -Destination $to -Force
  if((Get-FileHash $to -Algorithm SHA256).Hash -ne (Get-FileHash (Join-Path $OverlaySource $rel) -Algorithm SHA256).Hash){
   throw "Candidate source changed in transit: $rel"
  }
 }
 $env:VALORIA_GAMEPLAY_PROBE='1'
 $args=@('-batchmode','-force-d3d11','-projectPath',('"' + (Join-Path $Source 'Unity') + '"'),
  '-runTests','-testPlatform','PlayMode',
  '-testFilter','Eldoria.BetaMemory.Tests.LazyRegionStartupTests',
  '-testResults',('"' + $xml + '"'),'-logFile',('"' + $log + '"'))
 $process=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -PassThru
 $end=(Get-Date).AddMinutes(55)
 while(-not $process.WaitForExit(15000)){
  if((Get-Date) -gt $end){
   Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
   $null=$process.WaitForExit(15000)
   throw 'Own Unity lazy-world test exceeded 55 minutes; owner process stopped'
  }
 }
 if($process.ExitCode -ne 0){throw "Unity PlayMode failed with exit code $($process.ExitCode)"}
 if(-not(Test-Path $xml -PathType Leaf)){throw 'Unity did not emit a native PlayMode result XML'}
 [xml]$results=Get-Content $xml -Raw
 $root=$results.DocumentElement
 if($root.Name -ne 'test-run'){throw 'Not a Unity NUnit result'}
 $total=[int]$root.GetAttribute('total')
 $passed=[int]$root.GetAttribute('passed')
 $failed=[int]$root.GetAttribute('failed')
 $skipped=[int]$root.GetAttribute('skipped')
 # NUnit can pass the aggregate while the targeted fixture is skipped or
 # another case passes. Require the exact real-scene test case to have run.
 $requiredCase='RealWorldStartsOnFirstEligibleEntryWithoutSecondGatewayOrNewHUD'
 $cases=@($results.SelectNodes('//test-case')|Where-Object {
  $_.GetAttribute('name') -eq $requiredCase -or
  $_.GetAttribute('fullname').EndsWith('.'+$requiredCase,[StringComparison]::Ordinal)
 })
 if($total -ne 1 -or $passed -ne 1 -or $failed -ne 0 -or $skipped -ne 0 -or
    $root.GetAttribute('result') -ne 'Passed' -or $cases.Count -ne 1 -or
    $cases[0].GetAttribute('result') -ne 'Passed'){
  throw "Unity lazy-world real test not certified: total=$total passed=$passed failed=$failed skipped=$skipped matching_real_cases=$($cases.Count)"
 }
 $verified=$true
}catch{
 $failure=$_.Exception.Message
}finally{
 if($process -and -not $process.HasExited){
  $failure='Unity still running: project preserved for manual recovery, no unsafe restoration'
 }else{
  $issues=@()
  foreach($record in $records){
   $to=Join-Path $Source $record.path
   if($record.existed){
    $saved=Join-Path $backup $record.path
    try{Copy-Item -LiteralPath $saved -Destination $to -Force}catch{$issues+=$record.path;continue}
    if((Get-FileHash $to -Algorithm SHA256).Hash -ne $record.sha256){$issues+=$record.path}
   }elseif(Test-Path $to -PathType Leaf){
    try{Remove-Item -LiteralPath $to -Force}catch{$issues+=$record.path}
   }
  }
  $trackedAfter=@(git -C $Source status --porcelain -- Unity)
  if($LASTEXITCODE -ne 0 -or (@(Compare-Object $trackedBefore $trackedAfter).Count -gt 0)){
   $issues+='git status changed after test'
  }
  if($issues.Count){$failure="Restoration FAILED: $($issues -join ','); prior=$failure"}
  # Git status cannot detect changes to contents of *already dirty* art.
  # Without these SHA checks a PlayMode run could silently damage approved
  # imported models or .meta while still reporting 'restored=true'.
  $auditedInputChanges=@()
  foreach($item in $audit.files){
   $asset=Join-Path $Source $item.path
   if(-not(Test-Path $asset -PathType Leaf) -or
      (Get-FileHash -LiteralPath $asset -Algorithm SHA256).Hash -ne $item.sha256){
    $auditedInputChanges+=@($item.path)
   }
  }
  if($auditedInputChanges.Count -gt 0){
   $issues+=@("Audited production art byte changes: $($auditedInputChanges -join ',')")
   $failure="Restoration FAILED: $($issues -join ','); prior=$failure"
  }else{
   Write-Host 'PR179_AUDITED_ART_BYTES_UNCHANGED_AFTER_PLAYMODE'
  }
  $restored=$issues.Count -eq 0
  $report=@{
   source_head=$head;unity_tests_passed=$verified;restored=$restored;frozen_ux_files=$uxPaths.Count;
   hot_source_base=$hotHead;audited_input_files=@($audit.files).Count;
   failed_reason=$failure;owned_files=$overlays.Count;original_unity_status_count=$trackedBefore.Count;
   audited_input_byte_changes=$auditedInputChanges;
   tests_executed='Eldoria.BetaMemory.Tests.LazyRegionStartupTests';
   physical_iphone_verified=$false;testers_authorized=$false
  }
  $report|ConvertTo-Json -Depth 5|Set-Content -Path (Join-Path $Evidence 'lazy-world-native-result.json') -Encoding UTF8
 }
}
if($failure){throw $failure}
if(-not $verified){throw 'No PlayMode certification'}
Write-Host 'ELDORIA_P0_REAL_LAZY_WORLD_PLAYMODE_PASS_AND_PROJECT_RESTORED'
