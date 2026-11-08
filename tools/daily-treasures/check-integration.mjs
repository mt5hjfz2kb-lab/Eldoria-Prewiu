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
console.log('PASS daily treasures gateway integration source invariants');
