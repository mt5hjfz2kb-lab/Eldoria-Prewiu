import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const workflow=readFileSync('.github/workflows/eldoria-beta-lazy-world-native.yml','utf8');
test('mac A/B cannot run before real Unity PlayMode and single-owner native WebGL',()=>{
  assert.match(workflow,/  package-owner:\n    needs: native\n    if: needs.native.result == 'success'/);
  assert.match(workflow,/  mac-same-bundle-ab:\n    needs: package-owner\n    if: needs.package-owner.result == 'success'/);
  assert.match(workflow,/    runs-on: macos-15/);
});
test('same compiled site, browser URL flag only; never tester release',()=>{
  const control=workflow.match(/QA_URL='(http:\/\/127\.0\.0\.1:18765\/same-bundle\/\?eldoria_diag=1&eldoria_profile=1)'/);
  const candidate=workflow.match(/QA_URL='(http:\/\/127\.0\.0\.1:18765\/same-bundle\/\?eldoria_diag=1&eldoria_profile=1&eldoria_p0_lazy_world=1)'/);
  assert.ok(control&&candidate);
  assert.equal(candidate[1],control[1]+'&eldoria_p0_lazy_world=1');
  assert.match(workflow,/node tools\/delivery\/test-exact-decoded-transport\.mjs owner-only-site/);
  assert.match(workflow,/physical_iphone_30min_verified'\] is False/);
  assert.doesNotMatch(workflow,/\bpeaceiris\/actions-gh-pages\b|\bcloudflare\/pages-action\b/);
});
