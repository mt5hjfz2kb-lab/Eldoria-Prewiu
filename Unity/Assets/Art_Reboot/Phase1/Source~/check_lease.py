"""Read-only live lease validation; no lease acquisition or resource reassignment."""
import base64,json,os,urllib.request
OWNER='chatgpt-visual-reboot-phase1-continuation-20261116'
ID='eldoria-art-visual-reboot-phase1-20261115'
BRANCH='art/visual-reboot'
def validate(reg, head, current_head):
 if current_head!=head:raise RuntimeError('Stale source commit')
 holders=[w for w in reg['active'] if 'windows-runner-heavy' in w.get('resources',[])]
 if len(holders)!=1:raise RuntimeError('No sole Windows holder')
 w=holders[0]
 if (w.get('id'),w.get('owner'),w.get('branch'),w.get('ownership_state'),w.get('status'))!=(ID,OWNER,BRANCH,'claimed','active'):
  raise RuntimeError('Windows reservation belongs to another owner/task')
 if w.get('native_source_sha')!=head:raise RuntimeError('Lease does not pin this source commit')
 return w['ownership_revision']
def main():
 repo=os.environ['GITHUB_REPOSITORY'];head=os.environ['GITHUB_SHA']
 def get(path):
  req=urllib.request.Request('https://api.github.com/repos/'+repo+'/'+path,headers={
   'Authorization':'Bearer '+os.environ['GH_TOKEN'],'Accept':'application/vnd.github+json','User-Agent':'visual-reboot-lease'})
  with urllib.request.urlopen(req,timeout=30) as response:return json.load(response)
 current_head=get('git/ref/heads/'+BRANCH)['object']['sha']
 file=get('contents/pipeline/active-workstreams.json?ref=main')
 if file.get('encoding')!='base64':
  expected=file['sha'];file=get('git/blobs/'+expected)
  if file.get('sha')!=expected:raise RuntimeError('Registry blob identity mismatch')
 if file.get('encoding')!='base64':raise RuntimeError('Unsupported registry encoding')
 reg=json.loads(base64.b64decode(file['content']))
 revision=validate(reg,head,current_head)
 print('REBOOT_LIVE_EXACT_SOURCE_LEASE_PASS',head,revision)
if __name__=='__main__':main()
