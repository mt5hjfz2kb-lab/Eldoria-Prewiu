"""Read-only source-grounded preflight for model-authored Blender plans.

NEVER rewrite a model-selected value. Reject impossible original proposals,
explain the exact contract error and allow the model to design a NEW proposal.
"""
from contract import RejectedPlan
from sculpt_scale_contract import validate_source_anchor_brush

def relative_triangle_limit(inventory):
    """Safe QA compares evaluated real Blender triangle counts, not face*N."""
    if not inventory or any(type(o.get("evaluated_triangles")) is not int or
                            o["evaluated_triangles"] < 0 for o in inventory):
        raise RejectedPlan("Physical evaluated source triangle counts required; never estimate from face count")
    source_triangles=sum(o["evaluated_triangles"] for o in inventory)
    if source_triangles <= 0:raise RejectedPlan("Empty source evaluated mesh")
    # Preserve an inherited asset's existing geometry; reject substantial
    # growth but never misclassify the old 33k-triangle model as 'too dense'
    # solely because its ~5k faces include 3+ triangles per face.
    return min(220000,max(20000,int(source_triangles*1.25)+1500))


def verify_source_plan(document,inventory):
    if not inventory:raise RejectedPlan("Source Blender inventory missing")
    known={obj["name"]:obj for obj in inventory}
    available=set(known)
    topology_changed=set()
    errors=[]
    structural=False
    for i,action in enumerate(document.get("actions",[]),1):
        op=action["op"]; target=action["target"]; params=action["params"]
        if op not in ("mesh","primitive","beam_between_faces","duplicate","import_asset") and target not in available:
            errors.append(f"Action {i}: unknown target {target!r}; use actual inspected Blender names")
            continue
        if op in ("mesh","primitive","beam_between_faces","duplicate","import_asset"):
            if target in available:errors.append(f"Action {i}: new target shadows {target!r}")
            if op=="duplicate" and params.get("source") not in available:errors.append(f"Action {i}: duplicate source missing")
            if op=="beam_between_faces":
                anchor_source=known.get(params.get("source"))
                if anchor_source is None:
                    errors.append(f"Action {i}: source for two anchor faces not found")
                else:
                    allowed={h.get("face_index"):h for h in
                        anchor_source.get("camera_visible_face_landmarks",[])
                        if h.get("camera_visible") is True and
                           h.get("camera_ray_hit_original_face") is True}
                    aa,bb=params.get("start_face"),params.get("end_face")
                    if aa==bb or aa not in allowed or bb not in allowed:
                        errors.append(f"Action {i}: only TWO DISTINCT camera-ray-hit verified source faces allowed")
                    elif not all(isinstance(allowed[q].get("world_center_m"),list)
                                 and len(allowed[q]["world_center_m"])==3
                                 for q in (aa,bb)):
                        errors.append(f"Action {i}: missing REAL WORLD source face centers")

            available.add(target);structural=True;continue
        actual=known.get(target)
        if op in ("extrude","inset","subdivide"):
            structural=True
            if actual is None or target in topology_changed:
                errors.append(f"Action {i}: face topology has changed/was created; re-inspect in a subsequent Blender cycle")
                continue
            indices=params.get("faces",[])
            if indices and (not isinstance(indices,list) or any(
                    type(v)!=int or v<0 or v>=actual["faces"] for v in indices)):
                errors.append(f"Action {i}: INVALID FACE; {actual['faces']} real Blender faces "
                              f"allow FACE indices 0..{actual['faces']-1}; vertex_index is NOT face index")
            topology_changed.add(target)
        elif op=="sculpt":
            structural=True
            if actual:
                points=[p.get("local_position_m") for p in actual.get("local_extrema",{}).values()]
                points += [p.get("local_position_m") for p in actual.get("vertex_landmarks",[])]
                points=[p for p in points if isinstance(p,list) and len(p)==3]
                center=params.get("center",[])
                radius=params.get("radius",0)
                if not points or not isinstance(center,list) or len(center)!=3 or not isinstance(radius,(int,float)) or radius<=0 or not any(
                        sum((float(a)-float(b))**2 for a,b in zip(center,point)) <= radius**2 for point in points):
                    errors.append(f"Action {i}: sculpt center/radius has no verified inherited mesh landmark in range; "
                                  "choose your OWN center within radius of local_extrema local_position_m, "
                                  "or your OWN sculpt_anchor; don't invent coordinates or use world position")
        elif op in ("material","uv","shader_graph","texture"):
            # Imported production GLB has a shared texture atlas across the
            # entire dense inherited mesh. Whole-object repaint or smart UV
            # unwrap destroys existing art even for a one-face edit.
            # Qwen may finish independent NEW meshes in their own cycles.
            if actual and actual.get("faces",0)>2500:
                errors.append(
                    f"Action {i}: PRESERVE_INHERITED_TEXTURE_ATLAS: "
                    f"whole-object {op} is forbidden on dense source "
                    f"{target!r}. Do a LOCAL structural edit only, or "
                    "create a distinct small mesh and finish THAT object "
                    "in a separate safely inspected cycle.")
        elif op=="sculpt_anchor":
            structural=True
            if actual and params.get("anchor") not in actual.get("local_extrema",{}):
                errors.append(f"Action {i}: invalid inherited-mesh anchor")
            if actual:
                try:validate_source_anchor_brush(action,actual)
                except ValueError as e:errors.append(f"Action {i}: {e}")
        elif op=="bevel":
            structural=True
            if actual is None or target in topology_changed:
                errors.append(f"Action {i}: selected bevel edges require a freshly inspected source mesh")
            else:
                edges=params.get("edges")
                if not isinstance(edges,list) or not edges or any(
                        type(v)!=int or v<0 or v>=actual.get("edges",0) for v in edges):
                    errors.append(f"Action {i}: global bevel forbidden on inherited source; "
                                  f"choose actual edge_landmarks edge_index 0..{actual.get('edges',0)-1}")
                elif actual.get("faces",0)>2500 and (params.get("width",0)>0.09 or params.get("segments",0)>2):
                    errors.append(f"Action {i}: dense source bevel width/segments too destructive")
            topology_changed.add(target)
        elif op in ("native_operator","boolean","modifier","transform"):
            # Non-selective smoothing/bevel/subdivision on a detailed imported
            # game source can destroy silhouettes and multiply triangles.
            # AI may select local face/edge authoring instead; never rewrite.
            if actual and actual.get("faces",0)>2500:
                if op=="modifier" and params.get("kind") in ("SUBSURF","BEVEL"):
                    errors.append(f"Action {i}: dense source forbids nonselective "
                                  f"{params.get('kind')} modifier; use source-grounded selected edges/faces")
                if op=="native_operator" and params.get("operator")=="mesh.subdivide":
                    errors.append(f"Action {i}: dense source forbids whole-object mesh.subdivide")
            if op=="boolean" or (op=="native_operator" and
                   str(params.get("operator","")).startswith("mesh.")) or (op=="modifier" and params.get("apply",False)):
                topology_changed.add(target)
            if op!="native_operator" or str(params.get("operator","")).startswith("mesh."):
                structural=True
    if not structural:errors.append("Full artist plan needs a structural change; surface-only repaint is insufficient")
    if errors:raise RejectedPlan("SOURCE_GROUNDING_REJECTED: "+" | ".join(errors))
    return True
