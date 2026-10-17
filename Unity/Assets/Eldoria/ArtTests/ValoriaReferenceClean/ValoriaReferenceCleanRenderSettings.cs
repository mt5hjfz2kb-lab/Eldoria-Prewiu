using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Eldoria.ReferenceClean {
[ExecuteAlways]
public class ValoriaReferenceCleanRenderSettings:MonoBehaviour {
 RenderPipelineAsset previous;UniversalRenderPipelineAsset local;
 void OnEnable(){
  previous=QualitySettings.renderPipeline;
  var source=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
  if(!source)return;
  local=Instantiate(source);local.name="Valoria isolated scene URP";local.hideFlags=HideFlags.HideAndDontSave;
  local.shadowDistance=180;local.shadowCascadeCount=2;local.msaaSampleCount=4;
  QualitySettings.renderPipeline=local;
 }
 void OnDisable(){if(local){QualitySettings.renderPipeline=previous;if(UnityEngine.Application.isPlaying)Destroy(local);else DestroyImmediate(local);}}
}
}
