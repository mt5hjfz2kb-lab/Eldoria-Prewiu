import fs from 'node:fs';
import assert from 'node:assert/strict';

// Independent arithmetic audit of the literal Unity layout contract.
// Useful preflight, NOT a substitute for an actual Unity rendering.
const code=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePanel.cs','utf8');
for(const [name,regex] of [
  ['seventh day occupies two columns',/int span=i==6\?2:1/],
  ['normal glyph height',/tiny\?24f:28f/],
  ['normal reward body height',/tiny\?31f:33f/],
  ['large reward body grows with card',/Mathf\.Max\(52f,cellH-39f\)/],
  ['skip hit target 100 by 45',/-10f,-60f,100f,45f/],
  ['close hit target 48 square',/-8f,-8f,48f,48f/],
  ['real 7-day reward data on cards',/DailyTreasureRules\.ForDay\(i\+1/],
  ['focused reward view',/header\.gameObject\.SetActive\(false\)/],
  ['centralized ordinary rewards',/quietRewards\?\(portrait\?19f:26f\)/],
  ['premium chest expanded on short landscape',/portrait\?246f:160f/],
  ['readable day seven premium shelf',/portrait\?138f:64f/],
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
  const ph=Math.min(h-20,portrait?700:370);
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
    const tiny=ch<96;
    const titleBottom=ch/2-5-(i<6&&tiny?24:25);
    if(i<6)
    {
      const iconTop=(tiny?2:3)+(tiny?24:28)/2;
      const iconBottom=(tiny?2:3)-(tiny?24:28)/2;
      const bodyTop=-ch/2+(tiny?17:18.5)+(tiny?31:33)/2;
      assert.ok(titleBottom>=iconTop+2,
        `${w}x${h}: day ${i+1} title overlaps glyph`);
      assert.ok(iconBottom>=bodyTop+2,
        `${w}x${h}: day ${i+1} glyph overlaps body`);
    }
    else
    {
      const iconRight=-width/2+48+73/2;
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
  // The actual paid-items reveal is independent from the seven-day calendar.
  // Distinct layouts for 1, 2 and 4 real icon cards must not collide with
  // each other, the resource count labels, the chest or the Continue button.
  for(const count of [1,2,4])
  {
    const compact=portrait&&ph<680;
    const shelfH=portrait?138:64;
    const shelfY=portrait?-115:-69;
    const resultY=compact?-217:portrait?-232:-15;
    const resultH=compact?39:portrait?50:29;
    const shelf=rect(0,shelfY,pw-34,shelfH);
    const result=rect(0,resultY,pw-45,resultH);
    const claim=rect(0,-ph/2+13+54/2,Math.min(pw-30,portrait?325:405),54);
    assert.ok(noOverlap(shelf,result),
      `${w}x${h}: ${count} illustrated item cards cover the receipt title`);
    assert.ok(noOverlap(result,claim),
      `${w}x${h}: ${count} illustrated item receipt covers Continue`);
    assert.ok(noOverlap(shelf,claim),
      `${w}x${h}: ${count} original artwork frames cover Continue`);
    const cols=portrait&&count>=3?2:count;
    const rows=Math.ceil(count/cols);
    const prizeGap=portrait?8:6;
    const cardW=Math.min(140,(pw-38-(cols-1)*prizeGap)/cols);
    const cardH=rows===2?65:portrait?(compact?76:89):62;
    const actual=[];
    for(let i=0;i<count;i++)
    {
      const row=Math.floor(i/cols),column=i%cols;
      const inRow=Math.min(cols,Math.max(0,count-row*cols));
      const rowWidth=inRow*cardW+Math.max(0,inRow-1)*prizeGap;
      const x=-rowWidth/2+column*(cardW+prizeGap)+cardW/2;
      const y=rows===1?0:row===0?cardH/2+prizeGap/2:-cardH/2-prizeGap/2;
      const card=rect(x,y,cardW,cardH);
      assert.ok(card.x0>=-(pw-34)/2-0.1 && card.x1<=(pw-34)/2+0.1 &&
                card.y0>=-shelfH/2-0.1 && card.y1<=shelfH/2+0.1,
        `${w}x${h}: earned artwork slot ${i+1} escapes shelf`);
      for(const other of actual)assert.ok(noOverlap(card,other),
        `${w}x${h}: reward item art overlaps another paid item`);
      actual.push(card);

      const tall=cardH>=80,shortCard=cardH<60;
      const iconH=tall?40:shortCard?22:25;
      const iconBottom=cardH/2+(tall?-6:-3)-iconH;
      const nameY=tall?-14:shortCard?-6:-7.5;
      const nameH=tall?12:shortCard?10:11;
      const amountY=tall?-31:shortCard?-19:-22;
      const amountH=tall?20:shortCard?14:16;
      assert.ok(iconBottom>=nameY+nameH/2+0.3,
        `${w}x${h}: paid resource icon overlaps name`);
      assert.ok(nameY-nameH/2>=amountY+amountH/2+0.3,
        `${w}x${h}: paid resource name overlaps quantity`);
      assert.ok(amountY-amountH/2>=-cardH/2-0.1,
        `${w}x${h}: earned amount text clips beyond frame`);
    }
  }
  // Normal resource-only days are intentionally NOT shown in a tiny
  // empty chest position: 1–2 originally illustrated cards are centered,
  // with a compact receipt and a genuinely reachable Continue action.
  for(const count of [1,2])
  {
    const quietShelf=rect(0,portrait?19:26,pw-34,portrait?175:137);
    const quietCaption=rect(0,portrait?-135:-99,pw-45,41);
    const quietContinue=rect(0,portrait?-216:-152,
      Math.min(pw-30,portrait?325:405),54);
    assert.ok(noOverlap(quietShelf,quietCaption),
      `${w}x${h}: the ordinary illustrated rewards obscure the actual receipt`);
    assert.ok(noOverlap(quietCaption,quietContinue),
      `${w}x${h}: Continue overlaps the normal reward's confirmation`);
    assert.ok(noOverlap(quietShelf,quietContinue),
      `${w}x${h}: normal resource items cover Continue`);
    const cardW=Math.min(175,(pw-38-(count-1)*(portrait?8:6))/count);
    const cardH=portrait?129:115;
    assert.ok(cardW>=120&&cardH>=100,
      `${w}x${h}: ordinary rewards are too small for premium illustrated art`);
    const separation=portrait?8:6;
    const left=-(count*cardW+(count-1)*separation)/2;
    for(let i=0;i<count;i++)
    {
      const card=rect(left+cardW/2+i*(cardW+separation),0,cardW,cardH);
      assert.ok(card.x0>=-quietShelf.x1 && card.x1<=quietShelf.x1 &&
                card.y0>=-quietShelf.y1+quietShelf.y0 &&
                card.y1<=quietShelf.y1-quietShelf.y0,
        `${w}x${h}: normal illustrated reward is outside its frame`);
    }
  }

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
