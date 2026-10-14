"""Small-model-safe, operation-discriminated schemas for real Blender artistry.

Qwen independently chooses its operations and ALL numeric design parameters.
Schemas only encode verified executable tool signatures and inspected bounds.
This is a progressive subset of the Blender toolbox, NOT all Blender.
"""
def source_artist_action_schema(objects):
    names=[x["name"] for x in objects]
    if not names:raise ValueError("Inspected existing Blender objects required")
    face_max=max(0,max(x["faces"] for x in objects)-1)
    face_ids={"type":"array","minItems":1,"maxItems":3,
              "items":{"type":"integer","minimum":0,"maximum":face_max}}
    # Enumerate only facts measured in actual Blender. Qwen still decides
    # WHICH available face/edge to modify from visible source evidence.
    # Never fabricate an index or substitute it after generation.
    observed_faces=sorted({int(item["face_index"]) for obj in objects
                           for item in obj.get("face_landmarks",[])
                           if type(item.get("face_index")) is int and
                           0 <= item["face_index"] < obj.get("faces",0)})
    observed_edges=sorted({int(item["edge_index"]) for obj in objects
                           for item in obj.get("edge_landmarks",[])
                           if type(item.get("edge_index")) is int and
                           0 <= item["edge_index"] < obj.get("edges",0)})
    if observed_faces:
        face_ids["items"]={"type":"integer","enum":observed_faces}
    numeric={"type":"number"}
    v3={"type":"array","minItems":3,"maxItems":3,"items":numeric}
    rgba={"type":"array","minItems":4,"maxItems":4,
          "items":{"type":"number","minimum":0,"maximum":1}}
    enum=lambda x:{"type":"string","enum":list(x)}
    defs={
        "sculpt_anchor":({"anchor":enum(["x_min","x_max","y_min","y_max","z_min","z_max"]),
                          "direction":enum(["+X","-X","+Y","-Y","+Z","-Z"]),
                          "distance":{"type":"number","minimum":0.65,"maximum":1.8},
                          "radius":{"type":"number","minimum":1.5,"maximum":8}},
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
        "modifier":({"kind":enum(["BEVEL","SOLIDIFY","WEIGHTED_NORMAL","DECIMATE","DISPLACE","SUBSURF"]),
                     "options":{"type":"object"},
                     "apply":{"type":"boolean"}},["kind","options","apply"]),
        "native_operator":({"operator":enum(["object.shade_smooth","object.shade_flat",
                                           "mesh.poke","mesh.subdivide","uv.smart_project"]),
                            "kwargs":{"type":"object"},
                            "selection":{"const":"ALL"}},["operator","kwargs","selection"])
    }
    variants=[]
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
