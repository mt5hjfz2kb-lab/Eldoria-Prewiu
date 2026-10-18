"""Continue exact recovered terrain: genuine local Qwen art direction, Blender execution.
Only World4X outputs; original terrain, references, Valoria and saves remain immutable.
Run with Blender --background --python-exit-code 1 --python ... -- --out PATH.
"""
import argparse, base64, hashlib, json, math, os, pathlib, sys, time, urllib.request
import bpy
import numpy as np
from mathutils import Vector
from mathutils.bvhtree import BVHTree

ROOT = pathlib.Path(__file__).resolve().parents[3]
REL = pathlib.Path('Unity/Assets/Eldoria/ArtTests/World4XZone1')
SOURCE = ROOT/'docs/world4x-zone1/evidence/general-ground-38580519176/independent/general-ground-candidate.glb'
EXPECTED = '6c8331bd5118f0f14529580f5ac2be4ef87f1b969e1c840e5f401b04e9e460f8'
TARGET = 'ZoneOne continuous prairie source'

def digest(p): return hashlib.sha256(pathlib.Path(p).read_bytes()).hexdigest()
def write(p, data): pathlib.Path(p).write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n',encoding='utf-8')
def request(route, payload=None, timeout=20):
    data=None if payload is None else json.dumps(payload).encode()
    req=urllib.request.Request('http://127.0.0.1:11434'+route,data=data,headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=timeout) as response: return json.load(response)
def smooth(a,b,x):
    t=np.clip((x-a)/(b-a),0,1);return t*t*(3-2*t)
def srgb_to_linear(c):
    c=np.asarray(c,dtype=np.float32)/255
    return np.where(c<=.04045,c/12.92,((c+.055)/1.055)**2.4)
def world_points(obj):
    return np.array([list(obj.matrix_world@v.co) for v in obj.data.vertices],dtype=np.float64)
def lighting():
    scene=bpy.context.scene
    world=bpy.data.worlds.new('4X review daylight');scene.world=world;world.use_nodes=True
    world.node_tree.nodes['Background'].inputs['Color'].default_value=(.52,.64,.78,1)
    world.node_tree.nodes['Background'].inputs['Strength'].default_value=.45
    light=bpy.data.lights.new('4X review sun','SUN');light.energy=2.2;light.angle=.10
    obj=bpy.data.objects.new(light.name,light);scene.collection.objects.link(obj)
    obj.rotation_euler=(math.radians(28),math.radians(-23),math.radians(-35))
    camera=bpy.data.cameras.new('4X fixed review camera');cam=bpy.data.objects.new(camera.name,camera)
    scene.collection.objects.link(cam);scene.camera=cam;camera.type='ORTHO';camera.sensor_fit='VERTICAL'
    scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True
    scene.render.resolution_percentage=100
    scene.view_settings.view_transform='Standard';scene.view_settings.look='Medium High Contrast' if 'Medium High Contrast' in [i.name for i in scene.view_settings.bl_rna.properties['look'].enum_items] else 'None'
    scene.view_settings.exposure=0;scene.view_settings.gamma=1
    return scene,cam
VIEWS=[('global',(0,0,0),245,1000,900),('regional',(-18,-5,0),150,1000,900),('local',(-18,-24,0),84,1000,900),('mobile',(-18,-5,0),155,540,960)]
def render(out,prefix,views=VIEWS):
    scene=bpy.context.scene;cam=scene.camera;records=[]
    for label,focus,scale,w,h in views:
        target=Vector(focus);cam.location=target+Vector((0,-210,300));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler()
        cam.data.ortho_scale=scale;scene.render.resolution_x=w;scene.render.resolution_y=h
        p=out/(prefix+'-'+label+'-blender.png');scene.render.filepath=str(p);bpy.ops.render.render(write_still=True)
        records.append({'view':label,'width':w,'height':h,'ortho_scale':scale,'matrix':[list(r) for r in cam.matrix_world],'sha256':digest(p),'filename':p.name})
    return records

