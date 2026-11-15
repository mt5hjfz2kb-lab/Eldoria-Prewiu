#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Eldoria.Presentation;
using Eldoria.ReferenceClean;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;

namespace Eldoria.StylizedDirectionPilot.Editor
{
    /// <summary>
    /// Genuine Unity scene/look-development experiment, NOT the gameplay scene.
    /// Uses only existing verified repository prefabs for ruins, pines and rock.
    /// It does not add parcels, colliders, hotspots, new resources or save data.
    /// Every source render stays intact; all additions and camera/Volume changes
    /// are disposed after the native Editor captures.
    /// </summary>
    public sealed class CompositionPilot : IDisposable
    {
        readonly Camera camera;
        readonly Vector3 originalPosition;
        readonly Quaternion originalRotation;
        readonly float originalSize;
        readonly bool originalHDR;
        readonly bool originalPost;
        readonly UniversalAdditionalCameraData cameraData;
        readonly GameObject root;
        readonly VolumeProfile profile;
        readonly List<Material> created=new List<Material>();
        readonly List<string> receipts=new List<string>();
        readonly int stagedObjects;
        readonly Vector3 originalGroundCentre;
        public IReadOnlyList<string> Receipts=>receipts;
        public int StagedObjects=>stagedObjects;

        public CompositionPilot(Camera source)
        {
            if(!source)throw new ArgumentNullException(nameof(source));
            camera=source;
            originalPosition=camera.transform.position;
            originalRotation=camera.transform.rotation;
            originalSize=camera.orthographicSize;
            originalHDR=camera.allowHDR;
            cameraData=camera.GetUniversalAdditionalCameraData();
            originalPost=cameraData.renderPostProcessing;
            root=new GameObject("EXPERIMENT ONLY / Eldoria stylized diorama composition");
            // In the production city geometry is real. These background props
            // intentionally exist only for the single exploratory capture.
            int count=0;
            count+=Add("WorldInventory/Arch_Gothic","ancient arch / western horizon",
                new Vector3(-11.2f,.01f,10.8f),5.4f,7.5f,25f,new Color(.58f,.57f,.51f,1));
            count+=Add("WorldInventory/Wall_Broken","broken imperial wall / west",
                new Vector3(-14.6f,.01f,6.5f),4.6f,3.8f,68f,new Color(.52f,.52f,.49f,1));
            count+=Add("WorldInventory/Arch_Gothic","distant eastern ruin",
                new Vector3(14.3f,.01f,12.6f),4.3f,6.6f,170f,new Color(.53f,.52f,.48f,1));
            count+=Add("WorldInventory/Wall_Broken","eastern ancient remains",
                new Vector3(13.7f,.01f,5.7f),4.2f,3.2f,202f,new Color(.50f,.51f,.49f,1));
            // Shape forests in deliberate edge-groups rather than evenly
            // scattering new trees across the original gameplay clearing.
            float[,] trees={
                {-19f,1f, 1.6f,4.6f,17f},{-17.3f,5.4f,1.9f,5.6f,58f},
                {-19.8f,9.4f,1.6f,5.8f,144f},{-16.1f,13.3f,1.9f,6.3f,209f},
                {-11.7f,16.0f,1.5f,5.2f,24f},{-7.6f,18.3f,1.8f,5.8f,93f},
                {18.5f,-.6f,1.7f,5.5f,46f},{16.5f,4.3f,1.8f,5.9f,85f},
                {18.9f,8.6f,1.6f,5.4f,166f},{16.7f,13.2f,2.0f,6.0f,223f},
                {8.5f,17.0f,1.7f,5.3f,35f},{11.8f,19.0f,1.8f,5.6f,94f}
            };
            for(int i=0;i<trees.GetLength(0);i++)
                count+=Add(i%3==0?"WorldInventory/Tree01B":"WorldInventory/Tree01A",
                    "existing cedar cluster "+i,
                    new Vector3(trees[i,0],-.06f,trees[i,1]),trees[i,2],trees[i,3],
                    trees[i,4],new Color(.39f,.48f,.40f,1));
            count+=Add("WorldInventory/Rock02","western ridge rock",
                new Vector3(-14.9f,-.12f,9f),2.1f,1.25f,20f,new Color(.48f,.47f,.42f,1));
            count+=Add("WorldInventory/Rock02","eastern ridge rock",
                new Vector3(15.8f,-.12f,9.4f),2.5f,1.45f,100f,new Color(.47f,.48f,.45f,1));
            stagedObjects=count;
            if(count<10) throw new InvalidOperationException("Insufficient real project models for pilot composition: "+count);

            // Use Unity's URP post-processing, not a mocked PNG filter.
            profile=ScriptableObject.CreateInstance<VolumeProfile>();
            var go=new GameObject("EXPERIMENT ONLY / subtle global color script");
            go.transform.SetParent(root.transform,false);
            var volume=go.AddComponent<Volume>();
            volume.isGlobal=true;
            volume.priority=2000;
            volume.profile=profile;
            var tone=profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);
            var grade=profile.Add<ColorAdjustments>(true);
            grade.postExposure.Override(-.04f);
            grade.contrast.Override(12f);
            grade.saturation.Override(-16f);
            grade.colorFilter.Override(new Color(.95f,.975f,.985f,1f));
            var bloom=profile.Add<Bloom>(true);
            bloom.intensity.Override(.12f);
            bloom.threshold.Override(1.1f);
            camera.allowHDR=true;
            cameraData.renderPostProcessing=true;

