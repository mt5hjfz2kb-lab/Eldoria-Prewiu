"""Artist-directed connected cutter, rounded banks and quiet horizontal strata.
Replaces the rejected sampled-grid cleft, retaining the accepted low uplands.
"""
import bpy,bmesh,numpy as np,math,pathlib,json,hashlib,struct,gzip
from mathutils import Vector,Matrix
from mathutils.bvhtree import BVHTree
out=pathlib.Path(bpy.app.driver_namespace['interactive_output']);source=pathlib.Path(bpy.app.driver_namespace['interactive_source']);scene=bpy.context.scene
terrain=bpy.data.objects['BLD_Terrain'];old=terrain.data
raw=(source.parent/'RegionOneBlenderWorld.glb').read_bytes();export_names={n.get('name') for n in json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])['nodes']}
layout=json.loads((source.parent/'production-region.json').read_text(encoding='utf-8-sig'));faults=layout['faults']
query=gzip.decompress((source.parent/'RegionOneBlenderQueries.bytes').read_bytes());magic,nx,nz,width,depth=struct.unpack_from('<iiiff',query);count=nx*nz
heights=np.frombuffer(query,dtype='<f4',count=count,offset=20).reshape(nz,nx).copy();masks=np.frombuffer(query,dtype=np.uint8,count=count*4,offset=20+count*4).reshape(nz,nx,4).copy()
images_before={im.name:hashlib.sha256(im.packed_file.data).hexdigest() for im in bpy.data.images if im.packed_file}
def smooth(t):t=max(0,min(1,t));return t*t*(3-2*t)
def sample(a,x,y):
 h,w=a.shape[:2];u=np.clip((np.asarray(x)+910)/1820*(w-1),0,w-1);v=np.clip((np.asarray(y)+1800)/3600*(h-1),0,h-1);ix=u.astype(int);iy=v.astype(int);jx=np.minimum(w-1,ix+1);jy=np.minimum(h-1,iy+1);tx=u-ix;ty=v-iy
 return (a[iy,ix]*(1-tx)+a[iy,jx]*tx)*(1-ty)+(a[jy,ix]*(1-tx)+a[jy,jx]*tx)*ty
def nearest(x,y):
 best=1e10;answer=None
 for f in faults:
  ax,ay,bx,by=f['ax'],f['az'],f['bx'],f['bz'];dx=bx-ax;dy=by-ay;t=max(0,min(1,((x-ax)*dx+(y-ay)*dy)/(dx*dx+dy*dy)));px=ax+t*dx;py=ay+t*dy;d=math.hypot(x-px,y-py)
  if d<best:best=d;answer=(d,px,py,-dy/math.hypot(dx,dy),dx/math.hypot(dx,dy))
 return answer
protected_positions=sorted(tuple(v.co) for v in old.vertices if abs(v.co.x)<=96.001 and -78.001<=v.co.y<=270.001)
patch_faces=[p for p in old.polygons if all(-470<v.co.x<-98 and -20<v.co.y<320 for v in (old.vertices[i] for i in p.vertices))]
patch_ids={p.index for p in patch_faces};used=sorted({i for p in patch_faces for i in p.vertices});remap={i:k for k,i in enumerate(used)}
vertices=[]
for i in used:
 q=old.vertices[i].co;d,px,py,nxv,nyv=nearest(q.x,q.y);weight=1-smooth((d-20)/12)
 bank=(float(sample(heights,q.x+nxv*43,q.y+nyv*43))+float(sample(heights,q.x-nxv*43,q.y-nyv*43)))*.5
 vertices.append((q.x,q.y,q.z*(1-weight)+bank*weight))
