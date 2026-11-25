using System;
using Eldoria.AldricCity;
using GLTFast;
using UnityEngine;
using UnityEngine.Networking;

namespace Eldoria.HeroesUI
{
    // The modular city has no roaming 3D actor. Load the approved rig only
    // when the player opens Aldric's profile; the hall owns its lifetime.
    public sealed class HeroesAldricModel : MonoBehaviour
    {
        public Action Loaded;
        GameObject model;
        GltfImport importer;
        bool requested,loading;

        public GameObject Request()
        {
            if(!requested)
            {
                requested=true;
                Load();
            }
            return loading?null:model;
        }

        async void Load()
        {
            loading=true;
            bool ready=false;
            try
            {
                string path=UnityEngine.Application.streamingAssetsPath.TrimEnd('/')+"/"+AldricCityRoaming.Asset;
                string url=path.Contains("://")?path:new Uri(path).AbsoluteUri;
                byte[] source;
                using(var request=UnityWebRequest.Get(url))
                {
                    await request.SendWebRequest();
                    if(!this || request.result!=UnityWebRequest.Result.Success) return;
                    source=request.downloadHandler.data;
                }
                if(source.Length!=AldricCityRoaming.ModelBytes ||
                    AldricCityRoaming.Digest(source)!=AldricCityRoaming.ApprovedSha256)
                    throw new InvalidOperationException("Unexpected Aldric model");
                byte[] bytes=AldricCityRoaming.PrepareRequiredNormalStreams(source,out _);
                source=null;
                importer=new GltfImport(logger:new GLTFast.Logging.ConsoleLogger());
                if(!await importer.Load(bytes,importSettings:new ImportSettings{AnimationMethod=AnimationMethod.Legacy}) || !this) return;
                model=new GameObject("Aldric / approved portrait rig");
                model.SetActive(false);
                model.transform.SetParent(transform,false);
                var factory=new GameObjectInstantiator(importer,model.transform);
                if(!await importer.InstantiateMainSceneAsync(factory) || !this) return;
                ready=model.GetComponentInChildren<SkinnedMeshRenderer>(true)!=null;
            }
            catch(Exception error)
            {
                if(this) Debug.LogWarning("Aldric profile: "+error.Message);
            }
            finally
            {
                loading=false;
                if(!this || !ready) Release();
            }
            if(this && ready) Loaded?.Invoke();
        }

        void OnDestroy()
        {
            if(!loading) Release();
        }

        void Release()
        {
            if(model) Destroy(model);
            model=null;
            importer?.Dispose();
            importer=null;
        }
    }
}
