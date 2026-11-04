$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'native_settings.ps1')
$root=Join-Path ([IO.Path]::GetTempPath()) ('metadata-identity-test-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $root | Out-Null
try {
    foreach($newline in @("`n","`r`n")){
        $path=Join-Path $root 'ProjectSettings.asset'
        $text=(@('PlayerSettings:','  companyName: DefaultCompany','  productName: Unity','  keepThis: unchanged') -join $newline)+$newline
        $original=[Text.Encoding]::UTF8.GetBytes($text)
        [IO.File]::WriteAllBytes($path,$original)
        $identity=Set-MetadataTestIdentity $path '123456'
        $changed=[IO.File]::ReadAllText($path)
        if(-not $changed.Contains('companyName: MetadataValidation_123456')){throw 'Company identity not isolated'}
        if(-not $changed.Contains('productName: EldoriaMetadata_123456')){throw 'Product identity not isolated'}
        if(-not $changed.Contains('keepThis: unchanged')){throw 'Unrelated setting changed'}
        Restore-MetadataTestIdentity $identity
        if([Convert]::ToBase64String([IO.File]::ReadAllBytes($path)) -ne [Convert]::ToBase64String($original)){throw 'Original settings bytes not restored'}
    }
    $rejected=$false
    try {Set-MetadataTestIdentity $path '../unsafe'} catch {$rejected=$true}
    if(-not $rejected){throw 'Unsafe run identity accepted'}
    [IO.File]::WriteAllText($path,'  productName: Unity')
    $before=[IO.File]::ReadAllBytes($path)
    $rejected=$false
    try {Set-MetadataTestIdentity $path '123'} catch {$rejected=$true}
    if(-not $rejected){throw 'Ambiguous/incomplete settings accepted'}
    if([Convert]::ToBase64String([IO.File]::ReadAllBytes($path)) -ne [Convert]::ToBase64String($before)){throw 'Rejected settings were changed'}
    Write-Host 'METADATA_TEST_IDENTITY_PASS LF/CRLF isolation, exact restoration, invalid input rejection'
} finally {Remove-Item -LiteralPath $root -Recurse -Force}
