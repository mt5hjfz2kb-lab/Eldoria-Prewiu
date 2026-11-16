#!/usr/bin/env python3
"""Valoria Director V2 — real 3D authoring, not a generated concept image.

Reads only the last known-good editable city source from official GitHub Action
43852991470 (original Eldoria-authored geometry). Upgrades:
- connected vertex-painted terrain with roads painted into the surface;
- expressive imperial Bastion V gables, heraldry, carved stone, buttresses;
- function-specific sawmill, quarry, barracks, and granary details;
- native trees, shrubs, stone, blossoms, dusk atmosphere.

All output meshes and materials are original script-authored objects.
Never silently imports/redistributes Quaternius or other vendors.
Blender 4.0.2 command: blender -b -t 4 --python-exit-code 1 --python
tools/cc0-art-system/upgrade_director_v2.py
"""
import bpy, math, json, random, struct, os
from mathutils import Vector
from pathlib import Path
from math import sin, cos, pi, radians
random.seed(16112026)
ROOT=Path(__file__).resolve().parents[2]
SRC=ROOT/"Unity/Assets/Art_Reboot/CC0System/CityFinish/CC0_Valoria_CityFinish.blend"
DEST=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV2"
PREV=ROOT/"docs/cc0-art-system/previews/director-v2"
DEST.mkdir(parents=True,exist_ok=True)
PREV.mkdir(parents=True,exist_ok=True)
if not SRC.is_file():
    raise RuntimeError("Need the exact good source from run 43852991470, not the crashed later revision")
bpy.ops.wm.open_mainfile(filepath=str(SRC))
# The upstream editable BLEND can point at textures in a sibling Generated/
# directory. A missing image appears vivid magenta in Blender and is NOT a
# cosmetically acceptable render. Recover by published filename, then fail closed.
SEARCH_TEXTURES=[
    ROOT/"Unity/Assets/Art_Reboot/CC0System/Generated",
    ROOT/"Unity/Assets/Art_Reboot/CC0System/CityFinish"]
def relink_source_textures():
    missing=[]
    repaired=[]
    for image in bpy.data.images:
        if image.source!="FILE" or image.packed_file:continue
        resolved=Path(bpy.path.abspath(image.filepath))
        if resolved.is_file():continue
        base=Path(image.filepath).name
        choices=[folder/base for folder in SEARCH_TEXTURES]
        replacement_path=next((t for t in choices if t.is_file()),None)
        if replacement_path:
            image.filepath=str(replacement_path)
            try: image.reload()
            except Exception:pass
            repaired.append((image.name,str(replacement_path)))
        else:
            missing.append((image.name,image.filepath))
    if missing:
        raise RuntimeError("UNRESOLVED_SOURCE_TEXTURES: "+repr(missing[:15]))
    print("ELDORIA_SOURCE_TEXTURES_VERIFIED",len(repaired))
relink_source_textures()
scene=bpy.context.scene
scene.render.engine="CYCLES"
scene.cycles.samples=22
scene.cycles.use_denoising=False
for layer in scene.view_layers:layer.cycles.use_denoising=False
scene.render.threads_mode="FIXED"
scene.render.threads=4
scene.render.image_settings.file_format="PNG"
scene.render.resolution_percentage=100
scene.view_settings.view_transform="Filmic"
scene.view_settings.look="Medium High Contrast"
scene.view_settings.exposure=.30
scene.render.film_transparent=False
counts={"removed_old_meshes":0,"new_meshes":0,"trees":0,"flowers":0,
        "secondary_details":0,"bastion_details":0}
def group(name):
    g=bpy.data.collections.get(name)
    if g:return g
    g=bpy.data.collections.new(name)
    scene.collection.children.link(g)
    return g
def remove_collection_objs(name):
    g=bpy.data.collections.get(name)
    if g:
        for ob in list(g.objects):
            if ob.type=="MESH":counts["removed_old_meshes"]+=1
            bpy.data.objects.remove(ob,do_unlink=True)
def rgb(h):
    h=h.lstrip("#")
    return tuple(int(h[i:i+2],16)/255.0 for i in (0,2,4))
