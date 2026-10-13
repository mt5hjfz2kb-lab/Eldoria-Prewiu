#!/usr/bin/env python3
"""Merge verified GPT-6 IA directed art with original Bastion I city, not an ART_PASS.

Distinct from the strict Qwen-original plan consumer: the Blender sculpture is
GPT-6 art direction from a genuine Qwen critique. Never label it as authored
Qwen coordinates. Preserve scene architecture, parcels, terrain material slots
and UV mapping. Use a real camera-locked visual check BEFORE invoking this.
"""
import argparse
import hashlib
import json
import math
import pathlib
import sys

import bpy

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    p=argparse.ArgumentParser()
    for name in ('source','art','art-proof','source-sha','target','out'):
        p.add_argument('--'+name,required=True)
    a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
    source=pathlib.Path(a.source)
    artist=pathlib.Path(a.art)
    proof_path=pathlib.Path(a.art_proof)
    out=pathlib.Path(a.out)
    if not source.is_file() or not artist.is_file() or not proof_path.is_file():
        raise ValueError('Missing real original 3D source, authored output or evidence')
    if sha(source)!=a.source_sha:
        raise ValueError('Original city source SHA mismatch')
    proof=json.loads(proof_path.read_text('utf8'))
    if (proof.get('art_direction_author')!='GPT-6 AI art direction'
        or proof.get('art_approved') is not False
        or proof.get('unity_verified') is not False
        or proof.get('result_blend_sha256')!=sha(artist)
        or not proof.get('original_material_slots_preserved')
        or not proof.get('original_uv_preserved')
        or proof.get('protected_cell_changes')!=0
        or proof.get('source_vertices')!=12601
        or proof.get('source_faces')!=23296
        or proof.get('target_object')!=a.target):
        raise ValueError('Incorrect or self-approved original GPT-6 art evidence')
    if out.resolve() in (source.parent.resolve(),artist.parent.resolve()):
        raise ValueError('Original sources must remain frozen and immutable')
    out.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.open_mainfile(filepath=str(source.resolve()))
    original=bpy.data.objects.get(a.target)
    if not original or original.type!='MESH':
        raise ValueError('Protected Bastion I source terrain missing')
    original_count=len(original.data.vertices)
    if original_count!=12601 or len(original.data.polygons)!=23296:
        raise ValueError('Unexpected Bastion I source geometry')
    old_faces=len(original.data.polygons)
    old_mats=list(original.data.materials)
    source_world=original.matrix_world.copy()
    objects_before=sorted((o.name,o.type) for o in bpy.data.objects if o.name!=a.target)
    if len(objects_before)<40:
        raise ValueError('Bastion I architecture absent or incomplete')
    with bpy.data.libraries.load(str(artist.resolve()),link=False) as (src,dst):
        if a.target not in src.objects:
            raise ValueError('GPT-6-authored original target absent')
        dst.objects=[a.target]
    art_obj=dst.objects[0]
    if not art_obj or art_obj.type!='MESH':
        raise ValueError('Authored scarp is not a Blender mesh')
    edited=art_obj.data.copy()
    edited.transform(source_world.inverted()@art_obj.matrix_world)
    if (len(edited.vertices)!=original_count or len(edited.polygons)!=old_faces
        or not edited.uv_layers):
        raise ValueError('Authored terrain changes mesh topology or loses UV')
    material_indices=[poly.material_index for poly in edited.polygons]
    changed=0;protected=0;max_delta=0.
    for before,after in zip(original.data.vertices,edited.vertices):
        delta=(before.co-after.co).length
        if not math.isfinite(delta):
            raise ValueError('Nonfinite GPT-6 art coordinates')
        if delta>.001:changed+=1
        max_delta=max(max_delta,delta)
        if (abs(before.co.x)<22. and -34.<before.co.y<36.
            and before.co.z>-1. and delta>.035):
            protected+=1
    if changed<350 or not 1.<max_delta<=5.05 or protected:
        raise ValueError(f'Failed genuine geometry safety check: {changed=} {max_delta=} {protected=}')
    if changed!=proof['changed_vertices'] or abs(max_delta-proof['max_vertex_delta_m'])>.025:
        raise ValueError('Merged geometry differs from original GPT-6 authored evidence')
    edited.materials.clear()
    for material in old_mats:edited.materials.append(material)
    for poly,slot in zip(edited.polygons,material_indices):poly.material_index=slot
    if len(old_mats)<3:raise ValueError('Original PBR materials absent')
    original.data=edited
    edited.update()
    bpy.context.view_layer.update()
    after_objects=sorted((o.name,o.type) for o in bpy.data.objects if o.name!=a.target)
    if after_objects!=objects_before:
        raise ValueError('Original architecture changed')
    blend=out/'Bastion1GPT6DirectedScene.blend'
    glb=out/'Bastion1GPT6DirectedScene.glb'
    bpy.ops.wm.save_as_mainfile(filepath=str(blend.resolve()),compress=True)
    bpy.ops.export_scene.gltf(filepath=str(glb.resolve()),export_format='GLB',export_cameras=False,export_lights=False)
    result={
        'art_direction_author':'GPT-6 AI art direction',
        'qwen_authorship_claim':False,
        'source_city_sha256':sha(source),
        'original_sculpt_sha256':sha(artist),
        'sculpt_art_proof_sha256':sha(proof_path),
        'output_blend_sha256':sha(blend),
        'output_glb_sha256':sha(glb),
        'target_object':a.target,'changed_vertices':changed,
        'max_vertex_delta_m':round(max_delta,5),
        'protected_cell_changes':0,
        'materials_uv_preserved':True,'architecture_unchanged':True,
        'gameplay_qa':False,'unity_a_b_verified':False,'visual_art_approved':False
    }
    (out/'integration-proof.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf8')
    print('GPT6_AI_DIRECTED_SOURCE_LOCKED_BASTION_FULL_CITY',json.dumps(result))
if __name__=='__main__':main()