mesh=bpy.data.meshes.new('Continuous geological modelling patch');mesh.from_pydata(vertices,[],[[remap[i] for i in p.vertices] for p in patch_faces]);mesh.update()
# Quiet rock with deposited bands in physical elevation, never stretched slab UV.
rock=bpy.data.materials.new('Breach fractured basalt continuous geological wall');rock.diffuse_color=(.22,.235,.25,1);rock.use_nodes=True
nodes=rock.node_tree.nodes;nodes.clear();bsdf=nodes.new('ShaderNodeBsdfPrincipled');output=nodes.new('ShaderNodeOutputMaterial');rock.node_tree.links.new(bsdf.outputs['BSDF'],output.inputs['Surface']);bsdf.inputs['Base Color'].default_value=(.22,.235,.25,1);bsdf.inputs['Roughness'].default_value=.9
geometry=nodes.new('ShaderNodeNewGeometry');separate=nodes.new('ShaderNodeSeparateXYZ');multiply=nodes.new('ShaderNodeMath');multiply.operation='MULTIPLY';multiply.inputs[1].default_value=math.tau/2.2;sine=nodes.new('ShaderNodeMath');sine.operation='SINE';ramp=nodes.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].position=.12;ramp.color_ramp.elements[0].color=(.17,.18,.195,1);ramp.color_ramp.elements[1].position=.85;ramp.color_ramp.elements[1].color=(.26,.275,.285,1)
links=rock.node_tree.links;links.new(geometry.outputs['Position'],separate.inputs[0]);links.new(separate.outputs['Z'],multiply.inputs[0]);links.new(multiply.outputs[0],sine.inputs[0]);links.new(sine.outputs[0],ramp.inputs[0]);links.new(ramp.outputs['Color'],bsdf.inputs['Base Color'])
soil_material=old.materials[0];temporary_bottom=bpy.data.materials.new('Temporary modeller underside');temporary_rim=bpy.data.materials.new('Temporary modeller exterior')
for m in [soil_material,temporary_bottom,temporary_rim,rock]:mesh.materials.append(m)
layer=mesh.uv_layers.new(name='UVMap')
for p in mesh.polygons:
 for li in p.loop_indices:
  q=mesh.vertices[mesh.loops[li].vertex_index].co;layer.data[li].uv=((q.x+910)/1820,(q.y+1800)/3600)
patch=bpy.data.objects.new('Temporary connected canyon authoring patch',mesh);scene.collection.objects.link(patch)
# The previous sampled cleft contained coincident projected edges. Weld only
# this detached, wholly unprotected patch before creating a closed volume.
bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.0001);bmesh.ops.dissolve_degenerate(bm,edges=list(bm.edges),dist=.00001);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free();mesh.update()
def apply(ob,modifier):
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob;bpy.ops.object.modifier_apply(modifier=modifier.name)
solid=patch.modifiers.new('Temporary closed modelling volume','SOLIDIFY');solid.thickness=48;solid.offset=-1;solid.material_offset=1;solid.material_offset_rim=2;apply(patch,solid)
# Hand-directed branching capsule loft. A voxel union rounds the joins before
# the exact cut, so the terrain grid cannot dictate the silhouette of the wound.
cv=[];cf=[]
for index,f in enumerate(faults):
 ax,ay,bx,by=f['ax'],f['az'],f['bx'],f['bz'];angle=math.atan2(by-ay,bx-ax);rings=[]
 for z,radius in [(-13.1,4.5),(-7,7.5),(2,12),(16,14.2),(36,16)]:
  ring=[]
  for cx,cy,start in [(bx,by,angle-math.pi/2),(ax,ay,angle+math.pi/2)]:
   for j in range(8):
    theta=start+math.pi*j/7;variation=1+.07*math.sin(cx*.047+cy*.061+theta*2)+.04*math.sin(theta*5+index)
    ring.append(len(cv));cv.append((cx+math.cos(theta)*radius*variation,cy+math.sin(theta)*radius*variation,z+.35*math.sin(cx*.021+cy*.025)))
  rings.append(ring)
 for a,b in zip(rings,rings[1:]):
  for j in range(len(a)):k=(j+1)%len(a);cf.append((a[j],a[k],b[k],b[j]))
 cf.extend([tuple(reversed(rings[0])),tuple(rings[-1])])
