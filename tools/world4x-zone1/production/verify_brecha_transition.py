"""Independent unchanged border/pad and genuine relief report. Never claims art approval."""
import argparse,hashlib,json,math,pathlib,sys
import bpy
p=argparse.ArgumentParser()
p.add_argument("--before",type=pathlib.Path,required=True)
p.add_argument("--after",type=pathlib.Path,required=True)
p.add_argument("--out",type=pathlib.Path,required=True)
a=p.parse_args(sys.argv[sys.argv.index("--")+1:])
a.out.mkdir(parents=True,exist_ok=False)
def state(path):
 bpy.ops.wm.open_mainfile(filepath=str(path.resolve()))
 meshes=[o for o in bpy.data.objects if o.type=="MESH" and not o.name.startswith("_EldoriaReview")]
 if len(meshes)!=1:raise ValueError("Exactly one authored source mesh")
 o=meshes[0]
 return [[float(c) for c in v.co] for v in o.data.vertices],len(o.data.polygons),[[float(c) for c in row] for row in o.matrix_world]
b,n0,w0=state(a.before);c,n1,w1=state(a.after)
issues=[];changed=0;heights=[]
if len(b)!=len(c) or n0!=n1:issues.append("Topology changed")
if w0!=w1:issues.append("Object transform moved")
if len(b)==len(c):
 for initial,final in zip(b,c):
  if any(abs(initial[j]-final[j])>1e-4 for j in (0,1)):issues.append("XY changed");break
 for initial,final in zip(b,c):
  x,y,z=initial;dz=final[2]-z
  if abs(x)>=118 or abs(y)>=118 or (-40<=x<=4 and -52<=y<=-8):
   if abs(dz)>1e-4:issues.append("City pad or border changed");break
  if abs(dz)>1e-4:changed+=1;heights.append(dz)
 if not heights:issues.append("No genuine relief changes")
 else:
  if min(heights)>-3:issues.append("No meaningful valley after projection")
  if max(heights)<3:issues.append("No meaningful rise after projection")
proof={"state":"GEOMETRY_PRESERVED_REVIEW_REQUIRED" if not issues else "GEOMETRY_REJECTED",
 "issues":issues,"source_sha256":hashlib.sha256(a.before.read_bytes()).hexdigest(),
 "candidate_sha256":hashlib.sha256(a.after.read_bytes()).hexdigest(),
 "vertex_count":len(c),"face_count":n1,"changed_vertices":changed,
 "real_min_dz":min(heights) if heights else None,"real_max_dz":max(heights) if heights else None,
 "pad_and_border_unchanged":not any("border" in e.lower() or "pad" in e.lower() for e in issues),
 "geometry_pass":not issues,"art_approved":False,"unity_tested":False}
(a.out/"geometry-gate.json").write_text(json.dumps(proof,indent=2)+"\n")
if issues:raise RuntimeError(str(issues))
