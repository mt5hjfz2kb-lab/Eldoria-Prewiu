"""GPT-directed, source-derived entry-zone authoring. No original file is modified.
Reprofile the existing V3 mesh to measured spatial V1; recover the authored gate family.
This is a candidate, never automatic art approval. Native Unity XYZ <-> Blender X,-Z,Y.
"""
import bpy,bmesh,sys,os,json,math,hashlib,struct
from mathutils import Vector,Matrix
root=os.getcwd();out=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016/entry-zone-v7')
os.makedirs(out,exist_ok=True)
sha=lambda p:hashlib.sha256(open(p,'rb').read()).hexdigest()
source=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016/terrain-v3/ValoriaTerrainRiverCliffV3.blend')
gatefile=os.path.join(root,'art-source/valoria/production/lower-gate-family-v1/LowerGateFamilyV1.blend')
assert sha(source)=='768a08535f066a40c707fd52901a0c16efad91ec06ce91f35ca31cf625b5792f'
report=json.load(open('docs/evidence/valoria-layered-construction-20261016/spatial-v1/spatial-native.json'))
contacts={c['id']:c['native_xyz'] for c in report['contacts']}
sx,sy,sz=contacts['CentralStair'];stair_delta=sz-2.75
bpy.ops.wm.open_mainfile(filepath=source)
chosen=[o for o in bpy.data.objects if o.type=='MESH' and not o.hide_render]
assert len(chosen)==9,[(o.name,o.hide_render) for o in bpy.data.objects if o.type=='MESH']
for o in list(bpy.data.objects):
 if o not in chosen:bpy.data.objects.remove(o,do_unlink=True)
terrain=next(o for o in chosen if 'LOWER_REAL_TERRAIN_NATIVE_HEIGHTFIELD' in o.name)
terrain.name='ValoriaEntryTerrainV7'
water=next(o for o in chosen if 'RIVER_CONTINUOUS_RIPPLED_WATER' in o.name)
# Primary terrain reflow uses real observed positions, not a repeated decorative noise facade.
def smooth(a,b,v):
 t=max(0.,min(1.,(v-a)/(b-a)));return t*t*(3-2*t)
def map_z(z):
 keys=[(-40.,-45.),(-19.65,-19.65),(-9.35,-9.35),(3.01,sz+.26),(9.,contacts['Bastion'][2]),(40.,100.)]
 for (a,b),(c,d) in zip(keys,keys[1:]):
  if z<=c:return b+(z-a)*(d-b)/(c-a)
 return keys[-1][1]+z-keys[-1][0]
def width(z):return 29.+5.*math.sin(max(0,min(1,(z+8)/75))*math.pi)+1.2*math.sin(z*.115)
def ground(x,z):
 near=-9.7+1.1*math.sin(x*.10)+.45*math.sin(x*.29+.7)
 far=-20.1+.65*math.sin(x*.12+.8)
 riverbed=-4.5+.12*math.sin(x*.15+z*.09)
 north=smooth(near-6.0,near+1.5,z)
 south=1-smooth(far-2.0,far+3.5,z)
 relief=.28+.07*math.sin(x*.13+z*.055)*math.sin(x*.069-z*.10)
 # Sculpted scarp: an irregular deep shoulder rather than a vertical retaining slab.
 scarp=sz-3.5+1.7*math.sin(x*.18)+.65*math.sin(x*.43+1.2)
 upper=2.42*smooth(scarp-3.5,scarp+2.0,z)
 # The real 12-step throat stays open; upper plateau starts behind last tread.
 throat=1-smooth(1.85,2.65,abs(x-sx))
 upper*=1-throat*(1-smooth(sz+3.0,sz+3.35,z))
 perimeter=1-smooth(width(z),width(z)+3.0,abs(x))
 north*=perimeter
 h=riverbed+north*(relief+upper-riverbed)+south*(relief-riverbed)
 # Multi-scale displaced strata confined to exposed earth, with eroded gullies.
 bank=4*north*(1-north)+4*south*(1-south)
 scarp_mask=4*smooth(scarp-3.5,scarp+2.0,z)*(1-smooth(scarp-3.5,scarp+2.0,z))
 weather=(.35*math.sin(x*.73+z*.39)+.18*math.sin(x*1.57-z*.81)+.10*math.sin(x*3.19+z*1.77))
 h+=weather*max(bank,scarp_mask)
 # Exact measured bridge shore grades, flared actual banks. No collider offsets.
 for end in (-19.65,-9.35):
  band=(1-smooth(1.8,4.8,abs(x)))*(1-smooth(.15,1.25,abs(z-end)))
  h=h*(1-band)+.38*band
 # Main road contact: terrain lies just below the real road; no rock across passage.
 if -8.7<z<sz-2.6 and abs(x-sx*(z+8.7)/(sz-2.6+8.7))<2.1:h=min(h,.28)
 return h
