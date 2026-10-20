"""Three original conifer meshes: curved branch sweeps and folded serrated needle sprays.
No cones/icosphere crowns, downloaded replacement or AI-generated executable code.
"""
import bpy,bmesh,json,math,random,pathlib,hashlib
from mathutils import Vector
ROOT=pathlib.Path.cwd();EV=ROOT/'docs/evidence/valoria-reference-clean/world204';EV.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
palette=[(.18,.105,.047),(.055,.115,.035),(.095,.19,.055),(.16,.27,.085)]
mats=[]
for i,c in enumerate(palette):
 m=bpy.data.materials.new(['Weathered branch bark','Needle undersides','Mature needles','Sunward needle tips'][i]);m.diffuse_color=(*c,1);m.use_nodes=True;n=m.node_tree.nodes;p=next((n for n in n if n.type=='BSDF_PRINCIPLED'),None) or n.new('ShaderNodeBsdfPrincipled');out=next((q for q in n if q.type=='OUTPUT_MATERIAL'),None) or n.new('ShaderNodeOutputMaterial');m.node_tree.links.new(p.outputs['BSDF'],out.inputs['Surface']);p.inputs['Base Color'].default_value=(*c,1);p.inputs['Roughness'].default_value=.94;mats.append(m)
variants=[];low_variants=[];preview=[]
for variant,(height,spread,tiers,lean) in enumerate([(8.4,2.45,8,.16),(7.2,2.05,7,-.34),(9.1,2.55,9,.42)]):
 rng=random.Random(2041+variant*173);verts=[];faces=[];slots=[]
 def add(points,polygons,material):
  base=len(verts);verts.extend([tuple(p) for p in points]);faces.extend([tuple(base+i for i in f) for f in polygons]);slots.extend([material]*len(polygons))
 def trunkpoint(t):return Vector((lean*t*t,.13*math.sin(t*4+variant)*t,height*t))
 def sweep(points,radii,sides=6):
  vv=[];ff=[]
  for j,p in enumerate(points):
   tangent=(points[min(j+1,len(points)-1)]-points[max(j-1,0)]).normalized();axis=tangent.cross(Vector((0,1,0))).normalized();other=tangent.cross(axis).normalized()
   for k in range(sides):vv.append(p+radii[j]*(axis*math.cos(k*math.tau/sides)+other*math.sin(k*math.tau/sides)))
  ff.append(tuple(reversed(range(sides))))
  for j in range(len(points)-1):
   for k in range(sides):a=j*sides+k;b=j*sides+(k+1)%sides;ff.append((a,b,b+sides,a+sides))
  ff.append(tuple((len(points)-1)*sides+k for k in range(sides)));add(vv,ff,0)
 sweep([trunkpoint(i/12) for i in range(13)],[.23*(1-i/12)**.8+.018 for i in range(13)],8)
 # Visible flared roots are continuation sweeps, buried slightly below the pivot.
 for k in range(4):
  a=k*math.tau/4+.3;d=Vector((math.cos(a),math.sin(a),0));sweep([Vector((0,0,.27)),d*.37+Vector((0,0,.07)),d*.72+Vector((0,0,-.035))],[.15,.09,.012],5)
 for tier in range(tiers):
  t=.21+.70*tier/(tiers-1)+rng.uniform(-.018,.018);count=6 if tier<tiers-3 else 4
  for k in range(count):
   angle=k*math.tau/count+tier*2.399+variant*.61+rng.uniform(-.19,.19);direction=Vector((math.cos(angle),math.sin(angle),0));start=trunkpoint(t);length=spread*(1-(t-.18)/.85)**.63*rng.uniform(.83,1.12)
   points=[start+direction*(length*f)+Vector((0,0,-.28*math.sin(math.pi*f)+.13*f)) for f in [0,.27,.57,.82,1]];sweep(points,[.073*(1-t),.052*(1-t),.032,.018,.006],5)
   for index,f in enumerate([.40,.84]):
    base=start+direction*(length*f)+Vector((0,0,-.21*math.sin(math.pi*f)+rng.uniform(-.04,.07)))
    # Alternate secondary sprays spread through a volume, never a stacked cone.
    for side in [-1,1]:
     a=angle+side*(.42 if index<2 else .27);d=Vector((math.cos(a),math.sin(a),.14+index*.045));cross=Vector((-math.sin(a),math.cos(a),0));L=(.78+.32*(1-t))*rng.uniform(.86,1.13);W=(.36+.18*(1-t))*rng.uniform(.86,1.13);vv=[]
     for j in range(6):
      q=j/5;outline=math.sin(math.pi*q)**.6*(.72 if j%2==0 else 1.12);middle=base+d*(L*q)+Vector((0,0,-.13*math.sin(math.pi*q)))
      vv.extend([middle-cross*(W*outline)+Vector((0,0,-.05*outline)),middle+Vector((0,0,.10*outline)),middle+cross*(W*outline)+Vector((0,0,-.05*outline))])
     ff=[]
     for j in range(5):
      a0=j*3;ff.extend([(a0,a0+3,a0+1),(a0+1,a0+3,a0+4),(a0+1,a0+4,a0+2),(a0+2,a0+4,a0+5)])
     mat=2 if (tier+k+index)%3 else 3;add(vv,ff,mat)
     add(vv,[tuple(reversed(p)) for p in ff],1)
 mesh=bpy.data.meshes.new('Authored branch conifer '+str(variant));mesh.from_pydata(verts,[],faces);mesh.update()
 for m in mats:mesh.materials.append(m)
 for p,slot in zip(mesh.polygons,slots):p.material_index=slot;p.use_smooth=slot==0
 bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.triangulate(bm,faces=bm.faces);bm.to_mesh(mesh);bm.free();mesh.update()
 obj=bpy.data.objects.new(mesh.name,mesh);bpy.context.collection.objects.link(obj);preview.append(obj)
 outv=[];normals=[];uvs=[];sub=[[] for _ in mats]
 for poly in mesh.polygons:
  for vi in poly.vertices:
   v=mesh.vertices[vi];outv.append({'x':v.co.x,'y':v.co.z,'z':-v.co.y});normal=v.normal if poly.material_index==0 else poly.normal;normals.append({'x':normal.x,'y':normal.z,'z':-normal.y});uvs.append({'x':v.co.x,'y':v.co.z});sub[poly.material_index].append(len(outv)-1)
 variants.append({'name':obj.name,'vertices':outv,'normals':normals,'uv':uvs,'submeshes':[{'indices':s} for s in sub]})
 # A shared lower mesh is retained for the strategic camera, not a second scene.
 low=obj.copy();low.data=mesh.copy();bpy.context.collection.objects.link(low);bpy.context.view_layer.objects.active=low
 dec=low.modifiers.new('Strategic silhouette LOD','DECIMATE');dec.ratio=.38;bpy.ops.object.modifier_apply(modifier=dec.name);lm=low.data;lm.calc_loop_triangles();lv=[];ln=[];ls=[[] for _ in mats]
 for tri in lm.loop_triangles:
  for vi in tri.vertices:
   v=lm.vertices[vi];lv.append({'x':v.co.x,'y':v.co.z,'z':-v.co.y});normal=lm.polygons[tri.polygon_index].normal;ln.append({'x':normal.x,'y':normal.z,'z':-normal.y});ls[tri.material_index].append(len(lv)-1)
 low_variants.append({'name':obj.name+' strategic LOD','vertices':lv,'normals':ln,'submeshes':[{'indices':s} for s in ls]});low.hide_render=True;low.hide_viewport=True
 obj.location.x=(variant-1)*7
