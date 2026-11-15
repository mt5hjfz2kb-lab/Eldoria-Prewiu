$ErrorActionPreference='Stop'
$env:GH_TOKEN='fixture-not-a-token'
$env:GITHUB_REPOSITORY='fixture/repository'
$env:GITHUB_SHA=('a'*40)
function Invoke-RestMethod {
 param($Uri,$Headers)
 if($Uri -like '*/git/ref/heads/art/visual-reboot'){return @{object=@{sha=$script:head}}}
 if($Uri -like '*/contents/pipeline/active-workstreams.json?ref=main'){
  $json=@{active=$script:entries}|ConvertTo-Json -Depth 10 -Compress
  return @{encoding='base64';content=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($json))}
 }
 throw "Unexpected fixture URL: $Uri"
}
$passed=0
foreach($case in @('valid','foreign_owner','foreign_task','two_holders','no_holder','stale_head','wrong_pin','inactive')){
 $script:head=$env:GITHUB_SHA
 $w=[pscustomobject]@{id='eldoria-art-visual-reboot-phase1-20261115';owner='chatgpt-visual-reboot-recovery-55321388b3c8';
 branch='art/visual-reboot';ownership_state='claimed';status='active';resources=@('windows-runner-heavy');native_source_sha=$env:GITHUB_SHA;ownership_revision=2}
 $script:entries=@($w)
 switch($case){
  'foreign_owner'{$w.owner='another-chat'}
  'foreign_task'{$w.id='another-task'}
  'two_holders'{$script:entries=@($w,$w)}
  'no_holder'{$w.resources=@()}
  'stale_head'{$script:head=('b'*40)}
  'wrong_pin'{$w.native_source_sha=('b'*40)}
  'inactive'{$w.status='complete'}
 }
 $rejected=$false
 try {& "$PSScriptRoot/check_lease.ps1" | Out-Null} catch {$rejected=$true}
 if(($case -eq 'valid' -and $rejected) -or ($case -ne 'valid' -and -not $rejected)){
  throw "Incorrect lease decision for $case"
 }
 $passed++
}
Write-Output "POWERSHELL_LEASE_TESTS_PASS $passed/8"
