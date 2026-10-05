import bpy, bmesh, math, json, hashlib, os
from pathlib import Path
from mathutils import Vector
import numpy as np

ROOT=Path(os.environ.get('GITHUB_WORKSPACE',os.getcwd()))
SRC=ROOT/'art-source/valoria/lookdev/golden-slice-v1/geometry-v1'
EVID=ROOT/'docs/evidence/valoria-golden-lookdev-slice-v1/geometry-v1'
SRC.mkdir(parents=True,exist_ok=True);EVID.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)

ORIGIN=Vector((4.0,-22.0,7.0))
objs=[]

def mat_tex(name,base,tex_file=None,rough=.8):
    m=bpy.data.materials.new(name);m.use_nodes=True
    bsdf=m.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value=(*base,1)
    bsdf.inputs['Roughness'].default_value=rough
    if tex_file:
        p=ROOT/'art-source/valoria/lookdev/golden-slice-v1/surface-v2'/tex_file
        if p.is_file():
            im=bpy.data.images.load(str(p),check_existing=True)
            tx=m.node_tree.nodes.new('ShaderNodeTexImage');tx.image=im
            m.node_tree.links.new(tx.outputs['Color'],bsdf.inputs['Base Color'])
    return m

GROUND=mat_tex('Golden Ground V2',(.47,.40,.27),'ground_albedo.png',.88)
ROCK=mat_tex('Golden Rock V2',(.44,.43,.39),'rock_albedo.png',.84)
STONE=mat_tex('Golden Stone V2',(.72,.63,.50),'stone_albedo.png',.68)
BARK=mat_tex('Golden Local Bark',(.25,.14,.075),None,.82)
NEEDLE=mat_tex('Golden Local Needles',(.25,.43,.24),'vegetation_albedo.png',.78)

def metric_uv(o,scale=3.0):
    uv=o.data.uv_layers.new(name='MetricUV');o.data.uv_layers.active=uv;uv.active_render=True
    for poly in o.data.polygons:
        axis=max(range(3),key=lambda k:abs(poly.normal[k]))
        for li in poly.loop_indices:
            vi=o.data.loops[li].vertex_index;v=o.data.vertices[vi].co
            uv.data[li].uv=((v.y/scale,v.z/scale) if axis==0 else (v.x/scale,v.z/scale) if axis==1 else (v.x/scale,v.y/scale))

def smooth_obj(o):
    for p in o.data.polygons:p.use_smooth=True

def grid_patch(name,x0,x1,y0,y1,zfn,nx,ny,mat,edge_taper=False):
    verts=[];faces=[]
    for j in range(ny+1):
        v=j/ny;y=y0+(y1-y0)*v
        for i in range(nx+1):
            u=i/nx;x=x0+(x1-x0)*u
            z=zfn(x,y,u,v)
            if edge_taper:
                e=min(u,1-u,v,1-v);z-=max(0,.12-e)*.10
            verts.append((x,y,z))
    for j in range(ny):
        for i in range(nx):
            a=j*(nx+1)+i;b=a+1;c=a+(nx+1);d=c+1
            faces += [(a,c,b),(b,c,d)]
    me=bpy.data.meshes.new(name+'_Mesh');me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(mat);metric_uv(o,3.2);smooth_obj(o);objs.append(o);return o

def cliff_z(side):
    def f(x,y,u,v):
        t=np.clip((y+35.0)/11.2,0,1)
        terrace=(math.sin(t*math.pi*5.2+u*1.1)*.16 + math.sin(u*math.pi*4.0+v*.8)*.10)
        macro=math.sin(x*.31+y*.19)*.16+math.sin(x*.13-y*.37)*.11
        shoulder=(t*t*(3-2*t))*6.55+.38
        return shoulder+terrace+macro+(0.12 if side=='east' else 0)
    return f

# Screen-visible terrain shells: explicit bounded patches, not global platform replacement.
grid_patch('Rock_GoldenCliff_West',-24,-9.7,-35.2,-24.0,cliff_z('west'),24,18,ROCK,True)
grid_patch('Rock_GoldenCliff_East',17.5,36.0,-35.0,-23.8,cliff_z('east'),28,18,ROCK,True)

def gate_ground(x,y,u,v):
    return 7.05 + math.sin(x*.55+y*.31)*.055 + math.sin(x*.21-y*.62)*.035
grid_patch('GoldenGround_GateWest',-10.2,-.65,-28.0,-23.1,gate_ground,18,10,GROUND,True)
grid_patch('GoldenGround_GateEast',9.65,20.4,-27.8,-22.95,gate_ground,20,10,GROUND,True)

def road_ground(x,y,u,v):
    return 7.03 + math.sin(y*.72+x*.41)*.035 + math.sin(y*.27)*.025
grid_patch('GoldenGround_RoadWest',-.8,2.35,-22.4,-12.7,road_ground,6,20,GROUND,True)
grid_patch('GoldenGround_RoadEast',5.0,8.0,-22.0,-12.8,road_ground,6,20,GROUND,True)

def shoulder(x,y,u,v):
    return 3.20 + math.sin(x*.7+y*.55)*.045
