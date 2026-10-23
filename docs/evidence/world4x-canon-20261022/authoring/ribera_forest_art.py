"""F: reshape inherited pine crowns into readable silhouettes; keep all world positions."""
import bpy,sys,pathlib,hashlib,json,math
import numpy as np
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);src=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene
assert hashlib.sha256(src.read_bytes()).hexdigest()=='337cb6d79f6a1c13988a8d70c81d5e5f23d5a40d32e5546f8cbe07bf702715a1'
assert scene.get('finishing_stage','').startswith('E connected meadow')
trees=sorted([o for o in scene.objects if o.name.startswith('Pilot_Pine')],key=lambda o:o.name);original=trees[0].data
# Preserve the exact inherited source as an editable hidden master; reuse its crown and trunk.
archive=bpy.data.collections.new('07_Pine_source_and_variants');scene.collection.children.link(archive)
master=bpy.data.objects.new('Inherited pine before crown sculpt',original.copy());archive.objects.link(master);master.hide_render=True;master.hide_viewport=True
needle=[i for i,m in enumerate(original.materials) if m and m.name.startswith('Needles')]
assert needle,'Inherited pine foliage material slots not found'
def subset(name,leaf):
 faces=[p for p in original.polygons if (p.material_index in needle)==leaf];ids=sorted({v for p in faces for v in p.vertices});index={v:i for i,v in enumerate(ids)}
 mesh=bpy.data.meshes.new(name);mesh.from_pydata([original.vertices[i].co[:] for i in ids],[],[[index[v] for v in p.vertices] for p in faces]);mesh.update();return mesh
trunk=subset('Inherited trunk',False);trunk.materials.append(next(m for m in original.materials if m and 'bark' in m.name.lower()))
leafmat=original.materials[needle[1 if len(needle)>1 else 0]]
variants=[];stats=[]
for k,(width,height,bend,voxel) in enumerate([(1.12,.98,.28,.22),(.92,1.10,-.32,.20),(1.25,.83,.42,.24)]):
 mesh=subset('Crown sculpt '+str(k),True);mesh.materials.append(leafmat)
 for v in mesh.vertices:
  z=v.co.z;t=max(0,min(1,(z-.9)/5.5));v.co.x=v.co.x*width+bend*t*t;v.co.y=v.co.y*(1.0 if k==0 else .9)+bend*.35*t;v.co.z=.9+(z-.9)*height
 o=bpy.data.objects.new('Pine crown author '+str(k),mesh);scene.collection.objects.link(o);bpy.context.view_layer.objects.active=o;o.select_set(True)
 rem=o.modifiers.new('Merge fine lobes into crown masses','REMESH');rem.mode='VOXEL';rem.voxel_size=voxel;rem.use_smooth_shade=True;bpy.ops.object.modifier_apply(modifier=rem.name)
 smooth=o.modifiers.new('Sculpt broad crown surfaces','SMOOTH');smooth.factor=.8;smooth.iterations=3;bpy.ops.object.modifier_apply(modifier=smooth.name)
 dec=o.modifiers.new('Strategic crown simplification','DECIMATE');dec.ratio=.55;bpy.ops.object.modifier_apply(modifier=dec.name)
 for p in o.data.polygons:p.use_smooth=True;p.material_index=0
 # Join the retained trunk without remeshing away its narrow silhouette.
 verts=[v.co[:] for v in o.data.vertices];faces=[tuple(p.vertices) for p in o.data.polygons];count=len(faces);offset=len(verts);verts.extend(v.co[:] for v in trunk.vertices);faces.extend(tuple(offset+i for i in p.vertices) for p in trunk.polygons)
 data=bpy.data.meshes.new('Pine family '+str(k));data.from_pydata(verts,[],faces);data.materials.append(leafmat);data.materials.append(trunk.materials[0]);data.update()
 for i,p in enumerate(data.polygons):p.material_index=0 if i<count else 1;p.use_smooth=i<count
 variants.append(data);stats.append({'family':k,'vertices':len(verts),'faces':len(faces),'voxel':voxel});bpy.data.objects.remove(o,do_unlink=True)
