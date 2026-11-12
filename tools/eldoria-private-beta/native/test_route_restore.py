"""Real Git/PowerShell recovery guards; no Unity/device certification."""
import hashlib,json,os,pathlib,shutil,subprocess,tempfile
HERE=pathlib.Path(__file__).resolve().parent
PWSH=os.environ.get('ELDORIA_TEST_PWSH') or shutil.which('pwsh') or shutil.which('powershell')
if not PWSH:raise RuntimeError('PowerShell executable required')
PATH='Unity/Assets/Eldoria/World4XCanon/RegionOneTerrainRoutes.cs'
checks=0
def need(ok,message):
 global checks
 checks+=1
 if not ok:raise AssertionError(message)
def git(root,*args):
 return subprocess.run(['git','-C',str(root),*args],check=True,stdout=subprocess.PIPE,stderr=subprocess.PIPE).stdout.decode().strip()
def status(root):
 return subprocess.check_output(['git','-C',str(root),'status','--porcelain','--','Unity']).decode().splitlines()
with tempfile.TemporaryDirectory(prefix='eldoria-route-recovery-') as temp:
 for case in ['recorded','crlf','already_clean','unknown_route','foreign_change','staged','dirty_changed','wrong_head']:
  root=pathlib.Path(temp)/case;root.mkdir()
  git(root,'init');git(root,'config','user.email','native-fixture@example.invalid');git(root,'config','user.name','Route recovery fixture');git(root,'config','core.autocrlf','false')
  dirty='Unity/Assets/owned-pending.txt';extra='Unity/Assets/unexpected.cs'
  original=b'original tracked route\n'
  for path in [PATH,dirty,extra]:
   out=root/path;out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes(original)
  (root/'.gitattributes').write_text('*.cs text eol=lf\n')
  git(root,'add','.');git(root,'commit','-m','Exact fixture baseline');head=git(root,'rev-parse','HEAD')
  (root/dirty).write_bytes(b'preserve original pending input\n');digest=hashlib.sha256((root/dirty).read_bytes()).hexdigest()
  audit={'base':head,'status':status(root),'files':[{'path':dirty,'sha256':digest}]}
  audit_path=root/'audit.json';audit_path.write_text(json.dumps(audit))
  known=root/'recorded.cs.txt';known.write_bytes(b'exact prior comparison route\n')
  if case!='already_clean':(root/PATH).write_bytes(b'exact prior comparison route\r\n' if case=='crlf' else known.read_bytes())
  if case=='unknown_route':(root/PATH).write_bytes(b'unknown authored route modification\n')
  if case=='foreign_change':(root/extra).write_bytes(b'foreign edit\n')
  if case=='staged':git(root,'add',PATH)
  if case=='dirty_changed':(root/dirty).write_bytes(b'altered pending input\n')
  before=(root/PATH).read_bytes();evidence=root/'evidence';evidence.mkdir()
  runner=root/'run.ps1';runner.write_text('''param($Helper,$Source,$AuditPath,$Evidence,$ExpectedHead,$Known)
$ErrorActionPreference='Stop'
. $Helper
$audit=Get-Content -LiteralPath $AuditPath -Raw|ConvertFrom-Json
$count=Repair-PR179RecordedRouteIntermediate -Source $Source -Audit $audit -Evidence $Evidence -ExpectedHead $ExpectedHead -KnownIntermediate $Known
Write-Host "ROUTE_RESTORED=$count"
''')
  r=subprocess.run([PWSH,'-NoLogo','-NoProfile','-File',str(runner),str(HERE/'recover_route_intermediate.ps1'),str(root),str(audit_path),str(evidence),'0'*40 if case=='wrong_head' else head,str(known)],stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True)
  allowed=case in ['recorded','crlf','already_clean']
  need((r.returncode==0)==allowed,case+' execution: '+r.stdout)
  if allowed:
   need(status(root)==audit['status'],case+' original exact status')
   need((root/PATH).read_bytes()==original,case+' exact original route bytes')
   need(hashlib.sha256((root/dirty).read_bytes()).hexdigest()==digest,case+' original dirty input preserved')
   if case!='already_clean':
    need((evidence/'recorded-route-before-restore.cs.txt').read_bytes()==before,case+' original intermediate evidence retained')
    report=json.loads((evidence/'recorded-route-restoration.json').read_text(encoding='utf-8-sig'))
    need(report['restored_blob']==git(root,'rev-parse',head+':'+PATH),case+' frozen Git identity')
  else:need((root/PATH).read_bytes()==before,case+' refusal before any mutation')
  print('CASE_PASS',case)
print('REAL_GIT_POWERSHELL_ROUTE_RESTORE_PASS checks='+str(checks)+'; not Windows Unity execution')
