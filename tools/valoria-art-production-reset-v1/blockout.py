"""Isolated preproduction 3D mesh + deterministic CPU rasterizer. No runtime imports.
Run from repository root: python tools/valoria-art-production-reset-v1/blockout.py
Coordinates are X right, Y depth, Z up. Unity transfer: (X,Z,Y).
No reference pixels are used in rendering. Screen sketches only set world anchors.
"""
import json, math, hashlib
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw, ImageFont
from scipy.spatial import Delaunay

OUT = Path('docs/evidence/valoria-art-production-reset-v1')
OUT.mkdir(parents=True, exist_ok=True)
W,H = 1536,1024
PITCH,YAW = 35.,20.
SCALE = H/48.
RIGHT = np.array([math.cos(math.radians(YAW)), math.sin(math.radians(YAW)),0])
UP = np.array([-math.sin(math.radians(YAW))*math.sin(math.radians(PITCH)),math.cos(math.radians(YAW))*math.sin(math.radians(PITCH)),math.cos(math.radians(PITCH))])
VIEW = np.cross(RIGHT, UP)
MESHES=[]
COLORS={'ground':[151,151,139],'rock':[102,105,110],'stone':[189,187,180], 'road':[175,173,164],'wood':[135,126,118],'roof':[117,113,111],'tent':[138,143,153],'tree':[100,113,104],'water':[113,128,141],'background':[127,137,146]}

def anchor(px,py,z):
    a=(px-W/2)/SCALE
    b=(H/2-py)/SCALE-UP[2]*z
    xy=np.linalg.solve(np.stack([RIGHT[:2],UP[:2]]),[a,b])
    return [float(xy[0]),float(xy[1]),float(z)]

def mesh(name,verts,faces,family):
    MESHES.append({'name':name,'family':family,'vertices':np.asarray(verts).tolist(),'triangles':[list(f) for f in faces],'color':COLORS[family]})

def prism(name,poly,z0,z1,family='ground'):
    n=len(poly); verts=[[x,y,z0] for x,y in poly]+[[x,y,z1] for x,y in poly]
    # Centroid fans only for convex sketches; concave terrain uses triangulated top.
    faces=[]
    tri=Delaunay(np.asarray(poly)).simplices
    from matplotlib.path import Path as Polygon
    inside=Polygon(poly).contains_points(np.mean(np.asarray(poly)[tri],axis=1),radius=1e-7)
    for t in tri[inside]:faces.append(tuple(int(x+n) for x in t))
    for i in range(n):
        j=(i+1)%n;faces.extend([(i,j,j+n),(i,j+n,i+n)])
    mesh(name,verts,faces,family)

def screen_prism(name,points,z0,z1,family):
    prism(name,[anchor(x,y,z1)[:2] for x,y in points],z0,z1,family)

def box(name,center,size,family='stone'):
    x,y,z=center; a,b,c=np.array(size)/2
    v=[[x-a,y-b,z-c],[x+a,y-b,z-c],[x+a,y+b,z-c],[x-a,y+b,z-c],[x-a,y-b,z+c],[x+a,y-b,z+c],[x+a,y+b,z+c],[x-a,y+b,z+c]]
    f=[(0,2,1),(0,3,2),(4,5,6),(4,6,7),(0,1,5),(0,5,4),(1,2,6),(1,6,5),(2,3,7),(2,7,6),(3,0,4),(3,4,7)]
    mesh(name,v,f,family)

def atbox(name,px,py,z,size,family='stone'):
    box(name,np.array(anchor(px,py,z))+[0,0,size[2]/2],size,family)

def roof(name,center,size,family='roof'):
    x,y,z=center;a,b,c=np.array(size)/2
    v=[[x-a,y-b,z-c],[x+a,y-b,z-c],[x+a,y+b,z-c],[x-a,y+b,z-c],[x,y-b,z+c],[x,y+b,z+c]]
    mesh(name,v,[(0,1,4),(2,3,5),(0,4,5),(0,5,3),(1,2,5),(1,5,4),(0,3,2),(0,2,1)],family)

