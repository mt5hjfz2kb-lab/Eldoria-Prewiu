"""Compile the candidate's exact clock helper and verify actual GLB bucket parity.
This source laboratory is not a Unity build or browser/device performance test.
"""
import argparse, pathlib, subprocess, tempfile, shutil
p=argparse.ArgumentParser();p.add_argument('--repo',type=pathlib.Path,required=True);p.add_argument('--glb',type=pathlib.Path,required=True);p.add_argument('--output',type=pathlib.Path,required=True);p.add_argument('--dotnet',default='dotnet');a=p.parse_args()
s=(a.repo/'Unity/Assets/Eldoria/World4XCanon/RegionOneBlenderLandscape.cs').read_text()
start=s.index(' internal sealed class RegionOneFrameSlice');opening=s.index('{',start);depth=1;end=opening+1
while depth:
 if s[end]=='{':depth+=1
 elif s[end]=='}':depth-=1
 end+=1
helper='using System;\nnamespace Eldoria.World4XCanon {\n'+s[start:end]+'\n}\n'
a.output.parent.mkdir(parents=True,exist_ok=True)
with tempfile.TemporaryDirectory(prefix='eldoria-frame-slice-') as tmp:
 root=pathlib.Path(tmp);(root/'FrameSlice.cs').write_text(helper)
 for name in ['Lab.cs','Lab.csproj']:shutil.copyfile(pathlib.Path(__file__).with_name(name),root/name)
 subprocess.run([a.dotnet,'run','-c','Release','--project',str(root/'Lab.csproj'),'--',str(a.glb.resolve()),str(a.output.resolve())],check=True)
