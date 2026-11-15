import fs from 'node:fs';
import path from 'node:path';
const root=path.resolve(import.meta.dirname);
let html=fs.readFileSync(path.join(root,'index.html'),'utf8');
const css=fs.readFileSync(path.join(root,'src/styles.css'),'utf8');
const rules=fs.readFileSync(path.join(root,'src/rules.js'),'utf8');
const game=fs.readFileSync(path.join(root,'src/game.js'),'utf8');
html=html.replace('<link rel="stylesheet" href="src/styles.css">',`<style>\n${css}\n</style>`)
 .replace('<script src="src/rules.js"></script>',`<script>\n${rules}\n</script>`)
 .replace('<script src="src/game.js"></script>',`<script>\n${game}\n</script>`);
if(html.includes('src/styles.css')||html.includes('src/rules.js')||html.includes('src/game.js'))throw Error('unpacked reference');
fs.mkdirSync(path.join(root,'dist'),{recursive:true});
fs.writeFileSync(path.join(root,'dist/senda-brecha.html'),html);
console.log('Standalone built:',Buffer.byteLength(html),'bytes');