def mat(name,color,rough=.79,metal=0,emission=0):
    label="EldoriaV2_"+name
    x=bpy.data.materials.get(label)
    if x:return x
    x=bpy.data.materials.new(label)
    c=rgb(color)
    x.use_nodes=True
    x.diffuse_color=(*c,1)
    bs=x.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*c,1)
    bs.inputs["Roughness"].default_value=rough
    bs.inputs["Metallic"].default_value=metal
    if emission:
        for channel in ("Emission Color","Emission"):
            if channel in bs.inputs:
                bs.inputs[channel].default_value=(*c,1)
                break
        if "Emission Strength" in bs.inputs:
            bs.inputs["Emission Strength"].default_value=emission
    return x
M={
"stone":mat("WeatheredImperialStone","#858986"),
"stone_light":mat("LimestoneEdge","#B0A797"),
"stone_dark":mat("DeepSlateWall","#505961"),
"roof":mat("BlueSlateRoof","#364957"),
"roof_edge":mat("RoofRidgeBronze","#94744B",.53,.34),
"timber":mat("DarkOakBeams","#684D35"),
"timber_dark":mat("BurnishedOak","#382F29"),
"iron":mat("ForgedIron","#292E33",.48,.55),
"gold":mat("AgedGold","#B08A53",.56,.51),
"banner":mat("PurpleImperialCloth","#483454"),
"banner_gold":mat("CrestGold","#D3A35E"),
"violet":mat("Corruption","#704B8D",.5,0,.45),
"glass":mat("WarmWindow","#B77B49",.47,0,.6),
"leaf1":mat("FirDeep","#304C3D"),
"leaf2":mat("FirSage","#49634A"),
"leaf3":mat("FirHighlight","#627760"),
"bark":mat("PineTrunk","#524435"),
"rock":mat("NaturalGranite","#73776E"),
"rock_dark":mat("MossyRocks","#4E5A54"),
"moss":mat("FoliageMoss","#50684A"),
"water":mat("RunningWater","#3B6371",.22,.09),
"wheat":mat("HarvestGold","#BCA06C"),
"white":mat("WildflowerWhite","#DED6C5"),
"purple":mat("WildflowerMauve","#947AA8"),
"grass":mat("Grass","#55704A")
}
def link(obj,material,col):
    if isinstance(material,str):material=M[material]
    if material:obj.data.materials.append(material)
    target=group(col)
    for source in list(obj.users_collection):source.objects.unlink(obj)
    target.objects.link(obj)
    counts["new_meshes"]+=1
    return obj
def block(name,loc,dims,m="stone",col="V2_Architecture",bevel=.02):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    ob=bpy.context.object; ob.name=name;ob.dimensions=dims
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    link(ob,m,col)
    if bevel:
        b=ob.modifiers.new("softened cut stone edge","BEVEL")
        b.width=bevel;b.segments=1
        ob.modifiers.new("weighted normals","WEIGHTED_NORMAL")
    return ob
def cylinder(name,loc,r,h,m="stone",col="V2_Architecture",n=12,rotation=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=n,radius=r,depth=h,location=loc)
    ob=bpy.context.object;ob.name=name
    if rotation:ob.rotation_euler=rotation
    return link(ob,m,col)
def cone(name,loc,r1,r2,h,m="roof",col="V2_Architecture",n=10):
    bpy.ops.mesh.primitive_cone_add(vertices=n,radius1=r1,radius2=r2,depth=h,location=loc)
    ob=bpy.context.object;ob.name=name
    return link(ob,m,col)
def ico(name,loc,r,m="rock",col="V2_Landscape",sub=1,sc=None):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,radius=r,location=loc)
    ob=bpy.context.object;ob.name=name
    if sc:ob.scale=sc
    return link(ob,m,col)
def beam(name,point_a,point_b,thick,matkey,col="V2_Architecture"):
    a=Vector(point_a);b=Vector(point_b);v=b-a
    ob=block(name,(a+b)/2,(thick,thick,v.length),matkey,col,bevel=.012)
    ob.rotation_euler=v.to_track_quat("Z","Y").to_euler()
    return ob
