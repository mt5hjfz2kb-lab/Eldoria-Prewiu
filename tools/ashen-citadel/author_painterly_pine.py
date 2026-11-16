"""Blender authored swept boughs replace conical tree tiers; fortress layout unchanged."""
import bpy,bmesh,math,random,json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'artifacts/painterly-pine';OUT.mkdir(parents=True,exist_ok=True);MESH=ROOT/'Unity/Assets/Eldoria/ExperimentalAshenCitadel/Meshes'
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
rng=random.Random(731);mats=[];parts=[]
for name,color in [('wood',(.26,.17,.12)),('leaf',(.105,.235,.22)),('leafLight',(.17,.31,.27))]:
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(*color,1);mats.append(m)
bpy.ops.mesh.primitive_cone_add(vertices=9,radius1=.18,radius2=.035,depth=5.55,location=(0,0,2.775));trunk=bpy.context.object;trunk.data.materials.append(mats[0]);parts.append(trunk)
# Original generated branch albedo; alpha clipping in Unity, no transparent blending.
img=bpy.data.images.load(str(MESH.parent/'Textures/AshenNeedles.png'))
for mat in mats[1:]:
 nodes=mat.node_tree.nodes;links=mat.node_tree.links;bsdf=nodes.get('Principled BSDF');tex=nodes.new('ShaderNodeTexImage');tex.image=img
 links.new(tex.outputs['Color'],bsdf.inputs['Base Color']);links.new(tex.outputs['Alpha'],bsdf.inputs['Alpha'])
 mat.surface_render_method='DITHERED'
def card(base,angle,length,width,rise,material):
 vertices=[];uv=[];segments=3
 for j in range(segments+1):
  t=j/segments
  for side in [-1,1]:
   lateral=side*width*.5;rad=length*t
   vertices.append((base[0]+math.cos(angle)*rad-math.sin(angle)*lateral,base[1]+math.sin(angle)*rad+math.cos(angle)*lateral,base[2]+rise*t+.12*math.sin(t*math.pi)))
   uv.append(((side+1)/2,t))
 faces=[(j*2+2,j*2+3,j*2+1,j*2) for j in range(segments)]
 me=bpy.data.meshes.new('Needle card');me.from_pydata(vertices,[],faces);me.materials.append(mats[material]);me.uv_layers.new(name='UVMap')
 for poly in me.polygons:
  for li in poly.loop_indices:me.uv_layers.active.data[li].uv=uv[me.loops[li].vertex_index]
 ob=bpy.data.objects.new('Layered textured needles',me);bpy.context.collection.objects.link(ob);parts.append(ob)
for row in range(7):
 h=.65+row*.68;radius=1.85*(1-row/8.2);arms=6 if row<4 else 5
 for arm in range(arms):
  a=arm*math.tau/arms+row*1.37+rng.uniform(-.13,.13)
  card((0,0,h),a,radius*rng.uniform(.88,1.1),radius*.95,.15+row*.035,1 if arm%3 else 2)
  if row<5:card((0,0,h+.10),a+.26,radius*.80,radius*.72,.65,1)
for i in range(4):
 card((0,0,4.25),i*math.tau/4,.18,1.05,1.35,2)
bpy.ops.object.select_all(action='DESELECT')
for p in parts:p.select_set(True)
bpy.context.view_layer.objects.active=trunk;bpy.ops.object.join();obj=trunk;obj.name='Ashen_Pine';bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
bpy.ops.file.pack_all();bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Ashen_Pine_painterly.blend'))
bpy.ops.export_scene.gltf(filepath=str(OUT/'Ashen_Pine_painterly.glb'),export_format='GLB',use_selection=True)
lines=['# Original Blender-authored swept-bough pine','mtllib AshenPalette.mtl','o Ashen_Pine'];uv=[];normal=[];faces=[];v=0;me=obj.data;me.calc_loop_triangles()
for face in me.polygons:
 inds=[]
 for li in face.loop_indices:
  loop=me.loops[li];co=obj.matrix_world@me.vertices[loop.vertex_index].co;x,y,z=co.x,co.z,-co.y;lines.append(f'v {x:.6f} {y:.6f} {z:.6f}');uv.append(tuple(me.uv_layers.active.data[li].uv));n=me.corner_normals[li].vector;normal.append((n.x,n.z,-n.y));v+=1;inds.append(v)
 faces.append((me.materials[face.material_index].name,inds))
lines += [f'vt {u:.6f} {v:.6f}' for u,v in uv];lines += [f'vn {x:.6f} {y:.6f} {z:.6f}' for x,y,z in normal];last=None
for mat,inds in faces:
 if mat!=last:lines.append('usemtl '+mat);last=mat
 lines.append('f '+' '.join(f'{i}/{i}/{i}' for i in inds))
(MESH/'Ashen_Pine.obj').write_text('\n'.join(lines)+'\n');p=ROOT/'tools/ashen-citadel/art-manifest.json';j=json.loads(p.read_text());j['stats']['Pine']={'vertices':v,'polygons':len(faces),'triangles':sum(len(i)-2 for m,i in faces)};p.write_text(json.dumps(j,indent=2)+'\n');print('ASHEN_PINE_AUTHORED',j['stats']['Pine'])
world=bpy.data.worlds.new('Pine studio');bpy.context.scene.world=world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.12,.16,.20,1)
bpy.ops.object.light_add(type='AREA',location=(3,-4,7));bpy.context.object.data.energy=700;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=5
bpy.ops.object.camera_add(location=(7,-9,6));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,2.8))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=7.5
sc=bpy.context.scene;sc.camera=cam;sc.render.engine='CYCLES';sc.cycles.samples=24;sc.cycles.use_denoising=False;sc.render.resolution_x=650;sc.render.resolution_y=800;sc.render.resolution_percentage=100;sc.render.filepath=str(OUT/'Ashen_Pine_painterly.png');bpy.ops.render.render(write_still=True)
