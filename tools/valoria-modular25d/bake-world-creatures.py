"""Blender background renderer for the existing regional jabalí and engendro."""
from pathlib import Path
import math
from PIL import Image
import numpy as np
import bpy
from mathutils import Vector

assets=Path(__file__).resolve().parents[2]/'Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Creatures'
for family, angle in [('RockBoar',48),('Spawnling',64)]:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(assets/(family+'Near.glb')))
    scene=bpy.context.scene
    points=[obj.matrix_world @ Vector(v) for obj in scene.objects if obj.type=='MESH' for v in obj.bound_box]
    mid=(Vector(tuple(min(v[i] for v in points) for i in range(3)))+
         Vector(tuple(max(v[i] for v in points) for i in range(3))))/2
    radius=max((v-mid).length for v in points)
    bpy.ops.object.camera_add()
    cam=bpy.context.object
    scene.camera=cam
    cam.data.type='ORTHO'
    cam.data.clip_end=max(1000,radius*20)
    def light(kind,direction,energy,color):
        bpy.ops.object.light_add(type=kind,location=mid+radius*Vector(direction))
        obj=bpy.context.object
        obj.rotation_euler=(mid-obj.location).to_track_quat('-Z','Y').to_euler()
        obj.data.energy=energy
        obj.data.color=color
        obj.data.angle=math.radians(24)
    light('SUN',(-2,-3,4),2.8,(1,.86,.67))
    light('SUN',(2,1,3),1.45,(.72,.80,1))
    scene.world=bpy.data.worlds.new('Overcast bounce')
    scene.world.use_nodes=True
    scene.world.node_tree.nodes.get('Background').inputs[0].default_value=(.51,.54,.59,1)
    scene.world.node_tree.nodes.get('Background').inputs[1].default_value=.55
    scene.render.engine='CYCLES'
    scene.cycles.samples=36
    scene.cycles.use_denoising=False
    scene.render.film_transparent=True
    bpy.ops.mesh.primitive_plane_add(size=radius*5,location=(mid.x,mid.y,min(v.z for v in points)-radius*.002))
    bpy.context.object.is_shadow_catcher=True
    scene.view_settings.view_transform='AgX'
    scene.render.resolution_x=scene.render.resolution_y=320
    scene.render.resolution_percentage=100
    scene.render.image_settings.color_mode='RGBA'
    for i,heading in enumerate((angle,angle+84)):
        a=math.radians(heading)
        direction=Vector((math.sin(a),-math.cos(a),.83)).normalized()
        cam.location=mid+direction*radius*4
        cam.rotation_euler=(-direction).to_track_quat('-Z','Y').to_euler()
        bpy.context.view_layer.update()
        coords=[cam.matrix_world.inverted()@v for v in points]
        w=max(v.x for v in coords)-min(v.x for v in coords)
        h=max(v.y for v in coords)-min(v.y for v in coords)
        cam.data.ortho_scale=max(w,h)*1.03
        scene.render.filepath=str(assets/(family+'Paint'+str(i)+'.png'))
        bpy.ops.render.render(write_still=True)
        with Image.open(scene.render.filepath) as image:
            rgba=image.convert("RGBA")
        # Soften the shadow-catcher termination before the frame border. The
        # opaque creature pixels retain the native render albedo and alpha.
        pixels=np.array(rgba)
        yy,xx=np.mgrid[:rgba.height,:rgba.width]
        border=np.minimum(np.minimum(xx,rgba.width-1-xx),np.minimum(yy,rgba.height-1-yy))
        shadow=(pixels[:,:,:3].max(axis=2)<18)&(pixels[:,:,3]<240)
        pixels[:,:,3]=np.where(shadow,pixels[:,:,3]*.68*np.clip(border/26,0,1),pixels[:,:,3])
        rgba=Image.fromarray(pixels)
        box=rgba.getchannel("A").point(lambda a:255 if a>35 else 0).getbbox()
        rgba.crop((max(0,box[0]-5),max(0,box[1]-5),min(320,box[2]+5),min(320,box[3]+5))).save(scene.render.filepath,optimize=True)