def schema():
    rgb={'type':'array','items':{'type':'integer','minimum':0,'maximum':255},'minItems':3,'maxItems':3}
    num=lambda lo,hi:{'type':'number','minimum':lo,'maximum':hi}
    ellipse={'type':'object','properties':{'x':num(-92,92),'y':num(-92,92),'rx':num(12,48),'ry':num(12,58),'height':num(-3.5,8.5),'angle':num(-180,180)},'required':['x','y','rx','ry','height','angle'],'additionalProperties':False}
    palette={'type':'object','properties':{k:rgb for k in ['meadow','meadow_light','dry','soil','rock','path']},'required':['meadow','meadow_light','dry','soil','rock','path'],'additionalProperties':False}
    props={'diagnosis':{'type':'string'},'design_intent':{'type':'string'},'palette_srgb':palette,'landforms':{'type':'array','items':ellipse,'minItems':2,'maxItems':3},'macro_scale':num(24,60),'dry_amount':num(.12,.30),'micro_strength':num(.025,.09),'slope_rock_start':num(11,22),'slope_rock_end':num(25,40)}
    return {'type':'object','properties':props,'required':list(props),'additionalProperties':False}

def validate(plan):
    if set(plan)!=set(schema()['required']):raise ValueError('Missing or unknown plan field')
    pal=plan['palette_srgb']
    if set(pal)!=set(schema()['properties']['palette_srgb']['required']):raise ValueError('Palette fields')
    for k,v in pal.items():
        if len(v)!=3 or any(type(n)!=int or not 0<=n<=255 for n in v):raise ValueError('RGB must be authored sRGB integers')
    for k in ['meadow','meadow_light']:
        r,g,b=pal[k]
        if not (80<=r<=160 and 90<=g<=170 and 25<=b<=100 and g>=r and g>b+30):raise ValueError('Meadow must be a restrained olive green, not grey, neon or beige')
    if max(abs(a-b) for a,b in zip(pal['meadow'],pal['meadow_light']))>35:raise ValueError('Avoid previous camouflage contrast')
    if not (150<=pal['dry'][0]<=220 and 130<=pal['dry'][1]<=200 and 65<=pal['dry'][2]<=140):raise ValueError('Dry grass palette outside art brief')
    if not 2<=len(plan['landforms'])<=3:raise ValueError('2-3 original broad landforms required')
    for land in plan['landforms']:
        for k,v in land.items():
            bounds=schema()['properties']['landforms']['items']['properties'][k]
            if not isinstance(v,(int,float)) or not math.isfinite(v) or not bounds['minimum']<=v<=bounds['maximum']:raise ValueError('Out-of-bounds original landform')
    for k in ['macro_scale','dry_amount','micro_strength','slope_rock_start','slope_rock_end']:
        s=schema()['properties'][k]
        if not s['minimum']<=plan[k]<=s['maximum']:raise ValueError('Out-of-bounds original material recipe')
    if not any(abs(x['height'])>=3 for x in plan['landforms']):raise ValueError('Previous invisible relief edits must not repeat')

