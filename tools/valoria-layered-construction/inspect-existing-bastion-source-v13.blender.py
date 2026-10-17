"""Native Blender WORLD-space survey of existing V12 plus untouched fortification source families.
NO geometry export or edits. Historical donor material images are license-UNKNOWN.
"""
import bpy,os,json,hashlib
from mathutils import Vector
root=os.getcwd()
d=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016')
out=os.path.join(d,'entry-zone-v13-survey')
os.makedirs(out,exist_ok=True)
sha=lambda p:hashlib.sha256(open(p,'rb').read()).hexdigest()
v12=os.path.join(d,'entry-zone-v12','ValoriaEntryZoneV12.blend')
assert os.path.isfile(v12)
bpy.ops.wm.open_mainfile(filepath=v12)
def measured(obj):
    m=obj.matrix_world.copy()
    pts=[m@v.co for v in obj.data.vertices]
    lo=[float(min(v[i] for v in pts)) for i in range(3)]
    hi=[float(max(v[i] for v in pts)) for i in range(3)]
    return {'world_min_xyz':lo,'world_max_xyz':hi,
      'center_xyz':[(a+b)*.5 for a,b in zip(lo,hi)],
      'size_xyz':[b-a for a,b in zip(lo,hi)],
      'matrix_world':[[float(m[i][j]) for j in range(4)] for i in range(4)],
      'vertices':len(obj.data.vertices),'triangles':sum(len(f.vertices)-2 for f in obj.data.polygons),
      'materials':[m.name for m in obj.data.materials if m],
      'imported_image_names':sorted({n.image.name for mat in obj.data.materials if mat and mat.use_nodes for n in mat.node_tree.nodes if n.type=='TEX_IMAGE' and n.image})}
base_names=('ENTRY_LowerGate','ENTRY_V12','ARCH_FOOT','ValoriaEntry','Bastion','BASTION','HERO','UPPER','COURT','CIVIC')
original=[{'name':o.name,**measured(o)} for o in bpy.data.objects if o.type=='MESH' and any(k in o.name for k in base_names)]
sources=[
 ('bastion-family-v1','BastionFamilyV1.blend'),
 ('defensive-family-v1','Valoria_DefensiveFamily_v1.blend'),
 ('bastion-to-city-architectural-frame-v1','Valoria_BastionToCityArchitecturalFrame_v1.blend')]
result={'status':'NATIVE_WORLD_SURVEY_READ_ONLY',
 'source_v12_sha256':sha(v12),
 'source_gate_and_courtyard':original,
 'families':{},'no_geometry_saved_or_modified':True,
 'license_policy':'Source meshes may be used only if licensed; drop all donor materials/textures of unverified origin and use V7 CC0 PBR.'}
for family,fn in sources:
    path=os.path.join(root,'art-source/valoria/production',family,fn)
    assert os.path.isfile(path),path
    with bpy.data.libraries.load(path,link=False) as (src,dst):
        dst.objects=[name for name in src.objects if name]
    objects=[o for o in dst.objects if o is not None and o.type=='MESH']
    for o in objects:bpy.context.scene.collection.objects.link(o)
    bpy.context.view_layer.update()
    result['families'][family]={'path':os.path.relpath(path,root),
       'sha256':sha(path),'mesh_count':len(objects),
       'objects':[{'name':o.name,**measured(o)} for o in objects]}
    for o in objects:bpy.data.objects.remove(o,do_unlink=True)
    print('VALORIA_V13_FAMILY',family,len(objects))
dest=os.path.join(out,'bastion-source-world-bounds.json')
with open(dest,'w',encoding='utf-8') as file:json.dump(result,file,indent=2)
print('VALORIA_V13_SURVEY_SUCCESS',dest,
 len(original),[(k,v['mesh_count']) for k,v in result['families'].items()])
