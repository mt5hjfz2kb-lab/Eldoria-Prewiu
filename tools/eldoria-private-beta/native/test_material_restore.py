import hashlib, json, os, pathlib, shutil, subprocess, tempfile

HERE=pathlib.Path(__file__).resolve().parent
PWSH=os.environ.get('ELDORIA_TEST_PWSH') or shutil.which('pwsh') or shutil.which('powershell')
if not PWSH:raise RuntimeError('Set ELDORIA_TEST_PWSH to a PowerShell executable')
PATHS=[
 'Unity/Assets/Holotna/Mountain/Materials/Bush01.mat',
 'Unity/Assets/Holotna/Mountain/Materials/Leaf01A.mat',
 'Unity/Assets/Holotna/Mountain/Materials/Leaf01B.mat',
 *['Unity/Assets/Mega Fantasy Props Pack/Materials/'+p+'.mat' for p in
 ['bark_2_d','cobblestone_wall_1_fixed_d','granite_1_d','stone_wall_3_d','wood_1_d','wooden-boards-texture_d']]]
checks=0
def need(ok,message):
 global checks
 checks+=1
 if not ok:raise AssertionError(message)
def git(root,*args):
 return subprocess.run(['git','-C',str(root),*args],check=True,stdout=subprocess.PIPE,stderr=subprocess.PIPE).stdout.decode().strip()
def status(root):
 return subprocess.run(['git','-C',str(root),'status','--porcelain','--','Unity'],check=True,stdout=subprocess.PIPE).stdout.decode().splitlines()
with tempfile.TemporaryDirectory(prefix='eldoria-native-material-') as temp:
 for case in ['nine','subset','unknown','dirty_changed','staged','wrong_head','already_clean']:
  root=pathlib.Path(temp)/case;root.mkdir()
  git(root,'init');git(root,'config','user.email','native-fixture@example.invalid');git(root,'config','user.name','Native material fixture');git(root,'config','core.autocrlf','false')
  dirty='Unity/Assets/owned-pending.txt';extra='Unity/Assets/unexpected.mat'
  for path in PATHS+[dirty,extra]:
   out=root/path;out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes(b'original tracked source\n')
  git(root,'add','.');git(root,'commit','-m','Immutable fixture baseline');head=git(root,'rev-parse','HEAD')
  (root/dirty).write_bytes(b'preserve this initially dirty input\n')
  digest=hashlib.sha256((root/dirty).read_bytes()).hexdigest()
  audit={'base':head,'status':status(root),'files':[{'path':dirty,'sha256':digest}]}
  audit_path=root/'audit.json';audit_path.write_text(json.dumps(audit))
  changed=[] if case=='already_clean' else PATHS[:3] if case=='subset' else PATHS
  for path in changed:(root/path).write_bytes(b'material normalized during prior native test\n')
  if case=='unknown':(root/extra).write_bytes(b'foreign source edit\n')
  if case=='dirty_changed':(root/dirty).write_bytes(b'changed after original audit\n')
  if case=='staged':git(root,'add',PATHS[0])
  evidence=root/'evidence';evidence.mkdir()
  runner=root/'run.ps1';runner.write_text('''param($Helper,$Source,$AuditPath,$Evidence,$ExpectedHead)
$ErrorActionPreference='Stop'
. $Helper
$audit=Get-Content -LiteralPath $AuditPath -Raw|ConvertFrom-Json
$count=Repair-PR179KnownMaterialDrift -Source $Source -Audit $audit -Evidence $Evidence -ExpectedHead $ExpectedHead
Write-Host "FIXTURE_RESTORED=$count"
''')
  result=subprocess.run([PWSH,'-NoLogo','-NoProfile','-File',str(runner),str(HERE/'preserve_native_materials.ps1'),str(root),str(audit_path),str(evidence),'0'*40 if case=='wrong_head' else head],stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True)
  allowed=case in ['nine','subset','already_clean']
  need((result.returncode==0)==allowed,case+' execution:\n'+result.stdout)
  if allowed:
   need(status(root)==audit['status'],case+' original Git status restored')
   need(hashlib.sha256((root/dirty).read_bytes()).hexdigest()==digest,case+' initial dirty bytes preserved')
   for path in PATHS:need((root/path).read_bytes()==b'original tracked source\n',case+' correct material '+path)
   if changed:
    report=json.loads((evidence/'known-material-restoration.json').read_text(encoding='utf-8-sig'))
    need(report['restored_count']==len(changed),case+' exact restored count')
    for path in changed:need((evidence/'known-material-before-restore'/path).read_bytes()==b'material normalized during prior native test\n',case+' drift bytes retained as evidence')
  else:
   need((root/PATHS[-1]).read_bytes()==b'material normalized during prior native test\n',case+' rejects before material mutation')
  print('CASE_PASS',case)
print('REAL_GIT_POWERSHELL_MATERIAL_RESTORE_PASS checks='+str(checks)+'; not Windows Unity execution')
