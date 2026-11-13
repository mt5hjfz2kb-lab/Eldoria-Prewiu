import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
import test from 'node:test';
const require=createRequire(import.meta.url);
const {parsePS,freshBrowserProcesses}=require('./webkit-process-isolation.cjs');
test('ps parser keeps stable process names and valid PID/RSS',()=>{
 const rows=parsePS(' 301 1 90000 /System/Library/WebKit.WebContent\n' +
 ' 302 301 280000 /System/Library/Apple WebKit.WebContent\n' +
 ' 5 1 200 /usr/sbin/other\n' +
 'not-an-entry\n');
 assert.equal(rows.length,3);
 assert.deepEqual(rows[1],{pid:302,ppid:301,rssKiB:280000,
  name:'/System/Library/Apple WebKit.WebContent'});
});
test('old and unrelated WebContent are excluded from same-bundle memory A/B',()=>{
 const rows=parsePS(' 101 1 900000 WebKit.WebContent\n' +
 ' 102 1 750000 WebKit.WebContent\n' +
 ' 103 1 123 chrome-headless-shell\n' +
 ' 104 1 5000 OtherService\n');
 assert.deepEqual(freshBrowserProcesses(rows,new Set([101,103,104])).map(p=>p.pid),[102]);
 assert.deepEqual(freshBrowserProcesses(rows,new Set([101,102,103,104])),[]);
});
test('no prelaunch PID baseline is not a valid measured memory experiment',()=>{
 assert.throws(()=>freshBrowserProcesses([],null),/baseline/);
});
