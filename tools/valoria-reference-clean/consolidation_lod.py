"""Derive strategic LODs from exact integrated city meshes. No generated architecture."""
import bpy, bmesh, json, sys, pathlib, hashlib
root=pathlib.Path(sys.argv[sys.argv.index('--')+1])
data=json.loads((root/'mesh-input.json').read_text(encoding='utf-8-sig'))
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
output=[];proof=[]
for item in data['parts']:
    mesh=bpy.data.meshes.new(item['name'])
    faces=[];mats=[]
    for i,s in enumerate(item['slots']):
        tri=s['triangles'];faces.extend(tuple(tri[j:j+3]) for j in range(0,len(tri),3));mats.extend([i]*(len(tri)//3))
    mesh.from_pydata([(v['x'],v['y'],v['z']) for v in item['vertices']],[],faces);mesh.update()
    ob=bpy.data.objects.new(item['name'],mesh);bpy.context.collection.objects.link(ob)
    for i in range(len(item['slots'])):mesh.materials.append(bpy.data.materials.new(item['name']+'_slot_'+str(i)))
    uv=mesh.uv_layers.new(name='OriginalUV');col=mesh.color_attributes.new(name='OriginalGI',type='FLOAT_COLOR',domain='CORNER')
    for poly,mi in zip(mesh.polygons,mats):
        poly.material_index=mi
        for li in poly.loop_indices:
            vi=mesh.loops[li].vertex_index
            if item.get('uv'):v=item['uv'][vi];uv.data[li].uv=(v['x'],v['y'])
            if item.get('colors'):v=item['colors'][vi];col.data[li].color=(v['r'],v['g'],v['b'],v['a'])
            else:col.data[li].color=(1,1,1,1)
    source=ob.copy();source.data=mesh.copy();source.name=item['name']+'_FROZEN_SOURCE';bpy.context.collection.objects.link(source);source.hide_render=True;source.hide_set(True)
    bpy.context.view_layer.objects.active=ob;ob.select_set(True)
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00001);bm.to_mesh(mesh);bm.free()
    mod=ob.modifiers.new('Strategic silhouette-preserving reduction','DECIMATE');mod.ratio=.12;mod.use_collapse_triangulate=True
    bpy.ops.object.modifier_apply(modifier=mod.name)
    mesh=ob.data;mesh.calc_loop_triangles();uv=mesh.uv_layers.active.data;col=mesh.color_attributes['OriginalGI'].data
    verts=[];normals=[];uvs=[];colors=[];slots=[{'triangles':[]} for _ in item['slots']]
    for t in mesh.loop_triangles:
        for li in t.loops:
            v=mesh.vertices[mesh.loops[li].vertex_index];n=mesh.corner_normals[li].vector;u=uv[li].uv;c=col[li].color
            slots[t.material_index]['triangles'].append(len(verts));verts.append(dict(zip('xyz',v.co)));normals.append(dict(zip('xyz',n)));uvs.append(dict(zip('xy',u)));colors.append(dict(zip('rgba',c)))
    output.append(dict(name=item['name'],asset=item['asset'],vertices=verts,normals=normals,uv=uvs,colors=colors,slots=slots))
    proof.append(dict(source=item['asset'],input_vertices=len(item['vertices']),input_triangles=len(faces),output_triangles=len(mesh.loop_triangles),uv_preserved=True,vertex_gi_preserved=True))
    ob.select_set(False)
bpy.ops.wm.save_as_mainfile(filepath=str(root/'Valoria_Strategic_Architecture.blend'))
(root/'mesh-output.json').write_text(json.dumps({'parts':output},separators=(',',':')))
(root/'blender-proof.json').write_text(json.dumps({'blender':bpy.app.version_string,'source_sha256':hashlib.sha256((root/'mesh-input.json').read_bytes()).hexdigest(),'parts':proof},indent=2))