            originalGroundCentre=GroundViewCentre(camera);
            receipts.Add("Existing registered art reused: "+count+" instances (4 ruins, 12 trees, 2 rocks)");
            receipts.Add("URP original material shader remains; only temporary Volume grading");
            receipts.Add("Original camera position "+originalPosition+" ortho "+originalSize.ToString("F2"));
            ResetCamera();
        }

        int Add(string path,string label,Vector3 position,float footprint,float height,float yaw,Color tint)
        {
            var prefab=Resources.Load<GameObject>(path);
            if(!prefab){receipts.Add("MISSING prefab "+path);return 0;}
            GameObject go;
            if(path.Contains("Tree01"))
                go=ValoriaKit.BenchmarkPiece("Art proof "+label,prefab,position,footprint,height,Quaternion.Euler(0,yaw,0));
            else
                go=ValoriaKit.BenchmarkPieceIntegrated("Art proof "+label,prefab,position,footprint,height,Quaternion.Euler(0,yaw,0),tint);
            if(!go){receipts.Add("FAILED instance "+path);return 0;}
            go.transform.SetParent(root.transform,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
                foreach(var mat in r.sharedMaterials)
                    if(mat&&mat.name.StartsWith("Valoria integrated",StringComparison.Ordinal))
                        if(!created.Contains(mat))created.Add(mat);
            return 1;
        }

        static Vector3 GroundViewCentre(Camera c)
        {
            var ray=c.ViewportPointToRay(new Vector3(.5f,.5f,0f));
            var plane=new Plane(Vector3.up,Vector3.zero);
            return plane.Raycast(ray,out float distance)?ray.GetPoint(distance):Vector3.zero;
        }

        public void ResetCamera()
        {
            camera.transform.SetPositionAndRotation(originalPosition,originalRotation);
            camera.orthographicSize=originalSize;
        }

        /// <summary>
        /// A separate visual composition candidate; do not compare pixel
        /// differences against same-camera baseline as if material-only.
        /// </summary>
        public void FrameCity(float zoomRatio,float towardsBastion)
        {
            ResetCamera();
            camera.orthographicSize=originalSize*zoomRatio;
            // In the actual city scene the Bastion sits at the canonical
            // centre. Shift only by the original camera ground-target delta.
            var centre=Vector3.Lerp(originalGroundCentre,new Vector3(0,0,2.3f),Mathf.Clamp01(towardsBastion));
            camera.transform.position+=centre-originalGroundCentre;
        }

        public void Dispose()
        {
            ResetCamera();
            camera.allowHDR=originalHDR;
            if(cameraData)cameraData.renderPostProcessing=originalPost;
            if(root)Object.DestroyImmediate(root);
            foreach(var material in created)if(material)Object.DestroyImmediate(material);
            if(profile)Object.DestroyImmediate(profile);
        }
    }
}
#endif
