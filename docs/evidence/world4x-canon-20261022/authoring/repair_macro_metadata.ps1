$ErrorActionPreference='Stop'
$root=Join-Path $env:RUNNER_TEMP "region1-authoring-$env:GITHUB_RUN_ID"
$query=Join-Path $root 'RegionOneBlenderQueries.bytes'
$layoutPath=Join-Path $root 'production-region.json'
$layout=Get-Content $layoutPath -Raw | ConvertFrom-Json
$proof=Get-Content (Join-Path $root 'macro-macro-art-operations.json') -Raw | ConvertFrom-Json
$sourceHash=(Get-FileHash $query -Algorithm SHA256).Hash.ToLowerInvariant()
if($sourceHash -ne '4c2c5bd9b34b150bc60f27db645ab5699a551fe2152e5b94f25c448d4a56c166'){throw 'Unexpected reviewed query source'}
$input=[IO.MemoryStream]::new([IO.File]::ReadAllBytes($query))
$gzip=[IO.Compression.GZipStream]::new($input,[IO.Compression.CompressionMode]::Decompress)
$buffer=[IO.MemoryStream]::new()
try{$gzip.CopyTo($buffer);$bytes=$buffer.ToArray()}finally{$gzip.Dispose();$input.Dispose();$buffer.Dispose()}
$nx=[BitConverter]::ToInt32($bytes,4);$nz=[BitConverter]::ToInt32($bytes,8)
if([BitConverter]::ToInt32($bytes,0) -ne 0x31424c45 -or $nx -ne 456 -or $nz -ne 901){throw 'Unexpected query signature/grid'}
$oldWidth=[BitConverter]::ToSingle($bytes,12);$oldDepth=[BitConverter]::ToSingle($bytes,16)
$width=[single]($layout.bounds[2]-$layout.bounds[0]);$depth=[single]($layout.bounds[3]-$layout.bounds[1])
if($width -ne 1820 -or $depth -ne 3600){throw 'Canonical actual bounds changed'}
$hash=[Security.Cryptography.SHA256]::Create()
try{
 $before=[BitConverter]::ToString($hash.ComputeHash($bytes,20,$bytes.Length-20))
 [Array]::Copy([BitConverter]::GetBytes($width),0,$bytes,12,4)
 [Array]::Copy([BitConverter]::GetBytes($depth),0,$bytes,16,4)
 $after=[BitConverter]::ToString($hash.ComputeHash($bytes,20,$bytes.Length-20))
}finally{$hash.Dispose()}
if($before -ne $after){throw 'Header repair changed actual terrain/masks/vertices'}
$tail=20+$nx*$nz*8
$vertices=[BitConverter]::ToInt32($bytes,$tail+4)
if([BitConverter]::ToInt32($bytes,$tail) -ne 0x32564c45 -or $vertices -ne $proof.terrain_vertices_after){throw 'Actual vertex tail mismatch'}
$file=[IO.File]::Create($query)
$zip=[IO.Compression.GZipStream]::new($file,[IO.Compression.CompressionMode]::Compress)
try{$zip.Write($bytes,0,$bytes.Length)}finally{$zip.Dispose();$file.Dispose()}
$glb=[IO.File]::ReadAllBytes((Join-Path $root 'RegionOneBlenderWorld.glb'))
if([BitConverter]::ToInt32($glb,0) -ne 0x46546c67){throw 'Reviewed world is not GLB'}
$jsonLength=[BitConverter]::ToInt32($glb,12)
$gltf=[Text.Encoding]::UTF8.GetString($glb,20,$jsonLength) | ConvertFrom-Json
$trees=@($gltf.nodes | Where-Object {$_.name -and $_.name.StartsWith('BLD_ForestMass_')})
$donors=@($trees | ForEach-Object {$_.mesh} | Sort-Object -Unique)
if($trees.Count -lt 20 -or $donors.Count -ne 2){throw 'Actual export must retain its two shared canopy donors'}
$previousTrees=$layout.continental_trees
$layout.continental_trees=$trees.Count
$layout.terrain_vertices=$vertices
[IO.File]::WriteAllText($layoutPath,($layout | ConvertTo-Json -Depth 30),[Text.UTF8Encoding]::new($false))
$unity=Join-Path $env:GITHUB_WORKSPACE 'world4x-source/Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Blender'
Copy-Item $query (Join-Path $unity 'RegionOneBlenderQueries.bytes') -Force
Copy-Item $layoutPath (Join-Path $unity 'production-region.json') -Force
@{source_query_sha256=$sourceHash;previous_width=$oldWidth;previous_depth=$oldDepth;width=$width;depth=$depth;terrain_vertices=$vertices;previous_declared_trees=$previousTrees;actual_exported_trees=$trees.Count;shared_canopy_meshes=$donors.Count;payload_after_header_unchanged=($before -eq $after);query_sha256=(Get-FileHash $query -Algorithm SHA256).Hash.ToLowerInvariant();geometry_reauthored=$false} |
 ConvertTo-Json | Set-Content (Join-Path $root 'macro-query-header-repair.json')
Write-Host "MACRO_QUERY_HEADER_REPAIR_PASS width=$width depth=$depth vertices=$vertices trees=$($trees.Count) donors=$($donors.Count) actual_heights_masks_vertex_tail_unchanged=true"
