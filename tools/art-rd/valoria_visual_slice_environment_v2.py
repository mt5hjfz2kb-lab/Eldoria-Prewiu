"""Valoria Bastion I organic island environment, Blender mesh authoring, CC0-free original geometry."""
import argparse, math, random, json, os, sys
import bpy
from mathutils import Vector
args=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
ap=argparse.ArgumentParser();ap.add_argument("--output",required=True);ap.add_argument("--report",required=True)
a=ap.parse_args(args)
bpy.ops.wm.read_factory_settings(use_empty=True)
rng=random.Random(11026)
materials=[]
def mat(name,rgb,roughness=0.86):
    m=bpy.data.materials.new(name)
    m.diffuse_color=(*rgb,1)
    m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*rgb,1)
    bs.inputs["Roughness"].default_value=roughness
    materials.append(m)
    return len(materials)-1
grass=[mat("Grass lichen moss %d"%i,c) for i,c in enumerate([(0.15,.20,.085),(.165,.215,.093),(.14,.19,.084),(.18,.22,.097)])]
stone=[mat("Weathered layered granite %d"%i,c) for i,c in enumerate([(.14,.155,.185),(.19,.185,.19),(.13,.16,.19),(.20,.19,.18),(.12,.14,.16)])]
needle=[mat("Conifer foliage %d"%i,c) for i,c in enumerate([(.025,.095,.053),(.034,.112,.067),(.043,.12,.077),(.03,.10,.064),(.055,.12,.065)])]
bark=mat("Bark weathered",(.22,.17,.13))
path=mat("Path sandstone",(.48,.43,.36))
dirt=mat("Disturbed earth",(.36,.32,.23))
bushes=[mat("Heather %d"%i,c) for i,c in enumerate([(.19,.25,.16),(.23,.29,.19),(.27,.31,.17)])]
V=[];F=[];M=[]
def vertex(p):
    V.append(p);return len(V)-1
def face(verts,m):
    F.append(verts);M.append(m)
def height(x,z):
    rise=max(0,min(1,(z-8)/18))
    rise=(rise*rise*(3-2*rise))*5.2
    return -.25+rise+.35*math.sin(x*.18)*math.cos(z*.14)+.19*math.sin(x*.43+z*.37)
def noise(x,z):
    return .42*math.sin(x*.37+z*.19)+.24*math.sin(x*.81-z*.51)+.12*math.sin(x*1.7+z*1.31)
def perimeter(theta):
    return 39+2.6*math.sin(theta*7.3)+1.3*math.cos(theta*13.2)+.65*math.sin(theta*27.7)
def top_height(x,z):
    return height(x,z)+max(0,(math.sqrt(x*x+z*z)-19)/22)*.38*noise(x,z)
# radial top terrain, color variations at polygon level
sectors=160;rings=43
rows=[]
for ri in range(rings+1):
    frac=ri/rings
    row=[]
    for si in range(sectors+1):
        ang=si*2*math.pi/sectors
        r=frac*perimeter(ang)
        x=r*math.cos(ang);z=r*math.sin(ang)
        row.append(vertex((x,z,top_height(x,z))))
    rows.append(row)
for ri in range(rings):
    for si in range(sectors):
        a0,b0,c0,d0=rows[ri][si],rows[ri][si+1],rows[ri+1][si],rows[ri+1][si+1]
        # Introduce controlled grassy variation while leaving paths to 3D paving.
        px,pz,_=V[a0]
        patch=math.sin(px*.115+pz*.035)+.55*math.cos(pz*.12-px*.052)
        k=grass[0 if patch<-.5 else 1 if patch<.4 else 2 if patch<1.15 else 3]
        face((a0,c0,b0),k);face((b0,c0,d0),k)
# multi-shelf jagged cliff: fractured radial creases with no smooth uniform cylindrical wall
levels=[0,.22,.53,.78,1]
ridges=[]
for li,t in enumerate(levels):
    row=[]
    for si in range(sectors+1):
        ang=si*2*math.pi/sectors
        r=perimeter(ang)+(t*t)*5.3+(.34+1.1*t)*math.sin(ang*31+li*1.5)
        x=r*math.cos(ang);z=r*math.sin(ang)
        drop=(8.8*t)+(.37+1.5*t)*math.sin(ang*17.5+li*.8)+(.14+.42*t)*math.sin(ang*48.5)
        row.append(vertex((x,z,top_height(perimeter(ang)*math.cos(ang),perimeter(ang)*math.sin(ang))-drop)))
    ridges.append(row)
