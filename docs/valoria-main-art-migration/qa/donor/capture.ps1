param([Parameter(Mandatory=$true)][string]$Kit,[Parameter(Mandatory=$true)][string]$Tree,[Parameter(Mandatory=$true)][string]$Evidence)
$ErrorActionPreference='Stop'
$blender=(Get-Command blender.exe -ErrorAction SilentlyContinue).Source
if (!$blender) {
  $installed=Get-ChildItem 'C:\Program Files\Blender Foundation' -Recurse -File -Filter blender.exe | Sort-Object FullName -Descending | Select-Object -First 1
  $blender=$installed.FullName
}
if (!(Test-Path -LiteralPath $blender -PathType Leaf)) { throw 'Installed Blender not found' }
# The browser and its Python dependencies stay in the scratch folder owned by this run.
$venv=Join-Path $env:VM_ROOT 'python-env'
python -m venv $venv
if ($LASTEXITCODE) { throw 'Private Python environment could not be initialized' }
$python=Join-Path $venv 'Scripts/python.exe'
& $python -m pip install --disable-pip-version-check --no-cache-dir 'playwright==1.56.0' pillow
if ($LASTEXITCODE) { throw 'Private QA dependencies unavailable' }
$env:PLAYWRIGHT_BROWSERS_PATH=Join-Path $env:VM_ROOT 'browsers'
& $python -m playwright install chromium
if ($LASTEXITCODE) { throw 'Private Chromium or FFmpeg installation failed' }
$chrome=& $python -c "from playwright.sync_api import sync_playwright; p=sync_playwright().start(); print(p.chromium.executable_path); p.stop()"
if ($LASTEXITCODE -or !(Test-Path -LiteralPath $chrome -PathType Leaf)) { throw 'Private Chromium not found' }
New-Item -ItemType Directory -Path $Evidence | Out-Null
$native=Join-Path $Evidence 'region'
$browser=Join-Path $Evidence 'screens'
& $python "$Kit/donor/run.py" native --checkout $Tree --unity $env:UNITY_EDITOR_PATH --blender $blender --output $native
if ($LASTEXITCODE) { throw 'Experimental region fixture or private package failed' }
& $python "$Kit/donor/run.py" browser --checkout $Tree --native $native --chrome $chrome --dpr 1 --output $browser
if ($LASTEXITCODE) { throw 'Experimental secondary screen traversal failed' }
