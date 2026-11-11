'use strict';
// Native ps sampling must not attribute an older or unrelated WebKit process
// to our same-bundle eager/lazy Mac experiment. Each experiment starts with
// a PID baseline and reports newly spawned browser descendants only.
const processPattern=/WebKit|MiniBrowser|Chromium|Google Chrome|chrome-headless/;
function parsePS(text) {
 if(typeof text!=='string')throw TypeError('ps output missing');
 return text.split('\n').map(row=>{
  const m=row.trim().match(/^(\d+)\s+(\d+)\s+(\d+)\s+(.+)$/);
  return m?{pid:Number(m[1]),ppid:Number(m[2]),rssKiB:Number(m[3]),name:m[4]}:null;
 }).filter(Boolean);
}
function freshBrowserProcesses(rows, baseline) {
 if(!(baseline instanceof Set))throw TypeError('prelaunch PID baseline required');
 return rows.filter(p=>processPattern.test(p.name)&&!baseline.has(p.pid));
}
module.exports={parsePS,freshBrowserProcesses};
