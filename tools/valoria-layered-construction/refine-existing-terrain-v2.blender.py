# Blender 3.4+ art revision of EXISTING ValoriaTerrainRiverCliffV1.blend.
# Source mesh provenance is SHA pinned. Creates a separate EDITABLE .blend and GLB.
# No original asset mutation; no new library or facade terrain hiding collider gaps.
import bpy,sys,os,math,json,hashlib
from mathutils import Vector
src,srcmanifest,out=sys.argv[sys.argv.index("--")+1:]
os.makedirs(out,exist_ok=True)
meta=json.load(open(srcmanifest,encoding='utf-8'))
def sha(f):return hashlib.sha256(open(f,'rb').read()).hexdigest()
if sha(src)!=meta['source_blend_sha256']:raise RuntimeError('TerrainV1 source changed, refuse derivative')
bpy.ops.wm.open_mainfile(filepath=os.path.abspath(src))
old=[o for o in bpy.data.objects if o.type=='MESH']
assert len(old)>=5,(len(old),[x.name for x in old])
new=bpy.data.collections.new('VALORIA_EDITABLE_RIVER_TERRAIN_V2_DO_NOT_PROMOTE')
bpy.context.scene.collection.children.link(new)
def source_clone(o):
    clone=o.copy();clone.data=o.data.copy();clone.name="V2_"+o.name
    new.objects.link(clone);clone.hide_render=False;clone.hide_set(False)
    o.hide_render=True;o.hide_set(True)
    return clone
chosen=[source_clone(o) for o in old]
terrain=next(o for o in chosen if 'LOWER_REAL_TERRAIN_NATIVE_HEIGHTFIELD' in o.name)
water=next(o for o in chosen if 'RIVER_CONTINUOUS_RIPPLED_WATER' in o.name)
rocks=next(o for o in chosen if 'RIVERBANK_AUTHOR_ROCKS' in o.name)
flanks=[o for o in chosen if 'LOWER_TO_UPPER_FACETED_STONE_FLANK' in o.name]
assert len(flanks)==2
mats={m.name:m for m in bpy.data.materials}
def color_material(fragment,rgb,rough):
    for key,m in mats.items():
        if fragment in key:
            m.diffuse_color=(*rgb,1);m.use_nodes=True
            principled=m.node_tree.nodes.get('Principled BSDF')
            if principled:
                principled.inputs['Base Color'].default_value=(*rgb,1)
                principled.inputs['Roughness'].default_value=rough
            return m
    raise RuntimeError('Missing original editable material family '+fragment)
earth=color_material('Earth grassland shaded olive',(.22,.264,.178),.96)
meadow=color_material('Earth sunlit meadow',(.32,.358,.241),.96)
soil=color_material('clay bank exposed soil',(.39,.33,.255),.98)
limestone=color_material('Rocky limestone aged',(.43,.405,.346),.95)
shade=color_material('deep cliff fissures',(.29,.283,.271),.98)
watermat=color_material('River blue-grey',(.069,.174,.225),.30)
rock=color_material('Boulder striated slate',(.365,.361,.329),.96)
# Re-sculpt existing continuous terrain SOURCE mesh. Blender X,-UnityZ,UnityY.
# S-bend in river centre across x, 8m naturalistic banks rather than a 6m straight trench.
def terrain_y(x,z):
    bend=-16.0+1.23*math.sin(x*.13)+.40*math.sin(x*.39)
    d=(z-bend)
    channels=2.27*math.exp(-pow(abs(d)/4.0,3.2))
    geom=.12+.19*math.sin(x*.11+z*.055)+.10*math.cos(x*.23-z*.13)
    micro=.065*math.sin(x*.83+z*.71)*math.cos(z*.63)
    peripheral=max(0.,abs(x)-16.)/16.
    height=geom+.65*peripheral**2+micro-channels
    # Settled bridge abutment ground. Limited to far/near banks; no rise through river.
    for shore in (-19.65,-9.35):
        dist=(z-shore)/1.6
        height+=.11*math.exp(-dist*dist)*math.exp(-(x/6.8)**2)
    return height
for v in terrain.data.vertices:
    x,z=v.co.x,-v.co.y
    v.co.z=terrain_y(x,z)
terrain.data.update()
for poly in terrain.data.polygons:poly.use_smooth=True
# River sheet is within the full-depth channel, with gentle crossflow and bends.
for v in water.data.vertices:
    x,z=v.co.x,-v.co.y
    bend=-16.0+1.23*math.sin(x*.13)+.40*math.sin(x*.39)
    oldcentre=-16+.16*math.sin(.11*x)
    z=z+(bend-oldcentre)
    v.co.y=-z
    v.co.z=-1.83+.035*math.sin(.75*x+z*.5)+.016*math.sin(2.15*x+z*1.1)
water.data.update()
# Replace only the two planar facade meshes with real terraced cliff geometry from the source flank envelopes.
for oldfacade in flanks:
    chosen.remove(oldfacade);bpy.data.objects.remove(oldfacade,do_unlink=True)
