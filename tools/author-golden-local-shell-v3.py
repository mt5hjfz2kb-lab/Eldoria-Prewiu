import bpy, math, json, hashlib, os
from pathlib import Path
from mathutils import Vector

ROOT=Path(os.environ.get('GITHUB_WORKSPACE',os.getcwd()))
SRC=ROOT/'art-source/valoria/lookdev/golden-slice-v1/geometry-v3'
EVID=ROOT/'docs/evidence/valoria-golden-lookdev-slice-v1/geometry-v3'
SRC.mkdir(parents=True,exist_ok=True);EVID.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)

ORIGIN=Vector((4.0,-22.0,7.0))
objs=[]

def material(name,color,rough):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=rough
    return m
GROUND=material('Golden Ground shell',(.49,.42,.29),.88)
ROCK=material('Golden Rock shell',(.53,.50,.43),.86)
STONE=material('Golden Stone contact',(.70,.62,.50),.72)
BARK=material('Golden Local Bark',(.27,.16,.08),.85)
NEEDLE=material('Golden Local Needles',(.29,.46,.25),.80)

def metric_uv(o,scale=3.0):
    uv=o.data.uv_layers.new(name='MetricUV');o.data.uv_layers.active=uv;uv.active_render=True
    for poly in o.data.polygons:
        axis=max(range(3),key=lambda k:abs(poly.normal[k]))
        for li in poly.loop_indices:
            vi=o.data.loops[li].vertex_index;v=o.data.vertices[vi].co
            uv.data[li].uv=((v.y/scale,v.z/scale) if axis==0 else (v.x/scale,v.z/scale) if axis==1 else (v.x/scale,v.y/scale))

def smooth(o):
    for p in o.data.polygons:p.use_smooth=True

def radial_patch(name,cx,cy,rx,ry,zbase,mat,seed,seg=28,rings=4,slope=(0,0),height=.10):
    verts=[(cx,cy,zbase)]
    ring_ids=[]
    for r in range(1,rings+1):
        rr=r/rings;ids=[]
        for i in range(seg):
            a=2*math.pi*i/seg
            jitter=1+.09*math.sin(a*3+seed)+.045*math.sin(a*7+seed*.63)
            x=cx+math.cos(a)*rx*rr*jitter
            y=cy+math.sin(a)*ry*rr*(1+.06*math.sin(a*5+seed*1.2))
            macro=math.sin(x*.47+y*.31+seed)*height*(1-rr*.55)+math.sin(x*.19-y*.63)*height*.45
            taper=-.09*max(0,(rr-.72)/.28)
            z=zbase+slope[0]*(x-cx)+slope[1]*(y-cy)+macro+taper
            verts.append((x,y,z));ids.append(len(verts)-1)
        ring_ids.append(ids)
    faces=[]
    first=ring_ids[0]
    for i in range(seg):faces.append((0,first[i],first[(i+1)%seg]))
    for r in range(rings-1):
        a=ring_ids[r];b=ring_ids[r+1]
        for i in range(seg):
            n=(i+1)%seg;faces.extend([(a[i],b[i],a[n]),(a[n],b[i],b[n])])
    me=bpy.data.meshes.new(name+'_Mesh');me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(mat);metric_uv(o,3.0);smooth(o);objs.append(o);return o

def rock(name,c,r,phase):
    sx,sy,sz=r;seg=24;rings=12
    verts=[(c[0],c[1],c[2]+sz)]
    for j in range(1,rings):
        ph=math.pi*j/rings
        for i in range(seg):
            th=2*math.pi*i/seg
            noise=1+.10*math.sin(th*3+phase)+.055*math.sin(th*7+ph*2.1+phase)+.04*math.cos(ph*5-th*2)
            x=c[0]+sx*math.sin(ph)*math.cos(th)*noise
            y=c[1]+sy*math.sin(ph)*math.sin(th)*noise
            z=c[2]+sz*math.cos(ph)*(1+.045*math.sin(th*4+phase))
            verts.append((x,y,z))
    bottom=len(verts);verts.append((c[0],c[1],c[2]-sz*.72))
    faces=[]
    for i in range(seg):faces.append((0,1+i,1+(i+1)%seg))
    for j in range(rings-2):
        a=1+j*seg;b=a+seg
        for i in range(seg):
            n=(i+1)%seg;faces.extend([(a+i,b+i,a+n),(a+n,b+i,b+n)])
    last=1+(rings-2)*seg
    for i in range(seg):faces.append((last+i,bottom,last+(i+1)%seg))
    me=bpy.data.meshes.new(name+'_Mesh');me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(ROCK);metric_uv(o,2.35);smooth(o);objs.append(o);return o

