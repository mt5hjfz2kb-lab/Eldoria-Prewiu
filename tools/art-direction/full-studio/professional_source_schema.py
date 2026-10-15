"""Small-model-safe, operation-discriminated schemas for real Blender artistry.

Qwen independently chooses its operations and ALL numeric design parameters.
Schemas only encode verified executable tool signatures and inspected bounds.
This is a progressive subset of the Blender toolbox, NOT all Blender.
"""
# Namespaced but artist-chosen source-assembly objects cannot shadow frozen meshes.
# Only a safe namespace constraint, not a geometry/style/template choice.
from sculpt_scale_contract import conservative_schema_bounds

NEW_COMPONENT_PATTERN = r"^Eldoria_Added_[A-Za-z][A-Za-z0-9_]{3,42}$"

def source_artist_action_schema(objects):
    names=[x["name"] for x in objects]
    if not names:raise ValueError("Inspected existing Blender objects required")
    face_max=max(0,max(x["faces"] for x in objects)-1)
    face_ids={"type":"array","minItems":1,"maxItems":3,
              "items":{"type":"integer","minimum":0,"maximum":face_max}}
    # Enumerate only facts measured in actual Blender. Qwen still decides
    # WHICH available face/edge to modify from visible source evidence.
    # Never fabricate an index or substitute it after generation.
    # A real Blender camera ray-hit takes precedence over arbitrary local
    # percentile face sampling. Never label a hidden face as camera-visible.
    camera_proven_faces=sorted({int(item["face_index"]) for obj in objects
       for item in obj.get("camera_visible_face_landmarks",[])
       if item.get("camera_visible") is True and
          item.get("camera_ray_hit_original_face") is True and
          type(item.get("face_index")) is int and
          0 <= item["face_index"] < obj.get("faces",0)})
    observed_faces=sorted({int(item["face_index"]) for obj in objects
                           for item in obj.get("face_landmarks",[])
                           if type(item.get("face_index")) is int and
                           0 <= item["face_index"] < obj.get("faces",0)})
    observed_edges=sorted({int(item["edge_index"]) for obj in objects
                           for item in obj.get("edge_landmarks",[])
                           if type(item.get("edge_index")) is int and
                           0 <= item["edge_index"] < obj.get("edges",0)})
    if camera_proven_faces:
        face_ids["items"]={"type":"integer","enum":camera_proven_faces}
    elif observed_faces:
        face_ids["items"]={"type":"integer","enum":observed_faces}
    numeric={"type":"number"}
    v3={"type":"array","minItems":3,"maxItems":3,"items":numeric}
    rgba={"type":"array","minItems":4,"maxItems":4,
          "items":{"type":"number","minimum":0,"maximum":1}}
    enum=lambda x:{"type":"string","enum":list(x)}
    # A high-poly inherited source cannot safely receive a whole-object
    # BEVEL modifier. Qwen may still use precisely selected BMesh edges.
    # For low-poly or newly constructed components BEVEL stays available.
    large_inherited=any(obj.get("faces",0)>2500 for obj in objects)
    modifier_kinds=["SOLIDIFY","WEIGHTED_NORMAL","DECIMATE","DISPLACE","SUBSURF"]
    if not large_inherited:modifier_kinds.insert(0,"BEVEL")
    modifier_options={"type":"object","additionalProperties":False,"properties":{
        "width":{"type":"number","minimum":0.001,"maximum":0.09},
        "segments":{"type":"integer","minimum":1,"maximum":3},
        "levels":{"type":"integer","minimum":0,"maximum":2},
        "render_levels":{"type":"integer","minimum":0,"maximum":2},
        "thickness":{"type":"number","minimum":0.001,"maximum":0.2},
        "ratio":{"type":"number","minimum":0.05,"maximum":1},
        "strength":{"type":"number","minimum":0,"maximum":0.2},
        "texture_scale":{"type":"number","minimum":0.1,"maximum":5}
    }}
    # Exact contracts for NEW Qwen-chosen architectural components.
    # Model, not this executor, determines shape, placement and proportions.
    coord={"type":"number","minimum":-15,"maximum":15}
    xyz={"type":"array","minItems":3,"maxItems":3,"items":coord}
    rot={"type":"array","minItems":3,"maxItems":3,
         "items":{"type":"number","minimum":-3.1416,"maximum":3.1416}}
    scl={"type":"array","minItems":3,"maxItems":3,
         "items":{"type":"number","minimum":0.05,"maximum":6}}
    new_ops={
      "primitive":({"shape":enum(["cube","cylinder","cone","plane","uv_sphere","ico_sphere","torus"]),
                    "location":xyz,"rotation":rot,"scale":scl},
                   ["shape","location","rotation","scale"]),
      "mesh":({"vertices":{"type":"array","minItems":3,"maxItems":12,"uniqueItems":True,"items":xyz},
               "faces":{"type":"array","minItems":1,"maxItems":16,
                        "items":{"type":"array","minItems":3,"maxItems":8,
                                 "items":{"type":"integer","minimum":0,"maximum":11}}}},
              ["vertices","faces"])
    }
    if len(camera_proven_faces)>=2:
        # A professional high-level tool, NOT a fixed model: Qwen selects two
        # actual observed ray-hit face IDs and the beam cross section.
        # Blender calculates vertices/orientation from genuine source polygons.
        new_ops["beam_between_faces"]=(
            {"source":enum(names),
             "start_face":{"type":"integer","enum":camera_proven_faces},
             "end_face":{"type":"integer","enum":camera_proven_faces},
             "width_m":{"type":"number","minimum":0.06,"maximum":0.45},
             "depth_m":{"type":"number","minimum":0.06,"maximum":0.45}},
            ["source","start_face","end_face","width_m","depth_m"])
    safe_brush=conservative_schema_bounds(objects)
    anchor_distance=(safe_brush["distance"] if safe_brush else {"minimum":0.65,"maximum":1.8})
    anchor_radius=(safe_brush["radius"] if safe_brush else {"minimum":1.5,"maximum":8})
    defs={
        "sculpt_anchor":({"anchor":enum(["x_min","x_max","y_min","y_max","z_min","z_max"]),
                          "direction":enum(["+X","-X","+Y","-Y","+Z","-Z"]),
                          "distance":{"type":"number",**anchor_distance},
                          "radius":{"type":"number",**anchor_radius}},
                          ["anchor","direction","distance","radius"]),
        "sculpt":({"center":v3,"delta":v3,"radius":{"type":"number","minimum":0.2,"maximum":8}},
                  ["center","delta","radius"]),
        "extrude":({"faces":face_ids,"offset":v3},["faces","offset"]),
        "inset":({"faces":face_ids,"thickness":{"type":"number","minimum":0.005,"maximum":0.3}},
                 ["faces","thickness"]),
        "bevel":({"edges":{"type":"array","minItems":1,"maxItems":6,
                            "items":({"type":"integer","enum":observed_edges} if observed_edges else
                                     {"type":"integer","minimum":0,
                                      "maximum":max(0,max(x.get("edges",0) for x in objects)-1)})},
                  "width":{"type":"number","minimum":0.005,"maximum":0.09},
                  "segments":{"type":"integer","minimum":1,"maximum":2},
                  "apply":{"const":True}},["edges","width","segments","apply"]),
        "material":({"base_color":rgba,
                     "roughness":{"type":"number","minimum":0.45,"maximum":1},
                     "metallic":{"type":"number","minimum":0,"maximum":0.2}},
                     ["base_color","roughness","metallic"]),
        "uv":({"method":enum(["smart","cube","sphere","cylinder"])},["method"]),
        "modifier":({"kind":enum(modifier_kinds),
                     "options":modifier_options,
                     "apply":{"type":"boolean"}},["kind","options","apply"]),
        "native_operator":({"operator":enum(["object.shade_smooth","object.shade_flat",
                                           "mesh.poke","mesh.subdivide","uv.smart_project"]),
                            "kwargs":{"type":"object"},
                            "selection":{"const":"ALL"}},["operator","kwargs","selection"])
    }
    variants=[]
    for op,(properties,required) in new_ops.items():
        variants.append({"type":"object","additionalProperties":False,"properties":{
            "id":{"type":"string","minLength":2,"maxLength":60},
            "op":{"const":op},
            "target":{"type":"string","minLength":18,"maxLength":60,
                      "pattern":NEW_COMPONENT_PATTERN},
            "reason":{"type":"string","minLength":15,"maxLength":170},
            "params":{"type":"object","additionalProperties":False,
                      "properties":properties,"required":required}},
            "required":["id","op","target","reason","params"]})
    for op,(properties,required) in defs.items():
        variants.append({"type":"object","additionalProperties":False,"properties":{
            "id":{"type":"string","minLength":2,"maxLength":60},
            "op":{"const":op},
            "target":enum(names),
            "reason":{"type":"string","minLength":15,"maxLength":170},
            "params":{"type":"object","additionalProperties":False,
                      "properties":properties,"required":required}},
            "required":["id","op","target","reason","params"]})
    return {"oneOf":variants}