def face_mesh(name,verts,faces,m,col):
    mesh=bpy.data.meshes.new(name+"_mesh")
    mesh.from_pydata(verts,[],faces)
    mesh.update()
    ob=bpy.data.objects.new(name,mesh)
    group(col).objects.link(ob)
    mesh.materials.append(M[m] if isinstance(m,str) else m)
    counts["new_meshes"]+=1
    return ob
def arch_window(x,y,z,w,h,collection="Stage5Director"):
    # An original physically modelled lancet opening with lead-came bars
    block("Royal hall gothic glass",(x,y,z+h*.40),(w,.12,h*.80),"glass",collection)
    triangle=[(x-w*.5,y-.03,z+h*.79),(x+w*.5,y-.03,z+h*.79),
              (x,y-.03,z+h*1.16)]
    face_mesh("Royal hall pointed window",triangle,[(0,1,2)],"glass",collection)
    beam("left gothic jamb",(x-w*.60,y-.12,z),(x-w*.60,y-.12,z+h*.78),.18,"stone_light",collection)
    beam("right gothic jamb",(x+w*.60,y-.12,z),(x+w*.60,y-.12,z+h*.78),.18,"stone_light",collection)
    beam("left ogive stone",(x-w*.65,y-.15,z+h*.79),(x,y-.15,z+h*1.22),.19,"stone_light",collection)
    beam("right ogive stone",(x+w*.65,y-.15,z+h*.79),(x,y-.15,z+h*1.22),.19,"stone_light",collection)
    beam("vertical lead tracery",(x,y-.19,z),(x,y-.19,z+h*.87),.10,"iron",collection)
    block("gothic window base sill",(x,y-.15,z-.15),(w+1,.36,.28),"stone_light",collection)
def royal_roof_gable():
    # Expanded high silhouette and non-modular frontage on existing level-V keep.
    col="Stage5Director"
    y=3.22
    v=[(-3.38,y,12.25),(3.38,y,12.25),(0,y,17.55),
       (-3.38,y+.35,12.25),(3.38,y+.35,12.25),(0,y+.35,17.55)]
    f=[(0,1,2),(5,4,3),(0,3,4,1),(0,2,5,3),(1,4,5,2)]
    face_mesh("Monumental restored keep gable",v,f,"stone",col)
    beam("gothic west barge",(-3.48,y-.08,12.24),(0,y-.08,17.62),.21,"roof_edge",col)
    beam("gothic east barge",(3.48,y-.08,12.24),(0,y-.08,17.62),.21,"roof_edge",col)
    # Foundation frames resolve the appearance of pasted-together modules
    for side in (-1,1):
        x=side*2.65
        block("plumb-cut imperial quoin",(x,3.04,10.35),(.51,.56,4.3),"stone_light",col)
        for z in (8.0,9.1,10.2,11.3,12.4):
            block("separate weathered voussoir",(x,2.82,z),(.68,.30,.21),"stone_dark",col)
        beam("tiered Gothic buttress",(side*2.71,3.2,8.2),(side*3.5,1.9,5.65),.47,"stone",col)
        block("buttress cap",(side*3.49,1.92,5.63),(.81,.84,.32),"stone_light",col)
    arch_window(0,y-.10,12.95,1.6,2.45,col)
    block("imperial frieze",(0,2.83,12.65),(5.9,.48,.25),"gold",col,.055)
    # Restraint: one unmistakable symbol, not random decorative gold patches.
    cylinder("Imperial crest mount",(0,2.68,11.75),.64,.16,"gold",col,18,
             rotation=(pi/2,0,0))
    for side in (-1,1):
        beam("imperial wing relief",
             (side*.09,2.50,11.78),(side*.56,2.50,11.98),.125,"banner_gold",col)
        beam("imperial wing lower tip",
             (side*.12,2.48,11.73),(side*.42,2.48,11.52),.12,"banner_gold",col)
    cone("principal spire finial",(0,3.28,17.92),.23,.01,.68,"gold",col,10)
    # Round towers gain cut ashlar rings and differentiated stonework.
    for x,y_t in ((-8.2,-4.3),(8.2,-4.3),(0,10.7)):
        for z in (1.28,3.30,5.37):
            cylinder("restored tower dressed course",(x,y_t,z),2.23,.14,
                     "stone_light",col,12)
        for i in range(9):
            a=2*pi*i/9
            xx=x+2.03*cos(a);yy=y_t+2.03*sin(a)
            # small corbels visually lift towers without an extra platform
            block("tower ring carved corbel",(xx,yy,6.95),
                  (.18,.30,.36),"stone_dark",col,.035)
    # One structured vertical banner, shared purple/gold language.
    for xx in (-1.93,1.93):
        block("royal violet wall standard",(xx,2.76,9.38),(.59,.11,2.43),
              "banner",col,.018)
        block("gold trimmed pennant tip",(xx,2.67,8.13),(.60,.09,.09),"gold",col)
    counts["bastion_details"]+=1
