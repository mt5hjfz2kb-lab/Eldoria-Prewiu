"""Reuse verified CC0 photogrammetry from original Eldoria art studio for cliff detail.
This is not a new generator/brain. The supporting cliff continuum remains editable.
"""
import bpy,math,hashlib,random,os
from pathlib import Path
from mathutils import Vector
SOURCE=Path(os.environ.get('ELDORIA_CC0_ROCK_SOURCE','/tmp/eldoria-rock-source'))
def dress_continuous_cliff_with_cc0():
 obj=next(SOURCE.rglob('Rock_LOD4-2k.obj'),None)
 assert obj and obj.is_file(),'Original art-studio CC0 photogrammetry donor unavailable'
 folder=obj.parent
 albedo=folder/'Rock_LOD4_diffuse.tga'
 normal=folder/'Rock_LOD4_normals.tga'
 assert albedo.is_file() and normal.is_file(),'Missing verified PBR maps'
 before=set(bpy.data.objects)
 bpy.ops.wm.obj_import(filepath=str(obj))
 imported=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
 assert len(imported)==1,imported
 rock=imported[0]
 verts=[rock.matrix_world@v.co for v in rock.data.vertices]
 low=Vector(tuple(min(p[i] for p in verts) for i in range(3)))
 high=Vector(tuple(max(p[i] for p in verts) for i in range(3)))
 ext=high-low;assert min(ext)>0
 rock.matrix_world.identity()
 for vert,point in zip(rock.data.vertices,verts):
  vert.co=((point.x-(low.x+high.x)/2)/ext.x,(point.y-(low.y+high.y)/2)/ext.y,(point.z-low.z)/ext.z)
 for f in rock.data.polygons:f.use_smooth=True
 mat=bpy.data.materials.new('Eldoria CC0 scanned limestone weathering PBR')
 mat.use_nodes=True
 nodes=mat.node_tree.nodes;links=mat.node_tree.links
 bs=nodes.get('Principled BSDF');bs.inputs['Roughness'].default_value=.88
 im=bpy.data.images.load(str(albedo));im.scale(1024,1024)
 im.pack();texture=nodes.new('ShaderNodeTexImage');texture.image=im
 tint=nodes.new('ShaderNodeMixRGB');tint.blend_type='MULTIPLY';tint.inputs[0].default_value=1
 tint.inputs[2].default_value=(.77,.79,.77,1)
 links.new(texture.outputs['Color'],tint.inputs[1])
 links.new(tint.outputs['Color'],bs.inputs['Base Color'])
 ni=bpy.data.images.load(str(normal));ni.scale(1024,1024);ni.colorspace_settings.name='Non-Color';ni.pack()
 ntex=nodes.new('ShaderNodeTexImage');ntex.image=ni
 node=nodes.new('ShaderNodeNormalMap');node.inputs['Strength'].default_value=.55
 links.new(ntex.outputs['Color'],node.inputs['Color'])
 links.new(node.outputs['Normal'],bs.inputs['Normal'])
 rock.data.materials.clear();rock.data.materials.append(mat)
 # One oriented cliff-scanned mass per distinctive exposed zone, with physically
 # overlapping faces and no independent spikes or flat slabs.
 rng=random.Random(20261011)
 instances=[]
 for i in range(22):
  a=math.tau*(i+.12*math.sin(i*1.7))/22
  x=4.15*math.cos(a);y=3.52*math.sin(a)
  p=rock.copy();p.data=rock.data;bpy.context.collection.objects.link(p)
  p.name='CC0 integrated cliff geology '+str(i).zfill(2)
  p.location=(x,y,-3.75+.14*math.sin(i*2.1))
  p.rotation_euler=(.08*math.sin(i),.13*math.cos(i),a+.52*math.sin(i*.67))
  p.scale=(1.1+.25*rng.random(),.85+.22*rng.random(),2.0+.4*rng.random())
  instances.append(p)
 bpy.data.objects.remove(rock,do_unlink=True)
 return {'cc0_scan_origin':'https://opengameart.org/content/free-photogrammetric-rock',
 'source_obj_sha256':hashlib.sha256(obj.read_bytes()).hexdigest(),'instances':len(instances),
 'pbr_albedo_and_normal':True,'compatible_with_continuous_cliff':True}
