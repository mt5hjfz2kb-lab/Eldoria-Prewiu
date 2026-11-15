'use strict';
const fs = require('node:fs');
const path = require('node:path');
const dir = __dirname;
let html = fs.readFileSync(path.join(dir,'index.html'),'utf8');
const css=fs.readFileSync(path.join(dir,'style.css'),'utf8');
const engine=fs.readFileSync(path.join(dir,'engine.js'),'utf8');
const app=fs.readFileSync(path.join(dir,'app.js'),'utf8');
html=html.replace('<link rel="stylesheet" href="style.css">','<style>\n'+css+'\n</style>')
  .replace('<script src="engine.js"></script>','<script>\n'+engine+'\n</script>')
  .replace('<script src="app.js"></script>','<script>\n'+app+'\n</script>');
if(/(?:href="style.css"|src="engine.js"|src="app.js")/.test(html))throw Error('Bundler left an external dependency');
const out=process.argv[2]||path.join(dir,'senda-brecha-standalone.html');
fs.writeFileSync(out,html);
console.log('Portable offline HTML created:',out,fs.statSync(out).size,'bytes');