grid_patch('GoldenGround_BridgeWestShoulder',-3.7,.65,-31.0,-28.0,shoulder,10,8,GROUND,True)
grid_patch('GoldenGround_BridgeEastShoulder',7.0,11.3,-30.8,-27.8,shoulder,10,8,GROUND,True)

def box_beveled(name,center,size,mat,bevel=.08):
    bpy.ops.mesh.primitive_cube_add(size=1,location=center);o=bpy.context.object;o.name=name;o.scale=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    o.data.materials.append(mat)
    mod=o.modifiers.new('Profile bevel','BEVEL');mod.width=bevel;mod.segments=3;mod.limit_method='ANGLE'
    bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
    metric_uv(o,2.8);objs.append(o);return o
box_beveled('GateFoundationContact_GoldenWest',(-4.25,-24.50,7.10),(8.7,.66,.42),STONE,.09)
box_beveled('GateFoundationContact_GoldenEast',(14.45,-24.15,7.10),(8.2,.66,.42),STONE,.09)

def rock(name,c,r,phase):
    sx,sy,sz=r; seg=20;rings=10
    verts=[(c[0],c[1],c[2]+sz)]
    for j in range(1,rings):
        phi=math.pi*j/rings
        for i in range(seg):
            th=2*math.pi*i/seg
            noise=1+.13*math.sin(th*3+phase)+.08*math.sin(th*5+phi*2.3+phase*.7)+.05*math.cos(phi*4-th*2)
            x=c[0]+sx*math.sin(phi)*math.cos(th)*noise
            y=c[1]+sy*math.sin(phi)*math.sin(th)*noise
            z=c[2]+sz*math.cos(phi)*(1+.05*math.sin(th*4+phase))
            if z<c[2]-sz*.68:z=c[2]-sz*.68
            verts.append((x,y,z))
    verts.append((c[0],c[1],c[2]-sz*.68));bottom=len(verts)-1
    faces=[]
    for i in range(seg):faces.append((0,1+i,1+(i+1)%seg))
    for j in range(rings-2):
        a=1+j*seg;b=a+seg
        for i in range(seg):
            n=(i+1)%seg;faces += [(a+i,b+i,a+n),(a+n,b+i,b+n)]
    last=1+(rings-2)*seg
    for i in range(seg):faces.append((last+i,bottom,last+(i+1)%seg))
    me=bpy.data.meshes.new(name+'_Mesh');me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(ROCK);metric_uv(o,2.2);smooth_obj(o);objs.append(o);return o

rocks=[
 ('Rock_GoldenWest_A',(-7.45,-26.65,6.38),(1.65,1.20,1.10),.2),
 ('Rock_GoldenWest_B',(-4.85,-27.40,6.20),(1.20,.95,.82),1.1),
 ('Rock_GoldenEast_A',(13.30,-26.55,6.42),(1.55,1.18,1.05),2.0),
 ('Rock_GoldenEast_B',(16.10,-25.85,6.18),(1.82,1.32,1.12),2.8),
 ('ShoreRock_GoldenWest',(-2.75,-33.85,.52),(1.65,1.20,.72),.8),
 ('ShoreRock_GoldenEast',(8.15,-33.48,.50),(1.55,1.14,.70),2.3)
]
for a,b,c,d in rocks:rock(a,b,c,d)

# Wet bank lips, smoother transition than flat strips.
def shorefn(x,y,u,v):
    return .16 + v*.52 + math.sin(x*.42+y*.61)*.035
grid_patch('ShoreRock_GoldenBankWest',-9.5,1.4,-35.0,-32.1,shorefn,22,8,ROCK,True)
grid_patch('ShoreRock_GoldenBankEast',5.5,20.5,-35.0,-31.8,shorefn,28,8,ROCK,True)

def tree(name,pos,height,radius,phase):
    x,y,z=pos
    # tapered trunk
    seg=12;verts=[];faces=[];rings=5
    for j in range(rings):
        t=j/(rings-1);rr=radius*.24*(1-.45*t);zz=z+height*.76*t
        for i in range(seg):
            th=2*math.pi*i/seg
            wob=.05*math.sin(th*3+phase+j*.7)
            verts.append((x+math.cos(th)*(rr+wob),y+math.sin(th)*(rr+wob),zz))
    for j in range(rings-1):
        for i in range(seg):
            a=j*seg+i;b=j*seg+(i+1)%seg;c=(j+1)*seg+i;d=(j+1)*seg+(i+1)%seg
            faces += [(a,c,b),(b,c,d)]
    me=bpy.data.meshes.new(name+'_TrunkMesh');me.from_pydata(verts,[],faces);me.update()
    tr=bpy.data.objects.new(name+'_Trunk',me);bpy.context.collection.objects.link(tr);tr.data.materials.append(BARK);metric_uv(tr,1.8);smooth_obj(tr);objs.append(tr)

    # overlapping organic canopy lobes, avoiding stacked cone silhouette.
    lobe_specs=[(0,.40,1.00,.95),(-.22,.53,.82,.72),(.25,.56,.78,.70),(-.15,.68,.60,.58),(.12,.78,.48,.48),(.0,.88,.32,.35)]
    for k,(xo,zo,rx,rz) in enumerate(lobe_specs):
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,location=(x+xo*radius,y+math.sin(k*1.7+phase)*radius*.22,z+height*zo))
        o=bpy.context.object;o.name=f'{name}_Canopy_{k:02d}'
        o.scale=(radius*rx*(1+.08*math.sin(phase+k)),radius*rx*.82,height*.18*rz)
        bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
        # asymmetric sculpt-like deformation
        for vtx in o.data.vertices:
            p=vtx.co;ang=math.atan2(p.y,p.x);s=1+.08*math.sin(ang*3+phase+k*.8)+.05*math.sin(p.z*5+k)
            p.x*=s;p.y*=s
        o.data.materials.append(NEEDLE);metric_uv(o,2.0);smooth_obj(o);objs.append(o)

