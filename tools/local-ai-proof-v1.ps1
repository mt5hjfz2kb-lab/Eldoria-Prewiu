param([Parameter(Mandatory=$true)][string]$EvidenceDir)
$ErrorActionPreference='Stop'
$ProgressPreference='SilentlyContinue'
$model='qwen2.5-coder:3b'
$base='http://127.0.0.1:11434'
function RequestLocal([string]$path,$payload=$null) {
  if ($null -eq $payload) { return Invoke-RestMethod -Uri "$base$path" -TimeoutSec 180 }
  $body=$payload | ConvertTo-Json -Depth 12 -Compress
  return Invoke-RestMethod -Uri "$base$path" -Method Post -ContentType 'application/json' -Body ([Text.Encoding]::UTF8.GetBytes($body)) -TimeoutSec 180
}
New-Item -ItemType Directory -Force -Path $EvidenceDir | Out-Null
$tags=RequestLocal '/api/tags'
$entry=@($tags.models | Where-Object {$_.name -eq $model})
if ($entry.Count -ne 1) { throw 'Exact local model not found.' }
$prompt=@'
Return only a JSON object with key "code" containing PowerShell source.
Implement function Clamp with param($value, $low, $high). Assume low <= high.
Use exactly two sequential if statements with one return each, followed by one final return.
First compare $value -lt $low and return $low. Then compare $value -gt $high and return $high. Finally return $value.
Use braces around both if bodies. No type annotations, comments, strings, calls, assignments, extra functions, else branches or markdown fences.
'@
$response=RequestLocal '/api/generate' @{model=$model;stream=$false;keep_alive='30s';format='json';options=@{temperature=0;num_ctx=2048;num_predict=512;num_thread=4};prompt=$prompt}
$response | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $EvidenceDir 'response.json') -Encoding UTF8
if (!$response.done -or $response.model -ne $model) { throw 'Incomplete or wrong model response.' }
$decoded=$response.response | ConvertFrom-Json
if ($decoded.code -isnot [string] -or $decoded.code.Length -gt 4000) { throw 'Invalid or oversized generated code.' }
$source=$decoded.code
$source | Set-Content (Join-Path $EvidenceDir 'generated-clamp.txt') -Encoding UTF8
# Parse an intentionally tiny closed grammar. Generated text is never executed.
$var='(?:value|low|high)'
$op='(?:lt|gt|le|ge|eq|ne)'
$pattern='(?is)^\s*function\s+Clamp\s*\{\s*param\s*\(\s*\$value\s*,\s*\$low\s*,\s*\$high\s*\)\s*'
for ($i=1;$i -le 2;$i++) {
  $pattern+='if\s*\(\s*\$(?<a'+$i+'>'+$var+')\s+-(?<op'+$i+'>'+$op+')\s+\$(?<b'+$i+'>'+$var+')\s*\)\s*\{\s*return\s+\$(?<r'+$i+'>'+$var+')\s*;?\s*\}\s*'
}
$pattern+='return\s+\$(?<r3>'+$var+')\s*;?\s*\}\s*$'
$match=[regex]::Match($source,$pattern)
if (!$match.Success) { throw 'Generated code outside bounded pure-function grammar.' }
$program=@()
for ($i=1;$i -le 2;$i++) {
  $program+=@{a=$match.Groups['a'+$i].Value.ToLowerInvariant();op=$match.Groups['op'+$i].Value.ToLowerInvariant();b=$match.Groups['b'+$i].Value.ToLowerInvariant();r=$match.Groups['r'+$i].Value.ToLowerInvariant()}
}
$last=$match.Groups['r3'].Value.ToLowerInvariant()
$cases=@(@(-3,0,10,0),@(5,0,10,5),@(14,0,10,10),@(0,0,10,0),@(10,0,10,10),@(-7,-10,-2,-7),@(-20,-10,-2,-10),@(20,-10,-2,-2),@(5,3,3,3))
$passed=0
foreach($case in $cases) {
  $envValues=@{value=$case[0];low=$case[1];high=$case[2]}
  $actual=$envValues[$last]
  foreach($node in $program) {
    $a=$envValues[$node.a];$b=$envValues[$node.b];$condition=$false
    switch($node.op) {
      'lt' {$condition=$a -lt $b}
      'gt' {$condition=$a -gt $b}
      'le' {$condition=$a -le $b}
      'ge' {$condition=$a -ge $b}
      'eq' {$condition=$a -eq $b}
      'ne' {$condition=$a -ne $b}
      default {throw 'Unsupported comparison.'}
    }
    if ($condition) { $actual=$envValues[$node.r];break }
  }
  if ($actual -ne $case[3]) { throw "Generated function failed case $passed" }
  $passed++
}
$ps=RequestLocal '/api/ps'
$sha=[Security.Cryptography.SHA256]::Create()
try { $hash=([BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($source)))).Replace('-','').ToLowerInvariant() } finally { $sha.Dispose() }
$proof=@{status='PASS';model=$model;digest=$entry[0].digest;test_cases_passed=$passed;endpoint=$base;external_api_calls=0;arbitrary_generated_code_executed=$false;method='closed-grammar interpretation of generated pure PowerShell function';source_sha256=$hash;eval_count=$response.eval_count;eval_duration_ns=$response.eval_duration;loaded_models=$ps;parsed_program=$program;final_return=$last}
$proof | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $EvidenceDir 'proof.json') -Encoding UTF8
$null=RequestLocal '/api/generate' @{model=$model;keep_alive=0}
$after=RequestLocal '/api/ps'
if (@($after.models).Count) { throw 'Model remains loaded after explicit unload.' }
Write-Output ($proof | ConvertTo-Json -Depth 12 -Compress)
