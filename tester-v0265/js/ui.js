/* Eldoria v0.24 UI boundary. Keeps DOM concerns out of state/economy/task primitives. */
(()=>{'use strict';const E=window.ELDORIA=window.ELDORIA||{};
E.ui={format:n=>Math.floor(Number(n)||0).toLocaleString('es-ES'),clearOverlays:()=>document.querySelectorAll('.e22-overlay,.aldric-cinematic,.tutorialDialogue,.tutorialAldric').forEach(x=>x.remove())};
})();