tree('Tree_GoldenLocal_WestGate',(-5.6642,-18.7011,7.0),8.2,2.15,.3)
tree('Tree_GoldenLocal_ForegroundWest',(-8.9408,-32.8607,3.0),9.1,2.45,1.2)
tree('Tree_GoldenLocal_BridgeEast',(25.0353,-26.0233,0.0),8.8,2.35,2.1)

# Set pivots to shared local-family origin then export local.
for o in objs:
    bpy.context.view_layer.objects.active=o
    bpy.context.scene.cursor.location=ORIGIN
    o.select_set(True);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');o.select_set(False)

blend=SRC/'GoldenLocalVisualShellV1.blend'
bpy.ops.wm.save_as_mainfile(filepath=str(blend))
for o in objs:o.location-=ORIGIN
bpy.ops.object.select_all(action='DESELECT')
for o in objs:o.select_set(True)
glb=SRC/'GoldenLocalVisualShellV1.glb'
bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True,export_yup=True,export_materials='EXPORT',export_normals=True,export_tangents=True)
for o in objs:o.location+=ORIGIN
bpy.context.view_layer.update()

# Review renders.
scene=bpy.context.scene
scene.render.engine='BLENDER_EEVEE_NEXT';scene.render.resolution_x=1200;scene.render.resolution_y=800;scene.render.resolution_percentage=100
world=bpy.data.worlds.new('Golden local neutral');world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.22,.24,.27,1);world.node_tree.nodes['Background'].inputs[1].default_value=.55;scene.world=world
center=Vector((4,-25,5.0))
for loc,energy,size in [(( -18,-42,30),1700,10),((25,-15,18),900,9)]:
    bpy.ops.object.light_add(type='AREA',location=loc);l=bpy.context.object;l.data.energy=energy;l.data.shape='DISK';l.data.size=size;l.rotation_euler=(center-l.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type='ORTHO';scene.camera=cam
views=[
 ('source-lit-three-quarter',(30,-52,30),42,False),
 ('source-official-proxy',(24,-55,28),38,False),
 ('source-clay',(30,-52,30),42,True)
]
clay=bpy.data.materials.new('Golden shell clay');clay.diffuse_color=(.56,.54,.50,1);clay.roughness=.9
previews=[]
for name,loc,span,isclay in views:
    cam.location=loc;cam.data.ortho_scale=span;cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
    scene.view_layers[0].material_override=clay if isclay else None
    out=EVID/(name+'.png');scene.render.filepath=str(out);bpy.ops.render.render(write_still=True);previews.append(str(out.relative_to(ROOT)).replace('\\','/'))
scene.view_layers[0].material_override=None

tris=verts=0;bounds=[]
for o in objs:
    if o.type!='MESH':continue
    o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles);verts+=len(o.data.vertices)
    bounds.extend([o.matrix_world@Vector(c) for c in o.bound_box])
lo=[min(p[i] for p in bounds) for i in range(3)];hi=[max(p[i] for p in bounds) for i in range(3)]
sha=lambda p:hashlib.sha256(Path(p).read_bytes()).hexdigest()
report={
 'authoring_standard':'BLENDER_PROFESSIONAL_V1',
 'family':'GoldenLocalVisualShellV1',
 'scope':'Bounded Golden Slice geometry escalation after three valid surface-only failures. Local cliff/ground/shore/contact/3 vegetation replacements only.',
 'source':str(blend.relative_to(ROOT)).replace('\\','/'),
 'export':str(glb.relative_to(ROOT)).replace('\\','/'),
 'source_sha':sha(blend),'export_sha':sha(glb),
 'geometry_metrics':{'triangles':tris,'vertices':verts,'objects':len(objs),'bounds_world_blender':[lo,hi],'uv':True,'normals':True,'tangents_exported':True},
 'unity_placement':{'position':[ORIGIN.x,ORIGIN.z,ORIGIN.y],'rotation_y':180,'scale':1},
 'surface_basis':'Golden Surface V2 persisted maps',
 'preview_evidence':previews,
 'gameplay_geometry':False,'colliders':0,'macro_positions_changed':False,'tripo_credits':0,
 'isolated_art_review':'PENDING'
}
(EVID/'source-report.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(json.dumps(report,indent=2))
