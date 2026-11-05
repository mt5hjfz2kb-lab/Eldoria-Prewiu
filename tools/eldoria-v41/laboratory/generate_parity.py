"""Generate independent Python oracle fixtures; never import or infer C# results."""
import csv, hashlib, pathlib, random
import v41_evolution_tuned as lab
root=pathlib.Path(__file__).parent
rng=random.Random(13820261031)
cases=[]
for f in range(3):
 for t in range(1,6):
  for g in range(3):
   for u in range(1,6):
    cases.append((lab.Force.one(f,t,100),lab.Force.one(g,u,100)))
for i in range(200):
 def force():
  n=[[rng.randrange(0,21) for t in range(5)] for f in range(3)]
  h=rng.choice([('aldric',None,None),('lyra',None,None),('aldric','lyra',None),('lyra','aldric',None)])
  return lab.Force(tuple(tuple(x) for x in n),h)
 cases.append((force(),force()))
for f in range(3):
 for g in range(3):
  cases.append((lab.Force.one(f,1,1000),lab.Force.one(g,5,1)))
with (root/'parity.csv').open('w',newline='') as stream:
 writer=csv.writer(stream,lineterminator='\n')
 writer.writerow(['# independent oracle '+hashlib.sha256((root/'v41_evolution_tuned.py').read_bytes()).hexdigest()])
 for a,b in cases:
  r=lab.resolve(a,b)
  writer.writerow([*[n for row in a.troops for n in row],*[n for row in b.troops for n in row],*[h or '-' for h in a.heroes],*[h or '-' for h in b.heroes],r.outcome,r.rounds,r.remaining_a,r.remaining_b,*r.damage_by_family_a,*r.damage_by_family_b,*[n for row in r.damaged_hp_a for n in row],*[n for row in r.damaged_hp_b for n in row]])
print('Generated',len(cases),'independent oracle cases')
