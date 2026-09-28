using System.Collections;
using Eldoria.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Eldoria.Tests
{
    public sealed class SliceSceneSmokeTests
    {
        [UnityTest]
        public IEnumerator ValoriaAndFrontierExposeVisibleSceneAndActions()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            Assert.That(Camera.main,Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<SlicePresenter>(),Is.Not.Null);
            Assert.That(Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None).Length,Is.GreaterThan(1));
            SceneManager.LoadScene("Frontier");
            yield return null;
            Assert.That(Camera.main,Is.Not.Null);
            Assert.That(Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None).Length,Is.GreaterThan(1));
        }

        [UnityTest]
        public IEnumerator ValoriaPlayableDistrictSkeletonHasReadableLayersAndRealBuildingPanels()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            var cameraGo=GameObject.Find("Isometric camera");
            var camera=cameraGo!=null?cameraGo.GetComponent<Camera>():null;
            var presenter=Object.FindFirstObjectByType<SlicePresenter>();
            Assert.That(camera,Is.Not.Null);
            Assert.That(presenter,Is.Not.Null);

            Assert.That(GameObject.Find("VPD · continuous terrain"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · L0 civic floor"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · L0 main street"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · vertical stair 1"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · vertical stair 12"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · L1 landing"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · L1 west plot"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · L1 east plot"),Is.Not.Null);

            var select=typeof(SlicePresenter).GetMethod("Select",
                System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            Assert.That(select,Is.Not.Null);

            foreach(var pair in new[]{("Aserradero · target","sawmill","ASERRADERO"),
                                      ("Cuartel · target","barracks","CUARTEL"),
                                      ("Bastion · target","bastion","BASTIÓN")})
            {
                var target=GameObject.Find(pair.Item1);
                Assert.That(target,Is.Not.Null,pair.Item1+" missing");
                var hotspot=target.GetComponent<WorldHotspot>();
                Assert.That(hotspot,Is.Not.Null);
                Assert.That(hotspot.Id,Is.EqualTo(pair.Item2));
                var resolve=typeof(SlicePresenter).GetMethod("ResolveHotspot",
                    System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
                Assert.That(resolve,Is.Not.Null);
                bool clearClick=false;
                string resolvedIds="";
                foreach(var candidate in Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None))
                {
                    if(candidate.Id!=pair.Item2)continue;
                    var collider=candidate.GetComponent<Collider>();
                    if(collider==null||!collider.enabled)continue;
                    var points=new[]{
                        collider.bounds.center,
                        collider.bounds.center+Vector3.up*collider.bounds.extents.y*.45f,
                        collider.bounds.center+camera.transform.right*collider.bounds.extents.x*.35f,
                        collider.bounds.center-camera.transform.right*collider.bounds.extents.x*.35f
                    };
                    foreach(var aim in points)
                    {
                        var screenPoint=camera.WorldToScreenPoint(aim);
                        if(screenPoint.z<=0)continue;
                        var resolved=resolve.Invoke(presenter,new object[]{new Vector2(screenPoint.x,screenPoint.y)}) as WorldHotspot;
                        resolvedIds+=candidate.gameObject.name+"@"+Mathf.RoundToInt(screenPoint.x)+","+Mathf.RoundToInt(screenPoint.y)+"=>"+(resolved==null?"null":resolved.Id+"("+resolved.gameObject.name+")")+"; ";
                        if(resolved!=null&&resolved.Id==pair.Item2){clearClick=true;break;}
                    }
                    if(clearClick)break;
                }
                Assert.That(clearClick,Is.True,pair.Item1+" has no reliable player click point from the official camera. TargetWorld="+target.transform.position+" TargetScreen="+camera.WorldToScreenPoint(target.GetComponent<Collider>().bounds.center)+" CameraPos="+camera.transform.position+" CameraFwd="+camera.transform.forward+" Ortho="+camera.orthographicSize+" Aspect="+camera.aspect+" PixelRect="+camera.pixelRect+" Rect="+camera.rect+" Screen="+Screen.width+"x"+Screen.height+" Resolved: "+resolvedIds);
                select.Invoke(presenter,new object[]{pair.Item2});
                yield return null;
                var panel=GameObject.Find("Building interaction panel");
                Assert.That(panel,Is.Not.Null);
                Assert.That(panel.activeInHierarchy,Is.True);
                var title=GameObject.Find("Building title").GetComponent<UnityEngine.UI.Text>();
                Assert.That(title.text,Does.Contain(pair.Item3));
                panel.SetActive(false);
            }
        }

    }
}
