$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$headers=@{Authorization="Bearer $env:GH_TOKEN";Accept='application/vnd.github+json'}
$api="https://api.github.com/repos/$env:GITHUB_REPOSITORY"
$head=Invoke-RestMethod -Uri "$api/git/ref/heads/art/visual-reboot" -Headers $headers
if($head.object.sha -ne $env:GITHUB_SHA){throw 'Stale source commit'}
$file=Invoke-RestMethod -Uri "$api/contents/pipeline/active-workstreams.json?ref=main" -Headers $headers
if($file.encoding -ne 'base64'){
 $expected=$file.sha
 $file=Invoke-RestMethod -Uri "$api/git/blobs/$expected" -Headers $headers
 if($file.sha -ne $expected){throw 'Registry blob identity mismatch'}
}
if($file.encoding -ne 'base64'){throw 'Unsupported registry encoding'}
$registry=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($file.content)) | ConvertFrom-Json
$holders=@($registry.active | Where-Object {$_.resources -contains 'windows-runner-heavy'})
if($holders.Count -ne 1){throw 'No sole Windows holder'}
$w=$holders[0]
if($w.id -ne 'eldoria-art-visual-reboot-phase1-20261115' -or
 $w.owner -ne 'chatgpt-visual-reboot-recovery-55321388b3c8' -or
 $w.branch -ne 'art/visual-reboot' -or $w.ownership_state -ne 'claimed' -or $w.status -ne 'active'){
 throw 'Windows reservation belongs to another owner/task'
}
if($w.native_source_sha -ne $env:GITHUB_SHA){throw 'Lease does not pin this source commit'}
Write-Output "REBOOT_LIVE_EXACT_SOURCE_LEASE_PASS $env:GITHUB_SHA $($w.ownership_revision)"
