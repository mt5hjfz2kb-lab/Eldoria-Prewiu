using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools
{
    public static class ValoriaStrongestCompositeGateV1
    {
        const string Folder="ValoriaStrongestCompositeV1Captures";

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            Directory.CreateDirectory(Folder);
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

            ProductionVisualIntegration.ResetVisualCachesForGate();
            ProductionVisualIntegration.StoneArchitectureEnabled=true;
            ProductionVisualIntegration.TerrainTerraceEnabled=true;
            ProductionVisualIntegration.SurfaceCellEnabled=false;
            ProductionVisualIntegration.ProductionCellEnabled=false;
            ProductionVisualIntegration.CoherentCastleProofEnabled=false;
            ProductionVisualIntegration.SlavicDistrictProofEnabled=false;
            ProductionVisualIntegration.CompactFootprintReframeEnabled=true;

            AssetVisualUpliftPassV1.Enabled=false;
            AssetLibraryReprocessingPassV1.Enabled=true;
            MidTierDistrictProduction.Enabled=true;
            ValoriaFullFrameArchitectureBatchV1.Enabled=true;
            ValoriaFullFrameForegroundEdgePassV1.Enabled=true;
            ValoriaOpenValleyCompositionV1.Enabled=true;
            ValoriaReferenceConvergencePassV2.Enabled=false;
            ValoriaInCitySurfacePassV1.Enabled=false;
            ValoriaStairLandingIntegrationV1.Enabled=false;
            ValoriaFullFrameConvergenceIteration1.Enabled=false;
            ValoriaFullFrameConvergenceIteration2.Enabled=false;
            ValoriaBenchmarkCompositeV2.Enabled=false;
            ValoriaEnvironmentUpliftV1.Enabled=false;
            ValoriaArchitectureCoherenceV1.Enabled=false;
            ValoriaBackplateCandidateV1.Enabled=false;
            ValoriaCliffIslandReframeV1.Enabled=false;
            ValoriaCliffIslandCleanupV2.Enabled=false;
            ValoriaResidualCleanupV1.Enabled=false;
            ValoriaMaterialResidueCleanupV2.Enabled=false;
            ValoriaFullFrameArtifactCleanupV1.Enabled=false;
            ValoriaWorldFrameMountainTerrainV1.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var c=Camera.main;
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(c==null||root==null)throw new System.Exception("Valoria capture prerequisites missing.");
            var p=new Vector3(18.2f,14.6f,-25.8f);
            var t=new Vector3(0f,3.35f,5.6f);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            ValoriaBenchmarkCompositeV2.Enabled=true;ValoriaBenchmarkCompositeV2.Build(root.transform,state);
            ValoriaEnvironmentUpliftV1.Enabled=true;ValoriaEnvironmentUpliftV1.Build(root.transform,state);
            ValoriaArchitectureCoherenceV1.Enabled=true;ValoriaArchitectureCoherenceV1.Build(root.transform,state);
            ValoriaCliffIslandReframeV1.Enabled=true;ValoriaCliffIslandReframeV1.Build(root.transform,state);
            ValoriaBackplateCandidateV1.Enabled=true;
            if(!ValoriaBackplateCandidateV1.Build(root.transform,c,"kiara3_1"))throw new System.Exception("Kiara 3 backplate unavailable.");
            ValoriaBackplateCandidateV1.FitAspect(1280f/720f);

            Save(c,Folder+"/baseline-production.png",p,t,9.1f,1280,720);

            ValoriaCliffIslandCleanupV2.Enabled=true;ValoriaCliffIslandCleanupV2.Build(root.transform,state);
            ValoriaResidualCleanupV1.Enabled=true;ValoriaResidualCleanupV1.Build(root.transform,state);
            ValoriaMaterialResidueCleanupV2.Enabled=true;ValoriaMaterialResidueCleanupV2.Build(root.transform,state);
            ValoriaFullFrameArtifactCleanupV1.Enabled=true;ValoriaFullFrameArtifactCleanupV1.Build(root.transform,state);

            Physics.SyncTransforms();
            WriteVisibleRendererAudit(c,Path.Combine(Folder,"visible-renderers.tsv"));
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new System.Exception("Strongest composite altered gameplay signature.");

            Save(c,Folder+"/after-19.png",p,t,19f,1280,720);
            Save(c,Folder+"/after-12.png",p,t,12f,1280,720);
            Save(c,Folder+"/after-9.png",p,t,9f,1280,720);
            Save(c,Folder+"/after-8.png",p,t,8f,1280,720);
            Save(c,Folder+"/after-production.png",p,t,9.1f,1280,720);
            ValoriaBackplateCandidateV1.FitAspect(390f/844f);
            Save(c,Folder+"/after-mobile.png",p,t,9.1f,390,844);

            File.WriteAllText(Folder+"/evidence.json",$"{{\n"+
                $"  \"collider_hotspot_signature_equal\": true,\n"+
                $"  \"cliff_cleanup_outer_suppressed\": {ValoriaCliffIslandCleanupV2.SuppressedOuterRenderers},\n"+
                $"  \"cliff_cleanup_barracks_suppressed\": {ValoriaCliffIslandCleanupV2.SuppressedBarracksRenderers},\n"+
                $"  \"residual_suppressed\": {ValoriaResidualCleanupV1.Suppressed},\n"+
                $"  \"residual_replacements\": {ValoriaResidualCleanupV1.Replacements},\n"+
                $"  \"material_residue_suppressed\": {ValoriaMaterialResidueCleanupV2.SuppressedPeripheral},\n"+
                $"  \"material_residue_disconnected_suppressed\": {ValoriaMaterialResidueCleanupV2.SuppressedDisconnectedPeripheral},\n"+
                $"  \"material_residue_environment_slots\": {ValoriaMaterialResidueCleanupV2.NormalizedEnvironment},\n"+
                $"  \"artifact_rock_renderers\": {ValoriaFullFrameArtifactCleanupV1.RockRenderersNormalized},\n"+
                $"  \"artifact_terrace_renderers\": {ValoriaFullFrameArtifactCleanupV1.TerraceRenderersNormalized},\n"+
                $"  \"artifact_foliage_lifted\": {ValoriaFullFrameArtifactCleanupV1.FoliageRenderersLifted},\n"+
                $"  \"artifact_dark_flat_lifted\": {ValoriaFullFrameArtifactCleanupV1.DarkFlatRenderersLifted},\n"+
                $"  \"background\": \"Kiara 3 Morning CC0\",\n"+
                $"  \"tripo_credits\": 0\n"+
                $"}}\n");

            EditorApplication.Exit(0);
        }

        static void WriteVisibleRendererAudit(Camera c,string path)
        {
            var rows=new List<string>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;
                if(Mathf.Max(b.size.x,Mathf.Max(b.size.y,b.size.z))<.55f)continue;
                var v=c.WorldToViewportPoint(b.center);
                if(v.z<=0f||v.x<-.10f||v.x>1.10f||v.y<-.10f||v.y>1.10f)continue;

                float minLum=99f,maxLum=-1f;bool anyTex=false;var mats=new StringBuilder();
                foreach(var m in r.sharedMaterials)
                {
                    if(m==null)continue;
                    Color col=Color.white;
                    if(m.HasProperty("_BaseColor"))col=m.GetColor("_BaseColor");
                    else if(m.HasProperty("_Color"))col=m.GetColor("_Color");
                    else if(m.HasProperty("_BaseColorFactor"))col=m.GetColor("_BaseColorFactor");
                    float lum=.2126f*col.r+.7152f*col.g+.0722f*col.b;
                    minLum=Mathf.Min(minLum,lum);maxLum=Mathf.Max(maxLum,lum);
                    Texture tex=null;
                    if(m.HasProperty("_BaseMap"))tex=m.GetTexture("_BaseMap");
                    if(tex==null&&m.HasProperty("_MainTex"))tex=m.GetTexture("_MainTex");
                    if(tex!=null)anyTex=true;
                    if(mats.Length>0)mats.Append(";");
                    mats.Append(m.name);
                }
                if(minLum>90f){minLum=.5f;maxLum=.5f;}

                var chain=new StringBuilder();
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(chain.Length>0)chain.Append(" <- ");
                    chain.Append(t.name);
                }

                float screenArea=Mathf.Max(.001f,b.size.x*b.size.y+b.size.x*b.size.z+b.size.y*b.size.z);
                rows.Add($"{v.x:F3}\t{v.y:F3}\t{v.z:F2}\t{screenArea:F2}\t{minLum:F3}\t{maxLum:F3}\t{(anyTex?"tex":"no_tex")}\t{r.gameObject.name}\t{mats}\tcenter=({b.center.x:F2},{b.center.y:F2},{b.center.z:F2})\tsize=({b.size.x:F2},{b.size.y:F2},{b.size.z:F2})\t{chain}");
            }
            rows.Sort((a,b)=>string.CompareOrdinal(a,b));
            var sb=new StringBuilder();
            sb.AppendLine("viewport_x\tviewport_y\tdepth\tbound_area\tmin_lum\tmax_lum\ttexture\trenderer\tmaterials\tbounds_center\tbounds_size\tchain");
            foreach(var row in rows)sb.AppendLine(row);
            File.WriteAllText(path,sb.ToString());
        }

        static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
        {
            c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
            try{
                c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
                var im=new Texture2D(w,h,TextureFormat.RGB24,false);
                im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
                File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);
            } finally {
                c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}
