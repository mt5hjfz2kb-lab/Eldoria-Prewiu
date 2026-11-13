param(
 [Parameter(Mandatory=$true)][string]$UxSource,
 [Parameter(Mandatory=$true)][string]$OutputPath
)
# Compose on the EXACT archived 19-overlay beta10 control, not on a later
# 727-line UX development copy. Three reversible changes, never two host copies.
$ErrorActionPreference='Stop'
$expected='9866EB11239F2A0C1ECD1083EBA122D0E1724C20CAA825A7380AFEB45DFCB426'
if(-not(Test-Path $UxSource -PathType Leaf)){throw 'Frozen UX control absent'}
if((Get-FileHash $UxSource -Algorithm SHA256).Hash -ne $expected){throw 'Frozen beta10 UX source hash changed'}
$source=[IO.File]::ReadAllText((Resolve-Path $UxSource).Path,[Text.Encoding]::UTF8)
$nl="`n"
if($source.Contains("`r")){throw 'Frozen beta10 UX unexpected CR/newline'}
function Replace-Once([string]$From,[string]$To){
 if([regex]::Matches($script:result,[regex]::Escape($From)).Count -ne 1){
  throw "P0 cannot uniquely compose: $($From.Substring(0,[Math]::Min($From.Length,55)))"
 }
 $script:result=$script:result.Replace($From,$To)
}
$anchor='  // All existing navigation/selection callers keep one readiness contract:'+$nl+
        '  // the map must be complete before any world camera or target can be exposed.'+$nl
$property=@(
 '  // Owner-only WebGL opt-in: defer large World initialization for a city A/B.'
 '  // Default, native Editor and other routes keep canonical world behavior.'
 '  public static bool P0CityOnlyProbe {'
 '   get {'
 '#if UNITY_WEBGL && !UNITY_EDITOR'
 '    return UnityEngine.Application.absoluteURL.Contains("eldoria_p0_city_only=1");'
 '#else'
 '    return false;'
 '#endif'
 '   }'
 '  }'
) -join $nl
$property+=$nl
$oldReady='  public bool Ready=>presentationBindingsReady&&(!worldPresentation||worldPresentation.IsReady);'
$newReady='  public bool Ready=>presentationBindingsReady&&(!worldPresentation||worldPresentation.IsReady||P0CityOnlyProbe);'
$worldAnchor='  public bool TrySetWorld(bool open){'+$nl+'   if(!Ready)return false;'+$nl
$guard=@(
 '   if(open&&P0CityOnlyProbe){'
 '    if(Hud)Hud.Feedback.Put("Diagnostico P0: mundo diferido; la partida sigue intacta.");'
 '    return false;'
 '   }'
) -join $nl
$guard+=$nl
$result=$source
Replace-Once $anchor ($anchor+$property)
Replace-Once $oldReady $newReady
Replace-Once $worldAnchor ($worldAnchor+$guard)
$back=$result.Replace($property,'').Replace($newReady,$oldReady).Replace($guard,'')
if($back -cne $source){throw 'City-only change is NOT exactly reversible to beta10 UX'}
if(([regex]::Matches($result,'P0CityOnlyProbe')).Count -ne 3){throw 'P0 flag was not used exactly three times'}
$out=[IO.Path]::GetFullPath($OutputPath)
$parent=Split-Path $out -Parent
if(-not(Test-Path $parent)){New-Item -ItemType Directory -Force $parent|Out-Null}
if(Test-Path $out){throw 'Do not overwrite a previously composed P0 artifact'}
[IO.File]::WriteAllText($out,$result,(New-Object Text.UTF8Encoding($false)))
if((Get-FileHash $out -Algorithm SHA256).Hash -eq $expected){throw 'P0 candidate was not changed'}
Write-Host ('P0_CITY_ONLY_COMPOSITION_EXACT_3_CHANGES PASS frozenSHA='+$expected+' candidateSHA='+((Get-FileHash $out -Algorithm SHA256).Hash))
