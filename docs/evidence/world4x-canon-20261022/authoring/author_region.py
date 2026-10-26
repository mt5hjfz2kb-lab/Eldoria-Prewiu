"""Eldoria Phase A: author an editable Blender world from reference-space guides.

No Unity mesh is read. Curves, terrain, masks, forest volumes, materials and cameras
are created in Blender. GLB and sampled query data are exports of that scene.
Run: blender -b --python author_region.py -- OUTPUT REFERENCES CAMERAS_JSON
"""
import bpy, sys, math, json, pathlib, hashlib, gzip, struct
import numpy as np
from mathutils import Vector
from mathutils.geometry import interpolate_bezier, delaunay_2d_cdt
from mathutils.bvhtree import BVHTree
from bpy_extras.object_utils import world_to_camera_view

out, refs, camera_file = map(pathlib.Path, sys.argv[sys.argv.index('--')+1:])
out.mkdir(parents=True, exist_ok=True)
if (out/'eldoria-region1-author.blend').exists():
    raise RuntimeError('Never overwrite frozen authoring evidence')
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene
scene.unit_settings.system='METRIC'; scene.unit_settings.scale_length=1
collections={}
for name in ['00_REFERENCE_AND_COMPOSITION','10_EDITABLE_LAYOUT_CURVES','20_SCULPTED_TERRAIN',
             '30_WATER_AND_PATHS','40_FOREST_MASS_BLOCKOUT','50_GEOLOGICAL_BREACH','90_LIGHTS_AND_CAMERAS']:
    c=bpy.data.collections.new(name); scene.collection.children.link(c);collections[name[:2]]=c
operations=[]
def link(obj, key):
    for c in list(obj.users_collection): c.objects.unlink(obj)
    collections[key].objects.link(obj);return obj
def record(op, objects, reason):operations.append(dict(operation=op,objects=objects,artistic_intent=reason))

# Correspondence to the image's scene area, not its header/footer aspect ratio.
# The physical Breach remains (8,61); current gameplay markers are not moved.
def point(px,py):return np.array([8+(px-253)*1.44,61+(430-py)*1.60])
def linear(c):return c/12.92 if c<=.04045 else ((c+.055)/1.055)**2.4
palette={'Meadow': '#adb04e','DryMeadow':'#b9ad59','ForestGround':'#788543',
         'Rock':'#aaa17c','Cliff':'#b3a17a','Path':'#c5b274','Water':'#318b9c',
         'Sea':'#24647b','ForestDeep':'#315b38','ForestLight':'#53713b',
         'Shale':'#72616b','BreachInterior':'#302b3d'}
mats={}
for name,value in palette.items():
    color=tuple(linear(int(value[i:i+2],16)/255) for i in (1,3,5))
    m=bpy.data.materials.new('Eldoria_'+name);m.diffuse_color=(*color,1);m.use_nodes=True
    # Explicit node creation is independent of UI language and startup defaults.
    m.node_tree.nodes.clear()
    bsdf=m.node_tree.nodes.new('ShaderNodeBsdfPrincipled')
    output=m.node_tree.nodes.new('ShaderNodeOutputMaterial')
    m.node_tree.links.new(bsdf.outputs['BSDF'],output.inputs['Surface'])
    bsdf.inputs['Base Color'].default_value=(*color,1)
    bsdf.inputs['Roughness'].default_value=.92 if name not in ['Water','Sea'] else .48
    mats[name]=m

def curve(name, coords, key='10', closed=False, radii=None):
    data=bpy.data.curves.new(name,'CURVE');data.dimensions='3D';data.resolution_u=12
    spline=data.splines.new('BEZIER');spline.bezier_points.add(len(coords)-1)
    for i,(bp,p) in enumerate(zip(spline.bezier_points,coords)):
        bp.co=(float(p[0]),float(p[1]),0);bp.handle_left_type='AUTO';bp.handle_right_type='AUTO'
        bp.radius=radii[i] if radii else 1
    spline.use_cyclic_u=closed
    obj=bpy.data.objects.new(name,data);collections[key].objects.link(obj)
    obj.hide_render=True;obj['authoring_role']='Editable layout authority; not a Unity reconstruction'
    return obj
