Actual public browser reload diagnosis, 2026-11-12

Run43178818584 downloaded the unchanged published owner engine and installed the exact current opt-in telemetry script in its disposable browser only. Its first22 actual responses were202, including session start, game ready, orientation and return visit. The report preserved a Fetch access-control error at07:59:07.331, immediately after the explicit Quit/reload. The generic QA message said Unity failed during reload, but its concrete recorded error was the telemetry request during navigation; it is not proof of engine crash or device memory failure.

The optional client used ordinary fetch for the final pagehide event. Current Fetch documentation specifies keepalive for bounded requests that should survive document unload while retaining custom headers and normal CORS. sendToQA now uses keepalive:true and rejects bodies above4000 bytes before network dispatch. The API key, consent, allowed origin, source profile and all browser-error checks stay intact. Nothing is swallowed or filtered to force a pass. The existing queue retains event UUIDs for idempotent retry.

Locally tested:7 client tests pass, including actual Request.keepalive semantics, oversized-request rejection with zero network calls, consent, privacy and durable retry. Actual reload fix remains to verify on desktop WebKit. The next same-engine public journey explicitly includes the existing eldoriaBetaTelemetry=1 native query; a missing query now fails before browser startup. No pilot page, game binary, art or owner save was published or changed.

Reference: https://developer.mozilla.org/en-US/docs/Web/API/Request/keepalive and https://developer.mozilla.org/en-US/docs/Web/API/RequestInit