# Sources retain all three independently editable meshes and material assignments.
bpy.context.preferences.filepaths.save_version=0;bpy.ops.wm.save_as_mainfile(filepath=str(EV/'ConiferFamily.blend'),compress=True)
(EV/'conifer-family.json').write_text(json.dumps({'variants':variants,'low_variants':low_variants,'colors':[{'r':r,'g':g,'b':b,'a':1} for r,g,b in palette]},separators=(',',':')))
proof={'author':'Independent director original mesh authoring','operations':['curved tapered trunk and branch sweeps','flared root continuation','unequal branch whorls','folded serrated needle sprays with undersides','three controlled silhouette variants'],'triangles':[len(v['vertices'])//3 for v in variants],'low_triangles':[len(v['vertices'])//3 for v in low_variants],'blend_sha256':hashlib.sha256((EV/'ConiferFamily.blend').read_bytes()).hexdigest(),'mesh_sha256':hashlib.sha256((EV/'conifer-family.json').read_bytes()).hexdigest(),'art_approved':False};(EV/'conifer-author-proof.json').write_text(json.dumps(proof,indent=2))
# Isolated diagnostic preview, not a Unity result or final approval.
bpy.ops.object.camera_add(location=(16,-28,18));camera=bpy.context.object;camera.rotation_euler=(Vector((0,0,4))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=25;bpy.context.scene.camera=camera
bpy.ops.object.light_add(type='AREA',location=(-8,-12,18));bpy.context.object.data.energy=2400;bpy.context.object.data.size=10
scene=bpy.context.scene
try:scene.render.engine='BLENDER_EEVEE_NEXT'
except TypeError:scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=1200;scene.render.resolution_y=700;scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG';scene.render.film_transparent=True;scene.render.filepath=str(EV/'conifer-family-diagnostic.png');bpy.ops.render.render(write_still=True)
print('VALORIA_CONIFER_ORIGINAL_AUTHOR_COMPLETE')
