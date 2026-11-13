# Dot-sourced by the isolated native pipeline. Never applies to the shared project.
$uxRoot=Join-Path $Tools 'tools/eldoria-private-beta/native/ux'
$uxManifest=Get-Content (Join-Path $uxRoot 'manifest.json') -Raw|ConvertFrom-Json
$uxFiles=@($uxManifest.files)
function Get-UxCandidate([string]$Path){
 $entry=@($uxFiles|Where-Object {$_.path -eq $Path})
 if($entry.Count -eq 1){return Join-Path $uxRoot $entry[0].overlay}
 return Join-Path $Tools $Path
}
function Install-UxOverlay {
 # Validate the entire transaction before writing a single source file.
 foreach($entry in $uxFiles){
  $from=Join-Path $uxRoot $entry.overlay;$to=Join-Path $Source $entry.path
  if(-not(Test-Path $to) -or (Get-FileHash $to -Algorithm SHA256).Hash -ne $entry.baseline_sha256){throw "UX base differs: $($entry.path)"}
  if((Get-FileHash $from -Algorithm SHA256).Hash -ne $entry.candidate_sha256){
   # A reused Windows checkout can retain CRLF after new LF attributes arrive.
   # Accept ONLY the exact manifest-pinned LF bytes; no changed source is tolerated.
   $normalized=[Text.Encoding]::UTF8.GetBytes([IO.File]::ReadAllText($from).Replace("`r`n","`n"))
   $sha=[Security.Cryptography.SHA256]::Create()
   try{$normalizedHash=[BitConverter]::ToString($sha.ComputeHash($normalized)).Replace('-','')}finally{$sha.Dispose()}
   if($normalizedHash -ne $entry.candidate_sha256){throw "UX candidate differs: $($entry.path)"}
   [IO.File]::WriteAllBytes($from,$normalized)
   Write-Host "Restored exact LF payload bytes: $($entry.path)"
  }
 }
 foreach($entry in $uxFiles){Copy-Item (Join-Path $uxRoot $entry.overlay) (Join-Path $Source $entry.path) -Force}
 Copy-Item (Join-Path $uxRoot 'manifest.json') (Join-Path $Evidence 'ux-source-overlay.json')
}
