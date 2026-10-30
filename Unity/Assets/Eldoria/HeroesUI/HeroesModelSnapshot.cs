using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Eldoria.HeroesUI
{
    /// <summary>Cached render of existing approved geometry, with no duplicate importer or gameplay actor.</summary>
    public sealed class HeroesModelSnapshot : MonoBehaviour
    {
        RenderTexture texture;
        GameObject previous;
        float yaw=90, renderedYaw;
        public bool HasModel => texture && previous;
        public void Rotate(float degrees){yaw=Mathf.Repeat(yaw+degrees,360);}
        public bool Show(GameObject model,RawImage target,float initialYaw=90f)
        {
            if(!model||!target)return false;
            if(previous!=model)yaw=initialYaw;
            if(previous==model&&texture&&Mathf.Abs(renderedYaw-yaw)<.01f){target.texture=texture;target.enabled=true;return true;}
            var stage=new GameObject("Temporary approved hero render data");
            stage.hideFlags=HideFlags.HideAndDontSave;stage.transform.position=new Vector3(0,-12000,0);
            var temporaryMeshes=new List<Mesh>();Bounds bounds=default;bool any=false;
            try{
                foreach(var source in model.GetComponentsInChildren<Renderer>(true)){
                    Mesh mesh=null;
                    if(source is SkinnedMeshRenderer skin&&skin.sharedMesh){mesh=new Mesh();skin.BakeMesh(mesh);temporaryMeshes.Add(mesh);}
                    else if(source is MeshRenderer){var filter=source.GetComponent<MeshFilter>();if(filter)mesh=filter.sharedMesh;}
                    if(!mesh||!source.enabled)continue;
                    var part=new GameObject("Hero visual mesh");part.layer=31;part.transform.SetParent(stage.transform,false);
                    var rotation=Quaternion.Inverse(model.transform.rotation);
                    part.transform.localPosition=rotation*(source.transform.position-model.transform.position);
                    part.transform.localRotation=rotation*source.transform.rotation;
                    part.transform.localScale=source.transform.lossyScale;
                    part.AddComponent<MeshFilter>().sharedMesh=mesh;
                    var renderer=part.AddComponent<MeshRenderer>();renderer.sharedMaterials=source.sharedMaterials;
                    renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
                    if(any)bounds.Encapsulate(renderer.bounds);else{bounds=renderer.bounds;any=true;}
                }
                if(!any)return false;
                if(!texture){texture=new RenderTexture(384,576,24,RenderTextureFormat.ARGB32);texture.name="Approved hero cached preview";texture.Create();}
                var cameraObject=new GameObject("One-shot hero camera");cameraObject.transform.SetParent(stage.transform,false);
                var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.aspect=2f/3f;
                camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.045f,.055f,.075f,1);
                camera.allowHDR=false;camera.allowMSAA=false;camera.nearClipPlane=.01f;
                float radius=Mathf.Max(.1f,bounds.extents.magnitude);
                camera.orthographicSize=Mathf.Max(bounds.extents.y,bounds.extents.x/camera.aspect)*1.13f;
                camera.farClipPlane=radius*10;camera.transform.rotation=Quaternion.Euler(5,yaw,0);
                camera.transform.position=bounds.center-camera.transform.forward*radius*4;camera.targetTexture=texture;
                var lamp=new GameObject("Hero preview light");lamp.transform.SetParent(stage.transform,false);
                var light=lamp.AddComponent<Light>();light.type=LightType.Directional;light.cullingMask=1<<31;
                light.intensity=1.2f;light.shadows=LightShadows.None;light.transform.rotation=Quaternion.Euler(35,yaw-25,0);
                camera.Render();camera.targetTexture=null;target.texture=texture;target.enabled=true;
                previous=model;renderedYaw=yaw;return true;
            }catch(Exception error){Debug.LogWarning("HERO_PREVIEW_UNAVAILABLE "+error.GetType().Name);return false;}
            finally{stage.SetActive(false);Destroy(stage);foreach(var mesh in temporaryMeshes)Destroy(mesh);}
        }
        public Sprite CopyPortrait()
        {
            if(!HasModel)return null;
            var prior=RenderTexture.active;
            var copy=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
            try{
                RenderTexture.active=texture;copy.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);copy.Apply();
                copy.name="Lyra portrait from verified Unity geometry";
                return Sprite.Create(copy,new Rect(copy.width*.20f,copy.height*.48f,copy.width*.60f,copy.height*.48f),
                    new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            }catch{Destroy(copy);throw;}
            finally{RenderTexture.active=prior;}
        }
        public void Release(){previous=null;if(texture){texture.Release();Destroy(texture);texture=null;}}
        void OnDestroy()=>Release();
    }
}
