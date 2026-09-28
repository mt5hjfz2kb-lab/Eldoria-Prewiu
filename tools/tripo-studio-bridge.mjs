import { chromium } from 'playwright';
import fs from 'node:fs';
import path from 'node:path';

const requestPath = process.argv[2] || 'pipeline/tripo-studio-request.json';
const outPath = process.argv[3] || 'tripo-studio-probe.json';

const request = JSON.parse(fs.readFileSync(requestPath, 'utf8'));
const endpoint = request.cdp_endpoint || 'http://127.0.0.1:9222';
const connectTimeoutMs = Number(request.connect_timeout_ms || 10000);
const pageProbeTimeoutMs = Number(request.page_probe_timeout_ms || 12000);
const mode = request.mode || 'probe';

function withTimeout(promise, ms, label) {
  return Promise.race([
    promise,
    new Promise((_, reject) => setTimeout(() => reject(new Error(`${label} timed out after ${ms} ms`)), ms))
  ]);
}

async function inspectPage(page) {
  let title = '';
  let buttons = [];
  let generateVisible = false;
  let lastError = null;

  for (let attempt = 1; attempt <= 3; attempt++) {
    try {
      await page.waitForLoadState('domcontentloaded', { timeout: Math.min(pageProbeTimeoutMs, 8000) }).catch(() => {});
      title = await withTimeout(page.title(), pageProbeTimeoutMs, 'page title read');
      buttons = await withTimeout(page.locator('button').allTextContents(), pageProbeTimeoutMs, 'button inventory');
      buttons = buttons.map(x => x.trim()).filter(Boolean);
      generateVisible = buttons.some(x => /^Generar\b/i.test(x));
      lastError = null;
      break;
    } catch (error) {
      lastError = error;
      await new Promise(resolve => setTimeout(resolve, 1200));
    }
  }
  if (lastError) throw lastError;

  const controls = await page.evaluate(() => {
    const inputs = Array.from(document.querySelectorAll('input')).map((el, i) => ({
      index: i,
      type: el.getAttribute('type') || '',
      accept: el.getAttribute('accept') || '',
      name: el.getAttribute('name') || '',
      placeholder: el.getAttribute('placeholder') || '',
      ariaLabel: el.getAttribute('aria-label') || '',
      hidden: !!(el.hidden || getComputedStyle(el).display === 'none' || getComputedStyle(el).visibility === 'hidden')
    }));
    const textareas = Array.from(document.querySelectorAll('textarea')).map((el, i) => ({
      index: i,
      placeholder: el.getAttribute('placeholder') || '',
      ariaLabel: el.getAttribute('aria-label') || ''
    }));
    return { inputs, textareas };
  });

  return {
    url: page.url(),
    title,
    generate_button_visible: generateVisible,
    visible_button_sample: buttons.slice(0, 40),
    controls
  };
}

