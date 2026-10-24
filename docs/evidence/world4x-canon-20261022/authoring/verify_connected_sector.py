"""R: reopen exact Q source for integration verification; zero new artistic operations."""
import bpy,sys,pathlib,hashlib,json,shutil
out,src=map(pathlib.Path,sys.argv[sys.argv.index('--')+1:]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='6a28e42db779bff8af5ec10a13bac87d38b97e594c9528aad8f7cbd211a5828e'
bpy.ops.wm.open_mainfile(filepath=str(src))
assert bpy.data.objects.get('Pilot_Terrain_1') and bpy.data.objects.get('Editable Q forest planting points')
# Preserve exact bytes of reviewed Q source and export, rather than pretending this is new art.
for f in src.parent.iterdir():
 if f.is_file() and f.suffix.lower() in {'.blend','.glb','.png','.json'}:shutil.copy2(f,out/f.name)
(out/'R-verification.json').write_text(json.dumps({'source_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'artistic_operations':0,'purpose':'Exact Q reopened in native Blender; corrected Unity selection probe, no art mutation'},indent=2))
