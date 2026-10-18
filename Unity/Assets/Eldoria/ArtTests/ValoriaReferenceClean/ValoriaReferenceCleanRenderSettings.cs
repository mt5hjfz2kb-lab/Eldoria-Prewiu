using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Eldoria.ReferenceClean {
[ExecuteAlways]
public class ValoriaReferenceCleanRenderSettings:MonoBehaviour {
 public UniversalRenderPipelineAsset scenePipeline;
 RenderPipelineAsset previous;bool applied;
 void OnEnable(){Apply();}
 public void Apply(){if(applied||!scenePipeline)return;previous=QualitySettings.renderPipeline;QualitySettings.renderPipeline=scenePipeline;applied=true;}
 public void Restore(){if(!applied)return;QualitySettings.renderPipeline=previous;applied=false;}
 void OnDisable(){Restore();}
}
}
