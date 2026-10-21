import fs from 'node:fs';
import { requireLiveLease } from './lease.mjs';

  try {
    const [snapshotPath, livePath, id] = process.argv.slice(2);
    if (!snapshotPath || !livePath || !id) throw Error('Usage: check.mjs SNAPSHOT_JSON LIVE_JSON WORKSTREAM_ID');
    const result = requireLiveLease(JSON.parse(fs.readFileSync(snapshotPath, 'utf8').replace(/^\uFEFF/, '')),
      JSON.parse(fs.readFileSync(livePath, 'utf8').replace(/^\uFEFF/, '')), id);
    console.log('WORKSTREAM_LEASE_PASS ' + JSON.stringify(result));
  } catch (error) {
    console.error('WORKSTREAM_LEASE_FAIL: ' + error.message);
    process.exitCode = 1;
  }
