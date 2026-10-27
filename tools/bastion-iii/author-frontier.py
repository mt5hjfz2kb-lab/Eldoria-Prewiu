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
# Bastion III is a PRIVATE shallow fault, not a reusable glowing resource.
# Keep an inlaid crack and sparse, naturally broken dirt/slate flakes. Wide
# continuous stone ribbons were rejected in real native19 screenshots.
soil_dark=mat('Old earth dark scuffs',(.17,.135,.09))
soil_warm=mat('Exposed brown earth',(.205,.169,.106))
soil_cool=mat('Damp shadowed earth',(.13,.122,.092))
stone=mat('Scattered weathered slate',(.18,.171,.151))
stone_ochre=mat('Stained stone in grass',(.22,.195,.146))
void=mat('Unlit inner soil split',(.022,.015,.023))
ether=mat('Rare fine fissure afterglow',(.102,.031,.126))

# Tapering edges are asymmetrical and sharply varied, unlike the smooth
# sinusoidal 19-point ribbon. An irregular fault remains subtle from the
# overworld camera while still legible and selectable.
centres=[]
n=43
for i in range(n):
 t=i/(n-1)
 x=-3.85+7.70*t
 y=(.17*math.sin(2.7*math.pi*t+.18)+
    .071*math.sin(13.5*math.pi*t)+.024*math.sin(39*math.pi*t+.5))
 taper=max(.012,math.sin(math.pi*t)**.82)
 gap=taper*(.105+.045*abs(math.sin(5.8*math.pi*t+.9))+.024*math.sin(26.3*t)**2)
 centres.append((x,y,gap,taper))

# Entire slit is a dark, shallow recess. Only this one uninterrupted thin
# surface is needed, so the fissure feels like a break in the ground.
floor=[]
for x,y,gap,taper in centres:
 floor.extend([(x,y-gap*.89,.006),(x,y+gap*1.11,.006)])
mesh('Dark split floor',floor,
     [(i*2+2,i*2+3,i*2+1,i*2) for i in range(n-1)],void)

def broken_banks(side):
 # Separate polygons with local colour variation rather than a single
 # artificial raised border. Sparse gaps reveal the existing ground shader.
 for palette,variant,inner in ((soil_dark,0,False),
                               (soil_warm,1,False),
                               (soil_cool,2,False),
                               (stone,0,True),
                               (stone_ochre,1,True)):
  verts=[];faces=[]
  for i in range(n-1):
   x,y,g,t=centres[i];xn,yn,gn,tn=centres[i+1]
   if t<.12 or tn<.12:continue
   if inner:
    if (i*7+3)%11 in (0,1,2,7):continue
    if (i%2)!=variant:continue
    lo=g*(1+.04*math.sin(i*2.3))
    hi=lo+(.055+.065*((i*3)%5)/4)*t
    lon=gn*(1+.04*math.sin((i+1)*2.3))
    hin=lon+(.055+.065*(((i+1)*3)%5)/4)*tn
   else:
    if (i*5+side*3)%13 in (0,1,5,7,10):continue
    if (i%3)!=variant:continue
    lo=g+(.065+.043*(i%3))*t
    hi=lo+(.21+.17*((i*7)%5)/4)*t
    lon=gn+(.065+.043*((i+1)%3))*tn
    hin=lon+(.21+.17*(((i+1)*7)%5)/4)*tn
   # Patch widths and skew are purposefully unequal on either bank.
   p=len(verts)
   z=.012 if inner else .008
   verts.extend([(x,y+side*lo,z),(x,y+side*hi,z),
                 (xn,yn+side*hin,z),(xn,yn+side*lon,z)])
   faces.append((p,p+1,p+2,p+3))
  if faces:mesh(('Detached slate fragments ' if inner else
                 'Broken loam shoulders ')+str(side)+' '+str(variant),
                verts,faces,palette)
broken_banks(-1);broken_banks(1)

# Fracture-related debris lies flush with the ground, never built as a wall.
for side in (-1,1):
 verts=[];faces=[]
 for j in (3,7,11,17,22,27,33,38):
  x,y,g,t=centres[j]
  o=side*(g+.16*t)
  k=len(verts);x+=.03*math.sin(j*2.1);y+=o
  s=.065+.05*(j%3)
  verts.extend([(x-s,y-.06,.012),(x+s*.75,y-.045,.019),
                (x+s*.5,y+.08,.034),(x-.045,y+.09,.014)])
  faces.append((k,k+1,k+2,k+3))
 mesh('Earth-embedded stone chips '+str(side),verts,faces,stone)

# Two tiny interrupted remnants hint at ether influence, but the visible
# fissure is primarily DARK SOIL. No bright continuous violet outlines.
for a in (10,29):
 verts=[]
 for j in range(a,a+3):
  x,y,g,t=centres[j]
  shift=.015*math.sin(j*1.2)
  verts.extend([(x,y+shift-.009,.018),(x,y+shift+.009,.018)])
 mesh('Faint isolated ether vein '+str(a),verts,
      [(0,1,3,2),(2,3,5,4)],ether)

export('PrivateFissure')
print('BASTION_III_IRREGULAR_SOIL_FISSURE_EXPORTED')
