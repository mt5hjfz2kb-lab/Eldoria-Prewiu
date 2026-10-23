"""X: macro-sculpted continuous ridges, canopy stands and authored ground masses on Q; O untouched."""
import bpy,sys,pathlib,hashlib,json,math
import numpy as np
from mathutils import Vector
from mathutils.geometry import interpolate_bezier
out,src=map(pathlib.Path,sys.argv[sys.argv.index('--')+1:]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='6a28e42db779bff8af5ec10a13bac87d38b97e594c9528aad8f7cbd211a5828e'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene;terrain=bpy.data.objects['Pilot_Terrain_1'];coll=bpy.data.collections['16 Connected sector P - forest meadow low ridge'];N=193*85
original=[tuple(v.co) for v in terrain.data.vertices[:N]]
# Broad proportional-edit strokes raise an asymmetric shoulder; no changes to the inherited O boundary.
brushes=[(31,110,31,30,3.2),(39,133,34,37,4.0),(24,155,28,30,2.2)]
for v in terrain.data.vertices[N:]:
 x,y,z=v.co;fade=min(1,max(0,(y-78)/15));delta=0
 for cx,cy,rx,ry,amp in brushes:
  r=((x-cx)/rx)**2+((y-cy)/ry)**2
  if r<1:delta+=amp*(1-r)**2
 v.co.z+=delta*fade
# W: physically sculpt an elongated secondary saddle instead of more nearly invisible pigment.
# All edits remain north of O's exact boundary; asymmetry avoids a radial hill.
for v in terrain.data.vertices[N:]:
 x,y,z=v.co
 fade=max(0,min(1,(y-80)/20));fade=fade*fade*(3-2*fade)
 d=((x-57)/37)**2+((y-133)/31)**2
 saddle=max(0,1-d)**2
 d2=((x-3)/27)**2+((y-147)/25)**2
 shoulder=max(0,1-d2)**2
 v.co.z+=fade*(3.4*saddle+1.7*shoulder)
# X: author three interconnected landforms, with a shallow swale between them.
# Sculpt vertices only in the new extension; the O mesh, seams and gameplay datum are immutable.
# Long oblique ridges and asymmetric hollows make silhouette changes legible from camera.
for v in terrain.data.vertices[N:]:
 x,y,z=v.co
 u=max(0,min(1,(y-83)/20));fade=u*u*(3-2*u)
 north=max(0,min(1,(166-y)/17))
 spine_x=36+11*math.sin((y-101)/35)
 spine=math.exp(-((x-spine_x)/24)**2)*math.exp(-((y-132)/39)**4)
 west=math.exp(-((x+24+6*math.sin(y/19))/25)**2)*math.exp(-((y-143)/30)**4)
 hollow=math.exp(-((x-5)/13)**2)*math.exp(-((y-118)/24)**2)
 eastern_toe=math.exp(-((x-65)/22)**2)*math.exp(-((y-111)/32)**4)
 v.co.z+=fade*north*(8.8*spine+5.8*west+3.9*eastern_toe-2.6*hollow)
terrain.data.update();bpy.context.view_layer.update()
def ground(x,y):
 # Avoid numerical ray/triangle ambiguity exactly on the open mesh boundary.
 x=max(-95.999,min(95.999,x));y=max(-77.999,min(169.999,y))
 ok,p,n,i=terrain.ray_cast(Vector((x,y,100)),Vector((0,0,-1)));assert ok,(x,y);return p.z
# Scale existing new-sector fir crowns as cohesive groves, keeping the authored glade and planting points.
trees=sorted([o for o in scene.objects if o.name.startswith('Pilot_Pine_P_')],key=lambda o:o.name)
for i,o in enumerate(trees):
 x,y=o.location.xy;core=((x+57)/24)**2+((y-105)/24)**2
 factor=1.48 if core<1 else 1.20
 o.scale.x*=factor;o.scale.y*=factor;o.scale.z*=1.12 if core<1 else 1.04;o.location.z=ground(x,y)
for o in scene.objects:
 if o.name.startswith('P ridge exposed stone'):o.location.z=ground(o.location.x,o.location.y)+.18;o.scale*=1.65
# Curve control points follow the actual sculpt, exported points and surface remain in one source.
for o in scene.objects:
 if o.type=='CURVE' and o.name.startswith('Editable P connected path'):
  for sp in o.data.splines:
   for p in sp.bezier_points:p.co.z=ground(p.co.x,p.co.y)+.03
# Hand-directed broad pigment strokes follow slope, meadow clearings and the canopy footprint.
im0=bpy.data.images['Q continuous sector meadow pigment'];W,H=im0.size;pix=np.array(im0.pixels[:],np.float32).reshape(H,W,4);X,Y=np.meshgrid(np.linspace(-96,96,W),np.linspace(78,170,H));seam=np.clip((Y-78)/12,0,1);seam=seam*seam*(3-2*seam)
# The Q road mask was an unused Blender image datablock and can be purged on
# save/reopen. Recover it deterministically from the still-editable Bezier routes.
# Match Q's original raster and widths; do not invent another road network.
path=np.zeros((H,W),np.float32)
for k in range(3):
 route=bpy.data.objects['Editable P connected path '+str(k)].data.splines[0]
 sampled=[]
 for a,b in zip(route.bezier_points,route.bezier_points[1:]):
  sampled.extend(interpolate_bezier(a.co,a.handle_right,b.handle_left,b.co,32)[:-1])
 sampled.append(route.bezier_points[-1].co.copy())
 for p in sampled:
  xx=(p.x+96)/192*(W-1);yy=(p.y-78)/92*(H-1)
  rx=1.65/192*W;ry=1.65/92*H
  a=max(0,int(xx-rx-1));b=min(W,int(xx+rx+2))
  c=max(0,int(yy-ry-1));d=min(H,int(yy+ry+2))
  if a>=b or c>=d:continue
  iy,ix=np.mgrid[c:d,a:b]
  dist=np.sqrt(((ix-xx)/rx)**2+((iy-yy)/ry)**2)
  path[c:d,a:b]=np.maximum(path[c:d,a:b],np.clip((1-dist)*2.1,0,1))
assert np.any(path>0),'Recovered editable paths generated no pigment mask'
# X: complete the forest structure before painting the same texture, using editable pine meshes.
# Build three interlocking stands with visible trail clearings; never scatter global noise.
rng=np.random.default_rng(20261023);old_new_sector_count=len(trees);new_trees=[]
for group,(cx,cy,rx,ry,quota) in enumerate([(-13,112,24,19,12),(20,138,23,19,12),(62,119,22,23,12)]):
 made=0
 for attempt in range(1100):
  if made>=quota:break
  angle=float(rng.uniform(0,2*math.pi));radius=math.sqrt(float(rng.random()))
  x=float(cx+rx*radius*math.cos(angle));y=float(cy+ry*radius*math.sin(angle))
  if not(-90<x<90 and 91<y<160):continue
  if any((x-o.location.x)**2+(y-o.location.y)**2<5.1**2 for o in trees):continue
  ix=int((x+96)/192*(W-1));iy=int((y-78)/92*(H-1))
  px=max(2,int(3.7/192*W));py=max(2,int(3.7/92*H))
  if np.max(path[max(0,iy-py):min(H,iy+py+1),max(0,ix-px):min(W,ix+px+1)])>.08:continue
  donor=trees[(group*13+made*7)%old_new_sector_count]
  o=donor.copy();o.data=donor.data;coll.objects.link(o)
  o.name='Pilot_Pine_X_'+str(group)+'_'+str(made)
  o.location=(x,y,ground(x,y))
  scale=float(rng.uniform(.86,1.15));o.scale=donor.scale*scale;o.rotation_euler.z=float(rng.uniform(-math.pi,math.pi))
  trees.append(o);new_trees.append(o);made+=1
 assert made>=10,('Forest stand cannot be planted without crossing an authored road',group,made)
assert len(trees)+156<=256,'Pilot atlas limit; never silently drop authored trees'
mask=np.zeros((H,W),np.float32)
for o in trees:
 x,y=o.location.xy;r=((X-x)/6.7)**2+((Y-y)/6.7)**2;mask=np.maximum(mask,np.clip(1-r,0,1)*.44)
# Connected litter under crowns, with visible grassy clearings, not floating dark oval meshes.
a=mask*seam*(1-path);pix[:,:,:3]=pix[:,:,:3]*(1-a[:,:,None])+np.array([.34,.39,.17])*a[:,:,None]
# Long curved paint strokes, deliberately drawn along the landform; taper both ends.
strokes=[([(21,87),(26,99),(31,111),(33,125)],8,.38,[.66,.59,.34]), ([(47,102),(57,116),(58,132),(48,146)],10,.35,[.65,.60,.36]), ([(9,106),(14,121),(10,138),(-1,149)],11,.16,[.63,.61,.29]), ([(-18,85),(-21,98),(-16,111)],7,.22,[.43,.50,.22])]
paint=np.zeros((H,W),np.float32)
for points,width,strength,colour in strokes:
 a=np.zeros((H,W),np.float32)
 for j,((ax,ay),(bx,by)) in enumerate(zip(points,points[1:])):
  t=np.clip(((X-ax)*(bx-ax)+(Y-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2),0,1);d=np.sqrt((X-ax-t*(bx-ax))**2+(Y-ay-t*(by-ay))**2);u=(j+t)/(len(points)-1);taper=np.sin(u*np.pi)**.6
  a=np.maximum(a,np.clip(1-d/width,0,1)**1.6*taper*strength)
 a*=seam*(1-path)* (1-mask);paint=np.maximum(paint,a);pix[:,:,:3]=pix[:,:,:3]*(1-a[:,:,None])+np.array(colour)*a[:,:,None]
# V: path shoulder erosion tied to the SAME editable routes, avoiding new geometry.
# The inner path stays walkable/legible; only its outer fringes gain broken soil.
edge=np.clip(path*.95,0,1)
fringe=np.clip(path*.42,0,.42)*(1-edge)
grain=(np.sin(X*.77+Y*.23)*np.sin(Y*.61-X*.31)+1)*.5
fringe*=np.clip((grain-.18)*1.35,0,1)*seam
soil=np.array([.49,.43,.27],np.float32)
pix[:,:,:3]=pix[:,:,:3]*(1-fringe[:,:,None])+soil*fringe[:,:,None]
# One irregular damp grass transition below the forest canopy, not repeated spots.
transition=np.clip(mask*.24,0,.15)*seam*(1-path)
pix[:,:,:3]=pix[:,:,:3]*(1-transition[:,:,None])+np.array([.30,.37,.19])*transition[:,:,None]
# U: directed microhabitats within the retained meadow, never a global noise field.
# These strokes follow the three authored paths and the shoulder's lower contours.
habitats=[
 ([(5,94),(9,106),(17,118),(22,133)],5.5,.17,[.43,.49,.25]),
 ([(63,92),(57,105),(63,120),(75,137)],8.0,.13,[.72,.65,.39]),
 ([(0,150),(12,154),(26,162),(42,167)],7.0,.14,[.49,.55,.28]),
 ([(45,91),(52,102),(49,114)],4.5,.15,[.48,.42,.27])]
for points,width,strength,colour in habitats:
 a=np.zeros((H,W),np.float32)
 for j,((ax,ay),(bx,by)) in enumerate(zip(points,points[1:])):
  t=np.clip(((X-ax)*(bx-ax)+(Y-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2),0,1)
  d=np.sqrt((X-ax-t*(bx-ax))**2+(Y-ay-t*(by-ay))**2)
  u=(j+t)/(len(points)-1)
  a=np.maximum(a,np.clip(1-d/width,0,1)**2*np.sin(np.pi*u)**.8*strength)
 a*=seam*(1-path)*(1-mask)
 pix[:,:,:3]=pix[:,:,:3]*(1-a[:,:,None])+np.array(colour)*a[:,:,None]
# X: one topography-directed ecological color hierarchy, not isolated pigment speckles.
r1=np.exp(-((X-(36+11*np.sin((Y-101)/35)))/29)**2)*np.exp(-((Y-132)/40)**4)
r2=np.exp(-((X+24+6*np.sin(Y/19))/28)**2)*np.exp(-((Y-143)/34)**4)
v1=np.exp(-((X-6)/27)**2)*np.exp(-((Y-112)/32)**2)
ridge=np.clip((r1*.62+r2*.51)*seam*(1-path)*(1-mask*.55),0,.72)
valley=np.clip(v1*.37*seam*(1-path)*(1-mask*.62),0,.45)
strata=.76+.24*np.sin(X*.31+Y*.17+np.sin(Y*.09))*np.sin(Y*.29-X*.11)
ridge*=strata
pix[:,:,:3]=pix[:,:,:3]*(1-ridge[:,:,None])+np.array([.56,.50,.30])*ridge[:,:,None]
pix[:,:,:3]=pix[:,:,:3]*(1-valley[:,:,None])+np.array([.32,.40,.22])*valley[:,:,None]
for name,data,noncolor in [('X slope and canopy painted pigment',pix,False),('X editable slope glaze mask',paint,True),('X editable connected litter mask',mask,True)]:
 im=bpy.data.images.new(name,width=W,height=H);im.colorspace_settings.name='Non-Color' if noncolor else 'sRGB'
 if data.ndim==2:
  a=np.ones((H,W,4),np.float32);a[:,:,:3]=data[:,:,None];data=a
 im.pixels.foreach_set(data.astype(np.float32).ravel())
 im.use_fake_user=True # Retain editable masks when the .blend is reopened
mat=bpy.data.materials['P connected meadow forest floor paths'];bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images['X slope and canopy painted pigment'];mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
assert all(tuple(v.co)==original[i] for i,v in enumerate(terrain.data.vertices[:N]))
scene['finishing_stage']='X interconnected ridges, woodland corridors and ecological material masses'
for im in bpy.data.images:
 if im.size[0]>0 and im.source in {'GENERATED','FILE'}:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.render.filepath=str(out/'blender-sector.png');bpy.ops.render.render(write_still=True)
(out/'sector-authoring.json').write_text(json.dumps({'stage':'X','source_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'O_vertices_unchanged':N,'sculpt_brushes':brushes,'new_trees_modified':old_new_sector_count,'new_trees_added':len(new_trees),'total_trees':len(trees)+156,'O_trees_unchanged':156,'art_approved':False,'scope':'Q extension only; W preserved then X large-scale connected ridges, three forest stands, material hierarchy; O 16405 vertices and 156 trees unchanged'},indent=2))
