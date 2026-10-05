"""Generic editable-mesh authoring/export engine. No asset-specific geometry grammar.
Input vertices/faces are the art source. Operations, material recipes and previews are request driven.
Source review remains a human/agent visual gate; this tool never issues ART SOURCE PASS.
"""
import bpy,bmesh,json,os,sys,math,hashlib,random
from pathlib import Path
from mathutils import Vector
import numpy as np
ROOT=Path(os.environ.get('GITHUB_WORKSPACE',os.getcwd()))
req=json.loads((ROOT/'pipeline/valoria-production-art-starter-run-request.json').read_text())
spec=json.loads((ROOT/req['mesh_spec']).read_text());SRC=ROOT/req['source_dir'];PRE=ROOT/req['evidence_dir'];SRC.mkdir(parents=True,exist_ok=True);PRE.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
def image_data(name,arr):
 h,w,c=arr.shape; im=bpy.data.images.new(name,width=w,height=h,alpha=True);rgba=np.ones((h,w,4),dtype=np.float32);rgba[:,:,:c]=arr
 im.pixels.foreach_set(rgba.reshape(-1));im.filepath_raw=str(SRC/(name+'.png'));im.file_format='PNG';im.save();im.pack();return im
# Tileable metric coursed masonry. Joints/stone height kept coarse enough for landscape/mobile.
def surface_maps(kind):
 n=512;y,x=np.mgrid[0:n,0:n];rng=np.random.default_rng(7105);grain=rng.random((n,n))*.035
 if kind=='masonry':
  row=y//64;xx=(x+((row%2)*64))%128;yy=y%64
  edge=np.minimum.reduce([xx,127-xx,yy,63-yy]).astype(float)
  stonehash=np.sin((row*13+(x+((row%2)*64))//128)*7.16)*.07
  height=np.clip(edge/4,0,1)*.8+grain
  base=.82+stonehash+grain
  base=np.where(edge<2,.40,base);base=np.where((edge>=2)&(edge<4),base*.82,base)
 elif kind=='paving':
  # Large staggered civic paving: broader slabs than wall masonry, readable at gameplay camera.
  row=y//96;shift=(row%2)*64;xx=(x+shift)%128;yy=y%96
  edge=np.minimum.reduce([xx,127-xx,yy,95-yy]).astype(float)
  cell=(x+shift)//128+row*7
  stonehash=np.sin(cell*5.731)*.055+np.cos(cell*2.119)*.025
  height=np.clip(edge/5,0,1)*.72+grain*.7
  base=.84+stonehash+grain*.65
  base=np.where(edge<2,.43,base);base=np.where((edge>=2)&(edge<5),base*.84,base)
 else:
  height=.72+grain; base=.88+grain+np.sin(x*.025)*.025
 albedo=np.repeat(base[:,:,None],3,axis=2)
 # Normal map from actual height, all tile edges periodic.
 dx=(np.roll(height,-1,1)-np.roll(height,1,1))*.9;dy=(np.roll(height,-1,0)-np.roll(height,1,0))*.9
 z=np.ones_like(dx);length=np.sqrt(dx*dx+dy*dy+z*z);normal=np.stack((-dx/length*.5+.5,-dy/length*.5+.5,z/length*.5+.5),2)
 return image_data(kind+'_albedo',albedo),image_data(kind+'_normal',normal)
tex={k:surface_maps(k) for k in ['masonry','stone_grain','paving']}
# Heraldry follows the large gold/blue accent already present in the exact target.
n=256;y,x=np.mgrid[0:n,0:n];u=(x-128)/256;v=(y-128)/256
cloth=np.zeros((n,n,3));cloth[:]=[.023,.087,.25];cloth+=((np.sin(x*.18)*.013+np.cos(y*.09)*.012)[:,:,None])
gold=(abs(u)<.022)&(abs(v)<.21)
gold|=((u/.05)**2+((v-.17)/.085)**2<1)
gold|=((abs(u)-.085)**2/.045**2+(v-.04)**2/.085**2<1)
gold|=(abs(v+.05)<.019)&(abs(u)<.11)
cloth[gold]=[.81,.56,.21];banner=image_data('banner_heraldry',np.clip(cloth,0,1))
materials={}
for key,s in spec['materials'].items():
 m=bpy.data.materials.new('Eldoria_'+key);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=s['color'];p.inputs['Roughness'].default_value=s.get('roughness',.85);p.inputs['Metallic'].default_value=s.get('metallic',0)
 if s.get('texture'):
  k=s['texture'];tx=m.node_tree.nodes.new('ShaderNodeTexImage');tx.image=banner if k=='banner' else tex[k][0]
  # Multiply albedo by shared family tint; glTF export requires explicit image color so tint stays in base factor.
  p.inputs['Base Color'].default_value=s['color'];m.node_tree.links.new(tx.outputs['Color'],p.inputs['Base Color'])
  if k!='banner':
   nt=m.node_tree.nodes.new('ShaderNodeTexImage');nt.image=tex[k][1];nt.image.colorspace_settings.name='Non-Color';nm=m.node_tree.nodes.new('ShaderNodeNormalMap');nm.inputs['Strength'].default_value=.55;m.node_tree.links.new(nt.outputs['Color'],nm.inputs['Color']);m.node_tree.links.new(nm.outputs['Normal'],p.inputs['Normal'])
 materials[key]=m
# Source images include desired warm tint (keep Blender and Unity material response consistent).
for kind,tint in [('masonry',[.61,.55,.46]),('stone_grain',[.70,.64,.54]),('paving',[.66,.60,.51])]:
 im=tex[kind][0];p=np.array(im.pixels[:],dtype=np.float32).reshape(-1,4);p[:,:3]*=np.array(tint);im.pixels.foreach_set(p.reshape(-1));im.save();im.pack()
objs=[]
for item in spec['meshes']:
 data=bpy.data.meshes.new(item['name']);data.from_pydata(item['vertices'],[],item['faces']);data.update();o=bpy.data.objects.new(item['name'],data);bpy.context.collection.objects.link(o);o.data.materials.append(materials[item['material']]);o['source_group']=item['group'];o['authoring_source']='editable_mesh_spec';objs.append(o)
 bpy.context.view_layer.objects.active=o;o.select_set(True)
 bm=bmesh.new();bm.from_mesh(data);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(data);bm.free()
 for cutter in item.get('cutters',[]):
  lo=Vector(cutter['min']);hi=Vector(cutter['max']);bpy.ops.mesh.primitive_cube_add(size=1,location=(lo+hi)/2);c=bpy.context.object;c.scale=hi-lo;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
  mod=o.modifiers.new('Authored embrasure','BOOLEAN');mod.operation='DIFFERENCE';mod.object=c;mod.solver='EXACT';bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name);bpy.data.objects.remove(c,do_unlink=True)
 if item.get('bevel',0)>0:
  mod=o.modifiers.new('Selective structural edge bevel','BEVEL');mod.width=item['bevel'];mod.segments=2;mod.limit_method='ANGLE';bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
 # Discard modifier-created attribute layers before deliberate UV/material assignment.
 old=o.data;clean=bpy.data.meshes.new(item['name']+'_authored')
 clean.from_pydata([tuple(v.co) for v in old.vertices],[],[list(p.vertices) for p in old.polygons]);clean.update();o.data=clean;clean.materials.append(materials[item['material']])
 bm=bmesh.new();bm.from_mesh(clean);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(clean);bm.free()
 for poly in clean.polygons:poly.material_index=0
 uv=o.data.uv_layers.new(name='MetricUV');o.data.uv_layers.active=uv;uv.active_render=True
 for poly in o.data.polygons:
  axis=max(range(3),key=lambda k:abs(poly.normal[k]))
  for li in poly.loop_indices:
   vi=o.data.loops[li].vertex_index;v=o.data.vertices[vi].co
   uv.data[li].uv=item['uv'][vi] if item.get('uv') and vi<len(item['uv']) else ((v.y/4,v.z/4) if axis==0 else (v.x/4,v.z/4) if axis==1 else (v.x/4,v.y/4))
 o.select_set(False)

# Optional zero-credit geometric donor relief. This is deliberately guarded by the family spec:
# it may enrich one authored module but never replaces the locked family macro silhouette.
donor=spec.get('donor_relief')
if donor:
 donor_path=ROOT/donor['source_glb']
 if not donor_path.is_file(): raise RuntimeError('Bastion donor GLB missing: '+str(donor_path))
 before=set(bpy.data.objects)
 bpy.ops.import_scene.gltf(filepath=str(donor_path))
 imported=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
 if not imported: raise RuntimeError('Donor GLB imported no mesh objects')
 world_pts=[]
 for io in imported:
  world_pts.extend([io.matrix_world@v.co for v in io.data.vertices])
 dlo=Vector([min(p[i] for p in world_pts) for i in range(3)])
 dhi=Vector([max(p[i] for p in world_pts) for i in range(3)])
 dext=dhi-dlo
 crop=donor['crop_normalized'];target=donor['target_bbox'];exclusions=donor.get('exclude_target_boxes',[])
 verts=[];faces=[];vmap={}
 def inside(n):
  return crop['x'][0]<=n[0]<=crop['x'][1] and crop['y'][0]<=n[1]<=crop['y'][1] and crop['z'][0]<=n[2]<=crop['z'][1]
 def remap(n,axis):
  a0,a1=crop[axis];t0,t1=target[axis]
  q=0 if abs(a1-a0)<1e-8 else (n-a0)/(a1-a0)
  return t0+q*(t1-t0)
 def excluded_target(points):
  if not exclusions:return False
  cx=sum(p[0] for p in points)/len(points);cy=sum(p[1] for p in points)/len(points);cz=sum(p[2] for p in points)/len(points)
  return any(b['x'][0]<=cx<=b['x'][1] and b['y'][0]<=cy<=b['y'][1] and b['z'][0]<=cz<=b['z'][1] for b in exclusions)
 for oi,io in enumerate(imported):
  for poly in io.data.polygons:
   ids=list(poly.vertices);norms=[]
   for vi in ids:
    p=io.matrix_world@io.data.vertices[vi].co
    norms.append(((p.x-dlo.x)/dext.x,(p.y-dlo.y)/dext.y,(p.z-dlo.z)/dext.z))
   if not all(inside(n) for n in norms):continue
   mapped=[[remap(n[0],'x'),remap(n[1],'y'),remap(n[2],'z')] for n in norms]
   if excluded_target(mapped):continue
   out=[]
   for vi,n,p in zip(ids,norms,mapped):
    key=(oi,vi)
    if key not in vmap:vmap[key]=len(verts);verts.append(p)
    out.append(vmap[key])
   if len(out)>=3:
    for k in range(1,len(out)-1):faces.append([out[0],out[k],out[k+1]])
 for io in imported:bpy.data.objects.remove(io,do_unlink=True)
 if len(faces)<500:raise RuntimeError('Donor crop produced insufficient relief geometry: '+str(len(faces)))
 data=bpy.data.meshes.new(donor['name']);data.from_pydata(verts,[],faces);data.update()
 o=bpy.data.objects.new(donor['name'],data);bpy.context.collection.objects.link(o)
 o.data.materials.append(materials[donor['material']]);o['source_group']=donor['group'];o['authoring_source']='certified_zero_credit_geometry_donor_crop';objs.append(o)
 bm=bmesh.new();bm.from_mesh(data);bmesh.ops.recalc_face_normals(bm,faces=bm.faces)
 if donor.get('close_boundaries',False):
  boundary=[e for e in bm.edges if e.is_boundary]
  if boundary:bmesh.ops.holes_fill(bm,edges=boundary,sides=0)
  bmesh.ops.recalc_face_normals(bm,faces=bm.faces)
 bm.to_mesh(data);bm.free();data.update()
 if donor.get('decimate_ratio',1)<.999:
  mod=o.modifiers.new('Donor relief simplification','DECIMATE');mod.ratio=float(donor['decimate_ratio']);mod.use_collapse_triangulate=True
  bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
 uv=o.data.uv_layers.new(name='MetricUV');o.data.uv_layers.active=uv;uv.active_render=True
 for poly in o.data.polygons:
  axis=max(range(3),key=lambda k:abs(poly.normal[k]))
  for li in poly.loop_indices:
   vi=o.data.loops[li].vertex_index;v=o.data.vertices[vi].co
   uv.data[li].uv=((v.y/3,v.z/3) if axis==0 else (v.x/3,v.z/3) if axis==1 else (v.x/3,v.y/3))
 o.select_set(False)
 print('DONOR_RELIEF_AUTHORED',donor['name'],'verts',len(o.data.vertices),'polys',len(o.data.polygons),'source',donor['source_glb'],'closed',donor.get('close_boundaries',False),'decimate',donor.get('decimate_ratio',1))

# Join by semantic module, retain per-material slots; reusable modules have an origin at foundation.
groups={}
for o in objs:groups.setdefault(o['source_group'],[]).append(o)
joined=[]
for key,group in groups.items():
 bpy.ops.object.select_all(action='DESELECT')
 for o in group:o.select_set(True)
 bpy.context.view_layer.objects.active=group[0];bpy.ops.object.join();o=bpy.context.object;o.name=key;joined.append(o)
 # Blender join already shares identical material datablocks. Validate rather than remap in-place.
 assert all(p.material_index<len(o.data.materials) for p in o.data.polygons), 'Invalid source material index'
 assert len(o.data.uv_layers)==1, 'Exactly one authored UV channel required'
 bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.triangulate(bm,faces=bm.faces);bm.to_mesh(o.data);bm.free()
 bpy.context.scene.cursor.location=spec['export_origin'];bpy.ops.object.origin_set(type='ORIGIN_CURSOR');o.select_set(False)
# Save artist source in its inherited world placement; export deterministic local family pivot.
blend=SRC/(spec['family']+'.blend');bpy.ops.wm.save_as_mainfile(filepath=str(blend))
origin=Vector(spec['export_origin'])
for o in joined:o.location-=origin
bpy.ops.object.select_all(action='DESELECT')
for o in joined:o.select_set(True)
glb=SRC/(spec['family']+'.glb');bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True,export_yup=True,export_materials='EXPORT',export_normals=True,export_tangents=True)
for o in joined:o.location+=origin
bpy.context.view_layer.update()
# Source sanity: degeneracy/nonmanifold are measured, never an art verdict.
verts=tris=0;nm_edges=0;bounds=[]
for o in joined:
 o.data.calc_loop_triangles();verts+=len(o.data.vertices);tris+=len(o.data.loop_triangles)
 bm=bmesh.new();bm.from_mesh(o.data);nm_edges+=sum(not e.is_manifold for e in bm.edges);bm.free();bounds.extend([o.matrix_world@Vector(v) for v in o.bound_box])
