// Mathematical smoke-test of CanvasScaler=.5 layout (not a Unity PlayMode test).
import {readFileSync} from "node:fs";
import assert from "node:assert/strict";

const source=readFileSync("Unity/Assets/Eldoria/Branding/EonscarTrial/Runtime/EonscarBrandingTrialOverlay.cs","utf8");
for (const snippet of [
  "Rect safeArea = Screen.safeArea;",
  "new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height)",
  "new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height)",
  "float scale = Mathf.Sqrt((Screen.width / 1920f) * (Screen.height / 1080f));",
  "float virtualWidth = safeArea.width / scale;",
  "float virtualHeight = safeArea.height / scale;",
  "Mathf.Max(portrait ? 128f : 112f, 48f / scale)"
]) assert(source.includes(snippet),"Source diverges from accepted responsive formula: "+snippet);

const cases=[
  [1920,1080,0,0,1920,1080],
  [1280,720,0,0,1280,720],
  [844,390,0,0,844,390],
  [844,390,0,21,844,369],
  [844,390,44,34,756,356],
  [390,844,0,34,390,763],
  [360,640,0,0,360,640],
  [1024,768,0,0,1024,768],
  [2560,1440,0,0,2560,1440]
];
const ratio=512/384;
for(const [w,h,x,y,sw,sh] of cases){
  assert(sw>0&&sh>0,"Invalid safe bounds");
  assert(x>=0&&y>=0&&x+sw<=w&&y+sh<=h,"Safe area lies outside viewport");
  const scale=Math.max(.001,Math.sqrt(w/1920*h/1080));
  const vw=sw/scale,vh=sh/scale, portrait=h>w;
  const maxW=Math.min(1120,vw*.84), maxH=Math.min(650,vh*(portrait?.37:.50));
  const logoH=Math.min(maxH,maxW/ratio), logoCenter=(portrait?.54:.64)*vh;
  const logoBottom=logoCenter-logoH/2, logoTop=logoCenter+logoH/2;
  const capCenter=(portrait?.285:.27)*vh,capTop=capCenter+32,capBottom=capCenter-32;
  const buttonH=Math.max(portrait?128:112,48/scale);
  const buttonCenter=(portrait?.15:.11)*vh;
  const buttonTop=buttonCenter+buttonH/2,buttonBottom=buttonCenter-buttonH/2;
  assert(logoTop<=vh&&logoBottom>=0,"Logo clips: "+w+"x"+h);
  assert(logoBottom-capTop>=24,"Logo intersects chapter caption: "+w+"x"+h);
  assert(capBottom-buttonTop>=24,"Caption intersects CTA: "+w+"x"+h);
  assert(buttonBottom>=0&&buttonTop<=vh,"CTA clips: "+w+"x"+h);
  assert(buttonH*scale>=47.9,"CTA is too small to tap: "+w+"x"+h);
  console.log("EONSCAR_LAYOUT_OK "+w+"x"+h+" safe "+sw+"x"+sh+" logoBottom="+Math.round(logoBottom)+" tapPx="+Math.round(buttonH*scale));
}
console.log("EONSCAR_LAYOUT_MATH_SMOKE_PASS "+cases.length+" cases; UNITY_RENDER_NOT_RUN");