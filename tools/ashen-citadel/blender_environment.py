"""Independent Blender composition of the Ashen Citadel's five staged settlements.
Uses the artist-owned 3D assets, not any original Eldoria/Valoria scene meshes.
Preserved in each level's Blender .blend file, NOT in exported single-model GLB.
"""
import bpy
import math
import random

def build_environment(level,palette,meshes_dir,import_obj,remap,pbr):
    def add_model(name,placements):
        if not placements:return
        objects=import_obj(meshes_dir/(name+".obj"))
        remap(objects,palette)
        for base in objects:
            for i,(x,y,scale,turn) in enumerate(placements):
                instance=base if i==0 else base.copy()
                if i:
                    instance.data=base.data
                    bpy.context.collection.objects.link(instance)
                instance.location=(x,y,0)
                instance.rotation_euler[2]=turn
                instance.scale=(scale,scale,scale)
                instance.name=f"Art staged {name} · {i}"
    # Blender imports the same actual sculpted terrain that Unity will render.
    terrain=import_obj(meshes_dir/"AshenLandscape.obj")
    remap(terrain,palette)
    for mesh_obj in terrain:
        mesh_obj.name="ASHEN · authored continuous valley + wild verge"
    # Look-dev assumes the building has been completed at that stage.
    # Gameplay visibility and timers remain driven solely by saved PlayerState.
    add_model("Ashen_Sawmill",[(-17.2,-6.2,1.0,0)])
    if level>=2:add_model("Ashen_Barracks",[(16.8,-7.5,1.0,0)])
    if level>=3:add_model("Ashen_Granary",[(-16.6,7.8,1.0,0)])
    if level>=5:add_model("Ashen_Stoneworks",[(17.4,8.8,1.0,0)])

    rng=random.Random(4923)
    trees=[]
    for i in range(58):
        angle=i*2.399963+(.18 if i%3==0 else 0)
        radius=20+8*rng.random()+2*((i//7)%2)
        x=radius*math.cos(angle)
        y=.84*radius*math.sin(angle)
        if -17<x<17 and -17<y<15:continue
        trees.append((x,y,.8+.35*rng.random(),angle))
    add_model("Ashen_Pine",trees)
    stones=[]
    for i in range(30):
        angle=i*2.51
        radius=16+15*rng.random()
        x=math.cos(angle)*radius
        y=math.sin(angle)*radius*.76
        if abs(x)<11 and abs(y)<11:continue
        stones.append((x,y,.38+.52*rng.random(),angle))
    add_model("Ashen_Rock",stones)

    earth=bpy.data.materials.new("Earth · weathered stone path")
    pbr(earth,(.51,.43,.34))
    earth.use_nodes=True
    bsdf=earth.node_tree.nodes.get("Principled BSDF")
    noise=earth.node_tree.nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value=1.7
    ramp=earth.node_tree.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].color=(.34,.28,.23,1)
    ramp.color_ramp.elements[1].color=(.56,.48,.36,1)
    earth.node_tree.links.new(noise.outputs["Fac"],ramp.inputs["Fac"])
    earth.node_tree.links.new(ramp.outputs["Color"],bsdf.inputs["Base Color"])

    wood=bpy.data.materials.new("Cut cedar logs")
    pbr(wood,(.41,.275,.19))
    for i in range(9):
        bpy.ops.mesh.primitive_cylinder_add(vertices=8,radius=.16,depth=1.85,
            location=(-13.65+(i%3)*.46,-5.25+(i//3)*.34,.38))
        log=bpy.context.object
        log.rotation_euler=(math.pi/2,0,(i%2)*.12)
        log.name="Sawmill timber stack "+str(i)
        log.data.materials.append(wood)

def meadow_material(mat):
    """A coherent cool-green ground with painted mottling, not a square plinth."""
    if not mat.use_nodes:return
    nodes=mat.node_tree.nodes
    bsdf=nodes.get("Principled BSDF")
    if bsdf is None:return
    noise=nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value=.32
    noise.inputs["Detail"].default_value=3.0
    noise.inputs["Roughness"].default_value=.61
    ramp=nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].position=.20
    ramp.color_ramp.elements[0].color=(.20,.23,.19,1)
    ramp.color_ramp.elements[1].position=.77
    ramp.color_ramp.elements[1].color=(.37,.42,.31,1)
    links=mat.node_tree.links
    links.new(noise.outputs["Fac"],ramp.inputs["Fac"])
    links.new(ramp.outputs["Color"],bsdf.inputs["Base Color"])
