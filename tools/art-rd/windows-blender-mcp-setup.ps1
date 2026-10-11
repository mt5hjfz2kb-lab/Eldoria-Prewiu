param([switch]$Install,[switch]$StartBlender)
$ErrorActionPreference = 'Stop'
$found=@()
foreach($root in @("C:\Program Files\Blender Foundation", "$env:LOCALAPPDATA\Programs\Blender Foundation")){
 if(Test-Path $root){$found += Get-ChildItem $root -Filter blender.exe -Recurse -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName}
}
$blender=$found|Sort-Object -Descending|Select-Object -First 1
if(-not $blender){$cmd=Get-Command blender.exe -ErrorAction SilentlyContinue; if($cmd){$blender=$cmd.Source}}
if(-not $blender){throw "Blender not found. Install Blender on Windows first."}
$version=(& $blender --version | Select-Object -First 1)
$uvx=(Get-Command uvx.exe -ErrorAction SilentlyContinue)
$report=[ordered]@{blender=$blender; version=$version; uvx_available=[bool]$uvx; installed=$false; live_connection_tested=$false; ai_agent_connected=$false}
if($Install){
 if(-not $uvx){throw "uvx.exe not available. Install uv from https://docs.astral.sh/uv/getting-started/installation/ before running -Install."}
 $result=& $uvx.Source --from mcp-for-blender mcp-for-blender install-addon 2>&1
 if($LASTEXITCODE -ne 0){throw "MCP addon install failed: $result"}
 $report.installed=$true
 $report.install_output=($result|Out-String).Trim()
}
if($StartBlender){
 if(-not $report.installed){throw "Use -Install -StartBlender together."}
 Start-Process -FilePath $blender
 $report.note="In Blender enable the MCP for Blender addon and select Start MCP Server. Do not expose its socket to public network."
}
$report|ConvertTo-Json -Depth 5
