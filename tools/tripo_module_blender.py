import argparse, json, os, sys
import bpy
import mathutils

DEFAULT_TARGET = 49800
DEFAULT_MIN_TRIS = 49500
DEFAULT_MAX_TRIS = 50000

def parse_args():
    argv = sys.argv
    argv = argv[argv.index("--")+1:] if "--" in argv else []
    p = argparse.ArgumentParser()
    p.add_argument("--input", required=True)
    p.add_argument("--output", required=True)
    p.add_argument("--report", required=True)
    p.add_argument("--refine-config", default="")
    p.add_argument("--target-tris", type=int, default=DEFAULT_TARGET)
    p.add_argument("--min-tris", type=int, default=DEFAULT_MIN_TRIS)
    p.add_argument("--max-tris", type=int, default=DEFAULT_MAX_TRIS)
    p.add_argument("--diagnostic-only", action="store_true")
    p.add_argument("--surface-rescue-profile", choices=["", "rock"], default="")
    p.add_argument("--split-components-dir", default="")
    p.add_argument("--split-min-triangles", type=int, default=250)
    p.add_argument("--split-cluster-count", type=int, default=0)
    p.add_argument("--split-cluster-axes", choices=["xy","xz","yz"], default="xy")
    p.add_argument("--split-salvage-config", default="")
    return p.parse_args(argv)

def mesh_objects():
    return [o for o in bpy.context.scene.objects if o.type == "MESH" and o.data is not None]

def tri_count(obj):
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)

def metrics(label):
    objs = mesh_objects()
    mats = set()
    verts = tris = 0
    uv_ok = True
    normals_ok = True
    for o in objs:
        verts += len(o.data.vertices)
        tris += tri_count(o)
        uv_ok = uv_ok and bool(o.data.uv_layers)
        normals_ok = normals_ok and len(o.data.polygons) > 0
        for m in o.data.materials:
            if m: mats.add(m.name)
    bounds = []
    if objs:
        for axis in range(3):
            values = [(o.matrix_world @ v.co)[axis] for o in objs for v in o.data.vertices]
            bounds.append(round(max(values) - min(values), 6))
    return {
        "label": label,
        "objects": len(objs),
        "vertices": verts,
        "triangles": tris,
        "materials": len(mats),
        "images": len([i for i in bpy.data.images if i.source == "FILE"]),
        "bounds": bounds,
        "uv_present_all_meshes": uv_ok,
        "normals_present": normals_ok,
    }

def ensure_materials_and_uvs():
    neutral = bpy.data.materials.get("Eldoria_Module_Neutral")
    if neutral is None:
        neutral = bpy.data.materials.new("Eldoria_Module_Neutral")
        neutral.diffuse_color = (0.46, 0.44, 0.40, 1.0)
    for o in mesh_objects():
        if not o.data.materials:
            o.data.materials.append(neutral)
        if not o.data.uv_layers:
            bpy.context.view_layer.objects.active = o
            o.select_set(True)
            bpy.ops.object.mode_set(mode="EDIT")
            bpy.ops.mesh.select_all(action="SELECT")
            bpy.ops.uv.smart_project(angle_limit=1.15192, island_margin=0.02)
            bpy.ops.object.mode_set(mode="OBJECT")
            o.select_set(False)