def beveled_box(name,center,size):
    bpy.ops.mesh.primitive_cube_add(size=1,location=center);o=bpy.context.object;o.name=name;o.scale=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(STONE)
    mod=o.modifiers.new('Contact bevel','BEVEL');mod.width=.085;mod.segments=3;mod.limit_method='ANGLE'
    bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
    metric_uv(o,2.8);objs.append(o)

# Final bounded substrate treatment: one irregular hero apron and two sloped cliff aprons.
# These overlap the inherited substrate only inside the Golden Slice and taper at their irregular perimeter.
def hero_apron_z(x,y,u,v):
    return 7.035 + math.sin(x*.31+y*.23)*.045 + math.sin(x*.61-y*.17)*.025
radial_patch('GoldenGround_HeroApron',4.6,-24.7,14.6,4.35,7.035,GROUND,5.2,40,5,(0,.002),.055)

def cliff_patch(name,cx,cy,rx,ry,seed):
    def zfn(x,y,u,v):
        t=max(0,min(1,(y+35.2)/11.3))
        z=.30+t*6.55
        z+=math.sin(x*.37+y*.21+seed)*.095+math.sin(x*.19-y*.53+seed*.7)*.055
        return z
    # Build an irregular radial patch then overwrite its Z with the local cliff profile.
    o=radial_patch(name,cx,cy,rx,ry,3.3,ROCK,seed,36,5,(0,0),.035)
    for vtx in o.data.vertices:
        x,y=vtx.co.x,vtx.co.y
        t=max(0,min(1,(y+35.2)/11.3))
        edge=max(abs((x-cx)/rx),abs((y-cy)/ry))
        z=.30+t*6.55 + math.sin(x*.37+y*.21+seed)*.095 + math.sin(x*.19-y*.53+seed*.7)*.055
        if edge>.78: z-=((edge-.78)/.22)*.08
        vtx.co.z=z
    o.data.update()
    return o
cliff_patch('Rock_GoldenCliffApron_West',-13.7,-29.7,7.3,5.15,6.0)
cliff_patch('Rock_GoldenCliffApron_East',23.0,-29.2,8.1,5.25,7.1)

# Irregular berms/contact patches only — no broad rectangular cliff sheets.
radial_patch('GoldenGround_GateWest',-5.0,-25.1,4.9,2.25,7.03,GROUND,.3,30,4,(0,.004),.075)
radial_patch('GoldenGround_GateEast',14.2,-24.8,5.2,2.35,7.03,GROUND,1.4,30,4,(0,.004),.075)
radial_patch('GoldenGround_BridgeWestShoulder',-1.1,-29.5,2.35,1.45,3.20,GROUND,2.0,26,3,(0,.008),.055)
radial_patch('GoldenGround_BridgeEastShoulder',9.1,-29.2,2.35,1.45,3.20,GROUND,2.7,26,3,(0,.008),.055)

beveled_box('GateFoundationContact_GoldenWest',(-4.25,-24.50,7.10),(8.7,.66,.42))
beveled_box('GateFoundationContact_GoldenEast',(14.45,-24.15,7.10),(8.2,.66,.42))