def wall(name,p0,p1,z,height,thickness=.7,family='stone'):
    a=np.array(anchor(*p0,z));b=np.array(anchor(*p1,z));d=b[:2]-a[:2]
    n=np.array([-d[1],d[0]])/np.linalg.norm(d)*thickness/2
    prism(name,[a[:2]-n,b[:2]-n,b[:2]+n,a[:2]+n],z,z+height,family)

def tree(name,px,py,z,height=8,radius=2):
    c=np.array(anchor(px,py,z)); verts=[c.tolist()]
    for i in range(8):
        t=i*math.tau/8;verts.append((c+np.array([radius*math.cos(t),radius*math.sin(t),height*.2])).tolist())
    verts.append((c+[0,0,height]).tolist())
    faces=[]
    for i in range(8):faces.append((1+i,1+(i+1)%8,9))
    mesh(name,verts,faces,'tree')

def build():
    # Large flat water plane and low-cost world context; no textures or backdrop image.
    box('Water', [0,0,-.35],[240,240,.3],'water')
    screen_prism('BackgroundTerrain',[(-300,270),(100,240),(650,40),(1900,50),(1900,400),(1400,355),(600,260),(0,510)],-1,0,'background')
    screen_prism('ForegroundBank',[(-300,700),(120,785),(390,830),(690,945),(1040,1120),(-300,1250)],-.7,3,'ground')
    screen_prism('ForegroundRoad',[(306,956),(492,1008),(632,870),(435,815)],3.01,3.04,'road')
    platform=[(82,547),(116,482),(190,401),(240,358),(363,334),(485,301),(590,305),(686,328),(820,345),(1050,326),(1208,338),(1390,375),(1486,409),(1530,493),(1498,593),(1471,686),(1380,757),(1220,788),(1050,788),(890,735),(789,694),(675,681),(556,671),(405,647),(299,622),(186,586)]
    screen_prism('MainPlatform',platform,0,7,'rock')
    screen_prism('PlayableGround',platform,6.95,7.03,'ground')
    upper=[(426,195),(484,171),(540,153),(656,135),(790,135),(955,181),(1140,226),(1320,254),(1400,281),(1418,302),(1365,351),(1195,342),(1040,313),(932,308),(832,290),(710,281),(580,246),(476,228)]
    screen_prism('UpperTerraceCliff',upper,7.03,11,'rock')
    screen_prism('UpperTerraceGround',upper,10.98,11.04,'ground')
    # Access spine and bridge are separate volumes with fully open gate passage.
    screen_prism('MainRoad',[(625,647),(724,668),(895,370),(832,358)],7.031,7.08,'road')
    screen_prism('UpperRoad',[(827,297),(940,316),(1000,245),(896,221)],11.041,11.09,'road')
    a=np.array(anchor(847,366,7.04));b=np.array(anchor(884,296,11.04))
    count=12
    for i in range(count):
        t=(i+.5)/count;c=a*(1-t)+b*t
        # Twelve simple steps, full width clear between provisional side cheeks.
        box('Stair_%02d'%i,c-[0,0,.12],[6.7, np.linalg.norm((b-a)[:2])/count+.03,.26],'stone')
    for side,sign in [('Left',-1),('Right',1)]:
        p=a+np.array([sign*3.65,0,0]);q=b+np.array([sign*3.65,0,0]);off=np.array([.25,0,0])
        vv=[p-off,p+off,q+off,q-off,p-off+[0,0,1.1],p+off+[0,0,1.1],q+off+[0,0,1.1],q-off+[0,0,1.1]]
        mesh('StairCheek'+side,vv,[(0,1,5),(0,5,4),(1,2,6),(1,6,5),(2,3,7),(2,7,6),(3,0,4),(3,4,7),(4,5,6),(4,6,7)],'stone')
    # Bridge top is a sloping quad from foreground bank to main platform.
    bp=[anchor(427,820,3.2),anchor(630,867,3.2),anchor(751,728,7.03),anchor(590,697,7.03)]
    verts=bp+[(np.array(v)-[0,0,.7]).tolist() for v in bp]
    mesh('BridgeDeck',verts,[(0,1,2),(0,2,3),(0,4,5),(0,5,1),(1,5,6),(1,6,2),(2,6,7),(2,7,3),(3,7,4),(3,4,0)],'stone')
    for side,ids in [('L',(0,3)),('R',(1,2))]:
        p,q=[np.array(bp[k]) for k in ids];d=q-p;off=np.cross(d,[0,0,1]);off=off/np.linalg.norm(off)*.24
        vv=[p-off,p+off,q+off,q-off,p-off+[0,0,1.3],p+off+[0,0,1.3],q+off+[0,0,1.3],q-off+[0,0,1.3]]
        mesh('BridgeParapet'+side,vv,[(0,1,5),(0,5,4),(1,2,6),(1,6,5),(2,3,7),(2,7,6),(3,0,4),(3,4,7),(4,5,6),(4,6,7)],'stone')
    support=(np.array(bp[1])+np.array(bp[2]))/2
    support[2]=2.3
    box('BridgeSupport',support,[2,2,4.6],'rock')
    # Lower gate: actual void between towers. No arch ornament in greybox.
    atbox('LowerGate_WestTower',611,691,7.1,[3.3,3.2,8.3])
    atbox('LowerGate_EastTower',778,722,7.1,[3.7,3.2,8.3])
    p=anchor(694,704,7.1);box('LowerGate_Lintel',np.array(p)+[0,0,6.7],[8.2,2.5,2.4])
    wall('LowerGate_WestWing',(567,654),(442,603),7.06,3.0)
    wall('LowerGate_EastWing',(815,693),(971,728),7.06,3.0)
    wall('WestPartialWall',(354,545),(471,576),7.06,3.6)
    atbox('WestWallTowerA',351,547,7.06,[1.8,1.8,5.2])
    atbox('WestWallTowerB',471,578,7.06,[1.6,1.6,4.8])
    wall('EastPartialWall',(1261,642),(1436,568),7.06,3.4)
    atbox('EastWallTowerA',1261,642,7.06,[1.7,1.7,4.7])
    atbox('EastWallTowerB',1435,567,7.06,[2,2,5.6])
    # Upper Bastion horizontal curtain + one dominant keep + wings, not old asset.
    wall('BastionCurtainWest',(639,211),(877,243),11.1,4.6,1.0)
    wall('BastionCurtainEast',(1012,239),(1237,267),11.1,4.6,1.0)
    atbox('BastionWestTower',631,216,11.1,[1.9,2.8,4.2])
    atbox('BastionEastTower',1235,265,11.1,[2.4,3.4,6.8])
    atbox('BastionKeep',896,226,11.1,[3.9,4.7,7.6])
    atbox('BastionLeftWing',802,207,11.1,[5.2,4.0,5.3])
    atbox('BastionRightGatePillar',991,238,11.1,[1.3,3.0,5.4])
    box('BastionGateLintel',np.array(anchor(953,242,11.1))+[0,0,4.5],[4.4,2.9,1.8])
    atbox('BastionFarWestBlock',723,193,11.1,[2.8,3.4,6.0])
    # One cabin and a two-tent camp; parcels deliberately mostly empty.
    atbox('Cabin',453,379,7.1,[4.4,2.6,2.1],'wood')
    roof('CabinRoof',np.array(anchor(453,379,7.1))+[0,0,2.6],[5.3,3.3,1.0])
    atbox('CabinChimney',439,328,10.1,[.55,.55,1.3],'stone')
    for i,(x,y) in enumerate([(1221,456),(1302,472)]):
        roof('CampTent_%d'%i,np.array(anchor(x,y,7.1))+[0,0,1.35],[3.6 if i else 2.6,3.7 if i else 2.8,2.7],'tent')
    wall('CabinFence',(350,406),(568,426),7.1,.6,.13,'wood')
    wall('CampFence',(1121,458),(1361,501),7.1,.6,.13,'wood')
    # Sparse large silhouette masses at borders. Seeded placements, no scatter noise.
    for i,(x,y,z,h,r) in enumerate([(469,204,11,7,1.6),(505,180,11,8,1.7),(606,143,11,11,2.0),(681,145,11,12,2.2),(748,139,11,9,1.8),(789,151,11,10,2),(1157,236,11,11,2),(1248,210,11,9,1.9),(1330,307,11,12,2),(1380,366,7,9,1.8),(1470,421,7,8,1.8),(170,541,7,9,2),(231,459,7,8,1.7),(304,375,7,7,1.5),(518,581,7,7,1.7),(349,800,3,10,2.2),(281,775,3,12,2.6),(93,909,3,14,3.1),(803,1020,3,8,2),(1080,916,0,10,2.4),(1223,905,0,12,2.7),(1438,956,0,9,2.6)]):tree('TreeMass_%02d'%i,x,y,z,h,r)
    # Extra faceted cliff shoulders, coarse macro segmentation only.
    for i,(x,y,hh,rr) in enumerate([(160,627,6,1.7),(244,673,7,2),(387,722,7,1.9),(925,839,7,2),(1047,879,6,1.7),(1254,882,7,2),(1450,747,7,2)]):
        p=anchor(x,y,0);prism('CliffShoulder_%d'%i,[(p[0]+rr*math.cos(j*math.tau/5),p[1]+rr*math.sin(j*math.tau/5)) for j in range(5)],0,hh,'rock')