cm=bpy.data.meshes.new('Hand-shaped branching geological cutter');cm.from_pydata(cv,[],cf)
for m in mesh.materials:cm.materials.append(m)
for p in cm.polygons:p.material_index=3
cm.update();cutter=bpy.data.objects.new('Temporary continuous fracture cutter',cm);scene.collection.objects.link(cutter)
remesh=cutter.modifiers.new('Union and soften the branching wound','REMESH');remesh.mode='VOXEL';remesh.voxel_size=1.1;remesh.use_smooth_shade=True;apply(cutter,remesh)
soften=cutter.modifiers.new('Natural eroded cut transitions','SMOOTH');soften.factor=1;soften.iterations=4;apply(cutter,soften)
for p in cutter.data.polygons:p.material_index=3
cut=patch.modifiers.new('Continuous geological incision','BOOLEAN');cut.operation='DIFFERENCE';cut.solver='EXACT';cut.object=cutter;apply(patch,cut)
bpy.data.objects.remove(cutter,do_unlink=True)
# Remove only the temporary modelling underside/exterior, leaving the actual
# cut wall and floor connected to the original physical ground surface.
bm=bmesh.new();bm.from_mesh(patch.data);remove=[p for p in bm.faces if p.material_index in {1,2}];bmesh.ops.delete(bm,geom=remove,context='FACES');bmesh.ops.delete(bm,geom=[v for v in bm.verts if not v.link_faces],context='VERTS');bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(patch.data);bm.free();patch.data.update()
assert len(patch.data.polygons)>1000 and any(p.material_index==3 for p in patch.data.polygons)
# A small continuous central basin preserves the established lowest elevation,
# so composition is assessed with the exact original world camera bounds.
lowest=min(v.co.z for v in old.vertices);assert min(v.co.z for v in patch.data.vertices)>=lowest-.01
basin=[(v,math.exp(-((v.co.x+260)**2+(v.co.y-150)**2)/(14**2))) for v in patch.data.vertices if v.co.z<-7 and math.hypot(v.co.x+260,v.co.y-150)<42]
assert len(basin)>5
lo,hi=0.,20.
for unused in range(45):
 mid=(lo+hi)*.5
 if min(v.co.z-mid*w for v,w in basin)>lowest:lo=mid
 else:hi=mid
for v,w in basin:v.co.z-=hi*w
min((v for v,w in basin),key=lambda v:v.co.z).co.z=lowest
patch.data.update()

# Reassemble without rebuilding the protected faces or their UV coordinates.
materials=list(old.materials)
if rock not in materials:materials.append(rock)
rock_index=materials.index(rock);new_vertices=[];new_faces=[];face_uv=[];face_normals=[];face_material=[];face_smooth=[];mapping={}
for p in old.polygons:
 if p.index in patch_ids:continue
 ids=[]
 for i in p.vertices:
  if i not in mapping:mapping[i]=len(new_vertices);new_vertices.append(tuple(old.vertices[i].co))
  ids.append(mapping[i])
 new_faces.append(ids);face_uv.append([tuple(old.uv_layers.active.data[li].uv) for li in p.loop_indices]);face_normals.append([tuple(old.corner_normals[li].vector) for li in p.loop_indices]);face_material.append(p.material_index);face_smooth.append(p.use_smooth)
offset=len(new_vertices);new_vertices.extend(tuple(v.co) for v in patch.data.vertices)
for p in patch.data.polygons:
 new_faces.append([offset+i for i in p.vertices]);face_uv.append([((patch.data.vertices[patch.data.loops[li].vertex_index].co.x+910)/1820,(patch.data.vertices[patch.data.loops[li].vertex_index].co.y+1800)/3600) for li in p.loop_indices]);face_normals.append([tuple(patch.data.corner_normals[li].vector) for li in p.loop_indices]);face_material.append(rock_index if p.material_index==3 else 0);face_smooth.append(True)
combined=bpy.data.meshes.new('Authored low uplands with continuous rounded geological incision');combined.from_pydata(new_vertices,[],new_faces)
for m in materials:combined.materials.append(m)
uv=combined.uv_layers.new(name='UVMap');normals=[]
for p,texcoord,normal,material,smooth_face in zip(combined.polygons,face_uv,face_normals,face_material,face_smooth):
 p.material_index=material;p.use_smooth=smooth_face
 for li,tc in zip(p.loop_indices,texcoord):uv.data[li].uv=tc
 normals.extend(normal)
combined.update();combined.normals_split_custom_set(normals);old.use_fake_user=True;terrain.data=combined;bpy.data.objects.remove(patch,do_unlink=True);bpy.context.view_layer.update()
assert sorted(tuple(v.co) for v in combined.vertices if abs(v.co.x)<=96.001 and -78.001<=v.co.y<=270.001)==protected_positions
bvh=BVHTree.FromObject(terrain,bpy.context.evaluated_depsgraph_get())
def soil(x,y):
 q=bvh.ray_cast(Vector((float(x),float(y),400)),Vector((0,0,-1)))[0];assert q is not None,(x,y);return q.z