def apply_surface_rescue(profile):
    if not profile:
        return {"enabled": False}
    if profile != "rock":
        raise RuntimeError(f"Unsupported surface rescue profile: {profile}")

    # Deterministic, texture-backed PBR rescue for rock/terrain-only support assets.
    # This does not segment or alter geometry. Complex mixed architecture must use
    # a later semantic-material pass rather than pretending one rock material fits all.
    import numpy as np
    size = 512
    y, x = np.mgrid[0:size, 0:size].astype(np.float32)
    u = x / float(size - 1)
    v = y / float(size - 1)

    height = (
        0.50
        + 0.18 * np.sin(u * 13.0 + v * 7.0)
        + 0.11 * np.sin(u * 31.0 - v * 19.0)
        + 0.07 * np.cos(u * 67.0 + v * 43.0)
    )
    height = np.clip(height, 0.0, 1.0)

    base_dark = np.array([0.22, 0.235, 0.225], dtype=np.float32)
    base_light = np.array([0.36, 0.355, 0.325], dtype=np.float32)
    mix = height[..., None]
    rgb = base_dark + (base_light - base_dark) * mix
    rgb *= (0.94 + 0.06 * np.sin((u + v) * 18.0))[..., None]
    rgb = np.clip(rgb, 0.0, 1.0)

    rough = np.clip(0.72 + (1.0 - height) * 0.20, 0.72, 0.94)

    gy, gx = np.gradient(height)
    strength = 3.2
    nx = -gx * strength
    ny = -gy * strength
    nz = np.ones_like(height)
    length = np.sqrt(nx * nx + ny * ny + nz * nz)
    nx, ny, nz = nx / length, ny / length, nz / length
    normal = np.stack((nx * 0.5 + 0.5, ny * 0.5 + 0.5, nz * 0.5 + 0.5), axis=-1)

    def make_image(name, data, colorspace):
        image = bpy.data.images.new(name, width=size, height=size, alpha=True)
        alpha = np.ones((size, size, 1), dtype=np.float32)
        rgba = np.concatenate((data, alpha), axis=-1) if data.ndim == 3 else np.concatenate((np.repeat(data[...,None], 3, axis=2), alpha), axis=-1)
        image.pixels.foreach_set(rgba.astype(np.float32).ravel())
        image.colorspace_settings.name = colorspace
        image.pack()
        return image

    base_img = make_image("Eldoria_Rock_SurfaceV1_BaseColor", rgb, "sRGB")
    rough_img = make_image("Eldoria_Rock_SurfaceV1_Roughness", rough, "Non-Color")
    normal_img = make_image("Eldoria_Rock_SurfaceV1_Normal", normal, "Non-Color")

    mat = bpy.data.materials.new("Eldoria Rock · Surface v1 Rescue")
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    links = mat.node_tree.links
    nodes.clear()

    out = nodes.new("ShaderNodeOutputMaterial")
    bsdf = nodes.new("ShaderNodeBsdfPrincipled")
    base_node = nodes.new("ShaderNodeTexImage")
    base_node.image = base_img
    rough_node = nodes.new("ShaderNodeTexImage")
    rough_node.image = rough_img
    normal_tex = nodes.new("ShaderNodeTexImage")
    normal_tex.image = normal_img
    normal_map = nodes.new("ShaderNodeNormalMap")
    normal_map.inputs["Strength"].default_value = 0.45

    links.new(base_node.outputs["Color"], bsdf.inputs["Base Color"])
    links.new(rough_node.outputs["Color"], bsdf.inputs["Roughness"])
    links.new(normal_tex.outputs["Color"], normal_map.inputs["Color"])
    links.new(normal_map.outputs["Normal"], bsdf.inputs["Normal"])
    links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    if "Metallic" in bsdf.inputs:
        bsdf.inputs["Metallic"].default_value = 0.0
    if "Specular IOR Level" in bsdf.inputs:
        bsdf.inputs["Specular IOR Level"].default_value = 0.24

    for o in mesh_objects():
        o.data.materials.clear()
        o.data.materials.append(mat)

    return {
        "enabled": True,
        "profile": profile,
        "texture_size": size,
        "material": mat.name,
        "textures": [base_img.name, rough_img.name, normal_img.name],
        "geometry_modified": False,
    }

def load_refine_config(path):
    if not path:
        return {}
    if not os.path.isfile(path):
        raise FileNotFoundError(path)
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)

def connected_component_report():
    report = []
    for o in mesh_objects():
        mesh = o.data
        adjacency = [set() for _ in mesh.vertices]
        for e in mesh.edges:
            a, b = e.vertices
            adjacency[a].add(b)
            adjacency[b].add(a)
        unseen = set(range(len(mesh.vertices)))
        components = []
        while unseen:
            seed = unseen.pop()
            stack = [seed]
            count = 0
            while stack:
                v = stack.pop()
                count += 1
                for n in adjacency[v]:
                    if n in unseen:
                        unseen.remove(n)
                        stack.append(n)
            components.append(count)
        components.sort(reverse=True)
        report.append({
            "object": o.name,
            "component_count": len(components),
            "largest_vertex_counts": components[:20],
        })
    return report