def pixels(name, coords, **kw):return curve(name,[point(*p) for p in coords],**kw)
def sample(obj, count=10):
    points=obj.data.splines[0].bezier_points;result=[]
    segments=len(points) if obj.data.splines[0].use_cyclic_u else len(points)-1
    for i in range(segments):
        a=points[i];b=points[(i+1)%len(points)]
        samples=interpolate_bezier(a.co,a.handle_right,b.handle_left,b.co,count+1)
        result.extend((p.x,p.y) for p in samples[:-1])
    if not obj.data.splines[0].use_cyclic_u:result.append((points[-1].co.x,points[-1].co.y))
    return np.array(result,dtype=np.float32)

coast_points=[(-910,-1000),(-910,1000),(90,1000)]
coast_pixels=[(318,142),(335,163),(319,179),(359,193),(380,216),(414,225),
 (430,244),(457,250),(468,273),(452,286),(470,311),(460,334),(476,352),
 (468,370),(482,392),(461,410),(476,435),(466,459),(483,478),(462,497),
 (469,522),(449,540),(465,559),(450,580),(462,604),(438,626),(423,650),
 (397,666),(375,686),(344,697),(323,717),(298,719),(272,739),(243,735),
 (214,749),(186,732),(159,725),(139,703),(106,695),(78,672),(45,670)]
coast_points.extend(point(*p) for p in coast_pixels)
coast=curve('Guide · continental east and south coast',coast_points,closed=True)
for bp in coast.data.splines[0].bezier_points:
    if bp.co.x< -800 or bp.co.y>800:bp.handle_left_type='VECTOR';bp.handle_right_type='VECTOR'
river_pixels=[
 [(280,145),(267,173),(284,193),(268,218),(277,236),(257,260),(250,292),(230,321),(239,348),(220,373),(220,404),(205,440),(193,468),(203,496),(185,521),(187,544),(171,566),(184,591),(206,608),(232,627),(258,644),(286,661),(310,682)],
 [(99,184),(114,209),(98,235),(117,259),(124,285),(148,311),(151,338),(179,363),(193,390),(220,404)],
 [(33,284),(60,293),(75,310),(101,312),(125,333),(151,338)],
 [(381,194),(361,220),(356,246),(324,261),(304,284),(270,286),(250,292)],
 [(423,305),(390,314),(362,336),(333,347),(307,370),(283,373)],
 [(449,450),(417,468),(396,490),(362,508),(337,534),(304,549),(281,570),(253,571),(224,592),(206,608)],
 [(193,468),(221,482),(239,506),(270,516),(304,549)],
 [(304,549),(328,570),(353,584),(377,609),(399,624),(423,650)],
 [(53,477),(79,495),(101,499),(122,518),(142,541),(171,566)]]
rivers=[pixels('River %02d · editable basin'%i,p) for i,p in enumerate(river_pixels)]
# Preserve the actual city datum and join its near brook to the same trunk.
brook=curve('River 09 · genuine city foreground',[(117,-23),(83,-19),(47,-29),(18,-20),(-8,-28),(-33,-17),(-58,-31),(-88,-28),(-100,-12)])
trunk=sample(rivers[0]);merge=trunk[np.argmin(np.linalg.norm(trunk-[-70,-20],axis=1))];brook.data.splines[0].bezier_points[-1].co=(*merge,0)
rivers.append(brook)
road_pixels=[
 [(299,157),(306,202),(295,244),(309,285),(296,326),(310,362),(293,399),(280,440),(300,480),(283,521),(296,564),(283,602),(310,644),(302,702)],
 [(142,187),(160,229),(149,274),(165,312),(153,355),(171,394),(157,435),(176,479),(158,520),(178,563),(158,608),(182,660),(173,708)],
 [(55,248),(107,239),(151,254),(198,242),(245,258),(295,244),(342,267),(391,254),(447,271)],
 [(38,365),(83,348),(125,369),(171,354),(212,375),(260,361),(310,362),(354,382),(408,364),(467,389)],
 [(35,491),(86,474),(131,492),(176,479),(225,496),(265,478),(300,480),(344,461),(389,480),(449,465)],
 [(62,609),(112,596),(158,608),(204,592),(247,612),(283,602),(331,624),(377,609),(438,626)],
 [(110,694),(152,678),(199,697),(239,681),(285,695),(323,717)]]