# Overlapping rock banks hide patch boundaries and provide non-faceted silhouette without replacing the global platform.
rock_specs=[
('Rock_GoldenWest_A',(-8.15,-26.75,6.10),(2.35,1.55,1.30),.2),
('Rock_GoldenWest_B',(-5.30,-27.25,6.15),(1.65,1.20,.95),1.1),
('Rock_GoldenWest_C',(-2.85,-26.55,6.35),(1.45,1.05,.80),2.4),
('Rock_GoldenEast_A',(12.25,-26.45,6.25),(1.65,1.18,.95),.8),
('Rock_GoldenEast_B',(15.15,-26.15,6.05),(2.15,1.45,1.20),1.9),
('Rock_GoldenEast_C',(18.10,-25.35,6.30),(1.60,1.12,.90),2.8),
('ShoreRock_GoldenWest_A',(-4.0,-33.9,.48),(1.85,1.25,.74),.5),
('ShoreRock_GoldenWest_B',(-1.8,-33.45,.45),(1.30,.95,.61),1.8),
('ShoreRock_GoldenEast_A',(7.2,-33.5,.47),(1.55,1.05,.66),2.3),
('ShoreRock_GoldenEast_B',(9.25,-33.2,.44),(1.45,.98,.58),3.1)
]
for args in rock_specs:rock(*args)

# Shore lips are compact irregular patches, not ribbons/sheets.
radial_patch('ShoreRock_GoldenBankWest',-3.0,-33.75,4.1,1.35,.20,ROCK,3.2,32,3,(0,.06),.045)
radial_patch('ShoreRock_GoldenBankEast',8.2,-33.35,4.0,1.40,.20,ROCK,4.1,32,3,(0,.06),.045)

def cone_mesh(name,cx,cy,z0,radius,height,phase,mat):
    seg=18;verts=[];faces=[]
    # shallow drooping whorl, not a perfect cone
    verts.append((cx,cy,z0+height))
    for ring in range(1,4):
        t=ring/3;rr=radius*t*(1+.08*math.sin(phase+ring))
        z=z0+height*(1-t*.92)
        start=len(verts)
        for i in range(seg):
            a=2*math.pi*i/seg
            wob=1+.08*math.sin(a*3+phase)+.035*math.sin(a*7+phase*.7)
            verts.append((cx+math.cos(a)*rr*wob,cy+math.sin(a)*rr*wob,z-.09*math.sin(a*2+phase)))
        if ring==1:
            for i in range(seg):faces.append((0,start+i,start+(i+1)%seg))
        if ring>1:
            prev=start-seg
            for i in range(seg):
                n=(i+1)%seg;faces.extend([(prev+i,start+i,prev+n),(prev+n,start+i,start+n)])
    me=bpy.data.meshes.new(name+'_Mesh');me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(mat);metric_uv(o,1.6);smooth(o);objs.append(o)

def tree(name,pos,height,radius,phase):
    x,y,z=pos
    # tapered trunk
    seg=12;verts=[];faces=[]
    for j in range(5):
        t=j/4;rr=radius*.12*(1-.52*t);zz=z+height*.78*t
        for i in range(seg):
            a=2*math.pi*i/seg;verts.append((x+math.cos(a)*rr,y+math.sin(a)*rr,zz))
    for j in range(4):
        for i in range(seg):
            a=j*seg+i;b=j*seg+(i+1)%seg;c=(j+1)*seg+i;d=(j+1)*seg+(i+1)%seg
            faces.extend([(a,c,b),(b,c,d)])
    me=bpy.data.meshes.new(name+'_TrunkMesh');me.from_pydata(verts,[],faces);me.update()
    tr=bpy.data.objects.new(name+'_Trunk',me);bpy.context.collection.objects.link(tr);tr.data.materials.append(BARK);metric_uv(tr,1.4);smooth(tr);objs.append(tr)
    # overlapping asymmetric conifer whorls compatible with retained family.
    levels=[(.28,1.00,.22),(.40,.88,.20),(.52,.74,.18),(.64,.60,.16),(.75,.46,.14),(.84,.30,.12)]
    for k,(zf,rf,hf) in enumerate(levels):
        cone_mesh(f'{name}_Canopy_{k:02d}',x+math.sin(phase+k*.8)*radius*.07,y+math.cos(phase+k*.7)*radius*.05,z+height*zf,radius*rf,height*hf,phase+k*.55,NEEDLE)

