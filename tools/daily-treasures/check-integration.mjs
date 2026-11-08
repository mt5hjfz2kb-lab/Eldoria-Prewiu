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
assert.match(retention,/dailyTreasures\.PresentCommittedMilestone\(/);
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
assert.ok(panel.includes('"DailyTreasureArt/"+DayIllustrations[i]'));
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
assert.match(native,/pictures\.Count==16/);
assert.match(nativeWorkflow,/\$report\.images -ne 16/);
// PR173's canonical independently rendered Canvas hides the old Royal HUD.
// Genuine daily UI must relocate to the native safe area or be invisible.
assert.match(panel,/GameObject\.Find\("VIGILIA \/ independent canvas"\)/);
assert.match(panel,/vigilia\.transform\.Find\("Safe composition"\)/);
assert.match(panel,/root\.SetParent\(safe,false\)/);
assert.match(panel,/root\.SetParent\(originalSafeArea,false\)/);
assert.match(panel,/onVigiliaCanvas\?\(portrait\?102f:177f\)/);
assert.match(retention,/if\(!dailyTreasures && safe\)/);
assert.match(panel,/milestoneQueue\.Observe\(gateway\.Snapshot\(\)\)/);
assert.match(panel,/milestoneQueue\.ConfirmPresented\(milestone\.EventId\)/);
assert.doesNotMatch(panel,/new GameObject\([^\n]+typeof\(Canvas\)/);
// Both real chest ceremony tiers must be exercised with real LocalGateway
// payments, not a screenshot of unearned or hardcoded rewards.
assert.match(native,/special-day4-calendar/);
assert.match(native,/special-day4-opening/);
assert.match(native,/specialAfter\.Resources\.Wood==specialBefore\.Resources\.Wood\+90/);
assert.match(native,/pictures\.Count==24 && inputChecks==32 && report\.realGatewayClaims==8/);
assert.match(nativeWorkflow,/\$report\.images -ne 24/);
assert.match(nativeWorkflow,/\$report\.realGatewayClaims -ne 8/);
assert.match(nativeWorkflow,/\$report\.graphicalPointerHits -ne 32/);
assert.match(panel,/reward\.CycleDay==7\?DailyTreasureCeremonyTier\.Legendary/);
console.log('PASS daily treasures gateway integration source invariants');
