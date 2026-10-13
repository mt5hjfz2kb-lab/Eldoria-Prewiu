"""Consume the completed material experiment, preserving the full city geometry.

This is isolated Unity lookdev, not a Qwen authorship or ART_APPROVED claim.
"""
import argparse, hashlib, json, pathlib, sys
import bpy

def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def main():
    p=argparse.ArgumentParser()
    for name in ('source','materials','proof','out'):
        p.add_argument('--'+name,required=True,type=pathlib.Path)
    a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
    proof=json.loads(a.proof.read_text())
    if sha(a.materials)!=proof['result_blend_sha256'] or proof['geometry_modified']:
        raise ValueError('Completed material source/proof mismatch')
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.gltf(filepath=str(a.source.resolve()))
    target=bpy.data.objects.get('Valoria natural playable hilltop and rock scarps')
    if not target or len(target.data.vertices)!=12601:
        raise ValueError('Exact full-city terrain missing')
    signature=[(o.name,tuple(o.matrix_world),len(o.data.vertices) if o.type=='MESH' else 0) for o in bpy.data.objects]
    geometry=hashlib.sha256(str([(o.name,[tuple(v.co) for v in o.data.vertices]) for o in bpy.data.objects if o.type=='MESH']).encode()).hexdigest()
    slots={m.name:i for i,m in enumerate(target.data.materials)}
    with bpy.data.libraries.load(str(a.materials.resolve()),link=False) as (src,dst):
        names=tuple(proof['material_variants'])
        if any(n not in src.materials for n in names):
            raise ValueError('Packed PBR materials unavailable')
        dst.materials=list(names)
    for name,material in zip(names,dst.materials):
        if name not in slots:raise ValueError('Terrain slot not found: '+name)
        target.data.materials[slots[name]]=material
    current=hashlib.sha256(str([(o.name,[tuple(v.co) for v in o.data.vertices]) for o in bpy.data.objects if o.type=='MESH']).encode()).hexdigest()
    if current!=geometry or signature!=[(o.name,tuple(o.matrix_world),len(o.data.vertices) if o.type=='MESH' else 0) for o in bpy.data.objects]:
        raise ValueError('Material transfer changed geometry or transforms')
    a.out.mkdir(parents=True,exist_ok=True)
    blend=a.out/'Bastion1ReferenceAssembly.blend'
    glb=a.out/'Bastion1ReferenceAssembly.glb'
    bpy.ops.wm.save_as_mainfile(filepath=str(blend.resolve()),compress=True)
    bpy.ops.export_scene.gltf(filepath=str(glb.resolve()),export_format='GLB',export_cameras=False,export_lights=False)
    result={'source_glb_sha256':sha(a.source),'completed_material_blend_sha256':sha(a.materials),
        'output_glb_sha256':sha(glb),'output_blend_sha256':sha(blend),'geometry_unchanged':True,
        'materials_transferred':names,'source_material_run':38388922583,
        'qwen_authorship_claim':False,'art_approved':False,'unity_verified':False}
    (a.out/'cc0-integration-proof.json').write_text(json.dumps(result,indent=2)+'\n')
    print('EXISTING_CC0_MATERIALS_FULL_CITY_TRANSFER',json.dumps(result))

if __name__=='__main__':main()
