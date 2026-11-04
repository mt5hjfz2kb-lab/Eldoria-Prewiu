param([Parameter(Mandatory=$true)][string]$RequestPath)
$ErrorActionPreference='Stop'
$requestMetadata=Get-Content -LiteralPath $RequestPath -Raw | ConvertFrom-Json
if($requestMetadata.candidate_sha -notmatch '^[0-9a-f]{40}$'){ throw 'Invalid exact candidate SHA' }
$validationMode=[string]$requestMetadata.validation_mode
if($validationMode -notin @('identities_only','full')){ throw 'Explicit validation_mode required' }
$headers=@{Authorization="Bearer $env:GH_TOKEN";Accept='application/vnd.github+json'}

function Assert-MetadataLease {
    $url='https://api.github.com/repos/mt5hjfz2kb-lab/Eldoria-Prewiu/contents/pipeline/active-workstreams.json?ref=main'
    $response=Invoke-RestMethod -Uri $url -Headers $headers
    $live=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String(($response.content -replace '\s',''))) | ConvertFrom-Json
    $holders=@($live.active | Where-Object { @($_.resources) -contains 'windows-runner-heavy' })
    if($holders.Count -ne 1 -or $holders[0].id -ne $requestMetadata.workstream_id -or
       $holders[0].owner -ne $requestMetadata.owner -or
       $holders[0].ownership_revision -ne $requestMetadata.ownership_revision -or
       $holders[0].ownership_state -ne 'claimed'){
        throw 'Live Windows lease unavailable or changed; no Editor launched'
    }
}

