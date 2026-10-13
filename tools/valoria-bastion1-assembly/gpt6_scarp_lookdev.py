#!/usr/bin/env python3
"""Human-reviewable GPT-6 art direction implemented in Blender, from a REAL Qwen diagnosis.

Do not claim Qwen authored these coordinates: Qwen4B diagnosed flat, uniform
scarp erosion/crest transitions in genuine attempt #38384261910 but failed its
independent camera footprint preflight twice. GPT-6 directs this alternate
full-width, source-anchored broad erosion composition. Outputs remain ART_PENDING.
No edits to runtime scripts, plateau parcels, material assignments or vertex IDs.
"""
import argparse
import hashlib
import json
import math
import pathlib
import sys

import bpy

def smoothstep(a,b,v):
    t=max(0.,min(1.,(v-a)/(b-a)))
    return t*t*(3.-2.*t)

def gaussian(x,mu,sigma):
    return math.exp(-.5*((x-mu)/sigma)**2)

def front_scarp_intent(x,y,z):
    """Sculpt ~55m-wide visually readable faces, coves and buttress masses.

    True observed foreground mesh anchors: crest x=-22..22, y=-26, z=-12;
    lower foot x=-20..20, y=-29, z=-18. Gaussian bays are deliberate erosion
    corridors, not arbitrary subdivision noise. Plateau is untouched.
    """
    if z>=-1.0 or y>=-12.0 or y<=-47.0 or abs(x)>=44.0:
        return (0.,0.,0.)
    w=smoothstep(-12.,-23.,y)*(1.-smoothstep(-34.,-46.,y))
    w*=smoothstep(-1.,-6.,z)
    w*=1.-smoothstep(35.,44.,abs(x))
    if w<=0:return (0.,0.,0.)
    # Architectural hierarchy: substantial cliff buttresses around a recessed
    # pair of runoff gullies, not a uniformly planar, square terrain edge.
    buttress=(1.15+
      1.28*gaussian(x,-22.,10.)+
      1.55*gaussian(x,0.,11.)+
      1.18*gaussian(x,22.,8.))
    gullies=1.68*gaussian(x,-11.,3.6)+1.55*gaussian(x,13.,3.7)
    stratification=.48*math.sin(x*.16+.35)+.25*math.sin(x*.29-1.25)
    # -Y protrudes toward the locked player camera, +Y retreats in channels.
    dy=-w*(buttress-gullies+stratification)
    # Geological layering is correlated with major cliff masses; gentle crests
    # and recession into gullies, rather than high-frequency vertex noise.
    dz=w*(.52*math.sin(x*.18-.3)
           -.62*gaussian(x,-11.,4.)
           -.56*gaussian(x,13.,4.4)
           +.38*gaussian(x,2.,9.))
    dx=w*(.16*math.sin(x*.11+z*.17))
    return (dx,dy,dz)

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    p=argparse.ArgumentParser()
    p.add_argument('--source',type=pathlib.Path,required=True)
    p.add_argument('--source-sha256',required=True)
    p.add_argument('--target',required=True)
    p.add_argument('--out',required=True,type=pathlib.Path)
    a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
    if not a.source.is_file() or sha(a.source)!=a.source_sha256:
        raise ValueError('Refuse unverified source .blend')
    if a.out.resolve()==a.source.parent.resolve():
        raise ValueError('Never overwrite original source')
    a.out.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.open_mainfile(filepath=str(a.source.resolve()))
    obj=bpy.data.objects.get(a.target)
    if not obj or obj.type!='MESH':raise ValueError('Target scarp mesh absent')
    mesh=obj.data
    old_count=len(mesh.vertices);old_faces=len(mesh.polygons)
    before_slots=[f.material_index for f in mesh.polygons]
    before_materials=[m.name for m in mesh.materials]
    old_uv_layers=len(mesh.uv_layers)
    original=[tuple(v.co) for v in mesh.vertices]
    if old_count!=12601 or old_faces!=23296:raise ValueError('Unexpected terrain source geometry')
    changed=0;max_delta=0.;protected=0;extent=[]
    for vertex,before in zip(mesh.vertices,original):
        x,y,z=before
        dx,dy,dz=front_scarp_intent(x,y,z)
        magnitude=math.sqrt(dx*dx+dy*dy+dz*dz)
        if not math.isfinite(magnitude):raise ValueError('Invalid art geometry')
        if magnitude>.001:
            vertex.co.x+=dx
            vertex.co.y+=dy
            vertex.co.z+=dz
            changed+=1
            max_delta=max(max_delta,magnitude)
            extent.append((x,y,z))
            if abs(x)<22. and -34.<y<36. and z> -1. and magnitude>.035:
                protected+=1
    mesh.update()
    if changed<350 or max_delta<=1. or max_delta>5.05 or protected:
        raise ValueError(f'Compositional improvement failed source safety limits: {changed=} {max_delta=} {protected=}')
    if (len(mesh.vertices)!=old_count or len(mesh.polygons)!=old_faces or
        [f.material_index for f in mesh.polygons]!=before_slots or
        [m.name for m in mesh.materials]!=before_materials or
        len(mesh.uv_layers)!=old_uv_layers):
        raise ValueError('Never discard terrain faces, original PBR slots or UVs')
    out=a.out/'artist-output.blend'
    bpy.ops.wm.save_as_mainfile(filepath=str(out.resolve()),compress=True)
    proof={
      'art_direction_author':'GPT-6 AI art direction',
      'qwen_role':'Authentic Qwen4B independently documented real visual deficit: uniform flat cliff; Qwen coordinate plans REJECTED, NOT substituted or re-labelled',
      'design':'Wide S-facing stratified scarp with buttresses, paired runoff gullies and asymmetric crests; unchanged buildable plateau',
      'source_blend_sha256':sha(a.source),
      'result_blend_sha256':sha(out),
      'target_object':a.target,
      'source_vertices':old_count,'source_faces':old_faces,
      'changed_vertices':changed,'max_vertex_delta_m':round(max_delta,5),
      'protected_cell_changes':protected,
      'original_material_slots_preserved':True,'original_uv_preserved':True,
      'source_authority':'original Bastion I source, no generated geometry faces',
      'art_approved':False,'unity_verified':False
    }
    (a.out/'gpt6-art-direction.json').write_text(json.dumps(proof,indent=2)+'\n',encoding='utf8')
    print('GPT6_REAL_BLENDER_SOUTH_SCARP_GEO',json.dumps(proof))
if __name__=='__main__':main()
