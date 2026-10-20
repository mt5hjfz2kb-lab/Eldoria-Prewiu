"""Three original conifer meshes: curved branch sweeps and folded serrated needle sprays.
No cones/icosphere crowns, downloaded replacement or AI-generated executable code.
"""
import bpy,bmesh,json,math,random,pathlib,hashlib,numpy as np
from mathutils import Vector
ROOT=pathlib.Path.cwd();EV=ROOT/'docs/evidence/valoria-reference-clean/world204';EV.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
palette=[(.18,.105,.047),(.055,.115,.035),(.095,.19,.055),(.16,.27,.085),(.38,.25,.12)]
mats=[]
for i,c in enumerate(palette):
 m=bpy.data.materials.new(['Weathered branch bark','Needle undersides','Mature needles','Sunward needle tips','Exposed weathered cut wood'][i]);m.diffuse_color=(*c,1);m.use_nodes=True;n=m.node_tree.nodes;p=next((n for n in n if n.type=='BSDF_PRINCIPLED'),None) or n.new('ShaderNodeBsdfPrincipled');out=next((q for q in n if q.type=='OUTPUT_MATERIAL'),None) or n.new('ShaderNodeOutputMaterial');m.node_tree.links.new(p.outputs['BSDF'],out.inputs['Surface']);p.inputs['Base Color'].default_value=(*c,1);p.inputs['Roughness'].default_value=.94;mats.append(m)
# Original needle atlas: many slender needles around branching stems, with real
# alpha holes. This replaces the rejected solid paper-like leaflet silhouette.
W,H=256,512
xx,yy=np.meshgrid(np.arange(W)/(W-1),np.arange(H)/(H-1));alpha=np.zeros((H,W),dtype=np.float32);tone=np.zeros_like(alpha)
def stroke(a,b,r,value):
 dx,dy=b[0]-a[0],b[1]-a[1];t=np.clip(((xx-a[0])*dx+(yy-a[1])*dy)/(dx*dx+dy*dy),0,1)
 dist=np.sqrt((xx-a[0]-t*dx)**2+(yy-a[1]-t*dy)**2);mask=np.clip((r-dist)*512+0.5,0,1);np.maximum(alpha,mask,out=alpha);np.maximum(tone,mask*value,out=tone)
stroke((.5,.02),(.5,.98),.009,.48)
rngtex=random.Random(208)
for k in range(19):
 y=.06+k*.044;extent=.43*math.sin(math.pi*y)**.65
 for side in (-1,1):
  end=(.5+side*extent,y+.10);stroke((.5,y),end,.006,.63)
  for n in range(1,9):
   t=n/9;bx=.5+side*extent*t;by=y+.10*t
   for tilt in (-1,1):stroke((bx,by),(bx+side*rngtex.uniform(.035,.067),by+tilt*.027+.034),.0034,.72+.25*t)
pixels=np.ones((H,W,4),dtype=np.float32);pixels[:,:,:3]=np.maximum(tone,.35)[:,:,None];pixels[:,:,3]=alpha
atlas=bpy.data.images.new('Original conifer needle cutout',width=W,height=H,alpha=True);atlas.pixels.foreach_set(pixels.ravel());atlas.filepath_raw=str(EV/'conifer-needles.png');atlas.file_format='PNG';atlas.save();atlas.pack()
for material in mats[1:4]:
 nodes=material.node_tree.nodes;pr=next(n for n in nodes if n.type=='BSDF_PRINCIPLED');tex=nodes.new('ShaderNodeTexImage');tex.image=atlas;material.node_tree.links.new(tex.outputs['Alpha'],pr.inputs['Alpha'])
 try:material.surface_render_method='DITHERED'
 except AttributeError:pass
