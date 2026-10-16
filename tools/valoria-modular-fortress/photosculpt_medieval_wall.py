"""Reference-first, SINGLE 4m masonry wall module quality A/B.
Real medieval dressed blocks CC0 scanned source from Poly Haven, physically raised
mortar/stone surface relief, same review lens as frozen V3. No castle composition.
"""
import bpy,sys,json,math,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
oldglb,photodir,out=map(Path,args)
for part in ('before','after','glb','atlas','reports'):(out/part).mkdir(parents=True,exist_ok=True)
n='wall_panel_regular_4m'
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1152;s.render.resolution_y=900
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Valoria CC0 single stone wall lookdev');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.21,.26,.33,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.78;s.world=world
def light(name,power,pos,col):
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=10;d.color=col
 o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
light('early dusk golden sculpt',2000,(-9,-10,13),(1,.84,.69))
light('wide cold sky fill',1600,(7,4,12),(.7,.83,1))
light('rim highlight',920,(-4,10,9),(1,.96,.86))
camdata=bpy.data.cameras.new('frozen wall independent lens');cam=bpy.data.objects.new('frozen wall independent lens',camdata)
bpy.context.collection.objects.link(cam);s.camera=cam;camdata.type='ORTHO'
rig=set(bpy.data.objects)
def bbox(obj):
 p=[o.matrix_world@Vector(v) for o in obj for v in o.bound_box if o.type=='MESH']
 return (Vector([min(c[i] for c in p) for i in range(3)]),
         Vector([max(c[i] for c in p) for i in range(3)]))
def view(obj):
 lo,hi=bbox(obj);mid=(lo+hi)/2;dim=hi-lo
 cam.location=mid+Vector((.68,-1.55,1.06)).normalized()*max(dim.length,1)*3
 cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
 camdata.ortho_scale=max(dim.x,dim.y,dim.z)*1.53
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
if not oldglb.is_file():raise RuntimeError('Original V3 wall asset missing')
bpy.ops.import_scene.gltf(filepath=str(oldglb))
original=[ob for ob in bpy.data.objects if ob not in rig]
oldmesh=[ob for ob in original if ob.type=='MESH']
if not oldmesh:raise RuntimeError('No original physical wall geometry')
lo,hi=bbox(oldmesh);width=float(hi.x-lo.x);height=float(hi.z-lo.z);thickness=float(hi.y-lo.y)
view(oldmesh);render(out/'before'/(n+'.png'))
for obj in original:bpy.data.objects.remove(obj,do_unlink=True)
color=photodir/'medieval_blocks_03_diffuse.jpg'
normal=photodir/'medieval_blocks_03_nor_gl.jpg'
if not color.is_file() or not normal.is_file():raise RuntimeError('Poly Haven CC0 dressed-stone map incomplete')
source=bpy.data.images.load(str(color),check_existing=False)
source.scale(768,768)
array=np.empty(768*768*4,dtype=np.float32);source.pixels.foreach_get(array)
rgba=array.reshape((768,768,4));rgb=rgba[:,:,:3]
# Keep authentic hand-dressed block pattern while bringing its tonality to
# warm grey-beige Valoria limestone instead of moss-green stone.
means=rgb.mean(axis=(0,1))+.03
tint=np.array([.61,.57,.51],dtype=np.float32)
rgb[:]=np.clip(rgb/means[None,None,:]*tint[None,None,:],.04,.94)
outim=bpy.data.images.new('Valoria_hand_dressed_CC0_medieval_blocks',width=768,height=768,alpha=True)
outim.pixels.foreach_set(array);outim.update()
atlas=out/'atlas'/'valoria_hand_dressed_stone_CC0.png'
outim.filepath_raw=str(atlas);outim.file_format='PNG';outim.save()
bpy.data.images.remove(source)
material=bpy.data.materials.new('Valoria_CC0_photosculpted_medieval_wall');material.use_nodes=True
nodes=material.node_tree.nodes;nodes.clear();links=material.node_tree.links
tex=nodes.new('ShaderNodeTexImage');tex.image=outim;tex.extension='REPEAT'
bs=nodes.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.87
links.new(tex.outputs['Color'],bs.inputs['Base Color'])
img=bpy.data.images.load(str(normal),check_existing=True);img.colorspace_settings.name='Non-Color'
tn=nodes.new('ShaderNodeTexImage');tn.image=img;tn.extension='REPEAT'
normal_node=nodes.new('ShaderNodeNormalMap');normal_node.inputs['Strength'].default_value=.34
links.new(tn.outputs['Color'],normal_node.inputs['Color'])
links.new(normal_node.outputs['Normal'],bs.inputs['Normal'])
op=nodes.new('ShaderNodeOutputMaterial');links.new(bs.outputs[0],op.inputs['Surface'])
# Photographic geometry: vertices sampled in source lightness to approximate 4cm
# dark recessed mortar and stone-face irregularity. No unrelated wall mesh.
W,H=118,84
verts=[];faces=[];uvs=[];front=[];back=[]
lum=rgba[:,:,:3].dot(np.array([.28,.62,.10],dtype=np.float32))
mean_lum=float(lum.mean());std_lum=float(lum.std())+.025
R=768
for k in range(H):
 z=height*k/(H-1)
 row=[]
 for i in range(W):
  x=float(lo.x)+width*i/(W-1)
  u=i/(W-1)*2.0;v=k/(H-1)*1.40
  sample=lum[min(R-1,int((v%1)*R)),min(R-1,int((u%1)*R))]
  relief=.030*float(np.clip((sample-mean_lum)/std_lum,-1.15,1.2))
  y=float(lo.y)+relief
  row.append(len(verts));verts.append((x,y,z));uvs.append((u,v))
 front.append(row)
