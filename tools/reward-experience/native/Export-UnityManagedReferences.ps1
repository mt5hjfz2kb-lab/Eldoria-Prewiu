param([Parameter(Mandatory=$true)][string]$Project,[Parameter(Mandatory=$true)][string]$Output)
$ErrorActionPreference='Stop'
$Project=(Resolve-Path -LiteralPath $Project).Path
$Output=[IO.Path]::GetFullPath($Output)
if(Test-Path -LiteralPath $Output){throw 'Refuse overwriting managed-reference evidence'}
$managed=Join-Path (Split-Path $env:UNITY_EDITOR_PATH) 'Data/Managed'
if(-not(Test-Path -LiteralPath $managed -PathType Container)){throw 'Installed Unity managed API directory unavailable'}
New-Item -ItemType Directory -Path $Output|Out-Null
$records=@();$missing=@()
function Copy-ManagedReference([string]$Path,[string]$Relative,[string]$Kind){
 $target=Join-Path $Output $Relative
 New-Item -ItemType Directory -Force (Split-Path $target)|Out-Null
 Copy-Item -LiteralPath $Path -Destination $target
 $hash=(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
 if((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $hash){throw 'Managed API copy hash mismatch'}
 @{file=$Relative;kind=$Kind;sha256=$hash;bytes=(Get-Item -LiteralPath $target).Length}
}
# Read-only compiler metadata ONLY. No license, settings, credentials, logs,
# native executables, player data, PDBs or whole machine/project archive.
foreach($file in Get-ChildItem -LiteralPath $managed -Recurse -File -Filter '*.dll'){
 if($file.Name -notmatch '^(UnityEngine|UnityEditor)(\..*)?\.dll$'){continue}
 $relative='engine/'+$file.FullName.Substring($managed.Length+1).Replace('\','/')
 $records+=Copy-ManagedReference $file.FullName $relative 'installed-unity-api'
}
if(-not @($records|Where-Object {$_.file -like '*/UnityEngine.CoreModule.dll'}).Count){throw 'Installed Unity CoreModule API absent'}
$dependencies=@('Eldoria.Domain.dll','Eldoria.Application.dll',
 'Eldoria.RoyalGold.Core.dll','Eldoria.RoyalGold.UI.dll',
 'UnityEngine.UI.dll','Unity.InputSystem.dll','Unity.TextMeshPro.dll')
foreach($name in $dependencies){
 $file=Join-Path $Project ('Library/ScriptAssemblies/'+$name)
 if(Test-Path -LiteralPath $file -PathType Leaf){
  $records+=Copy-ManagedReference $file ('project/'+$name) 'existing-project-dependency'
 }else{$missing+=@($name)}
}
@{source=$env:GITHUB_SHA;run=$env:GITHUB_RUN_ID;unity_editor_version=(Get-Item -LiteralPath $env:UNITY_EDITOR_PATH).VersionInfo.ProductVersion;
 purpose='ACTUAL_COMPILER_METADATA_NOT_UNITY_RENDER_OR_DEVICE_PROOF';files=$records;missing_project_dependencies=$missing}|
 ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $Output 'managed-reference-manifest.json') -Encoding UTF8
Write-Host "ACTUAL_INSTALLED_UNITY_MANAGED_METADATA_COPIED count=$($records.Count); missing=$($missing.Count); no Unity process or machine changes"
