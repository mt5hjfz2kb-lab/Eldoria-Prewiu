"""T: softened connected hillside and restrained outcrops on Q; O stays untouched."""
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
for name,data,noncolor in [('T slope and canopy painted pigment',pix,False),('T editable slope glaze mask',paint,True),('T editable connected litter mask',mask,True)]:
 im=bpy.data.images.new(name,width=W,height=H);im.colorspace_settings.name='Non-Color' if noncolor else 'sRGB'
 if data.ndim==2:
  a=np.ones((H,W,4),np.float32);a[:,:,:3]=data[:,:,None];data=a
 im.pixels.foreach_set(data.astype(np.float32).ravel())
 im.use_fake_user=True # Retain editable masks when the .blend is reopened
mat=bpy.data.materials['P connected meadow forest floor paths'];bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images['T slope and canopy painted pigment'];mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
assert all(tuple(v.co)==original[i] for i,v in enumerate(terrain.data.vertices[:N]))
scene['finishing_stage']='T broad softened shoulder, cohesive forest crowns, restrained rock scale and slope paint over Q'
for im in bpy.data.images:
 if im.size[0]>0 and im.source in {'GENERATED','FILE'}:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.render.filepath=str(out/'blender-sector.png');bpy.ops.render.render(write_still=True)
(out/'sector-authoring.json').write_text(json.dumps({'stage':'T','source_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'O_vertices_unchanged':N,'sculpt_brushes':brushes,'new_trees_modified':len(trees),'O_trees_unchanged':156,'art_approved':False,'scope':'Q extension only; 3 broader and shallower sculpt strokes, 4 slope pigment strokes, canopy litter and 8 restrained ridge rocks; S retained in native run 39977858676'},indent=2))
