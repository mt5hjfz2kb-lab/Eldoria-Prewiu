"""Static/algorithmic gate for Unity scene source: does NOT compile C# or approve art."""
import itertools
import math
import pathlib
import re

SOURCE = pathlib.Path(__file__).resolve().parents[3] / 'Unity/Assets/Eldoria/SendaBrecha/SendaBrechaPrototype.cs'
if not SOURCE.exists():
    SOURCE = pathlib.Path(__file__).resolve().parents[4] / 'Unity/Assets/Eldoria/SendaBrecha/SendaBrechaPrototype.cs'
text = SOURCE.read_text(encoding='utf-8')
assert 'UnityEngine' in text and 'Camera' in text and 'PrimitiveType' in text
assert 'LocalGateway' not in text and 'PlayerPrefs' not in text and 'GameState' not in text
assert 'IsolatedFromMainGame => true' in text
assert 'Mathf.CeilToInt(Soldiers / 4f)' in text, 'Reinforcements must visibly grow the formation'
assert 'if (Application.isPlaying)' not in text, 'Namespace collision: qualify UnityEngine.Application'
assert 'GraphicsSettings.currentRenderPipeline' in text, 'Select shader by actual active render pipeline'
assert text.count('new Wave(') == 6
regex = re.compile(r'new Wave\("([^"]+)",(\d+),new Gate\(GateType\.(\w+),(\d+)\),new Gate\(GateType\.(\w+),(\d+)\),new Gate\(GateType\.(\w+),(\d+)\)\)')
waves = []
for match in regex.finditer(text):
    name,enemy,*rest = match.groups()
    gates = [(rest[i],int(rest[i+1])) for i in (0,2,4)]
    waves.append((name,int(enemy),gates))
assert len(waves) == 6,waves

def play(route):
    soldiers,power,ward = 22,0,0
    for stage,lane in enumerate(route):
        _,enemy,gates = waves[stage]
        typ,n = gates[lane]
        if typ=='Recruit': soldiers=min(100,soldiers+n)
        if typ=='Power': power=min(150,power+n)
        if typ=='Ward': ward=min(90,ward+n)
        if typ=='Trap': soldiers=max(1,soldiers-n)
        attack=soldiers*(1+power/100)
        effort=enemy*(1+.15*stage)
        total=max(1,math.ceil(effort*.36/(1+attack/48)))
        if attack<effort*.36:total+=math.ceil((effort*.36-attack)/3)
        absorbed=min(ward,total);ward-=absorbed
        soldiers-=min(soldiers,total-absorbed)
        if soldiers <= 0:return False,stage,soldiers
    return True,5,soldiers

all_routes = list(itertools.product(range(3), repeat=6))
results = [play(route) for route in all_routes]
wins=sum(r[0] for r in results)
assert 1 <= wins <= 728, wins
assert play((1,0,1,0,1,0))[0], 'Native screenshot test route must win'
assert not play((2,0,2,1,2,1))[0], 'A very poor route should be winnable? no: defeat must exist'
print(f'SENDA_UNITY_STATIC_CONTRACT_PASS waves={len(waves)} paths={len(all_routes)} wins={wins} defeats={729-wins}')
print('LIMIT: Algorithmic Python mirror and source inspection only; Unity C# compile/visual/iPhone QA pending.')