roads=[pixels('Path %02d · terrain-following route'%i,p) for i,p in enumerate(road_pixels)]
roads.append(curve('Path 07 · retained city access',[(0,20),(0,0),(-8,-28),(-25,-68)]))
ridge_specs=[([(103,174),(90,187),(69,201)],[8,25,2],24),
 ([(385,202),(401,218),(412,232)],[3,28,3],24),
 ([(443,366),(430,383),(413,397)],[3,29,2],25),
 ([(52,417),(67,432),(91,438)],[2,22,3],25),
 ([(127,632),(113,653),(92,670)],[4,31,3],29),
 ([(180,693),(202,705),(214,724)],[2,24,2],22)]
ridges=[]
for i,(pts,heights,width) in enumerate(ridge_specs):
    o=pixels('Ridge %02d · sculpt shoulder'%i,pts)
    o['crest_heights']=heights;o['influence_width']=width;ridges.append(o)

# Major groups traced from the original: unequal footprints and deliberate clear corridors.
groves=[(199,172,24,10),(267,149,19,8),(247,190,22,12),(290,217,24,13),
 (131,234,29,14),(82,249,24,13),(375,235,25,10),(357,277,23,11),
 (76,299,23,14),(144,296,26,13),(195,321,25,12),(400,316,28,13),
 (351,341,26,12),(74,367,28,17),(122,400,25,15),(340,416,29,13),
 (403,449,30,13),(91,454,31,16),(72,514,31,16),(143,543,24,13),
 (282,492,20,11),(380,536,31,15),(315,592,28,13),(222,582,26,14),
 (172,625,26,14),(255,656,23,12),(361,642,25,12),(272,704,29,13),
 (126,677,26,15),(60,598,27,15)]
forest_curves=[]
for i,(x,y,rx,ry) in enumerate(groves):
    controls=[point(x+rx*ax,y+ry*ay) for ax,ay in [(-1,0),(-.65,-.8),(.25,-1),(.9,-.3),(1,.4),(.35,.9),(-.5,.7)]]
    o=curve('Forest %02d · designed canopy footprint'%i,controls,closed=True);forest_curves.append(o)
fault_specs=[[(0,0),(-12,8),(-18,25),(-36,32),(-44,49)],
 [(0,0),(11,13),(12,28),(28,39),(33,54)],
 [(0,0),(16,-4),(29,-18),(49,-14),(64,-28)],
 [(0,0),(-15,-7),(-27,-25),(-45,-29),(-62,-45)],
 [(0,0),(2,-16),(-8,-34),(-3,-51)],
 [(-18,25),(-31,19),(-47,24),(-60,17)],
 [(12,28),(29,22),(43,30),(58,26)]]
faults=[curve('Fault %02d · cut into terrain'%i,[(8+x,61+y) for x,y in p]) for i,p in enumerate(fault_specs)]
record('Create editable Bezier composition', [o.name for o in [coast]+rivers+roads+ridges+forest_curves+faults],
       'Trace the approved panorama in one calibrated image-to-ground coordinate system; preserve live target anchors.')

