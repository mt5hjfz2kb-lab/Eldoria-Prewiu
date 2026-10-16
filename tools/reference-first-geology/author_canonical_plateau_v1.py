"""Eldoria reference-first: ONE contiguous editable 3D cliff + two-tier grassy top.

An individually reviewed geological donor; NOT an approved game scene, not an 8/10 claim.
Unlike old tiny/toy source, this is a full-scale topological plateau with one connected
mesh, irregular actual cliff strata, the rear rise, and road/stair compatible gradients.
Uses existing project albedo/normal textures; no paid APIs, imports, credits or services.
"""
import argparse, json, math, os, random, sys, hashlib, traceback
from pathlib import Path
import bpy
from mathutils import Vector, noise

def args_get():
    argv=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
    p=argparse.ArgumentParser()
    p.add_argument('--output', required=True)
    p.add_argument('--rock-albedo', required=True)
    p.add_argument('--rock-normal', required=True)
    return p.parse_args(argv)

A=args_get()
OUT=Path(A.output).resolve()
OUT.mkdir(parents=True,exist_ok=True)
REPORT={'asset':'Valoria_CanonicalPlateau_Geology_v1',
 'reference_sha256':'de476e0516f744f9cd899419404971fa5e8d42bd49f2f7db7670d1cb325f8a4f',
 'architecture_included':False,'art_approved':False,'license_verified':False,
 'cost_eur':0,'source_method':'single authored contiguous mesh with textured top and fractured skirt'}

def smooth(x):
    x=max(0.,min(1.,x))
    return x*x*(3.-2.*x)

def fbm(x,z):
    return (noise.noise(Vector((x*.027,z*.024,1.2)))*.60+
            noise.noise(Vector((x*.091,z*.071,5.7)))*.29+
            noise.noise(Vector((x*.31,z*.28,2.6)))*.11)

def width(z):
    t=(z+92.)/184.
    return 62.+11.*math.sin(math.pi*t)-6.*t+1.0*math.sin(8.*math.pi*t)

def elevation(x,z,u):
    rise=5.7*smooth((z-22.)/23.)
    irregular=.32*fbm(x,z)+.24*math.sin(z*.058+x*.024)
    # Main courtyard and the central gate-stair axis must stay genuinely traversable.
    nearroad=1.-smooth((abs(x)-8.)/7.)
    margin=smooth((abs(u)-.74)/.26)
    cliff_crest=(.44*noise.noise(Vector((x*.13,z*.17,2.)))+.28*math.sin(x*.27-z*.07))*margin
    return 9.3+rise+irregular*(1.-.72*nearroad)+cliff_crest

def create_textured_material(name,texture_file,normal_file=None,tint=(1,1,1,1)):
    mat=bpy.data.materials.new(name);mat.use_nodes=True
    nodes=mat.node_tree.nodes; links=mat.node_tree.links
    bsdf=next(n for n in nodes if n.type=='BSDF_PRINCIPLED')
    bsdf.inputs['Base Color'].default_value=tint
    bsdf.inputs['Roughness'].default_value=.86
    bsdf.inputs['Metallic'].default_value=0.
    tex=nodes.new('ShaderNodeTexImage')
    tex.name=name+'_SOURCE_ALBEDO'
    tex.image=bpy.data.images.load(str(texture_file),check_existing=True)
    links.new(tex.outputs['Color'],bsdf.inputs['Base Color'])
    if normal_file is not None:
        normal=nodes.new('ShaderNodeTexImage')
        normal.image=bpy.data.images.load(str(normal_file),check_existing=True)
        normal.image.colorspace_settings.name='Non-Color'
        nm=nodes.new('ShaderNodeNormalMap')
        nm.inputs['Strength'].default_value=.35
        links.new(normal.outputs['Color'],nm.inputs['Color'])
        links.new(nm.outputs['Normal'],bsdf.inputs['Normal'])
    return mat

def grass_image(path):
    """Prebaked diffuse study; no shader-only procedural texture lost during GLB export."""
    n=384
    img=bpy.data.images.new('Valoria_GrassSoil_Study_384',width=n,height=n,alpha=True)
    pixels=[0.0]*(n*n*4)
    for iy in range(n):
        for ix in range(n):
            u=ix/(n-1);v=iy/(n-1)
            broad=noise.noise(Vector((u*6.4+1.4,v*6.1+3.1,4.2)))
            medium=noise.noise(Vector((u*25.3,v*23.6,8.8)))
            grain=noise.noise(Vector((u*109.7,v*103.1,12.3)))
            dryness=smooth((broad*.62+medium*.38+.45)*.83)
            # warm olive grass and brown dry soil without orange checker patches
            grass=(.34,.345,.195)
            earth=(.49,.397,.269)
            noisev=medium*.032+grain*.024
            k=(iy*n+ix)*4
            for j in range(3):
                pixels[k+j]=max(.03,min(.86,grass[j]*(1-dryness)+earth[j]*dryness+noisev))
            pixels[k+3]=1.
    img.pixels.foreach_set(pixels);img.filepath_raw=str(path)
    img.file_format='PNG';img.save()
    return path

