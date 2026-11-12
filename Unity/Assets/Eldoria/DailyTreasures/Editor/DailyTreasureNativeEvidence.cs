#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eldoria.Application;
using Eldoria.Domain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object=UnityEngine.Object;

namespace Eldoria.DailyTreasures.Editor
{
    /// <summary>
    /// Real Unity-rendered isolated component evidence. No canonical scene, save,
    /// old HUD or user data is touched. This deliberately does NOT certify the
    /// final city integration, motion, sound, iOS, or artistic approval.
    /// </summary>
    public static class DailyTreasureNativeEvidence
    {
        const string Folder="DailyTreasureNativeEvidence";
        sealed class SimulatedClock : IClock { public long UtcTicks { get; set; } }
        sealed class TransientStore : IStateStore
        {
            public PlayerState State;
            public PlayerState Load()=>State;
            public void Save(PlayerState state){State=state;}
        }
        [Serializable] sealed class Shot
        {
            public string file,sha256,view;
            public int width,height,bytes;
            public bool nativeCanvas,coversReward,readyToClaim,preserved;
        }
        [Serializable] sealed class Report
        {
            public string status,source,note;
            public int images,realGatewayClaims,graphicalPointerHits;
            // Separately audited ordinary-resource (non-chest) days.
            // They are additional REAL gateway claims on isolated saves and
            // native canvas pointer events, not part of the 52-shot contract.
            public int quietGatewayClaims,quietPointerHits,quietImages;
            public Shot[] quietShots;
            // These are artistic style probes only; they do not imply an
            // actual earned milestone, a claim or any gateway battle.
            public int visualOnlyMilestoneCaptures;
            public bool artApproved,fullSceneVerified,iosVerified,authoredSpritesImported;
            public Shot[] shots;
        }
        static void Need(bool ok,string reason)
        {
            if(!ok)throw new InvalidOperationException(reason);
        }
        // Source integrity checks run on Ubuntu. This second gate validates
        // what Unity actually imported, not merely the nine .meta texts.
        // Native art is still UNAPPROVED until the resulting images are seen.
        static void RequireNineImportedSprites()
        {
            var expected=new[]{
                (name:"treasure_closed_512",width:512,height:512,border:0),
                (name:"treasure_opened_512",width:512,height:512,border:0),
                (name:"valoria_frame_main_512",width:512,height:512,border:46),
                (name:"valoria_frame_card_256",width:256,height:256,border:31),
                (name:"valoria_frame_button_512x128",width:512,height:128,border:24),
                (name:"resource_wood",width:128,height:128,border:0),
                (name:"resource_stone",width:128,height:128,border:0),
                (name:"resource_hourglass",width:128,height:128,border:0),
                (name:"resource_food",width:128,height:128,border:0)
            };
            foreach(var item in expected)
            {
                var sprite=Resources.Load<Sprite>("DailyTreasureArt/"+item.name);
                Need(sprite!=null,"Unity did not import the authored Sprite: "+item.name);
                Need(Mathf.Abs(sprite.rect.width-item.width)<.01f &&
                     Mathf.Abs(sprite.rect.height-item.height)<.01f,
                     "Unity imported wrong Sprite dimensions: "+item.name);
                var importer=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite))
                    as TextureImporter;
                Need(importer!=null &&
                     importer.textureType==TextureImporterType.Sprite &&
                     importer.spriteImportMode==SpriteImportMode.Single &&
                     importer.alphaIsTransparency && !importer.mipmapEnabled,
                     "Actual Unity Sprite import settings invalid: "+item.name);
                var border=sprite.border;
                Need(Mathf.Abs(border.x-item.border)<.01f &&
                     Mathf.Abs(border.y-item.border)<.01f &&
                     Mathf.Abs(border.z-item.border)<.01f &&
                     Mathf.Abs(border.w-item.border)<.01f,
                     "Actual Unity Sprite nine-slice borders invalid: "+item.name);
            }
        }

        // The rehearsal deliberately has legacy + Vigilia canvases while
        // migrating, but must never leave two enabled or add an EventSystem.
        static void VerifyUiOwnership(int canvasCount,EventSystem eventSystem)
        {
            var canvases=Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include,FindObjectsSortMode.None);
            var events=Object.FindObjectsByType<EventSystem>(
                FindObjectsInactive.Include,FindObjectsSortMode.None);
            Need(canvases.Length==canvasCount &&
                 canvases.Count(c=>c.isActiveAndEnabled)==1 &&
                 events.Length==1 && events[0]==eventSystem,
                 "Duplicate enabled Canvas or EventSystem in treasury native proof");
        }
        static string Sha(string file)
        {
            using(var hash=SHA256.Create())
            using(var data=File.OpenRead(file))
                return BitConverter.ToString(hash.ComputeHash(data)).Replace("-","").ToLowerInvariant();
        }
        static long Day(int day)
            =>new DateTime(2026,11,day,12,0,0,DateTimeKind.Utc).Ticks;
        static Button FindButton(Component component,string name)
            =>component.GetComponentsInChildren<Button>(true)
                .Single(x=>x.name==name);
        static RectTransform FindRect(Component component,string name)
            =>component.GetComponentsInChildren<RectTransform>(true)
                .Single(x=>x.name==name);
        static void VerifyCriticalTypography(DailyTreasurePanel panel,string phase)
        {
            // UnityEngine.UI.Text.preferredHeight is obtained from the
            // ORIGINAL Unity text generator, with vertical overflow allowed
            // and the current actual rect width. This detects visually
            // truncated paid receipt/button text that static CSS mockups
            // cannot find, particularly in landscape.
            Canvas.ForceUpdateCanvases();
            string[] essential={
                "Reward heading","Claim label","Confirmed received items",
                "Actual paid count","Reward name","Day heading",
                "Consecutive visits","Actual reward contents"
            };
            int count=0;
            foreach(var label in panel.GetComponentsInChildren<Text>(true))
            {
                if(!label||!label.isActiveAndEnabled ||
                   !label.gameObject.activeInHierarchy ||
                   string.IsNullOrWhiteSpace(label.text) ||
                   !essential.Contains(label.name))
                    continue;
                var bounds=label.rectTransform.rect;
                Need(label.font && label.fontSize>=10,
                    "Missing real Unity font/legibility: "+label.name+" / "+phase);
                Need(bounds.width>=20f && bounds.height>=10f,
                    "Too-small text hitbox: "+label.name+" / "+phase);
                float required=label.preferredHeight;
                Need(!float.IsNaN(required) && !float.IsInfinity(required),
                    "Text generator returned invalid metrics: "+phase);
                // TextGenerator adds a few font-descender/layout pixels.
                // Allow a small measured slack but not a missing entire line.
                float slack=Mathf.Max(6f,bounds.height*.18f);
                Need(required<=bounds.height+slack,
                    "Unity truncated premium text ("+phase+") "+label.name+
                    " required="+required.ToString("F1")+
                    " available="+bounds.height.ToString("F1")+
                    " value="+label.text);
                // A one-line header must fit horizontally too. A taller
                // box or disabled clipping could hide a wrap/close overlap.
                if(label.name=="Reward heading")
                    Need(label.preferredWidth<=bounds.width+3f,
                        "Unity premium title exceeds reserved close-safe width ("+
                        phase+") requiredWidth="+label.preferredWidth+
                        " availableWidth="+bounds.width);
                count++;
            }
            // A milestone result intentionally shows only its main caption
            // and the Continue label; that is a valid, uncluttered scene.
            Need(count>=2,"Real native text audit had no meaningful visible labels: "+phase);
        }

        static void VerifySeventhDayPreviewTruth(DailyTreasurePanel panel,
            string phase)
        {
            // An actual paid day-seven screenshot displayed wood, stone and
            // streak food, while a FOURTH real reward (five-minute speedup)
            // was clipped from the calendar. Verify the exact promised payout
            // BEFORE claiming, using Unity's live Text and measured line box.
            var card=panel.GetComponentsInChildren<RectTransform>(true)
                .Single(x=>x.name==
                    "Day 7 / non-clickable informational card");
            var label=card.GetComponentsInChildren<Text>(true)
                .Single(x=>x.name=="Actual reward contents");
            const string expected=
                "Madera 180 · Piedra 120\nComida 80 · 5 min x1";
            Need(label.gameObject.activeInHierarchy && label.text==expected,
                "Calendar must disclose all four real day-seven items: "+phase+
                " observed="+(label?label.text:"<null>"));
            Canvas.ForceUpdateCanvases();
            var rect=label.rectTransform.rect;
            Need(label.preferredHeight<=rect.height+3f &&
                 label.preferredWidth<=rect.width+3f &&
                 label.fontSize>=12,
                "Day seven promises all four items but truncates the preview "+
                "in Unity ("+phase+") width="+rect.width+
                " requiredWidth="+label.preferredWidth+
                " height="+rect.height+
                " requiredHeight="+label.preferredHeight);
        }

        static void VerifyCalendarMultipleResourceMargins(DailyTreasurePanel panel,
            string phase)
        {
            // The first real Unity screenshot showed Day 4 and Day 6 second
            // lines colliding with the lower nine-slice bronze frame. Verify
            // the exact RectTransform bounds and font geometry, not only that
            // data was correctly awarded in LocalGateway.
            foreach(int day in new[]{4,6})
            {
                string name="Day "+day+" / non-clickable informational card";
                var card=panel.GetComponentsInChildren<RectTransform>(true)
                    .Single(x=>x.name==name);
                var label=card.GetComponentsInChildren<Text>(true)
                    .Single(x=>x.name=="Actual reward contents");
                Need(label.gameObject.activeInHierarchy &&
                     label.text.Split('\n').Length==2,
                     "Expected two real resources on day "+day+" / "+phase);
                float margin=label.rectTransform.anchoredPosition.y+
                    label.rectTransform.rect.yMin-card.rect.yMin;
                Need(margin>=4.5f,
                     "Two-line day "+day+" painted onto bottom frame, "+
                     "margin="+margin.ToString("F1")+" / "+phase);
                Need(label.preferredHeight<=label.rectTransform.rect.height+6f,
                     "Two-line day "+day+" truncated in Unity Text / "+phase);
            }
        }

        static void VerifyCleanPaintedResources(DailyTreasurePanel panel,
            int earnedCount=0)
        {
            var day=panel.GetComponentsInChildren<Image>(true)
                .Where(x=>x.name=="Premium illustrated reward / no rays")
                .ToArray();
            string[] names={
                "resource_wood_clean_256","resource_stone_clean_256",
                "resource_hourglass_clean_256","treasure_closed_512",
                "resource_hourglass_clean_256","resource_food_clean_256",
                "treasure_closed_512"
            };
            Need(day.Length==names.Length,
                "Missing premium calendar illustration on one or more days");
            for(int i=0;i<names.Length;i++)
                Need(day[i].sprite==Resources.Load<Sprite>(
                         "DailyTreasureArt/"+names[i]) &&
                     day[i].preserveAspect,
                    "Calendar fallback displayed a screenshot-derived or clipped "+
                    "resource icon on day "+(i+1));

            if(earnedCount>0)
            {
                var rewards=panel.GetComponentsInChildren<Image>(true)
                    .Where(x=>x.name=="Real award icon").ToArray();
                Need(rewards.Length==4,"Original four-card premium tray missing");
                string[] paidIcons=earnedCount==4?
                    new[]{"resource_wood_clean_256","resource_stone_clean_256",
                          "resource_food_clean_256","resource_hourglass_clean_256"}:
                    new[]{"resource_wood_clean_256","resource_stone_clean_256"};
                for(int i=0;i<earnedCount;i++)
                    Need(rewards[i].sprite==Resources.Load<Sprite>(
                             "DailyTreasureArt/"+paidIcons[i]) &&
                         rewards[i].preserveAspect,
                        "Paid reward card used wrong/screenshot-derived icon "+i);
            }
        }

        static Rect BoundsInCard(RectTransform child)
        {
            var r=child.rect;
            return new Rect(child.anchoredPosition.x+r.x,
                child.anchoredPosition.y+r.y,r.width,r.height);
        }
        static void VerifyPremiumDailyComposition(
            DailyTreasurePanel panel,int width,int height,bool opening)
        {
            // The old native opening was a small 160px drawing sitting above
            // 100+px of empty parchment. Verify physical composition in
            // REAL Unity, not only hand-calculated rectangles or concept art.
            bool portrait=height>width;
            var stage=FindRect(panel,"Basalt and worn bronze reward parchment");
            var chest=FindRect(panel,"Authored chest / opened");
            var receipt=FindRect(panel,"Confirmed received items");
            var shelf=FindRect(panel,"Actually earned inventory reveal / no payout");
            float expectedW=Mathf.Min(width-24f,portrait?440f:650f);
            float expectedH=Mathf.Min(height-20f,portrait?
                (opening?450f:700f):(opening?300f:370f));
            Need(panel.IsOpen &&
                 Mathf.Abs(stage.rect.width-expectedW)<1.1f &&
                 Mathf.Abs(stage.rect.height-expectedH)<1.1f,
                "Premium daily ceremony retained an oversized blank frame");
            float chestTop=chest.anchoredPosition.y+chest.rect.height*.5f;
            float chestBottom=chest.anchoredPosition.y-chest.rect.height*.5f;
            float captionTop=receipt.anchoredPosition.y+
                receipt.rect.height*.5f;
            float captionBottom=receipt.anchoredPosition.y-
                receipt.rect.height*.5f;
            Need(chestTop<=stage.rect.height*.5f-8f &&
                 chestBottom>=captionTop+(opening?16f:1f),
                "Original chest overlaps its real-reward title or frame");
            if(opening)
            {
                Need(!shelf.gameObject.activeInHierarchy &&
                     Mathf.Abs(chest.rect.height-
                         (portrait?265f:194f))<1.1f &&
                     captionBottom>=-stage.rect.height*.5f+15f,
                    "Cinematic opening still has an empty lower third or "
                    +"exposes unearned item cards before the reveal");
            }
            else
            {
                Need(shelf.gameObject.activeInHierarchy &&
                     panel.EarnedItemCardCount>=2,
                    "Final premium receipt lost its actual paid inventory");
                float shelfTop=shelf.anchoredPosition.y+
                    shelf.rect.height*.5f;
                float shelfBottom=shelf.anchoredPosition.y-
                    shelf.rect.height*.5f;
                Need(portrait?shelfBottom>=captionTop+1f:
                     captionBottom>=shelfTop+1f,
                    "Paid inventory cards overlap the item receipt title");
                var cards=panel.GetComponentsInChildren<RectTransform>(true)
                    .Where(x=>x.name.StartsWith("Earned item ",
                            StringComparison.Ordinal))
                    .OrderBy(x=>x.name,StringComparer.Ordinal).ToArray();
                for(int i=0;i<panel.EarnedItemCardCount;i++)
                    Need(cards[i].gameObject.activeInHierarchy &&
                         cards[i].rect.width>=(portrait?150f:140f) &&
                         cards[i].rect.height>=70f,
                        "Real paid item remains a miniature unpainted tile: "+i);
            }
        }

        static void VerifyContentSizedQuietReward(
            DailyTreasurePanel panel,int count,int width,int height)
        {
            // Days 1,2,3,5,6 MUST feel like a real premium inventory
            // confirmation, not a single 175px icon lost inside a 740px
            // ornate modal with mostly empty stone.
            Need(count==1 || count==2,"Only true ordinary receipt layouts");
            bool portrait=height>width;
            float expectedWidth=Mathf.Min(width-24f,portrait?
                (count==1?336f:410f):(count==1?340f:448f));
            float expectedHeight=Mathf.Min(height-20f,portrait?430f:330f);
            var main=FindRect(panel,
                "Basalt and worn bronze reward parchment");
            Need(Mathf.Abs(main.rect.width-expectedWidth)<=1.1f &&
                 Mathf.Abs(main.rect.height-expectedHeight)<=1.1f &&
                 main.rect.width<=448f,
                "Ordinary paid item occupies giant mostly empty artwork");
            var shelf=FindRect(panel,
                "Actually earned inventory reveal / no payout");
            Need(shelf.gameObject.activeInHierarchy &&
                 shelf.rect.width>=expectedWidth-35f &&
                 shelf.rect.width<=expectedWidth-33f,
                "Centered paid-item shelf does not match its small card frame");
            var cards=panel.GetComponentsInChildren<RectTransform>(true)
                .Where(t=>t.name.StartsWith("Earned item ",
                    StringComparison.Ordinal))
                .OrderBy(t=>t.name,StringComparer.Ordinal).ToArray();
            float maxRight=float.NegativeInfinity;
            float minLeft=float.PositiveInfinity;
            for(int i=0;i<count;i++)
            {
                var item=cards[i];
                Need(item.gameObject.activeInHierarchy &&
                     item.rect.height>=(portrait?129f:115f)-1f &&
                     item.rect.width>=150f &&
                     item.rect.width<=175.5f,
                    "A genuine quiet reward became miniature or was clipped");
                minLeft=Mathf.Min(minLeft,
                    item.anchoredPosition.x-item.rect.width*.5f);
                maxRight=Mathf.Max(maxRight,
                    item.anchoredPosition.x+item.rect.width*.5f);
            }
            Need(Mathf.Abs(minLeft+maxRight)<1.1f,
                "Ordinary illustrated items are not centered in their receipt");
        }

        static void VerifyEarnedItemReadability(DailyTreasurePanel panel,int count)
        {
            // The first genuine Unity screenshots rendered four almost-empty
            // frames: 10-16px-high legacy labels disappeared even though icon
            // Sprite integrity tests and typography slack checks passed.
            // Test the REAL credited item name/quantity boxes, layout bounds
            // and disjoint painted icons, never just whether they exist.
            var cards=panel.GetComponentsInChildren<RectTransform>(true)
                .Where(x=>x.name.StartsWith("Earned item ",StringComparison.Ordinal))
                .OrderBy(x=>x.name,StringComparer.Ordinal).ToArray();
            Need(cards.Length==4,"Genuine receipt should have four reusable cards");
            for(int i=0;i<count;i++)
            {
                var card=cards[i];
                Need(card.gameObject.activeInHierarchy,
                    "A confirmed paid item has no visible card "+i);
                var title=card.GetComponentsInChildren<Text>(true)
                    .Single(x=>x.name=="Reward name");
                var amount=card.GetComponentsInChildren<Text>(true)
                    .Single(x=>x.name=="Actual paid count");
                var icon=card.GetComponentsInChildren<Image>(true)
                    .Single(x=>x.name=="Real award icon");
                Need(!string.IsNullOrWhiteSpace(title.text) &&
                     !string.IsNullOrWhiteSpace(amount.text) &&
                     icon.sprite && title.font && amount.font,
                     "Claimed resource is missing its own painted receipt "+i);
                Need(title.rectTransform.rect.height>=title.fontSize*1.35f &&
                     amount.rectTransform.rect.height>=amount.fontSize*1.35f,
                     "Paid card text box is shorter than a real font line "+i);
                var labelBounds=BoundsInCard(title.rectTransform);
                var amountBounds=BoundsInCard(amount.rectTransform);
                var artBounds=BoundsInCard(icon.rectTransform);
                var outer=card.rect;
                foreach(var bounds in new[]{labelBounds,amountBounds,artBounds})
                    Need(bounds.xMin>=outer.xMin-1f &&
                         bounds.xMax<=outer.xMax+1f &&
                         bounds.yMin>=outer.yMin-1f &&
                         bounds.yMax<=outer.yMax+1f,
                         "Actual paid receipt is off-card: "+i);
                Need(!labelBounds.Overlaps(amountBounds) &&
                     !labelBounds.Overlaps(artBounds) &&
                     !amountBounds.Overlaps(artBounds),
                     "The claimed resource icon/name/quantity overlap in Unity "+i);
                Need(title.preferredHeight<=title.rectTransform.rect.height+2f &&
                     amount.preferredHeight<=amount.rectTransform.rect.height+2f,
                     "A genuine painted paid item still clips its letters "+i);
            }
        }
        // A sprite reference or enabled Image is NOT proof that Lyra is
        // visible: a later, opaque alpha layer can hide the real actor.
        // Compare TWO genuine Unity camera renders, changing ONLY the
        // visual hero Image.enabled flag. This is a visual-only GPU readback,
        // never a fake hero unlock or reward, and does not alter the save.
        static Color32[] ReadNativePixels(Camera camera,RenderTexture rt,
            int width,int height)
        {
            var previous=RenderTexture.active;
            var frame=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                Canvas.ForceUpdateCanvases();
                camera.Render();
                camera.Render();
                RenderTexture.active=rt;
                frame.ReadPixels(new Rect(0,0,width,height),0,0);
                frame.Apply(false);
                return frame.GetPixels32();
            }
            finally
            {
                RenderTexture.active=previous;
                Object.DestroyImmediate(frame);
            }
        }

        static void VerifyActualLyraVisibleInNativePixels(
            DailyTreasurePanel panel,Camera camera,RenderTexture rt,
            int width,int height)
        {
            var hero=FindRect(panel,
                "Canonical Lyra / actual recruited hero illustration")
                .GetComponent<Image>();
            Need(hero && hero.sprite && hero.isActiveAndEnabled &&
                 hero.color.a>=.7f && hero.rectTransform.rect.width>=96f &&
                 hero.rectTransform.rect.height>=96f,
                 "Authentic Lyra sprite missing, transparent or postage-stamp sized");
            var original=Resources.Load<Sprite>(
                "DailyTreasureCeremonies/canonical_lyra_illustration");
            Need(original && hero.sprite==original,
                "Lyra ceremony reverted to a model render instead of the original illustration");
            var texture=original.texture;
            Need(texture,
                "Original Lyra illustration Sprite references no imported Texture2D");
            var sourcePath=AssetDatabase.GetAssetPath(original);
            var sourceImporter=AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            Debug.Log("NATIVE_CANONICAL_LYRA_IMPORT_STATE source="+sourcePath+
                " imported="+texture.width+"x"+texture.height+
                " readable="+texture.isReadable+
                " importerReadable="+(sourceImporter?sourceImporter.isReadable.ToString():"missing")+
                " defaultMax="+(sourceImporter?sourceImporter.maxTextureSize.ToString():"missing")+
                " standaloneMax="+(sourceImporter?sourceImporter.GetPlatformTextureSettings("Standalone").maxTextureSize.ToString():"missing"));
            // Editor textures remain CPU-accessible even when player Read/Write
            // is disabled. Validate the actual importer, not Editor residency.
            // Official explanation: unity.com/blog/engine-platform/accessing-texture-data-efficiently
            Need(sourceImporter && !sourceImporter.isReadable &&
                 !sourceImporter.mipmapEnabled && sourceImporter.maxTextureSize<=1024,
                "Canonical original Lyra importer must be non-readable, mipless and bounded");
            Need(sourcePath=="Assets/Eldoria/DailyTreasures/Resources/DailyTreasureCeremonies/canonical_lyra_illustration.png",
                "Native Lyra did not resolve the original-reference asset path");
            Need(Sha(Path.Combine(UnityEngine.Application.dataPath,
                sourcePath.Substring("Assets/".Length)))==
                "58cca82816e86ab8ae70ee29b3b941db591e80ecc85148da55487beecb1a4fef",
                "Native Lyra is not the exact original-reference extraction");
            Need(texture.width<=1024 && texture.height<=1024,
                "Canonical original Lyra exceeded bounded 1024 import: "+
                texture.width+"x"+texture.height);
            var withHero=ReadNativePixels(camera,rt,width,height);
            Color32[] withoutHero;
            try
            {
                hero.enabled=false;
                withoutHero=ReadNativePixels(camera,rt,width,height);
            }
            finally
            {
                hero.enabled=true;
                Canvas.ForceUpdateCanvases();
            }
            Need(withHero.Length==withoutHero.Length,
                "Native GPU portrait pixel comparison dimensions diverged");
            int visible=0;
            for(int i=0;i<withHero.Length;i++)
            {
                int d=Math.Abs(withHero[i].r-withoutHero[i].r)+
                      Math.Abs(withHero[i].g-withoutHero[i].g)+
                      Math.Abs(withHero[i].b-withoutHero[i].b);
                if(d>36)visible++;
            }
            int minimum=Math.Max(350,Mathf.RoundToInt(width*height*.002f));
            Need(visible>=minimum,
                "Authentic Lyra was hidden behind ornamental layers or "+
                "too faint in actual Unity GPU pixels: "+visible+
                " changed pixels; minimum "+minimum+" at "+width+"x"+height);
            Debug.Log("NATIVE_CANONICAL_LYRA_VISIBLE: "+visible+
                " altered scene pixels at "+width+"x"+height+
                " without changing hero identity or gameplay");
        }

        static void VerifyGenuineSceneComposition(
            DailyTreasurePanel panel,string eventId,int width,int height)
        {
            var expected=DailyTreasureStageDirection.Compose(eventId,width,height);
            Need(panel.ActiveMilestoneStage==expected.Kind,
                "Real event is presented with the wrong category art/layout");
            var painted=FindRect(panel,
                "Genuine Eldoria milestone scenery / visual only");
            var insignia=FindRect(panel,
                "Authentic Eldoria event insignia / visual only")
                .GetComponent<Image>();
            var expectedInsignia=expected.Kind==DailyTreasureStageKind.Explorers?
                "EldoriaRetention/scouts-crest":
                expected.Kind==DailyTreasureStageKind.Vigilants?
                "EldoriaRetention/vigilants-compass":null;
            Need(expectedInsignia==null?
                    !insignia.gameObject.activeInHierarchy:
                    insignia.gameObject.activeInHierarchy &&
                    insignia.sprite==Resources.Load<Sprite>(expectedInsignia) &&
                    !insignia.raycastTarget,
                "Non-canonical or missing real Eldoria milestone insignia");
            var trueLyra=FindRect(panel,
                "Canonical Lyra / actual recruited hero illustration")
                .GetComponent<Image>();
            Need(trueLyra && !trueLyra.raycastTarget,
                "Native Lyra illustration may not intercept gameplay input");
            if(expected.Kind!=DailyTreasureStageKind.Lyra)
                Need(!trueLyra.gameObject.activeInHierarchy &&
                     !trueLyra.sprite,
                     "Lyra original character leaked into another ceremony");
            else
                Need(panel.DisplaysCanonicalLyraPortrait &&
                     trueLyra.sprite && trueLyra.sprite.rect.width>=256f &&
                     trueLyra.sprite.rect.height>=256f,
                     "Native Lyra milestone has no visible authentic hero. "+
                     "A hall background and transparent arch cannot replace "+
                     "the canonical illustrated character. Verify the original "+
                     "reference extraction and native pixels before certifying art.");
            var chest=FindRect(panel,"Authored chest / opened");
            var closedChest=FindRect(panel,"Authored chest / closed");
            var fallbackChest=FindRect(panel,
                "Real animated chest / two-layer bronze oak graphic");
            Need(!panel.DisplaysCeremonialChest &&
                 !chest.gameObject.activeSelf &&
                 !closedChest.gameObject.activeSelf &&
                 !fallbackChest.gameObject.activeSelf,
                 "A milestone incorrectly repeats the DAILY loot chest");
            var receipt=FindRect(panel,"Confirmed received items");
            var banner=FindRect(panel,
                "Milestone heraldic event banner / visual only");
            Need(banner.gameObject.activeInHierarchy,
                "Original event-specific heraldic banner is hidden");
            // Actual player-facing scene identity must be a readable,
            // unobstructed literary heading on genuine painterly art, not
            // another clickable bronze button pasted above a nested panel.
            var title=banner.GetComponentsInChildren<Text>(true)
                .Single(t=>t.name=="Real milestone category");
            var headingPlate=banner.GetComponent<Image>();
            var headingRule=FindRect(panel,
                "Event-specific forged-ink underline / visual only");
            var headingRuleImage=headingRule.GetComponent<Image>();
            Need(headingPlate && !headingPlate.sprite &&
                 !headingPlate.raycastTarget &&
                 headingPlate.color.a<=.66f && title.font &&
                 title.fontSize>=16 &&
                 title.preferredHeight<=title.rectTransform.rect.height+2f &&
                 title.preferredWidth<=title.rectTransform.rect.width+2f &&
                 headingRule.rect.height>=1.9f &&
                 headingRule.rect.height<=2.1f &&
                 headingRuleImage && !headingRuleImage.raycastTarget &&
                 headingRuleImage.color.a<=.75f,
                "A real milestone title is cropped, disguised as a button "
                +"or styled with a heavy opaque overlay");
            // The original banner/Skip overlap on portrait made the upper
            // story title appear literally pasted over another control.
            // Compare the two actual Unity RectTransforms in their SHARED
            // parent coordinates, not separate anchoredPosition origins.
            var skip=FindRect(panel,"Skip animation / keep earned items");
            var titleBounds=RectTransformUtility.CalculateRelativeRectTransformBounds(
                banner.parent,banner);
            var skipBounds=RectTransformUtility.CalculateRelativeRectTransformBounds(
                banner.parent,skip);
            bool xOverlap=Mathf.Abs(titleBounds.center.x-skipBounds.center.x)<
                (titleBounds.size.x+skipBounds.size.x)*.5f;
            bool yOverlap=Mathf.Abs(titleBounds.center.y-skipBounds.center.y)<
                (titleBounds.size.y+skipBounds.size.y)*.5f;
            Need(!(xOverlap&&yOverlap),
                "Original milestone text overlaps the real Skip finger target");
            var theatre=DailyTreasureStageDirection.NarrativeTheatreFrame(width,height);
            if(expected.Kind==DailyTreasureStageKind.Bastion)
            {
                Need(painted.anchoredPosition.sqrMagnitude<1.21f &&
                     Mathf.Abs(painted.rect.width-width)<1.1f &&
                     Mathf.Abs(painted.rect.height-height)<1.1f,
                     "Real Bastion must cover the whole active viewport without a second camera at its edges");
            }
            else
                Need(painted.anchoredPosition.sqrMagnitude<1.21f &&
                     Mathf.Abs(painted.rect.width-theatre.Width+24f)<1.1f &&
                     Mathf.Abs(painted.rect.height-theatre.Height+24f)<1.1f,
                     "Single original setting must fill the native theatre interior for "+expected.Kind);
            Need(Mathf.Abs(receipt.anchoredPosition.x-expected.ReceiptX)<1.1f &&
                 Mathf.Abs(receipt.anchoredPosition.y-expected.ReceiptY)<1.1f &&
                 Mathf.Abs(receipt.rect.width-expected.ReceiptW)<1.1f &&
                 Mathf.Abs(receipt.rect.height-expected.ReceiptH)<1.1f,
                 "Actual event report is not placed legibly beside the event art");
            var screen=FindRect(panel,"Tributo de los siete días / modal");
            var backdrop=screen.GetComponent<Image>();
            bool worldThrough=expected.Kind==DailyTreasureStageKind.Bastion &&
                !panel.HasMilestoneScenery;
            float expectedBackdrop=worldThrough?
                DailyTreasureStageDirection.BastionWorldThroughBackdropOpacity:
                expected.BackdropOpacity;
            Need(backdrop &&
                 Mathf.Abs(backdrop.color.a-expectedBackdrop)<.03f,
                "The Bastion city / event stage is too opaque or too faint");
            // The Bastion is ONE full-theatre 3D image, with its masonry
            // fitted to the protected subject region BEFORE rendering. Its
            // full background intentionally extends behind the report. This
            // isolated fixture has no city, so it cannot certify projected
            // masonry bounds; the real-city fixture must do that separately.
            // Foreground reservations and factual receipt remain separate;
            // the single scenic setting intentionally continues behind both.
            if(expected.Kind==DailyTreasureStageKind.Bastion)
                Need(!panel.HasMilestoneScenery,
                    "Do not disguise a Bastion level by painting a fake city");
            else
                Need(panel.HasMilestoneScenery,
                    "Original real Eldoria artwork is absent from the milestone");
            var fullWorld=FindRect(panel,
                "Genuine story world / cinematic full-bleed atmosphere")
                .GetComponent<RawImage>();
            var originalWorld=painted.GetComponent<RawImage>();
            Need(!fullWorld.gameObject.activeInHierarchy,
                "Duplicate scenic crop would repeat architecture and map boundaries");
            if(expected.Kind!=DailyTreasureStageKind.Bastion)
            {
                Need(originalWorld.gameObject.activeInHierarchy &&
                     originalWorld.texture && !originalWorld.raycastTarget,
                    "Single genuine narrative setting is missing or blocks input");
                var uv=originalWorld.uvRect;
                float paintedAspect=originalWorld.texture.width*uv.width/
                    (originalWorld.texture.height*uv.height);
                float screenAspect=originalWorld.rectTransform.rect.width/
                    originalWorld.rectTransform.rect.height;
                Need(Mathf.Abs(paintedAspect-screenAspect)<.02f &&
                     uv.width>0f && uv.height>0f &&
                     uv.x>=0f && uv.y>=0f &&
                     uv.xMax<=1.001f && uv.yMax<=1.001f,
                     "Single full-bleed scenery stretches or repeats real source");
            }
            if(expected.Kind==DailyTreasureStageKind.Vigilants)
                Need(insignia.material && insignia.material.shader &&
                     insignia.material.shader.name==
                         "Eldoria/DailyHeraldicRelief",
                    "Original Vigilants compass still appears as a flat sticker");
            var uniqueStage=FindRect(panel,
                "Unique Eldoria event stage / transparent foreground");
            if(expected.Kind==DailyTreasureStageKind.Bastion)
                Need(!panel.HasDistinctMilestoneArt &&
                     panel.MilestoneArtworkResourceKey==null &&
                     !uniqueStage.gameObject.activeInHierarchy,
                    "Painted fake arch blocks authentic Bastion close-up");
            else if(expected.Kind==DailyTreasureStageKind.Lyra)
                Need(panel.DisplaysCanonicalLyraPortrait &&
                     !panel.HasDistinctMilestoneArt &&
                     panel.MilestoneArtworkResourceKey==null &&
                     !uniqueStage.gameObject.activeInHierarchy,
                    "Old portrait arch covers the original heroine; use her actual illustration and hall");
            else
                Need(panel.HasDistinctMilestoneArt &&
                     panel.MilestoneArtworkResourceKey==
                     "DailyTreasureCeremonies/"+
                     expected.Kind.ToString().ToLowerInvariant()+"_stage" &&
                     uniqueStage.GetComponent<Image>() &&
                     !uniqueStage.GetComponent<Image>().raycastTarget,
                    "Genuine category-specific layered art is missing");
            var ink=FindRect(panel,"Bastion / real-city milestone caption ink");
            var plate=FindRect(panel,
                "Basalt and worn bronze reward parchment").GetComponent<Image>();
            var ornament=FindRect(panel,
                "Authored narrative single-frame ornament / no hitbox")
                .GetComponent<Image>();
            if(width>height)
            {
                // A previous non-sliced 1376x768 frame was squeezed into
                // 740x370: real metal corners distorted ~11.6% across axes.
                // The signed source has 90px borders at 200 source PPU:
                // with reference PPU 100, all four corners remain 45x45 UI
                // pixels regardless of landscape size. One border only.
                var border=ornament && ornament.sprite?
                    ornament.sprite.border:Vector4.zero;
                Need(ornament && ornament.gameObject.activeInHierarchy &&
                     ornament.type==Image.Type.Sliced &&
                     ornament.sprite && !ornament.raycastTarget &&
                     Mathf.Abs(ornament.sprite.pixelsPerUnit-200f)<.1f &&
                     Mathf.Abs(border.x-90f)<.1f &&
                     Mathf.Abs(border.y-90f)<.1f &&
                     Mathf.Abs(border.z-90f)<.1f &&
                     Mathf.Abs(border.w-90f)<.1f &&
                     plate && !plate.sprite,
                    "Real forged bronze UI corners are stretched or "+
                    "two ornate metal frames overlap in landscape");
            }
            else
                Need(ornament && !ornament.gameObject.activeInHierarchy &&
                     plate && plate.sprite && plate.type==Image.Type.Sliced,
                    "Portrait must retain original nine-slice framing; "+
                    "wide bronze artwork must not be distorted vertically");
            if(expected.Kind==DailyTreasureStageKind.Bastion)
            {
                float expectedPlate=worldThrough?
                    DailyTreasureStageDirection.BastionWorldThroughPlateOpacity:
                    DailyTreasureStageDirection.BastionPlateOpacity;
                Need(ink.gameObject.activeInHierarchy && plate &&
                     Mathf.Abs(plate.color.a-expectedPlate)<.015f,
                     "Authentic Bastion hidden by wrong modal plate opacity");
                float liveLight=(1f-backdrop.color.a)*(1f-plate.color.a);
                Need(liveLight>(worldThrough?.70f:.40f),
                     "The original true Bastion city is too dark");
                Need(Mathf.Abs(ink.rect.width-expected.ReceiptW-16f)<1.1f &&
                     Mathf.Abs(ink.rect.height-expected.ReceiptH-12f)<1.1f &&
                     ink.GetComponent<Image>().color.a>.75f,
                     "City visible but the committed Bastion story is illegible");
            }
            else Need(ink.gameObject.activeInHierarchy &&
                Mathf.Abs(ink.anchoredPosition.x-expected.ReceiptX)<1.1f &&
                Mathf.Abs(ink.anchoredPosition.y-expected.ReceiptY)<1.1f &&
                Mathf.Abs(ink.rect.width-expected.ReceiptW-16f)<1.1f &&
                Mathf.Abs(ink.rect.height-expected.ReceiptH-12f)<1.1f &&
                Mathf.Abs(ink.GetComponent<Image>().color.a-.67f)<.02f,
                "Authentic painterly reading must use one bounded ink patch "+
                "on the committed receipt, not globally black out artwork");
        }
        static void ClickAtNativeScreenPosition(Canvas canvas,EventSystem events,
            Camera camera,Button button,int width,int height)
        {
            Need(button && button.gameObject.activeInHierarchy && button.interactable,
                "Button not active or interactable: "+(button ? button.name : "null"));
            var corners=new Vector3[4];
            ((RectTransform)button.transform).GetWorldCorners(corners);
            var bottomLeft=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);
            var topRight=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
            Need(topRight.x-bottomLeft.x>=43.5f &&
                 topRight.y-bottomLeft.y>=43.5f,
                "Touch area below 44 physical pixels: "+button.name);
            Need(bottomLeft.x>=-2f && bottomLeft.y>=-2f &&
                 topRight.x<=width+2f && topRight.y<=height+2f,
                "Real touch control offscreen: "+button.name);
            var pos=(bottomLeft+topRight)*.5f;
            var data=new PointerEventData(events){
                position=pos,pointerId=-1,button=PointerEventData.InputButton.Left
            };
            var hits=new List<RaycastResult>();
            canvas.GetComponent<GraphicRaycaster>().Raycast(data,hits);
            var entryImage=button.GetComponent<Graphic>();
            var localGraphics=GraphicRegistry.GetGraphicsForCanvas(canvas);
            string diagnostic=
                " canvas="+canvas.name+" target="+pos+
                " hit="+(hits.Count>0?hits[0].gameObject.name:"<none>")+
                " raycasts="+hits.Count+
                " registered="+(localGraphics==null?0:localGraphics.Count)+
                " entryRegistered="+(entryImage &&
                    localGraphics!=null && localGraphics.Contains(entryImage))+
                " depth="+(entryImage?entryImage.depth:-999)+
                " culled="+(entryImage && entryImage.canvasRenderer.cull)+
                " underPointer="+RectTransformUtility.RectangleContainsScreenPoint(
                    (RectTransform)button.transform,pos,camera)+
                " activeRaycaster="+
                    canvas.GetComponent<GraphicRaycaster>().isActiveAndEnabled;
            Need(hits.Count>0 &&
                 (hits[0].gameObject==button.gameObject ||
                  hits[0].gameObject.transform.IsChildOf(button.transform)),
                 (hits.Count==0?"No native graphic received real pointer: ":
                     "Another graphic steals native click: ")+
                 button.name+diagnostic);
            // Invoke the exact Unity event handlers used by real players.
            // NEVER bypass the pointer by directly invoking onClick.
            ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerClickHandler);
        }

        [MenuItem("Eldoria/Tesoros de Valoria/Captura Unity nativa de componente")]
        public static void Capture()
        {
            string output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../"+Folder));
            // Windows self-hosted jobs retain an earlier sparse checkout. A prior
            // failed proof left stale PNGs beside fresh ones; counting them as
            // this run's captures corrupts the evidence ledger. Remove ONLY
            // this method's generated output (never Assets, saves or other jobs).
            // The GitHub job already enforces exclusive CAS ownership.
            string project=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,".."));
            Need(string.Equals(Path.GetDirectoryName(output),project,
                     StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(Path.GetFileName(output),Folder,
                     StringComparison.Ordinal),
                 "Refuse to clear any directory outside the isolated native proof");
            if(Directory.Exists(output))Directory.Delete(output,true);
            Directory.CreateDirectory(output);
            var report=new Report{
                status="NATIVE_COMPONENT_PROOF_PENDING",
                source=Environment.GetEnvironmentVariable("GITHUB_SHA")??"local",
                note="One real Unity UI component with a disposable gateway, not the canonical city scene. This does not certify animations in time or owner artistic quality.",
                artApproved=false,fullSceneVerified=false,iosVerified=false
            };
            var pictures=new List<Shot>();
            var quietPictures=new List<Shot>();
            int inputChecks=0,quietInputChecks=0;
            var quietOutput=Path.Combine(output,"quiet");
            Directory.CreateDirectory(quietOutput);
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                RequireNineImportedSprites();
                report.authoredSpritesImported=true;
                foreach(var resolution in new []{(1600,900),(900,1600),(844,390),(390,844)})
                {
                    int width=resolution.Item1,height=resolution.Item2;
                    var clock=new SimulatedClock{UtcTicks=Day(1)};
                    var store=new TransientStore();
                    var gateway=new LocalGateway(clock,store);
                    // Seed previous six days using real commands, not direct
                    // mutations of game resources, reward counts or inbox.
                    for(int day=1;day<=6;day++)
                    {
                        clock.UtcTicks=Day(day);
                        var state=gateway.Snapshot();
                        var cmd=new GameCommand("qa-daily-"+day,state.PlayerId,
                            "ClaimDailyTreasure","valoria",state.Revision);
                        Need(gateway.Execute(cmd).Ok,"Real gateway seeding failed: "+day);
                    }
                    clock.UtcTicks=Day(7);
                    var before=gateway.Snapshot();
                    Need(before.DailyTreasures.CycleDay==6 && before.Mailbox.Messages.Count>=6,
                        "Day-seven candidate has no honest prior claim ledger");
                    var go=new GameObject("Disposable reward-capture camera");
                    var camera=go.AddComponent<Camera>();
                    camera.clearFlags=CameraClearFlags.SolidColor;
                    camera.backgroundColor=new Color(.025f,.028f,.030f,1f);
                    camera.orthographic=true;camera.orthographicSize=height*.005f;
                    camera.transform.position=new Vector3(0f,0f,-25f);
                    camera.nearClipPlane=.1f;camera.farClipPlane=100f;
                    camera.aspect=width/(float)height;
                    camera.cullingMask=1<<5;
                    // The project uses URP. A hand-created Camera without its
                    // pipeline metadata may silently render an empty frame.
                    var pipeline=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
                    if(pipeline!=null)
                    {
                        var additional=System.Type.GetType(
                            "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
                        Need(additional!=null,
                            "Active URP requires UniversalAdditionalCameraData on the disposable QA camera");
                        go.AddComponent(additional);
                    }
                    var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
                    camera.targetTexture=rt;
                    var canvasObj=new GameObject("Disposable native uGUI component",
                        typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
                    canvasObj.layer=5;
                    var rect=(RectTransform)canvasObj.transform;
                    rect.sizeDelta=new Vector2(width,height);
                    rect.localScale=Vector3.one*.01f;
                    var canvas=canvasObj.GetComponent<Canvas>();
                    canvas.renderMode=RenderMode.WorldSpace;
                    canvas.worldCamera=camera;
                    var panel=DailyTreasurePanel.Install(rect);
                    panel.Bind(gateway,()=>clock.UtcTicks,()=>{});
                    // Imported assets alone are NOT evidence of their use.
                    // Fail rather than reporting an authored-art PASS when the
                    // component quietly falls back to procedural geometry.
                    Need(panel.HasAuthoredChestArt,
                        "Native panel lost the two commissioned chest poses");
                    VerifyCleanPaintedResources(panel);
                    var exactClosed=FindRect(panel,
                        "Authored chest / closed").GetComponent<Image>();
                    var exactOpened=FindRect(panel,
                        "Authored chest / opened").GetComponent<Image>();
                    Need(exactClosed && exactOpened &&
                         exactClosed.sprite==Resources.Load<Sprite>(
                            "DailyTreasureArt/treasure_closed_512") &&
                         exactOpened.sprite==Resources.Load<Sprite>(
                            "DailyTreasureArt/treasure_opened_512") &&
                         exactClosed.preserveAspect && exactOpened.preserveAspect,
                         "Native UI does not display the two original imported chest sprites");
                    report.authoredSpritesImported &=
                        panel.HasAuthoredChestArt;
                    var eventObj=new GameObject("Disposable event system",typeof(EventSystem));
                    var eventSystem=eventObj.GetComponent<EventSystem>();
                    VerifyUiOwnership(1,eventSystem);
                    // Regression for Unity 6's slash-as-path rule. The
                    // Heroes screen's REAL literal name has a slash and
                    // starts inactive; a reward must wait while it is
                    // visibly open, then resume when dismissed. No payout
                    // or extra Canvas is created by this isolated probe.
                    var heroProbe=new GameObject(
                        "Heroes / canonical destination",typeof(RectTransform));
                    heroProbe.layer=5;
                    heroProbe.transform.SetParent(rect,false);
                    heroProbe.SetActive(false);
                    Need(!panel.ProbeHeroRoomVisibilityForNativeEvidence(),
                        "Inactive Heroes room incorrectly blocks daily reward");
                    heroProbe.SetActive(true);
                    Need(panel.ProbeHeroRoomVisibilityForNativeEvidence(),
                        "Visible Lyra room not detected: slash-name lookup regression");
                    // Blocking the API is not enough: players must NOT SEE
                    // the optional medallion overlapping an actual hero room.
                    var heroEntry=FindButton(panel,
                        "Daily reward / optional medallion");
                    Need(!heroEntry.gameObject.activeInHierarchy,
                        "Treasury medallion still overlays canonical Lyra room");
                    panel.Open();
                    Need(!panel.IsOpen,
                        "Treasury modal opened over canonical Lyra room");
                    Need(!panel.PresentCommittedMilestone(
                            "This must not cover real Lyra",
                            "Visual-only rejection probe",true,"milestone:hero:lyra"),
                        "Direct ceremony API improperly covered canonical hero room");
                    heroProbe.SetActive(false);
                    Need(!panel.ProbeHeroRoomVisibilityForNativeEvidence() &&
                         heroEntry.gameObject.activeInHierarchy,
                        "Closing Lyra room did not restore the optional treasury medallion");
                    Object.DestroyImmediate(heroProbe);
                    Canvas.ForceUpdateCanvases();
                    pictures.Add(Render(camera,rt,output,width,height,
                        "hud-entry",
                        "Genuine unclaimed original chest medallion on live native canvas",
                        true,false));
                    // The entry medallion itself must accept a true pointer
                    // sequence; invoking Open() would conceal hitbox failures.
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,"Daily reward / optional medallion"),
                        width,height);
                    inputChecks++;
                    Need(panel.IsOpen,"Real entry medallion did not open calendar");
                    Canvas.ForceUpdateCanvases();
                    var button=FindButton(panel,"Claim exact daily reward");
                    Need(button.interactable,"Correct day seven must be claimable");
                    // Capture BEFORE interacting, then tap through Unity's actual
                    // raycaster and selectable pointer sequence.
                    VerifyCriticalTypography(panel,"calendar day seven");
                    VerifyCalendarMultipleResourceMargins(panel,"calendar day seven");
                    VerifySeventhDayPreviewTruth(panel,"calendar day seven");
                    pictures.Add(Render(camera,rt,output,width,height,"calendar",
                        "Available calendar before claim",true,false));
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        button,width,height);
                    inputChecks++;
                    var after=gateway.Snapshot();
                    Need(after.DailyTreasures.CycleDay==7 &&
                        after.SpeedupFiveMinutes==before.SpeedupFiveMinutes+1,
                        "Native button failed to pay actual day-seven reward");
                    Need(after.Mailbox.Messages.Count==before.Mailbox.Messages.Count+1,
                        "Day-seven native button did not write the mailbox receipt");
                    Need(!DailyTreasureRules.CanClaim(after.DailyTreasures,clock.UtcTicks),
                        "Duplicate seven-day claim still available");
                    var beforeReplay=after.Resources.Wood;
                    var repeat=new GameCommand("qa-repeat-native-"+width+"x"+height,
                        after.PlayerId,"ClaimDailyTreasure","valoria",after.Revision);
                    Need(!gateway.Execute(repeat).Ok &&
                         gateway.Snapshot().Resources.Wood==beforeReplay,
                        "A second native reward command paid twice");
                    report.realGatewayClaims++;
                    Canvas.ForceUpdateCanvases();
                    VerifyPremiumDailyComposition(panel,width,height,true);
                    pictures.Add(Render(camera,rt,output,width,height,"closed",
                        "Same real claim: closed chest",true,true));
                    // Capture the exact authored runtime frame at its peak:
                    // this distinguishes a convincing transition from two
                    // attractive static endpoints and detects ghosting.
                    panel.SampleForNativeEvidence(
                        DailyTreasureRevealTimeline.RevealPeakSeconds);
                    Canvas.ForceUpdateCanvases();
                    pictures.Add(Render(camera,rt,output,width,height,"opening",
                        "Same real paid claim: illustrated opening at peak light",true,true));
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,"Skip animation / keep earned items"),width,height);
                    inputChecks++;
                    Canvas.ForceUpdateCanvases();
                    VerifyCleanPaintedResources(panel,4);
                    VerifyEarnedItemReadability(panel,4);
                    VerifyPremiumDailyComposition(panel,width,height,false);
                    VerifyCriticalTypography(panel,"paid day seven / illustration cards");
                    Need(panel.EarnedItemCardCount==4 &&
                         panel.HasRevealedIllustratedEarnedItems,
                        "Day seven must show four ORIGINAL painted resource cards after real claim");
                    pictures.Add(Render(camera,rt,output,width,height,"opened",
                        "Same real claim: chest opened and exact items",true,true));
                    if((width==390&&height==844)||(width==844&&height==390))
                    {
                        // Show the actual between-keyframes chest-to-inventory
                        // item flight, never only the two static endpoints.
                        panel.SamplePaidItemFlightForNativeEvidence(.23f);
                        Canvas.ForceUpdateCanvases();
                        pictures.Add(Render(camera,rt,output,width,height,
                            "day7-prize-flight",
                            "Real four-item day-seven chest-to-inventory arc",true,true));
                        panel.SamplePaidItemFlightForNativeEvidence(2f);
                    }
                    // A person can rotate the phone after being paid, before
                    // closing the full premium reveal. The exact same modal
                    // must change from a one-line landscape confirmation to
                    // a two-line portrait confirmation without text clipping,
                    // losing the cards or awarding a second set of resources.
                    var originalCanvasSize=rect.sizeDelta;
                    rect.sizeDelta=new Vector2(height,width);
                    Canvas.ForceUpdateCanvases();
                    panel.ProbeOrientationLayoutForNativeEvidence();
                    Canvas.ForceUpdateCanvases();
                    var rotatedLabel=FindRect(panel,
                        "Confirmed received items").GetComponent<Text>();
                    bool becamePortrait=width>height;
                    Need(rotatedLabel &&
                         (becamePortrait?
                            rotatedLabel.text.Contains("\nGuardado en tu inventario"):
                            rotatedLabel.text.Contains(" · GUARDADO")),
                         "Paid reward caption did not adapt to phone rotation");
                    VerifyCriticalTypography(panel,"paid chest after orientation change");
                    Need(panel.EarnedItemCardCount==4 &&
                         panel.HasRevealedIllustratedEarnedItems,
                         "Paid cards disappeared when device orientation changed");
                    rect.sizeDelta=originalCanvasSize;
                    Canvas.ForceUpdateCanvases();
                    panel.ProbeOrientationLayoutForNativeEvidence();
                    Canvas.ForceUpdateCanvases();
                    VerifyCriticalTypography(panel,"paid chest after rotation restored");
                    Need(gateway.Snapshot().Resources.Wood==after.Resources.Wood &&
                         gateway.Snapshot().Resources.Stone==after.Resources.Stone,
                         "Skipping, rotating or restoring ceremony must never pay rewards again");
                    // A visible CONTINUAR action is more important to player
                    // flow than a hidden assertion that its Button exists.
                    // Exercise both legitimate dismiss paths at real pointer
                    // coordinates across the four target orientations.
                    var daySevenDismiss=width>height?
                        "Claim exact daily reward":"Close tributes";
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,daySevenDismiss),width,height);
                    inputChecks++;
                    Need(!panel.IsOpen,
                        "Day-seven Continue or close cannot exit the paid reward UI");

                    // Distinct day-four ceremonial proof on the SAME exact
                    // Unity canvas and render target, with its OWN disposable
                    // real player state, not changing a day-seven save to
                    // fabricate an earlier reward after the fact.
                    clock.UtcTicks=Day(1);
                    var specialStore=new TransientStore();
                    var specialGateway=new LocalGateway(clock,specialStore);
                    for(int day=1;day<=3;day++)
                    {
                        clock.UtcTicks=Day(day);
                        var earned=specialGateway.Snapshot();
                        var seed=new GameCommand("qa-special-seed-"+day,
                            earned.PlayerId,"ClaimDailyTreasure","valoria",
                            earned.Revision);
                        Need(specialGateway.Execute(seed).Ok,
                            "Day-four candidate seeding failed "+day);
                    }
                    clock.UtcTicks=Day(4);
                    var specialBefore=specialGateway.Snapshot();
                    panel.Bind(specialGateway,()=>clock.UtcTicks,()=>{});
                    // This is a new disposable player's visit immediately
                    // after dismissing day seven. In batchmode no Unity player
                    // frame runs between those independent sessions. The new
                    // optional medallion is registered, but Graphic.depth=-1
                    // until its own canvas is actually drawn (native run
                    // 42798661482, Capture line 856). Advance the REAL render
                    // before the next REAL GraphicRaycaster pointer; never
                    // call Button.onClick, fabricate a receipt, or weaken the
                    // hit test. This matches the existing quiet-day rehearsal.
                    Canvas.ForceUpdateCanvases();
                    camera.Render();
                    camera.Render();
                    Canvas.ForceUpdateCanvases();
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,"Daily reward / optional medallion"),
                        width,height);
                    inputChecks++;
                    Need(panel.IsOpen,"Special-day entry not interactive");
                    Canvas.ForceUpdateCanvases();
                    VerifyCriticalTypography(panel,"calendar day four");
                    VerifyCalendarMultipleResourceMargins(panel,"calendar day four");
                    pictures.Add(Render(camera,rt,output,width,height,
                        "special-day4-calendar",
                        "Real day-four candidate before claim",true,false));
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,"Claim exact daily reward"),
                        width,height);
                    inputChecks++;
                    var specialAfter=specialGateway.Snapshot();
                    Need(specialAfter.DailyTreasures.CycleDay==4 &&
                         specialAfter.Resources.Wood==specialBefore.Resources.Wood+90 &&
                         specialAfter.Resources.Stone==specialBefore.Resources.Stone+60 &&
                         specialAfter.Mailbox.Messages.Count==specialBefore.Mailbox.Messages.Count+1,
                        "Day-four chest must represent its exact earned resource contents");
                    report.realGatewayClaims++;
                    VerifyPremiumDailyComposition(panel,width,height,true);
                    panel.SampleForNativeEvidence(
                        DailyTreasureRevealTimeline.RevealPeakSeconds);
                    Canvas.ForceUpdateCanvases();
                    pictures.Add(Render(camera,rt,output,width,height,
                        "special-day4-opening",
                        "Real special chest opening at verified peak light",true,true));
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,"Skip animation / keep earned items"),
                        width,height);
                    inputChecks++;
                    Canvas.ForceUpdateCanvases();
                    VerifyCleanPaintedResources(panel,2);
                    VerifyEarnedItemReadability(panel,2);
                    VerifyPremiumDailyComposition(panel,width,height,false);
                    VerifyCriticalTypography(panel,"paid day four / illustration cards");
                    Need(panel.EarnedItemCardCount==2 &&
                         panel.HasRevealedIllustratedEarnedItems,
                        "Day four must show two painted, real paid wood and stone cards");
                    pictures.Add(Render(camera,rt,output,width,height,
                        "special-day4-received",
                        "Actual day-four payment shown as two original illustrated resource cards",
                        true,true));
                    if((width==390&&height==844)||(width==844&&height==390))
                    {
                        panel.SamplePaidItemFlightForNativeEvidence(.19f);
                        Canvas.ForceUpdateCanvases();
                        pictures.Add(Render(camera,rt,output,width,height,
                            "special-day4-prize-flight",
                            "Real two-item day-four chest-to-inventory arc",true,true));
                        panel.SamplePaidItemFlightForNativeEvidence(2f);
                    }
                    Need(specialGateway.Snapshot().Resources.Wood==
                         specialAfter.Resources.Wood &&
                         specialGateway.Snapshot().Resources.Stone==
                         specialAfter.Resources.Stone,
                        "Skipping the special chest may not award extra resources");
                    var dayFourDismiss=width<height?
                        "Claim exact daily reward":"Close tributes";
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,dayFourDismiss),width,height);
                    inputChecks++;
                    Need(!panel.IsOpen,"Day-four Continue or close must work");

                    // Rehearse PR173's exact independent canvas identity and
                    // safe-area naming without editing or relying on that
                    // other workstream's assembly. The old Royal canvas is
                    // explicitly switched off, just as Vigilia does in game.
                    var nativeObj=new GameObject("VIGILIA / independent canvas",
                        typeof(RectTransform),typeof(Canvas),
                        typeof(CanvasScaler),typeof(GraphicRaycaster));
                    nativeObj.layer=5;
                    var nativeRoot=(RectTransform)nativeObj.transform;
                    nativeRoot.sizeDelta=new Vector2(width,height);
                    // PR173 runs its actual independent UI in ScreenSpaceCamera,
                    // NOT WorldSpace. A second WorldSpace fixture can pass (or
                    // fail) raycasts for the wrong reason. Mirror the real
                    // render mode, camera, sorting and pixel-scale contract.
                    nativeRoot.localScale=Vector3.one;
                    var nativeCanvas=nativeObj.GetComponent<Canvas>();
                    nativeCanvas.renderMode=RenderMode.ScreenSpaceCamera;
                    nativeCanvas.planeDistance=1f;
                    nativeCanvas.overrideSorting=true;
                    nativeCanvas.sortingOrder=5000;
                    nativeCanvas.worldCamera=camera;
                    var nativeScaler=nativeObj.GetComponent<CanvasScaler>();
                    nativeScaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
                    nativeScaler.scaleFactor=1f;
                    // On a ROOT Canvas overrideSorting is a NESTED-CANVAS
                    // property (Unity API contract). In batchmode Unity can
                    // report false for it even when it was assigned true;
                    // testing that irrelevant flag caused a false FAIL before
                    // any migrated-pointer evidence was exercised. PR173 uses
                    // root ScreenSpaceCamera, camera, order and pixel scale.
                    // Require those real functional properties, never bypass
                    // the GraphicRaycaster or synthesize UI clicks.
                    Canvas.ForceUpdateCanvases();
                    Need(nativeCanvas.renderMode==RenderMode.ScreenSpaceCamera &&
                         nativeCanvas.worldCamera==camera &&
                         nativeCanvas.sortingOrder==5000 &&
                         Mathf.Abs(nativeScaler.scaleFactor-1f)<.001f &&
                         nativeScaler.uiScaleMode==
                             CanvasScaler.ScaleMode.ConstantPixelSize,
                         "Native Vigilia Canvas fields differ: mode="+
                         nativeCanvas.renderMode+
                         " camera="+(nativeCanvas.worldCamera?
                             nativeCanvas.worldCamera.name:"null")+
                         " expected="+camera.name+
                         " sorting="+nativeCanvas.sortingOrder+
                         " overrideRoot="+nativeCanvas.overrideSorting+
                         " isRoot="+nativeCanvas.isRootCanvas+
                         " scale="+nativeScaler.scaleFactor+
                         " scaler="+nativeScaler.uiScaleMode);
                    var safeObj=new GameObject("Safe composition",
                        typeof(RectTransform));
                    safeObj.layer=5;
                    safeObj.transform.SetParent(nativeObj.transform,false);
                    var safeRect=(RectTransform)safeObj.transform;
                    safeRect.anchorMin=Vector2.zero;
                    safeRect.anchorMax=Vector2.one;
                    safeRect.offsetMin=Vector2.zero;
                    safeRect.offsetMax=Vector2.zero;
                    canvas.enabled=false;
                    panel.ProbePresentationCanvasForNativeEvidence();
                    Need(panel.UsesVigiliaCanvas &&
                         panel.transform.parent==safeRect,
                         "Daily treasure canvas remained invisible on Vigilia");
                    VerifyUiOwnership(2,eventSystem);
                    // A REAL Vigilia building selection must outrank Tesoros.
                    // Editor-only exact-name probe: no fake building upgrades,
                    // resources, reward receipts or new event systems.
                    var buildingProbe=new GameObject(
                        "Independent selected-building cartouche",
                        typeof(RectTransform));
                    buildingProbe.layer=5;
                    buildingProbe.transform.SetParent(safeRect,false);
                    buildingProbe.SetActive(false);
                    Need(!panel.ProbeVigiliaBuildingForNativeEvidence(),
                        "Hidden Vigilia building wrongly suppresses a reward");
                    buildingProbe.SetActive(true);
                    Need(panel.ProbeVigiliaBuildingForNativeEvidence(),
                        "Live building selection cannot suppress treasure access");
                    panel.Open();
                    Need(!panel.IsOpen,
                        "Treasure window covers real selected building controls");
                    buildingProbe.SetActive(false);
                    Need(!panel.ProbeVigiliaBuildingForNativeEvidence(),
                        "Dismissing building selection kept treasury suppressed");
                    Canvas.ForceUpdateCanvases();
                    var movedEntry=FindButton(panel,
                        "Daily reward / optional medallion");
                    Need(movedEntry.gameObject.activeInHierarchy,
                        "Treasury entry not restored after building selection");
                    var movedGraphic=movedEntry.GetComponent<Graphic>();
                    var migratedGraphics=
                        GraphicRegistry.GetGraphicsForCanvas(nativeCanvas);
                    Need(movedGraphic && movedGraphic.raycastTarget &&
                         migratedGraphics!=null &&
                         migratedGraphics.Contains(movedGraphic),
                         "Actual medallion is not registered as a raycastable "+
                         "Graphic under the new Vigilia canvas");
                    // The editor fixture constructs a second WorldSpace canvas
                    // and moves an already rendered panel to it WITHOUT the
                    // normal intervening player frame. The native raycaster
                    // explicitly ignores graphics with depth == -1.
                    // Render the newly parented UI on the SAME real Unity
                    // camera/target before the pointer event, never invoke a
                    // Button.onClick or grant a reward through the fixture.
                    camera.Render();
                    camera.Render();
                    Canvas.ForceUpdateCanvases();
                    Need(movedGraphic.depth>=0 && !movedGraphic.canvasRenderer.cull,
                         "Migrated medallion is not yet drawn in native Unity; "+
                         "its raycast would be physically impossible");
                    ClickAtNativeScreenPosition(nativeCanvas,eventSystem,camera,
                        movedEntry,width,height);
                    inputChecks++;
                    Need(panel.IsOpen,
                        "Independent Vigilia reward medallion cannot open");
                    // Opening the modal activates a previously disabled tree
                    // of uGUI Graphics. In this EDITOR batchmode fixture no
                    // player frame occurs between pointer events, so their
                    // depth stays -1 and the REAL GraphicRaycaster yields
                    // zero hits. Draw a genuine frame on the newly migrated
                    // Vigilia Canvas before testing the newly revealed close
                    // button, exactly as normal frame-based player input does.
                    // Never bypass the raycaster or call Button.onClick.
                    Canvas.ForceUpdateCanvases();
                    camera.Render();
                    camera.Render();
                    Canvas.ForceUpdateCanvases();
                    var modalClose=FindButton(panel,"Close tributes");
                    var closeGraphic=modalClose.GetComponent<Graphic>();
                    Need(closeGraphic && closeGraphic.depth>=0 &&
                         !closeGraphic.canvasRenderer.cull,
                         "Vigilia modal close is not actually rendered after "+
                         "the newly opened UI's first native frame");
                    ClickAtNativeScreenPosition(nativeCanvas,eventSystem,camera,
                        modalClose,width,height);
                    inputChecks++;
                    Need(!panel.IsOpen,
                        "Independent Vigilia reward modal cannot close");
                    // A building opened AFTER the treasury must close the
                    // optional modal/Foley without touching a paid wallet.
                    var priorBuilding=specialGateway.Snapshot();
                    panel.Open();
                    Need(panel.IsOpen,"Cannot reopen the already paid calendar");
                    buildingProbe.SetActive(true);
                    Need(panel.ProbeVigiliaBuildingForNativeEvidence() &&
                         !panel.IsOpen &&
                         !movedEntry.gameObject.activeInHierarchy,
                         "Selected building did not preempt a reward modal");
                    buildingProbe.SetActive(false);
                    Need(!panel.ProbeVigiliaBuildingForNativeEvidence() &&
                         movedEntry.gameObject.activeInHierarchy,
                         "Reward entry stayed hidden after building closed");
                    var afterBuilding=specialGateway.Snapshot();
                    Need(priorBuilding.Resources.Wood==afterBuilding.Resources.Wood &&
                         priorBuilding.Resources.Stone==afterBuilding.Resources.Stone &&
                         priorBuilding.Mailbox.Messages.Count==
                         afterBuilding.Mailbox.Messages.Count,
                         "Selected-building priority mutated real earned items");
                    Object.DestroyImmediate(buildingProbe);
                    nativeCanvas.enabled=false;
                    panel.ProbePresentationCanvasForNativeEvidence();
                    Need(!panel.UsesVigiliaCanvas &&
                         panel.transform.parent==rect,
                         "Daily treasure UI cannot restore to original canvas");
                    canvas.enabled=true;
                    Object.DestroyImmediate(nativeObj);
                    Canvas.ForceUpdateCanvases();
                    // The returned legacy fixture also needs one real render
                    // before testing additional pointer interaction.
                    camera.Render();
                    Canvas.ForceUpdateCanvases();
                    VerifyUiOwnership(1,eventSystem);

                    // Four DIFFERENT real-world milestone categories share
                    // one premium authored chest and ornate frame. Test each
                    // category's ambient palette in portrait and landscape.
                    // These are VISUAL-ONLY style probes (no battle, no hero
                    // unlock, no resources, no second reward, no fake receipt).
                    if((width==390&&height==844)||(width==844&&height==390))
                    {
                        var styleIds=new[]{
                            "milestone:hero:lyra","milestone:bastion:5",
                            "vigilant:chapter:25","scout:first:exploration"};
                        var styleLabels=new[]{
                            "Lyra se une a Valoria",
                            "El Bastión alcanza el nivel 5",
                            "Capítulo de los Vigilantes completado",
                            "Descubrimiento de los exploradores"};
                        var styleDetails=new[]{
                            "La exploradora puede participar en nuevas marchas.",
                            "Valoria extiende sus fronteras.",
                            "El combate y su resultado constan en el buzón.",
                            "Nuevo informe de exploración registrado."};
                        var styleSuffixes=new[]{"lyra","bastion","vigilants","scouts"};
                        // The hero-hall, exploratory map and Vigilants
                        // landmark use real painterly source textures from
                        // Eldoria, not just a different color behind a chest.
                        var requiredScenery=new[]{
                            "EldoriaV41/heroes-hall-illustrated-v2",
                            (string)null,
                            "EldoriaRetention/vigilants-painterly-v2",
                            "EldoriaRetention/scouts-painterly-v2"
                        };
                        for(int style=0;style<styleIds.Length;style++)
                        {
                            var wallet=specialGateway.Snapshot();
                            Need(panel.PresentCommittedMilestone(
                                    styleLabels[style],styleDetails[style],
                                    legendary:style!=3,eventId:styleIds[style]),
                                "Premium milestone style presentation unavailable: "+
                                styleIds[style]);
                            Need((requiredScenery[style]==null &&
                                  !panel.HasMilestoneScenery) ||
                                 (requiredScenery[style]!=null &&
                                  panel.HasMilestoneScenery &&
                                  panel.MilestoneSceneryKey==requiredScenery[style]),
                                "Wrong or absent original painterly milestone scenery: "+
                                styleIds[style]);
                            Canvas.ForceUpdateCanvases();
                            VerifyGenuineSceneComposition(
                                panel,styleIds[style],width,height);
                            panel.SampleForNativeEvidence(
                                DailyTreasureRevealTimeline.RevealPeakSeconds);
                            Canvas.ForceUpdateCanvases();
                            // Prove that the actual hero contributes visible
                            // GPU pixels; the previous native suite silently
                            // accepted an EMPTY Lyra room as a passing scene.
                            if(style==0)
                                VerifyActualLyraVisibleInNativePixels(
                                    panel,camera,rt,width,height);
                            pictures.Add(Render(camera,rt,output,width,height,
                                "style-"+styleSuffixes[style]+"-opening",
                                "Visual style preview ONLY, not an earned milestone: "+
                                    styleIds[style],true,false));
                            ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                                FindButton(panel,"Skip animation / keep earned items"),
                                width,height);
                            inputChecks++;
                            Canvas.ForceUpdateCanvases();
                            VerifyCriticalTypography(panel,
                                "authentic milestone "+styleSuffixes[style]);
                            pictures.Add(Render(camera,rt,output,width,height,
                                "style-"+styleSuffixes[style]+"-result",
                                "Visual result layout ONLY, with no milestone command executed: "+
                                    styleIds[style],true,false));
                            ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                                FindButton(panel,"Close tributes"),width,height);
                            inputChecks++;
                            Need(!panel.IsOpen,
                                "Premium milestone style refuses to close: "+styleIds[style]);
                            Need(!panel.HasMilestoneScenery &&
                                 panel.MilestoneSceneryKey==null,
                                "Optional painterly texture still pinned after ceremony close: "+
                                styleIds[style]);
                            var afterStyle=specialGateway.Snapshot();
                            Need(wallet.Resources.Wood==afterStyle.Resources.Wood &&
                                 wallet.Resources.Stone==afterStyle.Resources.Stone &&
                                 wallet.Resources.Food==afterStyle.Resources.Food &&
                                 wallet.Mailbox.Messages.Count==afterStyle.Mailbox.Messages.Count,
                                 "Visual-only style probe granted resources or mail: "+
                                 styleIds[style]);
                            report.visualOnlyMilestoneCaptures+=2;
                        }
                    }

                    // Narrative priority can arrive after a daily modal
                    // was ALREADY opened, not just before the initial tap.
                    // This must preempt the overlay and stop its Foley, while
                    // the real Day 4 claim and mailbox receipt remain saved.
                    var beforeInterrupt=specialGateway.Snapshot();
                    panel.Open();
                    Need(panel.IsOpen,"Previously paid calendar cannot be reopened");
                    panel.Suppressed=true;
                    Need(!panel.IsOpen &&
                         !FindButton(panel,"Daily reward / optional medallion")
                            .gameObject.activeInHierarchy,
                        "Narrative failed to preempt an open daily calendar");
                    panel.Suppressed=false;
                    Need(FindButton(panel,"Daily reward / optional medallion")
                            .gameObject.activeInHierarchy,
                        "Reward access did not recover after story dismissal");

                    // Apply the same interruption while a premium animation
                    // is genuinely playing. This is an event-style UI probe
                    // ONLY, with no new hero or combat reward command.
                    Need(panel.PresentCommittedMilestone(
                            "Hito previo a un tutorial",
                            "La información permanece en el buzón",
                            legendary:true,eventId:"vigilant:chapter:25"),
                        "Could not open an optional ceremonial overlay");
                    Need(panel.IsOpen,
                        "Premium ceremony was not visible before interruption");
                    panel.Suppressed=true;
                    Need(!panel.IsOpen,
                        "Tutorial overlay did not interrupt the premium coffer");
                    Need(!panel.PresentCommittedMilestone(
                            "This must not cover a tutorial",
                            "Visual-only rejection probe",true,"vigilant:chapter:25"),
                        "Direct ceremony API bypassed narrative/tutorial priority");
                    panel.Suppressed=false;
                    var afterInterrupt=specialGateway.Snapshot();
                    Need(beforeInterrupt.Resources.Wood==afterInterrupt.Resources.Wood &&
                         beforeInterrupt.Resources.Stone==afterInterrupt.Resources.Stone &&
                         beforeInterrupt.Resources.Food==afterInterrupt.Resources.Food &&
                         beforeInterrupt.Mailbox.Messages.Count==
                            afterInterrupt.Mailbox.Messages.Count,
                        "Interrupting a premium ceremony corrupted an already-paid reward");


                    // The original 52-shot contract covered only day 4/7
                    // chests, leaving five ordinary days completely invisible
                    // to native visual QA. Exercise EVERY remaining day with
                    // independent disposable LocalGateway state. Never inject
                    // resources into PlayerState, and NEVER count a quiet
                    // presentation as a day-4/day-7 chest claim.
                    foreach(int quietDay in new[]{1,2,3,5,6})
                    {
                        // A separate disposable player visit must begin with
                        // a FRESH native Canvas, not the render hierarchy just
                        // reparented twice during the Vigilia migration probe.
                        // The prior genuine Unity failure had depth=-1 only
                        // on this reused canvas, despite a registered graphic,
                        // an on-screen hitbox and a valid EventSystem. Rebinding
                        // unrelated saves five times without a player loop
                        // did not model distinct daily logins. Recreate the
                        // ordinary-day UI for each independent gateway visit,
                        // retaining real camera renders and GraphicRaycaster
                        // pointer tests; never fake a press or a prize.
                        Object.DestroyImmediate(canvasObj);
                        canvasObj=new GameObject("Disposable native uGUI component",
                            typeof(RectTransform),typeof(Canvas),
                            typeof(CanvasScaler),typeof(GraphicRaycaster));
                        canvasObj.layer=5;
                        rect=(RectTransform)canvasObj.transform;
                        rect.sizeDelta=new Vector2(width,height);
                        rect.localScale=Vector3.one*.01f;
                        canvas=canvasObj.GetComponent<Canvas>();
                        canvas.renderMode=RenderMode.WorldSpace;
                        canvas.worldCamera=camera;
                        panel=DailyTreasurePanel.Install(rect);
                        Need(panel.HasAuthoredChestArt,
                            "An independent daily session lost original chest art");
                        VerifyUiOwnership(1,eventSystem);

                        var quietStore=new TransientStore();
                        clock.UtcTicks=Day(1);
                        var quietGateway=new LocalGateway(clock,quietStore);
                        for(int seed=1;seed<quietDay;seed++)
                        {
                            clock.UtcTicks=Day(seed);
                            var original=quietGateway.Snapshot();
                            var command=new GameCommand(
                                "qa-quiet-seed-"+width+"x"+height+"-"+quietDay+"-"+seed,
                                original.PlayerId,"ClaimDailyTreasure",
                                "valoria",original.Revision);
                            Need(quietGateway.Execute(command).Ok,
                                "Real quiet-day seed failed for "+quietDay);
                        }
                        clock.UtcTicks=Day(quietDay);
                        var beforeQuiet=quietGateway.Snapshot();
                        var expectedQuiet=DailyTreasureRules.ForDay(quietDay,quietDay);
                        panel.Bind(quietGateway,()=>clock.UtcTicks,()=>{});
                        // Binding a fresh disposable player save updates the
                        // entry graphics. In editor batchmode there is no
                        // automatic player frame between consecutive saves;
                        // render the actual Canvas before a raycast. This is
                        // the same rendered-frame requirement as real input,
                        // not a synthetic Button.onClick or prize grant.
                        Canvas.ForceUpdateCanvases();
                        camera.Render();
                        camera.Render();
                        Canvas.ForceUpdateCanvases();
                        ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                            FindButton(panel,"Daily reward / optional medallion"),
                            width,height);
                        quietInputChecks++;
                        Need(panel.IsOpen,
                            "Ordinary day "+quietDay+" entry cannot be opened");
                        if(quietDay==3)
                        {
                            // On the third real visit the streak bonus adds
                            // food to a normally single-item speedup. This
                            // catches incorrect hard-coded day-4/6 layouts.
                            var card=FindRect(panel,
                                "Day 3 / non-clickable informational card");
                            var text=card.GetComponentsInChildren<Text>(true)
                                .Single(x=>x.name=="Actual reward contents");
                            Need(text.gameObject.activeInHierarchy &&
                                 text.text.Contains("Comida 25") &&
                                 text.text.Contains("1 min x1") &&
                                 text.text.Split('\n').Length==2,
                                "Real third-day loyalty food is absent from calendar");
                            float clear=text.rectTransform.anchoredPosition.y+
                                text.rectTransform.rect.yMin-card.rect.yMin;
                            Need(clear>=4.5f &&
                                 text.preferredHeight<=
                                    text.rectTransform.rect.height+6f,
                                "Third-day loyalty bonus text overlaps the "+
                                "forged bronze card border: "+clear);
                        }
                        // Opening the calendar activates the claim button,
                        // initially depth=-1 until Unity draws the new uGUI
                        // graphics. The previous exact-head native run failed
                        // at this real pointer (run 42712277475, line 1272),
                        // AFTER its entry button had correctly worked. Give
                        // the freshly activated button an actual rendered
                        // frame, then still require GraphicRaycaster hits.
                        Canvas.ForceUpdateCanvases();
                        camera.Render();
                        camera.Render();
                        Canvas.ForceUpdateCanvases();
                        ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                            FindButton(panel,"Claim exact daily reward"),
                            width,height);
                        quietInputChecks++;
                        var credited=quietGateway.Snapshot();
                        Need(credited.DailyTreasures.CycleDay==quietDay &&
                             credited.DailyTreasures.TotalClaims==quietDay &&
                             credited.Resources.Wood==beforeQuiet.Resources.Wood+expectedQuiet.Wood &&
                             credited.Resources.Stone==beforeQuiet.Resources.Stone+expectedQuiet.Stone &&
                             credited.Resources.Food==beforeQuiet.Resources.Food+expectedQuiet.Food &&
                             credited.SpeedupOneMinute==beforeQuiet.SpeedupOneMinute+
                                expectedQuiet.SpeedupOneMinute &&
                             credited.SpeedupFiveMinutes==beforeQuiet.SpeedupFiveMinutes+
                                expectedQuiet.SpeedupFiveMinutes &&
                             credited.Mailbox.Messages.Count==beforeQuiet.Mailbox.Messages.Count+1,
                             "Non-chest day "+quietDay+" actual receipt differs from gateway");
                        Need(!DailyTreasureRules.CanClaim(
                                credited.DailyTreasures,clock.UtcTicks),
                            "Non-chest day "+quietDay+" permits a second grant");
                        var descriptors=DailyTreasureRewardPresentation.FromCommittedReward(
                            expectedQuiet);
                        Canvas.ForceUpdateCanvases();
                        Need(panel.EarnedItemCardCount==descriptors.Length &&
                             panel.HasRevealedIllustratedEarnedItems &&
                             !FindButton(panel,
                                "Skip animation / keep earned items").gameObject.activeInHierarchy,
                            "Ordinary day "+quietDay+
                            " failed to show actual immediate illustrated item cards");
                        VerifyEarnedItemReadability(panel,descriptors.Length);
                        VerifyContentSizedQuietReward(panel,
                            descriptors.Length,width,height);
                        VerifyCriticalTypography(panel,
                            "quiet reward day "+quietDay);
                        quietPictures.Add(Render(camera,rt,quietOutput,width,height,
                            "quiet-day"+quietDay+"-received",
                            "Real paid day "+quietDay+" compact reward; no chest ceremony",
                            true,true));
                        // The day-three loyalty bonus contains two resource
                        // types; dismiss it through CONTINUAR, not X, so the
                        // ordinary reward's primary CTA is exercised too.
                        var quietDismiss=quietDay==3?
                            "Claim exact daily reward":"Close tributes";
                        ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                            FindButton(panel,quietDismiss),width,height);
                        quietInputChecks++;
                        Need(!panel.IsOpen,
                            "Non-chest Continue or X fails on day "+quietDay);
                        var previous=quietGateway.Snapshot();
                        var duplicate=new GameCommand(
                            "qa-quiet-repeat-"+width+"x"+height+"-"+quietDay,
                            previous.PlayerId,"ClaimDailyTreasure",
                            "valoria",previous.Revision);
                        Need(!quietGateway.Execute(duplicate).Ok,
                            "Non-chest day "+quietDay+" paid twice");
                        var still=quietGateway.Snapshot();
                        Need(still.Resources.Wood==previous.Resources.Wood &&
                             still.Resources.Stone==previous.Resources.Stone &&
                             still.Resources.Food==previous.Resources.Food &&
                             still.SpeedupOneMinute==previous.SpeedupOneMinute &&
                             still.SpeedupFiveMinutes==previous.SpeedupFiveMinutes &&
                             still.Mailbox.Messages.Count==previous.Mailbox.Messages.Count,
                            "Reopening quiet day could mutate wallet or mail");
                        report.quietGatewayClaims++;
                    }
                    Object.DestroyImmediate(eventObj);
                    Object.DestroyImmediate(canvasObj);
                    camera.targetTexture=null;
                    rt.Release();
                    Object.DestroyImmediate(rt);
                    Object.DestroyImmediate(go);
                }
                Need(pictures.Count==52 && inputChecks==56 &&
                     report.realGatewayClaims==8 &&
                     report.visualOnlyMilestoneCaptures==16 &&
                     quietPictures.Count==20 && quietInputChecks==60 &&
                     report.quietGatewayClaims==20,
                    "Real seven-day rewards, quiet receipts and four artistic stages incomplete");
                report.status=report.authoredSpritesImported
                    ? "NATIVE_COMPONENT_RENDERED_WITH_AUTHORED_SPRITES_ART_REVIEW_PENDING"
                    : "NATIVE_COMPONENT_RENDERED_FALLBACK_ONLY_ART_REJECTED";
                report.graphicalPointerHits=inputChecks;
                report.images=pictures.Count;
                report.shots=pictures.ToArray();
                report.quietImages=quietPictures.Count;
                report.quietPointerHits=quietInputChecks;
                report.quietShots=quietPictures.ToArray();
                File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));
                Debug.Log("DAILY_TREASURE_NATIVE_COMPONENT_CAPTURED: 52 contracted PNG plus 20 independently paid quiet-day PNG in quiet/; 20 additional actual claims and 60 UI pointer events; NOT ART_APPROVED / NOT FULL GAMEPASS");
                EditorApplication.Exit(0);
            }
            catch(Exception e)
            {
                // A failed interaction must never erase the REAL screenshots
                // and gateway actions already captured on the same Unity run.
                // Previous native failures wrote images=0/shots=[] despite
                // eight original PNGs being present in their artifacts,
                // obscuring precisely which visual stage needs correction.
                // Keep an explicit FAILED verdict; partial frames are review
                // evidence, NEVER the 52+20 completed certification.
                report.status="NATIVE_COMPONENT_FAILED";
                report.note=e.ToString();
                report.images=pictures.Count;
                report.shots=pictures.ToArray();
                report.graphicalPointerHits=inputChecks;
                report.quietImages=quietPictures.Count;
                report.quietShots=quietPictures.ToArray();
                report.quietPointerHits=quietInputChecks;
                File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }
        static Shot Render(Camera camera,RenderTexture rt,string output,
            int width,int height,string variant,string description,bool native,bool saved)
        {
            var previous=RenderTexture.active;
            var png=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                Canvas.ForceUpdateCanvases();
                camera.Render(); camera.Render();
                RenderTexture.active=rt;
                png.ReadPixels(new Rect(0,0,width,height),0,0);
                png.Apply(false);
                string name="real-unity-daily-"+variant+"-"+width+"x"+height+".png";
                string path=Path.Combine(output,name);
                File.WriteAllBytes(path,png.EncodeToPNG());
                long bytes=new FileInfo(path).Length;
                Need(bytes>7000,"Suspiciously empty native capture: "+path);
                return new Shot{file=name,sha256=Sha(path),view=description,
                    width=width,height=height,bytes=(int)bytes,
                    nativeCanvas=native,coversReward=saved,
                    readyToClaim=variant=="calendar" ||
                        variant=="special-day4-calendar",preserved=saved};
            }
            finally
            {
                RenderTexture.active=previous;
                Object.DestroyImmediate(png);
            }
        }
    }
}
#endif
