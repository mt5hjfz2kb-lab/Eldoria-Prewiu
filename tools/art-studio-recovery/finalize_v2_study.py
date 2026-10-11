"""Preserve matched real images and asset inventory; quality approval remains human."""
import hashlib, json, struct
from pathlib import Path
from PIL import Image,ImageDraw

ROOT=Path(__file__).resolve().parents[2]
V1=ROOT/'art-source/valoria/production/studio-rocks-v1'
SOURCE=ROOT/'art-source/valoria/production/studio-rocks-v2'
OUT=ROOT/'art-source/valoria/production/studio-rocks-v2-mcp'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()

canvas=Image.new('RGB',(1280,475),'#18202b');draw=ImageDraw.Draw(canvas)
for i,(p,label) in enumerate(((V1/'after-cliff.png','V1: un escaneado, tras MCP'),(OUT/'after-cliff.png','V2: dos escaneados CC0, tras MCP'))):
 canvas.paste(Image.open(p).convert('RGB').resize((640,400)),(i*640,35))
 draw.text((i*640+12,10),label,fill='white')
draw.text((12,447),'Misma camara y luz en Blender. Mejora parcial de variedad; no es una captura de Unity ni un ART PASS.',fill='white')
canvas.save(OUT/'Blender-V1-V2-Comparacion.png')

p=OUT/'ValoriaEnvironmentV2MCP.glb';raw=p.read_bytes()
chunk=struct.unpack_from('<I',raw,12)[0];gltf=json.loads(raw[20:20+chunk])
triangles=[sum(gltf['accessors'][x['indices']]['count']//3 for x in mesh['primitives']) for mesh in gltf['meshes']]
report=json.loads((SOURCE/'production-report.json').read_text())
mcp=json.loads((OUT/'mcp-report.json').read_text())
review={
 'status':'ARTIST_DIRECTOR_STUDY_RETAINED','reference_quality_met':False,'art_source_pass':False,
 'unity_tested':False,'mobile_performance_tested':False,'same_camera_lighting':True,
 'same_initial_source_scene':True,'before_v1_sha256':sha(V1/'after-cliff.png'),
 'after_v2_sha256':sha(OUT/'after-cliff.png'),
 'new_donor':'CC0 Yughues Free Photogrammetric Rock - Cluster',
 'new_donor_instances':report['cluster_instance_count'],
 'old_donor_retained':report['retained_old_course_count'],
 'mcp_response':mcp['response_status'],
 'observable_improvement':'A second independent scanned cluster interrupts the repeated rounded rock courses; 21 contacts were widened once.',
 'residual_failures':['Grass crown remains a flat jagged sheet against rock','Dark seams remain between some rock modules','Vertical geological strata and style remain below the owner reference','Current Unity integration terrain has moved; no Unity import of this candidate'],
 'rejected_alternative':'Rocks 03 CC0 produced horizontal waffle-like repeated shelves in matched Blender views; director rejected it.',
 'distinct_scanned_donors':2,'gltf_unique_mesh_triangles':sum(triangles),
 'gltf_instantiated_triangles':sum(triangles[node['mesh']] for node in gltf['nodes'] if 'mesh' in node),
 'gltf_bytes':len(raw),'gltf_sha256':sha(p),'services_cost_eur':0,
 'qwen_role':'Auxiliary advisory only; no automatic artistic acceptance'
}
(OUT/'quality-review.json').write_text(json.dumps(review,indent=2))
(OUT/'source-license-manifest.json').write_text(json.dumps({
 'v1_donor':{'name':'Free Photogrammetric Rock','author':'Yughues','license':'CC0','source':'https://opengameart.org/content/free-photogrammetric-rock','source_archive_sha256':'f33dbb5b2a2315a2fba5e5db09fabf799094fe8b5c437486ba52546e4486138d'},
 'v2_donor':{'name':'Free Photogrammetric Rock - Cluster','author':'Yughues','license':'CC0','source':'https://opengameart.org/content/free-photogrammetric-rock-cluster','source_archive_sha256':report['cluster_source_archive_sha256'],'source_obj_sha256':report['cluster_obj_sha256']},
 'rejected_comparison_donor':{'name':'Rocks 03','author':'Yughues / Nobiax','license':'CC0','source':'https://opengameart.org/content/rocks-03','source_archive_sha256':'c5c108b6847403d87e662e219d79e82508d906b3f5658971fb2e4131bbbb2799','exported_in_candidate':False},
 'paid_calls':0},indent=2))
