"""Write a small URP Lit Shader Graph: atlas * vertex colour, optional emission.
No custom HLSL, texture arrays, compute, VFX Graph or external subgraphs.
Serialization shapes inspected in the repository's SG3 graphs; node ports checked
against Unity-Technologies/Graphics VertexColorNode and UniversalLitSubTarget.
Actual Shader Graph import/compilation remains a separate Unity gate.
"""
import json,uuid
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
objects=[];nodes=[];edges=[]
def uid(label):return uuid.uuid5(uuid.NAMESPACE_URL,'eldoria-visual-reboot/'+label).hex
def ref(label):return {'m_Id':uid(label)}
def obj(label,kind,**data):
 d={'m_SGVersion':0,'m_Type':kind,'m_ObjectId':uid(label),**data};objects.append(d);return d
def slot(label,kind,num,name,output=False,**data):
 return obj(label,'UnityEditor.ShaderGraph.'+kind,m_Id=num,m_DisplayName=name,m_SlotType=int(output),m_Hidden=False,m_ShaderOutputName=name,m_StageCapability=3,**data)
def node(label,kind,slots,**data):
 nodes.append(ref(label));return obj(label,'UnityEditor.ShaderGraph.'+kind,m_Group={'m_Id':''},m_Name=label,m_DrawState={'m_Expanded':True,'m_Position':{'serializedVersion':'2','x':len(nodes)*180,'y':0,'width':160,'height':120}},m_Slots=[ref(s) for s in slots],synonyms=[],m_Precision=0,m_PreviewExpanded=False,m_DismissedVersion=0,m_PreviewMode=0,m_CustomColors={'m_SerializableColors':[]},**data)
def edge(a,port,b,inport):edges.append({'m_OutputSlot':{'m_Node':ref(a),'m_SlotId':port},'m_InputSlot':{'m_Node':ref(b),'m_SlotId':inport}})
def vec(n=1):return dict(x=n,y=n,z=n,w=n)
def prop(label,kind,value,reference,**data):
 return obj(label,'UnityEditor.ShaderGraph.Internal.'+kind,m_Guid={'m_GuidSerialized':str(uuid.UUID(uid(label+'property')))},m_Name=label,m_DefaultRefNameVersion=1,m_RefNameGeneratedByDisplayName=label,m_DefaultReferenceName=reference,m_OverrideReferenceName=reference,m_GeneratePropertyBlock=True,m_UseCustomSlotLabel=False,m_CustomSlotLabel='',m_DismissedVersion=0,m_Precision=0,overrideHLSLDeclaration=False,hlslDeclarationOverride=0,m_Hidden=False,m_Value=value,**data)
prop('Atlas','Texture2DShaderProperty',{'m_SerializedTexture':'{"texture":{"instanceID":0}}','m_Guid':''},'_Atlas',isMainTexture=True,useTilingAndOffset=False,m_Modifiable=True,m_DefaultType=0)
prop('Glow','Vector1ShaderProperty',0,'_Glow',m_FloatType=0,m_RangeValues={'x':0,'y':4})
slot('atlas-out','Texture2DMaterialSlot',0,'Out',True,m_BareResource=False)
node('Atlas property','PropertyNode',['atlas-out'],m_Property=ref('Atlas'))
slot('glow-out','Vector1MaterialSlot',0,'Out',True,m_Value=0,m_DefaultValue=0,m_Labels=[])
node('Glow property','PropertyNode',['glow-out'],m_Property=ref('Glow'))
slot('vertex-out','Vector4MaterialSlot',0,'Out',True,m_Value=vec(),m_DefaultValue=vec(),m_Labels=[])
node('Vertex Color','VertexColorNode',['vertex-out'])
slot('sample-out','Vector4MaterialSlot',0,'RGBA',True,m_Value=vec(0),m_DefaultValue=vec(0),m_Labels=[])
slot('sample-tex','Texture2DInputMaterialSlot',1,'Texture',m_BareResource=False,m_Texture={'m_SerializedTexture':'{"texture":{"instanceID":0}}','m_Guid':''},m_DefaultType=0)
slot('sample-uv','UVMaterialSlot',2,'UV',m_Value={'x':0,'y':0},m_DefaultValue={'x':0,'y':0},m_Labels=[],m_Channel=0)
slot('sample-sampler','SamplerStateMaterialSlot',3,'Sampler',m_BareResource=False)
node('Sample Texture 2D','SampleTexture2DNode',['sample-out','sample-tex','sample-uv','sample-sampler'],m_TextureType=0,m_NormalMapSpace=0,m_EnableGlobalMipBias=True,m_MipSamplingMode=0)
edge('Atlas property',0,'Sample Texture 2D',1)
def multiply(label):
 for n,name in enumerate(['A','B','Out']):
  slot(label+name,'DynamicValueMaterialSlot',n,name,n==2,m_Value={f'e{i}{j}':(1 if i==j else 0) for i in range(4) for j in range(4)},m_DefaultValue={f'e{i}{j}':(1 if i==j else 0) for i in range(4) for j in range(4)})
 node(label,'MultiplyNode',[label+n for n in ['A','B','Out']])
