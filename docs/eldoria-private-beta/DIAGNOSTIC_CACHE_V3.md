# Diagnostic delivery correction

IMG_5041.jpeg shows a second new document at22:28:34 but no startup-* records, despite the live v2 play.js containing those hooks. Original index still referenced unversioned play.js; adding a query only to the page does not invalidate that subresource. Cached bootstrap is a supported hypothesis, not proven inspection of the owner's cache.

V3 fixes this deployment weakness: content-addressed play-<SHA256 prefix>.js, exact byte verification, a fresh diagnostico-v3.html document URL in the same directory, visible Diagnóstico v3 label and diagnostic v3 journal marker. Old local diagnostic history is preserved and bounded; no save data is cleared. Normal game code is unchanged. Local tests pass for opt-in, privacy, 48-entry cap, reboot persistence, script syntax, matching hash and fresh entry.

This is NOT a repair or certification of the involuntary iPhone crash. Next device result must first visibly identify v3, then capture startup-* checkpoint before new boot. Avoid repeated blind links, cache clearing, heap-size guessing or claiming iPhone readiness from HTTP tests.

Preparation08fb51bb492bb81f89e42c1e59de1ff1ada40287; main deployment4abc6338f4b19b660e60d2d7d146f458b9db5d2c. Workflow live check validates the new document and exact versioned script bytes, not just release-identity.json.
