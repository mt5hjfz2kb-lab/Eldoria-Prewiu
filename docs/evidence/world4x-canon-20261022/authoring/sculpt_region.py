"""Direct DCC editing of the saved Blender scene. Never runs the terrain generator.
Artist-directed lattice composition and mesh sculpting preserve the protected gameplay core.
"""
import bpy,bmesh,sys,pathlib,json,hashlib,math,gzip,struct
import numpy as np
from mathutils import Vector
from mathutils.bvhtree import BVHTree
from mathutils.geometry import barycentric_transform
out,source,cameras_path=map(pathlib.Path,sys.argv[sys.argv.index('--')+1:])
out.mkdir(parents=True,exist_ok=True)
assert not (out/'eldoria-region1-author.blend').exists()
assert hashlib.sha256(source.read_bytes()).hexdigest()=='d7bdbbf91c05db69dc5f8ee1c933de3e98f1a8bdc154abd8efaf3de08917dbee'
bpy.ops.wm.open_mainfile(filepath=str(source.resolve()))
scene=bpy.context.scene;terrain=bpy.data.objects['BLD_Terrain']
operations=[]
def record(operation,objects,reason):operations.append(dict(operation=operation,objects=objects,artistic_intent=reason))
def focus(obj):
    bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj

# Native Blender lattice controls: broaden the developed western territory rather than leaving
# a narrow decorated strip pasted onto an otherwise empty continental rectangle.
# The central 227.5m column and 250m row retain their exact coordinates.
lattice=bpy.data.lattices.new('Continental composition control cage');lattice.points_u=17;lattice.points_v=17;lattice.points_w=2
lattice.interpolation_type_u='KEY_LINEAR';lattice.interpolation_type_v='KEY_LINEAR';lattice.interpolation_type_w='KEY_LINEAR'
cage=bpy.data.objects.new('AUTHOR · western continental shoulder and northern depth',lattice)
scene.collection.objects.link(cage);cage.scale=(1820,2000,200);cage.hide_render=True
west=[-910,-885,-850,-800,-720,-600,-430,-113.75,0,113.75,227.5,341.25,455,568.75,682.5,796.25,910]
north=[-1000,-930,-850,-750,-620,-430,-260,-125,0,125,250,390,580,730,850,950,1000]
for w in range(2):
    for v in range(17):
        for u in range(17):
            p=lattice.points[(w*17+v)*17+u];p.co_deform.x=west[u]/1820;p.co_deform.y=north[v]/2000
cage['intent']='Expand the western geographic shoulder and move the southern sea out of the composition; preserve central real targets.'
objects=[o for o in list(bpy.data.objects) if o.name.startswith('BLD_') and o.type=='MESH']
for obj in objects:
    focus(obj);mod=obj.modifiers.new('Authored continental composition','LATTICE');mod.object=cage;bpy.ops.object.modifier_apply(modifier=mod.name)
# Transform the editable guide curves through the same cage and preserve them as editable curves.
for obj in [o for o in list(bpy.data.objects) if o.type=='CURVE']:
    mod=obj.modifiers.new('Shared continental composition','LATTICE');mod.object=cage
record('Edit native lattice control cage and apply it to the existing Blender meshes',[o.name for o in objects],
       'Broaden the actual developed western land; reduce southern water occupancy; remove the narrow decorated strip while leaving gameplay coordinates unchanged.')

# Native edit-mode operations on existing forest meshes, not a new random tree generator.
# Subdivide each existing mass once, then sculpt a deliberately broken canopy silhouette.
forest=[o for o in objects if o.name.startswith('BLD_ForestMass_')]
sculpt_patterns=[
 [(-.56,.18,12),(-.20,.42,17),(.16,.15,23),(.48,-.15,15),(-.28,-.35,14)],
 [(-.48,-.28,17),(-.16,.15,24),(.27,.36,17),(.48,-.26,13),(.02,-.42,18)],
 [(-.58,.04,14),(-.27,-.32,19),(.06,.30,25),(.43,.12,19),(.27,-.38,14)]]
for i,obj in enumerate(forest):
    focus(obj);bm=bmesh.new();bm.from_mesh(obj.data)
    bmesh.ops.subdivide_edges(bm,edges=list(bm.edges),cuts=2,use_grid_fill=True);bm.to_mesh(obj.data);bm.free();obj.data.update()
    pts=[v.co.copy() for v in obj.data.vertices];xmin=min(p.x for p in pts);xmax=max(p.x for p in pts);ymin=min(p.y for p in pts);ymax=max(p.y for p in pts)
    cx=(xmin+xmax)/2;cy=(ymin+ymax)/2;rx=(xmax-xmin)/2;ry=(ymax-ymin)/2
    # Sculpt the canopy upper surface with five explicit primary peaks and lower linking saddles.
    base=min(p.z for p in pts)
    for v in obj.data.vertices:
        if v.co.z<base+2:continue
        u=(v.co.x-cx)/rx;w=(v.co.y-cy)/ry
        heights=[h*max(0,1-math.hypot((u-a)*1.25,(w-b)*1.4)/.43) for a,b,h in sculpt_patterns[i%3]]
        v.co.z=base+5+max(heights)
    for poly in obj.data.polygons:poly.use_smooth=False
    obj.data.update();obj['author_edit']='Subdivided original canopy, sculpted five uneven peaks and connecting saddles; composition mass, not individual final trees.'
record('Subdivide and sculpt existing canopy masses',[o.name for o in forest],
       'Replace identical flattened domes with tall, uneven forest skylines and lower connected edges, using the existing geographic groups.')

