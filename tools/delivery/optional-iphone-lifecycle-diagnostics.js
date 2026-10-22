/* Eldoria opt-in local-only lifecycle probe. Does not inspect game state or send data.
 * To enable in a diagnostic build: ?eldoria_diag=1 or #eldoria_diag=1.
 * Remove with ?eldoria_diag=0 or #eldoria_diag=0. Small bounded journal only.
 */
(function () {
  "use strict";
  var STORE = "eldoria.lifecycle.diagnostic.v1";
  var ENABLE = "eldoria.lifecycle.enabled.v1";
  var MAX = 48;
  var storage = null;
  try { storage = window.localStorage; } catch (_) {}
  var flags = String(window.location.search || "") + "&" + String(window.location.hash || "");
  var turnOn = /(?:[?&#])eldoria_diag=1(?:[&#]|$)/.test(flags);
  var turnOff = /(?:[?&#])eldoria_diag=0(?:[&#]|$)/.test(flags);
  if (storage && (turnOff || turnOn)) {
    try { storage.setItem(ENABLE, turnOn && !turnOff ? "1" : "0"); } catch (_) {}
  }
  if (turnOff || (!turnOn && (!storage || safeGet(ENABLE) !== "1"))) return;

  function safeGet(key) {
    try { return storage ? storage.getItem(key) : null; } catch (_) { return null; }
  }
  var history = [];
  try {
    var saved = safeGet(STORE);
    if (saved) {
      var parsed = JSON.parse(saved);
      if (Array.isArray(parsed)) history = parsed.slice(-MAX);
    }
  } catch (_) { history = []; }
  function typeOfNavigation() {
    try {
      var nav = performance.getEntriesByType("navigation");
      return nav.length && nav[0].type ? nav[0].type : "unknown";
    } catch (_) { return "unknown"; }
  }
  function orientation() {
    return window.innerWidth >= window.innerHeight ? "horizontal" : "vertical";
  }
  function classify(value) {
    var s = "";
    try { s = String(value || ""); } catch (_) {}
    if (/out.of.memory|allocation|memory|wasm/i.test(s)) return "memory";
    if (/webgl|context.lost|gpu/i.test(s)) return "graphics";
    if (/fetch|network|failed.to.load|timeout/i.test(s)) return "network";
    return "other"; // NEVER store error messages, URLs, stack traces or save data
  }
  var currentRevision = String(window.__eldoriaDiagnosticRevision || "unlabelled").replace(/[^A-Za-z0-9_.-]/g, "").slice(0,30);
  function add(kind, detail) {
    var event = {
      at: Date.now(), ms: Math.round(performance.now()),
      revision: currentRevision, kind: kind,
      detail: String(detail || "").slice(0,40), orientation: orientation()
    };
    history.push(event);
    history = history.slice(-MAX);
    try { if (storage) storage.setItem(STORE, JSON.stringify(history)); } catch (_) {}
    render();
  }
  var box = null, content = null;
  function render() {
    if (!content) return;
    content.textContent = history.slice(-12).map(function (v) {
      return new Date(v.at).toLocaleTimeString() + " " + v.kind + " " + v.detail;
    }).join("\n");
  }
  function mount() {
    if (!document.body || box) return;
    box = document.createElement("div");
    box.style.cssText = "position:fixed;left:8px;bottom:8px;z-index:2147483647;font:12px monospace;color:#fff;max-width:85vw;";
    var button = document.createElement("button");
    button.type = "button";
    button.textContent = "Diagnóstico";
    button.style.cssText = "background:#182b3a;color:#fff;border:1px solid #ba9a62;border-radius:5px;padding:6px;";
    content = document.createElement("pre");
    content.style.cssText = "display:none;overflow:auto;white-space:pre-wrap;max-height:190px;max-width:320px;background:#071521;border:1px solid #ba9a62;padding:8px;";
    button.addEventListener("click", function () {
      content.style.display = content.style.display === "none" ? "block" : "none";
      render();
    });
    box.appendChild(button); box.appendChild(content); document.body.appendChild(box);
  }
  var api = {
    read: function () { return history.slice(); },
    clear: function () { history = []; try { if (storage) storage.removeItem(STORE); } catch (_) {} render(); },
    enabled: true
  };
  Object.defineProperty(window, "EldoriaLocalDiagnostic", { value: api, configurable: true });
  add("boot", typeOfNavigation());
  window.addEventListener("error", function (e) { add("js-error", classify(e.message)); });
  window.addEventListener("unhandledrejection", function (e) { add("promise-error", classify(e.reason)); });
  window.addEventListener("pageshow", function (e) { add("pageshow", e.persisted ? "bfcache" : "normal"); });
  window.addEventListener("pagehide", function (e) { add("pagehide", e.persisted ? "bfcache" : "normal"); });
  document.addEventListener("visibilitychange", function () { add("visibility", document.visibilityState); });
  document.addEventListener("webglcontextlost", function () { add("webgl", "lost"); }, true);
  document.addEventListener("webglcontextrestored", function () { add("webgl", "restored"); }, true);
  window.addEventListener("resize", function () { add("resize", orientation()); });
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", mount);
  else mount();
})();
