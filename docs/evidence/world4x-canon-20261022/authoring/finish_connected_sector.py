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
# AA: two diagonal authored shoulders and a carved dry swale. These are actual mesh
# heights, taper to zero at O's seam and at the extension's northern outside edge.
# The central playable datum and all inherited O vertices remain unchanged.
for v in terrain.data.vertices[N:]:
 x,y,z=v.co
 u=max(0,min(1,(y-87)/18));fade=u*u*(3-2*u)
 north=max(0,min(1,(168-y)/15))
 ridge_a=math.exp(-((x+35+7*math.sin((y-119)/27))/18)**2)*math.exp(-((y-126)/38)**4)
 ridge_b=math.exp(-((x-55-9*math.sin((y-116)/26))/22)**2)*math.exp(-((y-133)/37)**4)
 dry_swale=math.exp(-((x-5-7*math.sin((y-116)/24))/17)**2)*math.exp(-((y-133)/31)**4)
 v.co.z+=fade*north*(5.9*ridge_a+6.8*ridge_b-3.3*dry_swale)
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
# Y: author interlocking macro-forest stands, not individual decorative islands.
# Forest massing responds to the ridgelines, keeps three original routes open and preserves O.
for group,(cx,cy,rx,ry,quota) in enumerate([
 (-47,124,23,27,23),(-15,116,25,21,26),(16,144,26,20,28),
 (60,130,26,25,29),(70,104,22,18,21)]):
 made=0
 for attempt in range(3200):
  if made>=quota:break
  angle=float(rng.uniform(0,2*math.pi));radius=math.sqrt(float(rng.random()))
  x=float(cx+rx*radius*math.cos(angle));y=float(cy+ry*radius*math.sin(angle))
  if not(-90<x<90 and 91<y<160):continue
  if any((x-o.location.x)**2+(y-o.location.y)**2<3.7**2 for o in trees):continue
  ix=int((x+96)/192*(W-1));iy=int((y-78)/92*(H-1))
  px=max(2,int(3.7/192*W));py=max(2,int(3.7/92*H))
  if np.max(path[max(0,iy-py):min(H,iy+py+1),max(0,ix-px):min(W,ix+px+1)])>.10:continue
  donor=trees[(group*13+made*7)%old_new_sector_count]
  o=donor.copy();o.data=donor.data;coll.objects.link(o)
  o.name='Pilot_Pine_X_'+str(group)+'_'+str(made)
  o.location=(x,y,ground(x,y))
  scale=float(rng.uniform(.86,1.15));o.scale=donor.scale*scale;o.rotation_euler.z=float(rng.uniform(-math.pi,math.pi))
  trees.append(o);new_trees.append(o);made+=1
 assert made>=min(quota-4,16),('Forest stand cannot be planted without crossing an authored road',group,made)
assert len(trees)+156<=1024,'Pilot atlas capacity; never silently drop authored trees'
# Z: actual editable low understorey and meadow clumps bind the authored pine stands.
# There are no colliders or navigation objects; O's former forest and river are untouched.
tuft_verts=[];tuft_faces=[];tuft_mat=[];shrub_count=0
rng_floor=np.random.default_rng(20261024)
for o in trees:
 cx,cy=o.location.xy
 if cy<92:continue
 for k in range(5):
  a=float(rng_floor.uniform(0,2*math.pi));r=float(rng_floor.uniform(1.1,5.4))
  tx=float(cx+r*math.cos(a));ty=float(cy+r*math.sin(a))
  if not(-92<tx<92 and 90<ty<166):continue
  ix=int((tx+96)/192*(W-1));iy=int((ty-78)/92*(H-1))
  if path[iy,ix]>.07:continue
  base=ground(tx,ty);height=float(rng_floor.uniform(.55,1.55));radius=float(rng_floor.uniform(.65,1.6))
  first=len(tuft_verts)
  # Six-sided irregular leaf mound: broad at the ground, tapered and faceted at crest.
  skew=float(rng_floor.uniform(-1.0,1.0))
  for q in range(6):
   ang=2*math.pi*q/6+skew
   rim=radius*(.79+.20*math.sin(q*2+tx))
   xx=tx+rim*math.cos(ang);yy=ty+rim*math.sin(ang)
   tuft_verts.append((xx,yy,ground(xx,yy)+.04))
  tuft_verts.append((tx+.2*math.sin(skew),ty+.2*math.cos(skew),base+height))
  for q in range(6):tuft_faces.append((first+q,first+(q+1)%6,first+6));tuft_mat.append(shrub_count%3)
  shrub_count+=1