let browser;
try {
  if (request.allow_credit_spend === true && mode !== 'generate') {
    throw new Error('Credit-spend flag is forbidden outside an explicit generate mode.');
  }

  browser = await withTimeout(chromium.connectOverCDP(endpoint), connectTimeoutMs, 'CDP connection');

  const contexts = browser.contexts();
  if (!contexts.length) throw new Error('No browser context available on the Edge CDP endpoint.');

  const pages = contexts.flatMap(c => c.pages());
  const tripoPages = pages.filter(p => p.url().includes('studio.tripo3d.ai'));
  if (!tripoPages.length) throw new Error('No open Tripo Studio tab was found in the attached Edge session.');

  const inspected = [];
  let selectedPage = null;
  let selectedInfo = null;

  for (const page of tripoPages) {
    try {
      const item = await inspectPage(page);
      inspected.push(item);
      if (!selectedPage && item.generate_button_visible) {
        selectedPage = page;
        selectedInfo = item;
      }
    } catch (error) {
      inspected.push({
        url: page.url(),
        title: '',
        generate_button_visible: false,
        error: String(error?.message || error)
      });
    }
  }

  if (!selectedPage) {
    selectedPage = tripoPages[0];
    selectedInfo = inspected[0];
  }

  const report = {
    ok: true,
    mode,
    endpoint,
    tripo_page_count: tripoPages.length,
    selected_page: selectedInfo,
    generate_button_visible_anywhere: inspected.some(x => x.generate_button_visible),
    inspected_pages: inspected
  };

  if (mode === 'stage_upload') {
    if (request.allow_credit_spend === true) {
      throw new Error('stage_upload refuses any request that allows credit spend.');
    }

    const uploadPath = path.resolve(String(request.upload_path || ''));
    if (!uploadPath || !fs.existsSync(uploadPath)) {
      throw new Error(`Upload source not found: ${uploadPath}`);
    }

    let uploadPage = null;
    let imageInput = null;

    for (let attempt = 1; attempt <= 8; attempt++) {
      const livePages = browser.contexts().flatMap(c => c.pages()).filter(p => p.url().includes('studio.tripo3d.ai'));
      for (const page of livePages) {
        await page.waitForLoadState('domcontentloaded', { timeout: 5000 }).catch(() => {});
        const candidate = page.locator('input[type="file"][accept*="image"]').first();
        if (await candidate.count()) {
          uploadPage = page;
          imageInput = candidate;
          break;
        }
      }
      if (imageInput) break;

      if (attempt === 4 && livePages.length) {
        await livePages[0].reload({ waitUntil: 'domcontentloaded', timeout: 15000 }).catch(() => {});
      }

      await new Promise(resolve => setTimeout(resolve, 1600));
    }

    if (!uploadPage || !imageInput) {
      throw new Error('Could not identify the Tripo Studio image-upload input at staging time.');
    }

    const beforeUrl = uploadPage.url();
    await withTimeout(imageInput.setInputFiles(uploadPath), pageProbeTimeoutMs, 'image upload staging');
    await new Promise(resolve => setTimeout(resolve, 1800));

    report.upload = {
      source_path: uploadPath,
      source_bytes: fs.statSync(uploadPath).size,
      file_name: path.basename(uploadPath),
      set_input_files_succeeded: true,
      page_url_before: beforeUrl,
      page_url_after: uploadPage.url(),
      generate_clicked: false,
      credits_spent: false
    };

    // Do not click Generate. If the original input survives, clear it; otherwise
    // the SPA has already replaced the control after accepting the file.
    const clearCandidate = uploadPage.locator('input[type="file"][accept*="image"]').first();
    if (await clearCandidate.count()) {
      try {
        await withTimeout(clearCandidate.setInputFiles([]), 5000, 'image upload clear');
        report.upload.cleared = true;
      } catch {
        report.upload.cleared = false;
      }
    } else {
      report.upload.cleared = false;
      report.upload.control_replaced_after_upload = true;
    }
  } else if (mode !== 'probe') {
    throw new Error(`Unsupported safe bridge mode: ${mode}`);
  }

  fs.writeFileSync(outPath, JSON.stringify(report, null, 2));
  console.log('TRIPO_STUDIO_BRIDGE_OK');
  console.log(JSON.stringify(report));

  if (report.generate_button_visible_anywhere) console.log('TRIPO_STUDIO_GENERATE_VISIBLE');
  else console.log('TRIPO_STUDIO_GENERATE_NOT_VISIBLE');

  if (mode === 'stage_upload') console.log('TRIPO_STUDIO_UPLOAD_STAGED_AND_CLEARED');

  // Never close the owner's real Edge session.
  process.exit(0);
} catch (error) {
  const report = {
    ok: false,
    mode,
    endpoint,
    error: String(error?.message || error),
    generate_clicked: false,
    credits_spent: false
  };
  try { fs.writeFileSync(outPath, JSON.stringify(report, null, 2)); } catch {}
  console.error('TRIPO_STUDIO_BRIDGE_FAIL');
  console.error(report.error);
  process.exit(1);
}
