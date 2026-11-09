param([Parameter(Mandatory=$true)][string]$Source)
$ErrorActionPreference='Stop'
$root=Join-Path $Source 'Unity/Assets/Eldoria/Scripts'
$files=@(Get-ChildItem "$root/Domain/*.cs")+@(Get-ChildItem "$root/Application/*.cs")
$ui=Join-Path $Source 'Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV1'
Add-Type -Path @($files.FullName + @("$ui/Core/RoyalContracts.cs","$ui/Bindings/ExistingSliceBinding.cs","$ui/Bindings/BastionTiming.cs",(Join-Path $PSScriptRoot 'PretestDomainChecks.cs')))
[PretestDomainChecks]::Run()