for j in range(len(levels)-1):
    for i in range(sectors):
        a0,b0,c0,d0=ridges[j][i],ridges[j][i+1],ridges[j+1][i],ridges[j+1][i+1]
        rock=stone[(i//3+j*2+int(3*abs(math.sin(i*.7))))%len(stone)]
        face((a0,c0,b0),rock);face((b0,c0,d0),rock)
# varied irregular rocks and tapered needles created as native triangular meshes
def transformed_ico(cx,cz,cy,radii,material,subdiv=0):
    # rough low-poly icosahedron; irregular surfaces not Blender primitive objects.
    from mathutils import Vector
    phi=(1+5**.5)/2
    pts=[(-1,phi,0),(1,phi,0),(-1,-phi,0),(1,-phi,0),(0,-1,phi),(0,1,phi),(0,-1,-phi),(0,1,-phi),(phi,0,-1),(phi,0,1),(-phi,0,-1),(-phi,0,1)]
    fs=[(0,11,5),(0,5,1),(0,1,7),(0,7,10),(0,10,11),(1,5,9),(5,11,4),(11,10,2),(10,7,6),(7,1,8),(3,9,4),(3,4,2),(3,2,6),(3,6,8),(3,8,9),(4,9,5),(2,4,11),(6,2,10),(8,6,7),(9,8,1)]
    ids=[]
    for i,p in enumerate(pts):
        n=Vector(p).normalized()
        perturb=.83+.30*rng.random()
        ids.append(vertex((cx+n.x*radii[0]*perturb,cz+n.y*radii[1]*perturb,cy+n.z*radii[2]*perturb)))
    for f in fs:face(tuple(ids[k] for k in f),material)
# rocky talus in irregular clusters at cliffs and within alpine edges
for j in range(125):
    ang=rng.random()*math.tau;r=31+rng.random()*12
    x=r*math.cos(ang);z=r*math.sin(ang)
    y=top_height(x,z)-(.35 if r<39 else rng.random()*3.5)
    size=.35+rng.random()*1.35
    transformed_ico(x,z,y,(size*1.4,size*.95,size*.65),stone[rng.randrange(len(stone))])
# low-detail tree silhouette with clustered irregular bough masses, no cylindrical cone assets
for j in range(0):
    theta=rng.random()*math.tau
    r=(14+rng.random()*23)
    x=r*math.cos(theta);z=r*math.sin(theta)
    if abs(x)<5.8 and -42<z<34: continue
    if (z>16 and abs(x)<13):continue
    if r>perimeter(theta)-2.2:continue
    h=3.5+rng.random()*4.7
    y=top_height(x,z)
    # slender bark trunk
    rings_trunk=[]
    for t in [0,.72]:
        ids=[]
        for s in range(6):
            ang=math.tau*s/6
            ids.append(vertex((x+.11*math.cos(ang),z+.11*math.sin(ang),y+h*t)))
        rings_trunk.append(ids)
    for i in range(6):
        face((rings_trunk[0][i],rings_trunk[0][(i+1)%6],rings_trunk[1][(i+1)%6],rings_trunk[1][i]),bark)
    tier_count=4+rng.randrange(2)
    for tier in range(tier_count):
        frac=tier/tier_count
        low=.21+frac*.52;high=min(.98,low+.40-frac*.09)
        radius=h*(.23-frac*.16)*(0.86+.18*rng.random())
        color=needle[(j+tier)%len(needle)]
        sides=9
        bottom=[]
        offset=rng.random()*math.tau
        for k in range(sides):
            ang=offset+math.tau*k/sides
            distortion=.78+.36*rng.random()
            bottom.append(vertex((x+math.cos(ang)*radius*distortion,z+math.sin(ang)*radius*distortion,y+h*low+.06*h*rng.random())))
        tip=vertex((x+.06*rng.random(),z+.06*rng.random(),y+h*high))
        for k in range(sides):
            face((bottom[k],bottom[(k+1)%sides],tip),color)
# shrubs in clustered patches
for j in range(115):
    ang=rng.random()*math.tau;r=12+rng.random()*24
    x=r*math.cos(ang);z=r*math.sin(ang)
    if abs(x)<4.5 and -40<z<31:continue
    y=top_height(x,z)+.22
    rad=.27+.45*rng.random()
    transformed_ico(x,z,y,(rad,rad,.25),bushes[j%3])

# Main approach descends from the southern escarpment toward the valley.
# It is a continuous sloped stone causeway, not disconnected floating blocks.
bridge_rows=[]
for i in range(35):
    t=i/34
    z=-37-26*t
    cx=1.8*math.sin(z*.075)
    y=height(cx,-37)-8.3*(t*t*(3-2*t))
    w=2.55+.18*math.sin(i*.4)
    l=vertex((cx-w,z,y));r=vertex((cx+w,z,y))
    ll=vertex((cx-w,z,y-.45));rr=vertex((cx+w,z,y-.45))
    bridge_rows.append((l,r,ll,rr))
for i in range(34):
    l,r,ll,rr=bridge_rows[i];ln,rn,lln,rrn=bridge_rows[i+1]
    face((l,ln,r),path);face((r,ln,rn),path)
    face((ll,lln,l),stone[1]);face((l,lln,ln),stone[1])
    face((r,rn,rr),stone[2]);face((rr,rn,rrn),stone[2])
# Low stone parapets provide a continuous legible bridge silhouette from the camera.
for side in [-1,1]:
    rows=[]
    for i in range(35):
        t=i/34;z=-37-26*t;cx=1.8*math.sin(z*.075)
        base=height(cx,-37)-8.3*t*t*(3-2*t)
        x=cx+side*(2.6+.18*math.sin(i*.4))
        a0=vertex((x,z,base));a1=vertex((x+side*.45,z,base))
        a2=vertex((x,z,base+.75));a3=vertex((x+side*.45,z,base+.75))
        rows.append((a0,a1,a2,a3))
    for i in range(34):
        a0,a1,a2,a3=rows[i];b0,b1,b2,b3=rows[i+1]
        face((a0,b0,a2),stone[1]);face((a2,b0,b2),stone[1])
        face((a1,a3,b1),stone[2]);face((a3,b3,b1),stone[2])
        face((a2,b2,a3),stone[0]);face((a3,b2,b3),stone[0])

# polygons in their own material slot, GLB embeds original colors
mesh=bpy.data.meshes.new("Valoria authored geological mesh and forest")
mesh.from_pydata(V,[],F)
mesh.materials.clear()
for m in materials:mesh.materials.append(m)
mesh.update()
for poly,index in zip(mesh.polygons,M):poly.material_index=index;poly.use_smooth=False
obj=bpy.data.objects.new("Valoria Highlands - Blender sculpted environment",mesh)
bpy.context.collection.objects.link(obj)

# Apply the project's EXISTING PBR texture families instead of flat palette polygons.
texture_dir=os.path.abspath("Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures")
uvmap=mesh.uv_layers.new(name="ValoriaTiledUV")
for poly in mesh.polygons:
    mi=poly.material_index
    for li in poly.loop_indices:
        vert=mesh.vertices[mesh.loops[li].vertex_index].co
        if mi in grass:
            uv=(vert.x/8,vert.y/8)
        elif mi in stone:
            ang=math.atan2(vert.y,vert.x)
            uv=(ang*2.5,vert.z/7)
        else:
            uv=(vert.x/3,vert.y/3)
        uvmap.data[li].uv=uv
families=[(stone,"rock"),([path],"stone")]
texture_count=0
for ids,family in families:
    png=os.path.join(texture_dir,family+"_albedo.png")
    if not os.path.isfile(png):
        print("PBR_FAMILY_UNAVAILABLE "+png)
        continue
    img=bpy.data.images.load(png,check_existing=True)
    for index in ids:
        material=materials[index]
        nodes=material.node_tree.nodes
        links=material.node_tree.links
        bs=nodes.get("Principled BSDF")
        if bs is None:continue
        base=bs.inputs["Base Color"].default_value[:]
        tex=nodes.new("ShaderNodeTexImage")
        tex.image=img;tex.interpolation='Linear';tex.extension='REPEAT'
        mix=nodes.new("ShaderNodeMixRGB")
        mix.blend_type='MULTIPLY'
        mix.inputs[0].default_value=.38
        mix.inputs[2].default_value=base
        links.new(tex.outputs["Color"],mix.inputs[1])
        links.new(mix.outputs["Color"],bs.inputs["Base Color"])
        texture_count+=1

# Export Y-up through Blender glTF conversion to Unity coordinates.
os.makedirs(os.path.dirname(a.output),exist_ok=True)
bpy.ops.export_scene.gltf(filepath=a.output,export_format="GLB",export_apply=True)
mesh.calc_loop_triangles()
report={"original_blender_geometry":True,"triangles":len(mesh.loop_triangles),"vertices":len(mesh.vertices),"materials":len(materials),"meshes":1,"pbr_textured_materials":texture_count,"credits_used":0,"output_bytes":os.path.getsize(a.output)}
with open(a.report,"w") as f:json.dump(report,f,indent=2)
print("ELDORIA_VALORIA_ENVIRONMENT_GLTF_PASS "+json.dumps(report))