assert soil(-260,150)<-4
# Clear only decorative crowns from the newly readable cut and ground their pair.
regrounded=0;relocated=0
for ob in list(scene.objects):
 if ob.name not in export_names or not ob.name.startswith('BLD_ForestMass_'):continue
 a,b=ob.location.x,ob.location.y
 if not(-470<a<-98 and -20<b<320):continue
 d,px,py,nxv,nyv=nearest(a,b);oldz=ob.location.z
 if d<16:
  sign=1 if (a-px)*nxv+(b-py)*nyv>=0 else -1;a=px+sign*nxv*23;b=py+sign*nyv*23;ob.location.x=a;ob.location.y=b;relocated+=1
 ob.location.z=soil(a,b);regrounded+=1;far=bpy.data.objects.get(ob.name.replace('BLD_ForestMass_','BLD_ForestFar_',1))
 if far:far.location.x=a;far.location.y=b;far.location.z+=ob.location.z-oldz
# Repaint the flat settlement reserves with surrounding soil rather than yellow
# circles. These remain logically reserved, accessible and geometrically flat.
pigment=next(n.image for n in soil_material.node_tree.nodes if n.type=='TEX_IMAGE' and n.image);W,H=pigment.size;pixels=np.empty(len(pigment.pixels),np.float32);pigment.pixels.foreach_get(pixels);pixels=pixels.reshape(H,W,4)
for cx,cy,r in layout['city_reserves']:
 radius=r+64;x0=max(1,int((cx-radius+910)/1820*(W-1)));x1=min(W-1,int((cx+radius+910)/1820*(W-1))+1);y0=max(1,int((cy-radius+1800)/3600*(H-1)));y1=min(H-1,int((cy+radius+1800)/3600*(H-1))+1)
 X,Y=np.meshgrid(np.linspace(-910,910,W)[x0:x1],np.linspace(-1800,1800,H)[y0:y1]);distance=np.hypot(X-cx,Y-cy);inside=distance<r+53
 patch=pixels[y0:y1,x0:x1,:3].copy();ring=(distance>r+55)&(distance<r+63);mean=patch[ring].mean(axis=0) if ring.any() else patch.mean(axis=(0,1));patch[inside]=mean
 for unused in range(180):
  padded=np.pad(patch,((1,1),(1,1),(0,0)),mode='edge');average=(padded[:-2,1:-1]+padded[2:,1:-1]+padded[1:-1,:-2]+padded[1:-1,2:])*.25;patch[inside]=average[inside]
 pixels[y0:y1,x0:x1,:3]=patch
image=bpy.data.images.new('Continuous living geography without settlement circles',W,H,alpha=True);image.pixels.foreach_set(pixels.ravel());image.filepath_raw=str(out/'ContinuousLivingGeography.png');image.file_format='PNG';image.save();packed=bpy.data.images.load(image.filepath_raw,check_existing=False);packed.pack()
for m in bpy.data.materials:
 if m.use_nodes:
  for n in m.node_tree.nodes:
   if n.type=='TEX_IMAGE' and n.image==pigment:n.image=packed
# Geological masks retain the broad original footprint. Walls are rendered by
# the physical terrain itself; no reprojected plate is allowed to bridge them.
footprints={}
for name in ['BLD_BreachFloor','BLD_BreachOutlyingFractures']:
 ob=bpy.data.objects[name];footprints[name]=BVHTree.FromPolygons([(v.co.x,v.co.y,0) for v in ob.data.vertices],[list(p.vertices) for p in ob.data.polygons],all_triangles=False)
candidates=[p for p in combined.polygons if -450<p.center.x<-110 and 5<p.center.y<290]
def copy_ground(name,faces,offset):
 vv=[];ff=[];uvs=[];mat=[]
 for p in faces:
  face=[]
  for li in p.loop_indices:
   q=combined.vertices[combined.loops[li].vertex_index].co;face.append(len(vv));vv.append((q.x,q.y,soil(q.x,q.y)+offset));uvs.append(tuple(uv.data[li].uv))
  ff.append(face);mat.append(p.material_index)
 assert len(ff)>10,(name,len(ff))
 ob=bpy.data.objects[name];ob.data.use_fake_user=True;m=bpy.data.meshes.new(name+' actual connected ground exposure');m.from_pydata(vv,[],ff)
 for material in combined.materials:m.materials.append(material)
 layer=m.uv_layers.new(name='UVMap')
 for p,index in zip(m.polygons,mat):p.material_index=index;p.use_smooth=True
 for i,tc in enumerate(uvs):layer.data[i].uv=tc
 m.update();ob.data=m