for i,o in enumerate(trees):
 # Art-directed family mix follows local group: tall anchors, broad edge trees and a quieter middle.
 family=(i*7+int(abs(o.location.x)//9))%3;o.data=variants[family];o['crown_family']=family
S=2048;rgb=np.ones((S,S,4),np.float32);rgb[:,:,:3]=np.array((187,154,88))/255
paint=np.zeros((S,S),np.float32)
# Three distinct, irregular brush silhouettes, used to connect grass masses, not scatter noise.
shapes=[ [(-1,-.3),(-.7,-.8),(-.2,-.65),(.15,-.95),(.55,-.65),(.9,-.7),(1,-.1),(.7,.15),(.9,.6),(.35,.55),(.1,.85),(-.35,.6),(-.85,.7),(-.6,.1)], [(-.9,-.5),(-.4,-.8),(.1,-.55),(.65,-.7),(.85,-.2),(.55,.1),(1,.45),(.5,.8),(0,.6),(-.3,.95),(-.7,.4),(-1,.1)], [(-1,-.2),(-.8,-.7),(-.25,-.4),(.1,-.7),(.4,-.2),(.9,-.4),(1,.2),(.55,.45),(.2,.9),(-.15,.5),(-.55,.7),(-.65,.15)] ]
def brush(cx,cy,rx,ry,angle,shape,colour,opacity=1):
 ca=math.cos(angle);sa=math.sin(angle);controls=[(cx+x*rx*ca-y*ry*sa,cy+x*rx*sa+y*ry*ca) for x,y in shapes[shape%3]]
 pts=[]
 for i,p1 in enumerate(controls):
  p0=np.array(controls[(i-1)%len(controls)]);p1=np.array(p1);p2=np.array(controls[(i+1)%len(controls)]);p3=np.array(controls[(i+2)%len(controls)])
  for t in np.linspace(0,1,8,endpoint=False):
   q=.5*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t);pts.append(tuple(q))
 x0=max(0,int((min(x for x,y in pts)+96)/192*S)-2);x1=min(S,int((max(x for x,y in pts)+96)/192*S)+3);y0=max(0,int((min(y for x,y in pts)+78)/156*S)-2);y1=min(S,int((max(y for x,y in pts)+78)/156*S)+3)
 if x0>=x1 or y0>=y1:return
 Y,X=np.mgrid[y0:y1,x0:x1].astype(np.float32);X=X/S*192-96;Y=Y/S*156-78;m=np.zeros(X.shape,bool);a=pts[-1]
 for b in pts:m^=((a[1]>Y)!=(b[1]>Y))&(X<(b[0]-a[0])*(Y-a[1])/(b[1]-a[1]+1e-12)+a[0]);a=b
 m=m.astype(np.float32)
 for _ in range(4):m=(m*4+np.roll(m,1,0)+np.roll(m,-1,0)+np.roll(m,1,1)+np.roll(m,-1,1))/8
 a=m[:,:,None]*opacity;rgb[y0:y1,x0:x1,:3]=rgb[y0:y1,x0:x1,:3]*(1-a)+np.array(colour)/255*a;paint[y0:y1,x0:x1]=np.maximum(paint[y0:y1,x0:x1],m)
# Designed broad overlapping meadow groups. Clear earth corridors are kept around the inherited paths.
rows=[(-67,[-84,-62,-39,-17,8,32,57,82]),(-47,[-92,-71,-48,-26,-2,23,47,72,94]),(-26,[-83,-60,-38,-12,12,36,60,83]),(-5,[-94,-72,-49,-25,0,25,49,73,95]),(17,[-85,-61,-37,-13,12,36,61,84]),(38,[-94,-72,-48,-24,0,25,49,74,96]),(59,[-84,-61,-37,-12,13,37,61,85]),(77,[-94,-72,-47,-23,1,26,51,76,97])]
for row,(y,xs) in enumerate(rows):
 for k,x in enumerate(xs):
  angle=[-.2,.35,-.45,.1][(row+k)%4];shape=(row+k)%3
  brush(x,y,17,14,angle,shape,(143,153,63),.96)
  brush(x-3,y+2,10,7,angle+.2,shape+1,(130,147,54),.6)
  brush(x+5,y-3,6,5,angle-.3,shape+2,(160,164,76),.5)
# Retain paths and geographic masks from the existing source, sampled to this texture resolution.
def mask(name):
 im=bpy.data.images[name];w,h=im.size;a=np.array(im.pixels[:],np.float32).reshape(h,w,4)[:,:,0];ix=np.minimum((np.arange(S)*w/S).astype(int),w-1);iy=np.minimum((np.arange(S)*h/S).astype(int),h-1);return a[iy[:,None],ix[None,:]]
for name,colour,strength in [('06 worn shoulders',(181,150,83),.75),('07 footpath',(207,174,111),.95),('04 eroded bank crown',(197,168,109),.85),('05 moist edge',(112,128,73),.55),('08 mineral outcrops',(157,153,116),.7)]:
 a=mask(name)[:,:,None]*strength;rgb[:,:,:3]=rgb[:,:,:3]*(1-a)+np.array(colour)/255*a
# Foliage floor and edge strokes grow from actual trees, rather than uniform dots over the whole patch.
for i,o in enumerate(sorted([o for o in scene.objects if o.name.startswith('Pilot_Pine')],key=lambda o:o.name)):
 x,y,z=o.location;brush(x,y,3.1*o.scale.x,2.6*o.scale.y,.23*(i%4),i,(103,128,56),.7)
 for j,(dx,dy) in enumerate([(-2.6,-.8),(2,-1.5),(.8,2.6)]):
  brush(x+dx,y+dy,.65,.4,.3*j,i+j,(111,137,55),.8);brush(x+dx-.12,y+dy+.12,.35,.22,.3*j,i+j,(182,186,94),.65)
ao=mask('Ribera baked contact AO');rgb[:,:,:3]*=(.74+.26*ao[:,:,None])
def save(name,a):
 h,w=a.shape[:2];v=np.ones((h,w,4),np.float32);v[:,:,:3]=a[:,:,:3] if a.ndim==3 else a[:,:,None]
 im=bpy.data.images.new(name,width=w,height=h);im.colorspace_settings.name='sRGB';im.pixels.foreach_set(v.ravel());im.filepath_raw=str(out/(name+'.png'));im.file_format='PNG';im.save();im.pack();return im
im=save('F_organic_meadow_painted',rgb);soil=bpy.data.materials['Ochre meadow'];bs=next(n for n in soil.node_tree.nodes if n.type=='BSDF_PRINCIPLED');n=soil.node_tree.nodes.new('ShaderNodeTexImage');n.image=im;soil.node_tree.nodes.active=n;soil.node_tree.links.new(n.outputs['Color'],bs.inputs['Base Color'])
n=soil.node_tree.nodes.new('ShaderNodeTexImage');n.image=save('F_organic_meadow_mask',paint);n.label='Editable connected meadow coverage'

# Small existing rocks grouped closer to the forest edges; no new nodes or geometry.
# Keep positions unchanged here so the surface intervention remains directly comparable.
# Re-bake actual crown contact into a separate map and combine with the E pigment canvas.
soil=bpy.data.materials['Ochre meadow'];nodes=soil.node_tree.nodes;links=soil.node_tree.links;bs=next(n for n in nodes if n.type=='BSDF_PRINCIPLED');output=next(n for n in nodes if n.type=='OUTPUT_MATERIAL')
ao=bpy.data.images.new('F crown contact AO',width=1024,height=1024);ao.colorspace_settings.name='Non-Color';target=nodes.new('ShaderNodeTexImage');target.image=ao;nodes.active=target
occ=nodes.new('ShaderNodeAmbientOcclusion');occ.inputs['Distance'].default_value=4.;occ.samples=16;em=nodes.new('ShaderNodeEmission');links.new(occ.outputs['Color'],em.inputs['Color']);links.new(em.outputs[0],output.inputs['Surface'])
bpy.ops.object.select_all(action='DESELECT')
for side in (-1,1):bpy.data.objects['Pilot_Terrain_'+str(side)].select_set(True)
bpy.context.view_layer.objects.active=bpy.data.objects['Pilot_Terrain_1'];scene.render.engine='CYCLES';scene.cycles.samples=16;bpy.ops.object.bake(type='EMIT',margin=4,use_clear=True);links.new(bs.outputs[0],output.inputs['Surface'])
base=bpy.data.images['F_organic_meadow_painted'];S=base.size[0];rgb=np.array(base.pixels[:],np.float32).reshape(S,S,4);small=np.array(ao.pixels[:],np.float32).reshape(1024,1024,4)[:,:,:3];big=np.repeat(np.repeat(small,2,0),2,1);old=bpy.data.images['Ribera baked contact AO'];w,h=old.size;oldpixels=np.array(old.pixels[:],np.float32).reshape(h,w,4)[:,:,0];ix=np.minimum((np.arange(S)*w/S).astype(int),w-1);iy=np.minimum((np.arange(S)*h/S).astype(int),h-1);oldao=oldpixels[iy[:,None],ix[None,:]];rgb[:,:,:3]=rgb[:,:,:3]/(.74+.26*oldao[:,:,None])*(.74+.26*big)
im=bpy.data.images.new('F meadow with sculpted crown contact',width=S,height=S);im.pixels.foreach_set(rgb.ravel());n=nodes.new('ShaderNodeTexImage');n.image=im;links.new(n.outputs['Color'],bs.inputs['Base Color']);nodes.active=n
scene['finishing_stage']='F inherited crowns remeshed and sculpted into three families';scene['source_E_sha256']=hashlib.sha256(src.read_bytes()).hexdigest()
for im in bpy.data.images:
 if im.type=='IMAGE' and im.size[0]>0:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'F','source_E_sha256':scene['source_E_sha256'],'tree_positions_preserved':True,'trees':len(trees),'families':stats,'method':'inherited crown extraction, voxel remesh, sculpt smoothing and decimation; original trunk retained','art_approved':False},indent=2))