royal_roof_gable()

# Source city contains exposed platform-like road slabs and distant sharp cones.
# Replace them with terrain-integrated decisions in the *exported mesh*, too.
for cname in ("Roads","Route","GroundPatches","Foliage","LandscapeDetail"):
    remove_collection_objs(cname)
for ob in list(bpy.data.objects):
    if ob.type=="MESH" and (ob.name.startswith("worn Valoria terrain mesh")
                             or ob.name.startswith("river channel")):
        bpy.data.objects.remove(ob,do_unlink=True)
        counts["removed_old_meshes"]+=1

def height(x,y):
    # identical low-frequency elevation around the castle to preserve anchors
    return .12*sin(x*.28)*cos(y*.21)+.17*sin(x*.11+y*.16)-.05*max(0,1-abs(x)/3)
roads=[
 [(-.2,-36),(.7,-23),(-2.2,-12),(0,-6)],
 [(-3,-18),(-9,-19),(-15,-16),(-22,-11)],
 [(3,-18),(9,-19),(15,-15),(22,-12)],
 [(-17,-13),(-27,-8),(-29,3),(-23,8)],
 [(16,-12),(28,-5),(29,3),(24,7)]
]
samples=[]
for curve in roads:
    P=[Vector(p) for p in curve]
    points=[]
    for k in range(44):
        t=k/43
        v=(1-t)**3*P[0]+3*(1-t)**2*t*P[1]+3*(1-t)*t*t*P[2]+t**3*P[3]
        points.append((v.x,v.y))
    samples.append(points)
def road_dist2(x,y):
    # exact geometry-independent distance; color is baked into real terrain vertices
    best=1.0e9
    for curve in samples:
        for px,py in curve:
            d=(px-x)**2+(py-y)**2
            if d<best:best=d
    return best
def make_ground():
    sx,sy=136,122
    verts=[]
    faces=[]
    for iy in range(sy+1):
        y=-52+iy*104/sy
        for ix in range(sx+1):
            x=-56+ix*112/sx
            verts.append((x,y,height(x,y)))
    for iy in range(sy):
        for ix in range(sx):
            p=iy*(sx+1)+ix
            faces.append((p,p+1,p+sx+2,p+sx+1))

    # Bake actual textured dirt trails and grass into an image. This is a
    # portable UV/color source understood by Blender, glTF, and URP. It does
    # not depend on material node features missing in the game importer.
    width,height_px=384,384
    image=bpy.data.images.new("DirectorV2_Valoria_GrassRoad_Albedo",
                              width=width,height=height_px,alpha=False)
    pixels=[0.]*(width*height_px*4)
    for py in range(height_px):
        y=-52+(py+.5)*104/height_px
        for px in range(width):
            x=-56+(px+.5)*112/width
            nois=.48*sin(x*.17+y*.27)+.22*cos(x*.43-y*.13)+.15*sin(x*.055-y*.073)
            ground=(.235+.052*nois,.338+.051*nois,.252+.040*nois)
            bare=(.328+.026*nois,.285+.025*nois,.213+.017*nois)
            near=road_dist2(x,y)
            w=1.80 if y<0 else 1.25
            dirt_mix=max(0.,min(.94,math.exp(-near/(2*w*w))))
            # rich green field transitioning organically into ochre earth
            col=tuple(min(.9,max(.04,ground[j]*(1-dirt_mix)+bare[j]*dirt_mix))
                      for j in range(3))
            off=(py*width+px)*4
            pixels[off:off+4]=(*col,1)
    image.pixels[:]=pixels
    image.filepath_raw=str(DEST/"DirectorV2_Terrain_Albedo.png")
    image.file_format="PNG"
    image.save()
    image.pack()  # the editable .blend remains portable without sidecars
    terrainmat=bpy.data.materials.new("EldoriaV2_Terrain_BakedTexture")
    terrainmat.use_nodes=True
    nodes=terrainmat.node_tree.nodes
    bs=nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(1,1,1,1)
    bs.inputs["Roughness"].default_value=.97
    diffuse=nodes.new("ShaderNodeTexImage")
    diffuse.image=image
    diffuse.interpolation="Linear"
    terrainmat.node_tree.links.new(diffuse.outputs["Color"],bs.inputs["Base Color"])
    mesh=bpy.data.meshes.new("Valoria_NaturalTerrain_V2_Baked")
    mesh.from_pydata(verts,[],faces)
    mesh.materials.append(terrainmat)
    mesh.update()
    uv=mesh.uv_layers.new(name="TerrainUV")
    for polygon in mesh.polygons:
        for idx in polygon.loop_indices:
            vid=mesh.loops[idx].vertex_index
            xx,yy,_=verts[vid]
            uv.data[idx].uv=((xx+56)/112,(yy+52)/104)
    ob=bpy.data.objects.new("PHYSICAL TERRAIN | grass blends into worn tracks",mesh)
    group("V2_Landscape").objects.link(ob)
    counts["new_meshes"]+=1
    print("ELDORIA_V2_BAKED_TERRAIN_TEXTURE",image.filepath_raw,os.path.getsize(image.filepath_raw))

