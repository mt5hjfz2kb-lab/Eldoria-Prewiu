"""Extended native Blender modelling, shader nodes, and Geometry Nodes.

A Qwen action specifies the complete operator or node graph, not Python code.
The executor permits only harmless 3D editing surfaces and node types. Nothing
may access Blender's script/file-system operators or run arbitrary expressions.
"""
from __future__ import annotations
import math

SAFE_NATIVE_OPS={
 "object.shade_smooth","object.shade_flat","object.transform_apply",
 "mesh.bevel","mesh.inset","mesh.poke","mesh.subdivide",
 "mesh.tris_convert_to_quads","mesh.quads_convert_to_tris",
 "mesh.merge_by_distance","mesh.dissolve_limited","mesh.beautify_fill",
 "mesh.symmetrize","mesh.flip_normals","mesh.mark_seam","mesh.mark_sharp",
 "uv.unwrap","uv.smart_project","uv.cube_project",
 "uv.sphere_project","uv.cylinder_project",
}
SAFE_SHADER_NODES={
 "ShaderNodeOutputMaterial","ShaderNodeBsdfPrincipled","ShaderNodeTexNoise",
 "ShaderNodeTexVoronoi","ShaderNodeTexChecker","ShaderNodeTexCoord",
 "ShaderNodeMapping","ShaderNodeValToRGB","ShaderNodeBump",
 "ShaderNodeNormalMap","ShaderNodeMath","ShaderNodeVectorMath",
 "ShaderNodeMixRGB","ShaderNodeMix","ShaderNodeHueSaturation",
 "ShaderNodeRGB","ShaderNodeValue","ShaderNodeSeparateColor",
 "ShaderNodeCombineColor","ShaderNodeTexImage",
}
SAFE_GEOMETRY_NODES={
 "NodeGroupInput","NodeGroupOutput","GeometryNodeMeshCube",
 "GeometryNodeMeshCylinder","GeometryNodeMeshCone",
 "GeometryNodeMeshUVSphere","GeometryNodeMeshIcoSphere",
 "GeometryNodeMeshGrid","GeometryNodeMeshCircle",
 "GeometryNodeTransform","GeometryNodeSetPosition",
 "GeometryNodeJoinGeometry","GeometryNodeSetMaterial",
 "GeometryNodeSubdivisionSurface","GeometryNodeRealizeInstances",
 "GeometryNodeInstanceOnPoints","GeometryNodeDistributePointsOnFaces",
 "GeometryNodeMeshBoolean","GeometryNodeDeleteGeometry",
 "GeometryNodeInputPosition","GeometryNodeInputNormal",
 "GeometryNodeMeshToCurve","GeometryNodeCurveToMesh",
 "GeometryNodeCurvePrimitiveLine","GeometryNodeCurvePrimitiveCircle",
 "GeometryNodeResampleCurve","GeometryNodeSetShadeSmooth",
 "ShaderNodeMath","ShaderNodeVectorMath","FunctionNodeCompare",
}

def allowed_values(x,depth=0):
    if depth>5:return False
    if x is None or type(x) in (bool,int,float):
        return type(x) not in (int,float) or math.isfinite(x) and abs(x)<1000000
    if isinstance(x,str):
        return len(x)<=120 and not any(c in x for c in ("\n","\r",";","\\","/"))
    if isinstance(x,list):return len(x)<=64 and all(allowed_values(v,depth+1) for v in x)
    if isinstance(x,dict):
        return len(x)<=45 and all(isinstance(k,str) and len(k)<=100 and
                                  allowed_values(v,depth+1) for k,v in x.items())
    return False

def validate_extended(a, fail):
    name=a["op"];p=a["params"]
    if name=="native_operator":
        fail(p.get("operator") in SAFE_NATIVE_OPS,"Unsafe or unknown bpy operator")
        fail(allowed_values(p.get("kwargs",{})),"Unsafe Blender operator kwargs")
        fail(isinstance(p.get("kwargs",{}),dict),"Operator kwargs must be dict")
        fail(p.get("selection") in (None,"ALL","FACE","EDGE","VERT"),"Invalid selection context")
        if "indices" in p:
            fail(isinstance(p["indices"],list) and len(p["indices"])<=60000 and
                 all(type(i) is int and 0<=i<=200000 for i in p["indices"]),"Invalid element indices")
    else:
        nodes=p.get("nodes");links=p.get("links")
        fail(isinstance(nodes,list) and 1<=len(nodes)<=80,"Node graphs require 1..80 authored nodes")
        fail(isinstance(links,list) and len(links)<=160,"Node graphs maximum 160 links")
        allowed=SAFE_SHADER_NODES if name=="shader_graph" else SAFE_GEOMETRY_NODES
        ids=set()
        for n in nodes:
            fail(isinstance(n,dict),"Node must be object")
            fail(isinstance(n.get("id"),str) and len(n["id"])<90 and
                 n["id"] not in ids,"Node ID invalid/duplicate")
            ids.add(n["id"])
            fail(n.get("type") in allowed,"Node type disallowed")
            fail(allowed_values(n.get("inputs",{})) and isinstance(n.get("inputs",{}),dict),
                 "Node inputs invalid")
            if "image" in n:fail(isinstance(n["image"],str) and len(n["image"])<90,"Image datablock invalid")
        for link in links:
            fail(isinstance(link,dict),"Graph link must be object")
            fail(link.get("from") in ids and link.get("to") in ids,"Graph link refers to missing node")
            for key in ("output","input"):
                fail(type(link.get(key)) in (str,int) and
                     (type(link[key]) is int and 0<=link[key]<=40 or
                      type(link[key]) is str and 1<=len(link[key])<80),
                     "Invalid socket")
        if name=="shader_graph":
            fail(any(n["type"]=="ShaderNodeOutputMaterial" for n in nodes),
                 "Shader graph requires output material")
        if name=="geometry_nodes":
            fail(any(n["type"]=="NodeGroupOutput" for n in nodes),
                 "Geometry Nodes require a real group output")

