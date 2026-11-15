"""Regression for the inward-facing roofs and solids seen in native run43770724604.
Usage: python verify_winding.py [mesh-directory]. Not an artistic approval.
"""
import sys
from pathlib import Path
root=Path(sys.argv[1]) if len(sys.argv)>1 else Path(__file__).resolve().parents[2]/'Unity/Assets/Eldoria/ExperimentalAshenCitadel/Meshes'
fail=[]
for p in sorted(root.glob('*.obj')):
 if p.stem=='AshenLandscape':continue
 v=[];mat='';vol=0.;bad_roofs=0
 for line in p.read_text().splitlines():
  if line.startswith('v '):v.append(tuple(map(float,line.split()[1:])))
  elif line.startswith('usemtl '):mat=line.split()[1]
  elif line.startswith('f '):
   idx=[int(t.split('/')[0])-1 for t in line.split()[1:]];a=v[idx[0]]
   for j in range(1,len(idx)-1):
    b=v[idx[j]];c=v[idx[j+1]]
    cross=(b[1]*c[2]-b[2]*c[1],b[2]*c[0]-b[0]*c[2],b[0]*c[1]-b[1]*c[0])
    vol+=sum(a[k]*cross[k] for k in range(3))/6
    ny=(b[2]-a[2])*(c[0]-a[0])-(b[0]-a[0])*(c[2]-a[2])
    if mat=='roof' and ny < -1e-7:bad_roofs+=1
 if vol<=0 or bad_roofs:fail.append(f'{p.name}: signed volume={vol:.3f}, inward roof triangles={bad_roofs}')
 print(p.name,round(vol,3),bad_roofs)
if fail:raise SystemExit('\n'.join(fail))
print('OUTWARD_SOLIDS_AND_ROOF_NORMALS_PASS')
