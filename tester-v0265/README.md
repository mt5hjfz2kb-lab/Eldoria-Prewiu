# Eldoria Closed Playtest T1

Temporary construction snapshot for real-player testing.

## Isolation
- Source snapshot: main @ 67f6ff8fd10a01a428d29b22c678f51e5a7225f1.
- Runtime copied into tester-v0265/ so later main development cannot change the tested game.
- The testing layer lives only inside tester-v0265/.
- This is a frozen snapshot, not a second development line.

## Feedback
The tester layer records:
- session/build/version/commit;
- Bastion, current screen, resources, Power and active hero;
- progression flags and selected object when available;
- last relevant tap;
- recent game events and client errors;
- one-tap contextual answers;
- blocked/help reports;
- final 10-question binary survey plus two optional open answers.

Feedback is queued in localStorage first and retried automatically. Central persistence uses Supabase REST with the public anon key under an insert-only RLS policy. The browser never receives a service-role key.

## Required backend
Apply supabase-schema.sql, then set endpoint and anonKey in tester-config.js. The anon role must have INSERT only; reading feedback is done through the authenticated owner/admin connection.

## Certification
qa/e2e-tester-build.js checks first-run intro, one-tap answers, negative follow-up, permanent blocked button, context payload, offline queue/retry, final survey, reload persistence and mobile overflow.

Do not merge or publish this snapshot until backend persistence has been exercised with fictitious records and queried back successfully.
