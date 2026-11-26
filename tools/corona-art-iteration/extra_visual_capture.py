"""Extra actual screen captures from the unmodified WebGL journey; no synthetic UI."""
import argparse,hashlib,json,sys
from pathlib import Path
from PIL import Image,ImageDraw

ROLES=('council-b1','sawmill-built','bastion-upgrade','council-b2','barracks-plot')
BASE='5ef2aee67cf8fde57ba2fc4b3d929b517ee67944'
UI='3486754d5f4c28db2ae4ffa2a7def3b0f000fb3c'

def patch(source,dest):
    s=source.read_text(encoding='utf-8')
    def add(anchor,extra):
        nonlocal s
        if s.count(anchor)!=1:raise ValueError('Unexpected journey source anchor: '+anchor)
        s=s.replace(anchor,anchor+'\n'+extra,1)
    add("            await tap('Current real construction objective',False);assert state['council']",
        "            await page.wait_for_timeout(350);await page.screenshot(path=OUT/'extra-council-b1.png')")
    add("            await reload();assert state['wood']==150",
        "            await tap('Sawmill world nameplate',False);assert state['sawmillPanel']\n"
        "            await page.wait_for_timeout(350);await page.screenshot(path=OUT/'extra-sawmill-built.png')\n"
        "            await tap('Close sawmill',False)")
    add("            await tap('VisitBuilding/bastion')\n            assert state['building']=='bastion' and not state['council'],state",
        "            if n==2:\n                await page.wait_for_timeout(350);await page.screenshot(path=OUT/'extra-bastion-upgrade.png')")
    add("                await tap('VisitBuilding/'+target)\n                assert state['building']==target and not state['council'],state",
        "                if target=='barracks':\n                    await page.wait_for_timeout(350);await page.screenshot(path=OUT/'extra-barracks-plot.png')")
    add("            await funds(450,300);await ascend(2,120)",
        "            assert state['council'];await page.wait_for_timeout(350);await page.screenshot(path=OUT/'extra-council-b2.png')")
    add("            await funds(1000,1000);await order('Build/barracks')",
        "            assert state['level']==2 and state['barracks']>=1,state\n"
        "            print('EXTRA_ART_REAL_JOURNEY_PASS',flush=True)\n            return")
    compile(s,str(dest),'exec');dest.write_text(s,encoding='utf-8')

def manifest(folder,sha,run):
    if (sha,run) not in ((BASE,'45638738009'),(UI,'45824434222')):raise ValueError('Unknown source')
    data=json.loads((folder/'journey.json').read_text())
    if data['last']['level']!=2 or data['last']['barracks']<1:raise ValueError('Journey incomplete')
    if data['viewport']!=[390,844]:raise ValueError('Wrong viewport')
    frames={}
    for role in ROLES:
        p=folder/('extra-'+role+'.png')
        with Image.open(p) as im:
            if im.size!=(390,844):raise ValueError('Incorrect dimensions '+role)
        frames[role]=hashlib.sha256(p.read_bytes()).hexdigest()
    (folder/'extra-manifest.json').write_text(json.dumps({'source_sha':sha,'source_run':run,'frames':frames,'art_approved':False},indent=2))

def pairs(before,after,out):
    out.mkdir(exist_ok=False)
    records=[]
    for folder,sha,run in ((before,BASE,'45638738009'),(after,UI,'45824434222')):
        m=json.loads((folder/'extra-manifest.json').read_text())
        if (m['source_sha'],m['source_run'])!=(sha,run) or set(m['frames'])!=set(ROLES):raise ValueError('Source mismatch')
        records.append(m)
    for role in ROLES:
        canvas=Image.new('RGB',(792,870),'#172028');draw=ImageDraw.Draw(canvas)
        for x,folder,m,label in ((0,before,records[0],'ORIGINAL'),(402,after,records[1],'CORONA')):
            p=folder/('extra-'+role+'.png')
            if hashlib.sha256(p.read_bytes()).hexdigest()!=m['frames'][role]:raise ValueError('Frame hash mismatch')
            with Image.open(p) as im:
                if im.size!=(390,844):raise ValueError('Frame size mismatch')
                canvas.paste(im.convert('RGB'),(x,26))
            draw.text((x+5,5),label+' / '+role,fill='white')
        canvas.save(out/(role+'.png'))
    (out/'manifest.json').write_text(json.dumps({'source_runs':records,'roles':ROLES,'visual_approval':False,'same_camera_not_certified':True,'note':'Genuine browser progression at matching stage, not a fixed-camera native A/B.'},indent=2))

if __name__=='__main__':
    op=sys.argv[1]
    if op=='patch':patch(Path(sys.argv[2]),Path(sys.argv[3]))
    elif op=='manifest':manifest(Path(sys.argv[2]),sys.argv[3],sys.argv[4])
    elif op=='pairs':pairs(*map(Path,sys.argv[2:5]))
    else:raise ValueError('Unknown command')