for name,mask in footprints.items():
 faces=[]
 for p in candidates:
  if p.normal.z<.15 or mask.ray_cast(Vector((p.center.x,p.center.y,40)),Vector((0,0,-1)),80)[0] is None:continue
  if all(abs(soil(combined.vertices[i].co.x,combined.vertices[i].co.y)-combined.vertices[i].co.z)<.003 for i in p.vertices):faces.append(p)
 copy_ground(name,faces,.015)
for name,lo,hi in [('BLD_BreachBrokenBasaltPlates',10,15),('BLD_BreachTerracedStrata',15,21),('BLD_BreachWeatheredRockfall',21,29)]:
 faces=[p for p in candidates if p.normal.z>.4 and lo<nearest(p.center.x,p.center.y)[0]<hi and all(abs(soil(combined.vertices[i].co.x,combined.vertices[i].co.y)-combined.vertices[i].co.z)<.003 for i in p.vertices)]
 copy_ground(name,faces,.026)
# Rare mineral vents and their real interior light anchors follow the new floor.
core=bpy.data.objects['BLD_BreachMineralHeart']
for v in core.data.vertices:v.co.z=soil(v.co.x,v.co.y)+.034
for i in range(3):
 anchor=bpy.data.objects['BLD_BreachInteriorLightAnchor_'+str(i)];anchor.location.z=soil(anchor.location.x,anchor.location.y)+1.4
 lamp=bpy.data.objects.get('ArtistBreachInterior_'+str(i))
 if lamp:lamp.location=anchor.location
# Query heights and the full verification vertex tail derive from this mesh.
for j in range(nz):
 y=-1800+j*3600/(nz-1)
 if not(-20<y<320):continue
 for i in range(nx):
  x=-910+i*1820/(nx-1)
  if -470<x<-98:heights[j,i]=soil(x,y)
coords=np.array([tuple(v.co) for v in combined.vertices],np.float32)
with gzip.open(out/'RegionOneBlenderQueries.bytes','wb') as file:
 file.write(struct.pack('<iiiff',magic,nx,nz,width,depth));file.write(heights.astype('<f4').tobytes());file.write(masks.tobytes());file.write(struct.pack('<ii',0x32564c45,len(coords)));file.write(coords[:,[0,2,1]].astype('<f4').tobytes())
layout['terrain_vertices']=len(coords);layout['living_geography']['continuous_canyon_boolean']=True;layout['living_geography']['settlement_colour_circles_removed']=True;(out/'production-region.json').write_text(json.dumps(layout,indent=2))
assert all(hashlib.sha256(bpy.data.images[name].packed_file.data).hexdigest()==value for name,value in images_before.items())
bpy.ops.object.select_all(action='DESELECT')
for ob in scene.objects:
 if ob.name in export_names and ob.type in {'MESH','EMPTY'}:ob.select_set(True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'),compress=True);bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneBlenderWorld.glb'),export_format='GLB',use_selection=True)
report={'method':'actual BlenderMCP continuous cutter and rounded-bank geological modelling','source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'terrain_vertices_after':len(coords),'protected_vertices_unchanged':len(protected_positions),'original_packed_images_unchanged':len(images_before),'rejected_approach':'sampled-heightfield cleft made grid-stepped walls and stretched slate UV','retained_upland_spines':4,'continuous_canyon_boolean':True,'canyon_cut_faces':sum(p.material_index==rock_index for p in combined.polygons),'decorative_crowns_regrounded':regrounded,'decorative_crowns_cleared_from_wound':relocated,'interior_light_anchors':3,'new_gameplay_targets':0,'new_colliders':0,'files':{name:hashlib.sha256((out/name).read_bytes()).hexdigest() for name in ['Eldoria_Region1_Production.blend','RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes','production-region.json']},'art_approved':False};(out/'macro-art-operations.json').write_text(json.dumps(report,indent=2))
# Preview the same strategic mesh envelopes used by the runtime.
helpers=pathlib.Path(bpy.app.driver_namespace['interactive_helpers']);text=(helpers.parent/'sculpt_living_geography.py').read_text(encoding='utf-8');preview=text.split("bpy.app.driver_namespace['artist_keep_interior_lights']=True",1)[1];bpy.app.driver_namespace['artist_keep_interior_lights']=True
exec(compile(preview,str(helpers.parent/'sculpt_living_geography.py')+'-review','exec'))
