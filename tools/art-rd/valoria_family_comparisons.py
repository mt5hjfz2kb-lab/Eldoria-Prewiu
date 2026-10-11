"""Real V3-target/asset-before/BlenderMCP-after side-by-side evidence.
Pixel delta is an edit-proof diagnostic ONLY, never an art-quality verdict.
"""
from pathlib import Path
from PIL import Image,ImageOps,ImageDraw,ImageChops,ImageStat
import json
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art-source/valoria/production/first-asset-family-v1'
V3=ROOT/'art-source/valoria/production/first-hero-gate-v1/quality-v3'
names=('wall_straight','watchtower','wall_corner','wall_connections',
       'stone_path_platform','cliff_rocks','defensive_props')
report={}
for name in names:
 folder=OUT/name
 evidence=json.loads((folder/'mcp-authoring-evidence.json').read_text())
 rows=[];delta={}
 for view in ('hero','strategic'):
  baseline=V3/('after-hero-matched.png' if view=='hero' else 'after-strategic.png')
  before=folder/('before-'+view+'.png')
  after=folder/('mcp-after-'+view+'.png')
  assert all(p.is_file() and p.stat().st_size>0 for p in (baseline,before,after)),(name,view)
  im1=Image.open(before).convert('RGB')
  im2=Image.open(after).convert('RGB')
  assert im1.size==im2.size,'Comparison needs pixel-identical camera and resolution'
  stat=ImageStat.Stat(ImageChops.difference(im1,im2))
  delta[view]=sum(stat.mean)/len(stat.mean)
  panels=[]
  for label,path in [('V3 MINIMUM',baseline),('BEFORE '+name,before),('AFTER MCP '+name,after)]:
   im=Image.open(path).convert('RGB')
   im.thumbnail((600,405))
   panel=Image.new('RGB',(620,460),(25,28,33))
   x=(620-im.width)//2
   panel.paste(im,(x,40))
   ImageDraw.Draw(panel).text((14,12),label,fill=(230,230,230))
   panels.append(panel)
  row=Image.new('RGB',(1860,460),(21,24,28))
  for i,panel in enumerate(panels):row.paste(panel,(i*620,0))
  rows.append(row)
 board=Image.new('RGB',(1860,920),(21,24,28))
 for i,row in enumerate(rows):board.paste(row,(0,i*460))
 board.save(folder/'v3-before-after-comparison.png',optimize=True)
 report[name]={'real_before_after_difference':delta,'mcp_evidence':str((folder/'mcp-authoring-evidence.json').relative_to(ROOT)),
               'comparative_board':str((folder/'v3-before-after-comparison.png').relative_to(ROOT)),
               'reference':'unchanged Hero V3','art_pass':False,'unity_pass':False,
               'artist_authored_change':bool(evidence.get('mcp_tcp_executed') or evidence.get('blender_mcp_status')=='executed_tcp_success')}
 assert report[name]['artist_authored_change'],('Missing actual MCP change',name)
(OUT/'family-visual-comparison-index.json').write_text(json.dumps(report,indent=2))
print('ELDORIA_FAMILY_SEVEN_MCP_COMPARISONS_EXPORTED; ART_PASS_FALSE')