# Add topology only around the two river banks, where the original coarse field
# cannot describe a 4m cliff. Keep the distant grid inexpensive and editable.
bm=bmesh.new();bm.from_mesh(terrain.data)
edges=list(bm.edges)
bmesh.ops.subdivide_edges(bm,edges=edges,cuts=3,use_grid_fill=True)
bm.to_mesh(terrain.data);bm.free()
for v in terrain.data.vertices:
 x=v.co.x*1.4;z=map_z(-v.co.y);v.co=Vector((x,-z,ground(x,z)))
terrain.data.update()
for o in list(chosen):
 if o==terrain:continue
 if o==water:
  for v in o.data.vertices:v.co.x*=2.6;v.co.z=-3.7+.025*math.sin(v.co.x*.8+v.co.y*.5)
 elif 'IRREGULAR_RETERRACED_CLIFF' in o.name:
  for v in o.data.vertices:
   x=v.co.x;z=-v.co.y
   v.co.x=sx+math.copysign(2.1+max(0,abs(x)-2.1)*2.2,x)
   v.co.y=-(z+stair_delta)
 elif 'REAL_BRIDGE_BANK_STONE' in o.name:
  pass # Frozen measured shore rocks remain at the exact bridge contacts.
 else:
  for v in o.data.vertices:
   x=v.co.x*1.4;z=map_z(-v.co.y);v.co.x=x;v.co.y=-z
   v.co.z=ground(x,z)+max(-.25,min(.45,v.co.z))
 o.data.update()
for o in list(chosen):
 if 'RIVERBANK_AUTHOR_ROCKS' in o.name or 'REAL_BRIDGE_BANK_STONE' in o.name or 'IRREGULAR_RETERRACED_CLIFF' in o.name:
  chosen.remove(o);bpy.data.objects.remove(o,do_unlink=True)
# Continuous world-scale UVs and existing PBR maps, never a color-only final claim.
surface='art-source/valoria/lookdev/golden-slice-v1/surface-v2'
def pbr(label,family,rough):
 m=bpy.data.materials.new(label);m.use_nodes=True;n=m.node_tree.nodes;l=m.node_tree.links;p=next((x for x in n if x.type=='BSDF_PRINCIPLED'),None) or n.new('ShaderNodeBsdfPrincipled');output=next((x for x in n if x.type=='OUTPUT_MATERIAL'),None) or n.new('ShaderNodeOutputMaterial');l.new(p.outputs['BSDF'],output.inputs['Surface']);p.inputs['Roughness'].default_value=rough
 for kind,socket in [('albedo','Base Color'),('normal','Normal')]:
  path=os.path.abspath(f'{surface}/{family}_{kind}.png');tex=n.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(path,check_existing=True)
  if kind=='normal':
   tex.image.colorspace_settings.name='Non-Color';normal=n.new('ShaderNodeNormalMap');normal.inputs['Strength'].default_value=.55;l.new(tex.outputs['Color'],normal.inputs['Color']);l.new(normal.outputs['Normal'],p.inputs[socket])
  else:l.new(tex.outputs['Color'],p.inputs[socket])
 return m
meadow=pbr('ENTRY_existing_ground_PBR','ground',.93);rock=pbr('ENTRY_existing_rock_PBR','rock',.94);shore=pbr('ENTRY_existing_shore_PBR','shore',.94)
# Use the SHA-verified, already-authored CC0 meadow atlas from the paused 63-asset work.
atlas=os.path.join(out,'inputs/meadow_grass_and_real_loam_CC0_1k.png')
assert sha(atlas)=='d528215ccc37271a505c99773c454c224c024f12a619b03c47c154f733081d32'
for node in meadow.node_tree.nodes:
 if node.type=='TEX_IMAGE' and node.image and 'albedo' in node.image.name:node.image=bpy.data.images.load(atlas,check_existing=True)
