"""Art-directed major cliff terrain-to-rock intervention using V2.1 existing continuous cliff source, Qwen's selected major geological shelf option and real pinned BlenderMCP."""
import bpy,json,math
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
ENGINE=ROOT/'tools/art-rd/valoria_family_v2_1_mcp_author.py'
src=ENGINE.read_text().split('for asset,plan in PLANS.items():')[0]
ns={'__file__':str(ENGINE),'__name__':'valoria_cliff_v2_2_authorship'}
exec(compile(src,str(ENGINE),'exec'),ns)
OUT=ns['OUT']
source=OUT/'cliff_rocks/cliff_rocks-v2-1.blend'
assert source.is_file()
bpy.ops.wm.open_mainfile(filepath=str(source))
choice=ns['PLANS']['cliff_rocks']
assert choice['operation']=='sculpt_major_geological_shelves'
bed=next(x for x in bpy.data.objects if x.type=='MESH' and x.name.startswith('Sculpted continuous bedrock'))
records=[]
def rework():
 def authored_terrain(p):
  x,y,z=p.x,p.y,p.z
  theta=math.atan2(y,x)
  r=math.sqrt((x/4.6)**2+(y/3.9)**2)
  # Preserve broad fortification footprint, sculpt believable crest/rim and valleys.
  if z>=-.40:
   brow=max(0,min(1,(r-.58)/.43))
   brow=brow*brow*(3-2*brow)
   ridge=.64*math.sin(3.0*theta+.7)+.36*math.cos(7*theta+1.4)
   p.z+=brow*(.42*ridge-.24*math.sin(theta*5+1.1))
   # Decrease perfectly circular/quasi-rectangular cutline.
   if r>.85:
    drift=.32*math.sin(theta*5.2+.6)+.18*math.sin(theta*11.4)
    p.x+=drift*math.cos(theta)*brow
    p.y+=drift*math.sin(theta)*brow
  else:
   u=min(1,max(0,-z/4.8))
   # Major shelf planes with naturally broken interruptions, not object jitter.
   shelf=.38*math.sin(u*math.pi*5.2+.4)*math.sin(math.pi*u)
   fracture=.34*math.sin(theta*6+u*8.1)*math.sin(math.pi*u)
   gutter=-.58*max(0,math.cos(theta*5.1+u*.9))**7*math.sin(math.pi*u)
   shift=shelf+fracture+gutter
   norm=max(.1,math.hypot(x,y))
   p.x+=shift*x/norm;p.y+=shift*y/norm
   p.z+=.14*math.sin(theta*7.8+u*10.5)*math.sin(math.pi*u)
 ns['edit'](bed,authored_terrain,records)
 assert records and records[0]['vertices_modified']>100
def remote():rework()
ns['execute'](remote)
folder=OUT/'cliff_rocks'
ns['render'](folder)
ns['export']('cliff_rocks',folder)
for suffix in ('hero.png','strategic.png'):
 (folder/('v2-1-'+suffix)).rename(folder/('v2-2-'+suffix))
(folder/'cliff_rocks-v2-1.blend').rename(folder/'cliff_rocks-v2-2.blend')
(folder/'cliff_rocks-v2-1.glb').rename(folder/'cliff_rocks-v2-2.glb')
(folder/'cliff_rocks-v2-2-evidence.json').write_text(json.dumps({'qwen_selected_operation':choice['operation'],'diagnosis':choice['observation'],'artist_directed_extension':'major rim morphology, cohesive layered outcrops, deep erosional gullies, protected central footprint','mcp_real_execution':True,'modified':records,'art_pass':False},indent=2))
print('VALORIA_CLIFF_CONTINUOUS_TERRAIN_V2_2_REAL_MCP_DONE',records)