# Original images remain support objects only; packed and hidden from render/export.
reference_hashes={}
for i,name in enumerate(['01_vista_cercana.png','02_vista_regional.png','03_vista_panoramica.png']):
    path=refs/name;image=bpy.data.images.load(str(path.resolve()));image.pack()
    obj=bpy.data.objects.new('ORIGINAL · '+name,None);collections['00'].objects.link(obj)
    obj.empty_display_type='IMAGE';obj.data=image;obj.empty_display_size=600;obj.location=(-1500-i*650,0,250);obj.hide_render=True
    reference_hashes[name]=hashlib.sha256(path.read_bytes()).hexdigest()
guide=bpy.data.objects.new('Calibration · Breach pixel253,430 maps to X8 Z61',None);collections['00'].objects.link(guide)
guide.location=(8,61,-4);guide.empty_display_type='CIRCLE';guide.empty_display_size=48;guide.hide_render=True
guide['pixel_to_world']='X=8+(px-253)*1.44 ; Z=61+(430-py)*1.60. Framing evaluated with actual game cameras.'

def distance(points, path):
    result=np.full(len(points),1e9,dtype=np.float32)
    for a,b in zip(path[:-1],path[1:]):
        d=b-a; t=np.clip(((points-a)*d).sum(axis=1)/max(float(d@d),1e-8),0,1)
        result=np.minimum(result,np.linalg.norm(points-(a+t[:,None]*d),axis=1))
    return result
def polygon_field(points, path):
    path=np.concatenate([path,path[:1]]);inside=np.zeros(len(points),dtype=bool)
    for a,b in zip(path[:-1],path[1:]):
        if abs(float(b[1]-a[1]))<1e-8:continue
        inside^=((a[1]>points[:,1])!=(b[1]>points[:,1])) & (points[:,0]<(b[0]-a[0])*(points[:,1]-a[1])/(b[1]-a[1])+a[0])
    return distance(points,path)*np.where(inside,1,-1)
# Second macroscopic construction: feature-conforming triangulation, not a coarse height grid.
NX,NZ=240,260;WIDTH,DEPTH=1820.,2000.
river_samples=[sample(c,8) for c in rivers]
road_samples=[sample(c,8) for c in roads]
fault_samples=[sample(c,8) for c in faults]
coast_path=sample(coast,8)
constraints=[];layout_points=[]
def constrain(path,closed=False):
    start=len(layout_points);layout_points.extend((float(p[0]),float(p[1])) for p in path)
    constraints.extend((start+i,start+i+1) for i in range(len(path)-1))
    if closed:constraints.append((start+len(path)-1,start))
def offset(path,width,closed=False):
    d=np.roll(path,-1,axis=0)-np.roll(path,1,axis=0)
    if not closed:d[0]=path[1]-path[0];d[-1]=path[-1]-path[-2]
    d/=np.maximum(np.linalg.norm(d,axis=1)[:,None],.001)
    return path+np.column_stack([-d[:,1],d[:,0]])*np.asarray(width).reshape(-1,1)
constrain(coast_path,True)
for delta in [-4,4,9]:constrain(offset(coast_path,np.full(len(coast_path),delta),True),True)
for i,path in enumerate(river_samples):
    widths=np.linspace(1.4,4.1,len(path)) if i==0 else np.linspace(.6,2.3,len(path))
    constrain(path)
    for side in [-1,1]:
        constrain(offset(path,side*widths));constrain(offset(path,side*(widths+3)))
for path in road_samples:
    constrain(path)
    for side in [-1,1]:constrain(offset(path,np.full(len(path),side*2)))
for path in fault_samples:
    constrain(path)
    for side in [-1,1]:
        constrain(offset(path,np.full(len(path),side*1.5)));constrain(offset(path,np.full(len(path),side*5)))
for c in forest_curves:constrain(sample(c,4),True)
for x,y,rx,ry in [(224,265,22,12),(278,571,26,16)]:
    centre=point(x,y)
    for radius in [1,1.16]:
        ring=np.array([(centre[0]+math.cos(j*math.tau/40)*rx*1.44*radius,centre[1]+math.sin(j*math.tau/40)*ry*1.6*radius) for j in range(40)])
        constrain(ring,True)