# Remove the rejected masonry-like synthetic cliff map. Real CC0 rock meshes below
# provide geology; the joining mesh uses existing fine stone-grain, no false joints.
for node in rock.node_tree.nodes:
 if node.type=='TEX_IMAGE' and node.image:
  suffix='stone_grain_normal.png' if node.image.colorspace_settings.name=='Non-Color' else 'stone_grain_albedo.png'
  node.image=bpy.data.images.load(os.path.abspath('art-source/valoria/production/rock-terrain-family-v1/'+suffix),check_existing=True)
  if 'normal' in suffix:node.image.colorspace_settings.name='Non-Color'
terrain.data.materials.clear()
for m in (meadow,rock,shore):terrain.data.materials.append(m)
for p in terrain.data.polygons:
 p.material_index=1 if p.normal.z<.94 else 2 if p.center.z < -.1 else 0
 p.use_smooth=True
uv=terrain.data.uv_layers.active or terrain.data.uv_layers.new(name='Native_Metre_UV')
for p in terrain.data.polygons:
 for li in p.loop_indices:
  v=terrain.data.vertices[terrain.data.loops[li].vertex_index].co
  uv.data[li].uv=(v.x*.11,(v.z if p.normal.z<.78 else v.y)*.11)
for o in chosen:
 if 'IRREGULAR_RETERRACED_CLIFF' in o.name:
  o.data.materials.clear();o.data.materials.append(rock)
  for p in o.data.polygons:p.material_index=0
# Actual free CC0 PBR scan verified against the official file manifest.
import urllib.request
asset='aerial_rocks_02'
api={}
try:api=json.load(urllib.request.urlopen(urllib.request.Request('https://api.polyhaven.com/files/'+asset,headers={'User-Agent':'Eldoria-CC0-Asset-Import/1.0'}),timeout=45))
except Exception as exc:print('OFFICIAL_API_UNAVAILABLE_USE_OFFICIAL_DIRECT_DOWNLOAD',str(exc))
provenance={'source':'https://polyhaven.com/a/'+asset,'license':'https://polyhaven.com/license','license_id':'CC0-1.0','cost_eur':0,'credit':'Powered by Poly Haven','files':[]}
scans={}
for kind,fmt in [('diff','jpg'),('nor_gl','png'),('rough','jpg'),('disp','png')]:
 item=api.get(kind,{}).get('2k',{}).get(fmt) or {'url':f'https://dl.polyhaven.org/file/ph-assets/Textures/{fmt}/2k/{asset}/{asset}_{kind}_2k.{fmt}'};path=os.path.join(out,asset+'_'+kind+'.'+fmt)
 request=urllib.request.Request(item['url'],headers={'User-Agent':'Eldoria-CC0-Asset-Import/1.0'})
 raw=urllib.request.urlopen(request,timeout=90).read();open(path,'wb').write(raw)
 actual_md5=hashlib.md5(raw).hexdigest()
 if 'md5' in item:assert actual_md5==item['md5']
 scans[kind]=bpy.data.images.load(path,check_existing=True)
 if kind!='diff':scans[kind].colorspace_settings.name='Non-Color'
 provenance['files'].append({'map':kind,'url':item['url'],'md5':actual_md5,'official_manifest_md5_verified':'md5' in item,'sha256':sha(path)})
open(os.path.join(out,'cc0-rock-provenance.json'),'w').write(json.dumps(provenance,indent=2))
# Bake a single nonrepeating, geographically authored meadow/soil surface.
# The photographic inputs are unchanged CC0 assets; dirt follows actual paths,
# bridge aprons and gameplay parcels instead of a repeated polka-dot tile.
import numpy as np
N=2048
xx=np.linspace(-44.8,44.8,N,dtype=np.float32)[None,:]
zz=np.linspace(-31.3,86.2,N,dtype=np.float32)[:,None]
image=bpy.data.images.load(atlas,check_existing=True)
w,h=image.size;data=np.asarray(image.pixels[:],dtype=np.float32).reshape(h,w,4)
# A green photographic corner contains fine grass, avoiding the atlas's large
# repeated loam circles. Blend rotated samples before baking the unique map.
def grass_sample(angle,phase):
 u=np.mod((xx*math.cos(angle)+zz*math.sin(angle))*.29+phase,1)
 v=np.mod((-xx*math.sin(angle)+zz*math.cos(angle))*.29+.17,1)
 ix=(u*(w*.18)).astype(np.int32);iy=((.81+v*.18)*h).astype(np.int32)
 return data[np.clip(iy,0,h-1),np.clip(ix,0,w-1),:3]