# Sculpt actual terrain peaks using Blender proportional mesh editing, with deliberate asymmetric strokes.
# All strokes lie outside the real node core; no gameplay marker is moved.
strokes=[(-310,405,8,33),(-337,390,4,23),(215,405,10,27),(232,384,5,18),
         (245,118,9,28),(-530,66,8,35),(-480,-365,12,32),(-457,-388,5,19),(-70,-450,9,26)]
for x,y,dz,radius in strokes:
    focus(terrain)
    for v in terrain.data.vertices:v.select=False
    nearest=min(terrain.data.vertices,key=lambda v:(v.co.x-x)**2+(v.co.y-y)**2)
    if nearest.co.z< -8:continue
    nearest.select=True;bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.transform.translate(value=(0,0,dz),use_proportional_edit=True,proportional_edit_falloff='SHARP',proportional_size=radius)
    bpy.ops.object.mode_set(mode='OBJECT')
record('Sculpt ridge shoulders with native proportional edit strokes',[terrain.name],
       'Shape asymmetric modest rock crests instead of applying another global noise function; protect the city and its surrounding targets.')
terrain.data.update()
verts=[v.co.copy() for v in terrain.data.vertices];faces=[tuple(p.vertices) for p in terrain.data.polygons]
bvh=BVHTree.FromPolygons(verts,faces,all_triangles=True)
def hit(x,y):
    x=min(909.999,max(-909.999,float(x)));y=min(999.999,max(-999.999,float(y)))
    h=bvh.ray_cast(Vector((x,y,200)),Vector((0,0,-1)))
    if h[0] is None:raise RuntimeError('Missing authored surface at '+str((x,y)))
    return h
# Keep ground contact after actual vertex edits. No replacement height function.
for obj in objects:
    if obj.name.startswith('BLD_Path') or obj.name=='BLD_BreachFloor':
        for v in obj.data.vertices:v.co.z=hit(v.co.x,v.co.y)[0].z+.12
        obj.data.update()

# Preserve source identity and the complete editable scene, including the control cage and guide curves.
recipe=bpy.data.texts.new('DIRECT_SCULPT_OPERATIONS.py');recipe.write(pathlib.Path(__file__).read_text())
cameras=json.loads(cameras_path.read_text());camera_records={}
for label,m in cameras.items():
    scene.camera=bpy.data.objects[label];scene.render.resolution_x=m['width'];scene.render.resolution_y=m['height'];scene.render.resolution_percentage=100
    scene.render.filepath=str((out/('blender-blockout-'+label+'.png')).resolve());bpy.ops.render.render(write_still=True)
    camera_records[label]={'unity_camera':m,'blender_location':list(scene.camera.location),'blender_rotation_euler':list(scene.camera.rotation_euler),'ortho_scale':scene.camera.data.ortho_scale}
scene.camera=bpy.data.objects['panoramica-horizontal'];scene.render.resolution_x=1280;scene.render.resolution_y=720
bpy.ops.wm.save_as_mainfile(filepath=str((out/'eldoria-region1-author.blend').resolve()))
bpy.ops.object.select_all(action='DESELECT')
for o in objects:o.select_set(True)
bpy.context.view_layer.objects.active=terrain
bpy.ops.export_scene.gltf(filepath=str((out/'RegionOneBlenderWorld.glb').resolve()),export_format='GLB',use_selection=True,export_yup=True,export_apply=True,export_materials='EXPORT',export_cameras=False,export_lights=False)

# Queries are sampled from the edited Blender surface and its retained mesh attributes.
NX,NZ=240,260;WIDTH,DEPTH=1820.,2000.
attrs={name:terrain.data.attributes[name] for name in ['land_mask','river_mask','path_mask','forest_density']}
def attribute_at(h,name):
    poly=faces[h[2]];a,b,c=[verts[i] for i in poly]
    weights=barycentric_transform(h[0],a,b,c,Vector((1,0,0)),Vector((0,1,0)),Vector((0,0,1)))
    return max(0,min(1,sum(weights[j]*attrs[name].data[poly[j]].value for j in range(3))))
heights=[];flags=[]
for y in np.linspace(-1000,1000,NZ+1):
    for x in np.linspace(-910,910,NX+1):
        h=hit(x,y);heights.append(h[0].z);flags.append([attribute_at(h,n) for n in attrs])
raw=struct.pack('<iiiff',0x31424c45,NX+1,NZ+1,WIDTH,DEPTH)+np.array(heights,dtype='<f4').tobytes()+np.round(np.array(flags)*255).astype('uint8').tobytes()
raw+=struct.pack('<ii',0x32564c45,len(verts))+np.array([(v.x,v.z,v.y) for v in verts],dtype='<f4').tobytes()
(out/'RegionOneBlenderQueries.bytes').write_bytes(gzip.compress(raw,mtime=0))
(out/'blender-cameras.json').write_text(json.dumps(camera_records,indent=2))
manifest={'phase':'DIRECT_COMPOSITION_SCULPT_NOT_ART_APPROVED','blender_version':bpy.app.version_string,'source_blend_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'source':'Direct editing of saved Blender mesh; author_region.py not executed','artistic_operations':len(operations),'operations':operations,'terrain_vertices':len(verts),'source_assets':{}}
for name in ['eldoria-region1-author.blend','RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes']:manifest['source_assets'][name]=hashlib.sha256((out/name).read_bytes()).hexdigest()
(out/'blender-authoring-manifest.json').write_text(json.dumps(manifest,indent=2));print('ELDORIA_DIRECT_SCULPT_EXPORT',json.dumps(manifest))
