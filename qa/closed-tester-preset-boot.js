/* QA-only scenario boot: the frozen v0.32 production snapshot never imports qa-fixtures.
   Applied to disposable playtest/ alias ONLY, not v0220 source or public release.
   Use the real exact fixture once per browser session; preserve state on reload. */
(()=>{
'use strict';
const p=new URLSearchParams(location.search);
const scenario=p.get('preset');
if(p.get('qa')!=='1'||!scenario)return;
const fixtures=window.ELDORIA_QA_FIXTURES;
if(!fixtures||typeof fixtures.get!=='function')throw Error('QA preset file failed to load');
const marker='eldoria-qa-active-preset';
if(sessionStorage.getItem(marker)===scenario)return;
const game=fixtures.get(scenario); // Reject unknown presets rather than silently falling back.
if(!game||typeof game!=='object'||!game.chapterProgress)throw Error('QA preset must describe a coherent game chapter');
localStorage.setItem('eldoria-v022-consistent-loop',JSON.stringify(game));
sessionStorage.setItem(marker,scenario);
console.log('FROZEN_QA_PRESET_BOOT',scenario,'chapter',game.chapterProgress.current);
})();