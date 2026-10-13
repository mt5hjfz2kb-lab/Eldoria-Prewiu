import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const STATUS = new Set(['current','reference','historical','review_pending']);
const START_PAGES = ['README.md','docs/DOCUMENTATION_INDEX.md','docs/DOCUMENTATION_POLICY.md','docs/WORKFLOW_INDEX.md'];
const REQUIRED = ['AGENTS.md','README.md','docs/DOCUMENTATION_INDEX.md','docs/DOCUMENTATION_POLICY.md','docs/WORKFLOW_INDEX.md','docs/ELDORIA_ART_PIPELINE_INDEX.md'];

function isSafePath(p) { return typeof p === 'string' && p.length > 0 && !path.posix.isAbsolute(p) && !p.includes('\\') && !p.split('/').includes('..') && !p.startsWith('./'); }
export function localLinks(markdown) {
  const links = [];
  for (const m of markdown.matchAll(/\[[^\]]+\]\(([^)]+)\)/g)) {
    let ref = m[1].trim().replace(/\s+["'][^"']+["']$/, '');
    if (!ref || ref.startsWith('#') || /^(?:https?:|mailto:|data:)/i.test(ref)) continue;
    try { ref = decodeURIComponent(ref.split('#')[0].split('?')[0]); } catch { /* preserve original and report if not found */ }
    if (ref) links.push(ref);
  }
  return links;
}

export function auditDocs(root) {
  const errors = [], warnings = [];
  const abs = p => path.resolve(root,p);
  const file = p => fs.existsSync(abs(p)) && fs.statSync(abs(p)).isFile();
  let catalog;
  try { catalog = JSON.parse(fs.readFileSync(abs('docs/documentation-catalog.json'),'utf8')); }
  catch (e) { return { errors: ['Catalog missing or invalid: '+e.message], warnings, counts: {} }; }
  if (catalog.schema_version !== 1 || !Array.isArray(catalog.entries)) errors.push('Invalid catalog schema_version or entries');
  const entries = Array.isArray(catalog.entries) ? catalog.entries : [];
  const seen = new Map();
  for (const item of entries) {
    if (!item || !isSafePath(item.path)) { errors.push('Unsafe or empty catalog path'); continue; }
    if (seen.has(item.path)) errors.push('Duplicate catalog path: '+item.path);
    seen.set(item.path,item);
    if (!STATUS.has(item.status)) errors.push('Unsupported document status: '+item.path);
    if (!item.domain || typeof item.domain !== 'string') errors.push('Missing domain: '+item.path);
    if (!file(item.path)) errors.push('Catalog path missing: '+item.path);
  }
  for (const required of REQUIRED) if (!seen.has(required)) errors.push('Mandatory document missing in catalog: '+required);
  const docsDir = abs('docs');
  if (!fs.existsSync(docsDir)) errors.push('docs/ missing');
  else for (const name of fs.readdirSync(docsDir)) {
    if (name.endsWith('.md') && fs.statSync(path.join(docsDir,name)).isFile() && !seen.has('docs/'+name))
      errors.push('Direct docs Markdown is not inventoried: docs/'+name);
  }
  for (const pg of START_PAGES) {
    if (!file(pg)) { errors.push('Start page missing: '+pg); continue; }
    for (const link of localLinks(fs.readFileSync(abs(pg),'utf8'))) {
      const dest = path.resolve(path.dirname(abs(pg)),link);
      const prefix = root.endsWith(path.sep) ? root : root+path.sep;
      if (dest !== root && !dest.startsWith(prefix)) { errors.push('Link escapes repository: '+pg+' -> '+link); continue; }
      if (!fs.existsSync(dest)) errors.push('Broken start-page link: '+pg+' -> '+link);
    }
  }
  if (seen.get('AGENTS.md')?.status !== 'current') errors.push('AGENTS.md must stay current');
  if (seen.get('docs/ELDORIA_ART_PIPELINE_INDEX.md')?.status !== 'historical') errors.push('Legacy art index must not be marked active');
  if (file('docs/ELDORIA_ART_PIPELINE_INDEX.md') && !fs.readFileSync(abs('docs/ELDORIA_ART_PIPELINE_INDEX.md'),'utf8').includes('HISTÓRICO'))
    errors.push('Legacy art index lacks visible historical banner');
  for (const item of entries) if (item.status === 'review_pending') warnings.push(item.path);
  return {errors,warnings,counts:{catalogued:entries.length,review_pending:warnings.length,direct_docs:fs.existsSync(docsDir)?fs.readdirSync(docsDir).filter(n=>n.endsWith('.md')).length:0}};
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const root = path.resolve(process.cwd());
  const result = auditDocs(root);
  console.log('Documentation governance:',JSON.stringify(result.counts));
  if (result.warnings.length) console.log('NOTE: review_pending is non-authoritative; entries:',result.warnings.length);
  if (result.errors.length) { for (const s of result.errors) console.error('FAIL:',s); process.exitCode=1; }
  else console.log('Documentation integrity PASS (metadata, coverage, navigation links).');
}