tree('Tree_GoldenLocal_WestGate',(-5.6642,-18.7011,7.0),8.1,2.25,.3)
tree('Tree_GoldenLocal_ForegroundWest',(-8.9408,-32.8607,3.0),9.0,2.55,1.2)
tree('Tree_GoldenLocal_BridgeEast',(25.0353,-26.0233,0.0),8.7,2.42,2.1)

for o in objs:
    bpy.context.view_layer.objects.active=o;bpy.context.scene.cursor.location=ORIGIN
    o.select_set(True);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');o.select_set(False)

blend=SRC/'GoldenLocalVisualShellV3.blend';bpy.ops.wm.save_as_mainfile(filepath=str(blend))
for o in objs:o.location-=ORIGIN
bpy.ops.object.select_all(action='DESELECT')
for o in objs:o.select_set(True)
glb=SRC/'GoldenLocalVisualShellV3.glb'
bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True,export_yup=True,export_materials='EXPORT',export_normals=True,export_tangents=True)
for o in objs:o.location+=ORIGIN
bpy.context.view_layer.update()

scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE';scene.render.resolution_x=1200;scene.render.resolution_y=800;scene.render.resolution_percentage=100
world=bpy.data.worlds.new('Golden local neutral');world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.22,.24,.27,1);world.node_tree.nodes['Background'].inputs[1].default_value=.6;scene.world=world
center=Vector((4,-25,5.0))
for loc,energy,size in [((-18,-42,30),1700,10),((25,-15,18),900,9)]:
    bpy.ops.object.light_add(type='AREA',location=loc);l=bpy.context.object;l.data.energy=energy;l.data.size=size;l.rotation_euler=(center-l.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type='ORTHO';scene.camera=cam
clay=bpy.data.materials.new('Golden shell clay');clay.diffuse_color=(.56,.54,.50,1);clay.roughness=.9
views=[('source-lit-three-quarter',(30,-52,30),42,False),('source-official-proxy',(24,-55,28),38,False),('source-clay',(30,-52,30),42,True)]
previews=[]
for name,loc,span,isclay in views:
    cam.location=loc;cam.data.ortho_scale=span;cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
    scene.view_layers[0].material_override=clay if isclay else None
    out=EVID/(name+'.png');scene.render.filepath=str(out);bpy.ops.render.render(write_still=True);previews.append(str(out.relative_to(ROOT)).replace('\\','/'))
scene.view_layers[0].material_override=None

tris=verts=0;bounds=[]
for o in objs:
    if o.type!='MESH':continue
    o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles);verts+=len(o.data.vertices);bounds.extend([o.matrix_world@Vector(c) for c in o.bound_box])
lo=[min(p[i] for p in bounds) for i in range(3)];hi=[max(p[i] for p in bounds) for i in range(3)]
sha=lambda p:hashlib.sha256(Path(p).read_bytes()).hexdigest()
report={
 'authoring_standard':'BLENDER_PROFESSIONAL_V1','family':'GoldenLocalVisualShellV3',
 'scope':'Source03 final bounded substrate uplift: source02 retained plus irregular Hero Ground Apron and two irregular sloped Cliff Aprons covering only screen-visible inherited substrate facets.',
 'source':str(blend.relative_to(ROOT)).replace('\\','/'),'export':str(glb.relative_to(ROOT)).replace('\\','/'),
 'source_sha':sha(blend),'export_sha':sha(glb),
 'geometry_metrics':{'triangles':tris,'vertices':verts,'objects':len(objs),'bounds_world_blender':[lo,hi],'uv':True,'normals':True,'tangents_exported':True},
 'unity_placement':{'position':[ORIGIN.x,ORIGIN.z,ORIGIN.y],'rotation_y':180,'scale':1},
 'surface_basis':'Golden Surface V2 persisted maps on rock/ground/stone; source materials retained for local trees',
 'source02_retained_and_extended':['removed broad rectangular cliff sheets','removed rectangular road/ground sheets','irregular tapered borders','conifer-compatible tree language'],
 'preview_evidence':previews,'gameplay_geometry':False,'colliders':0,'macro_positions_changed':False,'tripo_credits':0,'isolated_art_review':'PENDING'
}
(EVID/'source-report.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(json.dumps(report,indent=2))
