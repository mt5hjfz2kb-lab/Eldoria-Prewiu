"""Zero-cost geological fissure candidate; acceptance requires native A/B.
Wolf is the separately pinned CC0 Quaternius model, never overwritten here.
"""
import bpy, math, os
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Resources/BastionIII'
EV=ROOT/'docs/bastion-iii/native'
OUT.mkdir(parents=True,exist_ok=True);EV.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def mat(name,color,rough=.85):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
 m.node_tree.nodes.clear();p=m.node_tree.nodes.new('ShaderNodeBsdfPrincipled');o=m.node_tree.nodes.new('ShaderNodeOutputMaterial');m.node_tree.links.new(p.outputs['BSDF'],o.inputs['Surface'])
 p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=rough
 return m
def mesh(name,verts,faces,material):
 me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update();o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(material);return o
def export(name):
 bpy.ops.object.select_all(action='DESELECT')
 objs=[o for o in bpy.context.scene.objects if o.type=='MESH']
 for o in objs:o.select_set(True)
 bpy.ops.export_scene.gltf(filepath=str(OUT/(name+'.glb')),export_format='GLB',use_selection=True)
 bpy.ops.wm.save_as_mainfile(filepath=str(EV/(name+'.blend')))
rock=mat('Weathered fissure stone',(.18,.16,.19));black=mat('Deep fissure',(.022,.012,.032));violet=mat('Residual ether',(.25,.045,.34))
# Irregular rim strata; silhouette remains ground-bound and elongated.
centres=[(-3.6,-.4),(-2.7,.12),(-1.6,-.18),(-.5,.35),(.65,-.23),(1.7,.12),(2.65,-.3),(3.5,.12)]
for side in [-1,1]:
 verts=[]
 for i,(x,y) in enumerate(centres):
  gap=.14+(.19 if i in [2,3,4,5] else .06)
  for offset,z in [(gap,-.09),(gap+.22,.16+(i%3)*.055),(gap+.68,.02)]:verts.append((x,y+side*offset,z))
 faces=[]
 for i in range(len(centres)-1):
  for k in range(2):faces.append((i*3+k,i*3+k+1,(i+1)*3+k+1,(i+1)*3+k))
 if side>0:faces=[tuple(reversed(f)) for f in faces]
 mesh('Fractured low rim '+str(side),verts,faces,rock)
verts=[]
for i,(x,y) in enumerate(centres):
 gap=.14+(.19 if i in [2,3,4,5] else .06)
 verts.extend([(x,y-gap,.005),(x,y+gap,.005)])
mesh('Dark split floor',verts,[(i*2+2,i*2+3,i*2+1,i*2) for i in range(len(centres)-1)],black)
mesh('Fine ether seam',[(x,y+dy,.012) for x,y in centres for dy in [-.018,.018]],[(i*2+2,i*2+3,i*2+1,i*2) for i in range(len(centres)-1)],violet)
export('PrivateFissure')
print('BASTION_III_AUTHORED_CANDIDATES_EXPORTED')
