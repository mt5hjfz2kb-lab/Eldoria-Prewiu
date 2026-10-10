"""Experimental rights-sensitive asset-kit proof: render only, NEVER export bundled FBX.
Edits separated from canonical Unity, and no quality approval implied.
"""
import bpy,bmesh,math,json,os
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[2]
out=Path(os.environ.get('ELDORIA_CURATED_OUTPUT',str(root/'artifacts-local/curated-bastion')))
out.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(root/'Unity/Assets/Eldoria/ProductionSlice/Experimental/ValoriaMeshOnlyWorld.glb'))
sc=bpy.context.scene;sc.render.engine='CYCLES';sc.cycles.samples=8
sc.render.resolution_x=960;sc.render.resolution_y=640;sc.render.resolution_percentage=100
sc.render.image_settings.file_format='PNG';sc.view_settings.view_transform='Standard'
w=bpy.data.worlds.new('Dark blue hour');sc.world=w;w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.25,.31,.40,1)
light=bpy.data.lights.new('Broad illumination','AREA');lamp=bpy.data.objects.new('Broad illumination',light);sc.collection.objects.link(lamp)
lamp.location=(-38,-24,66);light.energy=22000;light.size=32
camd=bpy.data.cameras.new('Canonical study framing');cam=bpy.data.objects.new('Canonical study framing',camd);sc.collection.objects.link(cam);sc.camera=cam
cam.location=(55,-82,78);cam.rotation_euler=(Vector((0,6,8))-cam.location).to_track_quat('-Z','Y').to_euler()
camd.type='ORTHO';camd.ortho_scale=94
sc.render.filepath=str(out/'before.png');bpy.ops.render.render(write_still=True)
centers=[(side*(11+(i%2)*6),-11+(i//2)*13) for side in (-1,1) for i in range(4)]
removed=0
for ob in list(bpy.data.objects):
 if ob.type!='MESH' or not ob.name.startswith('Static material batch'):continue
 bm=bmesh.new();bm.from_mesh(ob.data)
 faces=[]
 for face in bm.faces:
  p=ob.matrix_world@face.calc_center_median()
  if 8.0<=p.z<=13.15 and any(abs(p.x-x)<=3.9 and abs(p.y-y)<=3.9 for x,y in centers):
   faces.append(face)
 if faces:
  removed+=len(faces)
  bmesh.ops.delete(bm,geom=faces,context='FACES')
  bm.to_mesh(ob.data);ob.data.update()
 bm.free()
meshdir=root/'Unity/Assets/EmaceArt/Slavic World Free/Meshes'
variants=['EA03_Village_OutBuilding_Shed_01a.fbx','EA03_Village_OutBuilding_Cubby_01d.fbx',
          'EA03_Village_OutBuilding_Shed_03b.fbx','EA03_Town_Building_Administrative _01a.fbx']
atlas=root/'Unity/Assets/EmaceArt/Slavic World Free/Texture/EA03_FREE_Slavica.png'
image=bpy.data.images.load(str(atlas),check_existing=True)
results=[]
for i,(cx,cy) in enumerate(centers):
 name=variants[i%len(variants)]
 before=set(bpy.data.objects)
 bpy.ops.import_scene.fbx(filepath=str(meshdir/name))
 candidates=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
 if not candidates:raise RuntimeError('No FBX mesh '+name)
 lod0=[o for o in candidates if 'LOD0' in o.name.upper()]
 if not lod0:
  lod0=[max(candidates,key=lambda o:sum(len(p.vertices)-2 for p in o.data.polygons))]
 for o in candidates:
  if o not in lod0:bpy.data.objects.remove(o,do_unlink=True)
 vertices=[o.matrix_world@v.co for o in lod0 for v in o.data.vertices]
 minx,maxx=min(v.x for v in vertices),max(v.x for v in vertices)
 miny,maxy=min(v.y for v in vertices),max(v.y for v in vertices)
 minz=min(v.z for v in vertices)
 scale=min(5.0/max(.01,maxx-minx),5.5/max(.01,maxy-miny))
 angle=math.radians((i%4)*90)
 for o in lod0:
  coords=[o.matrix_world@v.co for v in o.data.vertices]
  o.matrix_world.identity()
  for v,old in zip(o.data.vertices,coords):
   x=(old.x-(minx+maxx)/2)*scale;y=(old.y-(miny+maxy)/2)*scale
   v.co=(cx+x*math.cos(angle)-y*math.sin(angle),cy+x*math.sin(angle)+y*math.cos(angle),8.05+(old.z-minz)*scale)
  o.name='Curated Slavic medieval variant '+str(i)
  for m in o.data.materials:
   if not m:continue
   m.use_nodes=True
   bs=m.node_tree.nodes.get('Principled BSDF')
   if bs and not bs.inputs['Base Color'].is_linked:
    tex=m.node_tree.nodes.new('ShaderNodeTexImage');tex.image=image
    m.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
 results.append({'source':name,'placement':[cx,cy],'scale':round(scale,3),'objects':len(lod0)})
sc.render.filepath=str(out/'after.png');bpy.ops.render.render(write_still=True)
report={'method':'licensed-library-curated-fbx-render-only','commercial_rights_unreviewed':True,
        'embeds_proprietary_mesh_as_public_artifact':False,'faces_removed':removed,'placed':results,
        'visual_pass':False,'unity_tested':False}
(out/'report.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print('ELDORIA_CURATED_GEOMETRY_VISUAL_PROOF_PASS',json.dumps(report))
