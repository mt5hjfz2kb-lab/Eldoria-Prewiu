import { chromium } from 'playwright';
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { spawn } from 'node:child_process';

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


const sleep = ms => new Promise(resolve => setTimeout(resolve, ms));
const normalizeUiText = value => String(value ?? '').replace(/\s+/g, ' ').trim();

function parseGenerateButton(buttons) {
  for (const raw of buttons || []) {
    const text = normalizeUiText(raw);
    const match = text.match(/^(?:Generar|Generate)\s+(\d+)$/i);
    if (match) return { text, cost: Number(match[1]) };
  }
  return null;
}

async function imageSnapshot(page) {
  return page.locator('img').evaluateAll(images => images.map((img, index) => {
    const style = getComputedStyle(img);
    const rect = img.getBoundingClientRect();
    const src = img.currentSrc || img.src || '';
    return {
      index,
      alt: img.alt || '',
      width: img.naturalWidth || 0,
      height: img.naturalHeight || 0,
      visible: style.display !== 'none' && style.visibility !== 'hidden' && rect.width > 0 && rect.height > 0,
      src_kind: src.startsWith('blob:') ? 'blob' : src.startsWith('data:') ? 'data' : src ? 'url' : 'none'
    };
  }).filter(img => img.width && img.height));
}

async function inlineImageHashes(page, expectedWidth, expectedHeight) {
  if (!(Number(expectedWidth) > 0 && Number(expectedHeight) > 0)) return [];
  return page.evaluate(async ({ width, height }) => {
    const out = [];
    const toHex = bytes => Array.from(bytes).map(b => b.toString(16).padStart(2, '0')).join('');
    for (const [index, img] of Array.from(document.querySelectorAll('img')).entries()) {
      if (img.naturalWidth !== width || img.naturalHeight !== height) continue;
      const src = img.currentSrc || img.src || '';
      if (!(src.startsWith('blob:') || src.startsWith('data:'))) continue;
      try {
        const response = await fetch(src);
        const buffer = await response.arrayBuffer();
        const digest = await crypto.subtle.digest('SHA-256', buffer);
        out.push({
          index,
          width: img.naturalWidth,
          height: img.naturalHeight,
          src_kind: src.startsWith('blob:') ? 'blob' : 'data',
          bytes: buffer.byteLength,
          sha256: toHex(new Uint8Array(digest))
        });
      } catch (error) {
        out.push({
          index,
          width: img.naturalWidth,
          height: img.naturalHeight,
          src_kind: src.startsWith('blob:') ? 'blob' : 'data',
          error: String(error?.message || error)
        });
      }
    }
    return out;
  }, { width: Number(expectedWidth), height: Number(expectedHeight) });
}

async function waitForStableUpload(page, beforeImages, expectedWidth, expectedHeight, timeoutMs = 60000) {
  const deadline = Date.now() + timeoutMs;
  let last = null;
  while (Date.now() < deadline) {
    const buttons = (await page.locator('button').allTextContents()).map(normalizeUiText).filter(Boolean);
    const generate = parseGenerateButton(buttons);
    const bodyText = await page.locator('body').innerText().catch(() => '');
    const uploading = /(?:Subiendo|Uploading)\.?/i.test(bodyText);
    const generating = /(?:Generando|Generating)\.?/i.test(bodyText);
    const images = await imageSnapshot(page);

    const beforeCounts = new Map();
    for (const img of beforeImages || []) {
      const key = img.width + 'x' + img.height + ':' + img.src_kind;
      beforeCounts.set(key, (beforeCounts.get(key) || 0) + 1);
    }
    const seen = new Map();
    const newImages = [];
    for (const img of images) {
      const key = img.width + 'x' + img.height + ':' + img.src_kind;
      const n = (seen.get(key) || 0) + 1;
      seen.set(key, n);
      if (n > (beforeCounts.get(key) || 0)) newImages.push(img);
    }

    const expectedDim = Number(expectedWidth) > 0 && Number(expectedHeight) > 0;
    const exactDimensionImages = expectedDim
      ? images.filter(img => img.width === Number(expectedWidth) && img.height === Number(expectedHeight))
      : [];
    const reflected = expectedDim
      ? exactDimensionImages.some(img => img.src_kind === 'blob' || img.src_kind === 'data') ||
        newImages.some(img => img.width === Number(expectedWidth) && img.height === Number(expectedHeight))
      : newImages.some(img => img.visible && (img.src_kind === 'blob' || img.src_kind === 'data'));

    last = {
      uploading,
      generating,
      generate,
      images,
      new_images: newImages,
      exact_dimension_images: exactDimensionImages,
      upload_reflected_in_ui: reflected,
      upload_stable: !uploading && !generating && !!generate && reflected
    };
    if (last.upload_stable) return last;
    await sleep(1200);
  }
  return last || {
    uploading: null,
    generating: null,
    generate: null,
    images: [],
    new_images: [],
    exact_dimension_images: [],
    upload_reflected_in_ui: false,
    upload_stable: false
  };
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
      buttons = buttons.map(normalizeUiText).filter(Boolean);
      generateVisible = buttons.some(x => /^(?:Generar|Generate)\b/i.test(x));
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

  const generateButton = parseGenerateButton(buttons);
  const dialogCount = await page.locator('[role="dialog"]').count().catch(() => 0);
  return {
    url: page.url(),
    title,
    generate_button_visible: generateVisible,
    visible_generate_button: generateButton,
    visible_button_sample: buttons.slice(0, 40),
    credit_cost_text_sample: creditCostText,
    generating_visible: /(?:Generando|Generating)\.?/i.test(bodyText),
    upload_in_progress_visible: /(?:Subiendo|Uploading)\.?/i.test(bodyText),
    dialog_count: dialogCount,
    controls
  };
}