# Horizontal stair throat centred at x=0 must stay open (between ±2.03 m).
# New facets have multiple near-depth bands and smaller shoulders rather than straight polygon sheets.
def B(x,y,z):return (x,-z,y)
def make_mesh(name,verts,faces,mix):
    data=bpy.data.meshes.new(name+'_editable_mesh')
    data.from_pydata([B(*v) for v in verts],[],faces);data.update(calc_edges=True)
    obj=bpy.data.objects.new(name,data);new.objects.link(obj)
    data.materials.append(limestone);data.materials.append(shade);data.materials.append(soil)
    for i,p in enumerate(data.polygons):
        p.material_index=mix[i]
        p.use_smooth=(p.material_index!=1)
    uv=data.uv_layers.new(name='MetreMapped_UV')
    for poly in data.polygons:
        for idx in poly.loop_indices:
            p=data.vertices[data.loops[idx].vertex_index].co
            uv.data[idx].uv=(p.x*.21,(p.z if abs(poly.normal.z)<.55 else p.y)*.21)
    chosen.append(obj)
    return obj
for side in (-1,1):
    cols=55;rows=14
    xmin,xmax=(2.05,13.8) if side>0 else (-13.8,-2.05)
    verts=[]
    for j in range(rows+1):
        t=j/rows
        y=.14+2.58*t
        for i in range(cols+1):
            x=xmin+(xmax-xmin)*i/cols
            # Wide-shelf bends; positions physical and repeatable (not random coordinates).
            bend=.34*math.sin(x*.34)+.24*math.sin(x*.87+.4)
            strata=.11*math.sin(j*3.9+i*.77)+.045*math.cos(i*1.41-j*.91)
            near=1.80+3.15*t + bend + strata + .27*math.sin(math.pi*t)*math.sin(x*.61)
            # Lower stones flare outward, upper face ties back into original L1 support.
            if t<.18:near-=.24*(.18-t)/.18
            verts.append((x,y,near))
    faces=[];mix=[]
    for j in range(rows):
      for i in range(cols):
        a=j*(cols+1)+i;b=a+cols+1
        faces.extend([(a,b,b+1),(a,b+1,a+1)])
        color=1 if (j%4==1 and (i+j)%3==0) else (2 if j<2 and i%4==0 else 0)
        mix.extend([color,color])
    make_mesh('VALORIA_V2_TERRACED_LIMESTONE_CLIFF_'+str(side),verts,faces,mix)
# Reposition individual riverbed boulders only if they were floating after the sculpt.
# The V1 boulders were joined as a source mesh, so whole mesh is conserved as authoring provenance.
# Smaller natural sediment ledges grounded near the two bridge banks (not extra architecture).
for side in (-1,1):
    for shore in (-19.75,-9.45):
        x=side*2.85
        z=shore
        foot=terrain_y(x,z)
        verts=[(x-1,foot-.19,z-.86),(x+1,foot-.19,z-.86),
               (x+.90,foot-.19,z+.83),(x-.87,foot-.19,z+.83),
               (x-.75,foot+.38,z-.6),(x+.75,foot+.38,z-.6),
               (x+.72,foot+.38,z+.61),(x-.72,foot+.38,z+.61)]
        faces=[(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7),(4,5,6,7)]
        make_mesh('VALORIA_V2_REAL_BRIDGE_BANK_STONE_'+str(side)+'_'+str(int(shore)),
                   verts,faces,[0]*len(faces))
# Editable source variant and export only cloned / newly created meshes.
for ob in bpy.data.objects:ob.select_set(False)
for ob in chosen:ob.hide_set(False);ob.hide_render=False;ob.select_set(True)
bpy.context.view_layer.objects.active=chosen[0]
path=os.path.join(out,'ValoriaTerrainRiverCliffV2.blend')
bpy.ops.wm.save_as_mainfile(filepath=path,compress=True)
glb=os.path.join(out,'ValoriaTerrainRiverCliffV2.glb')
bpy.ops.export_scene.gltf(filepath=glb,export_format='GLB',use_selection=True,export_apply=True)
triangles=sum(sum(len(p.vertices)-2 for p in ob.data.polygons) for ob in chosen)
assert len(chosen)>=8 and 13000<triangles<40000,(len(chosen),triangles)
manifest={
 'original_source_blend_sha256':meta['source_blend_sha256'],
 'original_source_glb_sha256':meta['export_glb_sha256'],
 'source_blend_v2_sha256':sha(path),'source_glb_v2_sha256':sha(glb),
 'original_preserved':True,'edited_original_scene':False,
 'river_center_function':'-16+1.23sin(0.13x)+0.40sin(0.39x)',
 'mesh_types':[ob.name for ob in chosen],
 'mesh_count':len(chosen),'triangles':triangles,
 'bridge_visual_overlap_only':True,'original_collider_changes':False,
 'stair_throat_open_width':4.10,'classification':'PRODUCTION SOURCE CANDIDATE / ART NOT APPROVED',
 'beauty_runtime':'Actual SHARP camera required before promotion'
}
open(os.path.join(out,'ValoriaTerrainRiverCliffV2-manifest.json'),'w').write(json.dumps(manifest,indent=2,ensure_ascii=False)+'\n')
print('VALORIA_TERRAIN_V2_EDITABLE_BLENDER_PASS',json.dumps({
 'sha_glb':manifest['source_glb_v2_sha256'],'tris':triangles,
 'mesh_count':len(chosen),'v1_original_unchanged':True}))
