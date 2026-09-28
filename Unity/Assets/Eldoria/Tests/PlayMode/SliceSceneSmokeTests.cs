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
                var targetScreen=camera.WorldToScreenPoint(target.GetComponent<Collider>().bounds.center);
                var targetRay=camera.ScreenPointToRay(new Vector2(targetScreen.x,targetScreen.y));
                var targetHits=Physics.RaycastAll(targetRay,100f);
                System.Array.Sort(targetHits,(a,b)=>a.distance.CompareTo(b.distance));
                string hitDump="";
                foreach(var h in targetHits)
                {
                    var hs=h.collider.GetComponent<WorldHotspot>();
                    hitDump+=h.collider.gameObject.name+"#"+h.distance.ToString("F2")+"@"+h.collider.bounds.center+"=>"+(hs==null?"none":hs.Id)+"; ";
                }
                Assert.That(clearClick,Is.True,pair.Item1+" has no reliable player click point from the official camera. TargetWorld="+target.transform.position+" TargetScreen="+targetScreen+" CameraPos="+camera.transform.position+" CameraFwd="+camera.transform.forward+" Ortho="+camera.orthographicSize+" Aspect="+camera.aspect+" PixelRect="+camera.pixelRect+" Rect="+camera.rect+" Screen="+Screen.width+"x"+Screen.height+" Hits="+hitDump+" Resolved: "+resolvedIds);
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
        [UnityTest]
        public IEnumerator ValoriaMasterEnvelopeAndCameraPanAreProductionReady()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            var camera=GameObject.Find("Isometric camera")?.GetComponent<Camera>();
            var presenter=Object.FindFirstObjectByType<SlicePresenter>();
            Assert.That(camera,Is.Not.Null);
            Assert.That(presenter,Is.Not.Null);

            foreach(var name in new[]{
                "VPD · master west district",
                "VPD · master east district",
                "VPD · master upper civic reserve",
                "VPD · master future reserve",
                "VPD · master west terrain apron",
                "VPD · master east terrain apron"})
                Assert.That(GameObject.Find(name),Is.Not.Null,name+" missing");

            var configure=typeof(SlicePresenter).GetMethod("ConfigureCityPanBounds",
                System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            var pan=typeof(SlicePresenter).GetMethod("PanCameraByScreenDelta",
                System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            var recenter=typeof(SlicePresenter).GetMethod("RecenterCamera",
                System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            var resolve=typeof(SlicePresenter).GetMethod("ResolveHotspot",
                System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            var isPan=typeof(SlicePresenter).GetMethod("IsPanGesture",
                System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
            Assert.That(configure,Is.Not.Null);
            Assert.That(pan,Is.Not.Null);
            Assert.That(recenter,Is.Not.Null);
            Assert.That(resolve,Is.Not.Null);
            Assert.That(isPan,Is.Not.Null);

            Assert.That((bool)isPan.Invoke(null,new object[]{Vector2.zero,new Vector2(4,4)}),Is.False);
            Assert.That((bool)isPan.Invoke(null,new object[]{Vector2.zero,new Vector2(20,0)}),Is.True);

            var home=camera.transform.position;
            var rotation=camera.transform.rotation;

            // Early-game bounds stay compact.
            configure.Invoke(presenter,new object[]{2});
            pan.Invoke(presenter,new object[]{new Vector2(5000,5000)});
            var early=camera.transform.position;
            Assert.That(Mathf.Abs(early.x-home.x),Is.LessThanOrEqualTo(5.01f));
            Assert.That(Mathf.Abs(early.z-home.z),Is.LessThanOrEqualTo(4.01f));
            Assert.That(Quaternion.Angle(rotation,camera.transform.rotation),Is.LessThan(.01f));

            // Late-game bounds open substantially farther without changing orientation.
            recenter.Invoke(presenter,null);
            configure.Invoke(presenter,new object[]{35});
            pan.Invoke(presenter,new object[]{new Vector2(1400,0)});
            var late=camera.transform.position;
            Assert.That(Vector3.Distance(late,home),Is.GreaterThan(5.1f));
            Assert.That(Quaternion.Angle(rotation,camera.transform.rotation),Is.LessThan(.01f));

            // Canonical hotspots remain resolvable after camera translation.
            var target=GameObject.Find("Bastion · target");
            Assert.That(target,Is.Not.Null);
            var screen=camera.WorldToScreenPoint(target.GetComponent<Collider>().bounds.center);
            var spot=resolve.Invoke(presenter,new object[]{new Vector2(screen.x,screen.y)}) as WorldHotspot;
            Assert.That(spot,Is.Not.Null);
            Assert.That(spot.Id,Is.EqualTo("bastion"));

            // Recenter always returns exactly to the certified home position.
            recenter.Invoke(presenter,null);
            Assert.That(Vector3.Distance(camera.transform.position,home),Is.LessThan(.001f));
        }


    }
}