assert shrub_count>350,('Missing ecological woodland mass',shrub_count)
under=bpy.data.meshes.new('Z authored linked woodland understory mesh')
under.from_pydata(tuft_verts,[],tuft_faces);under.update()
for shade in [(0.14,.23,.09),(0.18,.29,.11),(0.23,.34,.14)]:
 mat0=bpy.data.materials.new('Z woodland understory '+str(len(under.materials)))
 mat0.use_nodes=True
 bs0=next(node for node in mat0.node_tree.nodes if node.type=='BSDF_PRINCIPLED')
 bs0.inputs['Base Color'].default_value=(*shade,1)
 bs0.inputs['Roughness'].default_value=.93
 under.materials.append(mat0)
for q,material_index in enumerate(tuft_mat):under.polygons[q].material_index=material_index
undergo=bpy.data.objects.new('Z retained-sector woodland understory',under);coll.objects.link(undergo)

# AB: small linked fields of actual exposed strata along the newly sculpted ridges.
# One editable faceted mesh, zero colliders; all vertices rest on Blender's sculpted terrain.
# Do not lay slabs over an existing path, O ground, or gameplay nodes.
outcrop_centres=[(-72,132),(-61,154),(-54,120),(-43,147),(-31,156),
                  (-26,108),(-14,144),(3,156),(18,149),(30,129),
                  (44,152),(55,111),(62,155),(76,134),(80,155)]
rock_v=[];rock_f=[];rock_material=[];placed=[]
for ixcentre,(cx,cy) in enumerate(outcrop_centres):
 pi=int((cx+96)/192*(W-1));pj=int((cy-78)/92*(H-1))
 if path[pj,pi]>.12:continue
 # Exposed rock can thread between trunks; no level-of-detail collision mesh is added.
 if sum((cx-o.location.x)**2+(cy-o.location.y)**2<3.4**2 for o in trees)>2:continue
 radius=5.7+((ixcentre*7)%5)*.8
 a0=ixcentre*.71
 vertex0=len(rock_v)
 for ring in range(2):
  for k in range(9):
   a=2*math.pi*k/9+a0
   r=radius*(.7+.20*math.sin(k*2.1+ixcentre*.89))*(1 if ring==0 else .57)
   x=cx+r*math.cos(a);y=cy+r*math.sin(a)
   z=ground(x,y)+(.045 if ring==0 else .39+.18*math.sin(k*1.7+ixcentre))
   rock_v.append((x,y,z))
 rock_v.append((cx,cy,ground(cx,cy)+.75+.20*(ixcentre%3)))
 for k in range(9):
  j=(k+1)%9
  rock_f.append((vertex0+k,vertex0+j,vertex0+9+j));rock_material.append(1)
  rock_f.append((vertex0+k,vertex0+9+j,vertex0+9+k));rock_material.append(1)
  rock_f.append((vertex0+9+k,vertex0+9+j,vertex0+18));rock_material.append(0)
 placed.append((cx,cy))
