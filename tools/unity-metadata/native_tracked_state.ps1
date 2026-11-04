function Assert-MetadataTrackedState([string]$RepositoryRoot,[string]$GitExecutable,[string]$PolicyPath,[string]$RecoveryManifestPath) {
    $policy=Get-Content -LiteralPath $PolicyPath -Raw | ConvertFrom-Json
    $recovered=@((Get-Content -LiteralPath $RecoveryManifestPath -Raw | ConvertFrom-Json).entries | ForEach-Object {[string]$_.path})
    $allowed=@{}
    foreach($entry in $policy.entries){
        $path=[string]$entry.path
        if(-not $path.StartsWith('Unity/') -or $path.Contains('..') -or $allowed.ContainsKey($path) -or $recovered -contains $path){throw 'Invalid or recovery-overlapping serialization exception'}
        if($entry.source_blob -notmatch '^[0-9a-f]{40}$' -or $entry.observed_blob -notmatch '^[0-9a-f]{40}$'){throw 'Full exact serialization blob identities required'}
        $allowed[$path]=$entry
    }
    $changed=@(& $GitExecutable -C $RepositoryRoot -c core.quotepath=false diff --name-only --no-renames HEAD --)
    if($LASTEXITCODE -ne 0){throw 'Cannot inspect tracked state'}
    $reviewed=@()
    foreach($path in $changed){
        if(-not $allowed.ContainsKey($path)){throw "Unexpected tracked change, not a known baseline serialization: $path"}
        $entry=$allowed[$path]
        $before=(& $GitExecutable -C $RepositoryRoot rev-parse "HEAD:$path").Trim()
        if($LASTEXITCODE -ne 0 -or $before -ne $entry.source_blob){throw "Baseline source changed: $path"}
        $after=(& $GitExecutable -C $RepositoryRoot hash-object --path=$path -- $path).Trim()
        if($LASTEXITCODE -ne 0){throw "Cannot hash serialization result: $path"}
        if($entry.generated_lighting_reference){
            if($path -ne 'Unity/Assets/NatureStarterKit2/Scene/Demo.unity'){throw 'Generated lighting rule outside exact vendor demo'}
            $scene=[IO.File]::ReadAllText((Join-Path $RepositoryRoot $path)).Replace("`r`n","`n")
            $pattern='(?m)^(  m_LightingSettings: \{fileID: 4890085278179872738, guid: )([0-9a-f]{32})(, type: 2\})$'
            $matches=[regex]::Matches($scene,$pattern)
            if($matches.Count -ne 1){throw 'Expected one exact generated LightingSettings reference'}
            $lighting=Join-Path $RepositoryRoot $entry.generated_lighting_reference.path
            if(-not(Test-Path -LiteralPath $lighting -PathType Leaf)){throw 'Generated lighting asset missing'}
            $meta=[IO.File]::ReadAllText($lighting+'.meta')
            if($meta -notmatch ('(?m)^guid: '+$matches[0].Groups[2].Value+'\s*$')){throw 'Generated lighting GUID/reference mismatch'}
            if([IO.File]::ReadAllText($lighting) -notmatch '(?m)^LightingSettings:'){throw 'Generated asset is not LightingSettings'}
            $normalized=[regex]::Replace($scene,$pattern,('${1}' + ('0'*32) + '${3}'))
            $sha=[Security.Cryptography.SHA256]::Create()
            try{$normalizedHash=([BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($normalized)))).Replace('-','').ToLowerInvariant()}finally{$sha.Dispose()}
            if($normalizedHash -ne $entry.generated_lighting_reference.normalized_scene_sha256){throw 'Vendor scene changed beyond its generated lighting GUID'}
        } elseif($after -ne $entry.observed_blob){throw "Unrecognized serialization result: $path"}
        $reviewed+=@{path=$path;source_blob=$before;observed_blob=$after}
    }
    return @{state='PASS';pristine_checkout=($changed.Count -eq 0);known_baseline_serializations=$reviewed.Count;unexpected_changes=0;recovered_entries_exempted=0;reviewed=$reviewed}
}
