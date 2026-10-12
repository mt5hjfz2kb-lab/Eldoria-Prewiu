# Authored scenery envelope; preserve every original source asset and gameplay parcel.
# Replace only the previous rectangular, temporary ground support geometry.
rockside=mat('Valoria schist dark fractured cliff',(.27,.25,.24),.95)
rockdust=mat('Valoria cliff talus earth',(.255,.225,.19),.97)

# Portable, deterministic layered PBR albedo. No new paid assets or generated images.
# Baked into the GLB so Unity/URP uses it without a procedural Blender shader.
rock_image=bpy.data.images.new('Valoria stratified native stone albedo',width=256,height=256)
rock_pixels=[]
for yy in range(256):
    for xx in range(256):
        r=fractal(Vector((xx*.039,yy*.031,9.42)),.8,2.1,5)
        veins=math.sin(yy*.19+r*8.0+math.sin(xx*.021)*2.2)
        n=max(-.11,min(.11,r*.18))+veins*.024
        rock_pixels.extend((max(.09,.37+n),max(.09,.34+n*.92),max(.09,.31+n*.83),1.0))
rock_image.pixels=rock_pixels
rock_image.pack()
rock_tex=rockside.node_tree.nodes.new('ShaderNodeTexImage')
rock_tex.image=rock_image
rockside.node_tree.links.new(rock_tex.outputs['Color'],rockside.node_tree.nodes['Principled BSDF'].inputs['Base Color'])

valleygrass=mat('Valoria forested valley ground',(.18,.23,.15),.96)
mountainrock=mat('Valoria misted distant mountain rock',(.24,.29,.34),1)

def organic_outline(corners,divisions,seed):
    rng=random.Random(seed);output=[]
    cx=sum(pt[0] for pt in corners)/len(corners)
    cy=sum(pt[1] for pt in corners)/len(corners)
    for i,a0 in enumerate(corners):
        b0=corners[(i+1)%len(corners)]
        for j in range(divisions):
            t=j/divisions
            x=a0[0]*(1-t)+b0[0]*t;y=a0[1]*(1-t)+b0[1]*t
            ripple=(math.sin(math.pi*t)**2)*(rng.uniform(-3.8,3.8)+math.sin((i*divisions+j)*.81)*1.05)
            d=Vector((x-cx,y-cy));d.normalize()
            output.append((x+ripple*d.x,y+ripple*d.y))
    return output

def cliff_plateau(name,corners,z,base_z,seed):
    outline=organic_outline(corners,11,seed)
    polygon(name+'_walkable_cap',[(x,y,z) for x,y in outline],grass)
    cx=sum(x for x,y in outline)/len(outline);cy=sum(y for x,y in outline)/len(outline)
    ring_levels=((0,z),(.028,z-1.1),(.075,z-3.3),(.16,z-6.0),(.25,base_z))
    vertices=[];rng=random.Random(seed+900)
    for ring,(spread,z0) in enumerate(ring_levels):
        for x,y in outline:
            v=Vector((x-cx,y-cy))
            x2=x+v.x*spread;y2=y+v.y*spread
            jag=(rng.random()-.5)*(.11 if ring==0 else .9)
            vertices.append((x2+jag,y2+jag,z0+(.1 if ring==0 else (rng.random()-.5)*.45)))
    n=len(outline);faces=[]
    for k in range(len(ring_levels)-1):
        for i in range(n):
            p=k*n+i;q=k*n+(i+1)%n
            faces.append((p,p+n,q+n,q))
    mesh=bpy.data.meshes.new(name+'_weathered_bedrock_mesh')
    mesh.from_pydata(vertices,[],faces);mesh.materials.append(rockside)
    mesh.update();mesh.validate();uv=mesh.uv_layers.new(name='UVMap')
    for loop in mesh.loops:
        co=mesh.vertices[loop.vertex_index].co
        uv.data[loop.index].uv=(co.x*.055+co.z*.014,co.y*.06)
    obj=bpy.data.objects.new(name+'_irregular_cliff',mesh);scene.collection.objects.link(obj)
    return obj


# Continuous sculpted terrain replaces ONLY the two rectangular provisional
# fortress platforms. The separately authored approach valley, pines,
# stream, distant mountain range and source meshes are retained unchanged.
def smoothstep01(t):
    t=max(0,min(1,t));return t*t*(3-2*t)