# Coarse open meadow sampling; density is spent at shores, channels and faults.
for z in np.linspace(-1000,1000,101):
    for x in np.linspace(-910,910,92):layout_points.append((float(x),float(z)))
coords,_,faces,_,_,_=delaunay_2d_cdt([Vector(p) for p in layout_points],constraints,[],0,.005,False)
points=np.array(coords,dtype=np.float32)
shore=polygon_field(points,coast_path)
def fields(points):
    river_edge=np.full(len(points),1000,dtype=np.float32)
    for i,path in enumerate(river_samples):
        widths=np.linspace(1.4,4.1,len(path)) if i==0 else np.linspace(.6,2.3,len(path))
        for j,(a,b) in enumerate(zip(path[:-1],path[1:])):river_edge=np.minimum(river_edge,distance(points,np.array([a,b]))-widths[j])
    for x,y,rx,ry in [(224,265,22,12),(278,571,26,16)]:
        centre=point(x,y);q=(points-centre)/[rx*1.44,ry*1.6]
        river_edge=np.minimum(river_edge,(np.linalg.norm(q,axis=1)-1)*ry*1.6)
    road_distance=np.minimum.reduce([distance(points,path) for path in road_samples])
    forest_distance=np.maximum.reduce([polygon_field(points,sample(c,5)) for c in forest_curves])
    fault_distance=np.minimum.reduce([distance(points,path) for path in fault_samples])
    return river_edge,road_distance,forest_distance,fault_distance
river_edge,road_distance,forest_distance,fault_distance=fields(points)
heights=np.full(len(points),-4,dtype=np.float32)
for ridge in ridges:
    path=sample(ridge);dist=np.full(len(points),10000,dtype=np.float32);crest=np.zeros(len(points),dtype=np.float32)
    profile=np.interp(np.linspace(0,1,len(path)),np.linspace(0,1,len(ridge['crest_heights'])),ridge['crest_heights'])
    for j,(a,b) in enumerate(zip(path[:-1],path[1:])):
        q=distance(points,np.array([a,b]));near=q<dist;dist=np.minimum(dist,q);crest[near]=profile[j]
    heights+=np.clip(1-dist/float(ridge['influence_width']),0,1)**1.5*crest
heights[np.linalg.norm(points,axis=1)<100]=-4
heights-=np.clip(1-np.maximum(river_edge,0)/3,0,1)*2.7
# Fracture branches determine depth and colour. No circular dark disk.
scar=np.clip(1-fault_distance/9,0,1)*np.clip(1-np.linalg.norm(points-[8,61],axis=1)/100,0,1)
heights-=scar*2+np.clip(1-fault_distance/1.5,0,1)*np.clip(1-np.linalg.norm(points-[8,61],axis=1)/100,0,1)*5
heights=np.where(shore<0,-18,np.where(shore<4,-18+(heights+18)*shore/4,heights))
vertices=[(float(p[0]),float(p[1]),float(h)) for p,h in zip(points,heights)]
mesh=bpy.data.meshes.new('Editable feature-conforming terrain');mesh.from_pydata(vertices,[],faces);mesh.update()
terrain=bpy.data.objects.new('BLD_Terrain',mesh);collections['20'].objects.link(terrain)
terrain['source_authority']='Blender authored curves constrain actual terrain edges; no Unity mesh input.'
terrain['construction']='Constrained Delaunay: coast, channel banks, routes and physical fissures are explicit mesh edges.'
# Exportable vertex colour transitions replace block-shaped face material boundaries.
mat=bpy.data.materials.new('Eldoria_AuthoredTerrain');mat.use_nodes=True;mat.node_tree.nodes.clear()
bs=mat.node_tree.nodes.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.94
vc=mat.node_tree.nodes.new('ShaderNodeVertexColor');vc.layer_name='Color'
output=mat.node_tree.nodes.new('ShaderNodeOutputMaterial');mat.node_tree.links.new(vc.outputs['Color'],bs.inputs['Base Color']);mat.node_tree.links.new(bs.outputs['BSDF'],output.inputs['Surface']);mesh.materials.append(mat)
def colour(name):return np.array(mats[name].diffuse_color[:3])
colors=np.tile(colour('Meadow'),(len(points),1))
for name,weight in [('ForestGround',np.clip((forest_distance+6)/12,0,1)),('DryMeadow',np.clip(1-np.maximum(river_edge,0)/9,0,1)*.6),('Rock',np.clip((heights+1)/13,0,1)),('Cliff',np.clip(1-np.maximum(shore,0)/8,0,1)),('Sea',(shore<0).astype(float)),('Shale',scar),('BreachInterior',np.clip(1-fault_distance/2,0,1)*scar)]:
    colors=colors*(1-weight[:,None])+colour(name)*weight[:,None]
