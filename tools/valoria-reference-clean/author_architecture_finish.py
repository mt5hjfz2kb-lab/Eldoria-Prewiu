"""Measured bridge abutment returns: profiled masonry, not replacement architecture.
Independent director-authored Blender mesh operations; no new AI inference.
"""
import bpy,bmesh,json,math,hashlib,pathlib
import numpy as np
from mathutils import Vector
ROOT=pathlib.Path.cwd();EV=ROOT/'docs/evidence/valoria-reference-clean/architecture201'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
survey=json.loads((EV/'survey.json').read_text());old=json.loads((ROOT/'docs/evidence/valoria-reference-clean/terrain-bank192/survey.json').read_text());candidate=json.loads((ROOT/'docs/evidence/valoria-reference-clean/terrain-bank192/candidate.json').read_text())
assert survey['reference_sha256']=='86fbffa9410514e935b121a844802573cbb7f94c73abb7358ce2f8ee15845c5d'
bpy.ops.wm.read_factory_settings(use_empty=True)
def vec(p):return [p['x'],p['y'],p['z']]
s=old['terrain'];n=s['resolution'];origin=np.array(vec(s['origin']));size=np.array(vec(s['size']));height=np.array(candidate['heights']).reshape(n,n)
def ground(x,z):
 fx=np.clip((x-origin[0])/size[0]*(n-1),0,n-1);fz=np.clip((z-origin[2])/size[2]*(n-1),0,n-1);ix=int(fx);iz=int(fz);u=fx-ix;v=fz-iz;jx=min(ix+1,n-1);jz=min(iz+1,n-1)
 return origin[1]+size[1]*((height[iz,ix]*(1-u)+height[iz,jx]*u)*(1-v)+(height[jz,ix]*(1-u)+height[jz,jx]*u)*v)
angle=math.radians(14);u=np.array([math.cos(angle),-math.sin(angle)]);v=np.array([math.sin(angle),math.cos(angle)]);center=np.array([-1.5,-10.0])
def world(across,along):return center+u*across+v*along
# Real source context remains editable and separate from new export geometry.
for part in survey['parts']:
 if not part.get('vertices'):continue
 mesh=bpy.data.meshes.new(part['name']);mesh.from_pydata([(p['x'],-p['z'],p['y']) for p in part['vertices']],[],np.array(part['triangles']).reshape(-1,3).tolist());mesh.update();obj=bpy.data.objects.new('SOURCE_'+part['name'],mesh);bpy.context.collection.objects.link(obj);obj.hide_render=True
mat=bpy.data.materials.new('Existing sandstone blocks CC0');mat.use_nodes=True;nodes=mat.node_tree.nodes;bs=next((n for n in nodes if n.type=='BSDF_PRINCIPLED'),None) or nodes.new('ShaderNodeBsdfPrincipled');out=next((n for n in nodes if n.type=='OUTPUT_MATERIAL'),None) or nodes.new('ShaderNodeOutputMaterial');mat.node_tree.links.new(bs.outputs['BSDF'],out.inputs['Surface']);tex=nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(ROOT/'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/MaterialInputs/sandstone_blocks_05/sandstone_blocks_05_diff_1k.png'));tex.image.pack();mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);bs.inputs['Roughness'].default_value=.85
pieces=[]
for end,base,sign in [('north',-1,1),('south',-23,-1)]:
 for side in [-1,1]:
  # Curved/splayed retaining cheek lies outside original deck clearway.
  outline=[(3.25,-2.3),(4.35,-2.3),(4.85,-.9),(5.85,1.3),(5.75,2.2),(3.25,2.2)]
  if side<0:outline=[(-a,b) for a,b in outline][::-1]
  pts=[world(a,base+sign*b) for a,b in outline]
  if sign<0:pts=pts[::-1]
  floor=min(ground(x,z) for x,z in pts)-.20;top=.39
  if floor>top-.24:floor=top-.24
  floor=max(floor,-3.5);rows=max(1,math.ceil((top-floor)/.42));dy=(top-floor)/rows
  verts=[];faces=[]
  centroid=np.mean(pts,axis=0)
  for row in range(rows):
   low=floor+row*dy+.009;high=floor+(row+1)*dy-.009
   # Bottom batter, recessed bed joint, projecting dressed cap, bevelled arris.
   taper=1+.055*(1-(row+.5)/rows);cap=row==rows-1
   rings=[(low,.985),(low+.035,1.015 if cap else 1),(high-.035,1.015 if cap else 1),(high,.985)]
   start=len(verts)
   for yy,factor in rings:
    for p in pts:
     x,z=centroid+(p-centroid)*taper*factor;verts.append((float(x),float(-z),float(yy)))
   count=len(pts);faces.append(tuple(start+i for i in reversed(range(count))))
   for k in range(3):
    for i in range(count):j=(i+1)%count;faces.append((start+k*count+i,start+k*count+j,start+(k+1)*count+j,start+(k+1)*count+i))
   faces.append(tuple(start+3*count+i for i in range(count)))
  mesh=bpy.data.meshes.new('Dressed_abutment_'+end+str(side));mesh.from_pydata(verts,[],faces);mesh.update();obj=bpy.data.objects.new(mesh.name,mesh);bpy.context.collection.objects.link(obj);obj.data.materials.append(mat)
  bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bmesh.ops.triangulate(bm,faces=bm.faces);bm.to_mesh(mesh);bm.free();mesh.update()
  uv=mesh.uv_layers.new(name='Stone metre UV')
  for p in mesh.polygons:
   axis=max(range(3),key=lambda a:abs(p.normal[a]));axes=[a for a in range(3) if a!=axis]
   for li in p.loop_indices:co=mesh.vertices[mesh.loops[li].vertex_index].co;uv.data[li].uv=(co[axes[0]]/1.8,co[axes[1]]/1.8)
  # Split loop vertices keeps hard dressed edges and exact per-face UVs.
  outv=[];outuv=[];tris=[]
  for p in mesh.polygons:
   for li in p.loop_indices:
    co=mesh.vertices[mesh.loops[li].vertex_index].co;outv.append({'x':co.x,'y':co.z,'z':-co.y});uvv=uv.data[li].uv;outuv.append({'x':uvv.x,'y':uvv.y});tris.append(len(tris))
  pieces.append({'name':obj.name,'vertices':outv,'uv':outuv,'triangles':tris})
assert sum(len(p['triangles'])//3 for p in pieces)<6000
bpy.context.preferences.filepaths.save_version=0;bpy.ops.wm.save_as_mainfile(filepath=str(EV/'BridgeAbutments.blend'),compress=True)
result={'scene_sha256':survey['scene_sha256'],'survey_sha256':sha(EV/'survey.json'),'pieces':pieces,'art_approved':False};(EV/'candidate.json').write_text(json.dumps(result,separators=(',',':')))
(EV/'author-proof.json').write_text(json.dumps({'author':'Independent director; no Qwen inference or auto approval','operation':'Measured deck-axis splayed polygon extrusion, battered course profile, recessed bed joints, bevel rings, face normals, metre UV, terrain-buried foot','triangles':sum(len(p['triangles'])//3 for p in pieces),'clearway_min_halfwidth_m':3.25,'blend_sha256':sha(EV/'BridgeAbutments.blend'),'candidate_sha256':sha(EV/'candidate.json'),'scene_sha256':survey['scene_sha256'],'published':False},indent=2))
print('VALORIA_ARCHITECTURE_BLENDER_AUTHOR_COMPLETE')
