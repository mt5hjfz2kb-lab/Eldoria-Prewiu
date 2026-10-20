"""Directed weathering of the CURRENT measured riverbank; no scene reconstruction."""
import bpy,bmesh,json,hashlib,math
from pathlib import Path
from mathutils import Vector
EV=Path('docs/evidence/valoria-reference-clean/terrain-polish218')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
s=json.loads((EV/'survey.json').read_text(encoding='utf-8-sig'))
assert s['scene_sha256']=='305dc0dd0cb010291dcd3aa52bdea12c02c91dd10b0187f167673fab5a48768b'
g=next(g for g in s['meshes'] if g['name']=='River_bank_rocks')
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
# Unity Y-up to Blender Z-up. Reflection requires reversed polygon winding.
v=[(p['x'],p['z'],p['y']) for p in g['vertices']]
f=[tuple(reversed(g['triangles'][i:i+3])) for i in range(0,len(g['triangles']),3)]
m=bpy.data.meshes.new('Retained bank / weathering source');m.from_pydata(v,[],f);m.update()
o=bpy.data.objects.new('Valoria retained riverbank / sculpted weathered edges',m);bpy.context.collection.objects.link(o);bpy.context.view_layer.objects.active=o;o.select_set(True)
bm=bmesh.new();bm.from_mesh(m);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.0005);bmesh.ops.dissolve_degenerate(bm,edges=list(bm.edges),dist=.00001)
bmesh.ops.dissolve_limit(bm,angle_limit=math.radians(1),use_dissolve_boundaries=False,verts=list(bm.verts),edges=list(bm.edges),delimit={'NORMAL'})
bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(m);bm.free();m.update()
source_after_weld=len(m.vertices)
bevel=o.modifiers.new('Eroded convex corners / bounded 38 cm','BEVEL');bevel.width=.38;bevel.segments=2;bevel.limit_method='ANGLE';bevel.angle_limit=math.radians(26);bevel.use_clamp_overlap=True
bpy.ops.object.modifier_apply(modifier=bevel.name)
for p in o.data.polygons:p.use_smooth=True
normal=o.modifiers.new('Preserve broad stone planes / area weighted normals','WEIGHTED_NORMAL');normal.keep_sharp=True;normal.weight=65
bpy.ops.object.modifier_apply(modifier=normal.name)
m=o.data;m.calc_loop_triangles()
assert 10000<len(m.loop_triangles)<160000, len(m.loop_triangles)
# Per-face metric UV, independent of the triangulated source's stretched atlas.
uv=m.uv_layers.new(name='Metric dominant-plane stone UV')
for p in m.polygons:
 axis=max(range(3),key=lambda a:abs(p.normal[a]))
 axes=[i for i in range(3) if i!=axis]
 for li in p.loop_indices:
  q=m.vertices[m.loops[li].vertex_index].co;uv.data[li].uv=(q[axes[0]]*.20,q[axes[1]]*.20)
mat=bpy.data.materials.new('Weathered river stone / retained CC0 rock3');mat.use_nodes=True
nodes=mat.node_tree.nodes;bsdf=next(n for n in nodes if n.type=='BSDF_PRINCIPLED');bsdf.inputs['Roughness'].default_value=.93
tex=nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(Path('Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/MaterialInputs/rock_3/rock_3_diff_1k.png').resolve()));tex.image.pack();mat.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color']);m.materials.append(mat)
# Export split normals explicitly, so Unity does not harden all UV-seam vertices.
m.calc_loop_triangles();vv=[];nn=[];uu=[];tt=[]
for tri in m.loop_triangles:
 for li in reversed(tri.loops):
  p=m.vertices[m.loops[li].vertex_index].co;n=m.corner_normals[li].vector;u=uv.data[li].uv
  tt.append(len(vv));vv.append(dict(x=p.x,y=p.z,z=p.y));nn.append(dict(x=n.x,y=n.z,z=n.y));uu.append(dict(x=u.x,y=u.y))
out={'survey_sha256':sha(EV/'survey.json'),'source_scene_sha256':s['scene_sha256'],'vertices':vv,'normals':nn,'uv':uu,'triangles':tt}
(EV/'bank-weathered.json').write_text(json.dumps(out,separators=(',',':')))
bpy.ops.wm.save_as_mainfile(filepath=str((EV/'RiverbankWeathered.blend').resolve()))
bpy.ops.export_scene.gltf(filepath=str((EV/'RiverbankWeathered.glb').resolve()),export_format='GLB',use_selection=True)
(EV/'authoring-result.json').write_text(json.dumps({'blender':bpy.app.version_string,'source_vertices':len(v),'source_triangles':len(f),'welded_vertices':source_after_weld,'output_vertices':len(vv),'output_triangles':len(tt)//3,'operations':['weld split source topology','dissolve coplanar source triangles','bounded 0.38m two-segment convex bevel','area weighted normals','metric dominant-plane UV'],'candidate_sha256':sha(EV/'bank-weathered.json'),'blend_sha256':sha(EV/'RiverbankWeathered.blend'),'glb_sha256':sha(EV/'RiverbankWeathered.glb'),'collision_modified':False,'art_approved':False},indent=2))