attr=mesh.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='POINT');attr.data.foreach_set('color',np.column_stack([colors,np.ones(len(points))]).astype(np.float32).ravel())
uv=mesh.uv_layers.new(name='WorldUV')
for poly in mesh.polygons:
    poly.use_smooth=True
    for l in poly.loop_indices:
        i=mesh.loops[l].vertex_index;uv.data[l].uv=(points[i,0]/WIDTH+.5,points[i,1]/DEPTH+.5)
for name,values in [('forest_density',np.clip(forest_distance/10,0,1)),('river_mask',(river_edge<0).astype(float)),('path_mask',(road_distance<2).astype(float)),('land_mask',(shore>0).astype(float)),('breach_damage',scar)]:
    attr=mesh.attributes.new(name,'FLOAT','POINT');attr.data.foreach_set('value',values.astype(np.float32))
bvh=BVHTree.FromPolygons([Vector(v) for v in vertices],faces,all_triangles=True)
def height(x,y):
    # Rays exactly on the outer hull can miss through floating-point edge tests.
    # Move only outer-boundary queries inward by one millimetre; interior misses still fail.
    x=np.clip(x,-WIDTH/2+.001,WIDTH/2-.001);y=np.clip(y,-DEPTH/2+.001,DEPTH/2-.001)
    hit=bvh.ray_cast(Vector((float(x),float(y),200)),Vector((0,0,-1)))
    if hit[0] is None:raise RuntimeError('Authored terrain sample outside mesh: '+str((x,y)))
    return float(hit[0].z)
record('Conform terrain topology to editable geographic curves',[terrain.name],
       'Replace coarse square cells with explicit shore, riverbank and fault edges. Interpolate biome colours at vertices; physically incise branching fault instead of circular pigment.')

exports=[terrain]
def mesh_object(name,verts,faces,material,key):
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.materials.append(mats[material]);mesh.update()
    o=bpy.data.objects.new(name,mesh);collections[key].objects.link(o);exports.append(o);return o
def ribbon(name,path,widths,material,water=False):
    v=[];f=[]
    for i,p in enumerate(path):
        d=path[min(i+1,len(path)-1)]-path[max(i-1,0)];d=d/max(np.linalg.norm(d),.001);n=np.array([-d[1],d[0]])
        for side in [-1,1]:
            q=p+n*widths[i]*side;v.append((float(q[0]),float(q[1]),(-4.9 + .002*int(name.rsplit("_",1)[-1])) if water else height(*q)+.10))
        if i:f.append((2*i-2,2*i,2*i+1,2*i-1))
    return mesh_object(name,v,f,material,'30')
for i,path in enumerate(river_samples):
    widths=np.linspace(1.4,4.1,len(path)) if i==0 else np.linspace(.6,2.3,len(path))
    ribbon('BLD_River_%02d'%i,path,widths,'Water',True)
for i,c in enumerate(roads):
    p=sample(c);ribbon('BLD_Path_%02d'%i,p,np.full(len(p),1.7),'Path')
