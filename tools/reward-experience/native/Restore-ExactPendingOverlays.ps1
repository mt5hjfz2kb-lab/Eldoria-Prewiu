param(
 [Parameter(Mandatory=$true)][string]$Workspace,
 [Parameter(Mandatory=$true)][string]$Request,
 [Parameter(Mandatory=$true)][string]$Output
)
$ErrorActionPreference='Stop'
function Full([string]$p){[IO.Path]::GetFullPath($p).TrimEnd([IO.Path]::DirectorySeparatorChar)}
function Hash([string]$p){(Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.ToLowerInvariant()}
function NoLinks([string]$p){
 if(Test-Path -LiteralPath $p){if((Get-Item -LiteralPath $p -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw "Redirected recovery path: $p"}}
}
$root=Full $Workspace
$project=Full (Join-Path $root 'issue186-source/Unity')
NoLinks $root;NoLinks (Split-Path $project);NoLinks $project
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native process active; refuse source restoration'}
$data=Get-Content -LiteralPath $Request -Raw|ConvertFrom-Json
if($data.state -ne 'RECOVER_EXACT_PENDING_OVERLAYS' -or $data.ui_source -ne '92596b1550fcb2722fb582639bc05c24a7c6a7a8'){throw 'Unexpected exact recovery request'}
if(@($data.layers).Count -ne 2 -or $data.layers[0].run -ne '43178513272' -or $data.layers[1].run -ne '43175152232'){throw 'Exact newest-first overlay order required'}
$before=Full (Join-Path $root 'issue186-native-43178513272/vigilia-overlay.json')
$after=Full (Join-Path $root 'issue186-native-43175152232/vigilia-overlay.json')
$expected=@('68a2ef283c86e233fd385edcaafbeb1d5ce0c70785ca2be004d6e9d8d3022e25','1a426d956c821dc48ee59bf1b94aec4fd32445630665802af77735d455415382')
$manifests=@($before,$after)
$reviews=@('reviewed-vigilia-imported-metadata-nested-925.json','reviewed-vigilia-imported-metadata.json')
# Validate BOTH manifests and exact provenance before changing any source.
for($i=0;$i -lt 2;$i++){
 $path=$manifests[$i];NoLinks (Split-Path $path);NoLinks $path
 if((Hash $path) -ne $expected[$i]){throw 'Preserved original overlay manifest changed'}
 $saved=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
 if((Full $saved.project) -ne $project -or $saved.ui_source -ne $data.ui_source -or $saved.version -ne 1 -or @($saved.files).Count -ne 245){throw 'Preserved overlay provenance differs'}
}
$receipts=@()
for($i=0;$i -lt 2;$i++){
 $path=$manifests[$i];$saved=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
 $marker=$path+'.restored.json';$already=$false
 if(Test-Path -LiteralPath $marker -PathType Leaf){
  NoLinks $marker;$receipt=Get-Content -LiteralPath $marker -Raw|ConvertFrom-Json
  $already=$receipt.restored -eq $true -and (Full $receipt.project) -eq $project -and $receipt.ui_source -eq $data.ui_source -and $receipt.files -eq 245
 }
 if(-not $already){
  & (Join-Path $PSScriptRoot 'Set-VigiliaProofOverlay.ps1') -Operation Restore -Project $project -Manifest $path -ReviewedMetadata (Join-Path $PSScriptRoot $reviews[$i])
  if(-not(Test-Path -LiteralPath $marker -PathType Leaf)){throw 'Exact restoration receipt absent'}
 }
 $receipt=Get-Content -LiteralPath $marker -Raw|ConvertFrom-Json
 if($receipt.restored -ne $true -or (Full $receipt.project) -ne $project -or $receipt.files -ne 245){throw 'Exact restoration receipt invalid'}
 $receipts+=@{run=$data.layers[$i].run;manifest_sha256=$expected[$i];files=245;restored=$true;already_restored=$already}
}
# Independently verify the OLDEST original state after both layers peel.
$oldest=Get-Content -LiteralPath $after -Raw|ConvertFrom-Json
foreach($entry in @($oldest.files)){
 if($entry.path.Contains('..') -or $entry.path.Contains(':') -or -not($entry.path -eq 'Assets/Eldoria/BreachWatch.meta' -or $entry.path.StartsWith('Assets/Eldoria/BreachWatch/'))){throw 'Unowned final source path'}
 $path=Join-Path $project $entry.path;NoLinks $path
 if($entry.original){if(-not(Test-Path -LiteralPath $path -PathType Leaf) -or (Hash $path) -ne $entry.original){throw "Oldest exact original not restored: $($entry.path)"}}
 elseif(Test-Path -LiteralPath $path){throw "Temporary overlay file remained: $($entry.path)"}
}
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Unexpected native process after restoration'}
[IO.Directory]::CreateDirectory((Full $Output))|Out-Null
$report=@{status='EXACT_TWO_LAYER_SOURCE_RESTORATION_PASS';project=$project;source=$env:GITHUB_SHA;layers=$receipts;oldest_original_files_verified=245;unknown_overwrites=0;unity_executed=$false;cache_removed=$false;runtime_native_run=43178513272;runtime_png=20;runtime_firsthit_clicks=52;art_approved=$false}
[IO.File]::WriteAllText((Join-Path $Output 'source-restoration-report.json'),($report|ConvertTo-Json -Depth 8),(New-Object Text.UTF8Encoding($false)))
Write-Host 'ISSUE186_EXACT_TWO_LAYER_SOURCE_RESTORATION_PASS_245_245_NO_UNITY_NO_UNKNOWN_OVERWRITE'