g1=grass_sample(.17,0);g2=grass_sample(.83,.31)
variation=.5+.25*np.sin(xx*.11+zz*.17)+.15*np.sin(xx*.31-zz*.19)
grass=g1*(variation[:,:,None]*.4+.3)+g2*(.7-variation[:,:,None]*.4)
soilimg=bpy.data.images.load(os.path.abspath(surface+'/ground_albedo.png'),check_existing=True)
sw,sh=soilimg.size;soil=np.asarray(soilimg.pixels[:],dtype=np.float32).reshape(sh,sw,4)
su=np.broadcast_to(np.mod(xx*.16+zz*.031,1),(N,N));sv=np.broadcast_to(np.mod(zz*.16-xx*.017,1),(N,N))
dirt=soil[(sv*(sh-1)).astype(np.int32),(su*(sw-1)).astype(np.int32),:3]
centreline=sx*(zz+8.7)/(sz-2.6+8.7)
noise=.28*np.sin(xx*.47+zz*.27)+.21*np.sin(xx*.83-zz*.31)
distance=np.abs(xx-centreline)+noise
wear=np.clip((4.3-distance)/2.3,0,1)*((zz>-10)&(zz<sz+3.5))
for ident,radius in [('LeftCabinParcel',6.0),('RightCampParcel',6.5)]:
 x,y,z=contacts[ident];r=np.sqrt((xx-x)**2+(zz-z)**2)
 wear=np.maximum(wear,np.clip((radius-r+noise)/2.3,0,1)*.70)
# Trail from front bank to the bridge: a graded shoulder, not a floating beige tile.
wear=np.maximum(wear,np.clip((3.3-np.abs(xx)+noise)/1.8,0,1)*(zz<-19)*.9)
light=.91+.055*np.sin(xx*.067+zz*.091)+.045*np.sin(xx*.123-zz*.057)
rgb=np.clip((grass*(1-wear[:,:,None]) + dirt*wear[:,:,None])*light[:,:,None],0,1)
# Sample the scan in metres; slope masks are derived from the actual reprofile,
# not hard polygon categories. Preserve grass/soil contacts and the road throat.
rockimg=scans['diff'];rw,rh=rockimg.size;rockdata=np.asarray(rockimg.pixels[:],dtype=np.float32).reshape(rh,rw,4)
rx=np.broadcast_to(np.mod(xx/17.0+.31,1),(N,N));rz=np.broadcast_to(np.mod(zz/17.0+.18,1),(N,N))
rockrgb=rockdata[(rz*(rh-1)).astype(np.int32),(rx*(rw-1)).astype(np.int32),:3]
# Low-resolution height sampling is enough for the material mask and avoids
# millions of Python geometry calls. The unique 2k photographic map remains full res.
coarse=256;heights=np.array([[ground(float(x),float(z)) for x in np.linspace(-44.8,44.8,coarse)] for z in np.linspace(-31.3,86.2,coarse)],dtype=np.float32)
gz,gx=np.gradient(heights,117.5/(coarse-1),89.6/(coarse-1));slopes=np.sqrt(gx*gx+gz*gz)
ci=np.linspace(0,coarse-1,N);lower=ci.astype(np.int32);upperi=np.minimum(lower+1,coarse-1);f=ci-lower
slope=(slopes[lower[:,None],lower[None,:]]*(1-f[:,None])*(1-f[None,:])+slopes[upperi[:,None],lower[None,:]]*f[:,None]*(1-f[None,:])+slopes[lower[:,None],upperi[None,:]]*(1-f[:,None])*f[None,:]+slopes[upperi[:,None],upperi[None,:]]*f[:,None]*f[None,:])
rockmix=np.clip((slope-.16)/.32,0,1);rockmix=rockmix*rockmix*(3-2*rockmix)
rgb=rgb*(1-rockmix[:,:,None])+rockrgb*rockmix[:,:,None]
for poly in terrain.data.polygons:poly.material_index=0
pixels=np.ones((N,N,4),dtype=np.float32);pixels[:,:,:3]=rgb
baked=bpy.data.images.new('Entry_Geographic_CC0_MeadowSoil_2k',width=N,height=N,alpha=False)
baked.colorspace_settings.name='sRGB';baked.pixels.foreach_set(pixels.ravel());baked.update();baked.pack()
for node in meadow.node_tree.nodes:
 if node.type=='TEX_IMAGE' and node.image and node.image==image:node.image=baked
