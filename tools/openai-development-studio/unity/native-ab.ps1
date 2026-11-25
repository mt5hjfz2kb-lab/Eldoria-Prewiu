param([Parameter(Mandatory=$true)][string]$Packet,
      [Parameter(Mandatory=$true)][string]$Output)
$ErrorActionPreference='Stop'
$source='5ef2aee67cf8fde57ba2fc4b3d929b517ee67944'
$ident='eldoria-openai-development-studio-20261125'
$request=Get-Content -LiteralPath (Join-Path $Packet 'native-request.json') -Raw | ConvertFrom-Json
if($request.workstream_id -ne $ident -or $request.source_sha -ne $source -or $request.approved_native_execution -ne $true -or $request.candidate_sha256 -ne '4e582b25a420383798e3aa4895d1b71b33158f18de0087165b8055adef068785'){throw 'Native request mismatch'}
$owner=[string]$request.owner
$revision=[int]$request.revision
$root=Join-Path $env:RUNNER_TEMP ('eldoria-studio-native-'+$env:GITHUB_RUN_ID)
function Assert-Lease {
  $h=@{Authorization='Bearer '+$env:GH_TOKEN;Accept='application/vnd.github+json'}
  $api="https://api.github.com/repos/$env:GITHUB_REPOSITORY/"
  $meta=Invoke-RestMethod -Uri ($api+'contents/pipeline/active-workstreams.json?ref=main') -Headers $h
  $blob=Invoke-RestMethod -Uri ($api+'git/blobs/'+$meta.sha) -Headers $h
  if($blob.sha -ne $meta.sha){throw 'Registry SHA mismatch'}
  $reg=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String(($blob.content -replace '\s',''))) | ConvertFrom-Json
  $held=@($reg.active | Where-Object {@($_.resources) -contains 'windows-runner-heavy'})
  if($held.Count -ne 1 -or $held[0].id -ne $ident -or $held[0].owner -ne $owner -or
    $held[0].ownership_revision -ne $revision -or $held[0].ownership_state -ne 'claimed' -or
    $reg.resource_rotation.current_workstream -ne $ident){throw 'Studio exclusive lease no longer valid'}
  if(@(Get-Process Unity,blender,UnityShaderCompiler,UnityPackageManager -ErrorAction SilentlyContinue).Count){throw 'Another native process active; never interrupt it'}
  if(-not $env:UNITY_EDITOR_PATH -or -not(Test-Path -LiteralPath $env:UNITY_EDITOR_PATH)){throw 'Unity editor not available'}
}
function Digest([string]$path) { (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash.ToLowerInvariant() }
Assert-Lease
New-Item -ItemType Directory -Path $Output -Force | Out-Null
if((Get-Item $env:RUNNER_TEMP).PSDrive.Free -lt 7GB){throw 'Insufficient scratch disk; no other files deleted'}
if(Test-Path -LiteralPath $root){throw 'Refuse existing scratch'}
$g=Get-Command git.exe -ErrorAction SilentlyContinue
if(-not $g){
  $gpath=@(Get-ChildItem "$env:LOCALAPPDATA/GitHubDesktop" -Directory -Filter 'app-*' -ErrorAction SilentlyContinue |
    Sort-Object Name -Descending | ForEach-Object {Join-Path $_.FullName 'resources/app/git/cmd/git.exe'} | Where-Object{Test-Path $_})
  if(!$gpath.Count){throw 'No installed Git'}
  $env:PATH="$(Split-Path $gpath[0]);$env:PATH"
}
$made=$false
$cleanExit=$false
try {
  git init -q $root
  if($LASTEXITCODE){throw 'git init failed'}
  $made=$true
  git -C $root remote add origin "https://github.com/$env:GITHUB_REPOSITORY.git"
  git -C $root config core.autocrlf false
  git -C $root config http.version HTTP/1.1
  $includes=@('/Unity/Assets/Experimentos/ValoriaModular25D/',
     '/Unity/Assets/Experimentos/ValoriaModular25D.meta','/Unity/Assets/Experimentos.meta',
     '/Unity/Assets/Eldoria/','/Unity/Assets/Eldoria.meta',
     '/Unity/Assets/EmaceArt/Slavic World Free/Meshes/','/Unity/Assets/EmaceArt/Slavic World Free/Materials/',
     '/Unity/Assets/EmaceArt/Slavic World Free/Texture/','/Unity/Assets/EmaceArt/Slavic World Free/URP_Support/','/Unity/Assets/EmaceArt/Slavic World Free/Prefabs/Village/Building/Shed/',
     '/Unity/Assets/Holotna/','/Unity/Assets/Holotna.meta','/Unity/Assets/StreamingAssets/','/Unity/Assets/StreamingAssets.meta',
     '/Unity/Assets/Plugins/','/Unity/Assets/Plugins.meta',
     '/Unity/Assets/DefaultVolumeProfile.asset','/Unity/Assets/UniversalRenderPipelineGlobalSettings.asset',
     '/Unity/Packages/','/Unity/ProjectSettings/')
  git -C $root sparse-checkout set --no-cone @includes
  if($LASTEXITCODE){throw 'Sparse source setup failed'}
  # Reuse only read-only Git objects from the prior exact-source certified scratch if still present.
  # No source, Library, files or Git refs of the other workstream are edited.
  $donor=Join-Path $env:RUNNER_TEMP 'eldoria-region-modular-45638738009/.git/objects'
  if(Test-Path -LiteralPath $donor -PathType Container){
    $alt=Join-Path $root '.git/objects/info/alternates'
    [IO.File]::WriteAllText($alt,($donor -replace '\\','/')+[Environment]::NewLine,[Text.UTF8Encoding]::new($false))
    Write-Host 'READ_ONLY_GIT_OBJECT_REUSE'
  }
  git -C $root fetch --no-tags --depth=1 --filter=blob:none origin $source
  if($LASTEXITCODE){throw 'Frozen Git fetch failed'}
  $ok=$false
  for($i=1;$i -le 3;$i++){
    git -C $root checkout -q --detach $source
    if($LASTEXITCODE -eq 0){$ok=$true;break}
    Start-Sleep -Seconds (3*$i)
  }
  if(-not $ok -or (git -C $root rev-parse HEAD).Trim() -ne $source){throw 'Source checkout mismatch'}
  $project=Join-Path $root 'Unity'
  $base='Assets/Experimentos/ValoriaModular25D'
  $shader=Join-Path $project "$base/Resources/ValoriaModular25D/DistrictGround.shader"
  $candidate=Join-Path $Packet 'DistrictGround.candidate.shader'
  if((Digest $shader) -ne 'f1010a667125f9da262c9b622c03bd228f77aa49e7ccfe9c9c0402a0920f75e9' -or
     (Digest $candidate) -ne '4e582b25a420383798e3aa4895d1b71b33158f18de0087165b8055adef068785'){
    throw 'Frozen source or reviewed candidate hash mismatch'
  }
  $cs=[IO.File]::ReadAllText($candidate)
  $old='Shader "Eldoria/Valoria/District ground"'
  if(([regex]::Matches($cs,[regex]::Escape($old))).Count -ne 1){throw 'Candidate shader identity drift'}
  $cs=$cs.Replace($old,'Shader "Eldoria/Studio/District ground candidate"')
  $newShader=Join-Path $project "$base/Resources/ValoriaModular25D/StudioDistrictGroundCandidate.shader"
  if(Test-Path $newShader){throw 'Unexpected candidate asset'}
  [IO.File]::WriteAllText($newShader,$cs,[Text.UTF8Encoding]::new($false))
  $ground=Join-Path $project "$base/Scripts/Modular25DGround.cs"
  $text=[IO.File]::ReadAllText($ground)
  $old='        Material districtMaterial;'
  if(([regex]::Matches($text,[regex]::Escape($old))).Count -ne 1){throw 'Ground source anchor drift'}
  $add=@'
        Material districtMaterial;
        bool studioCandidate;
#if UNITY_EDITOR
        public void StudioSelectGroundShader(bool candidate,bool barracks,bool granary,bool quarry,int level)
        {
            var old= districtMaterial;
            studioCandidate=candidate;
            districtMaterial=null;
            SetDistricts(barracks,granary,quarry,level);
            if(old && old!=districtMaterial) UnityEngine.Object.DestroyImmediate(old);
        }
#endif
'@
  $text=$text.Replace($old,$add)
  $old='Resources.Load<Shader>("ValoriaModular25D/DistrictGround")'
  if(([regex]::Matches($text,[regex]::Escape($old))).Count -ne 1){throw 'Ground shader load anchor drift'}
  $text=$text.Replace($old,'Resources.Load<Shader>(studioCandidate?"ValoriaModular25D/StudioDistrictGroundCandidate":"ValoriaModular25D/DistrictGround")')
  [IO.File]::WriteAllText($ground,$text,[Text.UTF8Encoding]::new($false))
  Copy-Item -LiteralPath (Join-Path $Packet 'EldoriaStudioNativeAB.cs') -Destination (Join-Path $project "$base/Editor/EldoriaStudioNativeAB.cs")
  $diff=@(git -C $root diff --name-only)
  if($diff.Count -ne 1 -or $diff[0] -ne "Unity/$base/Scripts/Modular25DGround.cs"){throw ('Unexpected scratch changes: '+($diff -join ','))}
  [ordered]@{frozen_sha=$source;original_shader_sha256=(Digest $shader);reviewed_shader_sha256=(Digest $candidate);
    renamed_shader_sha256=(Digest $newShader);only_tracked_change=$diff[0];pilot_sha=$env:GITHUB_SHA;no_game_save=$true} |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Output 'provenance.json') -Encoding utf8
  Assert-Lease
  $env:ELDORIA_STUDIO_OUTPUT=$Output
  $log=Join-Path $Output 'unity-native.log'
  $args=@('-batchmode','-quit','-force-d3d11','-projectPath',('"'+$project+'"'),
      '-executeMethod','Eldoria.Experimentos.ValoriaModular25D.Editor.EldoriaStudioNativeAB.Capture','-logFile',('"'+$log+'"'))
  $p=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $args -PassThru
  if(-not $p.WaitForExit(2700000)){
    & taskkill.exe /PID $p.Id /T /F | Out-Null
    throw 'Owned scratch Unity process exceeded bounded runtime'
  }
  if($p.ExitCode -ne 0){Get-Content -LiteralPath $log -Tail 120;throw "Unity failed $($p.ExitCode)"}
  $proof=Join-Path $Output 'native-evidence.json'
  if(-not(Test-Path -LiteralPath $proof)){throw 'Missing native evidence'}
  $e=Get-Content -LiteralPath $proof -Raw | ConvertFrom-Json
  if($e.sourceSha -ne $source -or $e.sourceFrames -ne 48 -or $e.engineVersion -ne '6000.3.23f1' -or
     -not $e.nativeWorldRender -or $e.artApproved -ne 'NO'){throw 'Incomplete native result'}
  foreach($f in $e.frames){if((Digest (Join-Path $Output $f.file)) -ne $f.sha256){throw 'Capture hash mismatch'}}
  if((Digest $shader) -ne 'f1010a667125f9da262c9b622c03bd228f77aa49e7ccfe9c9c0402a0920f75e9'){throw 'Original shader changed'}
  $cleanExit=$true
  Write-Host 'STUDIO_NATIVE_AB_48_FRAME_VERIFIED'
} finally {
  if($made -and (Test-Path -LiteralPath $root) -and -not @(Get-Process Unity,blender,UnityShaderCompiler,UnityPackageManager -ErrorAction SilentlyContinue).Count) {
    Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction Stop
    Write-Host 'STUDIO_OWN_SCRATCH_REMOVED'
  }
  [ordered]@{native_pass=$cleanExit;scratch_removed=(-not(Test-Path -LiteralPath $root));run_id=$env:GITHUB_RUN_ID} |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Output 'cleanup.json') -Encoding utf8
}