def _active(obj):
    import bpy
    if bpy.context.object and bpy.context.object.mode!="OBJECT":
        bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active=obj

def _set_inputs(node,defaults):
    for label,value in defaults.items():
        socket=node.inputs.get(label) if isinstance(label,str) else node.inputs[int(label)]
        if socket is None:raise ValueError("Blender node socket missing: "+str(label))
        if not hasattr(socket,"default_value"):
            raise ValueError("Socket is not numerical/color configurable")
        if type(value) in (int,float,bool):
            socket.default_value=value
        elif isinstance(value,list):
            cur=socket.default_value
            if not hasattr(cur,"__len__") or len(value)!=len(cur):
                raise ValueError("Node input vector dimensionality mismatch")
            socket.default_value=value
        else:raise ValueError("Only numerical artistic node inputs allowed")

def execute_node_graph(obj,p,kind):
    import bpy
    if kind=="shader_graph":
        group=bpy.data.materials.new("Eldoria_AI_shader_network_"+obj.name)
        group.use_nodes=True;tree=group.node_tree;tree.nodes.clear()
    else:
        group=bpy.data.node_groups.new("Eldoria_AI_geometry_network_"+obj.name,"GeometryNodeTree")
        group.interface.new_socket(name="Geometry",in_out="INPUT",socket_type="NodeSocketGeometry")
        group.interface.new_socket(name="Geometry",in_out="OUTPUT",socket_type="NodeSocketGeometry")
        tree=group
    nodes={}
    for n in p["nodes"]:
        node=tree.nodes.new(n["type"]);nodes[n["id"]]=node
        node.label=n["id"]
        if n.get("image"):
            image=bpy.data.images.get(n["image"])
            if image is None:raise ValueError("Image datablock does not exist")
            if not hasattr(node,"image"):raise ValueError("Non-image Blender node")
            node.image=image
        _set_inputs(node,n.get("inputs",{}))
    for spec in p["links"]:
        from_node=nodes[spec["from"]];to_node=nodes[spec["to"]]
        out=from_node.outputs[spec["output"]]
        inp=to_node.inputs[spec["input"]]
        tree.links.new(out,inp)
    if kind=="shader_graph":
        obj.data.materials.clear();obj.data.materials.append(group)
    else:
        mod=obj.modifiers.new("Eldoria_AI_Author_Directed_Geometry_Nodes","NODES")
        mod.node_group=group
    return group

def native_bpy(obj,p):
    import bpy,bmesh
    op=p["operator"]
    namespace,op_name=op.split(".")
    _active(obj)
    edit=namespace in ("mesh","uv")
    if edit:
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="DESELECT")
        mode=p.get("selection","ALL")
        if mode=="ALL":
            bpy.ops.mesh.select_all(action="SELECT")
        else:
            bpy.ops.mesh.select_mode(type={"FACE":"FACE","EDGE":"EDGE","VERT":"VERT"}[mode])
            bm=bmesh.from_edit_mesh(obj.data)
            slots={"FACE":bm.faces,"EDGE":bm.edges,"VERT":bm.verts}[mode]
            slots.ensure_lookup_table()
            idx=p.get("indices",[])
            if not idx:raise ValueError("Specific native edit selection requires indices")
            if max(idx)>=len(slots):raise ValueError("AI selected nonexistent geometry index")
            for i in idx:slots[i].select_set(True)
            bmesh.update_edit_mesh(obj.data)
    try:
        getattr(getattr(bpy.ops,namespace),op_name)(**p.get("kwargs",{}))
    finally:
        if edit:bpy.ops.object.mode_set(mode="OBJECT")