Assert-MetadataLease
if(@(Get-Process Unity,blender,UnityShaderCompiler,UnityPackageManager -ErrorAction SilentlyContinue).Count){
    throw 'Existing native processes detected; do not touch their projects'
}
if(-not (Test-Path -LiteralPath $env:UNITY_EDITOR_PATH -PathType Leaf)){ throw 'Installed Editor missing' }
$editorVersion=& $env:UNITY_EDITOR_PATH -version
if($editorVersion -notmatch '6000\.3\.23f1'){ throw 'Unexpected Unity Editor version' }
. (Join-Path $PSScriptRoot 'native_process.ps1')
$processContract=& (Join-Path $PSScriptRoot 'test_native_process.ps1')
$gitMetadata=Get-Command git.exe -ErrorAction SilentlyContinue
if($gitMetadata){ $gitExe=$gitMetadata.Source }
else {
    $desktop=Join-Path $env:LOCALAPPDATA 'GitHubDesktop'
    $gitExe=Get-ChildItem -LiteralPath $desktop -Directory -Filter 'app-*' |
        Sort-Object Name -Descending | ForEach-Object {Join-Path $_.FullName 'resources/app/git/cmd/git.exe'} |
        Where-Object {Test-Path -LiteralPath $_} | Select-Object -First 1
}
if(-not $gitExe){ throw 'No existing Git executable; no install performed' }
# Unity Package Manager launches its own git process for Git dependencies.
# Calling Git through an absolute path alone does not expose it to Unity.
$gitDirectory=Split-Path -Parent $gitExe
$env:PATH=$gitDirectory + [IO.Path]::PathSeparator + $env:PATH
if(-not (Get-Command git.exe -ErrorAction SilentlyContinue)){ throw 'Git is not visible to child processes' }
$scratchRoot=Join-Path $env:RUNNER_TEMP ('em-native-' + $env:GITHUB_RUN_ID)
if(Test-Path -LiteralPath $scratchRoot){ throw 'Refuse to overwrite a previous native proof' }
New-Item -ItemType Directory -Path $scratchRoot | Out-Null
$sourceRoot=Join-Path $scratchRoot 'source'
$evidenceRoot=Join-Path $scratchRoot 'evidence'
New-Item -ItemType Directory -Path $evidenceRoot | Out-Null
$processContract | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $evidenceRoot 'process-contract.json')
$repositoryUrl='https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu.git'
# RUNNER_TEMP is cleaned between jobs. Always use an independent canonical
# clone; prior temporary checkouts are not durable cache or recovery sources.
$cloneArguments=@('clone','--filter=blob:none','--no-checkout','--depth=1','--branch','fix/unity-metadata-integrity-20261104',$repositoryUrl,$sourceRoot)
& $gitExe @cloneArguments
if($LASTEXITCODE -ne 0){ throw 'Isolated source clone failed' }
& $gitExe -C $sourceRoot config core.longpaths true
& $gitExe -C $sourceRoot config core.autocrlf false
& $gitExe -C $sourceRoot config core.eol lf
& $gitExe -C $sourceRoot fetch --depth=1 origin $requestMetadata.candidate_sha
if($LASTEXITCODE -ne 0){ throw 'Exact candidate fetch failed' }
& $gitExe -C $sourceRoot sparse-checkout set --no-cone '/Unity/' '/tools/' '/scripts/' '/pipeline/' '/docs/**/*.md' '/docs/**/*.json' '/docs/evidence/unity-metadata-20261104/' '/v0220/' '/tester-v0265/' '/references/' '/.gitattributes' '/.gitignore'
if($LASTEXITCODE -ne 0){ throw 'Isolated project materialization failed' }
& $gitExe -C $sourceRoot -c core.autocrlf=false checkout --force $requestMetadata.candidate_sha
if($LASTEXITCODE -ne 0){ throw 'Exact isolated checkout failed' }
$checked=(& $gitExe -C $sourceRoot rev-parse HEAD).Trim()
if($checked -ne $requestMetadata.candidate_sha){ throw 'Checkout identity mismatch' }
$projectRoot=Join-Path $sourceRoot 'Unity'
if($validationMode -eq 'full' -and (Test-Path -LiteralPath (Join-Path $projectRoot 'Assets/Eldoria/Resources/Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1.glb'))){
    throw 'Owner-rejected legacy Resource still present: no scene/runtime validation or Player build permitted'
}
# Git stores folder metadata but cannot store the corresponding empty directory.
# Reconstruct only explicitly declared folder identities before the first import.
Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets') -Filter '*.meta' -File -Recurse | ForEach-Object {
    if((Get-Content -LiteralPath $_.FullName -Raw) -match '(?m)^folderAsset:\s*yes\s*$'){
        $folderPath=$_.FullName.Substring(0,$_.FullName.Length-5)
        if(-not (Test-Path -LiteralPath $folderPath)){
            New-Item -ItemType Directory -Path $folderPath | Out-Null
        }
    }
}
$harnessDirectory=Join-Path $projectRoot 'Assets/Editor'
New-Item -ItemType Directory -Force -Path $harnessDirectory | Out-Null
Copy-Item -LiteralPath (Join-Path $sourceRoot 'tools/unity-metadata/MetadataNativeVerification.cs') -Destination (Join-Path $harnessDirectory 'MetadataNativeVerification.cs')
$manifest=Join-Path $sourceRoot 'docs/evidence/unity-metadata-20261104/recovery-manifest.json'
$recoveryEntries=@((Get-Content -LiteralPath $manifest -Raw | ConvertFrom-Json).entries | Where-Object {$_.disposition -eq 'restored'})
$checkoutRows=@($recoveryEntries | ForEach-Object {
    $metadataPath=Join-Path $sourceRoot $_.path
    $actual=(Get-FileHash -LiteralPath $metadataPath -Algorithm SHA256).Hash.ToLowerInvariant()
    @{path=$_.path;expected=$_.meta_sha256;actual=$actual;exact=($actual -eq $_.meta_sha256)}
})
$checkoutRows | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $evidenceRoot 'metadata-before-import.json')
if(@($checkoutRows | Where-Object {-not $_.exact}).Count){ throw 'Checkout changed original metadata bytes BEFORE Unity; inspect line endings/configuration' }
$nativeReport=Join-Path $evidenceRoot 'metadata-native.json'
. (Join-Path $sourceRoot 'tools/unity-metadata/native_settings.ps1')
. (Join-Path $sourceRoot 'tools/unity-metadata/native_test_contract.ps1')
$testIdentity=Set-MetadataTestIdentity (Join-Path $projectRoot 'ProjectSettings/ProjectSettings.asset') $env:GITHUB_RUN_ID
@{company=$testIdentity.Company;product=$testIdentity.Product;isolated_from_normal_save_identity=$true} |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidenceRoot 'persistence-isolation.json')

