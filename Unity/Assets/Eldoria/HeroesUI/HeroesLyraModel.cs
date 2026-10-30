using System;
using System.Collections;
using System.Security.Cryptography;
using GLTFast;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.HeroesUI
{
    /// <summary>Lazy, presentation-only reuse of the preserved PR127 Lyra. No actor, rig, save or command.</summary>
    public sealed class HeroesLyraModel : MonoBehaviour
    {
        public const string ApprovedSha256="a9fa7899e7ead2ad26e7b26d368c9006085ae8b8ded2df6515ca466c863f6725";
        public GameObject ModelRoot { get; private set; }
        public Sprite Portrait { get; private set; }
        public bool Ready { get; private set; }
        public Action Loaded;
        GltfImport importer;
        bool requested;
        public void Request()
        {
            if(requested)return;
            requested=true;StartCoroutine(Load());
        }
        IEnumerator Load()
        {
            var request=Resources.LoadAsync<TextAsset>("EldoriaV41/lyra-approved-model-v1");
            yield return request;
            var asset=request.asset as TextAsset;
            if(!asset){Debug.LogError("LYRA_APPROVED_MODEL_MISSING");yield break;}
            var bytes=asset.bytes;
            using(var hash=SHA256.Create()){
                string digest=BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();
                if(bytes.Length!=4770000||digest!=ApprovedSha256){
                    Resources.UnloadAsset(asset);
                    Debug.LogError("LYRA_APPROVED_MODEL_HASH_MISMATCH");yield break;
                }
            }
            var task=Import(bytes);
            while(!task.IsCompleted)yield return null;
            Resources.UnloadAsset(asset);
            if(task.IsFaulted||task.IsCanceled||!task.Result){
                Debug.LogError("LYRA_APPROVED_MODEL_IMPORT_FAILED "+(task.Exception?.ToString()??"rejected"));
                Release();yield break;
            }
            // The cached portrait comes from the actual imported geometry, never
            // from the conflicting pale-haired reference or a generated mockup.
            var capture=new GameObject("Lyra native portrait capture",typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));
            capture.hideFlags=HideFlags.HideAndDontSave;capture.SetActive(false);
            var snapshot=capture.AddComponent<HeroesModelSnapshot>();
            bool rendered=snapshot.Show(ModelRoot,capture.GetComponent<RawImage>(),0f);
            if(rendered)Portrait=snapshot.CopyPortrait();
            Destroy(capture);
            if(!Portrait){Debug.LogError("LYRA_APPROVED_MODEL_PORTRAIT_FAILED");Release();yield break;}
            Ready=true;
            Debug.Log("LYRA_APPROVED_MODEL_READY "+ApprovedSha256+"; static unrigged presentation only");
            Loaded?.Invoke();
        }
        async System.Threading.Tasks.Task<bool> Import(byte[] bytes)
        {
            importer=new GltfImport(logger:new GLTFast.Logging.ConsoleLogger());
            if(!await importer.Load(bytes)||!this)return false;
            ModelRoot=new GameObject("Lyra / preserved PR127 approved geometry");
            ModelRoot.hideFlags=HideFlags.HideAndDontSave;
            ModelRoot.transform.position=new Vector3(0,-14000,0);
            ModelRoot.SetActive(false);
            if(!await importer.InstantiateMainSceneAsync(new GameObjectInstantiator(importer,ModelRoot.transform))||!this)return false;
            return ModelRoot.GetComponentsInChildren<MeshRenderer>(true).Length>0;
        }
        void Release()
        {
            Ready=false;
            if(ModelRoot)Destroy(ModelRoot);ModelRoot=null;
            if(importer!=null){importer.Dispose();importer=null;}
            if(Portrait){var source=Portrait.texture;Destroy(Portrait);Destroy(source);Portrait=null;}
        }
        void OnDestroy()=>Release();
    }
}