lo=[min(p[i] for p in bounds) for i in range(3)];hi=[max(p[i] for p in bounds) for i in range(3)]
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=40;scene.cycles.use_denoising=False;scene.render.resolution_x=1200;scene.render.resolution_y=800;scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new('Neutral source review');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.17,.19,.22,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.6
scene.view_settings.view_transform='Standard';scene.view_settings.look='Medium High Contrast';scene.view_settings.exposure=0;scene.view_settings.gamma=1
ctr=Vector(spec['preview_center']);span=spec['preview_span']
for pos,power,size in [((-.8,-1.1,1.7),2200,12),((1,.5,1),1500,10)]:
 bpy.ops.object.light_add(type='AREA',location=ctr+Vector(pos)*span*.6);light=bpy.context.object;light.data.energy=power;light.data.size=size;light.rotation_euler=(ctr-light.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type='ORTHO';cam.data.ortho_scale=span;scene.camera=cam
clay=bpy.data.materials.new('Review clay');clay.diffuse_color=(.58,.55,.50,1)
previews=[]
for view in spec['preview_views']:
 cam.location=ctr+Vector(view['direction']).normalized()*60;cam.rotation_euler=(ctr-cam.location).to_track_quat('-Z','Y').to_euler();scene.view_layers[0].material_override=clay if view.get('clay') or view.get('wire') else None
 # Wire diagnostic overlay generated with render-only wire modifier on duplicated meshes.
 wires=[]
 if view.get('wire'):
  dark=bpy.data.materials.new('Topology dark');dark.diffuse_color=(.015,.02,.025,1)
  for o in joined:
   cp=o.copy();cp.data=o.data.copy();bpy.context.collection.objects.link(cp);cp.data.materials.clear();cp.data.materials.append(dark);mod=cp.modifiers.new('wire diagnostic','WIREFRAME');mod.thickness=.012;mod.use_replace=True;wires.append(cp)
  scene.view_layers[0].material_override=None
 scene.render.filepath=str(PRE/(view['name']+'.png'));bpy.ops.render.render(write_still=True);previews.append(str(Path(req['evidence_dir'])/(view['name']+'.png')))
 for o in wires:bpy.data.objects.remove(o,do_unlink=True)
sha=lambda p:hashlib.sha256(Path(p).read_bytes()).hexdigest()
report={'authoring_standard':'BLENDER_PROFESSIONAL_V1','authoring_method':'Editable designed profile/loft/arch meshes from locked family spec; DCC boolean embrasure, selective bevel, metric UV, tileable masonry normal/color; semantic consolidation. No asset generator.','tool_families':['mesh_edit','extrude','inset','curves_profiles','controlled_boolean','selective_bevel','uv_unwrap_texel_density'],'primitive_role':'Boolean cutters only; primary architecture is explicit mesh/profile source','source_sha':sha(blend),'export_sha':sha(glb),'source':str(blend.relative_to(ROOT)),'export':str(glb.relative_to(ROOT)),'geometry_metrics':{'triangles':tris,'vertices':verts,'modules':len(joined),'materials':len(materials),'bounds_world_blender':[lo,hi],'uv':True,'normals':True,'tangents_exported':True,'nonmanifold_edges':nm_edges,'nonmanifold_explanation':'Open cloth border and arch pieces mating at internal interfaces; no collision source'},'material_families':list(materials),'texture_sets':{**{k:[512,512,'albedo+normal'] for k in tex},'fabric':[256,256,'albedo']},'preview_evidence':previews,'isolated_art_review':'PENDING; no automatic artistic acceptance','intended_camera_role':'Approved orthographic20/35/24; exact lower entry placeholder replacement only','tripo_credits':0,'blender_version':bpy.app.version_string,'export_origin':spec['export_origin'],'unity_placement':{'position':[origin.x,origin.z,origin.y],'rotation_y':180,'scale':1}}
(PRE/'source-report.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report,indent=2))