variants=[];low_variants=[];preview=[]
for variant,(height,spread,tiers,lean) in enumerate([(8.4,2.45,8,.16),(7.2,2.05,7,-.34),(9.1,2.55,9,.42)]):
 rng=random.Random(2041+variant*173);verts=[];faces=[];slots=[];coords=[]
 def add(points,polygons,material,uv=None):
  coords.extend(uv if uv else [(0,0)]*len(points));base=len(verts);verts.extend([tuple(p) for p in points]);faces.extend([tuple(base+i for i in f) for f in polygons]);slots.extend([material]*len(polygons))
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
     sprayuv=[(u,j/5) for j in range(6) for u in (0,.5,1)];mat=2 if (tier+k+index)%3 else 3;add(vv,ff,mat,sprayuv)
     add(vv,[tuple(reversed(p)) for p in ff],1,sprayuv)
 mesh=bpy.data.meshes.new('Authored branch conifer '+str(variant));mesh.from_pydata(verts,[],faces);mesh.update()
 uvlayer=mesh.uv_layers.new(name='Authored needle UV')
 for poly in mesh.polygons:
  for li in poly.loop_indices:uvlayer.data[li].uv=coords[mesh.loops[li].vertex_index]
 for m in mats:mesh.materials.append(m)
 for p,slot in zip(mesh.polygons,slots):p.material_index=slot;p.use_smooth=True
 bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.triangulate(bm,faces=bm.faces);bm.to_mesh(mesh);bm.free();mesh.update()
 obj=bpy.data.objects.new(mesh.name,mesh);bpy.context.collection.objects.link(obj);preview.append(obj)
 outv=[];normals=[];uvs=[];sub=[[] for _ in mats]
 for poly in mesh.polygons:
  for li in poly.loop_indices:
   vi=mesh.loops[li].vertex_index;v=mesh.vertices[vi];outv.append({'x':v.co.x,'y':v.co.z,'z':-v.co.y});normal=v.normal;normals.append({'x':normal.x,'y':normal.z,'z':-normal.y});uvs.append({'x':mesh.uv_layers.active.data[li].uv.x,'y':mesh.uv_layers.active.data[li].uv.y});sub[poly.material_index].append(len(outv)-1)
 variants.append({'name':obj.name,'vertices':outv,'normals':normals,'uv':uvs,'submeshes':[{'indices':s} for s in sub]})
 # A shared lower mesh is retained for the strategic camera, not a second scene.
 low=obj.copy();low.data=mesh.copy();bpy.context.collection.objects.link(low);bpy.context.view_layer.objects.active=low
 dec=low.modifiers.new('Strategic silhouette LOD','DECIMATE');dec.ratio=.38;bpy.ops.object.modifier_apply(modifier=dec.name);lm=low.data;lm.calc_loop_triangles();lv=[];ln=[];lu=[];ls=[[] for _ in mats]
 for tri in lm.loop_triangles:
  for li in tri.loops:
   vi=lm.loops[li].vertex_index;v=lm.vertices[vi];lu.append({"x":lm.uv_layers.active.data[li].uv.x,"y":lm.uv_layers.active.data[li].uv.y});lv.append({'x':v.co.x,'y':v.co.z,'z':-v.co.y});normal=v.normal;ln.append({'x':normal.x,'y':normal.z,'z':-normal.y});ls[tri.material_index].append(len(lv)-1)
 low_variants.append({'name':obj.name+' strategic LOD','vertices':lv,'normals':ln,'uv':lu,'submeshes':[{'indices':s} for s in ls]});low.hide_render=True;low.hide_viewport=True
 obj.location.x=(variant-1)*7
# Original fallen wood uses the same curved profile tools, with exposed ends.
verts=[];faces=[];slots=[];coords=[]
sweep([Vector((-1.9+i*.475,.06*math.sin(i*.8),.23+.035*math.cos(i*.7))) for i in range(9)],[.22-i*.008 for i in range(9)],9)
slots[0]=4;slots[-1]=4
for x,sgn in [(-.6,1),(.7,-1)]:sweep([Vector((x,0,.25)),Vector((x+.25,sgn*.48,.40)),Vector((x+.38,sgn*.84,.48))],[.075,.045,.02],5)
mesh=bpy.data.meshes.new('Original fallen timber with broken branches');mesh.from_pydata(verts,[],faces);mesh.update()
for m in mats:mesh.materials.append(m)
for p,slot in zip(mesh.polygons,slots):p.material_index=slot;p.use_smooth=slot==0
bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.triangulate(bm,faces=bm.faces);bm.to_mesh(mesh);bm.free();mesh.update();obj=bpy.data.objects.new(mesh.name,mesh);bpy.context.collection.objects.link(obj)
ev=[];en=[];es=[[] for _ in mats]
for poly in mesh.polygons:
 for vi in poly.vertices:
  v=mesh.vertices[vi];ev.append({'x':v.co.x,'y':v.co.z,'z':-v.co.y});nn=v.normal if poly.material_index==0 else poly.normal;en.append({'x':nn.x,'y':nn.z,'z':-nn.y});es[poly.material_index].append(len(ev)-1)
extras=[{'name':mesh.name,'vertices':ev,'normals':en,'submeshes':[{'indices':a} for a in es]}]
# Sources retain all three independently editable meshes and material assignments.
bpy.context.preferences.filepaths.save_version=0;bpy.ops.wm.save_as_mainfile(filepath=str(EV/'ConiferFamily.blend'),compress=True)
(EV/'conifer-family.json').write_text(json.dumps({'variants':variants,'low_variants':low_variants,'extras':extras,'colors':[{'r':r,'g':g,'b':b,'a':1} for r,g,b in palette]},separators=(',',':')))
proof={'author':'Independent director original mesh authoring','operations':['curved tapered trunk and branch sweeps','flared root continuation','unequal branch whorls','folded serrated needle sprays with undersides','three controlled silhouette variants'],'triangles':[len(v['vertices'])//3 for v in variants],'low_triangles':[len(v['vertices'])//3 for v in low_variants],'blend_sha256':hashlib.sha256((EV/'ConiferFamily.blend').read_bytes()).hexdigest(),'mesh_sha256':hashlib.sha256((EV/'conifer-family.json').read_bytes()).hexdigest(),'art_approved':False};(EV/'conifer-author-proof.json').write_text(json.dumps(proof,indent=2))
# Isolated diagnostic preview, not a Unity result or final approval.
bpy.ops.object.camera_add(location=(16,-28,18));camera=bpy.context.object;camera.rotation_euler=(Vector((0,0,4))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=25;bpy.context.scene.camera=camera
bpy.ops.object.light_add(type='AREA',location=(-8,-12,18));bpy.context.object.data.energy=2400;bpy.context.object.data.size=10
scene=bpy.context.scene
try:scene.render.engine='BLENDER_EEVEE_NEXT'
except TypeError:scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=1200;scene.render.resolution_y=700;scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG';scene.render.film_transparent=True;scene.render.filepath=str(EV/'conifer-family-diagnostic.png');bpy.ops.render.render(write_still=True)
print('VALORIA_CONIFER_ORIGINAL_AUTHOR_COMPLETE')
