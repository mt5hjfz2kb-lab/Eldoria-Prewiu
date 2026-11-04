function Set-MetadataTestIdentity([string]$settingsPath,[string]$runId) {
    if($runId -notmatch '^\d+$'){ throw 'Invalid native test run identity' }
    $original=[IO.File]::ReadAllBytes($settingsPath)
    $text=[Text.Encoding]::UTF8.GetString($original)
    foreach($key in @('companyName','productName')){
        if([regex]::Matches($text,('(?m)^  '+$key+':[^\r\n]*')).Count -ne 1){
            throw ('Expected exactly one settings key: '+$key)
        }
    }
    $company='MetadataValidation_'+$runId
    $product='EldoriaMetadata_'+$runId
    $text=[regex]::Replace($text,'(?m)^  companyName:[^\r\n]*',('  companyName: '+$company))
    $text=[regex]::Replace($text,'(?m)^  productName:[^\r\n]*',('  productName: '+$product))
    [IO.File]::WriteAllText($settingsPath,$text,(New-Object Text.UTF8Encoding($false)))
    return [pscustomobject]@{Path=$settingsPath;OriginalBytes=$original;Company=$company;Product=$product}
}

function Restore-MetadataTestIdentity($identity) {
    [IO.File]::WriteAllBytes($identity.Path,$identity.OriginalBytes)
}