function Invoke-MetadataEditor([string]$name,[string[]]$extra) {
    Assert-MetadataLease
    if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native tools became busy; preserve their projects'}
    $editorLog=Join-Path $evidenceRoot ($name + '.log')
    $editorArguments=@('-batchmode','-force-d3d11','-projectPath',$projectRoot,'-logFile',$editorLog)+$extra
    $quotedArguments=@($editorArguments | ForEach-Object {if($_ -match '\s'){'"'+$_+'"'}else{$_}})
    $process=Start-Process -FilePath $env:UNITY_EDITOR_PATH -ArgumentList $quotedArguments -WorkingDirectory $projectRoot -PassThru
    $retainedEditorHandle=$process.Handle
    if(-not $process.WaitForExit(1800000)){
        & taskkill.exe /PID $process.Id /T /F | Out-Null
        throw "Only this isolated Editor timed out: $name"
    }
    $process.Refresh()
    if($process.ExitCode -ne 0){ Get-Content -LiteralPath $editorLog -Tail 100; throw "$name failed with exit $($process.ExitCode)" }
}

$testSummary=@()
$runtimePrepared=$false
try {
    if($validationMode -eq 'full'){
        # Reproduce the EXISTING canonical unity-slice.yml build preparation.
        # No source/geometry redesign or generated art is committed to Git.
        Assert-MetadataLease
        if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native tools busy before canonical preparation'}
        $blenderCommand=Get-Command blender.exe -ErrorAction SilentlyContinue
        $blenderPath=if($blenderCommand){$blenderCommand.Source}else{$null}
        if(-not $blenderPath){
            $blenderPath=Get-ChildItem 'C:\Program Files\Blender Foundation' -Recurse -File -Filter blender.exe -ErrorAction SilentlyContinue |
                Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
        }
        if(-not $blenderPath){throw 'Existing canonical Blender executable missing; no installation performed'}
        $authorScript=Join-Path $sourceRoot 'tools/bastion-iii/author-frontier.py'
        $blenderResult=Invoke-MetadataLoggedProcess $blenderPath @('-b','--python',$authorScript) $sourceRoot (Join-Path $evidenceRoot 'canonical-frontier.log') (Join-Path $evidenceRoot 'canonical-frontier.stderr.log') 300000
        $blenderResult | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidenceRoot 'blender-process.json')
        if($blenderResult.exit_code -ne 0){throw "Canonical frontier fixture generation failed, actual exit=$($blenderResult.exit_code)"}
        Invoke-MetadataEditor 'prepare-bastion-iii' @('-executeMethod','Eldoria.ReferenceClean.Editor.BastionIIIArtBake.Build')
        $preparedRoot=Join-Path $projectRoot 'Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Resources/BastionIII/Granary'
        $preparedRows=@(foreach($part in @('Granary','StockCrate','StockSack','StockBarrel','StoneWorkshop')){
            $prefab=Join-Path $preparedRoot ($part+'.prefab')
            if(-not(Test-Path -LiteralPath $prefab -PathType Leaf)){throw "Missing canonical runtime prefab: $part"}
            @{name=$part;sha256=(Get-FileHash -LiteralPath $prefab -Algorithm SHA256).Hash.ToLowerInvariant()}
        })
        @{scope='Unchanged canonical source replay in isolated project, not new artistic authoring';author_script_blob=(& $gitExe -C $sourceRoot rev-parse 'HEAD:tools/bastion-iii/author-frontier.py').Trim();prefabs=$preparedRows} |
            ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $evidenceRoot 'canonical-runtime-preparation.json')
        Copy-Item -LiteralPath (Join-Path $sourceRoot 'docs/bastion-iii/native/granary-bake.json') -Destination (Join-Path $evidenceRoot 'granary-bake.json')
        $runtimePrepared=$true
    }
    try {
        Invoke-MetadataEditor 'import-and-identities' @('-quit','-executeMethod','EldoriaMetadataNativeVerification.Run','-metadataManifest',$manifest,'-metadataCandidate',$checked,'-metadataReport',$nativeReport,'-metadataCompany',$testIdentity.Company,'-metadataProduct',$testIdentity.Product,'-metadataMode',$validationMode,'-metadataProjectRoot',$projectRoot)
    } finally {
        $observedDirectory=Join-Path $evidenceRoot 'metadata-observed'
        New-Item -ItemType Directory -Path $observedDirectory | Out-Null
        $changedRows=@($recoveryEntries | ForEach-Object {
            $metadataPath=Join-Path $sourceRoot $_.path
            $actual=(Get-FileHash -LiteralPath $metadataPath -Algorithm SHA256).Hash.ToLowerInvariant()
            if($actual -ne $_.meta_sha256){
                Copy-Item -LiteralPath $metadataPath -Destination (Join-Path $observedDirectory ($_.guid+'.meta'))
                @{path=$_.path;expected=$_.meta_sha256;actual=$actual;captured_file=$_.guid+'.meta'}
            }
        })
        ConvertTo-Json -InputObject $changedRows -Depth 4 | Set-Content -LiteralPath (Join-Path $evidenceRoot 'metadata-byte-differences.json')
    }
    # This historical folder identity could not be recovered. Preserve whatever
    # Unity genuinely generates in isolation, without calling it an original or
    # changing/instantiating the rejected model contained in that folder.
    $unrecoveredRelative='Assets/Eldoria/Resources/Valoria/HeroBastionGenerated.meta'
    $generatedMeta=Join-Path $projectRoot $unrecoveredRelative
    if(Test-Path -LiteralPath $generatedMeta){
        Copy-Item -LiteralPath $generatedMeta -Destination (Join-Path $evidenceRoot 'HeroBastionGenerated.new-folder.meta')
        @{path=$unrecoveredRelative;provenance='New folder identity generated by isolated Unity import, not recovered original';rejected_model_authorized=$false} |
            ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidenceRoot 'generated-folder-provenance.json')
    }
    $proof=Get-Content -LiteralPath $nativeReport -Raw | ConvertFrom-Json
    if($proof.state -ne 'PASS' -or $proof.candidate_sha -ne $checked){ throw 'Native identity verification did not pass' }
    if($validationMode -eq 'full') { foreach($platform in @('EditMode','PlayMode')){
        if($platform -eq 'PlayMode'){
            $fixture=Set-MetadataPlayModeFixture $evidenceRoot
            $fixture | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidenceRoot 'playmode-fixture.json')
        }
        $result=Join-Path $evidenceRoot ($platform + '.xml')
        Invoke-MetadataEditor $platform @('-runTests','-testPlatform',$platform,'-testResults',$result)
        $testSummary+=Assert-MetadataTestResult $result $platform $projectRoot
    }
    }
} finally {
    Restore-MetadataTestIdentity $testIdentity
    # Preserve diagnostics even when an importer or a test fails, before runner
    # cleanup removes this isolated checkout. Never reset differences to fake PASS.
    & $gitExe -C $sourceRoot status --porcelain --untracked-files=no | Set-Content -LiteralPath (Join-Path $evidenceRoot 'tracked-state.txt')
    & $gitExe -C $sourceRoot diff --no-ext-diff --binary HEAD -- . | Set-Content -LiteralPath (Join-Path $evidenceRoot 'tracked-changes.patch') -Encoding UTF8
}
$status=& $gitExe -C $sourceRoot status --porcelain --untracked-files=no
if($LASTEXITCODE -ne 0){ throw 'Post-import tracked state unavailable' }
$status | Set-Content -LiteralPath (Join-Path $evidenceRoot 'tracked-state.txt')
if(@($status).Count -gt 0){ throw 'Unity changed tracked files in the isolated candidate; inspect evidence' }
$certificateState=if($validationMode -eq 'full'){'PASS'}else{'IDENTITIES_PASS_ONLY'}
@{state=$certificateState;validation_mode=$validationMode;full_tests_run=($validationMode -eq 'full');canonical_runtime_prepared=$runtimePrepared;test_summary=$testSummary;player_build_run=$false;candidate_sha=$checked;run_id=$env:GITHUB_RUN_ID;unity_version=$proof.unity_version;restored_entries=@($proof.entries).Count;production_projects_modified=$false;harness_editor_only=$true;test_persistence_isolated=$true;original_project_settings_restored=$true} |
    ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $evidenceRoot 'certificate.json')
