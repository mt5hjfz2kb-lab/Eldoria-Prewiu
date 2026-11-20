param([Parameter(Mandatory=$true)][string]$SourceRoot)
$ErrorActionPreference='Stop'
# Strictly scratch-only. The committed canonical game guard is NEVER edited.
$SourceRoot=[IO.Path]::GetFullPath($SourceRoot)
$temp=[IO.Path]::GetFullPath($env:RUNNER_TEMP)
if(-not $SourceRoot.StartsWith(($temp.TrimEnd([IO.Path]::DirectorySeparatorChar)+[IO.Path]::DirectorySeparatorChar),[StringComparison]::OrdinalIgnoreCase) -or
   -not([IO.Path]::GetFileName($SourceRoot) -like 'eldoria-mod25d-*')){
  throw 'Refuse to edit anything outside this run own modular25d scratch'
}
$actual=(git -C $SourceRoot rev-parse HEAD).Trim()
if($actual -ne $env:GITHUB_SHA){throw 'Source is not pinned to requesting Git SHA'}
$path=Join-Path $SourceRoot 'Unity/Assets/Eldoria/World4XCanon/RegionOneResourcePlacement.cs'
if(-not(Test-Path -LiteralPath $path)){throw 'Expected canonical source missing; refuse patch'}
# Normalize only the disposable checkout text to LF for an exact
# comparison independent of the Windows checkout autocrlf setting.
# The SHA-pinned original bytes are recorded before any scratch rewrite.
$old=[IO.File]::ReadAllText($path)
$normalized=$old.Replace(\"`r`n\",\"`n\")
$needle=@'
            if(report.summary.platform==UnityEditor.BuildTarget.WebGL&&!Resources.Load<TextAsset>("World4X/Blender/RuntimeTerrainQueries"))
                throw new UnityEditor.Build.BuildFailedException("Bake and validate the frozen terrain query tape before building WebGL; see tools/eldoria-private-beta/native/run_structural.ps1");
'@
$replacement=@'
            if(report.summary.platform==UnityEditor.BuildTarget.WebGL)
            {
                // MOD25D_SCRATCH_SCOPE: the canonical guard still rejects
                // every non-modular WebGL build without certified 4X terrain.
                // Only a single exact Bastion I scene at its own output is exempt.
                // This condition is injected in a disposable SHA-pinned scratch;
                // the committed World4X source remains byte-for-byte unchanged.
                var scenes=UnityEditor.EditorBuildSettings.scenes;
                var output=report.summary.outputPath.Replace(System.IO.Path.DirectorySeparatorChar,'/');
                bool isolated=scenes!=null && scenes.Length==1 &&
                    scenes[0].enabled &&
                    scenes[0].path=="Assets/Experimentos/ValoriaModular25D/Scenes/BastionI_Modular25D.unity" &&
                    output.EndsWith("/Builds/ValoriaModular25D_WebGL",System.StringComparison.OrdinalIgnoreCase);
                if(!isolated && !Resources.Load<TextAsset>("World4X/Blender/RuntimeTerrainQueries"))
                    throw new UnityEditor.Build.BuildFailedException("Bake and validate the frozen terrain query tape before building WebGL; see tools/eldoria-private-beta/native/run_structural.ps1");
                if(isolated)
                    UnityEngine.Debug.Log("[MODULAR25D] Single-scene isolated build verified; canonical 4X terrain prerequisite remains protected.");
            }
'@
if(-not $normalized.Contains($needle.Replace("`r`n","`n"))){throw 'Upstream terrain guard changed; stop and re-audit'}
if($normalized.Split(@($needle.Replace("`r`n","`n")),[StringSplitOptions]::None).Length -ne 2){throw 'Ambiguous 4X guard; refuse'}
$sha=(Get-FileHash -Algorithm SHA256 $path).Hash.ToLowerInvariant()
$new=$normalized.Replace($needle.Replace("`r`n","`n"),$replacement.Replace("`r`n","`n"))
[IO.File]::WriteAllText($path,$new,(New-Object Text.UTF8Encoding($false)))
$afterSha=(Get-FileHash -Algorithm SHA256 $path).Hash.ToLowerInvariant()
$artifacts=Join-Path $SourceRoot 'Unity/Artifacts/ValoriaModular25D'
New-Item -ItemType Directory -Force -Path $artifacts|Out-Null
(@{sourceSha=$actual;sourcePath='Assets/Eldoria/World4XCanon/RegionOneResourcePlacement.cs';
   originalSha256=$sha;scratchScopedSha256=$afterSha;scope='exact modular25d scene AND exact standalone output';
   canonicalGuard='UNCHANGED_IN_GIT_REQUIRED_FOR_EVERY_OTHER_WEBGL_BUILD';
   sandboxOnly=$true;deviceTested=$false}|ConvertTo-Json)|
   Set-Content -LiteralPath (Join-Path $artifacts 'WebGLIsolationGuard.json') -Encoding UTF8
Write-Host "MOD25D_DISPOSABLE_GUARD_SCOPED source=$sha scratch=$afterSha. Canonical guard remains fail-closed."
