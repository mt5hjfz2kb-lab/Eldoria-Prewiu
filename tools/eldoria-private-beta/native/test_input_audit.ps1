param([string]$Runner=(Join-Path $PSScriptRoot 'run_warm_structural.ps1'))
$ErrorActionPreference='Stop'
$source=Get-Content $Runner -Raw
$tokens=$null;$errors=$null
[System.Management.Automation.Language.Parser]::ParseInput($source,[ref]$tokens,[ref]$errors)|Out-Null
if($errors.Count){throw 'Runner syntax invalid'}
$boundary='$env:ELDORIA_BASELINE_INPUT_AUDIT='
$end=$source.IndexOf($boundary)
if($end -lt 0){throw 'Input validation boundary missing'}
$validate=[scriptblock]::Create($source.Substring(0,$end))
$root=Join-Path ([IO.Path]::GetTempPath()) ('beta-input-audit-test-'+[guid]::NewGuid())
New-Item -ItemType Directory -Path $root|Out-Null
$auditPath=Join-Path $root 'audit.json'
$script:status=@();$script:paths=@();$script:head='948ffa9837168a0b68dab80be76b66350422b23a'
function git {
 $global:LASTEXITCODE=0
 if($args -contains 'rev-parse'){return $script:head}
 if($args -contains 'status'){return $script:status}
 if($args -contains 'ls-files'){return $script:paths}
 throw 'Unexpected Git invocation'
}
function Check([string]$Name,$Audit,[string]$ExpectedError=''){
 $Audit|ConvertTo-Json -Depth 5|Set-Content $auditPath
 $actual=''
 try{& $validate -Source $root -Tools $root -Evidence $root -WebOutput (Join-Path $root 'web') -InputAuditPath $auditPath}catch{$actual=$_.Exception.Message}
 if($ExpectedError){if($actual -notlike "*$ExpectedError*"){throw "$Name wrong rejection: $actual"}}
 elseif($actual){throw "$Name unexpectedly rejected: $actual"}
 Write-Output "PASS $Name"
}
try{
 $audit=@{base=$script:head;status=@();files=@()}
 Check 'clean clone accepts empty exact audit' $audit
 $dirty=@{base=$script:head;status=@(' M Unity/a');files=@(@{path='Unity/a';sha256='wrong'})}
 Check 'clean clone rejects legacy dirty audit' $dirty 'Pending source differs'
 $script:status=@('?? Unity/unexpected')
 Check 'clean audit rejects unexpected source mutation' $audit 'Pending source differs'
 $script:status=@(' M Unity/a');$script:paths=@('Unity/a')
 New-Item -ItemType Directory (Join-Path $root 'Unity')|Out-Null
 Set-Content (Join-Path $root 'Unity/a') 'reviewed input'
 Check 'dirty audit rejects wrong hash' $dirty 'Reviewed existing input changed'
 $dirty.files[0].sha256=(Get-FileHash (Join-Path $root 'Unity/a')).Hash
 Check 'legacy reviewed input still accepted' $dirty
 $script:paths=@('Unity/b')
 Check 'same count different membership rejected' $dirty 'Pending input membership changed'
 $script:head='wrong'
 Check 'wrong base rejected' $audit 'exact frozen commit'
}finally{Remove-Item $root -Recurse -Force}
