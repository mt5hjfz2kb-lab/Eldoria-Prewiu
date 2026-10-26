"""P: extend the owner's O source across its real north boundary, preserving all original vertices."""
import bpy,sys,pathlib,hashlib,json,math
import numpy as np
from mathutils import Vector
from mathutils.geometry import interpolate_bezier
out,src=map(pathlib.Path,sys.argv[sys.argv.index('--')+1:]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='a59b20127942ee986c74ff255102e127bb1e1ae8cc373f396046622890f27387'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene
coll=bpy.data.collections.new('16 Connected sector P - forest meadow low ridge');scene.collection.children.link(coll)
def guide(name,pts):
 c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';s=c.splines.new('BEZIER');s.bezier_points.add(len(pts)-1)
 for p,co in zip(s.bezier_points,pts):p.co=co;p.handle_left_type='AUTO';p.handle_right_type='AUTO'
 o=bpy.data.objects.new(name,c);coll.objects.link(o);o.hide_render=True;bpy.context.view_layer.update();samples=[]
 for a,b in zip(s.bezier_points,s.bezier_points[1:]):samples.extend(interpolate_bezier(a.co,a.handle_right,b.handle_left,b.co,32)[:-1])
 samples.append(s.bezier_points[-1].co.copy());return o,samples
terrain=bpy.data.objects['Pilot_Terrain_1'];old=terrain.data;oldverts=[tuple(v.co) for v in old.vertices];oldfaces=[tuple(p.vertices) for p in old.polygons];oldmats=[p.material_index for p in old.polygons];oldsmooth=[p.use_smooth for p in old.polygons];olduv=[tuple(x.uv) for x in old.uv_layers.active.data]
assert len(oldverts)==193*85
edge=np.array([oldverts[i*85+84] for i in range(193)]);assert np.max(abs(edge[:,1]-78))<.001
ridge,rs=guide('Editable P low ridge spine',[(24,90,3),(37,107,8),(43,123,11),(32,142,8),(34,160,4)])
def height(x,y):
 base=float(np.interp(x,edge[:,0],edge[:,2]));blend=max(0,min(1,(y-78)/18));blend=blend*blend*(3-2*blend)
 best=min(rs,key=lambda p:(p.x-x)**2+(p.y-y)**2);d=math.hypot(x-best.x,y-best.y);shoulder=max(0,1-(d/27)**2);shoulder=shoulder*shoulder
 return base+blend*(max(0,best.z-3)*shoulder+(.7 if x<-25 else .25)*(y-78)/92)
# Extend, weld by shared boundary indices: no second surface underneath or visible tile seam.
verts=oldverts[:];faces=oldfaces[:];N=len(verts);rows=46
for j in range(1,rows+1):
 y=78+j*2
 for i in range(193):x=-96+i;verts.append((x,y,height(x,y)))
def idx(i,j):return i*85+84 if j==0 else N+(j-1)*193+i
for j in range(rows):
 for i in range(192):faces.append((idx(i,j),idx(i+1,j),idx(i+1,j+1),idx(i,j+1)))
mesh=bpy.data.meshes.new('P O terrain with welded northward extension');mesh.from_pydata(verts,[],faces);mesh.update()
for m in old.materials:mesh.materials.append(m)
terrain.data=mesh
uv=mesh.uv_layers.new(name='O retained UV and P painted sector')
for p in mesh.polygons:
 if p.index<len(oldfaces):
  p.material_index=oldmats[p.index]
  for li in p.loop_indices:uv.data[li].uv=olduv[li]
 else:
  p.material_index=len(mesh.materials)
  for li in p.loop_indices:
   c=mesh.vertices[mesh.loops[li].vertex_index].co;uv.data[li].uv=((c.x+96)/192,(c.y-78)/92)
 p.use_smooth=oldsmooth[p.index] if p.index<len(oldfaces) else True
# Continuation starts with the original guide's exact endpoint and outgoing direction.
paths=[[(15,78),(19,88),(11,101),(-2,113),(-8,129),(5,149),(12,170)],[(-2,113),(24,122),(41,131),(58,145),(96,155)],[(-8,129),(-35,135),(-65,149),(-96,153)]]
samples=[]
for i,pts in enumerate(paths):
 _,s=guide('Editable P connected path '+str(i),[(x,y,height(x,y)+.03) for x,y in pts]);
 for point in s:point.z=height(point.x,point.y)+.03
 samples.append(s)
# Deliberate forest perimeter: connected western mass with a concave meadow edge.
poly=[(-83,82),(-47,80),(-26,89),(-31,105),(-23,119),(-41,129),(-57,138),(-81,129),(-88,107)]
guide('Editable P forest boundary',[(x,y,height(x,y)) for x,y in poly+[poly[0]]])
def inside(x,y):
 c=False
 for (ax,ay),(bx,by) in zip(poly,poly[1:]+poly[:1]):
  if (ay>y)!=(by>y) and x<(bx-ax)*(y-ay)/(by-ay)+ax:c=not c
 return c
# Existing sculpted sources, shared geometry and material assets; no new primitive tree family.
masters=[bpy.data.objects['Editable Layered fir source'],bpy.data.objects['Editable Slender fir source']]
positions=[]
for row,y in enumerate(np.arange(84,137,6.7)):
 for col,x in enumerate(np.arange(-85,-22,6.9)):
  x=float(x+([0,2.0,-1.2,1.0][row%4]));y1=float(y+([0,1.3,-.8][col%3]))
  if not inside(x,y1):continue
  if min((p.x-x)**2+(p.y-y1)**2 for s in samples for p in s)<20:continue
  positions.append((x,y1))
for i,(x,y) in enumerate(positions):
 srcobj=masters[1 if -75<x<-40 and 92<y<120 else 0];o=srcobj.copy();o.data=srcobj.data;coll.objects.link(o);o.name='Pilot_Pine_P_'+str(i);o.hide_render=False;o.hide_viewport=False;o.hide_set(False);o.location=(x,y,height(x,y));s=[.82,1.08,.94,1.18,.73,1.0][i%6];o.scale=(s,s,s);o.rotation_euler.z=[.2,1.7,3.0,4.9,2.4][i%5]
assert len(positions)+156<=256
# Small rock shoulders use the existing editable rock meshes, tied to the ridge, not isolated noise.
rocksrc=next(o for o in scene.objects if o.name.startswith('Pilot_Rock') and o.type=='MESH')
for i,(x,y,s) in enumerate([(35,108,.7),(39,110,1),(41,112,.6),(44,124,1),(47,125,.7),(46,128,.5),(32,142,.8),(35,144,.6)]):
 o=rocksrc.copy();o.data=rocksrc.data;coll.objects.link(o);o.name='P ridge exposed stone '+str(i);o.location=(x,y,height(x,y)+.25);o.scale=rocksrc.scale*s;o.rotation_euler.z+=i*.43
# Painterly extension atlas: preserve exact O seam, then directed forest/clearing/path masks.
base=bpy.data.images['I recovered exact retained surface'];S=base.size[0];original=np.array(base.pixels[:],np.float32).reshape(S,S,4)
W=2048;H=1024;X,Y=np.meshgrid(np.linspace(-96,96,W),np.linspace(78,170,H));ix=np.clip(((X+96)/192*(S-1)).astype(int),0,S-1);iy=np.clip(((Y-78)/92*(S-1)).astype(int),0,S-1)
pixels=original[iy,ix].copy();grain=np.clip(pixels[:,:,:3].mean(2)/original[:,:,:3].mean(),.94,1.06)
pixels[:,:,:3]=np.array([.53,.565,.24])*grain[:,:,None]
# Broad low-contrast pigments follow the forest mass and the ridge; avoid disconnected meadow spots.
forest=np.zeros((H,W),np.float32)
for x,y in positions:
 d=((X-x)/6.4)**2+((Y-y)/6.4)**2;forest=np.maximum(forest,np.clip(1-d,0,1))
forest=np.clip(forest*.38,0,.38);pixels[:,:,:3]*=(1-forest[:,:,None]*np.array([.32,.20,.22]))
pathmask=np.zeros((H,W),np.float32)
for s in samples:
 for p in s:
  px=(p.x+96)/192*(W-1);py=(p.y-78)/92*(H-1);rx=2.3/192*W;ry=2.3/92*H;x0=max(0,int(px-rx-1));x1=min(W,int(px+rx+2));y0=max(0,int(py-ry-1));y1=min(H,int(py+ry+2))
  if x0>=x1 or y0>=y1:continue
  yy,xx=np.mgrid[y0:y1,x0:x1];d=np.sqrt(((xx-px)/rx)**2+((yy-py)/ry)**2);a=np.clip((1-d)*2.5,0,1);pathmask[y0:y1,x0:x1]=np.maximum(pathmask[y0:y1,x0:x1],a)
pathcolour=original[-1,int((15+96)/192*(S-1)),:3];pixels[:,:,:3]=pixels[:,:,:3]*(1-pathmask[:,:,None])+pathcolour*pathmask[:,:,None]
seam=np.clip((Y-78)/12,0,1);seam=seam*seam*(3-2*seam);pixels[:,:,:3]=original[-1,ix,:3]*(1-seam[:,:,None])+pixels[:,:,:3]*seam[:,:,None];pixels[:,:,3]=1
im=bpy.data.images.new('P connected sector painted surface',width=W,height=H);im.colorspace_settings.name='sRGB';im.pixels.foreach_set(pixels.ravel())
for name,mask in [('P editable forest floor mask',forest),('P editable connected paths mask',pathmask)]:
 a=np.ones((H,W,4),np.float32);a[:,:,:3]=mask[:,:,None];m=bpy.data.images.new(name,width=W,height=H);m.colorspace_settings.name='Non-Color';m.pixels.foreach_set(a.ravel())
mat=old.materials[0].copy();mat.name='P connected meadow forest floor paths';bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);mesh.materials.append(mat)
# Authoritative export of route heights and mesh seam; integration reads the exported shape, not a Unity formula.
route=[{'path':i,'samples':[list(p) for p in s]} for i,s in enumerate(samples)];maxslope=max(math.degrees(math.atan2(abs(b.z-a.z),max(.001,math.hypot(b.x-a.x,b.y-a.y)))) for s in samples for a,b in zip(s,s[1:]))
assert all(tuple(mesh.vertices[i].co)==oldverts[i] for i in range(N))
scene['finishing_stage']='P connected forest meadow ridge extension of owner O';scene['P_source_sha256']=hashlib.sha256(src.read_bytes()).hexdigest()
for image in bpy.data.images:
 if image.size[0]>0 and image.source in {'GENERATED','FILE'}:image.filepath_raw=str(out/(image.name.replace(' ','_')+'.png'));image.file_format='PNG';image.save();image.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
scene.camera.location=(0,-120,180);target=Vector((0,45,0));scene.camera.rotation_euler=(target-scene.camera.location).to_track_quat('-Z','Y').to_euler();scene.camera.data.ortho_scale=240
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.cycles.samples=24;scene.render.filepath=str(out/'blender-sector.png');bpy.ops.render.render(write_still=True)
(out/'sector-authoring.json').write_text(json.dumps({'stage':'P','source_sha256':scene['P_source_sha256'],'old_terrain_vertices_unchanged':N,'welded_seam_vertices':193,'new_terrain_vertices':len(verts)-N,'added_trees':len(positions),'total_trees':len(positions)+156,'extent_blender_metres':[-96,96,-78,170],'max_authored_path_slope_degrees':maxslope,'paths':route,'art_approved':False},indent=2))