def apply_visual_refinement(cfg):
    if not cfg or not cfg.get("enabled", False):
        return {"enabled": False}
    result = {
        "enabled": True,
        "mode": cfg.get("mode", "diagnostic_material_cleanup"),
        "components_before": connected_component_report(),
        "material_changes": [],
        "mesh_cleanup": [],
    }
    merge_distance = float(cfg.get("merge_distance", 0.00005))
    recalc_normals = bool(cfg.get("recalculate_normals", True))
    for o in mesh_objects():
        bpy.context.view_layer.objects.active = o
        o.select_set(True)
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        before = len(o.data.vertices)
        if merge_distance > 0:
            bpy.ops.mesh.remove_doubles(threshold=merge_distance)
        if recalc_normals:
            bpy.ops.mesh.normals_make_consistent(inside=False)
        bpy.ops.object.mode_set(mode="OBJECT")
        after = len(o.data.vertices)
        o.select_set(False)
        result["mesh_cleanup"].append({
            "object": o.name,
            "vertices_before": before,
            "vertices_after": after,
            "merged_vertices": max(0, before-after),
        })
    roughness_floor = float(cfg.get("roughness_floor", 0.72))
    metallic_ceiling = float(cfg.get("metallic_ceiling", 0.08))
    specular_ior_level = float(cfg.get("specular_ior_level", 0.28))
    for mat in bpy.data.materials:
        if not mat or not mat.use_nodes or not mat.node_tree:
            continue
        for node in mat.node_tree.nodes:
            if node.type != "BSDF_PRINCIPLED":
                continue
            change = {"material": mat.name}
            if "Roughness" in node.inputs:
                old = float(node.inputs["Roughness"].default_value)
                node.inputs["Roughness"].default_value = max(old, roughness_floor)
                change["roughness"] = [old, float(node.inputs["Roughness"].default_value)]
            if "Metallic" in node.inputs:
                old = float(node.inputs["Metallic"].default_value)
                node.inputs["Metallic"].default_value = min(old, metallic_ceiling)
                change["metallic"] = [old, float(node.inputs["Metallic"].default_value)]
            if "Specular IOR Level" in node.inputs:
                old = float(node.inputs["Specular IOR Level"].default_value)
                node.inputs["Specular IOR Level"].default_value = min(old, specular_ior_level)
                change["specular_ior_level"] = [old, float(node.inputs["Specular IOR Level"].default_value)]
            result["material_changes"].append(change)
    result["components_after"] = connected_component_report()
    return result

def surface_diagnostics():
    images = []
    for image in bpy.data.images:
        if image.size[0] <= 0 or image.size[1] <= 0:
            continue
        images.append({
            "name": image.name,
            "source": image.source,
            "width": int(image.size[0]),
            "height": int(image.size[1]),
            "colorspace": getattr(getattr(image, "colorspace_settings", None), "name", ""),
            "packed": bool(image.packed_file),
            "filepath": image.filepath or "",
            "inferred_role": infer_image_role(image),
        })

    materials = []
    for mat in bpy.data.materials:
        if not mat:
            continue
        entry = {
            "name": mat.name,
            "use_nodes": bool(mat.use_nodes),
            "principled": [],
            "image_textures": [],
        }
        if mat.use_nodes and mat.node_tree:
            for node in mat.node_tree.nodes:
                if node.type == "BSDF_PRINCIPLED":
                    values = {}
                    for key in ("Base Color", "Metallic", "Roughness", "Specular IOR Level", "Alpha"):
                        if key in node.inputs and not node.inputs[key].is_linked:
                            value = node.inputs[key].default_value
                            if hasattr(value, "__len__"):
                                value = [float(v) for v in value]
                            else:
                                value = float(value)
                            values[key] = value
                    entry["principled"].append(values)
                elif node.type == "TEX_IMAGE" and getattr(node, "image", None):
                    targets = []
                    for output in node.outputs:
                        for link in output.links:
                            targets.append({
                                "from_socket": output.name,
                                "to_node": link.to_node.name,
                                "to_socket": link.to_socket.name,
                            })
                    entry["image_textures"].append({
                        "node": node.name,
                        "image": node.image.name,
                        "colorspace": node.image.colorspace_settings.name,
                        "targets": targets,
                    })
        materials.append(entry)
    return {"images": images, "materials": materials}

def infer_image_role(image):
    name = image.name.lower()
    path = (image.filepath or "").lower()
    text = name + " " + path
    if any(k in text for k in ("basecolor", "base_color", "albedo", "diffuse", "color")):
        return "basecolor"
    if any(k in text for k in ("normal", "nrm")):
        return "normal"
    if any(k in text for k in ("rough", "smooth")):
        return "roughness"
    if any(k in text for k in ("metal", "metallic")):
        return "metallic"
    if any(k in text for k in ("occlusion", "ambientocclusion", "_ao", " ao")):
        return "occlusion"
    if any(k in text for k in ("mask", "orm", "rma", "mra", "_rm", " rm", "roughmetal", "roughnessmetallic")):
        return "mask"
    return "unknown"