def terrace_blend(x,y,cx,cy,hx,hy,shoulder):
    # Offset the geometric boundary to avoid straight square escarpments.
    wx=x+math.sin(y*.21)*1.5
    wy=y+math.cos(x*.23)*1.3
    ox=max(0,abs(wx-cx)-hx);oy=max(0,abs(wy-cy)-hy)
    return 1-smoothstep01(math.hypot(ox,oy)/shoulder)

def terrain_level(x,y):
    ground=-19.8+fractal(Vector((x*.042,y*.037,16.1)),.78,2.0,4)*1.8
    # Architectural gameplay anchors remain at their exact authored heights.
    low=terrace_blend(x,y,0,-4,22.5,14,13.5)
    high=terrace_blend(x,y,0,23.5,21.5,9.5,13.2)
    # Preserve the entire existing stone route above the terrain. The first
    # sculpted hill overlaid its road meshes at z=0 (and occluded the staircase).
    height=ground*(1-max(low,high))-.18*low+3.44*high
    if -18.0<=y<=16.5:
        # A smooth, shallow axial corridor follows the author's stair rise;
        # no new road mesh, proxy town, or collider is constructed here.
        route=(-.24 if y<=8.0 else -.24+min(1.0,(y-8.0)/5.5)*3.63)
        edge=1-smoothstep01((abs(x)-3.5)/3.5)
        height=height*(1-edge)+min(height,route)*edge
    return height

terrain_mesh=bpy.data.meshes.new('Valoria continuous cliff plateau and ramps')
nx=104;ny=112
x0=-50.5;x1=50.5;y0=-49.0;y1=57.0
dx=(x1-x0)/nx;dy=(y1-y0)/ny
terrain_verts=[]
for iy in range(ny+1):
    y=y0+iy*dy
    for ix in range(nx+1):
        x=x0+ix*dx
        terrain_verts.append((x,y,terrain_level(x,y)))
terrain_faces=[];terrain_material_indices=[]
for iy in range(ny):
    for ix in range(nx):
        u=iy*(nx+1)+ix;v=u+nx+1
        h0=terrain_verts[u][2];hx=terrain_verts[u+1][2]
        hy=terrain_verts[v][2];hxy=terrain_verts[v+1][2]
        steep=max(abs(hx-h0)/dx,abs(hy-h0)/dy,
                  abs(hxy-hx)/dy,abs(hxy-hy)/dx)
        cx=x0+(ix+.5)*dx;cy=y0+(iy+.5)*dy
        habitat=max(terrace_blend(cx,cy,0,-4,22.5,14,13.5),
                    terrace_blend(cx,cy,0,23.5,21.5,9.5,13.2))
        choice=1 if steep>.59 else (0 if habitat>.50 else 2)
        terrain_faces.extend(((u,u+1,v),(u+1,v+1,v)))
        terrain_material_indices.extend((choice,choice))
terrain_mesh.from_pydata(terrain_verts,[],terrain_faces)
for material in (grass,rockside,valleygrass):terrain_mesh.materials.append(material)
terrain_mesh.update();terrain_mesh.validate()
uv=terrain_mesh.uv_layers.new(name='UVMap')
for poly,index in zip(terrain_mesh.polygons,terrain_material_indices):
    poly.material_index=index;poly.use_smooth=True
for loop in terrain_mesh.loops:
    p=terrain_mesh.vertices[loop.vertex_index].co
    uv.data[loop.index].uv=(p.x*.07,p.y*.07+p.z*.012)
terra=bpy.data.objects.new('Valoria natural playable hilltop and rock scarps',terrain_mesh)
scene.collection.objects.link(terra)

# Basin reaches outside the entire strategic frame; no floating island or flat
# blue rectangle posing as a distant landscape.
polygon('Valoria surrounding valley floor',
    [(-230,-210,-24),(230,-210,-24),(230,240,-24),(-230,240,-24)],valleygrass)
polygon('Valoria distant lake western channel',
    [(-170,-150,-23.85),(-56,-150,-23.85),(-58,10,-23.85),(-107,35,-23.85),(-175,-20,-23.85)],water)
