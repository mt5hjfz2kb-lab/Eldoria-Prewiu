using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Eldoria.ReferenceClean {
[ExecuteAlways]
public class ValoriaReferenceCleanRenderSettings:MonoBehaviour {
 RenderPipelineAsset previous;UniversalRenderPipelineAsset local;
 void OnEnable(){
  previous=QualitySettings.renderPipeline;
  var source=QualitySettings.renderPipeline as UniversalRenderPipelineAsset ?? GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
  if(!source)return;
  local=Instantiate(source);local.name="Valoria isolated scene URP";local.hideFlags=HideFlags.HideAndDontSave;
  #if UNITY_EDITOR
  var serialized=new UnityEditor.SerializedObject(local);var soft=serialized.FindProperty("m_SoftShadowsSupported");if(soft!=null){soft.boolValue=true;serialized.ApplyModifiedPropertiesWithoutUndo();}
  #endif
  local.shadowDistance=180;local.shadowCascadeCount=2;local.msaaSampleCount=4;
  QualitySettings.renderPipeline=local;
 }
 void OnDisable(){if(local){QualitySettings.renderPipeline=previous;if(UnityEngine.Application.isPlaying)Destroy(local);else DestroyImmediate(local);}}
}
}
