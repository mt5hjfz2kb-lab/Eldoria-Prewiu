import { chromium } from 'playwright';
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

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

  const bodyText = await withTimeout(page.locator('body').innerText(), pageProbeTimeoutMs, 'page text read').catch(() => '');
  const creditCostText = String(bodyText || '')
    .split(/\r?\n/)
    .map(x => x.trim())
    .filter(Boolean)
    .filter(x => /(credit|credits|cr[eé]dit|cr[eé]ditos|cost|coste|generate)/i.test(x))
    .slice(0, 60);

  return {
    url: page.url(),
    title,
    generate_button_visible: generateVisible,
    visible_button_sample: buttons.slice(0, 40),
    credit_cost_text_sample: creditCostText,
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

  if (mode === 'stage_upload' || mode === 'generate') {
    if (mode === 'stage_upload' && request.allow_credit_spend === true) {
      throw new Error('stage_upload refuses any request that allows credit spend.');
    }

    const uploadPath = path.resolve(String(request.upload_path || ''));
    if (!uploadPath || !fs.existsSync(uploadPath)) {
      throw new Error(`Upload source not found: ${uploadPath}`);
    }
    const sourceSha = crypto.createHash('sha256').update(fs.readFileSync(uploadPath)).digest('hex');
    if (sourceSha !== request.upload_sha256 || fs.statSync(uploadPath).size !== Number(request.upload_size_bytes)) {
      throw new Error('Tripo upload identity does not match the approved SHA/size.');
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
    await uploadPage.getByText('Subiendo...', { exact: false }).waitFor({ state: 'hidden', timeout: 60000 });
    await new Promise(resolve => setTimeout(resolve, 1500));

    report.upload = {
      source_path: uploadPath,
      source_sha256: sourceSha,
      source_bytes: fs.statSync(uploadPath).size,
      file_name: path.basename(uploadPath),
      set_input_files_succeeded: true,
      page_url_before: beforeUrl,
      page_url_after: uploadPage.url(),
      generate_clicked: false,
      credits_spent: false
    };
    report.post_upload_page = await inspectPage(uploadPage);
    report.upload.visible_generate_button = report.post_upload_page.visible_button_sample.find(x => /^Generar\s+\d+$/i.test(x)) || null;
    report.upload.visible_images = await uploadPage.locator('img').evaluateAll(images => images.map(img => ({
      alt: img.alt,
      width: img.naturalWidth,
      height: img.naturalHeight,
      src_kind: img.currentSrc.startsWith('blob:') ? 'blob' : img.currentSrc.startsWith('data:') ? 'data' : 'url'
    })).filter(img => img.width && img.height));
    const screenshotPath = path.join(path.dirname(outPath), 'tripo-studio-after-upload.png');
    await uploadPage.screenshot({ path: screenshotPath, fullPage: false });
    report.upload.screenshot_path = screenshotPath;
    report.upload.left_staged_for_owner_approval = mode === 'stage_upload';
    if (mode === 'generate') {
      const approvedCost = Number(request.authorized_credit_cost);
      if (request.allow_credit_spend !== true || approvedCost !== 55 ||
          request.approved_input_sha256 !== sourceSha) {
        throw new Error('The generation approval does not match this image and 55-credit cost.');
      }
      const button = uploadPage.getByRole('button', { name: /^Generar\s+55$/i });
      if (await button.count() !== 1 || !(await button.isEnabled())) {
        throw new Error('The approved 55-credit Generate button is not uniquely available.');
      }
      const guard = path.join(process.env.USERPROFILE || path.dirname(uploadPath), 'Downloads', '.Valoria_Aserradero_AP2_v1_generate_attempt.json');
      if (fs.existsSync(guard)) throw new Error(`Generation attempt already recorded: ${guard}`);
      fs.writeFileSync(guard, JSON.stringify({ sourceSha, approvedCost, requestId: request.request_id, begun: new Date().toISOString() }));
      report.generation = { guard, approved_cost: approvedCost, click_attempted: true };
      fs.writeFileSync(outPath, JSON.stringify(report, null, 2));
      await button.click();
      report.upload.generate_clicked = true;
      report.upload.credits_spent = null;
      report.generation.clicked_at = new Date().toISOString();
      await new Promise(resolve => setTimeout(resolve, 8000));
      report.generation.post_click_page = await inspectPage(uploadPage);
      report.generation.screenshot_path = path.join(path.dirname(outPath), 'tripo-studio-after-generate.png');
      await uploadPage.screenshot({ path: report.generation.screenshot_path, fullPage: false });
      fs.writeFileSync(guard, JSON.stringify({ sourceSha, approvedCost, requestId: request.request_id, clickedAt: report.generation.clicked_at }));
    }
  } else if (mode === 'watch') {
    const expectedTaskUrl = String(request.generated_task_url || '');
    if (!expectedTaskUrl || !selectedPage.url().startsWith(expectedTaskUrl)) {
      throw new Error(`Expected generated task page is not active: ${expectedTaskUrl}`);
    }
    const deadline = Date.now() + 11 * 60 * 1000;
    let bodyText = '';
    do {
      bodyText = await selectedPage.locator('body').innerText();
      if (!bodyText.includes('Generando...')) break;
      await new Promise(resolve => setTimeout(resolve, 10000));
    } while (Date.now() < deadline);
    report.watch = {
      generation_still_running: bodyText.includes('Generando...'),
      page: await inspectPage(selectedPage),
      page_text: bodyText.slice(0, 8000),
      screenshot_path: path.join(path.dirname(outPath), 'tripo-studio-after-watch.png')
    };
    await selectedPage.screenshot({ path: report.watch.screenshot_path, fullPage: false });
  } else if (mode === 'probe') {
    report.probe_screenshot_path = path.join(path.dirname(outPath), 'tripo-studio-probe.png');
    await selectedPage.screenshot({ path: report.probe_screenshot_path, fullPage: false });
    report.probe_page_text = (await selectedPage.locator('body').innerText()).slice(0, 7000);
  } else {
    throw new Error(`Unsupported safe bridge mode: ${mode}`);
  }

  fs.writeFileSync(outPath, JSON.stringify(report, null, 2));
  console.log('TRIPO_STUDIO_BRIDGE_OK');
  console.log(JSON.stringify(report));

  if (report.generate_button_visible_anywhere) console.log('TRIPO_STUDIO_GENERATE_VISIBLE');
  else console.log('TRIPO_STUDIO_GENERATE_NOT_VISIBLE');

  if (mode === 'stage_upload') {
    console.log('TRIPO_STUDIO_UPLOAD_STAGED_NO_GENERATE');
  } else if (mode === 'generate') {
    console.log('TRIPO_STUDIO_GENERATE_CLICKED_ONCE');
  }

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
