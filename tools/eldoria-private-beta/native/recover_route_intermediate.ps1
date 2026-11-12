function Repair-PR179RecordedRouteIntermediate {
 param([string]$Source,[object]$Audit,[string]$Evidence,[string]$ExpectedHead,[string]$KnownIntermediate)
 $ErrorActionPreference='Stop'
 # Resolve BEFORE git -C changes its own input path base. Production tools
 # are passed relative to the Actions workspace, not to the warm project.
 $KnownIntermediate=(Resolve-Path -LiteralPath $KnownIntermediate).Path
 $path='Unity/Assets/Eldoria/World4XCanon/RegionOneTerrainRoutes.cs'
 if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Native tools active: refuse route recovery'}
 if($ExpectedHead -notmatch '^[a-f0-9]{40}$' -or (git -C $Source rev-parse HEAD).Trim() -ne $ExpectedHead -or $LASTEXITCODE){throw 'Route recovery source identity differs'}
 foreach($item in $Audit.files){
  if(-not(Test-Path -LiteralPath (Join-Path $Source $item.path) -PathType Leaf) -or
     (Get-FileHash -LiteralPath (Join-Path $Source $item.path) -Algorithm SHA256).Hash -ne $item.sha256){throw 'Original dirty input differs; no route restoration'}
 }
 $current=@(git -C $Source status --porcelain -- Unity)
 if($LASTEXITCODE){throw 'Cannot inspect route delta'}
 $delta=@(Compare-Object @($Audit.status) $current)
 if(-not $delta.Count){return 0}
 if($delta.Count -ne 1 -or $delta[0].SideIndicator -ne '=>' -or
    [string]$delta[0].InputObject -cne (' M '+$path) -or @($Audit.files|Where-Object {$_.path -ceq $path}).Count){throw 'Route recovery refuses any unrecorded, staged or mixed change'}
 $frozen=(& git -C $Source rev-parse "${ExpectedHead}:$path").Trim()
 if($LASTEXITCODE){throw 'Frozen route source unavailable'}
 $index=(& git -C $Source rev-parse ":$path").Trim()
 if($LASTEXITCODE -or $index -ne $frozen){throw 'Route index differs'}
 # Compare exact Git-normalized source bytes: Windows CRLF is permitted,
 # arbitrary edits or a new current runtime route are never accepted.
 $known=(& git -C $Source hash-object "--path=$path" $KnownIntermediate).Trim()
 if($LASTEXITCODE){throw 'Recorded comparison source unavailable'}
 $actual=(& git -C $Source hash-object "--path=$path" $path).Trim()
 if($LASTEXITCODE -or $actual -ne $known){throw 'Route differs from exact recorded failed comparison intermediate'}
 $saved=Join-Path $Evidence 'recorded-route-before-restore.cs.txt'
 if(Test-Path -LiteralPath $saved){throw 'Refuse replacing route recovery evidence'}
 Copy-Item -LiteralPath (Join-Path $Source $path) -Destination $saved
 & git -C $Source restore --source=$ExpectedHead --worktree -- $path
 if($LASTEXITCODE){throw 'Targeted route restoration failed'}
 $restored=(& git -C $Source hash-object "--path=$path" $path).Trim()
 if($LASTEXITCODE -or $restored -ne $frozen){throw 'Restored route Git blob mismatch'}
 $after=@(git -C $Source status --porcelain -- Unity)
 if($LASTEXITCODE -or @(Compare-Object @($Audit.status) $after).Count){throw 'Route recovery did not restore original status'}
 foreach($item in $Audit.files){if((Get-FileHash -LiteralPath (Join-Path $Source $item.path) -Algorithm SHA256).Hash -ne $item.sha256){throw 'Original dirty bytes changed during route recovery'}}
 @{expected_head=$ExpectedHead;path=$path;recorded_intermediate_blob=$known;restored_blob=$restored;original_dirty_hashes_preserved=$true;before_sha256=(Get-FileHash $saved -Algorithm SHA256).Hash}|
  ConvertTo-Json|Set-Content -LiteralPath (Join-Path $Evidence 'recorded-route-restoration.json') -Encoding UTF8
 Write-Host 'PR179_RECORDED_ROUTE_INTERMEDIATE_RESTORED_EXACT_BASE_AND_DIRTY_BYTES_PRESERVED'
 return 1
}