# Exposed below the entry bridge in the owner's three-quarter camera, not a
# flat blue ground layer underneath the reconstruction yard.
polygon('Valoria lower western stream',
    [(-105,-145,-15.8),(-7,-145,-15.8),(-10,-33,-15.8),
     (-39,-19,-15.8),(-96,-32,-15.8)],water)


# Existing fortress geometry remains immutable. Add a continuous valley
# below the causeway so the castle no longer looks like an isolated slab.
front_width=73
front_depth=27
front_vertices=[]
for iy in range(front_depth+1):
    yy=-160+130*iy/front_depth
    for ix in range(front_width+1):
        xx=-180+360*ix/front_width
        shoulder=11.5*math.exp(-((abs(xx)-88)/50)**2)
        noise=fractal(Vector((xx*.025,yy*.033,15.1)),.78,2.0,4)*2.6
        front_vertices.append((xx,yy,-23.2+shoulder+noise))
front_faces=[]
for iy in range(front_depth):
    for ix in range(front_width):
        i=iy*(front_width+1)+ix;j=i+front_width+1
        front_faces.extend(((i,i+1,j),(i+1,j+1,j)))
front_mesh=bpy.data.meshes.new('Valoria approach valley rolling terrain')
front_mesh.from_pydata(front_vertices,[],front_faces)
front_mesh.materials.append(valleygrass)
front_mesh.update();front_mesh.validate()
uv=front_mesh.uv_layers.new(name='UVMap')
for loop in front_mesh.loops:
    co=front_mesh.vertices[loop.vertex_index].co
    uv.data[loop.index].uv=(co.x*.018,co.y*.018)
front_obj=bpy.data.objects.new('Valoria lower valley continuous terrain',front_mesh)
scene.collection.objects.link(front_obj)

def valley_height(x,y):
    peaks=((-109,114,58,49),(-70,96,43,42),(-26,146,72,48),
           (18,104,53,38),(64,136,68,50),(128,105,55,58))
    uplift=0
    for px,py,height,radius in peaks:
        d=((x-px)/radius)**2+((y-py)/(radius*.72))**2
        uplift+=height*math.exp(-d*1.1)
    uneven=fractal(Vector((x*.025,y*.031,5.7)),.85,2.0,4)*5
    return -25+uplift+uneven

nx=55;ny=30;x0=-175;x1=175;y0=40;y1=195
coords=[]
for iy in range(ny+1):
    y=y0+(y1-y0)*iy/ny
    for ix in range(nx+1):
        x=x0+(x1-x0)*ix/nx;z=valley_height(x,y)
        if y<46:z=min(z,-10)
        coords.append((x,y,z))
tris=[]
for iy in range(ny):
    for ix in range(nx):
        i=iy*(nx+1)+ix;b=i+1;c=i+nx+1;d=c+1
        tris.extend(((i,b,c),(b,d,c)))
mount=bpy.data.meshes.new('Misted distant valley terrain')
mount.from_pydata(coords,[],tris)
mount.materials.append(mountainrock);mount.update();mount.validate()
uv=mount.uv_layers.new(name='UVMap')
for loop in mount.loops:
    co=mount.vertices[loop.vertex_index].co;uv.data[loop.index].uv=(co.x*.01,co.y*.01)
obj=bpy.data.objects.new('Valoria true mountain backdrop',mount);scene.collection.objects.link(obj)

# Asymmetric side slopes integrate the cliffs into the valley.
for side in (-1,1):
    edge=[]
    for i in range(19):
        y=-80+i*10
        for j in range(9):
            x=side*(47+j*14)
            ridge=j*3.8+fractal(Vector((x*.026,y*.021,2)),.9,2.0,4)*5-23
            edge.append((x,y,ridge))
    tri=[]
    for i in range(18):
        for j in range(8):
            u=i*9+j;v=u+9
            tri.extend(((u,u+1,v),(u+1,v+1,v)))
    sl=bpy.data.meshes.new('Valoria side valley slope mesh')
    sl.from_pydata(edge,[],tri);sl.materials.append(mountainrock)
    sl.update();sl.validate();uvs=sl.uv_layers.new(name='UVMap')
    for loop in sl.loops:
        v=sl.vertices[loop.vertex_index].co
        uvs.data[loop.index].uv=(v.x*.015,v.y*.014)
    obj=bpy.data.objects.new('Valoria '+str(side)+' valley rock slope',sl)
    scene.collection.objects.link(obj)
