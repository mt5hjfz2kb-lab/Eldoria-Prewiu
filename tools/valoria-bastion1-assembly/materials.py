"""Give Unity JsonUtility explicit glTF defaults, including absent texture -1.
Unity's nested Serializable objects do not preserve omitted-field defaults.
"""
import json,pathlib,struct,sys
glb=pathlib.Path(sys.argv[1]);data=glb.read_bytes();length=struct.unpack_from('<I',data,12)[0]
source=json.loads(data[20:20+length]);result={'materials':[]}
for item in source['materials']:
 m=dict(item);p=dict(m.get('pbrMetallicRoughness',{}))
 for name,value in [('baseColorFactor',[1,1,1,1]),('metallicFactor',1),('roughnessFactor',1)]:p.setdefault(name,value)
 for name in ['baseColorTexture','metallicRoughnessTexture']:p.setdefault(name,{'index':-1})
 m['pbrMetallicRoughness']=p;m.setdefault('normalTexture',{'index':-1,'scale':1})
 m['normalTexture'].setdefault('scale',1);m.setdefault('alphaMode','OPAQUE');m.setdefault('alphaCutoff',.5)
 m.setdefault('emissiveFactor',[0,0,0]);m.setdefault('doubleSided',False);result['materials'].append(m)
out=glb.with_name('material-bindings.json');out.write_text(json.dumps(result,indent=2)+'\n')
print('EXPLICIT_GLTF_DEFAULTS',len(result['materials']),out)