multiply('Paint and vertex tint');multiply('Emission strength')
edge('Sample Texture 2D',0,'Paint and vertex tint',0);edge('Vertex Color',0,'Paint and vertex tint',1)
edge('Paint and vertex tint',2,'Emission strength',0);edge('Glow property',0,'Emission strength',1)
blocks=[]
for label,value in [('BaseColor',(.5,.5,.5)),('Emission',(0,0,0)),('Smoothness',.12),('Metallic',0)]:
 scalar=isinstance(value,(float,int));name='SurfaceDescription.'+label;blocks.append(ref(name))
 slot(name+'slot','Vector1MaterialSlot' if scalar else 'ColorRGBMaterialSlot',0,label,m_Value=value if scalar else dict(zip('xyz',value)),m_DefaultValue=value if scalar else dict(zip('xyz',value)),m_Labels=[],**({} if scalar else {'m_ColorMode':1 if label=='Emission' else 0,'m_DefaultColor':dict(zip('rgba',(*value,1))) }))
 node(name,'BlockNode',[name+'slot'],m_SerializedDescriptor=name)
edge('Paint and vertex tint',2,'SurfaceDescription.BaseColor',0);edge('Emission strength',2,'SurfaceDescription.Emission',0)
obj('URP target','UnityEditor.Rendering.Universal.ShaderGraph.UniversalTarget',m_Datas=[],m_ActiveSubTarget=ref('Lit'),m_AllowMaterialOverride=False,m_SurfaceType=0,m_ZTestMode=4,m_ZWriteControl=0,m_AlphaMode=0,m_RenderFace=2,m_AlphaClip=False,m_CastShadows=True,m_ReceiveShadows=True,m_SupportsLODCrossFade=False,m_CustomEditorGUI='',m_SupportVFX=False)
obj('Lit','UnityEditor.Rendering.Universal.ShaderGraph.UniversalLitSubTarget',m_WorkflowMode=1,m_NormalDropOffSpace=0,m_ClearCoat=False,m_BlendModePreserveSpecular=True)
graph={'m_SGVersion':3,'m_Type':'UnityEditor.ShaderGraph.GraphData','m_ObjectId':uid('graph'),'m_Properties':[ref('Atlas'),ref('Glow')],'m_Keywords':[],'m_Dropdowns':[],'m_CategoryData':[],'m_Nodes':nodes,'m_GroupDatas':[],'m_StickyNoteDatas':[],'m_Edges':edges,'m_VertexContext':{'m_Position':{'x':0,'y':0},'m_Blocks':[]},'m_FragmentContext':{'m_Position':{'x':1200,'y':0},'m_Blocks':blocks},'m_PreviewData':{'serializedMesh':{'m_SerializedMesh':'{"mesh":{"instanceID":0}}','m_Guid':''},'preventRotation':False},'m_Path':'Eldoria/Art Reboot','m_GraphPrecision':1,'m_PreviewMode':2,'m_OutputNode':{'m_Id':''},'m_SubDatas':[],'m_ActiveTargets':[ref('URP target')]}
target=ROOT/'Materials/Reboot_Atlas.shadergraph'
target.write_text('\n\n'.join(json.dumps(o,indent=2) for o in [graph]+objects)+'\n')
print(target)
