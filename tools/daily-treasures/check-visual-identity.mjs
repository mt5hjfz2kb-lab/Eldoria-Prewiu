import fs from 'node:fs';
import assert from 'node:assert/strict';

// Art direction contract from the actual Eldoria/Vigilia approved UI:
// dark slate, forged copper and gold, restrained burgundy, parchment ivory,
// actual 3D/painted kingdom imagery, realistic oak-and-bronze coffer.
//
// A beautiful but unrelated purple/fairyland/flower/coin/gem poster is a
// REJECTED concept and must never be mistaken for the playable project UI.
// This is a source-level guard. Native Unity and human art review are separate.

const text=(path)=>fs.readFileSync(path,'utf8');
const panel=text('Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePanel.cs');
const theme=text('Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureCeremonyTheme.cs');
const stage=text('Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureStageDirection.cs');
const grants=text('Unity/Assets/Eldoria/Scripts/Domain/DailyTreasureRules.cs');
const receipt=text('Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureRewardPresentation.cs');

for (const [name,source] of [
  ['Owner Vigilia charcoal scene',/new Color\(\.085f,\.090f,\.095f,1f\)/],
  ['Owner Vigilia copper linework',/new Color\(\.92f,\.70f,\.40f,1f\)/],
  ['Warm old-ivory, not bright-white text',/new Color\(\.98f,\.94f,\.84f,1f\)/],
  ['Vigilia-sourced burgundy call-to-action',/VigiliaArt\/PremiumSurfaces\/burgundy_action/],
  ['Vigilia-sourced solitary slate resource card',/VigiliaArt\/PremiumSurfaces\/slate_resource/],
  ['Original illustrated main modal is never stretched',/case "valoria_frame_main_512":/],
  ['Owner-consistent resource symbols',/LoadUnifiedResourceEmblem\(item\.Icon\)/],
  ['Vigilia-specific resource pack when installed',/VigiliaArt\/ResourceIcons\//],
  ['Fallback to original self-contained nine-slice',/DailyTreasureArt\/"\+resource/]
]) assert.match(panel,source,name);

for(const signature of [
  'milestone:hero:lyra','milestone:bastion:',
  'scout:','vigilant:'
]) assert.ok(theme.includes(signature),
  'Unknown or invented celebration category: '+signature);

// The Breach ALONE may glow violet; Vigilants' ceremonies use aged bronze
// and wrought iron. Keep all event palettes dark and low-intensity.
assert.match(theme,/strength=legendary\?\.33f:\.22f/);
assert.match(theme,/\.54f,\.49f,\.40f,strength,legendary/);
assert.doesNotMatch(theme,/\.44f,\.39f,\.53f,strength,legendary/);
// The original Lyra hall was genuinely too dark in 390x844 Unity capture.
// Only portrait Lyra receives increased visibility; never brighten the
// other painted milestones or the Bastion city with a blanket neon filter.
assert.match(stage,/\.95f,\.08f,false/);
assert.match(stage,/The daily coffer is absent/);
assert.match(panel,/static Color MilestonePaintTint\(/);
assert.match(panel,/hallPortrait\?\.82f:\.70f/);
assert.match(panel,/milestoneScenery\.color=MilestonePaintTint\(stage/);
assert.match(stage,/\.88f,\.92f,\.10f,true/);

// Lyra's focal image must be the actual image from the live, canonical
// HeroesApprovedDestination inventory. No invented character portrait or
// old whole-screen screenshot may masquerade as a genuine unlock.
assert.match(panel,/static Sprite TryCanonicalLyraPortrait\(\)/);
assert.match(panel,/Eldoria\.HeroesUI\.HeroesApprovedDestination/);
assert.match(panel,/GetField\("ReadHeroes"\)/);
assert.match(panel,/image\.rect\.width>=256f/);
assert.match(panel,/milestoneLyraPortrait=Plate\(lyraPortraitRect,Color\.white,false\)/);
assert.match(panel,/milestoneLyraPortrait\.gameObject\.SetActive\(portrait!=null\)/);
assert.match(panel,/milestoneLyraPortrait\.sprite=null/);


// The real daily ledger has exactly real owned resources and accelerators.
// Old fantasy mockups depicting gems, gold packs, potions and loot rarity are
// NOT a valid input to this UI. Only the actual LocalGateway credits matter.
for(const token of [
  'resource_wood','resource_stone','resource_food','resource_hourglass'
]) assert.ok(receipt.includes(token),'Required genuinely owned resource icon: '+token);
assert.doesNotMatch(grants,/\b(PremiumGems|FrostStars|Diamonds|GoldCoins|LootCrate|Lootbox)\b/i);
assert.doesNotMatch(receipt,/\b(PremiumGems|FrostStars|Diamonds|GoldCoins|LootCrate|Lootbox)\b/i);
assert.match(panel,/milestoneScenery\.raycastTarget=false/);
assert.match(panel,/bool hasImage=milestoneScenery\.texture!=null/);
assert.match(panel,/milestoneScenery\.gameObject\.SetActive\(hasImage\)/);
assert.match(panel,/milestoneVignette\.gameObject\.SetActive\(hasImage\)/);
assert.match(panel,/DailyTreasureBastionCamera\.Capture\(/);

// Preserve game ownership, rather than painting an imaginary Bastion city.
// Lyra/scout/Vigilant scenery is original project source, not a pasted fake HUD.
for(const path of [
  'EldoriaV41/heroes-hall-illustrated-v2',
  'EldoriaRetention/scouts-painterly-v2',
  'EldoriaRetention/vigilants-painterly-v2'
]) assert.ok(panel.includes(path));
assert.match(stage,/if\(kind==DailyTreasureStageKind\.Bastion\)/);
// Real Bastion visible through a single forged bronze main plate; genuine
// story supported by local ink, not a fictional city or another ornate frame.
// The real Bastion scene must remain visible through BOTH layers. Earlier
// approximately 35% light transmission was a barely readable city; the
// latest .38 backdrop + .34 genuine framed plate leaves over 40% visibility.
assert.match(stage,/BastionPlateOpacity=\.34f/);
assert.match(stage,/\.38f,0f,0f,false/);
assert.match(panel,/Bastion \/ real-city milestone caption ink/);
assert.match(panel,/bastionStoryInk\.gameObject\.SetActive\(eventStage\)/);
assert.match(panel,/realCity\?\.82f:\.67f/);
assert.match(panel,/currentStage==DailyTreasureStageKind\.Bastion/);
console.log('PASS Eldoria premium visual identity: matte charcoal, aged copper/gold, burgundy action, true source art, no mockup currency');
