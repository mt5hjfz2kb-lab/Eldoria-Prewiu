using UnityEngine;
using System.Collections.Generic;
using Eldoria.ReferenceClean;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Optional city camera framing owned by the NEW Vigilia preview only.
    /// The approved 864x1536 screenshot shows the whole bridge and the Bastion
    /// at approximately a third of phone width; old native 390x844 evidence
    /// zoomed so far in that the Bastion filled ~70% width and the bridge
    /// disappeared. That is a world-framing problem, not a button-skin problem.
    ///
    /// This adapter changes only the active city camera when portrait starts
    /// or orientation changes. It never changes meshes, terrain, saves or the
    /// canonical homeZoom; it never runs against the regional/world camera.
    /// A user's subsequent pan/pinch remains under canonical controls.
    /// </summary>
    public sealed class BreachWatchCityComposition : MonoBehaviour
    {
        ValoriaIntegratedWorld host;
        Camera cityCamera;
        float originalZoom;
        Vector3 originalPosition;
        bool captured,portraitFramed;
        // A user-approved cobalt roof is dramatically different from the
        // grey source-material roof. The independent Unity GPU material atlas
        // (native run 42948401195) proved original slots 8/9/10 and their
        // exact names. This reversible presentation-only tint changes no
        // mesh, prefab, shared material, texture, save or gameplay camera.
        sealed class RoofRestore
        {
            public Renderer Renderer;
            public int Index;
            public MaterialPropertyBlock Before;
            public bool HadBefore;
        }
        readonly List<RoofRestore> roofRestores=new List<RoofRestore>();
        // Native Unity A/B measurements on identical 900x1600 screen:
        // f6cf192c roof median [14,86,198] in a strict changed-pixel mask;
        // 3171065 roof median [13,118,214], against owner's original JPEG
        // roof median [12,100,180] in its corresponding cobalt-roof region.
        // 3171065 OVERCORRECTED toward cyan and is rejected artistically.
        // Predict using measured channel response: 0.15/1.28/1.84 should
        // approach the real JPEG color. Must verify in genuine Unity PNG,
        // not assume source code numbers prove final rendered appearance.
        static readonly Color OwnerApprovedCobaltRoof=
            new Color(.15f,1.28f,1.84f,1f);
        static readonly string[] RoofSlotMaterials={
            "material-3","material-4","material-5"
        };
        // This is a bounded art experiment on the actual Unity Terrain that
        // native run 42973492861 identified, not a screen-space color film.
        // The canonical TerrainData and shared TerrainLayer assets must never
        // be edited. Clone only the data/layer for the independent preview;
        // restore the exact original references on uninstall. The terrain
        // collider retains the same unmodified physical heightmap.
        Terrain paletteTerrain;
        TerrainData previousTerrainData;
        TerrainData previewTerrainData;
        readonly List<TerrainLayer> previewLayers=new List<TerrainLayer>();

        void ApplyReferenceGroundPalette()
        {
            foreach(var terrain in FindObjectsByType<Terrain>(FindObjectsSortMode.None))
            {
                if(!terrain || !terrain.gameObject.scene.isLoaded ||
                   terrain.name!="Valoria continuous sculpted terrain" ||
                   !terrain.terrainData)continue;
                var original=terrain.terrainData;
                var layers=original.terrainLayers;
                if(layers==null || layers.Length==0 || layers.Length>8)continue;
                if(!System.Array.Exists(layers,x=>x && x.name=="ShelteredGrass"))
                    continue;
                var candidate=Instantiate(original);
                candidate.name="Vigilia / private reversible ground data";
                var candidates=new TerrainLayer[layers.Length];
                int tinted=0;
                for(int i=0;i<layers.Length;i++)
                {
                    if(!layers[i]){candidates[i]=null;continue;}
                    if(layers[i].name!="ShelteredGrass")
                    {
                        candidates[i]=layers[i];
                        continue;
                    }
                    var own=Instantiate(layers[i]);
                    own.name="Vigilia / transient sheltered grass";
                    var originalMax=own.diffuseRemapMax;
                    // Owner JPEG uses darker, muted olive ground compared to
                    // lime terrain in previous native 900x1600. These are
                    // trial factors, not a certified RGB->GPU transfer curve.
                    own.diffuseRemapMax=new Vector4(
                        originalMax.x*.84f,originalMax.y*.74f,
                        originalMax.z*.71f,originalMax.w);
                    candidates[i]=own;
                    previewLayers.Add(own);
                    tinted++;
                }
                if(tinted==0){Destroy(candidate);continue;}
                candidate.terrainLayers=candidates;
                paletteTerrain=terrain;
                previousTerrainData=original;
                previewTerrainData=candidate;
                terrain.terrainData=candidate;
                Debug.Log("VIGILIA_GROUND_PALETTE_TRIAL originalLayers="+
                    layers.Length+" changedGrass="+tinted+
                    " isolatedTerrainData="+(previousTerrainData!=previewTerrainData));
                return;
            }
            Debug.Log("VIGILIA_GROUND_PALETTE_NOT_APPLIED_NO_CANONICAL_GRASS_LAYER");
        }
        void RestoreReferenceGroundPalette()
        {
            if(paletteTerrain && previousTerrainData)
                paletteTerrain.terrainData=previousTerrainData;
            if(previewTerrainData)Destroy(previewTerrainData);
            foreach(var owned in previewLayers)if(owned)Destroy(owned);
            previewLayers.Clear();
            paletteTerrain=null;
            previousTerrainData=null;
            previewTerrainData=null;
        }

        float lastAspect;

        public bool PortraitFramed => portraitFramed;
        public float OrthographicSize => cityCamera ? cityCamera.orthographicSize : 0f;

        public void Initialize(ValoriaIntegratedWorld source)
        {
            host=source;
            cityCamera=source && source.City ? source.City.PlayerCamera:null;
            if(cityCamera)
            {
                originalZoom=cityCamera.orthographicSize;
                originalPosition=cityCamera.transform.position;
                captured=cityCamera.orthographic;
            }
            ApplyReferenceGroundPalette();
            // The ephemeral HUD owns its own palette. The world will be
            // reverted on uninstall even if the user never opens the panel.
            if(source && source.City)
            {
                ApplyReferenceRoofPresentation(source.City.BastionOne);
                ApplyReferenceRoofPresentation(source.City.BastionTwo);
            }
        }

        void ApplyReferenceRoofPresentation(GameObject building)
        {
            if(!building)return;
            foreach(var renderer in building.GetComponentsInChildren<MeshRenderer>(true))
            {
                if(!renderer || !renderer.enabled)continue;
                var materials=renderer.sharedMaterials;
                // Native original Bastion uses 19 entries. Do not tint any
                // unknown level/model merely because it has a roof-like name.
                if(materials==null || materials.Length!=19)continue;
                if(renderer.name!="Bastion_I" &&
                   renderer.name!="Reviewed roof geometry / persistent probe lighting")
                    continue;
                for(int slot=8;slot<=10;slot++)
                {
                    var originalMaterial=materials[slot];
                    if(!originalMaterial ||
                       originalMaterial.name!=RoofSlotMaterials[slot-8] ||
                       !originalMaterial.HasProperty("_BaseColor"))continue;
                    var oldBlock=new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(oldBlock,slot);
                    var newBlock=new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(newBlock,slot);
                    newBlock.SetColor("_BaseColor",OwnerApprovedCobaltRoof);
                    roofRestores.Add(new RoofRestore {
                        Renderer=renderer,Index=slot,Before=oldBlock,
                        HadBefore=!oldBlock.isEmpty
                    });
                    renderer.SetPropertyBlock(newBlock,slot);
                }
            }
        }
        void RestoreOriginalRoofPresentation()
        {
            for(int i=roofRestores.Count-1;i>=0;i--)
            {
                var record=roofRestores[i];
                if(record.Renderer)
                    record.Renderer.SetPropertyBlock(
                        record.HadBefore?record.Before:null,record.Index);
            }
            roofRestores.Clear();
        }

        static bool TryBounds(GameObject gameObject,out Bounds bounds)
        {
            bounds=default;
            if(!gameObject)return false;
            bool found=false;
            foreach(var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                if(!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy ||
                   renderer.bounds.size.sqrMagnitude<.00001f)continue;
                if(!found){bounds=renderer.bounds;found=true;}
                else bounds.Encapsulate(renderer.bounds);
            }
            return found;
        }

        public void OnCityReentered(int pixelWidth,int pixelHeight)
        {
            if(!captured || !host || !host.City || host.InWorld || !cityCamera)return;
            // Canonical World -> City transition calls Controls.ResetView().
            // Our former portrait flag still says "already framed"; reset
            // that only at this actual mode transition, never on an ordinary
            // user pan or pinch. We then reapply the approved view exactly
            // once, preserving normal gesture ownership thereafter.
            portraitFramed=false;
            originalZoom=cityCamera.orthographicSize;
            originalPosition=cityCamera.transform.position;
            Layout(pixelWidth,pixelHeight);
        }

        public void Layout(int pixelWidth,int pixelHeight)
        {
            if(!captured || !cityCamera || !host || !host.City || host.InWorld)return;
            bool portrait=pixelHeight>pixelWidth;
            if(!portrait)
            {
                if(portraitFramed)
                {
                    // Returning to landscape restores the source camera's
                    // original home framing without changing its zoom preset.
                    cityCamera.orthographicSize=originalZoom;
                    cityCamera.transform.position=originalPosition;
                    host.City.Controls.FitToTerrain();
                    portraitFramed=false;
                }
                return;
            }
            float aspect=Mathf.Max(.30f,pixelWidth/(float)pixelHeight);
            if(portraitFramed && Mathf.Abs(aspect-lastAspect)<.012f)return;

            if(!portraitFramed)
            {
                originalZoom=cityCamera.orthographicSize;
                originalPosition=cityCamera.transform.position;
            }
            // At the 864:1536 approved reference, an 80–90-unit zoom
            // is approximately 1.8 times the former 48-unit first chapter
            // camera. Narrower phones need a little more separation so the
            // Bastion does not regain its giant 70%-screen footprint.
            float ratio=.5625f/aspect;
            float factor=Mathf.Clamp(1.78f*ratio,1.67f,2.65f);
            // Canonical city gesture controller clamps ZoomBy() to 14..65.
            // Starting at 75 or 105 made the FIRST legitimate pinch jump
            // abruptly to 65, breaking the feeling of a premium camera.
            // Never install a non-playable zoom; visual composition must
            // be the best achievable within actual game constraints.
            cityCamera.orthographicSize=Mathf.Clamp(originalZoom*factor,45f,65f);
            host.City.Controls.FitToTerrain();

            var state=host.City.Gateway!=null?host.City.Gateway.Snapshot():null;
            var bastion=state!=null && state.BastionLevel>=2 && host.City.BastionTwo
                && host.City.BastionTwo.activeInHierarchy
                    ?host.City.BastionTwo:host.City.BastionOne;
            if(TryBounds(bastion,out var bounds))
            {
                // Size the REAL model, not a hard-coded guess about a 16:9
                // camera. On 390x844 phones the previous aspect-only clamp
                // left the Bastion at 54% of the viewport despite its
                // accepted ~40% composition on 900x1600. This secondary
                // bounded projection fit is determined from the true model.
                float minX=float.PositiveInfinity,maxX=float.NegativeInfinity;
                var min=bounds.min;var max=bounds.max;
                for(int corner=0;corner<8;corner++)
                {
                    var point=new Vector3(
                        (corner&1)!=0?max.x:min.x,
                        (corner&2)!=0?max.y:min.y,
                        (corner&4)!=0?max.z:min.z);
                    var projected=cityCamera.WorldToViewportPoint(point);
                    if(projected.z<=0f)continue;
                    minX=Mathf.Min(minX,projected.x);
                    maxX=Mathf.Max(maxX,projected.x);
                }
                if(minX<float.PositiveInfinity)
                {
                    float currentWidth=maxX-minX;
                    float goalWidth=aspect<.50f?.43f:.40f;
                    if(currentWidth>goalWidth*1.035f && currentWidth<1.5f)
                    {
                        float nextZoom=cityCamera.orthographicSize*
                            (currentWidth/goalWidth);
                        cityCamera.orthographicSize=Mathf.Clamp(
                            nextZoom,cityCamera.orthographicSize,65f);
                        host.City.Controls.FitToTerrain();
                    }
                }
                // Position the EXISTING model at the upper central portion
                // of the phone and expose road, gate, river and resource sites.
                // Camera offset is strictly in the ground plane: avoid
                // changing height/tilt or producing perspective errors.
                Vector3 screen=cityCamera.WorldToViewportPoint(bounds.center);
                if(screen.z>0f)
                {
                    float height=2f*cityCamera.orthographicSize;
                    float width=height*aspect;
                    var offset=cityCamera.transform.right*(screen.x-.575f)*width+
                        cityCamera.transform.up*(screen.y-.72f)*height;
                    var forward=cityCamera.transform.forward;
                    if(Mathf.Abs(forward.y)>.04f)
                        offset-=forward*(offset.y/forward.y);
                    else offset.y=0f;
                    // Prevent a bad model bound from generating a huge warp.
                    if(offset.sqrMagnitude<25000f)
                    {
                        cityCamera.transform.position+=offset;
                        host.City.Controls.FitToTerrain();
                    }
                }
            }
            portraitFramed=true;
            lastAspect=aspect;
        }

        void OnDestroy()
        {
            RestoreReferenceGroundPalette();
            RestoreOriginalRoofPresentation();
            if(!captured || !portraitFramed || !cityCamera || !host || !host.City)return;
            cityCamera.orthographicSize=originalZoom;
            cityCamera.transform.position=originalPosition;
            host.City.Controls.FitToTerrain();
        }
    }
}