def make_geometry():
    nx,nz=80,120
    xmin,xmax=-92.,92.
    vertices=[];faces=[];face_mat=[]
    # A single top quad grid. All cliff-face rings share these boundary vertex indices.
    for iz in range(nz):
        z=-92.+184.*iz/(nz-1)
        hw=width(z)
        for ix in range(nx):
            u=2.*ix/(nx-1)-1.
            x=u*hw
            vertices.append((x,z,elevation(x,z,u)))
    def at(i,j):return j*nx+i
    for j in range(nz-1):
        for i in range(nx-1):
            p=at(i,j);q=at(i+1,j);r=at(i,j+1);d=at(i+1,j+1)
            faces.extend(((p,q,d),(p,d,r)));face_mat.extend((0,0))
    # Blender coordinates use z vertical; here vertex tuples (x, horizY, verticalZ).
    # Construct counterclockwise boundary in x/horizontalY plane.
    boundary=[at(i,0) for i in range(nx)]
    boundary.extend(at(nx-1,j) for j in range(1,nz))
    boundary.extend(at(i,nz-1) for i in range(nx-2,-1,-1))
    boundary.extend(at(0,j) for j in range(nz-2,0,-1))
    boundarylen=len(boundary)
    rings=[boundary]
    rng=random.Random(20261016)
    # Actual stratified cliff sections, solid connected to original mesh.
    for layer in range(1,6):
        frac=layer/5.
        ring=[]
        for index,sourceindex in enumerate(boundary):
            x,y,top=vertices[sourceindex]
            prior=vertices[boundary[(index-1)%boundarylen]]
            ahead=vertices[boundary[(index+1)%boundarylen]]
            tx=ahead[0]-prior[0];ty=ahead[1]-prior[1]
            norm=math.sqrt(tx*tx+ty*ty)+1e-8
            # polygon CCW: outward to the right of tangent
            ox,oy=ty/norm,-tx/norm
            bedding=.77*noise.noise(Vector((x*.16,y*.11,frac*4.1)))+.55*math.sin(index*.33+frac*5.7)
            outward=(1.25+frac*2.25)+bedding*(.4+frac*.9)
            down_y=top+16.2
            height=top-down_y*frac + .55*(1-frac)*math.sin(index*.19+layer*.31)
            # Overhang shoulders + fractured middle, not a single flat cylinder shell.
            xx=x+ox*outward; yy=y+oy*outward
            ring.append(len(vertices));vertices.append((xx,yy,height))
        rings.append(ring)
    for layer in range(5):
        upper,lower=rings[layer],rings[layer+1]
        for i in range(boundarylen):
            nxt=(i+1)%boundarylen
            a,b,c,d=upper[i],upper[nxt],lower[i],lower[nxt]
            faces.extend(((a,b,d),(a,d,c)))
            crack=(noise.noise(Vector((i*.19,layer*.8,3.1)))>.53)
            face_mat.extend((2 if crack else 1,2 if crack else 1))
    # End cap: fill bottom with fan to produce a watertight non-floating geological solid.
    cx=sum(vertices[k][0] for k in rings[-1])/boundarylen
    cy=sum(vertices[k][1] for k in rings[-1])/boundarylen
    center=len(vertices);vertices.append((cx,cy,-16.2))
    for i in range(boundarylen):
        faces.append((center,rings[-1][i],rings[-1][(i+1)%boundarylen]));face_mat.append(2)
    mesh=bpy.data.meshes.new('CanonicalPlateau_SINGLE_connected_terrain_mesh')
    mesh.from_pydata(vertices,[],faces)
    mesh.update(calc_edges=True)
    obj=bpy.data.objects.new('Valoria_CanonicalPlateau_ONE_GEOMETRIC_SOLID',mesh)
    bpy.context.collection.objects.link(obj)
    rock=Path(A.rock_albedo).resolve()
    normal=Path(A.rock_normal).resolve()
    if not rock.is_file() or not normal.is_file():
        raise FileNotFoundError('Original existing rock texture or normal missing')
    grass=grass_image(OUT/'CanonicalPlateau_grass_olive_albedo.png')
    soil_mat=create_textured_material('AUTHORED_OLIVE_GRASS_DIRT_PBR',grass)
    rock_mat=create_textured_material('REAL_STONE_GRAIN_ROCK_ALBEDO',rock,normal)
    darker=create_textured_material('STONE_GRAIN_SHADOW_CRACKS',rock,normal)
    darker.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.97
    for m in (soil_mat,rock_mat,darker):mesh.materials.append(m)
    for polygon,mi in zip(mesh.polygons,face_mat):
        polygon.material_index=mi
        polygon.use_smooth=(mi==0)
    uv=mesh.uv_layers.new(name='CanonicalUV')
    for poly in mesh.polygons:
        for loop in poly.loop_indices:
            co=mesh.vertices[mesh.loops[loop].vertex_index].co
            if poly.material_index==0:
                uv.data[loop].uv=(co.x*.042,co.y*.042)
            else:
                uv.data[loop].uv=(co.x*.06,co.z*.082)
    obj.select_set(True)
    bpy.context.view_layer.objects.active=obj
    return obj