def author(out, context, before):
    models=request('/api/tags')['models'];names=[m['name'] for m in models]
    ordered=['qwen3-vl:8b-instruct-q4_K_M','qwen3-vl:4b-instruct','eldoria-artist:latest']
    model=next((n for n in ordered if n in names),None)
    if model is None:raise RuntimeError('No previously installed visual Qwen model; no download or substitution')
    refs=ROOT/'docs/world4x-zone1/evidence/local-cycle-20261014/references'
    images=[];provenance=[]
    for p in [refs/'global-reference.png',refs/'regional-reference.png',refs/'local-reference.png',out/before[1]['filename']]:
        im=bpy.data.images.load(str(p),check_existing=False);w,h=im.size;factor=min(1,480/max(w,h));im.scale(int(w*factor),int(h*factor))
        small=out/('vision-'+p.stem+'.png');im.filepath_raw=str(small);im.file_format='PNG';im.save();bpy.data.images.remove(im)
        images.append(base64.b64encode(small.read_bytes()).decode());provenance.append({'source':str(p.relative_to(ROOT)) if p.is_relative_to(ROOT) else p.name,'source_sha256':digest(p),'vision_sha256':digest(small)})
    memory=(ROOT/'docs/world4x-zone1/GROUND_ART_REVIEW_20261014.json').read_text(encoding='utf-8')
    prompt='''Act as terrain and material artist for Eldoria Zone One, NOT Valoria city. Images 1-3 are the approved global/regional/local references; image4 is the actual recovered Blender ground you will modify. The map is ONE continuous open prairie, few dispersed forest/mountain masses, small sea only in the SOUTH of the final world, and Breach in the centre of the FINAL WORLD. This 240m test patch is a local sector, not the entire world: do not add sea or invent a central Breach here. Focus solely on an attractive readable terrain foundation, never add buildings/trees to hide it. The source has an unchanged city pad and border. Preserve prior terrain work and add 2-3 deliberate BROAD ASYMMETRIC hillside/basin forms, whose ORIGINAL centres, radii, heights and angle you choose. Positive height makes a hill, negative makes a broad hollow. Protected city pad x[-40,4],y[-52,-8] and a 4m falloff around it are excluded; border is unchanged with 6m falloff. Actual Blender WORLD X/Y horizontal, Z up, units metres. The executor evaluates your elliptical smooth brush once on the source without any coordinate rewrites. Choose readable relief, no repeated regular bumps. The art realization uses your six exact sRGB palettes, terrain slope-aware exposed rock, broad meadow variation at macro_scale metres, dry grass at dry_amount, subdued donor microdetail at micro_strength, and preserves the existing painted route-centre mask. No flat tint, no grey meadow, no camouflage, no noise carpet. Meadow sRGB: R80..160 G90..170 B25..100, G>=R and G>B+30; second meadow differs by at most35 in each channel. Dry sRGB R150..220 G130..200 B65..140. Soil and path should be warmer light earth, rocks warm grey, using restrained reference-like contrast. Choose values yourself. The result is a production CANDIDATE with independent review, never self-approve. Explain concrete visible defects and intended changes. Output ONLY the exact JSON schema.'''
    prompt+='\nACTUAL SOURCE CONTEXT\n'+json.dumps(context)+'\nVERIFIED PREVIOUS REJECTIONS\n'+memory
    rolepath=ROOT/'tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md'
    role=rolepath.read_text(encoding='utf-8') if rolepath.exists() else 'You are the local 3D asset author. Return original typed decisions, no arbitrary code.'
    errors=[]
    for attempt in range(1,4):
        content=prompt+('\nPrior original response was rejected; independently correct: '+str(errors[-1]) if errors else '')
        payload={'model':model,'stream':False,'format':schema(),'messages':[{'role':'system','content':role},{'role':'user','content':content,'images':images}],'options':{'temperature':.25,'num_ctx':16384,'num_predict':1800},'keep_alive':'2m'}
        write(out/f'artist-request-{attempt}.json',{'model':model,'prompt':content,'system_sha256':hashlib.sha256(role.encode()).hexdigest(),'image_provenance':provenance,'schema':schema(),'options':payload['options']})
        started=time.time();raw=request('/api/chat',payload,timeout=330);write(out/f'qwen-raw-{attempt}.json',raw)
        try:
            plan=json.loads(raw['message']['content']);validate(plan)
            write(out/'artist-plan.json',plan)
            write(out/'genuine-author.json',{'model':model,'seconds':time.time()-started,'attempt':attempt,'plan_sha256':digest(out/'artist-plan.json'),'new_inference':True,'parameters_rewritten':False,'memory_sha256':hashlib.sha256(memory.encode()).hexdigest(),'art_approved':False,'prior_errors':errors})
            return plan
        except (ValueError,KeyError,TypeError) as e:
            errors.append(str(e));write(out/f'plan-rejection-{attempt}.json',{'error':str(e),'edited':False})
    raise RuntimeError('Three original Qwen plans rejected; originals retained: '+str(errors))

def apply_terrain(terrain,plan):
    before=world_points(terrain);p=before.copy();x,y=p[:,0],p[:,1]
    border=1-smooth(112,118,np.maximum(np.abs(x),np.abs(y)))
    dx=np.maximum(np.maximum(-40-x,x-4),0);dy=np.maximum(np.maximum(-52-y,y+8),0)
    safe=smooth(0,5,np.maximum(dx,dy))*border
    for land in plan['landforms']:
        a=math.radians(land['angle']);u=(x-land['x'])*math.cos(a)+(y-land['y'])*math.sin(a);v=-(x-land['x'])*math.sin(a)+(y-land['y'])*math.cos(a)
        d=(u/land['rx'])**2+(v/land['ry'])**2
        p[:,2]+=land['height']*np.clip(1-d,0,1)**2*safe
    inverse=terrain.matrix_world.inverted()
    for vert,point in zip(terrain.data.vertices,p):vert.co=inverse@Vector(point)
    if terrain.data.has_custom_normals:terrain.data.normals_split_custom_set([(0,0,0)]*len(terrain.data.loops))
    terrain.data.update();bpy.context.view_layer.update()
    after=world_points(terrain);protected=(np.abs(x)>=118)|(np.abs(y)>=118)|((x>=-40)&(x<=4)&(y>=-52)&(y<=-8))
    assert np.max(np.abs(after[:,:2]-before[:,:2]))<.0001
    assert np.max(np.abs(after[protected]-before[protected]))<.0001
    return {'vertices':len(before),'changed_vertices':int(np.count_nonzero(np.abs(after[:,2]-before[:,2])>.0001)),'max_vertical_delta':float(np.max(np.abs(after[:,2]-before[:,2]))),'xy_preserved':True,'city_pad_and_border_preserved':True,'topology_preserved':True}

