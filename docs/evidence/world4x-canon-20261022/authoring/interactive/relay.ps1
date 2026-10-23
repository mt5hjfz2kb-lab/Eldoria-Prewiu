$ErrorActionPreference='Stop'
$root=Join-Path $env:LOCALAPPDATA 'Eldoria/World4XInteractive/95183774-20261023'
$helpers=Join-Path $env:GITHUB_WORKSPACE 'world4x-source/docs/evidence/world4x-canon-20261022/authoring/interactive'
"ELDORIA_INTERACTIVE_OUT=$root" >> $env:GITHUB_ENV
$ready=Join-Path $root 'ready.json'
if(Test-Path $ready){
 $saved=Get-Content $ready -Raw | ConvertFrom-Json
 if(-not(Get-Process -Id $saved.pid -ErrorAction SilentlyContinue)){Move-Item $ready (Join-Path $root "ready-ended-$env:GITHUB_RUN_ID.json")}
}
if(-not(Test-Path $ready)){
 if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue).Count){throw 'Another production process is active'}
 New-Item -ItemType Directory -Force $root | Out-Null
 $source=Get-ChildItem "$env:RUNNER_TEMP/interactive-source" -Recurse -Filter eldoria-region1-author.blend | Select-Object -First 1 -ExpandProperty FullName
 if(-not $source){throw 'Verified landscape source unavailable'}
 $blender=Get-ChildItem 'C:/Program Files/Blender Foundation' -Recurse -Filter blender.exe | Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
 $command='"'+$blender+'" --background --python-exit-code 1 --python "'+$helpers+'/session.py" -- "'+$root+'" "'+$source+'"'
 $created=Invoke-CimMethod -ClassName Win32_Process -MethodName Create -Arguments @{CommandLine=$command}
 if($created.ReturnValue -ne 0){throw 'Cannot launch independent owned Blender session'}
 for($i=0;$i -lt 60 -and -not(Test-Path $ready);$i++){Start-Sleep -Seconds 1;if(-not(Get-Process -Id $created.ProcessId -ErrorAction SilentlyContinue)){throw 'Blender session failed to start'}}
}
$session=Get-Content $ready -Raw | ConvertFrom-Json
$p=Get-Process -Id $session.pid -ErrorAction Stop
if($p.ProcessName -ne 'blender'){throw 'Saved session PID is not Blender'}
if(@(Get-Process Unity,blender -ErrorAction SilentlyContinue | Where-Object {$_.Id -ne $session.pid}).Count){throw 'Conflicting production process'}
$code=[System.IO.File]::ReadAllText("$helpers/command.py")
$request=@{type='execute_code';params=@{code=$code}} | ConvertTo-Json -Depth 5 -Compress
$request | Set-Content "$root/request-$env:GITHUB_RUN_ID.json" -Encoding UTF8
$client=New-Object System.Net.Sockets.TcpClient
try{
 $client.Connect('127.0.0.1',[int]$session.port);$stream=$client.GetStream();$stream.ReadTimeout=600000
 $bytes=[Text.Encoding]::UTF8.GetBytes($request);$stream.Write($bytes,0,$bytes.Length)
 $buffer=New-Object byte[] 65536;$data=New-Object IO.MemoryStream
 while($true){$n=$stream.Read($buffer,0,$buffer.Length);if($n -eq 0){throw 'BlenderMCP disconnected'};$data.Write($buffer,0,$n);$text=[Text.Encoding]::UTF8.GetString($data.ToArray());try{$reply=$text|ConvertFrom-Json;break}catch{}}
 $text | Set-Content "$root/response-$env:GITHUB_RUN_ID.json" -Encoding UTF8
 Write-Host $text
 if($reply.status -ne 'success'){throw 'BlenderMCP command failed'}
}finally{$client.Dispose()}
"ELDORIA_INTERACTIVE_OUT=$root" >> $env:GITHUB_ENV
Write-Host "Persistent BlenderMCP pid=$($session.pid) port=$($session.port)"
