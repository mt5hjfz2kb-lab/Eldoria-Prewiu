using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    // Entirely separate prototype, not a replacement of the certified Valoria scene.
    // The source is the authored Blender mesh-only GLB on this experiment branch.
    public static class ValoriaMeshOnlyBastionOneProof
    {
        const string Source = "Assets/Eldoria/ProductionSlice/Experimental/ValoriaMeshOnlyWorld.glb";
        const string Output = "Assets/Eldoria/ProductionSlice/Experimental/ValoriaMeshOnlyBastionOne.unity";
        const int CaptureWidth=1152, CaptureHeight=768;

        [MenuItem("Eldoria/Experimental/Build SHARP-free Bastion I")]
        public static void Create()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Source);
            if(source==null)throw new FileNotFoundException("Mesh-only authored GLB failed Unity import",Source);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=(GameObject)PrefabUtility.InstantiatePrefab(source);
            if(root==null)throw new InvalidOperationException("Worldspace GLB cannot instantiate");
            root.name="Valoria_BastionI_MeshOnly_Prototype";
            root.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            root.transform.localScale=Vector3.one;
            var renderers=root.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0)throw new InvalidOperationException("Mesh-only GLB has no renderers");
            var bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
            if(bounds.size.magnitude<10f || bounds.size.magnitude>10000f)
                throw new InvalidOperationException("Imported worldspace bounds implausible: "+bounds);

            var cameraObject=new GameObject("WorldspaceProofCamera");
            var camera=cameraObject.AddComponent<Camera>();
            camera.tag="MainCamera";
            camera.orthographic=true;
            camera.orthographicSize=51f;
            camera.nearClipPlane=.1f;
            camera.farClipPlane=350f;
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.34f,.43f,.54f);
            var cameraTarget=new Vector3(0f,8f,-6f);
            camera.transform.position=new Vector3(55f,78f,82f);
            camera.transform.rotation=Quaternion.LookRotation(cameraTarget-camera.transform.position,Vector3.up);
            camera.aspect=(float)CaptureWidth/CaptureHeight;

            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.67f,.68f,.74f);
            var sunObject=new GameObject("WorldspaceProofSun");
            var sunlight=sunObject.AddComponent<Light>();
            sunlight.type=LightType.Directional;
            sunlight.color=new Color(1f,.86f,.67f);
            sunlight.intensity=1.3f;
            sunObject.transform.rotation=Quaternion.Euler(45f,-34f,10f);
            Directory.CreateDirectory(Path.GetDirectoryName(Output));
            EditorSceneManager.SaveScene(scene,Output);
            var outputFolder=Path.GetFullPath(Path.Combine(Application.dataPath,"..","ValoriaMeshOnlyProof"));
            Directory.CreateDirectory(outputFolder);
            var outputPath=Path.Combine(outputFolder,"mesh-only-bastion-i-unity.png");
            var target=new RenderTexture(CaptureWidth,CaptureHeight,24,RenderTextureFormat.ARGB32);
            var previousTarget=camera.targetTexture;
            var previousActive=RenderTexture.active;
            var capture=new Texture2D(CaptureWidth,CaptureHeight,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target;
                RenderTexture.active=target;
                camera.Render();
                capture.ReadPixels(new Rect(0,0,CaptureWidth,CaptureHeight),0,0);
                capture.Apply();
                File.WriteAllBytes(outputPath,capture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=previousTarget;
                RenderTexture.active=previousActive;
                UnityEngine.Object.DestroyImmediate(capture);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
            Debug.Log("[VALORIA MESH ONLY] Import and render complete. Bounds="+bounds
                +" renderers="+renderers.Length+" screenshot="+outputPath);
        }
    }
}
