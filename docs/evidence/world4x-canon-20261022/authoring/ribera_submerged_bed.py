"""N: editable underwater relief fitted to the retained river boundaries; real alpha surface."""
import bpy,sys,pathlib,hashlib,json
import numpy as np
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);src=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='0f0d4cf3b84d7ebedfdbb776b78e104cdc8efa798f0fc826de2884be428998e8'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene
river=bpy.data.objects['Pilot_River'];assert len(river.data.vertices)==386
collection=bpy.data.collections.new('14 N submerged relief and transparent surface');scene.collection.children.link(collection)
# Native editable cross sections follow the existing banks, with an authored alternating thalweg.
keys=np.array([[-96,.40,1.2],[-70,.65,1.7],[-43,.72,2.1],[-18,.38,1.5],[4,.27,1.8],[25,.42,2.2],[47,.68,1.5],[72,.36,1.9],[96,.46,1.2]])
verts=[];faces=[];depths=[]
for i in range(193):
 a=river.data.vertices[i*2].co.copy();b=river.data.vertices[i*2+1].co.copy();x=(a.x+b.x)*.5
 c=float(np.interp(x,keys[:,0],keys[:,1]));d=float(np.interp(x,keys[:,0],keys[:,2]))
 for j in range(17):
  t=j/16;v=a.lerp(b,t);q=t/c if t<c else (1-t)/(1-c);depth=.24+d*max(0,q)**.85;v.z=.32-depth;verts.append(tuple(v));depths.append(depth)
  if i<192 and j<16:
   k=i*17+j;faces.append((k,k+17,k+18,k+1))
mesh=bpy.data.meshes.new('N editable asymmetric riverbed cross sections');mesh.from_pydata(verts,[],faces);mesh.update();bed=bpy.data.objects.new('Pilot_N_SubmergedBed',mesh);collection.objects.link(bed)
uv=mesh.uv_layers.new(name='N physical depth pigment')
for p in mesh.polygons:
 p.use_smooth=True
 for li in p.loop_indices:
  k=mesh.loops[li].vertex_index;uv.data[li].uv=((k//17)/192,(k%17)/16)
attr=mesh.attributes.new('N water depth metres','FLOAT','POINT')
for i,d in enumerate(depths):attr.data[i].value=d
# The pigment comes from actual bed depth, not three longitudinal painted stripes.
W=2048;H=256;xs=np.linspace(-96,96,W)[None,:];ts=np.linspace(0,1,H)[:,None];cent=np.interp(xs,keys[:,0],keys[:,1]);dep=np.interp(xs,keys[:,0],keys[:,2]);q=np.where(ts<cent,ts/cent,(1-ts)/(1-cent));depth=.24+dep*np.maximum(q,0)**.85
stops=[0,.24,.48,.9,1.5,2.5];cols=np.array([[.58,.59,.39],[.49,.57,.39],[.32,.52,.41],[.20,.46,.43],[.13,.37,.40],[.10,.29,.35]])
rgba=np.ones((H,W,4),np.float32)
for c in range(3):rgba[:,:,c]=np.interp(depth,stops,cols[:,c])
im=bpy.data.images.new('N depth-derived submerged earth',width=W,height=H);im.colorspace_settings.name='sRGB';im.pixels.foreach_set(rgba.ravel())
def material(name,image=None,color=(1,1,1,1)):
 m=bpy.data.materials.new(name);m.use_nodes=True;n=m.node_tree.nodes;n.clear();bs=n.new('ShaderNodeBsdfPrincipled');output=n.new('ShaderNodeOutputMaterial');m.node_tree.links.new(bs.outputs['BSDF'],output.inputs['Surface']);bs.inputs['Base Color'].default_value=color;bs.inputs['Roughness'].default_value=.85
 if image:
  tex=n.new('ShaderNodeTexImage');tex.image=image;m.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
 return m,bs
mat,bs=material('N submerged earth depth pigment',im);mesh.materials.append(mat)
water,bs=material('N River surface',color=(.22,.51,.57,.25));bs.inputs['Alpha'].default_value=.25;bs.inputs['Roughness'].default_value=.30
if hasattr(water,'surface_render_method'):
 enums=water.bl_rna.properties['surface_render_method'].enum_items.keys();water.surface_render_method='BLENDED' if 'BLENDED' in enums else 'DITHERED'
river.data.materials.clear();river.data.materials.append(water)
# Retain an editable authored guide and depth annotations, excluded from export/render.
curve=bpy.data.curves.new('N thalweg depth guide','CURVE');curve.dimensions='3D';s=curve.splines.new('BEZIER');s.bezier_points.add(len(keys)-1)
for point,(x,c,d) in zip(s.bezier_points,keys):
 i=int(x+96);a=river.data.vertices[i*2].co;b=river.data.vertices[i*2+1].co;v=a.lerp(b,float(c));point.co=(v.x,v.y,.08-d);point.handle_left_type='AUTO';point.handle_right_type='AUTO'
guide=bpy.data.objects.new('Editable N river depth guide',curve);collection.objects.link(guide);guide.hide_render=True
scene['finishing_stage']='N actual submerged bed and alpha surface on M K';scene['N_source_sha256']=hashlib.sha256(src.read_bytes()).hexdigest()
for im in bpy.data.images:
 if im.size[0]>0 and im.source in {'GENERATED','FILE'}:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'N','source_sha256':scene['N_source_sha256'],'bed_vertices':len(verts),'depth_guide':keys.tolist(),'water_boundary':'original M/K vertices unchanged','retained':'all terrain, banks, paths, forest, gameplay and cameras from M','changed':'submerged editable bed, depth-derived pigment, transparent water surface; pilot-only Unity shader','art_approved':False},indent=2))