# Separate UV sets: geographic meadow does not destroy metre-scale cliff texels.
geographic=terrain.data.uv_layers.new(name='Geographic_CC0')
for poly in terrain.data.polygons:
 for li in poly.loop_indices:
  v=terrain.data.vertices[terrain.data.loops[li].vertex_index].co
  geographic.data[li].uv=((v.x+44.8)/89.6,(-v.y+31.3)/117.5)
uvnode=meadow.node_tree.nodes.new('ShaderNodeUVMap');uvnode.uv_map='Geographic_CC0'
for node in meadow.node_tree.nodes:
 if node.type=='TEX_IMAGE' and node.image==baked:meadow.node_tree.links.new(uvnode.outputs['UV'],node.inputs['Vector'])
# Bake tangent normal and roughness at the same geographic coordinates.
for kind,label in [('nor_gl','Normal'),('rough','Roughness')]:
 src=scans[kind];tw,th=src.size;arr=np.asarray(src.pixels[:],dtype=np.float32).reshape(th,tw,4)
 sample=arr[(rz*(th-1)).astype(np.int32),(rx*(tw-1)).astype(np.int32),:3]
 base=np.array([.5,.5,1.] if kind=='nor_gl' else [.93,.93,.93],dtype=np.float32)
 combined=base*(1-rockmix[:,:,None])+sample*rockmix[:,:,None]
 pix=np.ones((N,N,4),dtype=np.float32);pix[:,:,:3]=combined
 img=bpy.data.images.new('Entry_Geographic_'+label+'_2k',width=N,height=N,alpha=False);img.colorspace_settings.name='Non-Color';img.pixels.foreach_set(pix.ravel());img.update();img.pack()
 tex=meadow.node_tree.nodes.new('ShaderNodeTexImage');tex.image=img;meadow.node_tree.links.new(uvnode.outputs['UV'],tex.inputs['Vector'])
 principled=next(n for n in meadow.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
 if kind=='nor_gl':
  norm=meadow.node_tree.nodes.new('ShaderNodeNormalMap');norm.inputs['Strength'].default_value=.7;meadow.node_tree.links.new(tex.outputs['Color'],norm.inputs['Color']);meadow.node_tree.links.new(norm.outputs['Normal'],principled.inputs['Normal'])
 else:meadow.node_tree.links.new(tex.outputs['Color'],principled.inputs['Roughness'])
# Sculpt displacement from the real scan along existing terrain normals;
# protected bridge, road and step contacts remain exact.
disp=scans['disp'];dw,dh=disp.size;dpx=np.asarray(disp.pixels[:],dtype=np.float32).reshape(dh,dw,4)
terrain.data.update()
sculpt_normals=[v.normal.copy() for v in terrain.data.vertices]
for vertex_index,v in enumerate(terrain.data.vertices):
 x=v.co.x;z=-v.co.y
 if abs(x)<4.5 and -24<z<sz+4:continue
 n=sculpt_normals[vertex_index];slope=math.sqrt(n.x*n.x+n.y*n.y)/max(.05,abs(n.z))
 weight=smooth(.18,.7,slope)
 h=float(dpx[int(((z/17+.18)%1)*(dh-1)),int(((x/17+.31)%1)*(dw-1)),0])-.5
 v.co+=n*(h*.28*weight)
terrain.data.update()
# Author stone road and twelve tread skins as joined bevelled masonry meshes.
# Existing route colliders remain at their measured grade; tops use that grade.
stone=pbr('ENTRY_road_masonry_PBR','stone',.88)
verts=[];faces=[]
def block(x,z,y,w,d,h):
 start=len(verts);verts.extend([(x+dx*w/2,-(z+dz*d/2),y+dy*h) for dy in (-1,0) for dz in (-1,1) for dx in (-1,1)])
 for f in [(0,1,3,2),(4,6,7,5),(0,4,5,1),(2,3,7,6),(0,2,6,4),(1,5,7,3)]:faces.append(tuple(start+i for i in f))
# Original road is gate-to-first-stair, with no arbitrary architectural stretch.
end=sz-2.6
for j in range(int((end+9.35)/.52)+1):
 z=-9.35+j*.52;t=(z+9.35)/(end+9.35);centre=sx*t;y=.38+(.30-.38)*t
 for i in range(7):
  x=centre+(i-3)*.49+.06*math.sin(j*3.7+i*2.3)
  block(x,z,y,.455,.485,.12)
for j in range(12):
 z=sz+(j-5)*.52;y=.48+j*.18
 for i in range(6):block(sx+(i-2.5)*.548,z,y,.526,.497,.15)
# South approach is paved to the exact bridge shore; no floating beige slab.
for j in range(8):
 z=-23.55+j*.52
 for i in range(7):block((i-3)*.49,z,.38,.455,.485,.12)
mesh=bpy.data.meshes.new('Entry_RoadAndTreads_AuthoredStone');mesh.from_pydata(verts,[],faces);mesh.update()
o=bpy.data.objects.new('ENTRY_AUTHORED_STONE_ROAD_AND_TWELVE_TREADS',mesh);bpy.context.scene.collection.objects.link(o);o.data.materials.append(stone)
bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(mesh);bm.free()
bev=o.modifiers.new('Hand cut stone edge bevels','BEVEL');bev.width=.018;bev.segments=2
uvroad=mesh.uv_layers.new(name='MetreStoneUV')
for poly in mesh.polygons:
 for li in poly.loop_indices:
  v=mesh.vertices[mesh.loops[li].vertex_index].co;uvroad.data[li].uv=(v.x*.17,v.y*.17)
chosen.append(o)
# Recover the actual authored asymmetric tall-tower gate, not another tower generator.
with bpy.data.libraries.load(gatefile,link=False) as (src,dst):dst.objects=[n for n in src.objects if n.startswith('LowerGate')]
gates=[o for o in dst.objects if o is not None and o.type=='MESH']
assert len(gates)>=6,[o.name for o in gates]
for o in gates:
 bpy.context.scene.collection.objects.link(o)
 o.hide_render=False;o.hide_set(False)
 # Bake source transforms once, then uniform scale .75: preserve mesh proportions.
 transform=o.matrix_world.copy()
 for v in o.data.vertices:
  w=transform@v.co;v.co=Vector((-w.x*.75,-w.y*.75+9.25,w.z*.75+.38))
 o.matrix_world=Matrix.Identity(4);o.name='ENTRY_'+o.name;o.data.update()
# Check ACTUAL transformed vertex bounds before export: source .blend is already
# normalized around the gate origin. This guards against the rejected double offset.
gpoints=[o.matrix_world@v.co for o in gates for v in o.data.vertices]
assert max(v.z for v in gpoints)>6.5 and min(v.z for v in gpoints)>0
assert 5 < sum(v.y for v in gpoints)/len(gpoints) < 13
# Reuse the genuine 63-asset CC0 rock_face_02 mesh. Undo its documented old axis-fit
# to recover natural donor proportions, then use uniform scales only in this scene.
rockpath=os.path.join(out,'inputs/cliff_rock_column.glb')
assert sha(rockpath)=='5a3fa128f87a51851aaf98b7269461db23536d31302e79b6ba7d2beb7bc761ae'
before=set(bpy.data.objects);bpy.ops.import_scene.gltf(filepath=rockpath)
rocks=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
assert len(rocks)>0
for o in rocks:
 M=o.matrix_world.copy()
 for v in o.data.vertices:
  w=M@v.co;v.co=Vector((w.x/.7556536793708801,w.y/.5851455926895142,w.z/1.4367778301239014))
 o.matrix_world=Matrix.Identity(4)
pts=[v.co for o in rocks for v in o.data.vertices]
centre=Vector(((min(v.x for v in pts)+max(v.x for v in pts))/2,(min(v.y for v in pts)+max(v.y for v in pts))/2,min(v.z for v in pts)))
for o in rocks:
 for v in o.data.vertices:v.co-=centre
 mod=o.modifiers.new('Mobile candidate screen-scale decimation','DECIMATE');mod.ratio=.34
# Deliberate clusters with broad natural interfaces, never a row of white primitives.
# A few overlapping three-dimensional outcrops seated in the sloping bed.
# Upper riser uses the sculpted mesh itself; no repeated cliff-face stamps.
placements=[(-23,-12.6,1.30,.35),(-19,-13.1,1.12,1.1),(-9,-12.8,1.38,-.4),
 (10,-12.4,1.42,.7),(14,-12.9,1.16,-.2),(25,-12.5,1.31,.3)]
for i,(x,z,scale,yaw) in enumerate(placements):
 for donor in rocks:
  o=donor.copy();o.data=donor.data;bpy.context.scene.collection.objects.link(o);o.name='ENTRY_SCULPTED_BANK_OUTCROP_'+str(i)
  o.location=(x,-z,-4.65);o.rotation_euler=(0,0,yaw);o.scale=(scale,scale,scale);o.hide_render=False;chosen.append(o)
for o in rocks:bpy.data.objects.remove(o,do_unlink=True)
for o in list(chosen):
 if o.name.startswith('ENTRY_SCULPTED_BANK_OUTCROP_'):
  chosen.remove(o);bpy.data.objects.remove(o,do_unlink=True)
# Extend genuine bridge footing meshes to the new bed; preserve the arch/deck.
bridgefile='docs/evidence/valoria-layered-construction-20261016/bridge-v1/BridgeLevelAccessV1.blend'
assert sha(bridgefile)=='44423997651b05c839da833cdef1914a433058f903db37a9427d66d173b889d0'
with bpy.data.libraries.load(os.path.abspath(bridgefile),link=False) as (src,dst):dst.objects=[n for n in src.objects if n.startswith('VALORIA_')]
bridgeparts=[o for o in dst.objects if o is not None and o.type=='MESH']
assert len(bridgeparts)==10
for o in bridgeparts:bpy.context.scene.collection.objects.link(o)
bpy.context.view_layer.update()
for o in bridgeparts:
 o.hide_render=False;o.hide_set(False)
 if 'ARCH_FOOTINGS' in o.name:
  M=o.matrix_world.copy()
  for v in o.data.vertices:
   w=M@v.co
   if w.z < -1.80:w.z-=2.13
   v.co=w
  o.matrix_world=Matrix.Identity(4);o.data.update()
 o.name='ENTRY_SUPPORTED_'+o.name;chosen.append(o)
# glTFast converts RH glTF to Unity by reflecting X, verified against the saved
# END_PIERS node (+2.18 Unity versus -2.18 glTF). Explicit native-coordinate
# terrain and road authoring therefore need this source-space conversion once.
# Gate/bridge source assets retain their already imported historic orientation.
for o in [terrain]+[o for o in chosen if o.name=='ENTRY_AUTHORED_STONE_ROAD_AND_TWELVE_TREADS']:
 for v in o.data.vertices:v.co.x=-v.co.x
 bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.reverse_faces(bm,faces=list(bm.faces));bmesh.ops.recalc_face_normals(bm,faces=bm.faces)
 # Open height fields have no signed outside: force upward terrain normals.
 bm.normal_update()
 if o==terrain and sum(f.normal.z*f.calc_area() for f in bm.faces)<0:bmesh.ops.reverse_faces(bm,faces=list(bm.faces))
 bm.normal_update();bm.to_mesh(o.data);bm.free();o.data.update()
terrain_normal_z=sum(v.normal.z for v in terrain.data.vertices)/len(terrain.data.vertices)
assert terrain_normal_z>.7,terrain_normal_z
# Validate real bridge footing world bounds; reject detached local-space cubes.
footings=[o for o in bridgeparts if 'ARCH_FOOTINGS' in o.name]
bpy.context.view_layer.update()
fp=[o.matrix_world@v.co for o in footings for v in o.data.vertices]
assert fp and min(-v.y for v in fp)>-19.7 and max(-v.y for v in fp)<-9.3
assert min(v.z for v in fp)<-4.2 and max(v.z for v in fp)<.05
# Recover only two selected, previously improved CC0 assets. They remain candidates
# judged in this scene, not approval inherited from an inventory or old score.
nature=[]
def plant_donor(asset,expected):
 path=os.path.join(out,'inputs',asset+'.glb');assert sha(path)==expected
 before=set(bpy.data.objects);bpy.ops.import_scene.gltf(filepath=path);bpy.context.view_layer.update()
 objects=[o for o in bpy.data.objects if o not in before and o.type=='MESH'];assert objects
 for o in objects:
  M=o.matrix_world.copy()
  for v in o.data.vertices:v.co=M@v.co
  o.parent=None;o.matrix_world=Matrix.Identity(4)
 points=[v.co for o in objects for v in o.data.vertices];centre=Vector(((min(v.x for v in points)+max(v.x for v in points))/2,(min(v.y for v in points)+max(v.y for v in points))/2,min(v.z for v in points)))
 for o in objects:
  for v in o.data.vertices:v.co-=centre
 return objects
pine=plant_donor('pine_medium','003d59a9d184cb6cb04b2b2b21a8cb35c2b92e838cf788575044e576f7846e3e')
shrub=plant_donor('shrub_1','5da94bd40f1afc7b03dc1fb052352ca97b986b35aafb49a69249cebb876f14f0')
for family,objects,placements in [('PINE',pine,[(-28,-4,.9),(-29,7,1.1),(-28,35,.95),(28,-1,.93),(29,29,1.1),(25,47,.85)]),('SHRUB',shrub,[(-10,-7,.75),(-6,-9,.6),(6,-8,.7),(10,-6,.85),(-13,-11,.65),(14,-10,.7),(-4,26,.7),(3.5,27,.7),(-6,29,.65),(5,30,.75),(-20,-7,.8),(21,-5,.85)])]:
 for i,(x,z,scale) in enumerate(placements):
  assert abs(x)>3.3,'Protected axial passage'
  for donor in objects:
   o=donor.copy();o.data=donor.data;bpy.context.scene.collection.objects.link(o);o.name='ENTRY_RECOVERED_CC0_'+family+'_'+str(i)
   o.location=(-x,-z,ground(x,z)-.06);o.rotation_euler=(0,0,i*.93);o.scale=(scale,scale,scale);o.hide_render=False;chosen.append(o);nature.append(o)
for o in pine+shrub:bpy.data.objects.remove(o,do_unlink=True)
# Pack original texture bytes into this isolated, editable source.
for img in bpy.data.images:
 if img.source=='FILE' and img.packed_file is None:
  if not os.path.isfile(bpy.path.abspath(img.filepath)):
   alt=os.path.join(os.path.dirname(gatefile),os.path.basename(img.filepath))
   if os.path.isfile(alt):img.filepath=alt
  if os.path.isfile(bpy.path.abspath(img.filepath)):img.pack()
for o in bpy.data.objects:o.select_set(o.type=='MESH')
bpy.context.view_layer.objects.active=terrain
blend=os.path.join(out,'ValoriaEntryZoneV7.blend');glb=os.path.join(out,'ValoriaEntryZoneV7.glb')
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
bpy.ops.export_scene.gltf(filepath=glb,export_format='GLB',use_selection=True,export_apply=True)
raw=open(glb,'rb').read();gltf=json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])
exported_triangles=sum(gltf['accessors'][p['indices']]['count']//3 for node in gltf['nodes'] if 'mesh' in node for p in gltf['meshes'][node['mesh']]['primitives'])
manifest={'author':'GPT current-session art direction; Qwen auxiliary remains separately recorded','blender_version':bpy.app.version_string,
 'source_blend_sha256':sha(source),'source_gate_blend_sha256':sha(gatefile),'result_blend_sha256':sha(blend),'result_glb_sha256':sha(glb),
 'terrain_mean_normal_up':terrain_normal_z,'bridge_footings_native_z':[min(-v.y for v in fp),max(-v.y for v in fp)],'selected_cc0_nature_mesh_instances':len(nature),'original_sources_unchanged':True,'canonical_modified':False,'terrain_target':terrain.name,'gltfast_native_x_conversion':'Explicit native terrain/road mirrored X once before RH GLB export; historic gate/bridge orientation preserved','gate_uniform_scale':.75,'source_gate_already_origin_normalized':True,'cc0_meadow_unique_bake_dimensions':[2048,2048],'bank_hypothesis':'smoothed sculpted banks, real-scan displacement and slope-blended CC0 photographic surface; incompatible face stamps removed','original_v1_rejected_for_double_origin':True,
 'stair_contact_native':contacts['CentralStair'],'bridge_contact_y':{str(z):ground(0,z) for z in (-19.65,-9.35)},
 'recovered_cc0_rock_asset_sha256':sha(rockpath),'recovered_cc0_meadow_sha256':sha(atlas),'source_bridge_blend_sha256':sha(bridgefile),'mesh_objects':len(chosen)+len(gates),'triangles':exported_triangles,'editable_source_triangles':sum(len(p.vertices)-2 for o in chosen+gates for p in o.data.polygons),
 'art_status':'CANDIDATE_REQUIRES_GENUINE_UNITY_AB','operations':['source-topology shore subdivision','direct source vertex reprofile','world-scale UV','existing PBR texture maps','reuse original Blender gate with uniform transform']}
open(os.path.join(out,'entry-source.json'),'w').write(json.dumps(manifest,indent=2)+'\n')
print('VALORIA_ENTRY_SOURCE_COMPLETE',json.dumps(manifest))