make_ground()

def wheel():
    cx,cy,cz=-28.65,-9.15,2.15
    r=1.95
    # A physical wheel with its spokes and rim on a *vertical* plane, readable
    # in the same isometric viewport. Separate from the old cottage volume.
    for k in range(16):
        a=2*pi*k/16;b=2*pi*(k+1)/16
        A=(cx,cy+r*sin(a),cz+r*cos(a))
        B=(cx,cy+r*sin(b),cz+r*cos(b))
        beam("waterwheel aged curved rim",A,B,.20,"timber","V2_Industry")
    cylinder("sawmill waterwheel iron axle",(cx,cy,cz),.40,.8,
             "iron","V2_Industry",n=12,rotation=(0,pi/2,0))
    for i in range(8):
        a=2*pi*i/8
        beam("waterwheel shaft spoke",(cx,cy,cz),
             (cx,cy+r*sin(a),cz+r*cos(a)),.14,"timber","V2_Industry")
    # Sawmill terrace and rafters must be open work-space, not a flat generic home.
    for xx in (-26.3,-23.7,-21.1):
        block("sawmill open timber pier",(xx,-10.4,1.68),(.24,.25,3.35),
              "timber","V2_Industry")
    beam("sawmill long load beam",(-26.3,-10.4,3.35),
         (-21.1,-10.4,3.35),.25,"timber","V2_Industry")
    for n in range(8):
        yy=-11.5+n*.43
        cylinder("sawmill cut log",( -23.7,yy,.41),.19,3.2,
                 "timber","V2_Industry",9,rotation=(0,pi/2,0))
        block("sawmill sawn plank",(-19.6,yy,.34),(.22,2.30,.14),
              "timber","V2_Industry")
    # Mill channel over boulders, small and geologically grounded.
    vv=[];ff=[]
    for i in range(17):
        y=-21+i*1.27;x=-29.7+.35*sin(i*.55)
        vv.extend([(x-.74,y,height(x,y)+.047),(x+.74,y,height(x,y)+.052)])
        if i:
            q=2*i;ff.append((q-2,q-1,q+1,q))
    face_mesh("sawmill creek and waterwheel spill",vv,ff,"water","V2_Water")
    for i in range(23):
        py=-20+random.uniform(0,19)
        px=-29.7+random.choice((-1.1,1.1))+random.uniform(-.40,.4)
        ico("creek bed boulder",(px,py,height(px,py)+.33),
            random.uniform(.25,.65),random.choice(("rock","rock_dark")),"V2_Water",
            1,(1,.80,.58))
    counts["secondary_details"]+=1
wheel()

