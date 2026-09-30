import argparse
import os
import sys

import bpy


def reset_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)


def rewrap(path: str):
    reset_scene()
    bpy.ops.import_scene.gltf(filepath=path)
    mesh_objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
    if not mesh_objects:
        raise RuntimeError(f"No mesh objects in {path}")

    for obj in mesh_objects:
        bpy.context.view_layer.objects.active=obj
        obj.select_set(True)
        try:
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        except Exception:
            pass
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        # World-like box projection is intentionally used for tileable stone/earth materials.
        # It avoids the source atlas UVs that stretched Valoria PBR into horizontal bands.
        bpy.ops.uv.cube_project(cube_size=1.25, correct_aspect=True, clip_to_bounds=False)
        bpy.ops.object.mode_set(mode='OBJECT')
        obj.select_set(False)

    tmp=path+'.uvfix.glb'
    bpy.ops.export_scene.gltf(
        filepath=tmp,
        export_format='GLB',
        export_apply=True,
        export_texcoords=True,
        export_normals=True,
        export_materials='EXPORT',
    )
    if not os.path.exists(tmp) or os.path.getsize(tmp) < 1000:
        raise RuntimeError(f"UV-fixed GLB missing/too small: {tmp}")
    os.replace(tmp,path)
    print(f"UV_REWRAPPED {os.path.basename(path)} meshes={len(mesh_objects)} bytes={os.path.getsize(path)}")


def main():
    argv=sys.argv
    argv=argv[argv.index('--')+1:] if '--' in argv else []
    ap=argparse.ArgumentParser()
    ap.add_argument('--input-dir',required=True)
    args=ap.parse_args(argv)
    files=[
        'gate.glb','wall.glb','tower-square.glb',
        'tower-square-top-roof-high-windows.glb',
        'ground-hills.glb','rocks-large.glb'
    ]
    for name in files:
        path=os.path.join(args.input_dir,name)
        if not os.path.exists(path):
            raise FileNotFoundError(path)
        rewrap(path)


if __name__=='__main__':
    main()
