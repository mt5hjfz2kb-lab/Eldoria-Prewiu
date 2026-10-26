"""K: sculpt inherited pine branch masses into angular foliage planes; retain I/J geography."""
import bpy,sys,pathlib,hashlib,json,math
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);src=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='36b13b35e8c144e1e02b629e9c24b3527bdb77ba8fd2deb9990f390136c395da'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene
assert scene.get('finishing_stage','').startswith('J authored')
# Visual review rejects J pigment: retain I water/bank paint, preserve J stone groups and source repair.
for name,image_name in [('Pilot_River','E_river_depth_and_current'),('Pilot_Cutbank_-1','Ribera_bank_strata'),('Pilot_Cutbank_1','Ribera_bank_strata')]:
 mat=bpy.data.objects[name].data.materials[0];bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images[image_name];mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
# Four selected bank collapses, sculpted in the existing boundary topology.
# Deliberate asymmetric cross-sections replace continuous wall bands with low earth approaches.
bank_edits=[]
profile=[0,.18,.49,.82,1,.90,.57,.24,0]
for side,cx in [(-1,-30),(1,-5),(-1,22),(1,52)]:
 terrain=bpy.data.objects['Pilot_Terrain_'+str(side)];bank=bpy.data.objects['Pilot_Cutbank_'+str(side)]
 for offset,weight in zip(range(-4,5),profile):
  i=int(cx+96+offset);top=terrain.data.vertices[i*85].co.z;drop=max(0,top-.85)*weight
  for row,falloff in enumerate([1,.78,.51,.27,.10,0]):terrain.data.vertices[i*85+row].co.z-=drop*falloff
  for row,falloff in enumerate([1,.88,.56,.08,0]):bank.data.vertices[i*5+row].co.z-=drop*falloff
  bank.data.vertices[i*5+2].co.y-=side*.55*weight
  bank.data.vertices[i*5+3].co.y-=side*.32*weight
 terrain.data.update();bank.data.update();bank_edits.append({'side':side,'centre_x':cx,'profile':profile,'target_crown_height':.85})

collection=bpy.data.collections.new('12_Editable_branch_sculpt_studies');scene.collection.children.link(collection)
base=bpy.data.objects['Pilot_Pine.003'].data
# Connected branch tufts are native editable islands in the inherited mesh.
adj=[set() for _ in base.vertices]
for e in base.edges:
 a,b=e.vertices;adj[a].add(b);adj[b].add(a)
left=set(range(len(adj)));components=[]
while left:
 first=left.pop();island={first};stack=[first]
 while stack:
  for i in adj[stack.pop()]:
   if i in left:left.remove(i);island.add(i);stack.append(i)
 components.append(island)
foliage=set()
for p in base.polygons:
 if base.materials[p.material_index].name.startswith('Needles'):foliage.update(p.vertices)
records=[];families=[]
for family in ['Layered fir','Slender fir']:
 mesh=base.copy();mesh.name='K '+family+' editable branch masses'
 for island in components:
  if not island.issubset(foliage):continue
  pts=[base.vertices[i].co.copy() for i in island];centre=sum(pts,Vector())/len(pts)
  if centre.z>5.4:continue # preserve the original leader
  radial=Vector((centre.x,centre.y,0));radial.normalize()
  # Sculpt broad overlapping boughs: flattened undersides, raised shoulders and lowered tips.
  for i in island:
   v=mesh.vertices[i];d=base.vertices[i].co-centre;along=d.dot(radial)
   v.co.z=centre.z+(d.z*.25 if d.z<0 else d.z*.95)-along*.38
   # Narrow the transverse leaf mass, keeping its radial reach and attachment.
   tangent=Vector((-radial.y,radial.x,0));v.co-=tangent*(d.dot(tangent)*.16)
   if family=='Slender fir':v.co.x*=.83;v.co.y*=.83;v.co.z*=1.06
  records.append({'family':family,'branch_center':list(centre),'vertices':len(island)})
 for p in mesh.polygons:
  if mesh.materials[p.material_index].name.startswith('Needles'):p.use_smooth=False
 mesh.update();families.append(mesh)
 master=bpy.data.objects.new('Editable '+family+' source',mesh);collection.objects.link(master);master.hide_render=True;master.hide_viewport=True
# Deliberate accent trees and group edge trees, preserving every inherited placement.
assignment={0:['Pilot_Pine.003','Pilot_Pine.008','Pilot_Pine.010','Pilot_Pine.001','Pilot_Pine.017','Pilot_Pine.020','Pilot_Pine.032','Pilot_Pine.037','Pilot_Pine.042','Pilot_Pine.052','Pilot_Pine.074'],1:['Pilot_Pine.002','Pilot_Pine.006','Pilot_Pine.011','Pilot_Pine.019','Pilot_Pine.023','Pilot_Pine.035','Pilot_Pine.039','Pilot_Pine.046','Pilot_Pine.053','Pilot_Pine.068','Pilot_Pine.079']}
assigned=[]
for family,names in assignment.items():
 for name in names:
  o=bpy.data.objects.get(name)
  if o and not o.hide_render:o.data=families[family];assigned.append({'object':name,'family':families[family].name,'position':list(o.location)})
scene['finishing_stage']='K branch-level and bank-collapse sculpt; I surface paint retained; J stones';scene['K_source_sha256']=hashlib.sha256(src.read_bytes()).hexdigest()
# Preserve all generated and file images, including contact bake; reopenable source is mandatory.
for im in bpy.data.images:
 if im.size[0]>0 and im.source in {'GENERATED','FILE'}:
  im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'))
scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'K','source_sha256':scene['K_source_sha256'],'sculpted_branches':records,'assigned_trees':assigned,'bank_sculpt':bank_edits,'rejected':'J pool and bank pigment maps; I materials restored','unchanged':'river course, paths, tree positions, game cameras, gameplay','art_approved':False},indent=2))

(out/'RiberaAssetManifest.json').write_text(json.dumps({'units':'metres','Blender_up':'Z','Unity_up':'Y','origin':[0,0,0],'retained_source_sha256':scene['K_source_sha256'],'families':[{'mesh':m.name,'vertices':len(m.vertices),'polygons':len(m.polygons),'materials':[x.name for x in m.materials]} for m in families],'assignments':assigned,'textures':[{'name':im.name,'dimensions':list(im.size),'packed':bool(im.packed_file)} for im in bpy.data.images if im.size[0]>0 and im.source in {'GENERATED','FILE'}],'gameplay_colliders':0,'hybrid':'existing runtime captures these exact 3D trees into atlas; existing 36-50 fade unchanged'},indent=2))
