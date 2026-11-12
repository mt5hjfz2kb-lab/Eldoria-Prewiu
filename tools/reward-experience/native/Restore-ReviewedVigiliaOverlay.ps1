param([Parameter(Mandatory=$true)][string]$Workspace,[Parameter(Mandatory=$true)][string]$Output)
$ErrorActionPreference='Stop'
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native process active before reviewed recovery'}
$source=Join-Path $Workspace 'issue186-source'
$project=Join-Path $source 'Unity'
$manifest=Join-Path $Workspace 'issue186-native-43146245415/vigilia-overlay.json'
if(-not(Test-Path -LiteralPath $manifest)){Write-Host 'NO_PRIOR_R19_OVERLAY';exit 0}
if(Test-Path -LiteralPath ($manifest+'.restored.json')){
 $r=Get-Content -LiteralPath ($manifest+'.restored.json') -Raw|ConvertFrom-Json
 if(-not $r.restored){throw 'Previous exact restoration marker invalid'}
 Write-Host 'EXACT_PRIOR_OVERLAY_ALREADY_RESTORED';exit 0
}
$head=(& git -C $source rev-parse HEAD).Trim()
if($LASTEXITCODE -or $head -ne '256117b03987216c4a100bfb607d3113c5e74f96'){throw 'Prior actual source advanced unexpectedly; preserve it'}
$tools=Join-Path $Output 'reviewed-restore-tools';[IO.Directory]::CreateDirectory($tools)|Out-Null
$hdr=@{Authorization="Bearer $env:GH_TOKEN";Accept='application/vnd.github+json';'User-Agent'='issue186-exact-reviewed-restoration'}
foreach($name in @('Set-VigiliaProofOverlay.ps1','reviewed-vigilia-imported-metadata.json')){
 $url="https://api.github.com/repos/$env:GITHUB_REPOSITORY/contents/tools/reward-experience/native/${name}?ref=$env:GITHUB_SHA"
 $f=Invoke-RestMethod -Uri $url -Headers $hdr
 if($f.encoding -ne 'base64' -or -not $f.content){throw 'Exact reviewed restoration input missing'}
 [IO.File]::WriteAllBytes((Join-Path $tools $name),[Convert]::FromBase64String($f.content))
}

$review=Get-Content -LiteralPath (Join-Path $tools 'reviewed-vigilia-imported-metadata.json') -Raw|ConvertFrom-Json
$generated=@()
foreach($entry in @($review.files)){
 if(-not $entry.installed){
  $p=Join-Path $project $entry.path
  if((Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.ToLowerInvariant() -ne $entry.current){throw 'Generated metadata changed after actual review'}
  $match=[regex]::Match([IO.File]::ReadAllText($p),'(?m)^guid: ([a-f0-9]{32})\r?$')
  if(-not $match.Success){throw 'Generated metadata GUID is not explicit'}
  $generated+=$match.Groups[1].Value
 }
}
if($generated.Count -ne 4){throw 'Expected exact four generated metadata GUIDs'}
$overlayRoot=[IO.Path]::GetFullPath((Join-Path $project 'Assets/Eldoria/BreachWatch'))+'\'
$refs=@();$checked=0
Get-ChildItem -LiteralPath (Join-Path $project 'Assets') -Recurse -File | Where-Object {
 $_.Extension -in @('.meta','.unity','.prefab','.asset','.mat','.controller','.overrideController','.anim','.mask','.playable') -and
 -not $_.FullName.StartsWith($overlayRoot,[StringComparison]::OrdinalIgnoreCase)
} | ForEach-Object {
 $checked++
 $hits=Select-String -LiteralPath $_.FullName -Pattern $generated -SimpleMatch -List
 if($hits){$refs+=$_.FullName.Substring($project.Length+1)}
}
$proof=@{checked=$checked;generated_guids=$generated;outside_overlay_references=$refs;source_mutated=$false}
[IO.File]::WriteAllText((Join-Path $Output 'generated-meta-reference-audit.json'),
 ($proof|ConvertTo-Json -Depth 6),(New-Object Text.UTF8Encoding($false)))
if($refs.Count){Write-Host ($refs|Out-String);throw 'Generated temporary metadata has external serialized references; preserve source'}
Write-Host "ISSUE186_GENERATED4_GUIDS_NO_OUTSIDE_OVERLAY_REFERENCES scanned=$checked"
& (Join-Path $tools 'Set-VigiliaProofOverlay.ps1') -Operation Restore -Project $project -Manifest $manifest
$r=Get-Content -LiteralPath ($manifest+'.restored.json') -Raw|ConvertFrom-Json
if(-not $r.restored -or $r.files -ne 245){throw 'Prior exact original245-file restoration missing'}
$dirty=@(& git -C $source diff --name-only)
if($LASTEXITCODE -or $dirty.Count){Write-Host ($dirty|Out-String);throw 'Tracked source remains changed after exact restoration; no checkout'}
Write-Host 'ISSUE186_REVIEWED_R19_EXACT245_RESTORED_WITH_UNKNOWN64_ARCHIVED; SOURCE_CLEAN_BEFORE_CHECKOUT'