def studio():
    world=bpy.data.worlds.new('Neutral dusk geometric review')
    world.use_nodes=True
    world.node_tree.nodes['Background'].inputs['Color'].default_value=(.22,.32,.46,1)
    world.node_tree.nodes['Background'].inputs['Strength'].default_value=.55
    bpy.context.scene.world=world
    def area(name,loc,power,color,size):
        light=bpy.data.lights.new(name,'AREA')
        light.energy=power;light.color=color;light.shape='DISK';light.size=size
        ob=bpy.data.objects.new(name,light);bpy.context.collection.objects.link(ob)
        ob.location=loc
        ob.rotation_euler=(Vector((0,0,0))-ob.location).to_track_quat('-Z','Y').to_euler()
    area('Warm long raking light',(-120,-100,180),6200,(1,.85,.70),105)
    area('Blue bounce rim',(120,45,120),4200,(.77,.86,1),95)
    scene=bpy.context.scene
    scene.render.engine='BLENDER_EEVEE'
    scene.render.resolution_x=1536
    scene.render.resolution_y=1024
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG'
    scene.view_settings.view_transform='AgX'
    camd=bpy.data.cameras.new('Locked oblique reference geological camera')
    cam=bpy.data.objects.new('Locked oblique reference geological camera',camd)
    bpy.context.collection.objects.link(cam)
    scene.camera=cam
    camd.type='ORTHO';camd.lens=42
    def capture(name,p,target,scale):
        cam.location=p
        cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler()
        camd.ortho_scale=scale
        scene.render.filepath=str(OUT/name)
        bpy.ops.render.render(write_still=True)
    capture('01-authored-plateau-overview.png',(164,-188,184),(0,0,0),258)
    capture('02-authored-cliff-structure.png',(94,-167,119),(0,-12,0),238)

def write_report(obj):
    bpy.context.view_layer.objects.active=obj
    bbox=[obj.matrix_world@Vector(corner) for corner in obj.bound_box]
    lo=[min(p[k] for p in bbox) for k in range(3)]
    hi=[max(p[k] for p in bbox) for k in range(3)]
    p=OUT/'Valoria_CanonicalPlateau_Geology_v1.glb'
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.ops.export_scene.gltf(filepath=str(p),export_format='GLB',
                              use_selection=True,export_apply=True)
    if not p.is_file() or p.stat().st_size<150000:
        raise RuntimeError('Missing or truncated real sculpted 3D export')
    REPORT.update({
        'technical':'NATIVE_BLENDER_EXPORTED_NEEDS_VISUAL_REVIEW',
        'blender':bpy.app.version_string,
        'mesh_count':1,
        'vertices':len(obj.data.vertices),
        'polygons':len(obj.data.polygons),
        'bounds_min':lo,'bounds_max':hi,
        'glb_bytes':p.stat().st_size,
        'glb_sha256':hashlib.sha256(p.read_bytes()).hexdigest(),
        'source_images':['CanonicalPlateau_grass_olive_albedo.png',Path(A.rock_albedo).name,Path(A.rock_normal).name],
        'frames':['01-authored-plateau-overview.png','02-authored-cliff-structure.png'],
        'integration':False,'art_approved':False
    })
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Valoria_CanonicalPlateau_Geology_v1.blend'))

try:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    obj=make_geometry()
    studio()
    write_report(obj)
except Exception as e:
    REPORT['technical']='FAILED'
    REPORT['error']=repr(e)
    traceback.print_exc()
finally:
    (OUT/'authoring-report.json').write_text(json.dumps(REPORT,indent=2),encoding='utf-8')
print('REFERENCE_FIRST_GEOLOGY_V1_AUTHORING_RESULT:',REPORT['technical'])
if REPORT['technical']!='NATIVE_BLENDER_EXPORTED_NEEDS_VISUAL_REVIEW':
    raise RuntimeError('Reference-first geology authoring failed; inspect exact report')
