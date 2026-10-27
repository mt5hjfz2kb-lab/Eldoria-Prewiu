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
 # Keep all shallow surfaces front-facing in the actual overhead camera.
 # A reversed winding silently culls the soil shoulder, leaving a bright strip.
 front=[]
 for face in faces:
  v0,v1,v2=(Vector(verts[j]) for j in face[:3])
  normal=(v1-v0).cross(v2-v0)
  front.append(tuple(reversed(face)) if normal.z<0 else face)
 me=bpy.data.meshes.new(name);me.from_pydata(verts,[],front);me.update()
 o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(material);return o
def export(name):
 bpy.ops.object.select_all(action='DESELECT')
 objs=[o for o in bpy.context.scene.objects if o.type=='MESH']
 for o in objs:o.select_set(True)
 bpy.ops.export_scene.gltf(filepath=str(OUT/(name+'.glb')),export_format='GLB',use_selection=True)
 bpy.ops.wm.save_as_mainfile(filepath=str(EV/(name+'.blend')))
# Private fissure is an exceptional fault in the landscape, not a portal.
# Every ring is very shallow. The Unity integration drapes each authored
# vertex onto actual Region 1 terrain without replacing the shared Breach.
# Shallow, staggered natural edges replace the former 0.2m elevated ribbon.
stone=mat('Fault slate darkened by earth',(.205,.195,.195))
earth=mat('Scuffed loamy bank',(.20,.165,.115))
void=mat('Deep unlit soil fissure',(.033,.021,.037))
ether=mat('Rare violet residue',(.13,.039,.15))
chip=mat('Weathered stone fragments',(.26,.25,.245))
# Smooth longitudinal meander with deliberately irregular, tapering widths.
centres=[]
for i in range(19):
 t=i/18.
 x=-3.85+7.7*t
 y=.24*math.sin(2.85*math.pi*t)+.13*math.sin(6.4*math.pi*t+.4)
 taper=max(.035,math.sin(math.pi*t)**.7)
 gap=taper*(.18+.063*math.sin(5.9*math.pi*t)**2)
 centres.append((x,y,gap,taper))
def strip(name,side,first,last,material):
 verts=[]
 for i,(x,y,gap,taper) in enumerate(centres):
  for offset,z in ((first(gap,taper),.017),(last(gap,taper),.006)):
   verts.append((x,y+side*offset,z))
 faces=[]
 for i in range(len(centres)-1):
  face=(i*2,i*2+1,(i+1)*2+1,(i+1)*2)
  faces.append(face if side>0 else tuple(reversed(face)))
 mesh(name,verts,faces,material)
# Each bank follows the soil rather than building an extrusion/retaining wall.
# The stone is a narrow sliver next to the dark crack, its textured shoulder
# gradually dies into the authored grass/soil underneath.
for side in (-1,1):
 strip('Broken slate lip '+str(side),side,
       lambda g,t:g,lambda g,t:g+.18*t,stone)
 strip('Scuffed soil shoulders '+str(side),side,
       lambda g,t:g+.18*t,lambda g,t:g+.58*t,earth)
# A single dark inlaid slit is required by the native terrain-contact proof.
floor=[]
for x,y,gap,taper in centres:
 floor.extend([(x,y-gap,.008),(x,y+gap,.008)])
floor_faces=[(i*2+2,i*2+3,i*2+1,i*2) for i in range(len(centres)-1)]
mesh('Dark split floor',floor,floor_faces,void)
# Discontinuous *thin* remnant veins, never two bright continuous bands.
for seg in (3,8,13):
 v=[]
 for j in (seg,seg+1,seg+2):
  x,y,gap,taper=centres[j]
  centre=.055*math.sin(j*1.9)
  v.extend([(x,y+centre-.018,.014),(x,y+centre+.018,.014)])
 mesh('Faint isolated ether vein '+str(seg),v,
      [(2,3,1,0),(4,5,3,2)],ether)
# Scattered low triangular stone flakes embedded in the two banks.
# The natural surface remains unobstructed (low < 6 cm, no tall walls).
for side in (-1,1):
 vertices=[];faces=[]
 for j in (2,5,9,12,16):
  x,y,gap,taper=centres[j]
  off=side*(gap+.28*taper)
  x+=.09*math.sin(j*1.1)
  y+=off
  k=len(vertices)
  vertices.extend([(x-.16,y-.06,.016),(x+.09,y-.12,.023),
                   (x+.13,y+.09,.042),(x-.09,y+.13,.024)])
  face=(k,k+1,k+2,k+3)
  faces.append(face if side>0 else tuple(reversed(face)))
 mesh('Fault scatter '+str(side),vertices,faces,chip)
export('PrivateFissure')
print('BASTION_III_GEOLOGICAL_GROUND_FISSURE_EXPORTED')