let browserRecovery = { attempted: false };
async function connectOwnerBrowser() {
  try {
    return await withTimeout(chromium.connectOverCDP(endpoint), connectTimeoutMs, 'CDP connection');
  } catch (initialError) {
    const recovery = request.browser_recovery;
    if (!recovery?.enabled || !/ECONNREFUSED/.test(String(initialError?.message || initialError))) throw initialError;
    if (process.platform !== 'win32') throw new Error('Owner browser recovery requires the Windows runner.');
    const target = new URL(endpoint);
    if (target.protocol !== 'http:' || target.hostname !== '127.0.0.1' || target.port !== '9222') {
      throw new Error('Owner browser recovery is restricted to http://127.0.0.1:9222.');
    }
    const executable = String(recovery.executable_path || '');
    const profile = String(recovery.user_data_dir || '');
    if (!path.isAbsolute(executable) || path.basename(executable).toLowerCase() !== 'msedge.exe' || !fs.existsSync(executable)) {
      throw new Error('Configured owner Edge executable is missing.');
    }
    if (!path.isAbsolute(profile) || path.basename(profile) !== 'Eldoria-Edge-Remote' ||
        !fs.existsSync(path.join(profile, 'Local State')) || !fs.existsSync(path.join(profile, 'Default', 'Preferences'))) {
      throw new Error('Existing dedicated Eldoria Edge profile is missing; refusing to create or substitute a profile.');
    }
    browserRecovery = { attempted: true, executable_path: executable, user_data_dir: profile, ready: false };
    const env = { ...process.env };
    // This is the owner's persistent browser, not an Actions child to terminate at job cleanup.
    delete env.RUNNER_TRACKING_ID;
    const child = spawn(executable, ['--remote-debugging-port=9222', '--user-data-dir=' + profile, 'https://studio.tripo3d.ai'],
      { detached: true, stdio: 'ignore', env });
    await new Promise((resolve, reject) => { child.once('spawn', resolve); child.once('error', reject); });
    child.unref();
    const deadline = Date.now() + Math.min(60000, Math.max(5000, Number(recovery.start_timeout_ms || 30000)));
    let lastError = initialError;
    while (Date.now() < deadline) {
      await sleep(1000);
      try {
        const connected = await chromium.connectOverCDP(endpoint, { timeout: 3000 });
        browserRecovery.ready = true;
        return connected;
      } catch (error) { lastError = error; }
    }
    throw new Error('Existing owner Edge profile did not reopen its CDP endpoint: ' + String(lastError?.message || lastError));
  }
}

