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
        public IEnumerator OwnerHudExposesOneGuidedPrimaryActionInsteadOfQaActionWall()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            var primary=GameObject.Find("Primary objective action");
            Assert.That(primary,Is.Not.Null);
            var buttons=primary.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            Assert.That(buttons.Length,Is.EqualTo(1),"Owner-facing HUD must present one dominant next action.");
            var label=buttons[0].GetComponentInChildren<UnityEngine.UI.Text>();
            Assert.That(label,Is.Not.Null);
            Assert.That(label.text,Is.Not.Empty);
            Assert.That(GameObject.Find("Actions"),Is.Null,"The old QA action wall must not return to owner/player builds.");
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
            var sawmillArt=GameObject.Find("Aserradero · dedicated sawmill");
            Assert.That(sawmillArt,Is.Not.Null,"The Art Pass 2 sawmill must load in real Valoria.");
            var sawmillRenderers=sawmillArt.GetComponentsInChildren<Renderer>();
            Assert.That(sawmillRenderers.Length,Is.GreaterThan(0));
            var sawmillBounds=sawmillRenderers[0].bounds;
            foreach(var renderer in sawmillRenderers)sawmillBounds.Encapsulate(renderer.bounds);
            Assert.That(sawmillBounds.size.x,Is.LessThanOrEqualTo(4.0f));
            Assert.That(sawmillBounds.size.z,Is.LessThanOrEqualTo(3.4f));
            Assert.That(sawmillBounds.max.y-.31f,Is.LessThanOrEqualTo(3.8f));
            Assert.That(sawmillBounds.max.x,Is.LessThan(-4.45f),"The central-street side must stay clear.");

            var barracksArt=GameObject.Find("Cuartel · dedicated barracks");
            Assert.That(barracksArt,Is.Not.Null,"The Art Pass 2 Cuartel must load in real Valoria.");
            var barracksRenderers=barracksArt.GetComponentsInChildren<Renderer>();
            Assert.That(barracksRenderers.Length,Is.GreaterThan(0));
            var barracksBounds=barracksRenderers[0].bounds;
            foreach(var renderer in barracksRenderers)barracksBounds.Encapsulate(renderer.bounds);
            Assert.That(barracksBounds.size.x,Is.InRange(3.5f,3.8f));
            Assert.That(barracksBounds.size.z,Is.InRange(2.9f,3.2f));
            Assert.That(barracksBounds.max.y-.31f,Is.InRange(3.4f,4.0f));
            Assert.That(barracksBounds.min.x,Is.GreaterThan(4.45f),"The west / central-street flank must stay clear.");
            foreach(var collider in barracksArt.GetComponentsInChildren<Collider>(true))
                Assert.That(collider.enabled,Is.False,"Cuartel visual mesh must not own gameplay click geometry.");

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
        public IEnumerator ValoriaArtPassPreservesGateTravelAndOfficialZoomEnvelope()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            var camera=GameObject.Find("Isometric camera")?.GetComponent<Camera>();
            var presenter=Object.FindFirstObjectByType<SlicePresenter>();
            Assert.That(camera,Is.Not.Null);
            Assert.That(presenter,Is.Not.Null);

            var zoom=typeof(SlicePresenter).GetMethod("Zoom",
                System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            var select=typeof(SlicePresenter).GetMethod("Select",
                System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            Assert.That(zoom,Is.Not.Null);
            Assert.That(select,Is.Not.Null);

            camera.orthographicSize=12f;
            zoom.Invoke(presenter,new object[]{-100f});
            Assert.That(camera.orthographicSize,Is.EqualTo(9f).Within(.001f));
            zoom.Invoke(presenter,new object[]{100f});
            Assert.That(camera.orthographicSize,Is.EqualTo(19f).Within(.001f));
            camera.orthographicSize=12f;
            Assert.That(camera.orthographicSize,Is.EqualTo(12f).Within(.001f));

            var gate=GameObject.Find("Puerta · ir al mundo");
            Assert.That(gate,Is.Not.Null);
            Assert.That(gate.GetComponent<WorldHotspot>(),Is.Not.Null);
            Assert.That(gate.GetComponent<WorldHotspot>().Id,Is.EqualTo("gate"));
            select.Invoke(presenter,new object[]{"gate"});
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("Frontier"));
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

            // Early game already contains the populated West Rebuilders quarter. Mobile portrait
            // must be able to travel far enough to inspect it instead of trapping it off-screen.
            configure.Invoke(presenter,new object[]{2});
            pan.Invoke(presenter,new object[]{new Vector2(5000,0)});
            var earlyA=camera.transform.position;
            recenter.Invoke(presenter,null);
            pan.Invoke(presenter,new object[]{new Vector2(-5000,0)});
            var earlyB=camera.transform.position;
            Assert.That(Mathf.Max(Vector3.Distance(earlyA,home),Vector3.Distance(earlyB,home)),Is.GreaterThan(10f));
            Assert.That(Mathf.Abs(earlyA.x-home.x),Is.LessThanOrEqualTo(16.1f));
            Assert.That(Mathf.Abs(earlyB.x-home.x),Is.LessThanOrEqualTo(16.1f));
            Assert.That(Quaternion.Angle(rotation,camera.transform.rotation),Is.LessThan(.01f));

            // Regression for the owner mobile report: the populated West Rebuilders quarter
            // must be reachable on-screen at Bastion I-II, not merely exist outside the pan clamp.
            var westDistrict=GameObject.Find("VPD · master west district");
            Assert.That(westDistrict,Is.Not.Null);
            bool WestVisible()
            {
                var vp=camera.WorldToViewportPoint(westDistrict.transform.position);
                return vp.z>0f&&vp.x>=.04f&&vp.x<=.96f&&vp.y>=.04f&&vp.y<=.96f;
            }
            bool westReachable=WestVisible();
            if(!westReachable)
            {
                camera.transform.position=earlyA;
                westReachable=WestVisible();
            }
            if(!westReachable)
            {
                camera.transform.position=earlyB;
                westReachable=WestVisible();
            }
            Assert.That(westReachable,Is.True,
                "Bastion I-II panning must be able to bring the West Rebuilders quarter into the mobile viewport.");

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

        [UnityTest]
        public IEnumerator OwnerHudExposesGuidedReferenceFrameAndReadableLighting()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            Assert.That(GameObject.Find("Reference topbar"),Is.Not.Null);
            Assert.That(GameObject.Find("Quest panel"),Is.Not.Null);
            Assert.That(GameObject.Find("World objective dock"),Is.Not.Null);
            Assert.That(GameObject.Find("Primary objective action"),Is.Not.Null);
            Assert.That(GameObject.Find("Bottom navigation"),Is.Not.Null);
            Assert.That(GameObject.Find("CIUDAD"),Is.Not.Null);
            Assert.That(GameObject.Find("MUNDO"),Is.Not.Null);
            Assert.That(GameObject.Find("HÉROES"),Is.Not.Null);
            Assert.That(GameObject.Find("ARCÓN"),Is.Not.Null);
            Assert.That(GameObject.Find("CÓDICE"),Is.Not.Null);
            Assert.That(RenderSettings.ambientLight.grayscale,Is.GreaterThan(.78f));
            var sun=GameObject.Find("Valoria · amber dusk")?.GetComponent<Light>();
            Assert.That(sun,Is.Not.Null);
            Assert.That(sun.intensity,Is.GreaterThanOrEqualTo(1.20f));
        }
        [UnityTest]
        public IEnumerator ValoriaGroundKitUsesReadableTiledSurfaceMaterials()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            var ground=GameObject.Find("VPD · GroundKit west workshop terrace · terrace");
            Assert.That(ground,Is.Not.Null,"Ground Kit terrace must be present in real Valoria.");
            var renderer=ground.GetComponent<Renderer>();
            Assert.That(renderer,Is.Not.Null);
            var material=renderer.sharedMaterial;
            Assert.That(material,Is.Not.Null);
            var texture=material.HasProperty("_BaseMap")?material.GetTexture("_BaseMap"):
                material.HasProperty("_MainTex")?material.GetTexture("_MainTex"):null;
            Assert.That(texture,Is.Not.Null,"Ground surfaces must expose a visible repeated texture, not a flat colour.");
            var scale=material.HasProperty("_BaseMap")?material.GetTextureScale("_BaseMap"):
                material.GetTextureScale("_MainTex");
            Assert.That(scale.x,Is.GreaterThanOrEqualTo(3f));
            Assert.That(scale.y,Is.GreaterThanOrEqualTo(3f));
        }


        [UnityTest]
        public IEnumerator CityObjectiveFocusPreservesCameraAndBringsTargetIntoView()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            var camera=GameObject.Find("Isometric camera")?.GetComponent<Camera>();
            var presenter=Object.FindFirstObjectByType<SlicePresenter>();
            var target=GameObject.Find("Aserradero · target");
            Assert.That(camera,Is.Not.Null);
            Assert.That(presenter,Is.Not.Null);
            Assert.That(target,Is.Not.Null);

            var rotation=camera.transform.rotation;
            var focus=typeof(SlicePresenter).GetMethod("FocusCityHotspot",
                System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            Assert.That(focus,Is.Not.Null);
            focus.Invoke(presenter,new object[]{"Aserradero · target"});
            yield return null;

            var point=camera.WorldToViewportPoint(target.GetComponent<Collider>().bounds.center);
            Assert.That(point.z,Is.GreaterThan(0f));
            Assert.That(point.x,Is.InRange(.08f,.92f));
            Assert.That(point.y,Is.InRange(.08f,.92f));
            Assert.That(Quaternion.Angle(rotation,camera.transform.rotation),Is.LessThan(.01f));
        }


        [UnityTest]
        public IEnumerator IrregularTerrainCarriesReadableUvsAndTiling()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            var ground=GameObject.Find("Valoria · valley floor");
            Assert.That(ground,Is.Not.Null);
            var mesh=ground.GetComponent<MeshFilter>()?.sharedMesh;
            Assert.That(mesh,Is.Not.Null);
            Assert.That(mesh.uv,Is.Not.Null);
            Assert.That(mesh.uv.Length,Is.EqualTo(mesh.vertexCount),
                "Large terrain sheets need UVs or their surface texture cannot render.");

            var material=ground.GetComponent<Renderer>()?.sharedMaterial;
            Assert.That(material,Is.Not.Null);
            var texture=material.HasProperty("_BaseMap")?material.GetTexture("_BaseMap"):
                material.HasProperty("_MainTex")?material.GetTexture("_MainTex"):null;
            Assert.That(texture,Is.Not.Null);
            var scale=material.HasProperty("_BaseMap")?material.GetTextureScale("_BaseMap"):
                material.GetTextureScale("_MainTex");
            Assert.That(scale.x,Is.GreaterThanOrEqualTo(4f));
            Assert.That(scale.y,Is.GreaterThanOrEqualTo(4f));

            SceneManager.LoadScene("Frontier");
            yield return null;
            var frontier=GameObject.Find("Frontier · valley floor");
            Assert.That(frontier,Is.Not.Null);
            Assert.That(frontier.GetComponent<MeshFilter>().sharedMesh.uv.Length,
                Is.EqualTo(frontier.GetComponent<MeshFilter>().sharedMesh.vertexCount));
        }


        [UnityTest]
        public IEnumerator FrontierUsesSemanticGroundAndContinuousMarchTrail()
        {
            SceneManager.LoadScene("Frontier");
            yield return null;

            var valley=GameObject.Find("Frontier · valley floor");
            var trail=GameObject.Find("Frontier · march trail");
            Assert.That(valley,Is.Not.Null);
            Assert.That(trail,Is.Not.Null);
            Assert.That(trail.transform.childCount,Is.GreaterThanOrEqualTo(5),
                "The Frontier route should read as a continuous worn trail, not isolated debug slabs.");

            var material=valley.GetComponent<Renderer>()?.sharedMaterial;
            Assert.That(material,Is.Not.Null);
            Assert.That(material.name,Does.Contain("surface earth"),
                "The general Frontier floor must use earth semantics instead of the stone-grid fallback.");
            Assert.That(GameObject.Find("Frontier · quarry shelf")?.GetComponent<Renderer>()?.sharedMaterial.name,
                Does.Contain("surface stone"));
            Assert.That(GameObject.Find("Frontier · corrupted shelf")?.GetComponent<Renderer>()?.sharedMaterial.name,
                Does.Contain("surface slate"));
        }



    }
}