def render(name,width,height,camera=None):
    camera=camera or {'pitch':35,'yaw':20,'vertical_span':48,'center':[0,0,0],'projection':'orthographic','distance':110,'fov':24.619954}
    pitch,yaw=map(math.radians,[camera['pitch'],camera['yaw']])
    r=np.array([math.cos(yaw),math.sin(yaw),0]);u=np.array([-math.sin(yaw)*math.sin(pitch),math.cos(yaw)*math.sin(pitch),math.cos(pitch)]);v=np.cross(r,u)
    center=np.array(camera['center']);s=height/camera['vertical_span']
    rgb=np.zeros((height,width,3),dtype=np.uint8);rgb[:]=[136,145,153]
    zz=np.full((height,width),-np.inf);labels=np.full((height,width),-1,np.int32)
    light=np.array([-.45,-.6,.9]);light/=np.linalg.norm(light)
    bounds={}
    def project(vertices):
        q=np.asarray(vertices)-center;depth=q@v
        k=np.ones(len(q))
        if camera['projection']=='perspective':k=camera['distance']/(camera['distance']-depth)
        return np.column_stack([width/2+q@r*s*k,height/2-q@u*s*k,depth])
    for mi,m in enumerate(MESHES):
        world=np.asarray(m['vertices']);p=project(world)
        bounds[m['name']]=[float(p[:,0].min()),float(p[:,1].min()),float(p[:,0].max()),float(p[:,1].max())]
        for f in m['triangles']:
            a,b,c=p[f]; xmin=max(0,int(np.floor(min(a[0],b[0],c[0]))));xmax=min(width-1,int(np.ceil(max(a[0],b[0],c[0]))));ymin=max(0,int(np.floor(min(a[1],b[1],c[1]))));ymax=min(height-1,int(np.ceil(max(a[1],b[1],c[1]))))
            if xmin>xmax or ymin>ymax:continue
            den=(b[1]-c[1])*(a[0]-c[0])+(c[0]-b[0])*(a[1]-c[1])
            if abs(den)<1e-7:continue
            X,Y=np.meshgrid(np.arange(xmin,xmax+1)+.5,np.arange(ymin,ymax+1)+.5)
            wa=((b[1]-c[1])*(X-c[0])+(c[0]-b[0])*(Y-c[1]))/den
            wb=((c[1]-a[1])*(X-c[0])+(a[0]-c[0])*(Y-c[1]))/den;wc=1-wa-wb
            dep=wa*a[2]+wb*b[2]+wc*c[2];sub=zz[ymin:ymax+1,xmin:xmax+1]
            ok=(wa>=-1e-6)&(wb>=-1e-6)&(wc>=-1e-6)&(dep>sub)
            if not ok.any():continue
            ww=world[f];normal=np.cross(ww[1]-ww[0],ww[2]-ww[0]);normal/=max(1e-8,np.linalg.norm(normal))
            shade=.66+.34*abs(normal@light)
            color=np.asarray(m['color'])*shade
            rgb[ymin:ymax+1,xmin:xmax+1][ok]=color.astype(np.uint8);sub[ok]=dep[ok];labels[ymin:ymax+1,xmin:xmax+1][ok]=mi
    Image.fromarray(rgb).save(OUT/(name+'.png'))
    family_stats={}
    for fam in COLORS:
        ids=[i for i,m in enumerate(MESHES) if m['family']==fam]
        family_stats[fam]=round(float(np.isin(labels,ids).sum())/(width*height)*100,3)
    visible={}
    for i,m in enumerate(MESHES):
        yy,xx=np.where(labels==i)
        visible[m['name']]={'pixels':len(xx),'bbox':None if len(xx)==0 else [int(xx.min()),int(yy.min()),int(xx.max()+1),int(yy.max()+1)]}
    (OUT/(name+'-metrics.json')).write_text(json.dumps({'renderer':'CPU 3D triangle rasterization + z buffer, no reference compositing','resolution':[width,height],'camera':camera,'projected_bounds':bounds,'visible':visible,'visible_family_occupancy_percent':family_stats},indent=2)+'\n')
    return rgb,bounds,visible