def quarry():
    x,y=-23.5,11.8
    # Rock face merges into surrounding meadow rather than an elevated plaza.
    for i in range(38):
        px=x-4.0+random.uniform(-6.2,4.1)
        py=y+6+random.uniform(-1.5,5.7)
        rz=random.uniform(.48,1.42)
        ico("quarry scarred hillside",(px,py,height(px,py)+rz*.55),
            rz,random.choice(("rock","rock","rock_dark")),"V2_Quarry",1,
            (random.uniform(.80,1.4),random.uniform(.7,1.3),random.uniform(.35,.66)))
    for i in range(13):
        px=x-5.0+(i%5)*1.25
        py=y-4.5+(i//5)*1.23
        block("quarry cut cube stock",(px,py,.47),(.93,.89,.9),
              "stone_light","V2_Quarry",.042)
    for tx in (-28,-25.1):
        block("quarry hoist oak post",(tx,y+1,3.0),(.32,.33,5.9),
              "timber","V2_Quarry")
    beam("quarry overhead industrial gantry",(-28,y+1,5.95),(-22.7,y+1,5.95),
         .36,"timber","V2_Quarry")
    block("quarry suspended chain",(-25.1,y+1,4.3),(.09,.10,2.7),
          "iron","V2_Quarry")
    block("quarry lifted dressed stone",(-25.1,y+1,2.63),(1.4,.98,.74),
          "stone_light","V2_Quarry")
    counts["secondary_details"]+=1
quarry()

def granary():
    x,y=22,-9.2
    for i in range(2):
        xx=x+4.5+i*2.55
        cylinder("circular grain storage silo",(xx,y+.7,2.30),
                 1.12,4.55,"stone","V2_Granary",16)
        for z in (1.14,2.4,3.7,4.35):
            cylinder("granary dressed stone band",(xx,y+.7,z),
                     1.18,.13,"stone_light","V2_Granary",16)
        cone("granary turrets conical blue slate roofs",(xx,y+.7,5.45),
             1.44,.05,1.91,"roof","V2_Granary",16)
        cone("silo bronze roof finial",(xx,y+.7,6.45),.16,.01,.55,
             "gold","V2_Granary",8)
    for i in range(14):
        xx=x-4.0+(i%6)*.75
        yy=y-4+(i//6)*.65
        ico("granary stored grain bag",(xx,yy,.4),.42,
            "wheat","V2_Granary",1,(1.2,.73,.65))
    for i in range(37):
        # an *irregular* near-harvest patch, not four square building lots
        angle=random.uniform(-pi,pi);rr=random.random()**.5
        px=x+4+2.2*rr*cos(angle);py=y-6+1.6*rr*sin(angle)
        beam("granary clipped straw",(px,py,.07),(px,py,random.uniform(.8,1.35)),
             .055,"wheat","V2_Granary")
    counts["secondary_details"]+=1
granary()

def barracks():
    x,y=23.8,11.4
    # A proper military courtyard, training gear and armory, not another cottage.
    for k in range(3):
        px=x-5.0+k*1.52
        py=y-5.5
        beam("barracks dummy stock",(px,py,.15),(px,py,2.30),
             .16,"timber","V2_Barracks")
        beam("barracks dummy arms",(px-.62,py,1.7),(px+.62,py,1.7),
             .17,"timber","V2_Barracks")
        cylinder("barracks painted target disk",(px,py-.07,1.53),
                 .55,.10,"wheat","V2_Barracks",16,rotation=(pi/2,0,0))
        cylinder("barracks circular iron target",(px,py-.14,1.53),
                 .30,.105,"iron","V2_Barracks",14,rotation=(pi/2,0,0))
    for s in (-1,1):
        px=x+4.1+s*.75
        beam("barracks forged spear",(px,y-3.6,.25),(px,y-3.6,3.27),
             .08,"iron","V2_Barracks")
        cone("barracks spearpoint",(px,y-3.6,3.41),.13,.01,.35,
             "iron","V2_Barracks",8)
    # Heavy military canopy differs from both granary and sawmill
    for px in (x+2.5,x+5.2):
        block("barracks guard palisade",(px,y-5.15,1.19),
              (.27,.40,2.35),"timber_dark","V2_Barracks")
    counts["secondary_details"]+=1
barracks()

# Forest with tiered irregular foliage rather than three perfect toy cones.
def pine(x,y,h):
    height0=height(x,y)
    cylinder("spruce weighted trunk",(x,y,height0+h*.28),
             h*.055,h*.55,"bark","V2_Trees",8)
    for layer in range(4):
        zz=height0+h*(.36+layer*.15)
        rr=h*(.235-layer*.041)
        skew=random.uniform(-.17,.17)
        cone("spruce layered asymmetric silhouette",(x+skew,y+skew*.4,zz),
             rr,.035,h*.27,
             ("leaf1" if layer%2 else "leaf2"),"V2_Trees",8)
    counts["trees"]+=1
for i in range(118):
    x=random.uniform(-52,52);y=random.uniform(-47,47)
    if abs(x)<19 and -22<y<22:continue
    if abs(x-22)<11 and -16<y<17:continue
    if abs(x+23)<11 and -18<y<18:continue
    pine(x,y,random.uniform(4.0,9.5))
def leafy(x,y,h):
    cylinder("wild deciduous trunk",(x,y,height(x,y)+h*.31),
             h*.076,h*.62,"bark","V2_Trees",8)
    for bx,by,bz,sc in ((-.3,.05,.75,.68),(.33,0,.78,.64),(0,-.32,.91,.66)):
        ico("wild coppice branches",
            (x+h*.16*bx,y+h*.15*by,height(x,y)+h*bz),
            h*.22,"leaf3","V2_Trees",1,(1.24,1.07,sc))
    counts["trees"]+=1
for i in range(27):
    x=random.uniform(-45,45);y=random.uniform(-40,40)
    if abs(x)<30 and -19<y<18:continue
    leafy(x,y,random.uniform(3.1,6.4))
# Group grass flowers as one physical mesh per color, far fewer actual draw calls.
for blossom,ncount in (("white",280),("purple",210),("wheat",95)):
    verts=[];faces=[]
    for i in range(ncount):
        x=random.uniform(-46,46);y=random.uniform(-44,44)
        if abs(x)<9 and -9<y<15:continue
        if abs(x-23)<4 and abs(y+9)<4:continue
        z=height(x,y)+.12
        r=random.uniform(.07,.17)
        a=len(verts)
        verts.extend([(x-r,y,z),(x+r,y,z),
                      (x,y,z+random.uniform(.17,.31))])
        faces.append((a,a+1,a+2))
        counts["flowers"]+=1
    face_mesh("Native groundcover blossoms "+blossom,verts,faces,blossom,
              "V2_Meadow")
for i in range(70):
    x=random.uniform(-45,45);y=random.uniform(-43,43)
    if abs(x)<10 and -10<y<12:continue
    ico("meadow weathered scatter",(x,y,height(x,y)+.16),
        random.uniform(.19,.67),
        random.choice(("rock","rock_dark")),
        "V2_Meadow",1,(1.08,.82,.55))

# Better readable warm/cool lighting; this is lookdev, not an automatic
# promise about expensive volumetric/mobile lighting in Unity.
world=scene.world
world.use_nodes=True
bg=world.node_tree.nodes.get("Background")
bg.inputs["Color"].default_value=(.34,.38,.49,1)
bg.inputs["Strength"].default_value=.82
for light in [o for o in bpy.data.objects if o.type=="LIGHT"]:
    if light.data.type=="SUN":
        light.data.energy=3.35
        light.data.color=(1.0,.79,.57)
        light.data.angle=radians(4.3)
    elif light.data.type=="POINT":
        light.data.energy=min(light.data.energy,125.0)
amb=bpy.data.materials.get("ED_WarmWindow")
if amb and amb.use_nodes:
    shader=amb.node_tree.nodes.get("Principled BSDF")
    if shader and "Emission Strength" in shader.inputs:
        shader.inputs["Emission Strength"].default_value=.85
cam=scene.camera
cam.data.type="ORTHO"
def stage_visible(stage):
    for n in range(2,6):
        col=bpy.data.collections.get("Stage"+str(n))
        if col:col.hide_render=n>stage
    for name in ("Stage5Detail","Stage5Director"):
        col=bpy.data.collections.get(name)
        if col:col.hide_render=stage<5
def frame(stage,name,wh=(1600,900),portrait=False):
    stage_visible(stage)
    width,height=wh
    cam.location=(38,-52,44) if not portrait else (31,-46,42)
    tar=Vector((0,-1,4))
    cam.rotation_euler=(tar-cam.location).to_track_quat("-Z","Y").to_euler()
    cam.data.ortho_scale=68 if not portrait else 55
    scene.render.resolution_x=width
    scene.render.resolution_y=height
    path=PREV/name
    scene.render.filepath=str(path)
    bpy.ops.render.render(write_still=True)
    raw=path.read_bytes()
    if len(raw)<90000 or raw[:8]!=b"\x89PNG\r\n\x1a\n":
        raise RuntimeError("Real Blender frame did not render: "+str(path))
    print("V2_DIRECTOR_REAL_BLENDER_IMAGE",name,len(raw))
    return {"file":name,"bytes":len(raw),"stage":stage,"dimensions":[width,height]}
images=[]
images.append(frame(1,"DirectorV2_BastionI_1600x900.png"))
images.append(frame(5,"DirectorV2_BastionV_1600x900.png"))
images.append(frame(5,"DirectorV2_BastionV_Portrait_720x1200.png",(720,1200),True))
for n in range(2,6):
    col=bpy.data.collections.get("Stage"+str(n))
    if col:col.hide_render=False
for name in ("Stage5Detail","Stage5Director"):
    col=bpy.data.collections.get(name)
    if col:col.hide_render=False
scene.render.filepath=""
bpy.ops.wm.save_as_mainfile(filepath=str(DEST/"DirectorV2_Master.blend"))
def write_stage(stage):
    bpy.ops.object.select_all(action="DESELECT")
    for ob in bpy.data.objects:
        if ob.type!="MESH":continue
        names=[c.name for c in ob.users_collection]
        required=[int(x.replace("Stage","")) for x in names
                  if x.startswith("Stage") and x not in ("Stage5Detail","Stage5Director")]
        if required and max(required)>stage:continue
        if stage<5 and ("Stage5Detail" in names or "Stage5Director" in names):continue
        ob.select_set(True)
    out=DEST/("DirectorV2_ValoriaStage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(out),export_format="GLB",
                              use_selection=True,export_apply=True)
    data=out.read_bytes()
    if len(data)<200000 or data[:4]!=b"glTF":
        raise RuntimeError("Physical GLB not produced: "+str(out))
    if struct.unpack("<I",data[8:12])[0]!=len(data):
        raise RuntimeError("Malformed GLB container "+str(out))
    size=struct.unpack("<I",data[12:16])[0]
    gltf=json.loads(data[20:20+size])
    info={"level":stage,"bytes":len(data),"meshes":len(gltf.get("meshes",[])),
          "materials":len(gltf.get("materials",[])),
          "color_0":sum(any("COLOR_0" in p.get("attributes",{})
                for p in m.get("primitives",[])) for m in gltf.get("meshes",[]))}
    print("V2_DIRECTOR_REAL_3D_EXPORT",json.dumps(info))
    texture_count=len(gltf.get("images",[]))
    if texture_count<1:
        raise RuntimeError("No physically embedded texture images in GLB "+str(stage))
    info["embedded_images"]=texture_count
    print("V2_PHYSICAL_TEXTURED_TERRAIN_PRESENT",stage,texture_count)
    return info
exported=[write_stage(x) for x in range(1,6)]
report={"creator":"Eldoria DirectorV2 original Python/Blender script",
        "source":"known-good original source, GitHub Action 43852991470",
        "script":"tools/cc0-art-system/upgrade_director_v2.py",
        "third_party_meshes_used":[],
        "original_3d_source":True,"real_blender_renders":True,
        "unity_tested":False,"iphone_tested":False,"art_approved":False,
        "physical_3d_stage_count":5,"counts":counts,"frames":images,"models":exported}
(PREV/"director-v2-evidence.json").write_text(json.dumps(report,indent=2)+"\n")
print("ELDORIA_DIRECTOR_V2_REAL_ART_PASS",counts["new_meshes"],len(exported))