for k in range(H):
 row=[]
 for i in range(W):
  x=float(lo.x)+width*i/(W-1);z=height*k/(H-1);u=i/(W-1)*2.0;v=k/(H-1)*1.4
  row.append(len(verts));verts.append((x,float(hi.y),z));uvs.append((u,v))
 back.append(row)
for k in range(H-1):
 for i in range(W-1):
  a,b,c,d=front[k][i],front[k][i+1],front[k+1][i],front[k+1][i+1]
  faces.extend([(a,b,d),(a,d,c)])
  a,b,c,d=back[k][i],back[k][i+1],back[k+1][i],back[k+1][i+1]
  faces.extend([(a,c,d),(a,d,b)])
for i in range(W-1):
 for k in (0,H-1):faces.append((front[k][i],back[k][i],back[k][i+1],front[k][i+1]))
for k in range(H-1):
 for i in (0,W-1):faces.append((front[k][i],front[k+1][i],back[k+1][i],back[k][i]))
me=bpy.data.meshes.new('genuine_relief_4m_wall');me.from_pydata(verts,[],faces);me.update()
ob=bpy.data.objects.new(n+'_ONE_REPEATABLE_PHOTO_RELIEF_WALL',me)
bpy.context.collection.objects.link(ob);me.materials.append(material)
uv=me.uv_layers.new(name='UVMap')
for polygon in me.polygons:
 for li in polygon.loop_indices:
  uv.data[li].uv=uvs[me.loops[li].vertex_index]
# Flat facets are extremely small; smooth normals for relief continuity, normal map for grain.
for p in me.polygons:p.use_smooth=True
render(out/'after'/(n+'.png'))
bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob
glb=out/'glb'/(n+'.glb')
bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True)
if glb.stat().st_size<100000:raise RuntimeError('Photo wall export incomplete')
report={'asset':n,'technical':'PASS_PENDING_ART','source_cc0':'Poly Haven medieval_blocks_03 diffuse normal 1K',
 'source_run':38860976002,'original_sha':sha(oldglb),'candidate_sha':sha(glb),
 'wall_world_dimensions':[width,height,thickness],
 'front_grid_vertices':W*H,'total_faces':len(me.polygons),
 'atlas_sha':sha(atlas),'art_approved':False,'unity_approved':False,
 'warning':'Only visual comparison decides; this wall tile may have texture repetitions and mismatched mortar seams.',
 'no_scene_composition':True}
(out/'reports'/'real_medieval_cc0_single_wall.json').write_text(json.dumps(report,indent=2))
print('ONE_PHOTOSCULPTED_MEDIEVAL_WALL_A_B_COMPLETE',len(me.polygons),glb.stat().st_size,flush=True)