assert len(placed)>=7,('No sufficient terrain-tied rock anatomy authored',placed)
rock_mesh=bpy.data.meshes.new('AB ridge-side exposed strata mesh')
rock_mesh.from_pydata(rock_v,[],rock_f);rock_mesh.update()
for k,(label,rgb) in enumerate([('AB sandstone warm upper facets',(.46,.42,.32)),('AB grey moss-streaked stone flanks',(.32,.35,.29))]):
 mat0=bpy.data.materials.new(label);mat0.use_nodes=True
 bs0=next(n for n in mat0.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
 bs0.inputs['Base Color'].default_value=(*rgb,1);bs0.inputs['Roughness'].default_value=.94
 rock_mesh.materials.append(mat0)
for k,index in enumerate(rock_material):rock_mesh.polygons[k].material_index=index
rock_object=bpy.data.objects.new('AB authored ridge rock shelves',rock_mesh)
coll.objects.link(rock_object)
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
# Z: soften the formerly perfect ribbon edges using continuous, irregular worn turf.
# The existing editable road network remains geometrically identical and walkable.
erosion_wave=.5+.25*np.sin(X*.61+Y*.35+np.sin(Y*.13))+.25*np.cos(X*.43-Y*.54)
frayed=np.clip(path*(1-path)*erosion_wave*1.25,0,.38)*seam
pix[:,:,:3]=pix[:,:,:3]*(1-frayed[:,:,None])+np.array([.37,.43,.23])*frayed[:,:,None]
# Coherent forest understory pigment is grounded in exactly the planted groves, not random meadows.
understain=np.clip(mask*.82*(1-path)*seam,0,.30)
pix[:,:,:3]=pix[:,:,:3]*(1-understain[:,:,None])+np.array([.27,.35,.18])*understain[:,:,None]
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
# AA: landscape-scale weathering follows the sculpted ridge axes and lower hollows.
# Avoid repeated circular stamps and keep trails, grassy glades and O seam readable.
warp=3.0*np.sin(Y*.085)+1.9*np.sin(X*.13+Y*.061)
shoulder1=np.exp(-((X+35+7*np.sin((Y-119)/27)+warp)/25)**2)*np.exp(-((Y-126)/46)**4)
shoulder2=np.exp(-((X-55-9*np.sin((Y-116)/26)+warp)/29)**2)*np.exp(-((Y-133)/40)**4)
damp=np.exp(-((X-5-7*np.sin((Y-116)/24))/22)**2)*np.exp(-((Y-133)/35)**4)
rock_band=np.clip((shoulder1*.68+shoulder2*.66)*seam*(1-path)*(1-mask*.7),0,.83)
rock_variation=.76+.24*(.5+.5*np.sin(X*.23+Y*.31)*np.cos(Y*.27-X*.17))
rock_band*=rock_variation
pix[:,:,:3]=pix[:,:,:3]*(1-rock_band[:,:,None])+np.array([.62,.55,.36])*rock_band[:,:,None]
cool_valley=np.clip(damp*.52*seam*(1-path)*(1-mask*.6),0,.55)
pix[:,:,:3]=pix[:,:,:3]*(1-cool_valley[:,:,None])+np.array([.30,.39,.24])*cool_valley[:,:,None]
# Uneven external path shoulders instead of the perfectly clean sand ribbon.
shift=5
road_outer=np.maximum.reduce((path,np.roll(path,shift,0),np.roll(path,-shift,0),np.roll(path,shift,1),np.roll(path,-shift,1)))
broken=.58+.23*np.sin(X*.36+Y*.77)+.19*np.sin(Y*.51-X*.49)
weather=np.clip((road_outer-path)*broken*seam*(1-mask*.6),0,.50)
pix[:,:,:3]=pix[:,:,:3]*(1-weather[:,:,None])+np.array([.49,.43,.29])*weather[:,:,None]
for name,data,noncolor in [('AA slope and canopy painted pigment',pix,False),('AA editable slope glaze mask',paint,True),('AA editable connected litter mask',mask,True)]:
 im=bpy.data.images.new(name,width=W,height=H);im.colorspace_settings.name='Non-Color' if noncolor else 'sRGB'
 if data.ndim==2:
  a=np.ones((H,W,4),np.float32);a[:,:,:3]=data[:,:,None];data=a
 im.pixels.foreach_set(data.astype(np.float32).ravel())
 im.use_fake_user=True # Retain editable masks when the .blend is reopened
mat=bpy.data.materials['P connected meadow forest floor paths'];bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images['AA slope and canopy painted pigment'];mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
assert all(tuple(v.co)==original[i] for i,v in enumerate(terrain.data.vertices[:N]))
scene['finishing_stage']='AB visible low rock anatomy, AA ridges, meadow palette and canopy on retained O'
for im in bpy.data.images:
 if im.size[0]>0 and im.source in {'GENERATED','FILE'}:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.render.filepath=str(out/'blender-sector.png');bpy.ops.render.render(write_still=True)
(out/'sector-authoring.json').write_text(json.dumps({'stage':'AB','source_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'O_vertices_unchanged':N,'sculpt_brushes':brushes,'new_trees_modified':old_new_sector_count,'new_trees_added':len(new_trees),'new_grounded_shrubs':shrub_count,'exposed_rock_shelves':len(placed),'total_trees':len(trees)+156,'O_trees_unchanged':156,'art_approved':False,'scope':'Q extension only; AB exposed modeled rock shelves over AA physical relief and Z canopy; no gameplay mutations; preserve O/K/I/Z'},indent=2))
