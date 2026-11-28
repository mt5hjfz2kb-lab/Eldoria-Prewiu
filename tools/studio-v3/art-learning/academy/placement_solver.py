"""Screen-space clearance lab: measure actual Blender projected mesh hulls,
solve bounded placements, keep the original camera and all assets.
Conservative convex hulls, not pixel visibility segmentation."""
import bpy,math,json
from bpy_extras.object_utils import world_to_camera_view
from mathutils import Vector
def hull(points):
    p=sorted(set(points))
    if len(p)<3:raise ValueError("Not enough projected points")
    def cross(o,a,b):return (a[0]-o[0])*(b[1]-o[1])-(a[1]-o[1])*(b[0]-o[0])
    l=[];u=[]
    for x in p:
        while len(l)>1 and cross(l[-2],l[-1],x)<=0:l.pop()
        l.append(x)
    for x in reversed(p):
        while len(u)>1 and cross(u[-2],u[-1],x)<=0:u.pop()
        u.append(x)
    return l[:-1]+u[:-1]
def gap(a,b):
    # Largest separating-axis gap. Negative means overlapping convex hulls.
    # This is a conservative screen clearance measure in pixels.
    best=-math.inf
    for p in (a,b):
        for i,x in enumerate(p):
            y=p[(i+1)%len(p)];dx=y[0]-x[0];dy=y[1]-x[1];n=math.hypot(dx,dy)
            if n<1e-9:continue
            ux,uy=-dy/n,dx/n
            aa=[z[0]*ux+z[1]*uy for z in a];bb=[z[0]*ux+z[1]*uy for z in b]
            best=max(best,min(bb)-max(aa),min(aa)-max(bb))
    return best
def shift(p,d):return [(x+d[0],y+d[1]) for x,y in p]
def solve(sc,v,path):
    bpy.context.view_layer.update()
    graph=bpy.context.evaluated_depsgraph_get()
    w,h=sc.render.resolution_x,sc.render.resolution_y
    assert sc.camera.data.type=="ORTHO"
    def point(p):
        q=world_to_camera_view(sc,sc.camera,p);return q.x*w,q.y*h
    zero=point(Vector((0,0,0)));xx=point(Vector((1,0,0)));yy=point(Vector((0,1,0)))
    ex=(xx[0]-zero[0],xx[1]-zero[1]);ey=(yy[0]-zero[0],yy[1]-zero[1])
    def delta(dx,dy):return (ex[0]*dx+ey[0]*dy,ex[1]*dx+ey[1]*dy)
    names={"hall":("hall ","front ","timber jamb","Main roof","forge chimney","anvil"),
           "road":("road",),"well":("well ",),"cart":("cart ","wheel spoke"),"stall":("stall ",)}
    groups={k:[o for o in sc.objects if o.type=="MESH" and any(o.name.startswith(n) for n in ns)] for k,ns in names.items()}
    assert all(groups.values())
    def outline(group):
        pts=[]
        for o in groups[group]:
            eo=o.evaluated_get(graph);mesh=eo.to_mesh()
            try:pts.extend(point(eo.matrix_world@vert.co) for vert in mesh.vertices)
            finally:eo.to_mesh_clear()
        return hull(pts)
    shapes={k:outline(k) for k in groups}
    def metric(k,p):
        obstacles=[x for x in shapes if x!=k]
        margins={x:gap(p,shapes[x]) for x in obstacles}
        margins["frame"]=min(min(x for x,y in p),w-max(x for x,y in p),min(y for x,y in p),h-max(y for x,y in p))
        return margins
    initial={k:metric(k,shapes[k]) for k in ("well","cart")}
    decisions={}
    # Search explicit bounded lab footprint; no global camera or scale change.
    for k,xrange,yrange in (("well",(-3.0,-1.8),(-1.2,.0)),("cart",(1.7,3.4),(-1.7,-.25))):
        orig=(v[k+"_x"],v[k+"_y"]);base=shapes[k];options=[]
        for ix in range(round((xrange[1]-xrange[0])/.05)+1):
            x=xrange[0]+ix*.05
            for iy in range(round((yrange[1]-yrange[0])/.05)+1):
                y=yrange[0]+iy*.05
                dx,dy=x-orig[0],y-orig[1];p=shift(base,delta(dx,dy));m=metric(k,p)
                if m["frame"]<24 or abs(x)<1.4:continue
                clearance=min(z for name,z in m.items() if name!="frame")
                distance=math.hypot(dx,dy)
                # Minimum change meeting a visible 12px gap, else best feasible margin.
                options.append((clearance>=12, -distance if clearance>=12 else clearance,-distance,x,y,m))
        assert options
        _,_,_,x,y,m=max(options,key=lambda q:q[:3])
        dx,dy=x-orig[0],y-orig[1];shapes[k]=shift(base,delta(dx,dy))
        for o in groups[k]:o.location.x+=dx;o.location.y+=dy
        decisions[k]={"original":orig,"corrected":[x,y],"offset":[dx,dy]}
    bpy.context.view_layer.update()
    measured={k:outline(k) for k in groups}
    # Re-measure actual moved meshes rather than trust the predicted translation.
    shapes.update(measured)
    after={k:metric(k,shapes[k]) for k in ("well","cart")}
    report={"method":"projected convex hull separation (conservative SAT pixels)","camera_unchanged":True,"resolution":[w,h],"minimum_target_px":12,"before":initial,"after":after,"placements":decisions,
            "lab_only":True,"actual_unity":False,"protected_objects":["hall","road","stall"],"limitations":"Hull overlap is conservative; not occlusion fraction or visual quality. Render review remains required."}
    path.write_text(json.dumps(report,indent=2))
    assert all(after[k]["hall"]>initial[k]["hall"] and after[k]["frame"]>=24 for k in ("well","cart")),report
    print("SCREEN_CLEARANCE_LAB",json.dumps(report),flush=True)
