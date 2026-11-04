$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'native_tracked_state.ps1')
$stateScratch=Join-Path ([IO.Path]::GetTempPath()) ('metadata-state-'+[Guid]::NewGuid().ToString('N'))
$gitCommand=(Get-Command git).Source
$script:stateRejections=0
function Reject-State([scriptblock]$Action){
    $rejected=$false
    try {& $Action | Out-Null}catch{$rejected=$true}
    if(-not $rejected){throw 'Invalid tracked state was accepted'}
    $script:stateRejections++
}
function Write-StateFile([string]$Name,[string]$Content){[IO.File]::WriteAllText((Join-Path $stateScratch $Name),$Content,[Text.UTF8Encoding]::new($false))}
try {
    New-Item -ItemType Directory -Path (Join-Path $stateScratch 'Unity') | Out-Null
    & $gitCommand -C $stateScratch init -q
    & $gitCommand -C $stateScratch config core.autocrlf false
    Write-StateFile '.gitattributes' "*.meta text eol=lf`n"
    Write-StateFile 'Unity/known.meta' "original`n"
    Write-StateFile 'Unity/recovered.meta' "preserved`n"
    Write-StateFile 'Unity/game.cs' "source`n"
    & $gitCommand -C $stateScratch add .
    & $gitCommand -C $stateScratch -c user.name=MetadataTest -c user.email=metadata@example.invalid commit -qm fixture
    if($LASTEXITCODE -ne 0){throw 'Fixture Git commit failed'}
    $source=(& $gitCommand -C $stateScratch rev-parse HEAD:Unity/known.meta).Trim()
    Write-StateFile 'Unity/known.meta' "normalized`n"
    $observed=(& $gitCommand -C $stateScratch hash-object --path=Unity/known.meta -- Unity/known.meta).Trim()
    $policy=@{entries=@(@{path='Unity/known.meta';source_blob=$source;observed_blob=$observed})}
    $policyPath=Join-Path $stateScratch 'policy.json'
    $recoveryPath=Join-Path $stateScratch 'recovery.json'
    $policy | ConvertTo-Json -Depth 5 | Set-Content $policyPath
    @{entries=@(@{path='Unity/recovered.meta'})} | ConvertTo-Json -Depth 5 | Set-Content $recoveryPath
    $review=Assert-MetadataTrackedState $stateScratch $gitCommand $policyPath $recoveryPath
    if($review.known_baseline_serializations -ne 1 -or $review.pristine_checkout){throw 'Exact observed serialization not recognized'}
    Write-StateFile 'Unity/known.meta' "normalized`r`n"
    if((Assert-MetadataTrackedState $stateScratch $gitCommand $policyPath $recoveryPath).known_baseline_serializations -ne 1){throw 'Git filter equivalence lost'}
    Write-StateFile 'Unity/game.cs' "changed`n"
    Reject-State {Assert-MetadataTrackedState $stateScratch $gitCommand $policyPath $recoveryPath}
    Write-StateFile 'Unity/game.cs' "source`n"
    Write-StateFile 'Unity/known.meta' "unexpected`n"
    Reject-State {Assert-MetadataTrackedState $stateScratch $gitCommand $policyPath $recoveryPath}
    Write-StateFile 'Unity/known.meta' "normalized`n"
    $policy.entries[0].source_blob='0000000000000000000000000000000000000000'
    $policy | ConvertTo-Json -Depth 5 | Set-Content $policyPath
    Reject-State {Assert-MetadataTrackedState $stateScratch $gitCommand $policyPath $recoveryPath}
    $policy.entries[0].source_blob=$source
    $policy.entries[0].path='Unity/recovered.meta'
    $policy | ConvertTo-Json -Depth 5 | Set-Content $policyPath
    Reject-State {Assert-MetadataTrackedState $stateScratch $gitCommand $policyPath $recoveryPath}
    $policy.entries[0].path='Unity/known.meta'
    $policy | ConvertTo-Json -Depth 5 | Set-Content $policyPath
    Write-StateFile 'Unity/known.meta' "original`n"
    $clean=Assert-MetadataTrackedState $stateScratch $gitCommand $policyPath $recoveryPath
    if(-not $clean.pristine_checkout -or $clean.known_baseline_serializations -ne 0){throw 'Clean tracked state not recognized'}
    if($script:stateRejections -ne 4){throw 'Missing rejection coverage'}
    $demo='Unity/Assets/NatureStarterKit2/Scene/Demo.unity'
    $lighting='Unity/Assets/NatureStarterKit2/Scene/DemoSettings.lighting'
    New-Item -ItemType Directory -Path (Split-Path (Join-Path $stateScratch $demo)) -Force | Out-Null
    Write-StateFile $demo "old demo`n"
    & $gitCommand -C $stateScratch add -- $demo
    & $gitCommand -C $stateScratch -c user.name=MetadataTest -c user.email=metadata@example.invalid commit -qm demo
    $demoSource=(& $gitCommand -C $stateScratch rev-parse "HEAD:$demo").Trim()
    $scene="  m_LightingSettings: {fileID: 4890085278179872738, guid: $('a'*32), type: 2}`n"
    $normalized=$scene.Replace(('a'*32),('0'*32))
    $hash=[Security.Cryptography.SHA256]::Create()
    $normalizedHash=([BitConverter]::ToString($hash.ComputeHash([Text.Encoding]::UTF8.GetBytes($normalized)))).Replace('-','').ToLowerInvariant()
    $hash.Dispose()
    $policy.entries+=@{path=$demo;source_blob=$demoSource;observed_blob=('1'*40);generated_lighting_reference=@{path=$lighting;normalized_scene_sha256=$normalizedHash}}
    $policy | ConvertTo-Json -Depth 6 | Set-Content $policyPath
    Write-StateFile $demo $scene
    Write-StateFile $lighting "LightingSettings:`n  serializedVersion: 6`n"
    Write-StateFile ($lighting+'.meta') "guid: $('a'*32)`n"
    Assert-MetadataTrackedState $stateScratch $gitCommand $policyPath $recoveryPath | Out-Null
    Write-StateFile $demo ($scene.Replace(('a'*32),('b'*32)))
    Write-StateFile ($lighting+'.meta') "guid: $('b'*32)`n"
    Assert-MetadataTrackedState $stateScratch $gitCommand $policyPath $recoveryPath | Out-Null
    Write-StateFile ($lighting+'.meta') "guid: $('c'*32)`n"
    Reject-State {Assert-MetadataTrackedState $stateScratch $gitCommand $policyPath $recoveryPath}
    Write-StateFile ($lighting+'.meta') "guid: $('b'*32)`n"
    Write-StateFile $demo (($scene.Replace(('a'*32),('b'*32)))+"unexpected: 1`n")
    Reject-State {Assert-MetadataTrackedState $stateScratch $gitCommand $policyPath $recoveryPath}
    Write-Host 'Tracked state contract PASS: exact baseline blobs/CRLF, clean state, unknown/source/post-change/recovery rejection'
} finally {if(Test-Path $stateScratch){Remove-Item -LiteralPath $stateScratch -Recurse -Force}}
