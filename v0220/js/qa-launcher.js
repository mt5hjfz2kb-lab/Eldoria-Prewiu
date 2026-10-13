/* Eldoria QA launcher for frozen v0.32. Never enabled in a normal player session.
 * All presets come from the existing deterministic QA fixtures; no invented game data.
 * Uses a disposable QA tab-local backup, not a second persistent game save.
 */
(function () {
 'use strict';
 const params=new URLSearchParams(location.search);
 if(params.get('qa')!=='1')return;
 const storageKey='eldoria-v022-consistent-loop';
 const activeKey='eldoria-qa-active-preset';
 const backupKey='eldoria-qa-original-save-v1';
 const NO_SAVE='__ELDORIA_QA_NO_SAVE__';
 function preserve(){
  if(!sessionStorage.getItem(backupKey))
   sessionStorage.setItem(backupKey,localStorage.getItem(storageKey)??NO_SAVE);
 }
 window.ELDORIA_QA_STORAGE={
  hasNormalSave:()=>{const prior=sessionStorage.getItem(backupKey);return prior!==null && prior!==NO_SAVE},
  restoreOriginal:()=>{
   const prior=sessionStorage.getItem(backupKey);
   if(prior===NO_SAVE)localStorage.removeItem(storageKey);
   else if(prior!==null)localStorage.setItem(storageKey,prior);
   sessionStorage.removeItem(activeKey);
   location.reload();
  }
 };
 const css=document.createElement('style');css.textContent=`
  #eldoria-qa-launch-button{position:fixed;top:5px;right:5px;z-index:2147483600;padding:6px 9px;font:600 11px Arial;background:#142936;color:#fff;border:1px solid #55c6a4;border-radius:6px}
  #eldoria-qa-launch-dialog{position:fixed;inset:2% 2% 6%;z-index:2147483601;background:#101f27;color:#f8f7ef;border:1px solid #55c6a4;border-radius:9px;padding:12px;overflow:auto;box-shadow:0 0 0 20px #05090bb8;font:13px Arial}
  #eldoria-qa-launch-dialog button{display:block;width:100%;margin:5px 0;padding:10px;text-align:left;border:1px solid #618778;border-radius:6px;background:#273c47;color:#fafafa;font:12px Arial;touch-action:manipulation}
  #eldoria-qa-launch-dialog button:focus-visible{outline:3px solid #ffe57b}
  #eldoria-qa-launch-dialog h3{margin:2px 0 10px;font:600 16px Arial}
  `;document.head.appendChild(css);
 const toggle=document.createElement('button');toggle.id='eldoria-qa-launch-button';
 toggle.type='button';toggle.dataset.testid='qa-launcher-button';toggle.textContent='🧪 QA';
 document.body.appendChild(toggle);
 function render(){
  const found=document.querySelector('[data-testid="qa-launcher"]');
  if(found)return found;
  const presets=window.ELDORIA_QA_FIXTURES?.list?.()||[];
  const panel=document.createElement('section');panel.id='eldoria-qa-launch-dialog';panel.dataset.testid='qa-launcher';
  panel.innerHTML='<h3>ELDORIA — QA / TESTER</h3><p>Fixtures de desarrollo · solo ?qa=1</p>';
  for(const preset of presets){
   const button=document.createElement('button');button.type='button';button.dataset.qaPreset=preset.id;
   button.textContent=preset.label+' · '+preset.mode;
   button.title=preset.description;
   button.onclick=()=>{
    preserve();
    const gameState=window.ELDORIA_QA_FIXTURES.get(preset.id);
    localStorage.setItem(storageKey,JSON.stringify(gameState));
    sessionStorage.setItem(activeKey,preset.id);
    location.reload();
   };
   panel.appendChild(button);
  }
  const fresh=document.createElement('button');fresh.dataset.qaFresh='1';fresh.type='button';fresh.textContent='↻ Partida de prueba nueva';
  fresh.onclick=()=>{preserve();localStorage.removeItem(storageKey);sessionStorage.setItem(activeKey,'fresh');location.reload()};
  panel.appendChild(fresh);
  const restore=document.createElement('button');restore.type='button';restore.textContent='Restaurar partida anterior';
  restore.onclick=()=>window.ELDORIA_QA_STORAGE.restoreOriginal();panel.appendChild(restore);
  const close=document.createElement('button');close.type='button';close.textContent='Cerrar';close.onclick=()=>panel.remove();panel.appendChild(close);
  document.body.appendChild(panel);
  return panel;
 }
 toggle.onclick=()=>{const old=document.querySelector('[data-testid="qa-launcher"]');if(old)old.remove();else render()};
})();
