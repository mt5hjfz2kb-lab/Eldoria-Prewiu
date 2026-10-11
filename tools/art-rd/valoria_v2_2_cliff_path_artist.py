"""Visual REWORK of selected V2.1 cliff crown and paving land connection.
The agent art direction is grounded in inspected matched V1/V2 renders:
cliff still resembles a flat artificial mesa; paving reads as detached circular podium.
This reuses existing MCP bridge and existing source meshes; no new art system.
"""
import bpy,sys,json,math,hashlib
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
helper=ROOT/'tools/art-rd/valoria_family_v2_1_mcp_author.py'
text=helper.read_text().split('for asset,plan in PLANS.items():')[0]
ns={'__file__':str(helper)}
exec(compile(text,str(helper),'exec'),ns)
OUT=ns['OUT']
def repair_cliff(records):
 beds=[o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith('Sculpted continuous bedrock')]
 assert len(beds)==1
 bed=beds[0]
 def geological(p):
  x,y,z=p.x,p.y,p.z
  angle=math.atan2(y,x);rr=math.hypot(x,y)
  crown_weight=max(0,min(1,(z+1.0)/1.0))
  radial_weight=max(0,min(1,(rr-2.2)/2.6))
  # Artist-designed wedge-shaped higher back shoulder, lower forward escarpment.
  # Blend away at central gate footing. Physically continues down the cliff lip.
  shoulder=(.73*math.sin(x*.72+y*.32)+.44*math.cos(y*.84-x*.44)
            +.26*math.sin(x*1.8+y*1.4)+.22*max(0,y)/4.6)
  p.z+=crown_weight*radial_weight*shoulder
  if z>=-.22 or z<=-4.7:return
  depth=min(1,max(0,-z/4.8))
  # Two major strata with angular breaks, rather than fine noise or uniform banding.
  fracture=(.41*math.sin((z+depth*.2)*3.7+angle*2.2)
            +.22*math.cos(angle*11.5+depth*9.1))
  erosion=-.29*(max(0,math.sin(angle*8.0+depth*2.5))**5)
  relief=(fracture+erosion)*math.sin(math.pi*depth)
  radial=max(.1,rr)
  p.x+=relief*x/radial;p.y+=relief*y/radial
 ns['edit'](bed,geological,records)
 # Retain original CC0 photogrammetric rocks and insert additional smaller embedded talus.
 material=next(m for m in bpy.data.materials if m.use_nodes)
 for i in range(15):
  a=math.tau*i/15+.11*math.sin(i*1.9)
  r=4.04+.23*math.sin(i*2.3)
  name='AI-composed geology contact talus '+str(i)
  s=.28+.06*((i*7)%5)
  ns['new_rock'](name,(r*math.cos(a),r*.87*math.sin(a),-4.42),(s*1.5,s*1.16,.35+.06*(i%4)),material)
 records.append({'artistic_reconstruction':'uneven natural cliff crown, bedrock strata, and 15 embedded talus rocks'})
def repair_path(records):
 foundation=next(o for o in bpy.data.objects if o.type=='MESH' and 'buried_stone_embankment' in o.name)
 def reshape(p):
  x,y,z=p.x,p.y,p.z
  angle=math.atan2(y,x)
  # Remove disk perimeter by transitioning it into an elongated, banked rocky apron.
  p.x=x*(1+.18*math.cos(angle*5.2)+.055*math.sin(angle*11))
  p.y=y*(1+.13*math.sin(angle*3.1)+.055*math.cos(angle*8))
  p.z=z+.08*math.cos(angle*6)+.07*math.sin(x*.5+y*.7)
 ns['edit'](foundation,reshape,records)
 # True roadbed beneath stepping stones: connected irregular sculpted ribbon.
 material=next(m for m in bpy.data.materials if m.use_nodes)
 verts=[];faces=[];stations=17;lanes=7
 for i in range(stations):
  v=i/(stations-1)
  y=-2.8+v*12.4
  center=.55*math.sin(v*2.3)
  halfwidth=2.9-1.05*v+.12*math.sin(v*14)
  z=-.29-.33*v+.10*math.sin(v*7.4)
  for k in range(lanes):
   t=(k/(lanes-1))*2-1
   x=center+t*halfwidth*(1+.08*math.sin(v*18+t*5))
   bank=.10*t*t+.08*math.sin(v*12+t*2.8)
   verts.append((x,y,z+bank))
 for i in range(stations-1):
  for k in range(lanes-1):
   j=i*lanes+k;faces.append((j,j+1,j+1+lanes,j+lanes))
 mesh=bpy.data.meshes.new('AI sculpted continuous path-ground transition');mesh.from_pydata(verts,[],faces);mesh.update()
 mesh.materials.append(material)
 ob=bpy.data.objects.new('AI continuous exposed roadbed beneath paving',mesh);bpy.context.scene.collection.objects.link(ob)
 uv=mesh.uv_layers.new(name='UVMap')
 for face in mesh.polygons:
  for loop in face.loop_indices:
   p=mesh.vertices[mesh.loops[loop].vertex_index].co
   uv.data[loop].uv=(p.x*.27,p.y*.27)
 records.append({'new_connected_roadbed_vertices':len(verts),'faces':len(faces),'artistic_reconstruction':'curved earthen support under paving, breaking detached circular platform'})
for asset,fn in [('cliff_rocks',repair_cliff),('stone_path_platform',repair_path)]:
 folder=OUT/asset
 src=folder/(asset+'-v2-1.blend')
 assert src.is_file(),src
 bpy.ops.wm.open_mainfile(filepath=str(src))
 edits=[]
 ns['execute'](lambda:fn(edits))
 assert edits
 ns['render'](folder)
 for view in ('hero','strategic'):
  (folder/('v2-1-'+view+'.png')).rename(folder/('v2-2-'+view+'.png'))
 ns['export'](asset,folder)
 # rename output from helper to unambiguously final V2.2 source.
 (folder/(asset+'-v2-1.blend')).rename(folder/(asset+'-v2-2.blend'))
 (folder/(asset+'-v2-1.glb')).rename(folder/(asset+'-v2-2.glb'))
 (folder/'v2-2-director-mcp-evidence.json').write_text(json.dumps(
 {'asset':asset,'diagnosis':'Matched V1/V2 review rejected floating circular apron or mesa crown',
  'agent_authoring_decision':'Explicit morphological rebuild of geological crown or physically supported path',
  'blender_mcp_actual':True,'edited_geometry':edits,'art_pass':False,'unity_pass':False},indent=2))
 print('ELDORIA_DIRECTED_ORGANIC_REWORK',asset,len(edits),flush=True)
