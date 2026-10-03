"""Create a visual-only Valoria terrain shell in Blender, in world-sized metres."""
import bpy
import math
import os
import sys
from mathutils import Vector

bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)

def smooth(a, b, v):
    t = max(0.0, min(1.0, (v-a)/(b-a)))
    return t*t*(3-2*t)

def seat(x, z, cx, cz, rx, rz, transition):
    r = math.sqrt(((x-cx)/rx)**2 + ((z-cz)/rz)**2)
    return 1.0-smooth(1.0-transition, 1.0+transition, r)

def height(x, z):
    # The upper and civic shelves are two terraces carved into one continuous bedrock.
    coarse = math.sin(x*.39+math.sin(z*.17))*math.cos(z*.31)*.25
    fine = math.sin(x*1.37+z*.29)*math.cos(z*1.19-x*.13)*.10
    edge = coarse+fine
    base = -3.15+edge*.6
    civic = seat(x,z,0,-2,12.4,10.8,.33)
    upper = seat(x,z,0,8.7,7.8,8.2,.27)
    y = base + (0.3+edge*.15-base)*civic
    y += max(0.0,upper)*(2.45-y)
    # Carved path is continuous, keeps functional routes in the same plot.
    if abs(x)<1.55 and -.4<z<5.6:
        path = .28 + smooth(-.4,5.6,z)*2.15
        y = y*.30+path*.70
    return y

n=161
xs=[-19+38*i/(n-1) for i in range(n)]
zs=[-18+44*j/(n-1) for j in range(n)]
# Two named material regions in one connected mesh.
verts=[(x,-z,height(x,z)) for z in zs for x in xs]
faces=[]
for j in range(n-1):
    for i in range(n-1):
        a=j*n+i; b=a+1; c=a+n; d=c+1
        faces.extend(((a,b,c),(b,d,c)))
mesh=bpy.data.meshes.new("Valoria authored contiguous rock and terraces")
mesh.from_pydata(verts,[],faces)
mesh.update()
obj=bpy.data.objects.new("ValoriaShell_ContinuousBedrock",mesh)
bpy.context.collection.objects.link(obj)

ground=bpy.data.materials.new("ShellGround")
rock=bpy.data.materials.new("ShellRock")
ground.diffuse_color=(.39,.36,.29,1)
rock.diffuse_color=(.53,.51,.46,1)
mesh.materials.append(ground)
mesh.materials.append(rock)
for poly in mesh.polygons:
    poly.use_smooth=True
    poly.material_index=0 if poly.normal.z>.72 else 1

# Deliberate exposed rock strata are attached to the same landform. They break
# the smooth cliff transition in silhouette without making separate floating pads.
for x,z,angle,length,width in [
    (-8,1,-.40,5.2,1.5),(8,2,.38,5.0,1.4),
    (-6,6,-.23,3.8,1.2),(6,6,.26,3.9,1.2),
    (-9,-6,-.36,4.3,1.4),(9,-6,.41,4.3,1.4)]:
    y=height(x,z)-.35
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,location=(x,-z,y))
    o=bpy.context.object
    o.name="ValoriaShell_BuriedRock"
    o.scale=(length,width,.75)
    o.rotation_euler[2]=angle
    o.data.materials.append(rock)

out=sys.argv[sys.argv.index("--")+1] if "--" in sys.argv else "ValoriaShellTransition.glb"
bpy.ops.object.select_all(action="SELECT")
bpy.ops.export_scene.gltf(filepath=os.path.abspath(out),export_format="GLB",export_apply=True,
                          export_materials="EXPORT",export_yup=True)
print("VALORIA_BLENDER_SHELL_BUILT",out,len(verts),len(faces))