def optimize_images():
    resized = []
    for image in bpy.data.images:
        if image.source != "FILE" or image.size[0] <= 0 or image.size[1] <= 0:
            continue
        role = infer_image_role(image)
        # Preserve form-defining color/normal detail longer; scalar/mask maps can usually
        # tolerate a lower authoring cap for the fixed isometric/mobile camera.
        limit = 2048 if role in ("basecolor", "normal") else 1024
        before = tuple(image.size)
        factor = min(1.0, limit / max(before))
        if factor < 1.0:
            image.scale(max(1, round(before[0] * factor)), max(1, round(before[1] * factor)))
            image.pack()
        resized.append({
            "name": image.name,
            "role": role,
            "limit": limit,
            "before": before,
            "after": tuple(image.size)
        })
    return resized

def apply_decimation_once(target):
    objs = mesh_objects()
    current = sum(tri_count(o) for o in objs)
    if current <= target:
        return current
    ratio = max(0.001, min(1.0, target / float(current)))
    for o in objs:
        before = tri_count(o)
        if before <= 20:
            continue
        bpy.context.view_layer.objects.active = o
        o.select_set(True)
        mod = o.modifiers.new(name="Eldoria_AutoDecimate", type="DECIMATE")
        mod.decimate_type = "COLLAPSE"
        mod.ratio = ratio
        mod.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=mod.name)
        o.select_set(False)
    return sum(tri_count(o) for o in mesh_objects())

def _world_centroid(obj):
    verts = [obj.matrix_world @ v.co for v in obj.data.vertices]
    if not verts:
        return mathutils.Vector((0.0, 0.0, 0.0))
    acc = mathutils.Vector((0.0, 0.0, 0.0))
    for v in verts:
        acc += v
    return acc / len(verts)

def _cluster_loose_parts(candidates, cluster_count, axes="xy"):
    axis_map = {"x":0,"y":1,"z":2}
    if axes not in ("xy","xz","yz"):
        raise RuntimeError(f"Unsupported cluster axes: {axes}")
    a0, a1 = axis_map[axes[0]], axis_map[axes[1]]
    def plane_point(center):
        return mathutils.Vector((center[a0], center[a1]))
    rows = []
    noise_floor = 5
    usable = []
    for o in candidates:
        tris = tri_count(o)
        center = _world_centroid(o)
        rows.append({
            "object": o,
            "name": o.name,
            "triangles": tris,
            "center": center,
            "usable": tris >= noise_floor,
        })
        if tris >= noise_floor:
            usable.append(rows[-1])

    if len(usable) < cluster_count:
        raise RuntimeError(f"Multipiece spatial clustering needs at least {cluster_count} usable islands; found {len(usable)}")

    # Cluster disconnected islands on an explicit sheet plane. Different image-to-3D
    # exports may map the source sheet to XY or XZ; keep this configurable instead of
    # assuming Y is always image-space vertical.
    first = max(usable, key=lambda r: r["triangles"])
    centers = [plane_point(first["center"])]
    while len(centers) < cluster_count:
        def seed_score(row):
            p = plane_point(row["center"])
            d2 = min((p-c).length_squared for c in centers)
            return d2 * max(1.0, row["triangles"] ** 0.5)
        nxt = max(usable, key=seed_score)
        centers.append(mathutils.Vector((nxt["center"].x, nxt["center"].y)))

    assignments = [0] * len(usable)
    for _ in range(16):
        for i, row in enumerate(usable):
            p = plane_point(row["center"])
            assignments[i] = min(range(cluster_count), key=lambda k: (p-centers[k]).length_squared)

        new_centers = []
        for k in range(cluster_count):
            members = [usable[i] for i, a in enumerate(assignments) if a == k]
            if not members:
                # Re-seed an empty cluster with the point farthest from every current center.
                row = max(usable, key=lambda r: min(
                    (plane_point(r["center"])-c).length_squared for c in centers))
                new_centers.append(mathutils.Vector((row["center"].x, row["center"].y)))
                continue
            total = sum(max(1, m["triangles"]) for m in members)
            u = sum(plane_point(m["center"]).x * max(1, m["triangles"]) for m in members) / total
            v = sum(plane_point(m["center"]).y * max(1, m["triangles"]) for m in members) / total
            new_centers.append(mathutils.Vector((u, v)))
        if all((new_centers[k]-centers[k]).length < 1e-7 for k in range(cluster_count)):
            centers = new_centers
            break
        centers = new_centers

    groups = []
    for k in range(cluster_count):
        members = [usable[i] for i, a in enumerate(assignments) if a == k]
        groups.append({
            "center": centers[k],
            "members": members,
            "triangles": sum(m["triangles"] for m in members),
        })
    # Stable sheet order on the selected sheet plane: top-to-bottom, left-to-right.
    groups.sort(key=lambda g: (-g["center"].y, g["center"].x))
    return rows, groups

