"""Matched Blender source A/B with protected-source geometry checks. NOT Unity approval."""
import argparse,hashlib,json,pathlib,sys,math
import bpy
p=argparse.ArgumentParser();p.add_argument('--cycle',type=pathlib.Path,required=True)
a=p.parse_args(sys.argv[sys.argv.index('--')+1:]);c=a.cycle
target='ZoneOne continuous prairie source'
bpy.ops.wm.open_mainfile(filepath=str((c/'source/derived-source.blend').resolve()))
before=[list(v.co) for v in bpy.data.objects[target].data.vertices]
other={o.name:[list(v.co) for v in o.data.vertices] for o in bpy.data.objects if o.type=='MESH' and o.name!=target}
camera=[list(row) for row in bpy.context.scene.camera.matrix_world]
bpy.ops.wm.open_mainfile(filepath=str((c/'edited/artist-output.blend').resolve()))
after=[list(v.co) for v in bpy.data.objects[target].data.vertices]
assert len(before)==len(after)
changes=[i for i,(x,y) in enumerate(zip(before,after)) if any(abs(a-b)>1e-5 for a,b in zip(x,y))]
assert changes,'Actual geometry edit required'
assert all(before[i][:2]==after[i][:2] for i in range(len(before))),'No lateral drift'
boundary=[i for i,v in enumerate(before) if abs(v[0])>=119.99 or abs(v[1])>=119.99]
pad=[i for i,v in enumerate(before) if -40<=v[0]<=4 and -52<=v[1]<=-8]
assert not set(changes).intersection(boundary),'Changed sector seams'
assert not set(changes).intersection(pad),'Changed city pad'
assert other=={o.name:[list(v.co) for v in o.data.vertices] for o in bpy.data.objects if o.type=='MESH' and o.name!=target}
assert camera==[list(row) for row in bpy.context.scene.camera.matrix_world]
mesh=bpy.data.objects[target].data;mesh.update()
areas=[(face.area,math.degrees(math.acos(max(-1.,min(1.,abs(face.normal.z)))))) for face in mesh.polygons]
area=sum(x[0] for x in areas)
slopes={'under_10_degrees_area_fraction':sum(x[0] for x in areas if x[1]<10)/area,
 'under_20_degrees_area_fraction':sum(x[0] for x in areas if x[1]<20)/area,
 'maximum_face_slope_degrees':max(x[1] for x in areas),
 'note':'Geometric source measurement, not certified navigation or mobile performance'}
scene=bpy.context.scene;scene.render.filepath=str((c/'blender-after.png').resolve());bpy.ops.render.render(write_still=True)
(c/'ground-ab-proof.json').write_text(json.dumps({'changed_vertices':len(changes),'protected_boundary_vertices':len(boundary),'protected_city_vertices':len(pad),'other_meshes_unchanged':True,'camera_matrix':camera,'image_type':'BLENDER_NOT_UNITY','art_approved':False,'slopes':slopes,'max_height_change_m':max(abs(before[i][2]-after[i][2]) for i in changes)},indent=2))
