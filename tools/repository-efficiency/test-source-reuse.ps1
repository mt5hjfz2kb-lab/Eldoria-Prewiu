$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot '../../docs/evidence/world4x-canon-20261022/authoring/reuse_editable_source.ps1')
$originalLocation=Get-Location
$temp=Join-Path ([IO.Path]::GetTempPath()) ('eldoria-source-reuse-'+[Guid]::NewGuid().ToString('N'))
function Assert($ok,$message){if(-not $ok){throw $message}}
function Reject($action,$message){$rejected=$false;try{& $action | Out-Null}catch{$rejected=$true};Assert $rejected $message}
function Git-Ok { & git @args | Out-Null; if($LASTEXITCODE){throw "git failed: $args"} }
try {
 New-Item -ItemType Directory $temp | Out-Null
 Set-Location $temp
 Git-Ok init -q
 Git-Ok config user.name Fixture
 Git-Ok config user.email fixture@example.invalid
 Git-Ok config core.autocrlf false
 $archive='docs/evidence/world4x-canon-20261022/sources'
 $name='Eldoria_Resource_Library.blend'
 $old="$archive/native-100/$name"
 New-Item -ItemType Directory "$archive/native-100" | Out-Null
 [IO.File]::WriteAllBytes((Join-Path $temp $old),[byte[]](0,1,2,255,13,10,0,8))
 Git-Ok add -- $old
 Git-Ok commit -qm baseline
 $generated=Join-Path $temp "generated files/$name"
 New-Item -ItemType Directory (Split-Path $generated) | Out-Null
 Copy-Item $old $generated
 $r=Save-EldoriaEditableSource $generated "$archive/native-200"
 Assert ($r.reused -and $r.path -eq $old -and $r.avoided_checkout_bytes -eq 8) 'Unchanged source must reference existing tracked bytes'
 Assert (-not(Test-Path "$archive/native-200/$name")) 'Must not create duplicate source'
 # An untracked lookalike cannot become the source of record.
 New-Item -ItemType Directory "$archive/native-050" | Out-Null
 Copy-Item $generated "$archive/native-050/$name"
 $r=Save-EldoriaEditableSource $generated "$archive/native-201"
 Assert ($r.path -eq $old) 'Only committed source paths may be reused'
 # Missing tracked file is recoverable; changed local file is never overwritten.
 Remove-Item $old
 $r=Save-EldoriaEditableSource $generated "$archive/native-202"
 Assert ((Test-Path $old) -and $r.reused) 'Restore one missing tracked source'
 [IO.File]::WriteAllText((Join-Path $temp $old),'local authoring work')
 Reject {Save-EldoriaEditableSource $generated "$archive/native-203"} 'Dirty tracked source must fail closed'
 Assert ((Get-Content $old -Raw) -eq 'local authoring work') 'Preserve local authoring work'
 Git-Ok checkout HEAD -- $old
 # A genuinely different source must remain independently editable.
 [IO.File]::WriteAllBytes($generated,[byte[]](9,8,7,0,255))
 $r=Save-EldoriaEditableSource $generated "$archive/native-204"
 Assert (-not $r.reused -and $r.bytes -eq 5 -and $r.avoided_checkout_bytes -eq 0) 'Keep changed source'
 Assert ((Get-FileHash $generated).Hash -eq (Get-FileHash $r.path).Hash) 'New source exact bytes'
 $again=Save-EldoriaEditableSource $generated "$archive/native-204"
 Assert ($again.path -eq $r.path) 'Idempotent retry must retain exact same destination'
 [IO.File]::WriteAllText((Join-Path $temp $r.path),'conflicting evidence')
 Reject {Save-EldoriaEditableSource $generated "$archive/native-204"} 'Refuse conflicting destination'
 Assert ((Get-Content $r.path -Raw) -eq 'conflicting evidence') 'Never overwrite conflicting evidence'
 Reject {Save-EldoriaEditableSource $generated '../outside'} 'Reject destination outside native archive'
 Reject {Save-EldoriaEditableSource (Join-Path $temp 'absent.blend') "$archive/native-205"} 'Reject unknown or missing source'
 # Existing manifest consumers follow file.path, not a fabricated per-run copy.
 Copy-Item $old $generated -Force
 $r=Save-EldoriaEditableSource $generated "$archive/native-206"
 $manifest=@{candidate_commit=(& git rev-parse HEAD).Trim();files=@(@{path=$r.path;sha256=$r.sha256;bytes=$r.bytes});editable_sources=@($r)}
 $decoded=$manifest | ConvertTo-Json -Depth 6 | ConvertFrom-Json
 foreach($file in $decoded.files){Assert ((Get-FileHash $file.path -Algorithm SHA256).Hash.ToLowerInvariant() -eq $file.sha256) 'Manifest referenced source must resolve and verify'}
 Git-Ok diff --exit-code HEAD -- $old
 Write-Host 'SOURCE_REUSE_PASS: exact reuse, changed/new sources, retry, missing recovery, dirty/conflicting protection, path bounds, manifest compatibility'
} finally {
 Set-Location $originalLocation
 # Only this test-created disposable directory; never repository or runner caches.
 if(Test-Path $temp){Remove-Item -LiteralPath $temp -Recurse -Force}
}
