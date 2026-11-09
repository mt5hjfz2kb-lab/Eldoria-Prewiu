import fs from 'node:fs';
import assert from 'node:assert/strict';

// Independent arithmetic audit of the literal Unity layout contract.
// Useful preflight, NOT a substitute for an actual Unity rendering.
const code=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePanel.cs','utf8');
const stageCode=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureStageDirection.cs','utf8');
assert.match(code,/stage\.SideBySide\?stage\.SceneX:-panelW\*\.10f/,
  'Portrait ceremony title must not collide with Skip');
assert.match(code,/Bastion \/ real-city milestone caption ink/);
assert.match(code,/DailyTreasureStageDirection\.BastionPlateOpacity/);
assert.match(stageCode,/BastionPlateOpacity=\.34f/);
assert.match(stageCode,/\.38f,0f,0f,false/);
assert.match(code,/void AnimateDailyCeremonyReflow\(float progress\)/,
  'The premium coffer-to-inventory layout must move smoothly');
assert.match(code,/AnimateDailyCeremonyReflow\(frame\.Text\)/,
  'Light settle also drives the actual frame transition');
const reflowBody=code.split('void AnimateDailyCeremonyReflow(float progress)')[1]
  .split('DailyTreasureRevealFrame PaintCeremonyFrame')[0];
assert.doesNotMatch(reflowBody,/new\[\]/,
  'No per-frame heap arrays during mobile chest animation');