for i,(x,y,rx,ry) in enumerate([(224,265,22,12),(278,571,26,16)]):
    c=point(x,y);v=[(*c,-4.9)];n=22
    for j in range(n):
        a=j*math.tau/n;radius=1;v.append((c[0]+math.cos(a)*rx*1.44*radius,c[1]+math.sin(a)*ry*1.6*radius,-4.9))
    mesh_object('BLD_Lake_%02d'%i,v,[(0,j+1,(j+1)%n+1) for j in range(n)],'Water','30')
record('Model river surfaces, lake basins and terrain-following paths',[o.name for o in exports if o!=terrain],
       'Water and routes derive from the same editable layout curves used to carve terrain.')

# Phase A ONLY: editable low-frequency canopy masses, deliberately no individual trees or scatter detail.
for i,c in enumerate(forest_curves):
    boundary=sample(c,3);centre=boundary.mean(axis=0);v=[]
    for p in boundary:v.append((*p,height(*p)+.1))
    n=len(boundary)
    for j,p in enumerate(boundary):
        q=centre+(p-centre)*.62;v.append((*q,height(*q)+9+(j%3)*2))
    v.append((*centre,height(*centre)+17));f=[]
    for j in range(n):f.extend([(j,(j+1)%n,(j+1)%n+n,j+n),(j+n,(j+1)%n+n,2*n)])
    o=mesh_object('BLD_ForestMass_%02d'%i,v,f,'ForestDeep' if i%3 else 'ForestLight','40')
    o['phase']='COMPOSITION ONLY — replace with designed tree families only after macroscopic acceptance'
record('Model deliberately grouped canopy volumes',[o.name for o in exports if 'ForestMass' in o.name],
       'Test silhouette, negative space and geographic grouping before any individual-tree detail.')

# Breach floor remains below the carved terrain, physically recessed, without a glowing billboard.
c=np.array([8,61]);v=[(*c,height(*c)+.15)]
outline=[(-9,1),(-4,7),(3,6),(8,2),(6,-5),(-2,-8),(-7,-4)]
for dx,dy in outline:v.append((8+dx,61+dy,height(8+dx,61+dy)+.15))
mesh_object('BLD_BreachFloor',v,[(0,j+1,(j+1)%len(outline)+1) for j in range(len(outline))],'BreachInterior','50')

sun_data=bpy.data.lights.new('Key · readable morning light','SUN');sun_data.energy=2.2;sun_data.angle=.15
sun=bpy.data.objects.new(sun_data.name,sun_data);collections['90'].objects.link(sun);sun.rotation_euler=(math.radians(28),math.radians(-24),math.radians(-25))
scene.world=bpy.data.worlds.new('Eldoria_AuthoringSky');scene.world.use_nodes=True
scene.world.node_tree.nodes.clear()
background=scene.world.node_tree.nodes.new('ShaderNodeBackground')
world_output=scene.world.node_tree.nodes.new('ShaderNodeOutputWorld')
scene.world.node_tree.links.new(background.outputs[0],world_output.inputs['Surface'])
background.inputs[0].default_value=(.24,.30,.36,1);background.inputs[1].default_value=.65
scene.render.engine='CYCLES';scene.cycles.samples=12;scene.cycles.use_denoising=True
scene.view_settings.view_transform='Standard';scene.render.image_settings.file_format='PNG'
cameras=json.loads(camera_file.read_text());camera_records={}
for label,m in cameras.items():
    data=bpy.data.cameras.new(label);o=bpy.data.objects.new(label,data);collections['90'].objects.link(o)
    cp=m['cameraPosition'];home=m['worldTarget'];o.location=(cp['x']-1018,cp['z']-30,cp['y'])
    target=Vector((home['x']-1018,home['z']-30,home['y']));o.rotation_euler=(target-o.location).to_track_quat('-Z','Y').to_euler()
    data.type='ORTHO';data.clip_end=3000;data.ortho_scale=1;scene.camera=o
    scene.render.resolution_x=m['width'];scene.render.resolution_y=m['height'];scene.render.resolution_percentage=100
    bpy.context.view_layer.update()
    a=world_to_camera_view(scene,o,target);b=world_to_camera_view(scene,o,target+Vector((1,0,0)))
    data.ortho_scale=abs(b.x-a.x)*2*m['zoom']*m['aspect'];bpy.context.view_layer.update()
    camera_records[label]={'unity_camera':m,'blender_location':list(o.location),'blender_rotation_euler':list(o.rotation_euler),'ortho_scale':data.ortho_scale,'match':'Width calibrated by actual projected unit segment; no conceptual pixel-equivalence claim'}
    scene.render.filepath=str((out/('blender-blockout-'+label+'.png')).resolve());bpy.ops.render.render(write_still=True)