def _join_group(members, name):
    bpy.ops.object.select_all(action="DESELECT")
    for row in members:
        row["object"].select_set(True)
    active = max(members, key=lambda r: r["triangles"])["object"]
    bpy.context.view_layer.objects.active = active
    if len(members) > 1:
        bpy.ops.object.join()
    active = bpy.context.view_layer.objects.active
    active.name = name
    return active

def _normalize_bottom_center(obj):
    world = [obj.matrix_world @ v.co for v in obj.data.vertices]
    minx = min(v.x for v in world); maxx = max(v.x for v in world)
    miny = min(v.y for v in world); maxy = max(v.y for v in world)
    minz = min(v.z for v in world); maxz = max(v.z for v in world)
    center = ((minx + maxx) * 0.5, (miny + maxy) * 0.5, minz)
    inv = obj.matrix_world.inverted()
    local_center = inv @ mathutils.Vector(center)
    for v in obj.data.vertices:
        v.co -= local_center
    obj.location = (0.0, 0.0, 0.0)
    return [round(maxx-minx,6), round(maxy-miny,6), round(maxz-minz,6)]

def _salvage_group_members(members, cfg, group_index):
    if not cfg or not cfg.get("enabled", False):
        return members, {"enabled": False}
    selected = set(int(x) for x in cfg.get("piece_indices", []))
    if selected and int(group_index) not in selected:
        return members, {"enabled": False, "reason": "piece_not_selected"}
    rows = sorted([(row, int(row["triangles"])) for row in members], key=lambda x: x[1], reverse=True)
    if not rows:
        return members, {"enabled": True, "removed": [], "kept": []}
    largest = rows[0][1]
    ratio = float(cfg.get("min_component_ratio", 0.08))
    floor = int(cfg.get("min_component_triangles", 120))
    threshold = max(floor, int(round(largest * ratio)))
    kept=[]; removed=[]
    for row,t in rows:
        if t >= threshold: kept.append(row)
        else: removed.append((row,t))
    # Never erase a whole clustered piece.
    if not kept:
        kept=[rows[0][0]]
        removed=rows[1:]
    for row,t in removed:
        bpy.data.objects.remove(row["object"], do_unlink=True)
    return kept, {
        "enabled": True,
        "policy": "disconnected_residue_only",
        "threshold_triangles": threshold,
        "largest_component_triangles": largest,
        "kept_components": len(kept),
        "removed_components": len(removed),
        "removed_triangles": sum(t for _,t in removed),
    }