def export():
    scene={'schema_version':1,'classification':'GREYBOX_ONLY','coordinates':'X right / Y depth / Z up; Unity (x,z,y)','camera':{'projection':'orthographic','pitch':PITCH,'yaw':YAW,'vertical_span':48,'orthographic_size':24,'distance':110,'center':[0,0,0],'basis_right':RIGHT.tolist(),'basis_up':UP.tolist(),'basis_toward_camera':VIEW.tolist()},'meshes':MESHES}
    (OUT/'blockout-scene.json').write_text(json.dumps(scene,indent=2)+'\n')
    unity={'classification':'GREYBOX_ONLY','meshes':[{'name':m['name'],'family':m['family'],'vertices':[{'x':v[0],'y':v[2],'z':v[1]} for v in m['vertices']],'indices':[idx for t in m['triangles'] for idx in t],'color':m['color']} for m in MESHES]}
    (OUT/'unity-blockout-input.json').write_text(json.dumps(unity,indent=2)+'\n')
    obj=['# Valoria PREPRODUCTION GREYBOX only. Z up. No final assets.'];offset=1
    for m in MESHES:
        obj.append('o '+m['name'])
        obj.extend('v '+' '.join('%.6f'%q for q in v) for v in m['vertices'])
        obj.extend('f '+' '.join(str(offset+q) for q in t) for t in m['triangles']);offset+=len(m['vertices'])
    (OUT/'full-scene-greybox.obj').write_text('\n'.join(obj)+'\n')

