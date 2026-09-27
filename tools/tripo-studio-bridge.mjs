import { chromium } from 'playwright';
import fs from 'node:fs';

const requestPath = process.argv[2] || 'pipeline/tripo-studio-request.json';
const outPath = process.argv[3] || 'tripo-studio-probe.json';

const request = JSON.parse(fs.readFileSync(requestPath, 'utf8'));
const endpoint = request.cdp_endpoint || 'http://127.0.0.1:9222';

const browser = await chromium.connectOverCDP(endpoint);
const contexts = browser.contexts();
if (!contexts.length) throw new Error('No browser context available on the Edge CDP endpoint.');

const pages = contexts.flatMap(c => c.pages());
const page = pages.find(p => p.url().includes('studio.tripo3d.ai'));
if (!page) throw new Error('No open Tripo Studio tab was found in the attached Edge session.');

const title = await page.title();
const buttons = (await page.locator('button').allTextContents()).map(x => x.trim()).filter(Boolean);
const generateVisible = buttons.some(x => /^Generar$/i.test(x));

const report = {
  ok: true,
  endpoint,
  url: page.url(),
  title,
  generate_button_visible: generateVisible,
  visible_button_sample: buttons.slice(0, 40)
};

fs.writeFileSync(outPath, JSON.stringify(report, null, 2));

if (!generateVisible) {
  throw new Error('Connected to Tripo Studio, but the Generate button is not currently visible.');
}

console.log('TRIPO_STUDIO_BRIDGE_OK');
console.log(JSON.stringify(report));