def _post_join_cleanup(obj, cfg, group_index):
    post = ((cfg or {}).get("post_join_cleanup", {}) or {}).get(str(int(group_index)))
    if not post:
        return obj, {"enabled": False}
    min_triangles = int(post.get("min_triangles", 0) or 0)
    drop_ratio = post.get("drop_if_min_y_above_ratio", None)
    spatial_neighbor_ratio = post.get("spatial_neighbor_ratio", None)
    axis_clip = post.get("axis_clip", None)

    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.separate(type="LOOSE")
    bpy.ops.object.mode_set(mode="OBJECT")
    parts = [o for o in bpy.context.selected_objects if o.type == "MESH" and o.data is not None]
    if not parts:
        parts = [obj]

    def world_bounds(part):
        verts = [part.matrix_world @ v.co for v in part.data.vertices]
        mins = [min(v[a] for v in verts) for a in range(3)]
        maxs = [max(v[a] for v in verts) for a in range(3)]
        return mins, maxs

    def bbox_gap(a, b):
        sq = 0.0
        for axis in range(3):
            if a["maxs"][axis] < b["mins"][axis]:
                d = b["mins"][axis] - a["maxs"][axis]
            elif b["maxs"][axis] < a["mins"][axis]:
                d = a["mins"][axis] - b["maxs"][axis]
            else:
                d = 0.0
            sq += d * d
        return sq ** 0.5

    rows = []
    for part in parts:
        mins, maxs = world_bounds(part)
        rows.append({
            "object": part,
            "triangles": tri_count(part),
            "mins": mins,
            "maxs": maxs,
        })

    y_min = min(r["mins"][1] for r in rows)
    y_max = max(r["maxs"][1] for r in rows)
    y_cut = None
    if drop_ratio is not None:
        ratio = float(drop_ratio)
        if ratio <= 0.0 or ratio >= 1.0:
            raise RuntimeError(f"Invalid drop_if_min_y_above_ratio for piece {group_index}: {ratio}")
        y_cut = y_min + (y_max - y_min) * ratio

    spatial_keep = None
    spatial_threshold = None
    if spatial_neighbor_ratio is not None:
        ratio = float(spatial_neighbor_ratio)
        if ratio <= 0.0 or ratio >= 1.0:
            raise RuntimeError(f"Invalid spatial_neighbor_ratio for piece {group_index}: {ratio}")
        mins_all = [min(r["mins"][a] for r in rows) for a in range(3)]
        maxs_all = [max(r["maxs"][a] for r in rows) for a in range(3)]
        span = max(maxs_all[a] - mins_all[a] for a in range(3))
        spatial_threshold = max(1e-6, span * ratio)
        strongest = max(rows, key=lambda r: r["triangles"])
        spatial_keep = {id(strongest)}
        frontier = [strongest]
        while frontier:
            current = frontier.pop()
            for other in rows:
                if id(other) in spatial_keep:
                    continue
                if bbox_gap(current, other) <= spatial_threshold:
                    spatial_keep.add(id(other))
                    frontier.append(other)

    kept = []
    removed = []
    for row in rows:
        reason = None
        if row["triangles"] < min_triangles:
            reason = "below_post_join_min_triangles"
        elif y_cut is not None and row["mins"][1] > y_cut:
            reason = "detached_upper_island"
        elif spatial_keep is not None and id(row) not in spatial_keep:
            reason = "spatial_outlier"
        if reason:
            removed.append((row, reason))
        else:
            kept.append(row)

    if not kept:
        strongest = max(rows, key=lambda r: r["triangles"])
        kept = [strongest]
        removed = [(r, "fallback_removed") for r in rows if r is not strongest]

    for row, _ in removed:
        bpy.data.objects.remove(row["object"], do_unlink=True)

    bpy.ops.object.select_all(action="DESELECT")
    for row in kept:
        row["object"].select_set(True)
    active = max(kept, key=lambda r: r["triangles"])["object"]
    bpy.context.view_layer.objects.active = active
    if len(kept) > 1:
        bpy.ops.object.join()
    active = bpy.context.view_layer.objects.active
    active.name = f"Eldoria_MultiPiece_Group_{int(group_index):02d}"

    clip_report = {"enabled": False}
    if axis_clip:
        axis_name = str(axis_clip.get("axis", "x")).lower()
        axis_index = {"x": 0, "y": 1, "z": 2}.get(axis_name)
        if axis_index is None:
            raise RuntimeError(f"Invalid axis_clip.axis for piece {group_index}: {axis_name}")
        side = str(axis_clip.get("side", "min")).lower()
        fraction = float(axis_clip.get("fraction", 0.0))
        if side not in ("min", "max") or fraction <= 0.0 or fraction >= 0.45:
            raise RuntimeError(f"Invalid axis_clip for piece {group_index}: side={side} fraction={fraction}")
        verts = [active.matrix_world @ v.co for v in active.data.vertices]
        lo = min(v[axis_index] for v in verts)
        hi = max(v[axis_index] for v in verts)
        cut = lo + (hi - lo) * fraction if side == "min" else hi - (hi - lo) * fraction

        bpy.ops.object.select_all(action="DESELECT")
        active.select_set(True)
        bpy.context.view_layer.objects.active = active
        bpy.ops.object.mode_set(mode="OBJECT")
        selected = 0
        for v in active.data.vertices:
            world = active.matrix_world @ v.co
            remove = world[axis_index] < cut if side == "min" else world[axis_index] > cut
            v.select = bool(remove)
            if remove:
                selected += 1
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_mode(type="VERT")
        bpy.ops.mesh.delete(type="VERT")
        bpy.ops.object.mode_set(mode="OBJECT")
        if len(active.data.polygons) == 0:
            raise RuntimeError(f"axis_clip erased piece {group_index}")
        clip_report = {
            "enabled": True,
            "axis": axis_name,
            "side": side,
            "fraction": fraction,
            "cut": round(float(cut), 6),
            "selected_vertices": selected,
        }

    return active, {
        "enabled": True,
        "policy": "post_join_architecture_cleanup",
        "min_triangles": min_triangles,
        "drop_if_min_y_above_ratio": drop_ratio,
        "spatial_neighbor_ratio": spatial_neighbor_ratio,
        "spatial_threshold": round(float(spatial_threshold), 6) if spatial_threshold is not None else None,
        "axis_clip": clip_report,
        "y_cut": round(float(y_cut), 6) if y_cut is not None else None,
        "kept_components": len(kept),
        "removed_components": len(removed),
        "removed_triangles": sum(r["triangles"] for r, _ in removed),
        "removed": [{
            "triangles": r["triangles"],
            "reason": reason,
            "mins": [round(float(v), 6) for v in r["mins"]],
            "maxs": [round(float(v), 6) for v in r["maxs"]],
        } for r, reason in removed],
    }

