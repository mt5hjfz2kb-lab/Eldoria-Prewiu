#!/usr/bin/env python3
"""GPT-6-directed terrain finishing with real 1K CC0 Poly Haven photographs.

The artist's decision is earth/leaf-litter on buildable terraces; mossed cracked
schist on cliffs; aerial grass rock outside the city. Existing material names,
polygons, faces, UVs and gameplate topology are unchanged. This is material
lookdev only, NOT a Unity ART_APPROVED asset or fake Qwen geometry.
"""
import argparse
import hashlib
import json
import pathlib
import sys

import bpy

def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
SLOTS={
 'PROTOTYPE terrace earth olive grass':('forest_ground_04',2.3,.75),
 'Valoria schist dark fractured cliff':('mossy_rock',3.2,.95),
 'Valoria forested valley ground':('aerial_grass_rock',1.0,.72)
}
def texture_nodes(material,asset,folder,uv_scale,normal_strength):
    material.use_nodes=True
    nodes=material.node_tree.nodes
    nodes.clear()
    links=material.node_tree.links
    output=nodes.new('ShaderNodeOutputMaterial');output.location=(680,90)
    shader=nodes.new('ShaderNodeBsdfPrincipled');shader.location=(380,90)
    shader.inputs['Roughness'].default_value=.9
    shader.inputs['Metallic'].default_value=0.
    links.new(shader.outputs['BSDF'],output.inputs['Surface'])
    coords=nodes.new('ShaderNodeTexCoord');coords.location=(-900,150)
    mapping=nodes.new('ShaderNodeMapping');mapping.location=(-670,150)
    mapping.inputs['Scale'].default_value=(uv_scale,uv_scale,1.)
    links.new(coords.outputs['UV'],mapping.inputs['Vector'])
    results={}
    for idx,(suffix,space) in enumerate((('diff','sRGB'),('rough','Non-Color'),('normal','Non-Color'))):
        file=folder/asset/(suffix+'.jpg')
        if not file.is_file() or file.stat().st_size>8_000_000 or file.stat().st_size<5000:
            raise ValueError('Missing approved CC0 1K PBR image: '+str(file))
        image=bpy.data.images.load(str(file.resolve()),check_existing=True)
        image.name='Poly Haven CC0 '+asset+' '+suffix+' 1K'
        image.colorspace_settings.name=space
        image.pack()
        n=nodes.new('ShaderNodeTexImage');n.location=(-420,230-idx*290);n.image=image
        links.new(mapping.outputs['Vector'],n.inputs['Vector'])
        results[suffix]=n
    links.new(results['diff'].outputs['Color'],shader.inputs['Base Color'])
    links.new(results['rough'].outputs['Color'],shader.inputs['Roughness'])
    norm=nodes.new('ShaderNodeNormalMap');norm.location=(100,-245)
    norm.inputs['Strength'].default_value=normal_strength
    links.new(results['normal'].outputs['Color'],norm.inputs['Color'])
    links.new(norm.outputs['Normal'],shader.inputs['Normal'])
    return {suffix:sha(folder/asset/(suffix+'.jpg')) for suffix in ('diff','rough','normal')}

def main():
    ap=argparse.ArgumentParser()
    ap.add_argument('--source',required=True,type=pathlib.Path)
    ap.add_argument('--source-sha256',required=True)
    ap.add_argument('--texture-dir',required=True,type=pathlib.Path)
    ap.add_argument('--out',required=True,type=pathlib.Path)
    ap.add_argument('--target',required=True)
    a=ap.parse_args(sys.argv[sys.argv.index('--')+1:])
    if not a.source.is_file() or sha(a.source)!=a.source_sha256:
        raise ValueError('No authentic frozen terrain source')
    if a.out.resolve()==a.source.parent.resolve():
        raise ValueError('Do not overwrite original source folder')
    a.out.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.open_mainfile(filepath=str(a.source.resolve()))
    ob=bpy.data.objects.get(a.target)
    if not ob or ob.type!='MESH' or len(ob.data.vertices)!=12601 or len(ob.data.polygons)!=23296:
        raise ValueError('Frozen source terrain changed')
    mesh=ob.data
    vertex_sha=hashlib.sha256(str([tuple(v.co) for v in mesh.vertices]).encode()).hexdigest()
    polygon_slots=[face.material_index for face in mesh.polygons]
    uv_sha=hashlib.sha256(str([[tuple(v.uv) for v in layer.data] for layer in mesh.uv_layers]).encode()).hexdigest()
    seen={}
    for material in mesh.materials:
        if material.name in SLOTS:
            asset,uv_scale,normal_strength=SLOTS[material.name]
            seen[material.name]={
              'asset':asset,'license':'CC0 1.0',
              'official_source':'https://polyhaven.com/a/'+asset,
              'api_credit':'Powered by Poly Haven',
              'texture_sha256':texture_nodes(material,asset,a.texture_dir,uv_scale,normal_strength)}
    if set(seen)!=set(SLOTS):raise ValueError('Original source material slots not found: '+str(set(SLOTS)-set(seen)))
    if (vertex_sha!=hashlib.sha256(str([tuple(v.co) for v in mesh.vertices]).encode()).hexdigest()
        or polygon_slots!=[face.material_index for face in mesh.polygons]
        or uv_sha!=hashlib.sha256(str([[tuple(v.uv) for v in layer.data] for layer in mesh.uv_layers]).encode()).hexdigest()):
        raise ValueError('Material finishing altered geometry, surface slots or UVs')
    outfile=a.out/'artist-output.blend'
    bpy.ops.wm.save_as_mainfile(filepath=str(outfile.resolve()),compress=True)
    report={'artist':'GPT-6 AI art direction with Poly Haven CC0 original 1K PBR maps',
       'source_blend_sha256':sha(a.source),'result_blend_sha256':sha(outfile),
       'material_variants':seen,'vertices':12601,'faces':23296,
       'geometry_modified':False,'uv_modified':False,'polygon_material_slots_modified':False,
       'art_approved':False,'unity_verified':False}
    (a.out/'cc0-material-proof.json').write_text(json.dumps(report,indent=2)+'\n')
    print('GPT6_CC0_ORIGINAL_TERRAIN_PBR_MATERIALS_APPLIED',json.dumps(report))
if __name__=='__main__':main()
