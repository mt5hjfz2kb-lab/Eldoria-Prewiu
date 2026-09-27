import { chromium } from 'playwright';
import fs from 'node:fs';

const requestPath = process.argv[2] || 'pipeline/tripo-studio-request.json';
const outPath = process.argv[3] || 'tripo-studio-probe.json';

const request = JSON.parse(fs.readFileSync(requestPath, 'utf8'));
const endpoint = request.cdp_endpoint || 'http://127.0.0.1:9222';
const connectTimeoutMs = Number(request.connect_timeout_ms || 10000);
const pageProbeTimeoutMs = Number(request.page_probe_timeout_ms || 12000);

function withTimeout(promise, ms, label) {
  return Promise.race([
    promise,
    new Promise((_, reject) => setTimeout(() => reject(new Error(`${label} timed out after ${ms} ms`)), ms))
  ]);
}

let browser;
try {
  browser = await withTimeout(
    chromium.connectOverCDP(endpoint),
    connectTimeoutMs,
    'CDP connection'
  );

  const contexts = browser.contexts();
  if (!contexts.length) throw new Error('No browser context available on the Edge CDP endpoint.');

  const pages = contexts.flatMap(c => c.pages());
  const tripoPages = pages.filter(p => p.url().includes('studio.tripo3d.ai'));
  if (!tripoPages.length) {
    throw new Error('No open Tripo Studio tab was found in the attached Edge session.');
  }

  const inspected = [];
  let selected = null;

  for (const page of tripoPages) {
    let title = '';
    let buttons = [];
    let generateVisible = false;

    try {
      title = await withTimeout(page.title(), pageProbeTimeoutMs, 'page title read');
      buttons = await withTimeout(
        page.locator('button').allTextContents(),
        pageProbeTimeoutMs,
        'button inventory'
      );
      buttons = buttons.map(x => x.trim()).filter(Boolean);
      generateVisible = buttons.some(x => /^Generar$/i.test(x));
    } catch (error) {
      inspected.push({
        url: page.url(),
        title,
        generate_button_visible: false,
        error: String(error?.message || error)
      });
      continue;
    }

    const item = {
      url: page.url(),
      title,
      generate_button_visible: generateVisible,
      visible_button_sample: buttons.slice(0, 40)
    };
    inspected.push(item);

    if (!selected && generateVisible) {
      selected = item;
    }
  }

  if (!selected) {
    selected = inspected[0];
  }

  const report = {
    ok: true,
    endpoint,
    tripo_page_count: tripoPages.length,
    selected_page: selected,
    generate_button_visible_anywhere: inspected.some(x => x.generate_button_visible),
    inspected_pages: inspected
  };

  fs.writeFileSync(outPath, JSON.stringify(report, null, 2));

  console.log('TRIPO_STUDIO_BRIDGE_OK');
  console.log(JSON.stringify(report));

  if (report.generate_button_visible_anywhere) {
    console.log('TRIPO_STUDIO_GENERATE_VISIBLE');
  } else {
    console.log('TRIPO_STUDIO_GENERATE_NOT_VISIBLE');
  }

  // We intentionally do not call browser.close(): that would close the owner's
  // real Edge session. Exit explicitly so the runner does not stay attached.
  process.exit(0);
} catch (error) {
  const report = {
    ok: false,
    endpoint,
    error: String(error?.message || error)
  };
  try {
    fs.writeFileSync(outPath, JSON.stringify(report, null, 2));
  } catch {}
  console.error('TRIPO_STUDIO_BRIDGE_FAIL');
  console.error(report.error);
  process.exit(1);
}
