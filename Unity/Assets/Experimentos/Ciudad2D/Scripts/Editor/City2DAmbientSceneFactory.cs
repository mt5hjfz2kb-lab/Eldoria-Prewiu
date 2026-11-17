#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D.Editor
{
    /// <summary>
    /// Builds only reversible atmospheric sprites in the existing isolated
    /// Valoria scene. The single I/V Bastion artwork and all build rules remain
    /// unchanged; one JSON owns effect positions for native/art QA.
    /// </summary>
    internal static class City2DAmbientSceneFactory
    {
        [Serializable] private sealed class AmbientDocument
        {
            public int schema, coordinate_width, coordinate_height, effect_count;
            public AmbientRecord[] effects;
        }
        [Serializable] private sealed class AmbientRecord
        {
            public string id, kind, sprite, owner;
            public int min_bastion, sort_offset;
            public float x, y, scale, opacity, rise_px, drift_px, period, phase;
        }
        private const string Root = "Assets/Experimentos/Ciudad2D/";
        private const string DataPath = Root + "QA/ambient_life_v1.json";
        private const string SpriteRoot = Root + "Sprites/";

        public static int Populate(Sprite terrain,
            Dictionary<string, CityBuilding> canonicalBuildings)
        {
            var source = AssetDatabase.LoadAssetAtPath<TextAsset>(DataPath);
            if (source == null) throw new InvalidOperationException("Ambient Life JSON missing");
            var document = JsonUtility.FromJson<AmbientDocument>(source.text);
            if (document == null || document.schema != 1 ||
                document.coordinate_width != 941 || document.coordinate_height != 1672 ||
                document.effects == null || document.effects.Length != 10 ||
                document.effect_count != document.effects.Length)
                throw new InvalidOperationException("Ambient Life data contract invalid");

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var effect in document.effects)
            {
                if (effect == null || string.IsNullOrEmpty(effect.id) ||
                    !ids.Add(effect.id) || effect.x < 30f || effect.x > 910f ||
                    effect.y < 40f || effect.y > 1630f ||
                    effect.scale <= .02f || effect.scale > .75f ||
                    effect.opacity <= 0f || effect.opacity > .55f ||
                    effect.rise_px < 4f || effect.rise_px > 45f ||
                    effect.drift_px < 0f || effect.drift_px > 22f ||
                    effect.period < 1f || effect.period > 8f ||
                    effect.min_bastion < 1 || effect.min_bastion > 5 ||
                    effect.sort_offset < 0 || effect.sort_offset > 8)
                    throw new InvalidOperationException("Invalid ambient parameters: "+effect?.id);

                bool smoke = effect.kind == "smoke";
                if (!smoke && effect.kind != "ember")
                    throw new InvalidOperationException("Unknown ambient kind: "+effect.kind);
                if (smoke != (effect.sprite == "Props/Humo_Valoria_A.png" ||
                             effect.sprite == "Props/Humo_Valoria_B.png") ||
                    !smoke && effect.sprite != "Props/HaloAntorcha.png")
                    throw new InvalidOperationException("Unexpected ambient source: "+effect.sprite);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteRoot+effect.sprite);
                if (sprite == null) throw new InvalidOperationException("Ambient source missing: "+effect.sprite);

                CityBuilding owner = null;
                if (!string.IsNullOrEmpty(effect.owner) &&
                    !canonicalBuildings.TryGetValue(effect.owner, out owner))
                    throw new InvalidOperationException("Unknown ambient building: "+effect.owner);
                if (smoke && owner == null)
                    throw new InvalidOperationException("Chimney smoke requires a completed building");

                var obj = new GameObject("AmbienteVivo_" + effect.id);
                obj.transform.position = new Vector3(
                    (effect.x / 941f - .5f) * terrain.bounds.size.x,
                    (.5f - effect.y / 1672f) * terrain.bounds.size.y,
                    -.05f);
                obj.transform.localScale = Vector3.one * effect.scale;
                var render = obj.AddComponent<SpriteRenderer>();
                render.sprite = sprite;
                render.sortingOrder = -Mathf.RoundToInt(
                    (.5f - effect.y / 1672f) * terrain.bounds.size.y * 100f)
                    + effect.sort_offset;
                // No physics or raycaster: these effects must not steal taps.
                obj.AddComponent<City2DAmbientMotion>().Configure(
                    effect.id,
                    smoke ? City2DAmbientMotion.MotionKind.Smoke :
                            City2DAmbientMotion.MotionKind.Ember,
                    owner, effect.min_bastion, effect.opacity,
                    effect.rise_px, effect.drift_px, effect.period, effect.phase);
            }
            return ids.Count;
        }
    }
}
#endif
