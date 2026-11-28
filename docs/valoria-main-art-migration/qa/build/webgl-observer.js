// Load as a browser init script for both private build measurements.
(() => {
  const glFrames = [];
  let draws = 0;
  let lastRaf;
  let active = false;
  let wrapped = 0;
  let rafId;
  const lost = [];
  const markLoss = event => lost.push({event: event.type, ms: performance.now()});
  document.addEventListener("webglcontextlost", markLoss, true);
  document.addEventListener("webglcontextrestored", markLoss, true);
  for (const context of [window.WebGLRenderingContext, window.WebGL2RenderingContext]) {
    if (!context) continue;
    const proto = context.prototype;
    for (const name of ["drawArrays", "drawElements", "drawArraysInstanced", "drawElementsInstanced", "drawRangeElements"]) {
      const desc = Object.getOwnPropertyDescriptor(proto, name);
      if (!desc || typeof desc.value !== "function") continue;
      const original = desc.value;
      Object.defineProperty(proto, name, {...desc, value: function (...args) {
        if (active) draws++;
        return Reflect.apply(original, this, args);
      }});
      wrapped++;
    }
  }
  function sample(t) {
    if (!active) return;
    if (lastRaf !== undefined) glFrames.push({dt: t - lastRaf, coreDrawCalls: draws});
    draws = 0;
    lastRaf = t;
    rafId = requestAnimationFrame(sample);
  }
  const memory = () => {
    const unity = window.valoriaGame;
    if (!unity || typeof unity.GetMemoryInfo !== "function") return null;
    try { return unity.GetMemoryInfo(); } catch { return null; }
  };
  let firstMemory, startedAt;
  window.valoriaMigrationMeasure = Object.freeze({
    begin() {
      if (active) throw Error("Sample already active");
      if (document.visibilityState !== "visible") throw Error("Page is not visible");
      active = true;
      glFrames.length = 0;
      lost.length = 0;
      lastRaf = undefined;
      draws = 0;
      startedAt = performance.now();
      firstMemory = memory();
      rafId = requestAnimationFrame(sample);
    },
    finish() {
      if (!active) throw Error("Sample was not started");
      active = false;
      cancelAnimationFrame(rafId);
      return {
        durationMs: performance.now() - startedAt, wrappedCoreMethods: wrapped,
        rafSamples: glFrames.slice(), webglContextEvents: lost.slice(),
        unityMemoryAtStart: firstMemory, unityMemoryAtEnd: memory(),
        visibility: document.visibilityState, browser: navigator.userAgent,
        cssWidth: innerWidth, cssHeight: innerHeight, dpr: devicePixelRatio
      };
    }
  });
})();
