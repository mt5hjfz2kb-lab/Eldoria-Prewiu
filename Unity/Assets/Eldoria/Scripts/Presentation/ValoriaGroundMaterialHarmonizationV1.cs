using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Replaces only large visual ground skins with the existing Valoria PBR/detailed material vocabulary.
    // Collision and topology stay on their hidden authoritative objects.
    public static class ValoriaGroundMaterialHarmonizationV1
    {
        public static bool Enabled=false;

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled)return;

            var art=ValoriaExternalAssetLibrary.Load();
            var core=ValoriaKit.PbrSurfaceMaterial(
                art!=null?art.ValoriaDirtSurface:null,
                new Color(.48f,.43f,.34f,1f),new Vector2(8.5f,9.0f),.025f,.72f);
            var lower=ValoriaKit.PbrSurfaceMaterial(
                art!=null?art.ValoriaDirtSurface:null,
                new Color(.56f,.49f,.38f,1f),new Vector2(7.0f,6.0f),.025f,.78f);
            var upper=ValoriaKit.PbrSurfaceMaterial(
                art!=null?art.ValoriaDirtSurface:null,
                new Color(.50f,.46f,.39f,1f),new Vector2(6.0f,5.0f),.025f,.70f);

            ApplyExact("VPD · inhabited mountain floor",core);
            ApplyExact("VPD · lower terrace earth",lower);
            ApplyExact("VPD · upper terrace earth",upper);

            // Harmonize only visible visual earth/seams; never road/stair gameplay surfaces.
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string n=r.gameObject.name;
                if(n.Contains("GroundKit")&&n.ToLowerInvariant().Contains("earth"))
                    r.sharedMaterial=lower;
                else if(n.Contains("CompactFootprint")&&n.ToLowerInvariant().Contains("future terrace"))
                    r.sharedMaterial=upper;
            }
        }

        static void ApplyExact(string name,Material material)
        {
            if(material==null)return;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                if(r!=null&&r.enabled&&r.gameObject.activeInHierarchy&&r.gameObject.name==name)
                    r.sharedMaterial=material;
        }
    }
}
