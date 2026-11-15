/* Owner-only local phase journal. No requests, raw logs, player/save data or URLs. */
(function () {
  'use strict';
  var api = window.EldoriaLocalDiagnostic;
  if (!api || !api.enabled) return;
  var key = 'eldoria.world-phase.diagnostic.v4', storage = null, saved = null;
  try { storage = window.localStorage; saved = JSON.parse(storage.getItem(key) || 'null'); } catch (_) {}
  var names = {
    ELDORIA_P0_LAZY_WORLD_WAITING_FOR_ENTRY: 'world-waiting',
    ELDORIA_P0_LAZY_WORLD_REQUESTED: 'world-requested',
    REGION1_BLENDER_IMPORTED: 'landscape-imported',
    ELDORIA_TERRAIN_QUERY_TAPE: 'terrain-query',
    REGION1_IMPORTED_PIGMENT_CONTACT: 'landscape-contact',
    REGION1_CANON_WORLD_INSTALLED: 'world-built',
    ELDORIA_P0_LAZY_WORLD_READY_FOR_ENTRY: 'world-ready',
    ELDORIA_P0_WORLD_CAMERA_MSAA_OPTIN_DISABLED: 'world-msaa-off',
    ELDORIA_P0_LAZY_WORLD_ENTERED: 'world-entered',
    REGION1_P0_LAZY_BUILD_FAILED: 'world-build-failed',
    REGION1_P0_LAZY_STAGE_DISPOSAL: 'world-disposal-error',
    REGION1_CANON_WORLD_MISSING_BINDINGS: 'world-bindings-missing',
    REGION1_CANON_WORLD_EXISTING_LAYER_NOT_READY: 'world-layer-not-ready'
  };
  var allowed = new Set(Object.values(names).concat(['world-frame-2','pagehide','pageshow','hidden','visible','webgl-lost','webgl-restored']));
  function clean(rows) {
    return Array.isArray(rows) ? rows.filter(function(e) {
      return e && allowed.has(e.phase) && Number.isFinite(e.at) && Number.isFinite(e.ms);
    }).slice(-32).map(function(e){return {phase:e.phase,at:e.at,ms:e.ms};}) : [];
  }
  var previous = clean(saved && saved.current);
  // A landing-only reload must not erase the last world attempt.
  if (!previous.some(function(e){return e.phase === 'world-requested';})) previous = clean(saved && saved.previous);
  var current = [], persistent = !!storage, panel = null;
  function persist() {
    try { if (!storage) throw Error(); storage.setItem(key, JSON.stringify({v:4,previous:previous,current:current})); }
    catch (_) { persistent = false; }
  }
  function record(phase) {
    if (!allowed.has(phase)) return;
    current.push({phase:phase,at:Date.now(),ms:Math.round(performance.now())});
    current = current.slice(-32); persist(); render();
  }
  function report() {
    return {diagnostic:'beta21-world-v4',persistent:persistent,
      interpretation:'Observaciones, no causa confirmada. Ausencia de pagehide no demuestra un cierre por memoria.',
      previous:previous.slice(),current:current.slice(),lifecycle:api.read()};
  }
  function render() {
    if (!panel) return;
    function lines(rows) {return rows.map(function(e){return new Date(e.at).toLocaleTimeString()+' '+e.phase;}).join('\n') || '(sin fases del mundo)';}
    panel.value = 'ANTERIOR\n'+lines(previous)+'\nACTUAL\n'+lines(current)+'\n'+(persistent?'Registro local guardado':'No se pudo guardar el registro');
  }
  api.readWorldReport = report;
  for (var level of ['log','warn','error']) {
    (function(level) {
      var original = window.console && window.console[level];
      if (typeof original !== 'function') return;
      window.console[level] = function() {
        // Inspect strings only: never call arbitrary object getters/toString.
        try {
          for (var i=0;i<arguments.length;i++) {
            if (typeof arguments[i] !== 'string') continue;
            var match = /^([A-Z0-9_]+)(?:\s|$)/.exec(arguments[i]);
            var phase = match && names[match[1]];
            if (!phase) continue;
            record(phase);
            if (phase === 'world-entered' && window.requestAnimationFrame) {
              window.requestAnimationFrame(function(){window.requestAnimationFrame(function(){record('world-frame-2');});});
            }
          }
        } catch (_) {} // Diagnostics cannot suppress a Unity log or interrupt gameplay.
        return original.apply(this,arguments);
      };
    })(level);
  }
  window.addEventListener('pagehide',function(){record('pagehide');});
  window.addEventListener('pageshow',function(){record('pageshow');});
  document.addEventListener('visibilitychange',function(){record(document.visibilityState==='hidden'?'hidden':'visible');});
  document.addEventListener('webglcontextlost',function(){record('webgl-lost');},true);
  document.addEventListener('webglcontextrestored',function(){record('webgl-restored');},true);
  persist();
  function mount() {
    if (!document.body || panel) return;
    var box=document.createElement('div'), toggle=document.createElement('button'), copy=document.createElement('button');
    box.style.cssText='position:fixed;left:8px;top:8px;z-index:2147483647;max-width:90vw;color:white;background:#071521;font:12px monospace';
    toggle.type=copy.type='button'; toggle.textContent='Diagnóstico · 21d4'; copy.textContent='Copiar registro';
    toggle.style.cssText=copy.style.cssText='background:#182b3a;color:white;border:1px solid #ba9a62;padding:7px';
    panel=document.createElement('textarea');panel.readOnly=true;
    panel.style.cssText='display:none;width:320px;max-width:85vw;height:240px;background:#071521;color:white;font:12px monospace';
    copy.style.display='none';
    toggle.addEventListener('click',function(){var show=panel.style.display==='none';panel.style.display=show?'block':'none';copy.style.display=show?'block':'none';render();});
    copy.addEventListener('click',async function(){
      var text=JSON.stringify(report(),null,2);
      try {await window.navigator.clipboard.writeText(text);copy.textContent='Copiado';}
      catch (_) {panel.value=text;panel.focus();panel.select();copy.textContent='Selecciona y copia';}
    });
    box.appendChild(toggle);box.appendChild(panel);box.appendChild(copy);document.body.appendChild(box);render();
  }
  if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',mount);else mount();
})();
