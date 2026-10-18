$ErrorActionPreference='Stop'
$repo='mt5hjfz2kb-lab/Eldoria-Prewiu'
$evidence='docs/evidence/valoria-reference-clean'
$request=Get-Content 'tools/valoria-reference-clean/request.json' -Raw|ConvertFrom-Json
if(-not $request.content_updates -or -not $request.publish_content){throw 'Explicit content publication request missing'}
if(-not $env:GITHUB_TOKEN){throw 'Configured Actions token unavailable; no alternative credential extraction'}
$manifestPath='Unity/Builds/ValoriaContent/android/content-channel.json'
$manifest=Get-Content $manifestPath -Raw|ConvertFrom-Json
$windows=Get-Content "$evidence/content93-policy.json" -Raw|ConvertFrom-Json
$native=Get-Content "$evidence/content93-result.json" -Raw|ConvertFrom-Json
$gameplay=Get-Content "$evidence/gameplay78-playmode.json" -Raw|ConvertFrom-Json
$identity=Get-Content "$evidence/android-update-verification.json" -Raw|ConvertFrom-Json
if($native.status -ne 'NATIVE_WINDOWS_SCENE_BUNDLE_PLAY_PASS' -or -not $native.canonical_saves_untouched){throw 'Actual scene-bundle validation missing'}
if($gameplay.status -ne 'EXISTING_UI_RULES_SAVE_I_II_PASS_CHAPTER_II_BUILD_TRAIN_MARCH_VICTORY_VERIFIED' -or -not $gameplay.canonical_save_untouched){throw 'Full real UI/gameplay/save proof missing'}
foreach($case in @('download','cached_offline','corrupt','incompatible')){
  $proof=Get-Content "$evidence/content-loader-$case.json" -Raw|ConvertFrom-Json
  if($proof.status -ne 'PASS' -or -not $proof.canonical_saves_untouched){throw "Content loader case not verified: $case"}
}
if($manifest.runtimeContract -ne $windows.runtimeContract -or $manifest.platform -ne 'android' -or $manifest.unityVersion -ne '6000.3.23f1' -or $manifest.revision -ne $request.revision){throw 'Tested runtime/platform/version mismatch'}
if($manifest.sceneSourceSha256 -ne $windows.sceneSourceSha256 -or $manifest.sceneSourceSha256 -ne (Get-FileHash 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Valoria_Reference_Clean.unity' -Algorithm SHA256).Hash.ToLowerInvariant()){throw 'Tested scene source changed'}
if($identity.source_sha -ne $env:GITHUB_SHA -or -not $identity.cryptographic_signature_verified -or $identity.package -ne 'com.eldoria.valoria.v5test' -or $identity.version_code -lt 6){throw 'Exact bootstrap APK identity gate missing'}
$tag="valoria-content-$($request.revision)"
$name="valoria-android-$($manifest.sha256).bundle"
$file="Unity/Builds/ValoriaContent/android/$name"
$url="https://github.com/$repo/releases/download/$tag/$name"
if($manifest.url -ne $url -or (Get-Item $file).Length -ne $manifest.bytes -or (Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $manifest.sha256){throw 'Exact content hash/size/URL mismatch'}
$api="https://api.github.com/repos/$repo"
$headers=@{Authorization="Bearer $env:GITHUB_TOKEN";Accept='application/vnd.github+json';'X-GitHub-Api-Version'='2022-11-28';'User-Agent'='Eldoria-verified-content'}
$release=$null
try{$release=Invoke-RestMethod "$api/releases/tags/$tag" -Headers $headers}catch{if([int]$_.Exception.Response.StatusCode -ne 404){throw}}
if(-not $release){
  $body=@{tag_name=$tag;target_commitish=$env:GITHUB_SHA;name="Valoria content $($request.revision)";draft=$true;prerelease=$true;body='Verified scene-only Android content. No downloaded assemblies, signing key changes or save resets. Requires compatible Eldoria bootstrap APK. Physical phone performance not certified.'}|ConvertTo-Json
  $release=Invoke-RestMethod "$api/releases" -Headers $headers -Method Post -ContentType 'application/json' -Body $body
}
if($release.target_commitish -ne $env:GITHUB_SHA){throw 'Release tag belongs to another candidate; refuse overwrite'}
foreach($item in @(@{path=$file;name=$name;type='application/octet-stream'},@{path=$manifestPath;name='content-channel.json';type='application/json'})){
  $sha=(Get-FileHash $item.path -Algorithm SHA256).Hash.ToLowerInvariant()
  $existing=@($release.assets|Where-Object {$_.name -eq $item.name})
  if($existing.Count){if($existing[0].digest -ne "sha256:$sha"){throw 'Immutable release asset already exists with a different or unknown digest'};continue}
  if(-not $release.draft){throw 'Published release is immutable; no additional asset writes'}
  $upload="https://uploads.github.com/repos/$repo/releases/$($release.id)/assets?name=$([Uri]::EscapeDataString($item.name))"
  $asset=Invoke-RestMethod $upload -Headers $headers -Method Post -ContentType $item.type -InFile $item.path
  if($asset.size -ne (Get-Item $item.path).Length){throw 'Uploaded content asset size differs'}
}
if($release.draft){$null=Invoke-RestMethod "$api/releases/$($release.id)" -Headers $headers -Method Patch -ContentType 'application/json' -Body '{"draft":false}'}
# Public HTTPS byte verification before the stable channel is advanced.
$download=Join-Path $env:RUNNER_TEMP "valoria-public-content-$($request.revision).bundle"
Invoke-WebRequest $url -OutFile $download -UseBasicParsing
if((Get-Item $download).Length -ne $manifest.bytes -or (Get-FileHash $download -Algorithm SHA256).Hash.ToLowerInvariant() -ne $manifest.sha256){throw 'Public content bytes differ; channel left unchanged'}
$public=Invoke-RestMethod "https://github.com/$repo/releases/download/$tag/content-channel.json" -Headers @{'Cache-Control'='no-cache'}
if($public.sha256 -ne $manifest.sha256 -or $public.runtimeContract -ne $manifest.runtimeContract){throw 'Public manifest mismatch; channel left unchanged'}
$channel=(Resolve-Path 'tools/valoria-reference-clean').Path+'\content-channel.json'
[IO.File]::WriteAllText($channel,($manifest|ConvertTo-Json -Depth 8),(New-Object Text.UTF8Encoding($false)))
@{status='PUBLIC_HTTPS_BYTES_VERIFIED_CHANNEL_STAGED';source_sha=$env:GITHUB_SHA;revision=$manifest.revision;runtime_contract=$manifest.runtimeContract;bytes=$manifest.bytes;sha256=$manifest.sha256;url=$url;phone_tested=$false;save_reset=$false}|ConvertTo-Json|Set-Content "$evidence/content-publication.json" -Encoding utf8