for(const [name,regex] of [
  ['seventh day occupies two columns',/int span=i==6\?2:1/],
  ['normal glyph height',/compactDouble\?23f:tiny\?24f:28f/],
  ['normal reward body height',/float labelHeight=compactDouble\?30f:/],
  ['roomy calendar real icon enlargement',/float iconY=roomy\?10f:/],
  ['compact doubled-resource still supported',/compactDouble\?3f:twoResources\?4f:/],
  ['premium 420px calendar on larger screens',/roomyCalendar\?420f:370f/],
  ['day-three streak uses actual preview text',/bool twoResources=dayBody\[i\]\.text\.IndexOf\('\\n'\)>=0/],
  ['quiet item avoids text collision',/0f,-cardH\*\.08f,cardW-15f,24f/],
  ['quiet item has real quantity bounds',/0f,-cardH\*\.35f,cardW-15f,28f/],
  ['large reward body grows with card',/Mathf\.Max\(52f,cellH-39f\)/],
  ['skip hit target 100 by 45',/-10f,-60f,100f,45f/],
  ['close hit target 48 square',/-8f,-8f,48f,48f/],
  ['real 7-day reward data on cards',/DailyTreasureRules\.ForDay\(i\+1/],
  ['focused reward view',/header\.gameObject\.SetActive\(false\)/],
  ['centralized ordinary rewards',/quietRewards\?\(portrait\?64f:35f\)/],
  ['cinematic opening chest enlarged',/artHeight=openingChest\?\(portrait\?265f:194f\)/],
  ['intentional opening width',/openingChest\|\|paidChest\?650f:740f/],
  ['responsive opening height',/focusedQuiet\?330f:openingChest\?300f:\s*roomyCalendar\?420f:370f/],
  ['readable day seven premium shelf',/portrait\?158f:76f/],
  ['larger genuine card width',/maxPaidCardW=prizeCount<=2\?190f:portrait\?170f:146f/],
  ['larger genuine card height',/prizeRows==2\?72f:/],
  ['real unaltered illustrated reward cards',/DailyTreasureRewardPresentation\.FromCommittedReward\(reward\)/],
  ['real command gateway rewards',/new GameCommand\(Guid\.NewGuid\(\)\.ToString\("N"\)/]
])assert.match(code,regex,name);

const checks=[];
function gap(a,b){return b-a;}
function rect(cx,cy,w,h){return {x0:cx-w/2,x1:cx+w/2,y0:cy-h/2,y1:cy+h/2};}
const eps=.01;
function noOverlap(a,b)
{
  return a.x1<=b.x0+eps||b.x1<=a.x0+eps||
    a.y1<=b.y0+eps||b.y1<=a.y0+eps;
}
for(const [w,h] of [[1600,900],[900,1600],[844,390],[390,844],
  [667,375],[375,667],[1024,768],[768,1024]])
{
  const portrait=h>w;
  const pw=Math.min(w-24,portrait?440:740);
  const ph=Math.min(h-20,portrait?700:h>=500?420:370);
  const cols=portrait?2:4, rows=portrait?4:2;
  const gap=7,left=15,gridTop=82,gridBottom=83;
  const gh=ph-gridTop-gridBottom;
  const cw=(pw-2*left-gap*(cols-1))/cols;
  const ch=(gh-gap*(rows-1))/rows;
  assert.ok(cw>=144 && ch>=90,`${w}x${h} card size is readable`);
  const cards=[];
  for(let i=0;i<7;i++)
  {
    const col=i%cols,row=Math.floor(i/cols),span=i===6?2:1;
    const width=cw*span+gap*(span-1);
    const x=-pw/2+left+col*(cw+gap)+width/2;
    const y=gh/2-row*(ch+gap)-ch/2;
    const card=rect(x,y,width,ch);
    assert.ok(card.x0>=-pw/2-eps && card.x1<=pw/2+eps,
      `${w}x${h}: day ${i+1} exceeds panel width`);
    assert.ok(card.y0>=-gh/2-eps && card.y1<=gh/2+eps,
      `${w}x${h}: day ${i+1} exceeds calendar height`);
    for(const other of cards)assert.ok(noOverlap(card,other),
      `${w}x${h}: day ${i+1} overlaps another reward`);
    cards.push(card);
    const tiny=ch<96,roomy=ch>=123.5;
    const titleBottom=ch/2-5-(i<6&&tiny?24:25);
    if(i<6)
    {
      // Mirror the CURRENT Unity layout rather than measuring an obsolete
      // 2026-11-08 version that previously passed despite real clipped text.
      // Day three can be ONE resource without loyalty, or TWO when a
      // real third-day streak adds food to the speedup. The production
      // layout now reads the actual preview text, never a hard-coded
      // day-4/day-6 assumption. Model BOTH legitimate states in all sizes.
      const rewardLineScenarios=i===2?[1,2]:(i===3||i===5)?[2]:[1];
      for(const lineCount of rewardLineScenarios)
      {
        const twoResources=lineCount===2;
        const compactDouble=twoResources&&ch<=103;
        const iconY=roomy?10:compactDouble?3:twoResources?4:tiny?2:3;
        // Keep BOTH actual illustrated PNG and fallback art within bounds.
        const iconH=roomy?40:compactDouble?23:tiny?24:28;
        const iconTop=iconY+iconH/2,iconBottom=iconY-iconH/2;
        const bottom=compactDouble?20:twoResources?27:tiny?17:18.5;
        const bodyH=compactDouble?30:twoResources?36:tiny?31:33;
        const bodyTop=-ch/2+bottom+bodyH/2;
        assert.ok(titleBottom>=iconTop+2,
          `${w}x${h}: day ${i+1} with ${lineCount} lines: heading hits icon`);
        assert.ok(iconBottom>=bodyTop+2,
          `${w}x${h}: day ${i+1} with ${lineCount} lines: icon hits reward text`);
        if(twoResources)assert.ok(bottom-bodyH/2>=4.5,
          `${w}x${h}: day ${i+1} loyalty/second item sits on the bronze border`);
      }
    }
    else
    {
      const iconRight=-width/2+48+(roomy?80:73)/2;
      const bodyLeft=38-(width-116)/2;
      const bodyTop=-12+Math.max(52,ch-39)/2;
      assert.ok(bodyLeft-iconRight>=8,
        `${w}x${h}: ceremonial chest overlaps actual rewards`);
      assert.ok(titleBottom>=bodyTop+1,
        `${w}x${h}: day seven description overlaps heading`);
    }
  }
  const topCards=gh/2+.5;
  const bottomCards=-gh/2+.5;
  const headerBottom=ph/2-49-26;
  const claimTop=-ph/2+13+54;
  assert.ok(headerBottom-topCards>=6,
    `${w}x${h}: subheading touches calendar`);
  assert.ok(bottomCards-claimTop>=12,
    `${w}x${h}: last row touches claim action`);
  assert.ok(48>=44 && 45>=44 && 54>=44,
    'physical hit targets are at least 44px');
  const skipBottom=ph/2-60-45;
  const receiptTop=(portrait?-140:-95)+(portrait?165:118)/2;
  assert.ok(skipBottom>receiptTop+10,
    `${w}x${h}: skip button touches the paid receipt`);
  // The premium opening is NOT the old 740x370 empty lower third. The
  // authored coffer becomes the focus in an intimate frame before real
  // credited cards take their place. These are actual Unity RectTransforms,
  // not a concept render.
  const openW=Math.min(w-24,portrait?440:650);
  const openH=Math.min(h-20,portrait?450:300);
  const openingChest=rect(0,portrait?64:42,330,portrait?265:194);
  const openingCaption=rect(0,portrait?-132:-100,openW-45,portrait?45:32);
  assert.ok(openingChest.x0>=-openW/2-0.01 &&
            openingChest.x1<=openW/2+0.01 &&
            openingChest.y1<=openH/2-8,
    `${w}x${h}: premium opening art no longer fits its cinematic stage`);
  assert.ok(openingChest.y0>=openingCaption.y1+15 &&
            openingCaption.y0>=-openH/2+15,
    `${w}x${h}: cinematic chest, caption and lower border are cramped`);
  assert.ok(openH<(portrait?700:370),
    `${w}x${h}: old oversized empty opening panel returned`);

  // Reveal choreography must ease from the opening to the REAL inventory
  // layout without a sudden cut, offscreen artwork or overlapping caption.
  // Its progress is the already-tested RevealTimeline.Text smoothstep.
  for(const t of [0,.25,.5,.75,1])
  {
    const lerp=(a,b)=>a+(b-a)*t;
    const tweenH=lerp(openH,Math.min(h-20,portrait?700:370));
    const shortLand=!portrait && Math.min(h-20,370)<365;
    const artY=lerp(portrait?64:42,
                    portrait?116:shortLand?84:89);
    const artH=lerp(portrait?265:194,
                    portrait?246:shortLand?164:170);
    const captionY=lerp(portrait?-132:-100,
      portrait?(Math.min(h-20,700)<680?-217:-232):-15);
    const captionH=portrait?45:32;
    const artTop=artY+artH/2,artBottom=artY-artH/2;
    const titleTop=captionY+captionH/2;
    const titleBottom=captionY-captionH/2;
    assert.ok(artTop<=tweenH/2-7 && artBottom>=titleTop+1 &&
              titleBottom>=-tweenH/2+14,
      `${w}x${h}: cinematic transition fraction ${t} clips artwork/title`);
  }

  // A *real* claim reveals the original chest and receipt cards on a denser
  // landscape stage. Day four (two rewards) receives larger individual
  // illustrated cards; day seven (four rewards) uses nearly the full width.
  const paidW=Math.min(w-24,portrait?440:650);
  const paidH=Math.min(h-20,portrait?700:370);
  const shortPaid=!portrait && paidH<365;
  const chest=rect(0,portrait?116:shortPaid?84:89,
    portrait?284:shortPaid?290:300,
    portrait?246:shortPaid?164:170);
  const captionY=portrait?(paidH<680?-217:-232):-15;
  const captionH=portrait?(paidH<680?39:50):29;
  const caption=rect(0,captionY,paidW-45,captionH);
  const shelf=rect(0,portrait?-115:-69,paidW-34,portrait?158:76);
  // Earned cards are children of prizeShelf and their anchored positions are
  // SHELF-LOCAL. Preserve global bounds for caption/Continue comparisons,
  // but never compare local card positions with global shelf coordinates.
  const localShelf=rect(0,0,paidW-34,portrait?158:76);
  const claim=rect(0,-paidH/2+13+54/2,
    Math.min(paidW-30,portrait?325:405),54);
  assert.ok(chest.y1<=paidH/2+eps && chest.y0>=caption.y1+1,
    `${w}x${h}: final authored chest obscures actual reward caption`);
  assert.ok(noOverlap(shelf,caption)&&noOverlap(caption,claim)&&
            noOverlap(shelf,claim),
    `${w}x${h}: paid caption, inventory and Continue overlap`);
  for(const count of [1,2,4])
  {
    const cols=portrait&&count>=3?2:count;
    const rows=Math.ceil(count/cols);
    const prizeGap=portrait?8:6;
    const maximum=count<=2?190:portrait?170:146;
    const cardW=Math.min(maximum,
      (paidW-38-(cols-1)*prizeGap)/cols);
    const cardH=rows===2?72:portrait?(paidH<680?82:92):72;
    const cards=[];
    for(let i=0;i<count;i++)
    {
      const row=Math.floor(i/cols),col=i%cols;
      const itemsInRow=Math.min(cols,count-row*cols);
      const rowWidth=itemsInRow*cardW+(itemsInRow-1)*prizeGap;
      const x=-rowWidth/2+col*(cardW+prizeGap)+cardW/2;
      const y=rows===1?0:row===0?cardH/2+prizeGap/2:
                                           -cardH/2-prizeGap/2;
      const frame=rect(x,y,cardW,cardH);
      assert.ok(frame.x0>=localShelf.x0+1 && frame.x1<=localShelf.x1-1 &&
                frame.y0>=localShelf.y0+1 && frame.y1<=localShelf.y1-1,
        `${w}x${h}: real paid card ${i+1} exceeds compact shelf`);
      for(const other of cards)assert.ok(noOverlap(frame,other),
        `${w}x${h}: real paid cards overlap one another`);
      cards.push(frame);
      // Precisely mirror Unity's independent icon, item title and actual
      // amount. Enlarged 18px quantities require 26px high rects; the
      // previous 23px rect would fail the genuine native typography gate.
      const icon=rect(-cardW/2+27,0,43,43);
      const labelWidth=cardW-62;
      const labelX=cardW/2-labelWidth/2-7;
      const title=rect(labelX,12,labelWidth,27);
      const amount=rect(labelX,-16,labelWidth,26);
      const cardBounds=rect(0,0,cardW,cardH);
      for(const [name,box] of [['icon',icon],['name',title],['amount',amount]])
        assert.ok(box.x0>=cardBounds.x0+1 && box.x1<=cardBounds.x1-1 &&
                  box.y0>=cardBounds.y0+1 && box.y1<=cardBounds.y1-1,
          `${w}x${h}: painted ${name} clips an earned card`);
      assert.ok(noOverlap(icon,title)&&noOverlap(icon,amount)&&
                noOverlap(title,amount),
        `${w}x${h}: original item art, name and amount overlap`);
      const nameSize=cardW>=170?12:11,amountSize=18;
      assert.ok(27>=nameSize*1.35 && 26>=amountSize*1.35,
        `${w}x${h}: text rectangles will clip grown reward fonts`);
    }
    assert.ok(cardW>=(portrait?150:140),
      `${w}x${h}: actual item frames too narrow for a premium receipt`);
  }
  // Normal resource-only days are intentionally NOT shown in a tiny
  // empty chest position: 1–2 originally illustrated cards are centered,
  // with a compact receipt and a genuinely reachable Continue action.
  for(const count of [1,2])
  {
    // The quiet receipt is intentionally NOT the 740px wide legendary
    // banner. Its actual geometry depends on one or two already-paid cards.
    const quietW=Math.min(w-24,portrait?
      (count===1?336:410):(count===1?340:448));
    const quietPanelH=Math.min(h-20,portrait?430:330);
    const quietShelf=rect(0,portrait?64:35,quietW-34,portrait?175:137);
    const quietCaption=rect(0,portrait?-106:-74,quietW-45,41);
    const quietContinue=rect(0,portrait?-175:-127,
      Math.min(quietW-30,portrait?325:405),54);
    assert.ok(quietW<=448 &&
              (count===1?quietW<=340:quietW>=351),
      `${w}x${h}: quiet card receipt reverted to an empty giant parchment`);
    assert.ok(noOverlap(quietShelf,quietCaption),
      `${w}x${h}: the ordinary illustrated rewards obscure the actual receipt`);
    assert.ok(quietShelf.y1<=quietPanelH/2-8 &&
              quietContinue.y0>=-quietPanelH/2+8,
      `${w}x${h}: compact premium window clips the artwork or Continue`);
    assert.ok(noOverlap(quietCaption,quietContinue),
      `${w}x${h}: Continue overlaps the normal reward's confirmation`);
    assert.ok(noOverlap(quietShelf,quietContinue),
      `${w}x${h}: normal resource items cover Continue`);
    const cardW=Math.min(175,(quietW-38-(count-1)*(portrait?8:6))/count);
    const cardH=portrait?129:115;
    assert.ok(cardW>=120&&cardH>=100,
      `${w}x${h}: ordinary rewards are too small for premium illustrated art`);
    const separation=portrait?8:6;
    const left=-(count*cardW+(count-1)*separation)/2;
    for(let i=0;i<count;i++)
    {
      const card=rect(left+cardW/2+i*(cardW+separation),
        portrait?64:35,cardW,cardH);
      assert.ok(card.x0>=quietShelf.x0 && card.x1<=quietShelf.x1 &&
                card.y0>=quietShelf.y0 && card.y1<=quietShelf.y1,
        `${w}x${h}: normal illustrated reward is outside its frame`);
      // The non-chest days (1/2/3/5/6) were missing from the 52-shot
      // Unity test. Independently reject overlap in their premium cards.
      // Native proof for this family is still separately required.
      const frame=rect(0,0,cardW,cardH);
      const icon=rect(0,cardH*.26,53,48);
      const title=rect(0,-cardH*.08,cardW-15,24);
      const quantity=rect(0,-cardH*.35,cardW-15,28);
      for(const [name,box] of [['icon',icon],['name',title],['amount',quantity]])
        assert.ok(box.x0>=frame.x0+2&&box.x1<=frame.x1-2&&
                  box.y0>=frame.y0+2&&box.y1<=frame.y1-2,
          `${w}x${h}: ordinary ${name} clips a premium frame`);
      assert.ok(noOverlap(icon,title)&&noOverlap(icon,quantity)&&
                noOverlap(title,quantity),
        `${w}x${h}: ordinary reward name and quantity collide with the art`);
      assert.ok(title.y1-title.y0>=12*1.35&&
                quantity.y1-quantity.y0>=20*1.35,
        `${w}x${h}: normal item text line box would vanish in Unity`);
    }
  }

  // Previously the heraldic title and Skip overlapped by 20+ pixels in
  // narrow portrait; check both physical hit areas and text composition.
  // Two distinct real event compositions: an original landscape scene
  // with the chest to the right; a true Bastion view with the built city
  // visible behind the chest. Neither may cover its required Skip touch area.
  // The portrait heading is lower and wider so a full-length chapter label
  // does not become a floating fake button layered over the real artwork.
  const eventH=Math.min(h-20,portrait?700:370);
  const headingW=portrait?Math.min(pw-36,280):Math.min(pw*.52,302);
  for(const family of ['painterly','bastion'])
  {
    const headingX=portrait?0:family==='painterly'?-.247*pw:-pw*.10;
    const heading=rect(headingX,eventH*(portrait?.29:.405),headingW,40);
    const skip=portrait?rect(pw/2-60,eventH/2-82.5,100,45):
      family==='painterly'
        ?rect(-.247*pw,eventH/2-88.5,100,45)
        :rect(pw/2-60,eventH/2-82.5,100,45);
    assert.ok(noOverlap(heading,skip),
      `${w}x${h}: ${family} story banner steals the Skip tap`);
    assert.ok(heading.x0>=-pw/2+2 && heading.x1<=pw/2-2 &&
              heading.y1<=eventH/2-5,
      `${w}x${h}: ${family} title cropped by its own stage`);
    // Real standalone underline is centered inside the unframed typographic
    // heading: never widen past the stage or impersonate a CTA button.
    const underline=rect(headingX,heading.y0+3,headingW-30,2);
    assert.ok(underline.x0>=heading.x0 && underline.x1<=heading.x1 &&
              underline.y0>=heading.y0 && underline.y1<=heading.y1,
      `${w}x${h}: ${family} forged-ink accent escapes its title`);
  }
  assert.ok((1-.38)*(1-.34)>.40,
    `${w}x${h}: actual Bastion city hidden by stacked panels`);
  // Real non-paying milestones also use the same premium authored chest.
  // Their short true-gameplay report line must remain clear of the Continue
  // button, even on a 375px portrait or 390px landscape device.
  const milestoneChest=rect(0,portrait?105:87,portrait?294:282,portrait?265:175);
  const milestoneCaption=rect(0,portrait?-148:-71,pw-45,portrait?141:66);
  const milestoneContinue=rect(0,-ph/2+13+54/2,Math.min(pw-30,portrait?325:405),54);
  assert.ok(noOverlap(milestoneChest,milestoneCaption),
    `${w}x${h}: hero or kingdom story overlays the premium chest`);
  assert.ok(noOverlap(milestoneCaption,milestoneContinue),
    `${w}x${h}: milestone report covers Continue action`);
  assert.ok(milestoneChest.y1<=ph/2+eps,
    `${w}x${h}: ceremonial chest exceeds top of frame`);

  checks.push({size:`${w}x${h}`,cards:cards.length,daySevenWidth:Math.round(cards[6].x1-cards[6].x0),touchTargets:true});
}
console.log('PASS daily treasures layout: '+checks.length+' aspect/size geometries, seven non-overlapping cards, real head/icon/body and >=44px controls');
for(const check of checks)console.log(JSON.stringify(check));