def split_components_to_glbs(output_dir, min_triangles=250, cluster_count=0, salvage_cfg=None):
    if not output_dir:
        return {"enabled": False, "pieces": []}
    os.makedirs(output_dir, exist_ok=True)

    objs = mesh_objects()
    if not objs:
        raise RuntimeError("No mesh objects available for multipiece separation")
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    if len(objs) > 1:
        bpy.ops.object.join()
    joined = bpy.context.view_layer.objects.active
    joined.name = "Eldoria_MultiPiece_Source"

    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.separate(type="LOOSE")
    bpy.ops.object.mode_set(mode="OBJECT")
    candidates = mesh_objects()

    if int(cluster_count) > 0:
        island_rows, groups = _cluster_loose_parts(candidates, int(cluster_count), cluster_axes)
        exported = []
        group_rows = []
        for index, group in enumerate(groups, start=1):
            members, salvage = _salvage_group_members(group["members"], salvage_cfg, index)
            obj = _join_group(members, f"Eldoria_MultiPiece_Group_{index:02d}")
            obj, post_join_cleanup = _post_join_cleanup(obj, salvage_cfg, index)
            if isinstance(salvage, dict):
                salvage["post_join_cleanup"] = post_join_cleanup
            tris = tri_count(obj)
            bounds = _normalize_bottom_center(obj)
            safe = f"piece_{index:02d}_{tris}tris.glb"
            path_out = os.path.join(output_dir, safe)
            bpy.ops.object.select_all(action="DESELECT")
            obj.select_set(True)
            bpy.context.view_layer.objects.active = obj
            bpy.ops.export_scene.gltf(
                filepath=path_out,
                export_format="GLB",
                export_apply=True,
                export_materials="EXPORT",
                export_yup=True,
                use_selection=True,
            )
            exported.append({
                "index": index,
                "source_object": obj.name,
                "file": safe,
                "triangles": tris,
                "bytes": os.path.getsize(path_out),
            })
            group_rows.append({
                "index": index,
                "island_count": len(group["members"]),
                "triangles": tris,
                "bounds": bounds,
                "sheet_center_xy": [round(group["center"].x,6), round(group["center"].y,6)],
                "salvage": salvage,
            })

        return {
            "enabled": True,
            "mode": "spatial_clusters",
            "expected_piece_count": int(cluster_count),
            "cluster_axes": cluster_axes,
            "candidate_islands": len(candidates),
            "clustered_islands": sum(len(g["members"]) for g in groups),
            "useful_piece_count": len(exported),
            "pieces": exported,
            "groups": group_rows,
            "all_islands": [{
                "name": row["name"],
                "triangles": row["triangles"],
                "usable_for_clustering": row["usable"],
                "center_xy": [round(row["center"].x,6), round(row["center"].y,6)]
            } for row in island_rows],
        }

    piece_rows = []
    useful = []
    for o in candidates:
        tris = tri_count(o)
        if tris < int(min_triangles):
            piece_rows.append({
                "name": o.name,
                "triangles": tris,
                "useful": False,
                "reason": "below_split_min_triangles"
            })
            continue
        bounds = _normalize_bottom_center(o)
        useful.append((o, tris))
        piece_rows.append({
            "name": o.name,
            "triangles": tris,
            "useful": True,
            "bounds": bounds
        })

    useful.sort(key=lambda item: item[1], reverse=True)
    exported = []
    for index, (o, tris) in enumerate(useful, start=1):
        bpy.ops.object.select_all(action="DESELECT")
        o.select_set(True)
        bpy.context.view_layer.objects.active = o
        safe = f"piece_{index:02d}_{tris}tris.glb"
        path_out = os.path.join(output_dir, safe)
        bpy.ops.export_scene.gltf(
            filepath=path_out,
            export_format="GLB",
            export_apply=True,
            export_materials="EXPORT",
            export_yup=True,
            use_selection=True,
        )
        exported.append({
            "index": index,
            "source_object": o.name,
            "file": safe,
            "triangles": tris,
            "bytes": os.path.getsize(path_out)
        })

    return {
        "enabled": True,
        "mode": "connected_components",
        "min_triangles": int(min_triangles),
        "candidate_islands": len(candidates),
        "useful_piece_count": len(exported),
        "pieces": exported,
        "all_islands": piece_rows,
    }