let browser;
try {
  if (request.allow_credit_spend === true && !['generate','generate_staged'].includes(mode)) {
    throw new Error('Credit-spend flag is forbidden outside an explicit generate mode.');
  }
  if (mode === 'stage_upload' && Number(request.authorized_credit_cost || 0) !== 0) {
    throw new Error('stage_upload requires authorized_credit_cost=0.');
  }

  browser = await connectOwnerBrowser();

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
    browser_recovery: browserRecovery,
    tripo_page_count: tripoPages.length,
    all_open_pages: await Promise.all(pages.map(async p => ({
      url: p.url(),
      title: await p.title().catch(() => '')
    }))),
    selected_page: selectedInfo,
    generate_button_visible_anywhere: inspected.some(x => x.generate_button_visible),
    inspected_pages: inspected
  };

  if (mode === 'probe' && request.probe_chatgpt_session === true) {
    const probePage = await contexts[0].newPage();
    try {
      await probePage.goto('https://chatgpt.com/', { waitUntil: 'domcontentloaded', timeout: 30000 });
      await new Promise(resolve => setTimeout(resolve, 2500));
      const body = await probePage.locator('body').innerText().catch(() => '');
      report.chatgpt_session_probe = {
        url: probePage.url(),
        title: await probePage.title().catch(() => ''),
        login_or_signup_visible: /log in|sign up|iniciar sesi[oó]n|registr/i.test(body),
        composer_visible: (await probePage.locator('textarea').count().catch(() => 0)) > 0 ||
          (await probePage.locator('[contenteditable="true"]').count().catch(() => 0)) > 0,
        body_sample: String(body).slice(0, 1200)
      };

      const chatgptFileId = String(request.probe_chatgpt_file_id || '').trim();
      if (chatgptFileId) {
        if (!/^file_[A-Za-z0-9_-]+$/.test(chatgptFileId)) throw new Error('Unsafe ChatGPT file id.');
        const expectedSha = String(request.upload_sha256 || '').toLowerCase();
        const expectedBytes = Number(request.upload_size_bytes || 0);
        const candidates = [
          `https://chatgpt.com/backend-api/files/${chatgptFileId}/download`,
          `https://chatgpt.com/backend-api/files/${chatgptFileId}/download?download=1`,
          `https://chatgpt.com/backend-api/files/${chatgptFileId}/content`,
          `https://chatgpt.com/backend-api/files/${chatgptFileId}`
        ];
        const attempts = [];
        for (const url of candidates) {
          try {
            const response = await contexts[0].request.get(url, { timeout: 30000, failOnStatusCode: false });
            const bytes = await response.body();
            const sha256 = crypto.createHash('sha256').update(bytes).digest('hex');
            attempts.push({ url, status: response.status(), content_type: response.headers()['content-type'] || '', bytes: bytes.length, sha256 });
            if (response.ok() && bytes.length === expectedBytes && sha256 === expectedSha) {
              const ext = String(request.upload_file_name || '').toLowerCase().endsWith('.jpeg') ? '.jpeg' : '.bin';
              const exactPath = path.join(path.dirname(outPath), 'chatgpt-exact-input' + ext);
              fs.writeFileSync(exactPath, bytes);
              report.chatgpt_exact_input_probe = {
                ok: true,
                file_id: chatgptFileId,
                path: exactPath,
                bytes: bytes.length,
                sha256,
                source_url: url
              };
              break;
            }
          } catch (error) {
            attempts.push({ url, error: String(error?.message || error) });
          }
        }
        if (!report.chatgpt_exact_input_probe) {
          report.chatgpt_exact_input_probe = { ok: false, file_id: chatgptFileId, attempts };
        } else {
          report.chatgpt_exact_input_probe.attempts = attempts;
        }
      }
    } finally {
      await probePage.close().catch(() => {});
    }
  }

  if (mode === 'stage_upload' || mode === 'generate') {
    if (mode === 'stage_upload' && request.allow_credit_spend === true) {
      throw new Error('stage_upload refuses any request that allows credit spend.');
    }

    let uploadPath = path.resolve(String(request.upload_path || ''));
    let renderedInput = null;
    if (request.render_svg_path) {
      const svgPath = path.resolve(String(request.render_svg_path));
      if (!fs.existsSync(svgPath)) throw new Error(`SVG source not found: ${svgPath}`);
      const svgBytes = fs.readFileSync(svgPath);
      const svgSha = crypto.createHash('sha256').update(svgBytes).digest('hex');
      if (request.render_svg_sha256 && svgSha !== String(request.render_svg_sha256).toLowerCase()) {
        throw new Error('SVG source identity does not match render_svg_sha256.');
      }
      const fileName = String(request.upload_file_name || 'tripo-rendered-input.png');
      if (!fileName.toLowerCase().endsWith('.png')) throw new Error('Rendered SVG input must use a .png upload_file_name.');
      const evidencePath = path.join(path.dirname(outPath), 'tripo-rendered-input.png');
      const downloadsPath = path.join(process.env.USERPROFILE || path.dirname(outPath), 'Downloads', fileName);
      const renderPage = await contexts[0].newPage();
      await renderPage.setViewportSize({ width: 1024, height: 1024 });
      await renderPage.setContent('<!doctype html><html><head><style>html,body{margin:0;width:100%;height:100%;overflow:hidden;background:#dedede}svg{display:block;width:1024px;height:1024px}</style></head><body>'+svgBytes.toString('utf8')+'</body></html>', { waitUntil: 'load' });
      await renderPage.screenshot({ path: evidencePath, type: 'png' });
      await renderPage.close();
      fs.copyFileSync(evidencePath, downloadsPath);
      uploadPath = downloadsPath;
      const renderedBytes = fs.readFileSync(uploadPath);
      renderedInput = {
        svg_path: svgPath,
        svg_sha256: svgSha,
        png_path: uploadPath,
        png_sha256: crypto.createHash('sha256').update(renderedBytes).digest('hex'),
        png_bytes: renderedBytes.length,
        evidence_path: evidencePath
      };
    }
    if (!uploadPath || !fs.existsSync(uploadPath)) {
      throw new Error(`Upload source not found: ${uploadPath}`);
    }
    const sourceSha = crypto.createHash('sha256').update(fs.readFileSync(uploadPath)).digest('hex');
    const sourceBytes = fs.statSync(uploadPath).size;
    const exactIdentityRequired = !(mode === 'stage_upload' && renderedInput && !request.upload_sha256);
    if (exactIdentityRequired && (sourceSha !== request.upload_sha256 || sourceBytes !== Number(request.upload_size_bytes))) {
      throw new Error('Tripo upload identity does not match the approved SHA/size.');
    }
    if (renderedInput) report.rendered_input = renderedInput;

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
    if (!/studio\.tripo3d\.ai\/(?:[a-z]{2}\/)?workspace\/generate/i.test(beforeUrl)) {
      throw new Error('Tripo preflight refused unexpected workspace URL: ' + beforeUrl);
    }
    const beforeText = await uploadPage.locator('body').innerText().catch(() => '');
    if (/(?:Generando|Generating)\.?/i.test(beforeText)) {
      throw new Error('Tripo preflight found a generation already in progress; staging is unsafe.');
    }
    const beforeImages = await imageSnapshot(uploadPage);
    const beforeScreenshotPath = path.join(path.dirname(outPath), 'tripo-studio-before-upload.png');
    await uploadPage.screenshot({ path: beforeScreenshotPath, fullPage: false });

    let setInputError = null;
    let setInputSucceeded = false;
    for (let attempt = 1; attempt <= 3; attempt++) {
      try {
        await withTimeout(imageInput.setInputFiles(uploadPath), Math.max(pageProbeTimeoutMs, 15000), 'image upload staging attempt ' + attempt);
        setInputSucceeded = true;
        setInputError = null;
        break;
      } catch (error) {
        setInputError = error;
        await sleep(1200);
        imageInput = uploadPage.locator('input[type="file"][accept*="image"]').first();
      }
    }
    if (!setInputSucceeded) {
      throw new Error('Image upload staging failed after 3 attempts: ' + String(setInputError?.message || setInputError));
    }

    const expectedWidth = Number(request.upload_width || request.visual_width || 0);
    const expectedHeight = Number(request.upload_height || request.visual_height || 0);
    const stagedState = await waitForStableUpload(uploadPage, beforeImages, expectedWidth, expectedHeight, 60000);
    const previewHashes = await inlineImageHashes(uploadPage, expectedWidth, expectedHeight);
    const previewHashMatch = previewHashes.some(item => item.sha256 === sourceSha && item.bytes === sourceBytes);

    report.upload = {
      source_path: uploadPath,
      source_sha256: sourceSha,
      source_bytes: fs.statSync(uploadPath).size,
      file_name: path.basename(uploadPath),
      set_input_files_succeeded: true,
      page_url_before: beforeUrl,
      page_url_after: uploadPage.url(),
      expected_width: expectedWidth || null,
      expected_height: expectedHeight || null,
      generate_clicked: false,
      credits_spent: false,
      before_screenshot_path: beforeScreenshotPath
    };
    report.post_upload_page = await inspectPage(uploadPage);
    report.upload.visible_generate_button = stagedState.generate?.text || null;
    report.upload.visible_credit_cost = stagedState.generate?.cost ?? null;
    report.upload.visible_images = stagedState.images;
    report.upload.new_images = stagedState.new_images;
    report.upload.exact_dimension_images = stagedState.exact_dimension_images;
    report.upload.preview_hashes = previewHashes;
    report.upload.preview_hash_matches_source = previewHashMatch;
    report.upload.upload_reflected_in_ui = stagedState.upload_reflected_in_ui;
    report.upload.upload_stable = stagedState.upload_stable;
    report.upload.no_generation_in_progress = stagedState.generating === false;
    report.upload.preflight = {
      workspace_url_valid: true,
      generation_in_progress_before_upload: false,
      image_input_found: true
    };
    const screenshotPath = path.join(path.dirname(outPath), 'tripo-studio-after-upload.png');
    await uploadPage.screenshot({ path: screenshotPath, fullPage: false });
    report.upload.screenshot_path = screenshotPath;
    report.upload.left_staged_for_owner_approval = mode === 'stage_upload';

    const verification = {
      asset_name: String(request.asset_name || ''),
      source_sha256: sourceSha,
      expected_sha256: String(request.upload_sha256 || '').toLowerCase(),
      source_bytes: sourceBytes,
      expected_bytes: Number(request.upload_size_bytes),
      exact_identity_verified: sourceSha === String(request.upload_sha256 || '').toLowerCase() &&
        sourceBytes === Number(request.upload_size_bytes),
      upload_reflected_in_ui: stagedState.upload_reflected_in_ui,
      preview_hashes: previewHashes,
      preview_hash_matches_source: previewHashMatch,
      upload_stable: stagedState.upload_stable,
      visible_generate_button: stagedState.generate?.text || null,
      visible_credit_cost: stagedState.generate?.cost ?? null,
      no_generation_in_progress: stagedState.generating === false,
      stage_upload_zero_spend_guard: mode !== 'stage_upload' || request.allow_credit_spend !== true,
      generate_clicked: false,
      credits_spent: false
    };
    fs.writeFileSync(path.join(path.dirname(outPath), 'tripo-upload-verification.json'), JSON.stringify(verification, null, 2));
    const flowState = {
      request_id: String(request.request_id || ''),
      asset_name: String(request.asset_name || ''),
      state: 'stage_upload_verified',
      source_sha256: sourceSha,
      source_bytes: sourceBytes,
      visible_credit_cost: verification.visible_credit_cost,
      preview_hash_matches_source: previewHashMatch,
      generate_clicked: false,
      credits_spent: false,
      verified_at: new Date().toISOString()
    };
    fs.writeFileSync(path.join(path.dirname(outPath), 'tripo-flow-state.json'), JSON.stringify(flowState, null, 2));

    if (!verification.exact_identity_verified || !verification.upload_reflected_in_ui || !verification.upload_stable ||
        !Number.isFinite(verification.visible_credit_cost) || verification.visible_credit_cost <= 0 ||
        !verification.no_generation_in_progress || !verification.stage_upload_zero_spend_guard) {
      throw new Error('Tripo staging verification failed: ' + JSON.stringify(verification));
    }
    if (mode === 'generate') {
      const approvedCost = Number(request.authorized_credit_cost);
      const assetName = String(request.asset_name || '').trim();
      if (request.allow_credit_spend !== true || !Number.isFinite(approvedCost) || approvedCost <= 0 ||
          request.approved_input_sha256 !== sourceSha || !assetName) {
        throw new Error('The generation approval does not match this exact image, visible credit cost and asset identity.');
      }
      const costPattern = new RegExp('^Generar\\s+' + approvedCost + '$', 'i');
      const button = uploadPage.getByRole('button', { name: costPattern });
      if (await button.count() !== 1 || !(await button.isEnabled())) {
        throw new Error(`The approved ${approvedCost}-credit Generate button is not uniquely available.`);
      }
      const safeAssetName = assetName.replace(/[^A-Za-z0-9_.-]/g, '_');
      const guard = path.join(process.env.USERPROFILE || path.dirname(uploadPath), 'Downloads', `.${safeAssetName}_generate_attempt.json`);
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
  } else if (mode === 'generate_staged') {
    const approvedCost = Number(request.authorized_credit_cost);
    const approvedSha = String(request.approved_input_sha256 || '').toLowerCase();
    const expectedSha = String(request.upload_sha256 || '').toLowerCase();
    const assetName = String(request.asset_name || '').trim();
    if (request.allow_credit_spend !== true || !Number.isFinite(approvedCost) || approvedCost <= 0 ||
        !assetName || !approvedSha || approvedSha !== expectedSha) {
      throw new Error('The staged generation approval does not match the exact approved input identity.');
    }
    if (!request.prior_stage_run_id || !request.prior_stage_artifact_id) {
      throw new Error('generate_staged requires prior staged run and artifact evidence.');
    }
    const livePreviewHashes = await inlineImageHashes(selectedPage, request.visual_width, request.visual_height);
    if (!livePreviewHashes.some(item => item.sha256 === approvedSha && item.bytes === Number(request.upload_size_bytes))) {
      throw new Error('Live staged image bytes do not match the authorized original; refusing Generate.');
    }
    const costPattern = new RegExp('^Generar\\s+' + approvedCost + '$', 'i');
    const button = selectedPage.getByRole('button', { name: costPattern });
    if (await button.count() !== 1 || !(await button.isEnabled())) {
      throw new Error(`The approved ${approvedCost}-credit Generate button is not uniquely available on the staged page.`);
    }
    const safeAssetName = assetName.replace(/[^A-Za-z0-9_.-]/g, '_');
    const guard = path.join(process.env.USERPROFILE || path.dirname(outPath), 'Downloads', `.${safeAssetName}_generate_attempt.json`);
    if (fs.existsSync(guard)) throw new Error(`Generation attempt already recorded: ${guard}`);
    report.staged_generation = {
      approved_input_sha256: approvedSha,
      approved_cost: approvedCost,
      prior_stage_run_id: request.prior_stage_run_id,
      prior_stage_artifact_id: request.prior_stage_artifact_id,
      no_restaging: true,
      click_attempted: true,
      guard
    };
    fs.writeFileSync(guard, JSON.stringify({ sourceSha: approvedSha, approvedCost, requestId: request.request_id, begun: new Date().toISOString(), noRestaging: true }));
    fs.writeFileSync(outPath, JSON.stringify(report, null, 2));
    await button.click();
    report.staged_generation.generate_clicked = true;
    report.staged_generation.clicked_at = new Date().toISOString();
    await new Promise(resolve => setTimeout(resolve, 8000));
    report.staged_generation.post_click_page = await inspectPage(selectedPage);
    report.staged_generation.task_url = selectedPage.url();
    report.staged_generation.screenshot_path = path.join(path.dirname(outPath), 'tripo-studio-after-generate.png');
    await selectedPage.screenshot({ path: report.staged_generation.screenshot_path, fullPage: false });
    fs.writeFileSync(guard, JSON.stringify({ sourceSha: approvedSha, approvedCost, requestId: request.request_id, clickedAt: report.staged_generation.clicked_at, taskUrl: report.staged_generation.task_url, noRestaging: true }));
  } else if (mode === 'export_glb') {
    const approvedTaskUrl = String(request.generated_task_url || '');
    const approvedTaskId = approvedTaskUrl.match(/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i)?.[0] || '';
    if (!approvedTaskId || !selectedPage.url().includes(approvedTaskId)) {
      throw new Error('The approved generated task is not active.');
    }
    let exportButtons = selectedPage.getByRole('button', { name: 'Exportar', exact: true });
    const dialogLabel = selectedPage.getByText('Nombre del archivo', { exact: true });
    if (!(await dialogLabel.isVisible())) {
      try {
        await exportButtons.first().waitFor({ state: 'visible', timeout: 15000 });
      } catch {
        const selectedAsset = selectedPage.locator('[role="tabpanel"] a.border-purple-1').first();
        if (await selectedAsset.count()) {
          await selectedAsset.click();
          await selectedPage.waitForTimeout(5000);
        }
        exportButtons = selectedPage.getByRole('button', { name: 'Exportar', exact: true });
        try {
          await exportButtons.first().waitFor({ state: 'visible', timeout: 20000 });
        } catch {
          await selectedPage.reload({ waitUntil: 'domcontentloaded', timeout: 60000 });
          await selectedPage.waitForTimeout(10000);
          if (!selectedPage.url().includes(approvedTaskId)) throw new Error('Reload left the approved generated task.');
          exportButtons = selectedPage.getByRole('button', { name: 'Exportar', exact: true });
          await exportButtons.first().waitFor({ state: 'visible', timeout: 60000 });
        }
      }
      await exportButtons.first().click();
    }
    await dialogLabel.waitFor({ state: 'visible', timeout: 12000 });
    exportButtons = selectedPage.getByRole('button', { name: 'Exportar', exact: true });
    if (await exportButtons.count() < 2) {
      throw new Error(`The GLB export dialog lacks its action button. Visible buttons: ${(await selectedPage.locator('button').allTextContents()).join(' | ')}`);
    }
    const nameInput = selectedPage.locator('input:not([type="file"])').last();
    const assetName = String(request.asset_name || '').trim();
    if (!assetName) throw new Error('asset_name is required for GLB export.');
    await nameInput.fill(assetName);
    const safeAssetName = assetName.replace(/[^A-Za-z0-9_.-]/g, '_');
    const downloadPath = path.join(process.env.USERPROFILE || '', 'Downloads', safeAssetName + '.glb');
    if (fs.existsSync(downloadPath)) throw new Error(`Existing export must be identified first: ${downloadPath}`);
    const [download] = await Promise.all([
      selectedPage.waitForEvent('download', { timeout: 300000 }),
      exportButtons.last().click()
    ]);
    if (!download.suggestedFilename().toLowerCase().endsWith('.glb')) {
      throw new Error(`Unexpected Tripo download: ${download.suggestedFilename()}`);
    }
    await download.saveAs(downloadPath);
    const bytes = fs.readFileSync(downloadPath);
    if (bytes.toString('ascii', 0, 4) !== 'glTF') throw new Error('Export does not have a GLB header.');
    report.export = {
      path: downloadPath,
      suggested_filename: download.suggestedFilename(),
      bytes: bytes.length,
      sha256: crypto.createHash('sha256').update(bytes).digest('hex'),
      task_url: selectedPage.url(),
      screenshot_path: path.join(path.dirname(outPath), 'tripo-studio-after-export.png')
    };
    await selectedPage.screenshot({ path: report.export.screenshot_path, fullPage: false });
  } else if (mode === 'export_probe') {
    if (!selectedPage.url().startsWith(String(request.generated_task_url || 'missing'))) {
      throw new Error('The approved generated task is not active.');
    }
    const exportButton = selectedPage.getByRole('button', { name: 'Exportar', exact: true });
    if (await exportButton.count() !== 1) throw new Error('Expected one Exportar button.');
    await exportButton.click();
    report.export_probe = {
      page: await inspectPage(selectedPage),
      page_text: (await selectedPage.locator('body').innerText()).slice(0, 8000),
      screenshot_path: path.join(path.dirname(outPath), 'tripo-studio-export-options.png')
    };
    await selectedPage.screenshot({ path: report.export_probe.screenshot_path, fullPage: false });
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
    report.probe_resources = await selectedPage.evaluate(() => performance.getEntriesByType('resource').map(e => e.name).filter(u => /a50f42b8|\\.glb(?:$|\\?)|tripo-data|api/i.test(u)).slice(-300));
    report.probe_controls = await selectedPage.locator('button, [role="button"], a').evaluateAll(nodes => nodes.slice(0, 250).map((el, index) => ({
      index,
      tag: el.tagName,
      text: (el.innerText || el.textContent || '').trim().slice(0, 160),
      ariaLabel: el.getAttribute('aria-label'),
      title: el.getAttribute('title'),
      dataTestId: el.getAttribute('data-testid'),
      className: typeof el.className === 'string' ? el.className.slice(0, 240) : '',
      disabled: !!el.disabled,
      visible: !!(el.offsetWidth || el.offsetHeight || el.getClientRects().length)
    })));
    report.probe_clickables = await selectedPage.locator('button, a, [role="button"], [tabindex]').evaluateAll(nodes =>
      nodes.map((el, index) => {
        const r = el.getBoundingClientRect();
        const s = getComputedStyle(el);
        return {
          index,
          tag: el.tagName,
          role: el.getAttribute('role') || '',
          text: (el.innerText || el.textContent || '').trim().replace(/\\s+/g, ' ').slice(0, 180),
          aria: el.getAttribute('aria-label') || '',
          title: el.getAttribute('title') || '',
          href: el.getAttribute('href') || '',
          html: el.outerHTML.slice(0, 900),
          className: String(el.className || '').slice(0, 240),
          visible: r.width > 0 && r.height > 0 && s.visibility !== 'hidden' && s.display !== 'none',
          x: Math.round(r.x), y: Math.round(r.y), width: Math.round(r.width), height: Math.round(r.height)
        };
      }).filter(x => x.visible).slice(0, 300)
    );
    report.probe_images = await selectedPage.locator('img').evaluateAll(nodes =>
      nodes.map((el, index) => {
        const r = el.getBoundingClientRect();
        const s = getComputedStyle(el);
        let p = el.parentElement;
        let depth = 0;
        while (p && depth < 5 && !p.matches('button, a, [role="button"], [tabindex]')) { p = p.parentElement; depth++; }
        return {
          index,
          alt: el.getAttribute('alt') || '',
          src: (el.getAttribute('src') || '').slice(0, 180),
          naturalWidth: el.naturalWidth || 0,
          naturalHeight: el.naturalHeight || 0,
          visible: r.width > 0 && r.height > 0 && s.visibility !== 'hidden' && s.display !== 'none',
          x: Math.round(r.x), y: Math.round(r.y), width: Math.round(r.width), height: Math.round(r.height),
          clickableAncestor: p ? {
            tag: p.tagName,
            role: p.getAttribute('role') || '',
            text: (p.innerText || p.textContent || '').trim().replace(/\\s+/g, ' ').slice(0, 180),
            aria: p.getAttribute('aria-label') || '',
            title: p.getAttribute('title') || '',
            className: String(p.className || '').slice(0, 240)
          } : null
        };
      }).filter(x => x.visible).slice(0, 200)
    );
    report.probe_network_resources = await selectedPage.evaluate(() => performance.getEntriesByType('resource')
      .map(e => String(e.name || ''))
      .filter(u => /a50f42b8|a91b5c26|glb|gltf|mesh|model|project|task|export|download|api/i.test(u))
      .slice(-500));
    report.probe_document_links = await selectedPage.locator('a').evaluateAll(nodes => nodes.map(a => ({
      text: (a.innerText || a.textContent || '').trim().replace(/\\s+/g, ' ').slice(0, 200),
      href: a.href || ''
    })).filter(x => /a50f42b8|a91b5c26|glb|gltf|mesh|model|project|task|export|download|api/i.test(x.href + ' ' + x.text)).slice(0, 300));
    const selectedAssetCard = selectedPage.locator('a.border-purple-1').first();
    if (await selectedAssetCard.count()) {
      await selectedAssetCard.hover();
      await selectedPage.waitForTimeout(400);
      const parent = selectedAssetCard.locator('xpath=..');
      report.probe_selected_asset = {
        card_html: (await selectedAssetCard.evaluate(el => el.outerHTML)).slice(0, 12000),
        parent_html: (await parent.evaluate(el => el.outerHTML)).slice(0, 24000),
        href: await selectedAssetCard.getAttribute('href'),
        action_tooltips: []
      };
      const actionButtons = parent.locator('button');
      const actionCount = await actionButtons.count();
      for (let i = 0; i < Math.min(actionCount, 8); i++) {
        const action = actionButtons.nth(i);
        const box = await action.boundingBox();
        if (!box) continue;
        await action.hover({ force: true });
        await selectedPage.waitForTimeout(450);
        const tooltips = await selectedPage.locator('[role="tooltip"], [data-radix-popper-content-wrapper]').evaluateAll(nodes =>
          nodes.filter(el => {
            const r = el.getBoundingClientRect();
            const s = getComputedStyle(el);
            return r.width > 0 && r.height > 0 && s.visibility !== 'hidden' && s.display !== 'none';
          }).map(el => (el.innerText || el.textContent || '').trim().replace(/\\s+/g, ' ').slice(0, 300))
        );
        report.probe_selected_asset.action_tooltips.push({
          index: i,
          className: await action.getAttribute('class'),
          aria: await action.getAttribute('aria-label'),
          title: await action.getAttribute('title'),
          html: (await action.evaluate(el => el.outerHTML)).slice(0, 5000),
          tooltips
        });
      }
      const selectedActionButtons = selectedAssetCard.locator('button');
      if (await selectedActionButtons.count()) {
        const moreButton = selectedActionButtons.filter({ has: selectedPage.locator('div.i-tripo\\:more-horizontal') }).first();
        const menuTrigger = await moreButton.count() ? moreButton : selectedActionButtons.first();
        await menuTrigger.click({ force: true });
        await selectedPage.waitForTimeout(500);
        report.probe_selected_asset.more_menu = {
          body_text_tail: (await selectedPage.locator('body').innerText()).slice(-5000),
          visible_surfaces: await selectedPage.locator('[role="menu"], [role="dialog"], [data-radix-popper-content-wrapper], [data-reka-popper-content-wrapper]').evaluateAll(nodes =>
            nodes.filter(el => {
              const r = el.getBoundingClientRect();
              const s = getComputedStyle(el);
              return r.width > 0 && r.height > 0 && s.visibility !== 'hidden' && s.display !== 'none';
            }).map(el => ({
              role: el.getAttribute('role') || '',
              text: (el.innerText || el.textContent || '').trim().replace(/\\s+/g, ' ').slice(0, 2000),
              html: el.outerHTML.slice(0, 10000)
            }))
          ),
          visible_buttons: await selectedPage.locator('button').evaluateAll(nodes =>
            nodes.filter(el => {
              const r = el.getBoundingClientRect();
              const s = getComputedStyle(el);
              return r.width > 0 && r.height > 0 && s.visibility !== 'hidden' && s.display !== 'none';
            }).map(el => ({
              text: (el.innerText || el.textContent || '').trim().replace(/\\s+/g, ' ').slice(0, 300),
              aria: el.getAttribute('aria-label') || '',
              title: el.getAttribute('title') || '',
              className: String(el.className || '').slice(0, 300)
            })).filter(x => /export|download|descarg|exportar|eliminar|delete|duplic|rename|renombr|share|compart/i.test(x.text + ' ' + x.aria + ' ' + x.title))
          )
        };
      }
      report.probe_selected_asset.screenshot_path = path.join(path.dirname(outPath), 'tripo-studio-selected-asset-actions.png');
      await selectedPage.screenshot({ path: report.probe_selected_asset.screenshot_path, fullPage: false });
    }
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
  } else if (mode === 'generate' || mode === 'generate_staged') {
    console.log('TRIPO_STUDIO_GENERATE_CLICKED_ONCE');
  }

  // Never close the owner's real Edge session.
  process.exit(0);
} catch (error) {
  try {
    const page = browser?.contexts().flatMap(c => c.pages()).find(p => p.url().includes('studio.tripo3d.ai'));
    if (page) await page.screenshot({ path: path.join(path.dirname(outPath), 'tripo-studio-error.png'), fullPage: false });
  } catch {}
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
