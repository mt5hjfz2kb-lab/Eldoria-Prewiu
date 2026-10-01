import { chromium } from 'playwright';
import fs from 'node:fs';

const requestPath = process.argv[2] || 'pipeline/tripo-advanced-probe-request.json';
const outPath = process.argv[3] || 'tripo-advanced-capability-probe.json';
const request = JSON.parse(fs.readFileSync(requestPath,'utf8'));
const endpoint = request.cdp_endpoint || 'http://127.0.0.1:9222';

const browser = await chromium.connectOverCDP(endpoint);
const pages = browser.contexts().flatMap(c => c.pages());
const page = pages.find(p => /studio\.tripo3d\.ai/i.test(p.url()));
if (!page) throw new Error('No open Tripo Studio page found on runner browser.');

await page.waitForLoadState('domcontentloaded',{timeout:8000}).catch(()=>{});
const snapshot = await page.evaluate(() => {
  const norm = s => String(s||'').replace(/\s+/g,' ').trim();
  const visible = el => {
    const s=getComputedStyle(el), r=el.getBoundingClientRect();
    return s.display!=='none' && s.visibility!=='hidden' && r.width>0 && r.height>0;
  };
  const buttons = Array.from(document.querySelectorAll('button,[role="button"]'))
    .filter(visible).map(el => norm(el.innerText || el.getAttribute('aria-label') || el.title)).filter(Boolean);
  const links = Array.from(document.querySelectorAll('a'))
    .filter(visible).map(el => ({text:norm(el.innerText || el.getAttribute('aria-label') || el.title), href:el.href})).filter(x=>x.text||x.href);
  const text = norm(document.body?.innerText || '');
  return {url:location.href,title:document.title,buttons,links,body_text:text.slice(0,120000)};
});

const families = {
  parts: /(generate in parts|parts?|segment|segmentation|separate|split)/i,
  part_completion: /(part completion|complete part|completion)/i,
  retopology: /(retopo|retopology|smart mesh|quad|topology|polycount|polygon)/i,
  textures: /(texture|pbr|magic brush|material)/i,
  rigging: /(rig|rigging|skeleton|animate|animation)/i,
  export: /(export|download|glb|fbx|obj)/i
};
const haystack = [snapshot.body_text,...snapshot.buttons,...snapshot.links.map(x=>x.text)].join('\n');
const detected = {};
for (const [name,re] of Object.entries(families)) {
  detected[name] = {
    visible_text_match: re.test(haystack),
    matching_buttons: snapshot.buttons.filter(x=>re.test(x)).slice(0,30),
    matching_links: snapshot.links.filter(x=>re.test(x.text)).slice(0,20)
  };
}

const result = {
  schema_version:1,
  mode:'read_only_ui_probe',
  credits_spent:0,
  clicks_performed:0,
  page:{url:snapshot.url,title:snapshot.title},
  detected,
  button_count:snapshot.buttons.length,
  visible_buttons:snapshot.buttons,
  note:'Presence in UI does not certify Eldoria automation. This probe performs no generation/transformation and no credit-spending action.'
};
fs.writeFileSync(outPath,JSON.stringify(result,null,2)+'\n');
console.log(JSON.stringify(result,null,2));
await browser.close();