def paint(terrain,plan,out):
    size=2048
    uv=np.array([list(v.uv) for v in terrain.data.uv_layers.active.data])
    pts=world_points(terrain);lo=pts.min(axis=0);hi=pts.max(axis=0)
    loopids=np.array([loop.vertex_index for loop in terrain.data.loops]);expected=(pts[loopids,:2]-lo[:2])/(hi[:2]-lo[:2])
    flip=[False,False]
    for axis in range(2):
        same=np.max(np.abs(uv[:,axis]-expected[:,axis]));opposite=np.max(np.abs(uv[:,axis]-(1-expected[:,axis])))
        if min(same,opposite)>.002:raise RuntimeError('Source UV map is not aligned; refusing guessed texture projection')
        flip[axis]=opposite<same
    original=next((n.image for m in terrain.data.materials if m and m.use_nodes for n in m.node_tree.nodes if n.type=='TEX_IMAGE' and n.image),None)
    if original is None:raise RuntimeError('Actual source colour atlas not found')
    ow,oh=original.size;arr=np.empty(ow*oh*4,dtype=np.float32);original.pixels.foreach_get(arr);arr=arr.reshape(oh,ow,4)
    ii=(np.arange(size)*ow//size).clip(0,ow-1);jj=(np.arange(size)*oh//size).clip(0,oh-1);old=arr[jj[:,None],ii[None,:],:3]
    src_srgb=np.where(old<=.0031308,old*12.92,1.055*np.maximum(old,0)**(1/2.4)-.055)
    road=smooth(.02,.09,src_srgb[:,:,0]-src_srgb[:,:,1])*smooth(.58,.76,src_srgb[:,:,2]/np.maximum(src_srgb[:,:,1],.001))
    del old,src_srgb
    xx=np.linspace(lo[0],hi[0],size,dtype=np.float32);yy=np.linspace(lo[1],hi[1],size,dtype=np.float32)
    if flip[0]:xx=xx[::-1]
    if flip[1]:yy=yy[::-1]
    X,Y=np.meshgrid(xx,yy)
    scale=plan['macro_scale']
    f=(np.sin((X+.24*Y)/scale*2.1)+.53*np.cos((Y-.38*X)/scale*3.4)+.24*np.sin((X+Y)/scale*6.7))/1.77
    patch=(f+1)*.5
    pal={k:srgb_to_linear(v) for k,v in plan['palette_srgb'].items()}
    colour=pal['meadow'][None,None,:]*(1-patch[:,:,None])+pal['meadow_light'][None,None,:]*patch[:,:,None]
    dry=smooth(.38,.88,patch)*plan['dry_amount']
    colour=colour*(1-dry[:,:,None])+pal['dry']*dry[:,:,None]
    normals=np.array([list(terrain.matrix_world.to_3x3()@v.normal) for v in terrain.data.vertices]);normals/=np.maximum(np.linalg.norm(normals,axis=1,keepdims=True),1e-9)
    slopes=np.degrees(np.arccos(np.clip(normals[:,2],-1,1)))
    gridn=121;grid=np.zeros((gridn,gridn),np.float32);counts=np.zeros_like(grid)
    gx=np.rint((pts[:,0]-lo[0])/(hi[0]-lo[0])*(gridn-1)).astype(int);gy=np.rint((pts[:,1]-lo[1])/(hi[1]-lo[1])*(gridn-1)).astype(int)
    np.add.at(grid,(gy,gx),slopes);np.add.at(counts,(gy,gx),1)
    if np.any(counts==0):raise RuntimeError('Unexpected topology in measured slope grid')
    grid/=counts
    axis=np.linspace(0,gridn-1,size);a0=axis.astype(int);a1=np.minimum(a0+1,gridn-1);t=axis-a0
    row=grid[:,a0]*(1-t)+grid[:,a1]*t
    slope=row[a0,:]*(1-t[:,None])+row[a1,:]*t[:,None]
    if flip[0]:slope=slope[:,::-1]
    if flip[1]:slope=slope[::-1,:]
    rock=smooth(plan['slope_rock_start'],plan['slope_rock_end'],slope)
    colour=colour*(1-rock[:,:,None])+pal['rock']*rock[:,:,None]
    lum=np.mean(arr[:,:,:3],axis=2);lum=lum[jj[:,None],ii[None,:]];mean=float(lum.mean());detail=np.clip(lum/max(mean,.001)-1,-.5,.5)*plan['micro_strength']
    colour*=1+detail[:,:,None]
    colour=colour*(1-road[:,:,None])+pal['path']*road[:,:,None]
    image=bpy.data.images.new('World4X authored meadow colour',width=size,height=size,alpha=True)
    rgba=np.ones((size,size,4),np.float32);rgba[:,:,:3]=np.clip(colour,0,1);image.pixels.foreach_set(rgba.ravel());image.update()
    image.filepath_raw=str(out/'ground-colour.png');image.file_format='PNG';image.save();image.pack()
    material=bpy.data.materials.new('World4X authored terrain finish');material.use_nodes=True
    nodes=material.node_tree.nodes;nodes.clear();bs=nodes.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.93;bs.inputs['Metallic'].default_value=0
    tex=nodes.new('ShaderNodeTexImage');tex.image=image;tex.interpolation='Linear';dest=nodes.new('ShaderNodeOutputMaterial')
    material.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);material.node_tree.links.new(bs.outputs['BSDF'],dest.inputs['Surface'])
    terrain.data.materials.clear();terrain.data.materials.append(material)
    return {'size':size,'linear_palette_used':{k:[float(n) for n in v] for k,v in pal.items()},'uv_alignment_verified':True,'uv_flipped_axes':flip,'measured_rock_coverage':float((rock>.3).mean()),'retained_route_mask_coverage':float((road>.3).mean()),'colour_sha256':digest(out/'ground-colour.png')}

