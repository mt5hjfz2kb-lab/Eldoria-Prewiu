import fs from 'node:fs';
import assert from 'node:assert/strict';
const domain=fs.readFileSync('Unity/Assets/Eldoria/Scripts/Domain/GameState.cs','utf8');
const gateway=fs.readFileSync('Unity/Assets/Eldoria/Scripts/Application/LocalGateway.cs','utf8');
const rules=fs.readFileSync('Unity/Assets/Eldoria/Scripts/Domain/DailyTreasureRules.cs','utf8');
const retention=fs.readFileSync(
  'Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV3EarlyRetentionScreens.cs','utf8');
const panel=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePanel.cs','utf8');
const native=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Editor/DailyTreasureNativeEvidence.cs','utf8');
const nativeWorkflow=fs.readFileSync(
  '.github/workflows/daily-treasures-native.yml','utf8');
const nativePixelAudit=fs.readFileSync(
  'tools/daily-treasures/audit_native_art.py','utf8');
assert.match(domain,/DailyTreasureState DailyTreasures/);
assert.match(gateway,/DailyTreasures=s\.DailyTreasures\?\.Copy\(\)/);
assert.match(gateway,/case "ClaimDailyTreasure":/);
assert.match(gateway,/DailyTreasureRules\.TryClaim\(/);
assert.match(gateway,/EventId="daily-treasure:"\+state\.DailyTreasures\.LastClaimUtcDay/);
assert.match(gateway,/RewardReceiptId=command\.Id/);
assert.match(gateway,/state\.Mailbox\.Receive\(/);
assert.match(gateway,/state\.CompletedCommandIds\.Add\(command\.Id\);/);
const executableRules=rules.replace(/\/\/[^\n]*/g,''); // comments are documentation, not executable dependencies
assert.doesNotMatch(executableRules,/\bPlayerPrefs\b|\bUnityEngine\b|\bSystem\.Random\b/);
assert.match(retention,/dailyTreasures\.QueueCommittedMilestone\(/);
assert.doesNotMatch(retention,/dailyTreasures\.PresentCommittedMilestone\(/);
assert.match(retention,/"vigilant:"\+cmd\.Id/);
assert.match(retention,/"exploration:"\+cmd\.Id/);
assert.match(panel,/milestoneQueue\.AddConfirmedReport\(/);
assert.match(panel,/legendary:milestone\.Legendary/);
assert.match(retention,/if\(chapterCompleted\)/);
assert.match(panel,/public bool PresentCommittedMilestone\(/);
// The ceremony must not construct a separate payout command. The actual
// chapter battle and its mailbox receipt already live in LocalGateway.
const milestone=panel.split('public bool PresentCommittedMilestone(')[1]
  .split('// Safe to reuse for milestone ceremonies')[0];
assert.doesNotMatch(milestone,/gateway\.Execute|new GameCommand|\.Resources\./);
assert.match(panel,/hasAuthoredChestArt=closedSource && openSource/);
assert.match(panel,/showChest && !hasAuthoredChestArt/);
assert.match(panel,/DailyTreasureArt\/treasure_opened_512/);
assert.match(native,/NATIVE_COMPONENT_RENDERED_FALLBACK_ONLY_ART_REJECTED/);
assert.match(native,/authoredSpritesImported/);
// The nine PNG sources alone cannot certify that Unity imported all nine
// Sprites, applied 9-slice borders or avoided a duplicate active Canvas.
assert.match(native,/RequireNineImportedSprites\(\)/);
assert.match(native,/Resources\.Load<Sprite>\("DailyTreasureArt\/"\+item\.name\)/);
assert.match(native,/importer\.spriteImportMode==SpriteImportMode\.Single/);
assert.match(native,/var border=sprite\.border/);
assert.match(native,/VerifyUiOwnership\(1,eventSystem\)/);
assert.match(native,/VerifyUiOwnership\(2,eventSystem\)/);
assert.match(nativeWorkflow,/DAILY_TREASURE_AUTHORED_PNG_BYTES_AND_IMPORTERS_PASS/);
assert.match(nativeWorkflow,/treasure_closed_512\.png/);
assert.match(nativeWorkflow,/treasure_opened_512\.png/);
assert.match(nativeWorkflow,/Illustrated chest sprites not imported in Unity/);
// Art must never be declared approved by a source or syntax check.
assert.doesNotMatch(native,/artApproved=true/);
// The presentation must make a failed collect visible and not rely on
// old transform scale when a ceremonial reward is skipped/reopened.
assert.match(panel,/claimNotice=string\.IsNullOrWhiteSpace\(result\.Message\)/);
assert.match(panel,/noticeUntil=Time\.unscaledTime\+4\.5f/);
assert.match(panel,/illustratedClosed\.rectTransform\.localScale=Vector3\.one/);
assert.match(panel,/illustratedOpen\.rectTransform\.localScale=Vector3\.one/);
assert.match(panel,/ApplyAuthoredFrame\(cardSurface,"valoria_frame_main_512"\)/);
assert.match(panel,/ApplyAuthoredFrame\(day,"valoria_frame_card_256"\)/);
assert.match(panel,/ApplyAuthoredFrame\(\(RectTransform\)claimButton\.transform,/);
assert.match(panel,/DayIllustrations=\{/);
assert.match(panel,/LoadUnifiedResourceEmblem\(DayIllustrations\[i\]\)/);
assert.match(panel,/Resources\.Load<Sprite>\("VigiliaArt\/ResourceIcons\/"\+owner\)/);
assert.match(panel,/Resources\.Load<Sprite>\(\s*"DailyTreasureArt\/"\+fallback\)/);
assert.match(panel,/dayGlyph\[i\]\.gameObject\.SetActive\(false\)/);
assert.match(panel,/icon\.raycastTarget=false/);
const scoutPolicy=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureMilestoneRules.cs','utf8');
assert.match(retention,/DailyTreasureMilestoneRules\.FirstScoutFamily\(s,updated,target\)/);
assert.match(retention,/EarlyRetentionRules\.ScoutDiscoveryText\(order\)/);
assert.match(scoutPolicy,/if\(\(before\.Retention\.ScoutDiscoveryMask & mask\)!=0\)return false;/);
assert.match(scoutPolicy,/if\(\(after\.Retention\.ScoutDiscoveryMask & mask\)==0\)return false;/);
assert.match(gateway,/EventId="milestone:bastion:"\+targetLevel/);
assert.match(gateway,/EventId="milestone:hero:lyra"/);
// Story-only milestone emails are distinct from authoritative payout
// receipts. Neither new mail type may award a duplicate reward.
assert.match(gateway,/Type="hero-unlocked"/);
assert.match(gateway,/Type="milestone-bastion"/);
assert.match(panel,/root\.SetAsLastSibling\(\)/);
assert.match(panel,/DailyTreasureRules\.NextConsecutiveBonusDay\(/);
assert.match(panel,/lastPaintedUtcDay/);
assert.match(panel,/SampleForNativeEvidence\(float elapsed\)/);
assert.match(panel,/PaintCeremonyFrame\(elapsed\)/);
assert.match(native,/DailyTreasureRevealTimeline\.RevealPeakSeconds/);
assert.match(native,/"opening"/);
assert.match(native,/pictures\.Count==52/);
assert.match(nativeWorkflow,/\$report\.images -ne 52/);
// PR173's canonical independently rendered Canvas hides the old Royal HUD.
// Genuine daily UI must relocate to the native safe area or be invisible.
assert.match(panel,/FindLiteralVigiliaCanvas\(\)/);
assert.match(panel,/candidate\.gameObject\.name=="VIGILIA \/ independent canvas"/);
assert.match(panel,/HasVisibleCanonicalHeroRoom\(\)/);
assert.match(panel,/cachedCanonicalHeroRoom\.gameObject\.activeInHierarchy/);
assert.doesNotMatch(panel,/GameObject\.Find\("VIGILIA \/ independent canvas"\)/);
assert.doesNotMatch(panel,/GameObject\.Find\("Heroes \/ canonical destination"\)/);
assert.match(panel,/canvas\.transform\.Find\("Safe composition"\)/);
assert.match(panel,/root\.SetParent\(safe,false\)/);
assert.match(panel,/root\.SetParent\(originalSafeArea,false\)/);
assert.match(panel,/float entryX=onVigiliaCanvas&&!portrait\?256f:14f/);
assert.match(retention,/if\(!dailyTreasures && safe\)/);
assert.match(panel,/milestoneQueue\.Observe\(gateway\.Snapshot\(\)\)/);
assert.match(panel,/milestoneQueue\.ConfirmPresented\(milestone\.EventId\)/);
assert.doesNotMatch(panel,/new GameObject\([^\n]+typeof\(Canvas\)/);
// Both real chest ceremony tiers must be exercised with real LocalGateway
// payments, not a screenshot of unearned or hardcoded rewards.
assert.match(native,/special-day4-calendar/);
assert.match(native,/special-day4-opening/);
assert.match(native,/special-day4-received/);
assert.match(native,/specialAfter\.Resources\.Wood==specialBefore\.Resources\.Wood\+90/);
assert.match(native,/pictures\.Count==52 && inputChecks==56/);
assert.match(nativeWorkflow,/\$report\.images -ne 52/);
assert.match(nativeWorkflow,/\$report\.realGatewayClaims -ne 8/);
assert.match(nativeWorkflow,/\$report\.graphicalPointerHits -ne 56/);
assert.match(panel,/reward\.CycleDay==7\?DailyTreasureCeremonyTier\.Legendary/);
assert.match(native,/panel\.ProbePresentationCanvasForNativeEvidence\(\)/);
assert.match(native,/panel\.ProbeHeroRoomVisibilityForNativeEvidence\(\)/);
assert.match(native,/"Heroes \/ canonical destination",typeof\(RectTransform\)/);
assert.match(native,/Closed Heroes room incorrectly blocks daily reward/);
assert.match(native,/nativeCanvas\.enabled=false/);
// The native workflow was previously damaged by a duplicated YAML block.
// Keep this quality gate executable: one render step, one artifact upload
// and no orphaned PowerShell fragments outside their job.
assert.equal((nativeWorkflow.match(/- name: Compile Unity and render genuine /g)||[]).length,1,
    'One native Unity render job only');
assert.equal((nativeWorkflow.match(/- name: Preserve real Unity screenshots/g)||[]).length,1,
    'Native artifact upload is unique');
assert.doesNotMatch(nativeWorkflow,/^\)\s*$/m,
    'No orphan PowerShell expressions outside YAML blocks');
assert.match(nativeWorkflow,/\$report\.graphicalPointerHits -ne 56/);
const paidCardSource=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureRewardPresentation.cs','utf8');
const originalRaySource=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureRaysGraphic.cs','utf8');
// Prestige is the visual legibility of actual credited items, not a generic
// "recompensas añadidas" text below a decorative animation.
assert.match(panel,/ConfigurePrizeCards\(reward\)/);
assert.match(panel,/DailyTreasureRewardPresentation\.FromCommittedReward\(reward\)/);
assert.match(panel,/prizeGroups\[i\]\.alpha=immediate\?1f:0f/);
assert.match(panel,/prizeShelf\.gameObject\.SetActive\(true\)/);
assert.match(panel,/claimCaption\.text="CONTINUAR"/);
assert.match(panel,/rays\.SetIntensity\(frame\.Glow\*\.79f\)/);
assert.match(panel,/HasRevealedIllustratedEarnedItems/);
assert.match(native,/panel\.EarnedItemCardCount==4/);
assert.match(native,/panel\.EarnedItemCardCount==2/);
assert.match(native,/panel\.HasRevealedIllustratedEarnedItems/);
assert.match(paidCardSource,/MaxSimultaneousItems=4/);
assert.match(originalRaySource,/public void SetIntensity\(float value\)/);
// Comments can explain the authoritative gateway; only executable
// presentation code must remain incapable of issuing payouts.
const paidCardExecutable=paidCardSource.replace(/\/\/[^\n]*/g,'');
assert.doesNotMatch(paidCardExecutable,/LocalGateway|GameCommand|\.Execute\(|PlayerPrefs/);
assert.match(native,/visualOnlyMilestoneCaptures==16/);
assert.match(native,/styleIds=new\[\]\{/);
assert.match(native,/coversReward=saved/);
assert.match(nativeWorkflow,/visualOnlyMilestoneCaptures -ne 16/);
assert.match(panel,/DailyTreasureCeremonyTheme\.ForEvent\(/);
assert.match(panel,/ambient\.color=new Color\(activeTheme\.R/);
// The daily entry must have its own original, recognizable, illuminated
// Valoria chest, and only signal availability when the saved claim is valid.
assert.match(panel,/entryArtwork\.sprite=Resources\.Load<Sprite>\(/);
assert.match(panel,/"DailyTreasureArt\/treasure_closed_512"/);
assert.match(panel,/entryNotice\.gameObject\.SetActive\(entryIsAvailable\|\|pending\)/);
assert.match(panel,/entryGlow\.sprite=glowSprite/);
assert.match(panel,/float entryX=onVigiliaCanvas&&!portrait\?256f:14f/);
assert.match(panel,/entryIsAvailable=DailyTreasureRules\.CanClaim\(/);
// The large original chest visually emits actual paid item cards.
// Motion is pure artwork, tested without a Unity backend or any reward award.
const flight=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePrizeFlight.cs','utf8');
assert.match(panel,/DailyTreasurePrizeFlight\.Sample\(/);
assert.match(panel,/prizeFlightOrigin/);
assert.match(flight,/public static DailyTreasurePrizeFlightFrame Sample\(/);
assert.match(flight,/public const float ChestDuration=\.64f/);
assert.match(flight,/public const float DelayBetweenCards=\.13f/);
assert.doesNotMatch(flight,/GameCommand|LocalGateway|\.Execute\(|PlayerPrefs/);
assert.match(native,/"hud-entry"/);
assert.match(native,/Genuine unclaimed original chest medallion/);
assert.match(nativeWorkflow,/\$unclaimed=\(\$variant -eq 'hud-entry' -or \$variant -eq 'calendar'/);
assert.match(nativeWorkflow,/\$item\.readyToClaim -ne \$claimCalendar/);
// Calendar frames are visually informative but MUST NOT be called earned.
assert.match(native,/"Available calendar before claim",true,false\)/);
assert.match(native,/"Real day-four candidate before claim",true,false\)/);
assert.match(native,/readyToClaim=variant=="calendar" \|\|/);
assert.match(nativeWorkflow,/four unclaimed HUD|four genuine HUD entries|four original HUD|four genuine unclaimed HUD/i);
assert.match(panel,/SamplePaidItemFlightForNativeEvidence\(float elapsed\)/);
assert.match(native,/"day7-prize-flight"/);
assert.match(native,/"special-day4-prize-flight"/);
assert.match(nativeWorkflow,/day7-prize-flight/);
assert.match(nativeWorkflow,/special-day4-prize-flight/);
assert.match(native,/pictures\.Count==52 && inputChecks==56/);
// Highlight the actual earned seventh-day chest without making the weekly
// reward calendar constantly flash or accidentally advertise an unearned prize.
assert.match(panel,/rareDayAvailable=reward\.IsValid && upcoming==7/);
assert.match(panel,/rareDayGlow\.sprite=glowSprite/);
assert.match(panel,/rareDayGlow\.color=new Color\(1f,\.78f,\.32f,gentle\)/);
assert.match(panel,/rareHalo\.SetAsFirstSibling\(\)/);
assert.match(panel,/public bool HasMilestoneScenery/);
assert.match(panel,/public string MilestoneSceneryKey/);
assert.match(native,/var requiredScenery=new\[\]/);
assert.match(native,/panel\.HasMilestoneScenery/);
assert.match(native,/panel\.MilestoneSceneryKey==requiredScenery\[style\]/);
assert.match(panel,/DailyTreasureStageDirection\.Compose\(/);
assert.match(panel,/currentStage==DailyTreasureStageKind\.Bastion/);
assert.match(panel,/milestoneScenery\.color=new Color\(\.70f,\.68f,\.65f,/);
assert.match(panel,/milestoneBannerCaption\.text="UNA HEROÍNA SE UNE"/);
assert.match(panel,/milestoneBannerCaption\.text="HORIZONTES DESCUBIERTOS"/);
assert.match(panel,/milestoneBannerCaption\.text="VICTORIA DE LOS VIGILANTES"/);
assert.match(panel,/milestoneBannerCaption\.text="EL REINO PROSPERA"/);
assert.match(native,/VerifyGenuineSceneComposition\(/);
assert.match(native,/panel\.ActiveMilestoneStage==expected\.Kind/);
assert.match(native,/backdrop\.color\.a-expected\.BackdropOpacity/);
// Countdown is never a second clock or a new claim rule: it translates the
// same authoritative UTC-day rollover into useful mobile-facing time.
assert.match(rules,/public static int MinutesUntilNextUtcDay\(long utcTicks\)/);
assert.match(panel,/private static string NextRewardCountdown\(long utcTicks\)|static string NextRewardCountdown\(long utcTicks\)/);
assert.match(panel,/DailyTreasureRules\.MinutesUntilNextUtcDay\(utcTicks\)/);
assert.match(panel,/claimCaption\.text=NextRewardCountdown\(currentUtc\)/);
// Leaving a ceremony is an actual UI/audio state transition, not just
// hiding artwork: no lingering bronze chime over the city or the tutorial.
assert.match(panel,/void SkipReveal\(\)\s*\{[\s\S]*?originalSfx\.Stop\(\);\s*ShowResult\(true\)/);
const closeBody=panel.split('public void Close()')[1].split('void Refresh()')[0];
assert.match(closeBody,/originalSfx\.Stop\(\)/);
assert.doesNotMatch(closeBody,/gateway\.Execute\(|ClaimDailyTreasure|new GameCommand/);
// One automatic milestone per session prevents popup chains and leaves the
// remaining real, paid/notified achievements accessible from the medallion.
assert.match(panel,/entryButton=ActionButton\(entryRect,Oak,OpenEntryOrDeferredMilestone\)/);
assert.match(panel,/bool PresentNextCommittedMilestone\(\)/);
assert.match(panel,/sessionAutoCeremonyPlayed=true/);
assert.match(panel,/if\(!sessionAutoCeremonyPlayed\)\s*PresentNextCommittedMilestone\(\)/);
assert.match(panel,/milestoneQueue\.PendingCount>0/);
assert.match(panel,/entryCaption\.text=entryIsAvailable\?"TESORO":\s*pending\?"HITOS":"COFRE"/);
assert.match(panel,/entryNotice\.gameObject\.SetActive\(entryIsAvailable\|\|pending\)/);
assert.match(panel,/if\(!claimAvailable && milestoneQueue\.PendingCount>0/);
// Even if the input medallion is tapped during a hero unlock, the
// canonical 3D recruitment remains above ALL reward surfaces.
for(const entryMethod of ['void OpenEntryOrDeferredMilestone()',
                           'public void Open()']){
  const body=panel.split(entryMethod)[1]?.split('{')[1]?.slice(0,240);
  assert.ok(body && body.includes('HasVisibleCanonicalHeroRoom()'),
    'Canonical Lyra room must block every path into optional treasury: '+
    entryMethod);
}
// Narratives can start after opening a daily calendar or a premium reveal.
// Suppressing must immediately hide the optional panel, stop Foley and
// preserve the paid wallet; the real native probe tests both sequences.
assert.match(panel,/if\(suppressed && IsOpen\)/);
assert.match(panel,/if\(originalSfx && originalSfx\.isPlaying\)originalSfx\.Stop\(\)/);
assert.match(native,/panel\.Suppressed=true/);
assert.match(native,/Tutorial overlay did not interrupt the premium coffer/);
assert.match(native,/beforeInterrupt\.Resources\.Wood==afterInterrupt\.Resources\.Wood/);
// The public present API is not a bypass around tutorial/hero priority.
// Queue and report remain accessible, but forced visual presentation is
// refused while a canonical narrative room owns the screen.
assert.match(panel,/string\.IsNullOrWhiteSpace\(title\) \|\| suppressed \|\|\s*HasVisibleCanonicalHeroRoom\(\)\)return false/);
assert.match(native,/Direct ceremony API improperly covered canonical hero room/);
assert.match(native,/Direct ceremony API bypassed narrative\/tutorial priority/);
// Avoid falling back to screenshot-extracted 128px icons with baked values.
// Both the seven-day calendar and the paid card tray must use the same
// alpha-clean original resource art with no native raycast blocking.
for(const item of ['resource_wood_clean_256','resource_stone_clean_256',
                   'resource_food_clean_256','resource_hourglass_clean_256'])
    assert.ok(panel.includes(item),'Standalone painted icon absent: '+item);
assert.ok(panel.includes('Resources.Load<Sprite>("DailyTreasureArt/"+clean)'));
assert.ok(panel.includes('var sprite=LoadUnifiedResourceEmblem(item.Icon);'));
assert.ok(native.includes('VerifyCleanPaintedResources(panel);'));
assert.ok(native.includes('VerifyCleanPaintedResources(panel,4);'));
assert.ok(native.includes('VerifyCleanPaintedResources(panel,2);'));
// The real Unity TextGenerator must determine whether labels are actually
// readable in the rendered boxes; 2-line chest receipts were too tall
// for the tiny landscape gap without choosing a one-line layout.
assert.match(native,/VerifyCriticalTypography\(DailyTreasurePanel panel,string phase\)/);
assert.match(native,/label\.preferredHeight/);
assert.match(native,/Unity truncated premium text/);
assert.match(native,/VerifyCriticalTypography\(panel,"paid day seven \/ illustration cards"\)/);
assert.match(native,/VerifyCriticalTypography\(panel,"paid day four \/ illustration cards"\)/);
assert.match(panel,/ceremonialTitle\+" · GUARDADO"/);
// Premium phone UI must survive rotations WHILE the reward is visible,
// not only cold-start in each portrait/landscape screen size.
assert.match(panel,/ProbeOrientationLayoutForNativeEvidence\(\)/);
assert.match(panel,/resultText\.text=portrait\?/);
assert.match(native,/rect\.sizeDelta=new Vector2\(height,width\)/);
assert.match(native,/VerifyCriticalTypography\(panel,"paid chest after orientation change"\)/);
assert.match(native,/VerifyCriticalTypography\(panel,"paid chest after rotation restored"\)/);
assert.match(native,/Paid cards disappeared when device orientation changed/);
// A REAL native Unity 6000 compiler caught the namespace shadowing bug:
// within Eldoria.DailyTreasures, unqualified Application.isPlaying resolves
// to Eldoria.Application (the gateway namespace), not UnityEngine.Application.
// Pure Roslyn syntax parsing cannot detect this semantic type error.
assert.equal((panel.match(/UnityEngine\.Application\.isPlaying/g)||[]).length,3);
assert.doesNotMatch(panel,/(?<!UnityEngine\.)Application\.isPlaying/);
// Genuine native Unity 6000 compilation also uncovered an unqualified
// Application.dataPath in this editor-only harness. Within the Eldoria
// namespace it resolves to Eldoria.Application rather than UnityEngine.
assert.match(native,/UnityEngine\.Application\.dataPath/);
assert.doesNotMatch(native,/(?<!UnityEngine\.)\bApplication\.dataPath/);
// Real Unity screenshots exposed another independent art/UX defect:
// the lower resource line on Days 4 and 6 overlapped the bronze border.
// Assert the responsive fix and its genuine native RectTransform gate.
assert.match(panel,/bool compactDouble=twoResources && cellH<=103f/);
assert.match(panel,/float labelBottom=compactDouble\?20f/);
assert.ok(panel.includes("bool twoResources=dayBody[i].text.IndexOf('\\n')>=0;"));
assert.match(panel,/if\(calendar && calendar\.gameObject\.activeInHierarchy\)\s*Layout\(\)/);
assert.match(native,/Third-day loyalty bonus text overlaps the/);
assert.match(native,/VerifyCalendarMultipleResourceMargins\(DailyTreasurePanel panel,/);
assert.match(native,/margin>=4\.5f/);
assert.match(native,/Actual reward contents/);
// The original 52 captured stages contained NO real images of the ordinary
// daily rewards: days 1,2,3,5,6 can fail without the luxurious chest.
// Keep the new 20 genuine claims and their native UI/visual receipts apart
// from the 8 contracted day-four/day-seven gateway claims. Prevent a future
// shortcut from disguising 20 colour mockups as actual paid items.
assert.match(native,/quietPictures\.Count==20 && quietInputChecks==60/);
assert.match(native,/report\.quietGatewayClaims==20/);
assert.match(native,/quietGateway\.Execute\(command\)\.Ok/);
assert.match(native,/VerifyEarnedItemReadability\(panel,descriptors\.Length\)/);
assert.match(native,/report\.quietShots=quietPictures\.ToArray\(\)/);
assert.match(nativeWorkflow,/\$report\.quietGatewayClaims -ne 20/);
assert.match(nativeWorkflow,/\$report\.quietPointerHits -ne 60/);
assert.match(nativeWorkflow,/\$report\.quietImages -ne 20/);
assert.match(nativePixelAudit,/def quiet_label_pixel_proof\(image,day,width,height\)/);
assert.match(nativePixelAudit,/Ordinary reward not backed by real paid data/);
assert.match(nativePixelAudit,/additionalQuietDayOriginalPngCount/);
assert.match(nativePixelAudit,/ordinary_paid_rewards_contact_sheet\.png/);
// The original genuine Unity day-seven calendar displayed only 3/4 paid
// resources: the five-minute accelerator was omitted by a 4-line text clip.
// Never declare visual success without ALL promised items, including streak
// food, visible at every tested screen size.
assert.match(panel,/if\(reward\.CycleDay==7 && reward\.Wood>0/);
assert.match(panel,/"Madera "\+reward\.Wood\+" · Piedra "\+reward\.Stone/);
assert.match(panel,/"5 min x"\+reward\.SpeedupFiveMinutes/);
assert.match(native,/VerifySeventhDayPreviewTruth\(panel,"calendar day seven"\)/);
assert.match(native,/label\.preferredWidth<=rect\.width\+3f/);
assert.match(native,/label\.preferredHeight<=rect\.height\+3f/);
console.log('PASS daily treasures gateway integration source invariants');
