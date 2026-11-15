$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
# Exercise the data shape used by native-qa.ps1, not only syntax parsing.
$hashes=@()
foreach($level in 1..5){
    $hashes+=@{file=("Ashen_Bastion_"+$level+"_390x844.png");sha256=("PORTRAIT"+$level);size_bytes=11000}
    $hashes+=@{file=("Ashen_Bastion_"+$level+"_1600x900.png");sha256=("LANDSCAPE"+$level);size_bytes=11000}
}
$portraitHashes=@($hashes | Where-Object {$_['file'] -like '*390x844.png'} |
    ForEach-Object {$_['sha256']} | Sort-Object -Unique)
if($portraitHashes.Count -ne 5){throw "Distinct portrait case should be five"}
$hashes[0]['sha256']=$hashes[2]['sha256']
$duplicate=@($hashes | Where-Object {$_['file'] -like '*390x844.png'} |
    ForEach-Object {$_['sha256']} | Sort-Object -Unique)
if($duplicate.Count -ne 4){throw "Duplicate portrait case must fail strict check"}
Write-Output 'ASHEN_POWERSHELL5_HASH_TABLE_PROOF_PASS'