def main():
    parser=argparse.ArgumentParser();parser.add_argument('--out',required=True,type=pathlib.Path);args=parser.parse_args(sys.argv[sys.argv.index('--')+1:])
    out=args.out.resolve();out.mkdir(parents=True,exist_ok=False)
    if digest(SOURCE)!=EXPECTED:raise RuntimeError('Recovered latest ground source hash mismatch')
    bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(SOURCE))
    terrain=bpy.data.objects.get(TARGET)
    if terrain is None or terrain.type!='MESH':raise RuntimeError('Actual retained source terrain absent')
    pts=world_points(terrain);context={'sha256':EXPECTED,'name':TARGET,'bounds':[pts.min(axis=0).tolist(),pts.max(axis=0).tolist()],'vertex_count':len(pts),'polygons':len(terrain.data.polygons),'protected_city_pad':[-40,4,-52,-8],'axes':'Blender WORLD X Y horizontal; Z up','reference_scope':'local proof sector, not full128x128 world'}
    write(out/'source-context.json',context);lighting()
    before=render(out,'before');bpy.ops.wm.save_as_mainfile(filepath=str(out/'before.blend'))
    plan=author(out,context,before);proof=apply_terrain(terrain,plan);write(out/'geometry-proof.json',proof)
    material=paint(terrain,plan,out);write(out/'material-proof.json',material)
    bpy.ops.wm.save_as_mainfile(filepath=str(out/'after.blend'))
    bpy.ops.object.select_all(action='DESELECT');terrain.select_set(True);bpy.context.view_layer.objects.active=terrain
    bpy.ops.export_scene.gltf(filepath=str(out/'ZoneOneGroundFinish.glb'),export_format='GLB',use_selection=True,export_apply=True)
    after=render(out,'after');assert [x['matrix'] for x in before]==[x['matrix'] for x in after]
    write(out/'result.json',{'state':'SOURCE_CANDIDATE_REQUIRES_INDEPENDENT_REVIEW','source_sha256':EXPECTED,'candidate_sha256':digest(out/'ZoneOneGroundFinish.glb'),'plan_sha256':digest(out/'artist-plan.json'),'genuine_author':json.loads((out/'genuine-author.json').read_text()),'geometry':proof,'material':material,'before':before,'after':after,'unity_executed':False,'art_approved':False,'paid_credits':0})
    if digest(SOURCE)!=EXPECTED:raise RuntimeError('Original source altered')
if __name__=='__main__':main()