if __name__=='__main__':
    build();export()
    render('BLOCKOUT-source-3x2',1536,1024)
    # Preserve complete source framing without distortion: expanded horizontal view.
    render('BLOCKOUT-16x9',1820,1024)
    render('BLOCKOUT-mobile-landscape',1280,720)
    render('BLOCKOUT-portrait-home',390,844,{'pitch':35,'yaw':20,'vertical_span':48,'center':anchor(833,312,0),'projection':'orthographic','distance':110,'fov':24.62})
    for name,xy in [('left',(453,430)),('right',(1270,458)),('entry',(691,703))]:
        render('BLOCKOUT-portrait-'+name,390,844,{'pitch':35,'yaw':20,'vertical_span':36,'center':anchor(*xy,0),'projection':'orthographic','distance':110,'fov':24.62})
    render('CAMERA-legacy-orientation',1536,1024,{'pitch':17.22,'yaw':30.09,'vertical_span':48,'center':[0,0,0],'projection':'orthographic','distance':110,'fov':24.62})
    render('CAMERA-perspective',1536,1024,{'pitch':35,'yaw':20,'vertical_span':48,'center':[0,0,0],'projection':'perspective','distance':110,'fov':24.62})
    print('GREYBOX_CREATED',len(MESHES),'meshes',sum(len(m['triangles']) for m in MESHES),'triangles')
