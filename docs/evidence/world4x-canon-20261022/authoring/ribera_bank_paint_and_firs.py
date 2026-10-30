"""O: bank-attached painted vegetation masks and production use of K's sculpted fir assets."""
import bpy,sys,pathlib,hashlib,json,math
import numpy as np
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);src=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='0f0d4cf3b84d7ebedfdbb776b78e104cdc8efa798f0fc826de2884be428998e8'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene
collection=bpy.data.collections.new('15 O bank paint masks and forest composition');scene.collection.children.link(collection)
base=bpy.data.images['I recovered exact retained surface'];S=base.size[0];ground=np.array(base.pixels[:],np.float32).reshape(S,S,4)
soil_im=bpy.data.images['M alluvial clay and mineral grain'];soil=np.array(soil_im.pixels[:],np.float32).reshape(S,S,4)
W=2048;H=512;u=np.linspace(0,192,W);v=np.linspace(0,4,H);X=np.broadcast_to((u-96)[None,:],(H,W));V=np.broadcast_to((v/4)[:,None],(H,W))
# Each brush is connected to the grass crown. Gaps deliberately retain bare erosion, stones and wet toes.
brushes={-1:[(-79,7,.34),(-64,4,.23),(-45,9,.45),(-28,7,.65),(-11,5,.27),(11,8,.36),(34,7,.53),(49,9,.60),(72,6,.25),(90,5,.38)],1:[(-89,6,.26),(-68,8,.55),(-43,5,.32),(-22,10,.38),(-3,6,.45),(13,9,.66),(31,5,.27),(50,7,.32),(65,9,.39),(86,6,.29)]}
records=[]
def image(name,pixels):
 h,w=pixels.shape[:2];im=bpy.data.images.new(name,width=w,height=h);im.colorspace_settings.name='sRGB';im.pixels.foreach_set(pixels.astype(np.float32).ravel());return im
for side in [-1,1]:
 bank=bpy.data.objects['Pilot_Cutbank_'+str(side)];co=np.array([tuple(p.co) for p in bank.data.vertices]).reshape(193,5,3)
 # Rasterize the existing editable bank surface into its own atlas; retain exact world pigment at crown.
 yi=np.stack([np.interp(u,np.arange(193),co[:,j,1]) for j in range(5)])
 Y=np.empty((H,W))
 for row,t in enumerate(v):
  j=min(3,int(t));f=t-j;Y[row]=yi[j]*(1-f)+yi[j+1]*f
 ix=np.clip(((X+96)/192*(S-1)).astype(int),0,S-1);iy=np.clip(((Y+78)/156*(S-1)).astype(int),0,S-1)
 coverage=np.full(W,.035)
 for x,r,depth in brushes[side]:
  # Rounded hand-brush profile with an asymmetric pulled end, no stochastic spots.
  t=(u-96-x)/r;profile=np.maximum(0,1-t*t);profile*=np.where(t>0,1-.24*t,1)
  coverage=np.maximum(coverage,depth*profile)
 mask=np.clip((coverage[None,:]-V)/.055+.5,0,1);mask=mask*mask*(3-2*mask)
 pigment=soil[iy,ix].copy();pigment[:,:,:3]*=(1-.22*np.clip((V-.73)/.27,0,1))[:,:,None]
 pigment[:,:,:3]=pigment[:,:,:3]*(1-mask[:,:,None])+ground[iy,ix,:3]*mask[:,:,None];pigment[:,:,3]=1
 im=image('O painted bank '+str(side),pigment);maskrgba=np.ones((H,W,4));maskrgba[:,:,:3]=mask[:,:,None];maskim=image('O editable grass mask '+str(side),maskrgba)
 mat=bank.data.materials[0].copy();mat.name='O painted grass earth wet toe '+str(side);bank.data.materials[0]=mat;bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;tex.label='Editable bank pigment composed from crown mask';mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);mn=mat.node_tree.nodes.new('ShaderNodeTexImage');mn.image=maskim;mn.label='SOURCE mask: paint this to edit grass reaches'
 uv=bank.data.uv_layers.active;uv.name='O bank paint atlas'
 for p in bank.data.polygons:
  for li in p.loop_indices:
   k=bank.data.loops[li].vertex_index;uv.data[li].uv=((k//5)/192,(k%5)/4)
 # Mask boundary guide remains editable in native Blender, grounded on actual bank cross-sections.
 curve=bpy.data.curves.new('O grass crown guide '+str(side),'CURVE');curve.dimensions='3D';s=curve.splines.new('BEZIER');s.bezier_points.add(48)
 for point,i in zip(s.bezier_points,range(0,193,4)):
  t=float(coverage[int(i/192*(W-1))])*4;j=min(3,int(t));f=t-j;xyz=co[i,j]*(1-f)+co[i,j+1]*f;point.co=xyz;point.handle_left_type='AUTO';point.handle_right_type='AUTO'
 guide=bpy.data.objects.new('Editable O grass edge '+str(side),curve);collection.objects.link(guide);guide.hide_render=True
 records.append({'bank':side,'crown_attached_brushes':brushes[side],'mask':maskim.name,'pigment':im.name})
# K proved the angular bough silhouettes on 22 accents. Apply these reusable assets to the remaining dominant round crowns.
family=[bpy.data.meshes['K Layered fir editable branch masses'],bpy.data.meshes['K Slender fir editable branch masses']]
trees=[o for o in scene.objects if o.name.startswith('Pilot_Pine') and o.type=='MESH' and not o.hide_render];assigned=[]
for o in trees:
 if o.data in family:continue
 nearest=min((o.location-t.location).length for t in trees if t!=o)
 # Dense stand interiors: slender habit. Open edges and isolated trees: layered spreading boughs.
 f=1 if nearest<5.8 else 0
 o.data=family[f];assigned.append({'name':o.name,'family':o.data.name,'nearest_neighbour_metres':nearest})
scene['finishing_stage']='O painted crown-to-bank transitions and sculpted fir families on M';scene['O_source_sha256']=hashlib.sha256(src.read_bytes()).hexdigest()
for im in bpy.data.images:
 if im.size[0]>0 and im.source in {'GENERATED','FILE'}:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'O','source_sha256':scene['O_source_sha256'],'painted_banks':records,'sculpted_family_assignments':assigned,'rejected':'N transparent physical bed: murky smooth appearance unlike canonical blue stream','retained':'M relief and all K/I ground and water pigment, paths, pine transforms, gameplay, cameras','art_approved':False},indent=2))
