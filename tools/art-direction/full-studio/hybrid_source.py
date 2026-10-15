"""Blender-only recovery of a missing source atlas, never a new design.

The Unity material GUID identifies the exact existing palette. Work on a copy,
preserve vertices/faces/UV, and export the same normalized source to both arms.
"""
import argparse, hashlib, json, pathlib, sys
import bpy
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent))
import blender_toolbox as studio

def digest(p): return hashlib.sha256(pathlib.Path(p).read_bytes()).hexdigest()
def uv_snapshot():
    return {o.name:[[list(v.uv) for v in layer.data] for layer in o.data.uv_layers]
            for o in bpy.data.objects if o.type=='MESH' and not o.name.startswith('_EldoriaReview')}

def main():
    p=argparse.ArgumentParser()
    for n in ('source','atlas','out','review'):p.add_argument('--'+n,type=pathlib.Path,required=True)
    a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
    if a.out.exists() and any(a.out.iterdir()):raise ValueError('FRESH_OUTPUT_REQUIRED')
    a.out.mkdir(parents=True,exist_ok=True)
    source_sha=digest(a.source)
    if digest(a.atlas)!='139f70a56e475755347b4fcaf4c8640f209b7b49133047d00c3b1f11de277f5c':
        raise ValueError('EXACT_UNITY_GUID_ATLAS_HASH_MISMATCH')
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(a.source.resolve()))
    meshes=studio.snapshot_meshes();uv=uv_snapshot()
    review=json.loads(a.review.read_text())
    studio.render_setup(review['render']);studio.camera_override(review['camera'])
    studio.ensure_review_lighting();studio.ensure_review_stage()
    bpy.context.scene.cycles.seed=0
    bpy.context.scene.render.filepath=str(a.out/'original-missing-atlas.png')
    bpy.ops.render.render(write_still=True)
    mats=[m for m in bpy.data.materials if m.name.startswith('EA02_LPNP-Slavika_Colorsheet_v1.png')]
    if len(mats)!=1:raise ValueError('EXACT_SOURCE_MATERIAL_NOT_FOUND')
    mat=mats[0]
    if any(n.type=='TEX_IMAGE' and n.image for n in mat.node_tree.nodes):
        raise ValueError('SOURCE_ALREADY_HAS_TEXTURE_DO_NOT_OVERWRITE')
    image=bpy.data.images.load(str(a.atlas.resolve()),check_existing=True);image.pack()
    bsdf=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=image
    tex.interpolation='Closest'
    mat.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color'])
    if meshes!=studio.snapshot_meshes() or uv!=uv_snapshot():raise ValueError('SOURCE_GEOMETRY_OR_UV_CHANGED')
    bpy.ops.object.select_all(action='DESELECT')
    for o in bpy.data.objects:
        if o.name in meshes:o.select_set(True)
    dest=a.out/'normalized-source.glb'
    bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True)
    bpy.context.scene.render.filepath=str(a.out/'atlas-restored.png')
    bpy.ops.render.render(write_still=True)
    if source_sha!=digest(a.source):raise ValueError('SOURCE_FILE_CHANGED')
    proof={'source_sha256':source_sha,'atlas_sha256':digest(a.atlas),
      'normalized_source_sha256':digest(dest),'mesh_and_uv_unchanged':True,
      'atlas_unity_material_guid':'4f4a523b69ecaf542bcff16bc65d3002',
      'scope':'SOURCE_TRANSPORT_REPAIR_NOT_ARTISTIC_MODEL_IMPROVEMENT',
      'art_approved':False,'unity_verified':False}
    (a.out/'normalization-proof.json').write_text(json.dumps(proof,indent=2)+'\n')

if __name__=='__main__':main()