def main():
    a = parse_args()
    if not os.path.isfile(a.input):
        raise FileNotFoundError(a.input)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=a.input)
    raw = metrics("raw_import")
    if a.diagnostic_only:
        report = {
            "mode": "diagnostic_only",
            "raw": raw,
            "surface_diagnostics": surface_diagnostics(),
            "input_bytes": os.path.getsize(a.input),
        }
        os.makedirs(os.path.dirname(a.report), exist_ok=True)
        with open(a.report, "w", encoding="utf-8") as f:
            json.dump(report, f, indent=2)
        print("ELDORIA_SURFACE_DIAGNOSTIC=" + json.dumps(report))
        return

    target = int(a.target_tris)
    min_tris = int(a.min_tris)
    max_tris = int(a.max_tris)
    if target <= 0 or min_tris <= 0 or max_tris <= 0 or not (min_tris <= target <= max_tris):
        raise RuntimeError(f"Invalid triangle profile: min={min_tris} target={target} max={max_tris}")
    if raw["triangles"] < min_tris:
        raise RuntimeError(
            f"Raw source is below the configured gate floor: {raw['triangles']} tris; "
            f"expected at least {min_tris}"
        )
    ensure_materials_and_uvs()
    surface_rescue = apply_surface_rescue(a.surface_rescue_profile)
    refine_cfg = load_refine_config(a.refine_config)
    refinement = apply_visual_refinement(refine_cfg)
    if raw["triangles"] > max_tris:
        for _ in range(3):
            now = apply_decimation_once(target)
            if min_tris <= now <= max_tris:
                break
            if now < min_tris:
                raise RuntimeError(f"Decimation undershot tolerance: {now} tris")
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    optimized = metrics("optimized")
    if not (min_tris <= optimized["triangles"] <= max_tris):
        raise RuntimeError(f"Optimized triangle gate failed: {optimized['triangles']} not in {min_tris}-{max_tris}")
    if not optimized["uv_present_all_meshes"]:
        raise RuntimeError("Optimized GLB is missing UV0 on at least one mesh")
    resized_images = optimize_images()
    os.makedirs(os.path.dirname(a.output), exist_ok=True)
    bpy.ops.export_scene.gltf(
        filepath=a.output,
        export_format="GLB",
        export_apply=True,
        export_materials="EXPORT",
        export_yup=True,
    )
    # Optional reusable multipiece extraction runs after the canonical combined export so
    # per-piece pivot normalization cannot alter the certified combined geometry.
    salvage_cfg = load_refine_config(a.split_salvage_config)
    multipiece = split_components_to_glbs(a.split_components_dir, a.split_min_triangles, a.split_cluster_count, salvage_cfg)
    report = {
        "target_triangles": target,
        "accepted_range": [min_tris, max_tris],
        "raw": raw,
        "optimized": optimized,
        "resized_images": resized_images,
        "surface_diagnostics": surface_diagnostics(),
        "refinement": refinement,
        "surface_rescue": surface_rescue,
        "input_bytes": os.path.getsize(a.input),
        "output_bytes": os.path.getsize(a.output),
        "multipiece": multipiece,
    }
    os.makedirs(os.path.dirname(a.report), exist_ok=True)
    with open(a.report, "w", encoding="utf-8") as f:
        json.dump(report, f, indent=2)
    print("ELDORIA_TRIPO_MODULE_REPORT=" + json.dumps(report))

if __name__ == "__main__":
    main()