scene.camera=bpy.data.objects['panoramica-horizontal'];scene.render.resolution_x=1280;scene.render.resolution_y=720
recipe=bpy.data.texts.new('AUTHORING_RECIPE.py');recipe.write(pathlib.Path(__file__).read_text())
bpy.ops.wm.save_as_mainfile(filepath=str((out/'eldoria-region1-author.blend').resolve()))
bpy.ops.object.select_all(action='DESELECT')
for o in exports:o.select_set(True)
bpy.context.view_layer.objects.active=terrain
bpy.ops.export_scene.gltf(filepath=str((out/'RegionOneBlenderWorld.glb').resolve()),export_format='GLB',use_selection=True,export_yup=True,export_apply=True,export_materials='EXPORT',export_cameras=False,export_lights=False)
# Query data is sampled from the actual authored Blender terrain, never rebuilt in Unity.
query_x,query_z=np.meshgrid(np.linspace(-WIDTH/2,WIDTH/2,NX+1),np.linspace(-DEPTH/2,DEPTH/2,NZ+1))
query_points=np.column_stack([query_x.ravel(),query_z.ravel()]).astype(np.float32)
query_heights=np.array([height(*p) for p in query_points],dtype='<f4')
query_shore=polygon_field(query_points,coast_path)
query_river,query_road,query_forest,_=fields(query_points)
flags=np.column_stack([np.clip(.5+query_shore/32,0,1),np.clip(1-query_river/3,0,1),np.clip(1-query_road/3,0,1),np.clip(query_forest/10,0,1)])
raw=struct.pack('<iiiff',0x31424c45,NX+1,NZ+1,WIDTH,DEPTH)+query_heights.tobytes()+np.round(flags*255).astype('uint8').tobytes()
# Preserve exact source positions for import validation; grid heights are only gameplay queries.
raw+=struct.pack('<ii',0x32564c45,len(vertices))+np.array([(v[0],v[2],v[1]) for v in vertices],dtype='<f4').tobytes()
(out/'RegionOneBlenderQueries.bytes').write_bytes(gzip.compress(raw,mtime=0))
(out/'blender-cameras.json').write_text(json.dumps(camera_records,indent=2))
manifest={'phase':'A_COMPOSITION_NOT_ART_APPROVED','blender_version':bpy.app.version_string,'source':'Blank Blender scene; no terrain imported from Unity','artistic_operations':len(operations),'operations':operations,'reference_sha256':reference_hashes,'units':'1 Blender metre = 1 Unity metre; GLB Y-up conversion; X unchanged, Blender Y becomes Unity Z','breach_anchor_unity':[8,61],'objects':len(exports),'terrain_vertices':len(mesh.vertices),'source_assets':{},'camera_records':'blender-cameras.json'}
for name in ['eldoria-region1-author.blend','RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes']:
    manifest['source_assets'][name]=hashlib.sha256((out/name).read_bytes()).hexdigest()
(out/'blender-authoring-manifest.json').write_text(json.dumps(manifest,indent=2))
print('ELDORIA_BLENDER_AUTHOR_PASS',json.dumps(manifest))
