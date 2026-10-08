# Eldoria — operating contract for the owner-approved autonomous studio

Authority: AGENTS.md, SESSION_HANDOFF.md, PROJECT_STATE.md, pipeline/active-workstreams.json, Issues #23/#25. This document records the *existing* operating objective; it does not create a parallel directorate.

## Original intent (owner ratified 2026-10-08)
Owner proposes game outcomes and approves finite production rounds. Dirección General audits 15 specialties, estimates improvements, plans dependencies and resources, supervises accepted results and reports to owner. Coordinator dispatches only approved bounded orders, preserves durable claim/evidence, passes outputs to independent QA and returns PASS/BLOCKED/decision to Dirección General. An authorized failed task may retry within an explicit cap, never start another round or spend credits automatically. Owner does not act as chat messenger or press Actions buttons.

## Fifteen departments (responsibilities, not claim that 15 local agents exist)
D01 Direction and governance; D02 technical architecture; D03 AI/automation; D04 visual R&D; D05 art production; D06 state and persistence; D07 gameplay/progression; D08 World 4X; D09 camera/touch; D10 UI/UX; D11 visual integration; D12 animation/life; D13 QA; D14 mobile/publication; D15 audio.

## What is proved (distinguish evidence from aspiration)
- First local Qwen/Ollama coding pilot on Windows: worker 37811949263 + coordinator 37812029139; 9/9 isolated verification, zero paid/external APIs; pilot closed.
- First local model BOM/JSON maintenance: worker 37820089071, independently checked 6/6, SHA/BOM and cleanup; accepted by coordinator, forwarded to Directorate Issue #23, workstream resource released. The reviewable correction remains separately governed.
- Existing M16 GitHub A→B handoff: real runner jobs and independent tests; owner-authorized event dispatch run 37822854326 produced workflow_dispatch run 37822865584 SUCCESS.
- Coordinator→Directorate issue handoff and scheduled DG issue watchdog; watchdog run 37822592466 SUCCESS. Owner ChatGPT condition-watch checks at most hourly and is NOT instantaneous event-based reopening of a chat.
- Tested finite authorization/QA/retry/reporter state reducer merged in commit 109c2afcc70c198de651a3a3723a18d07b133e60; GitHub test run 37822036668 SUCCESS. The reducer itself is NOT a general live dispatcher until bound to authenticated event and durable persistence.
- For the M16 acceptance relay, .github/workflows/eldoria-dg-m16-acceptance-intake.yml independently checks A/B job conclusions, upstream SHA and run artifact; its *live completion* must be confirmed separately, not inferred from its code.

## Safety and non-regression requirements
Repository outranks chat. Do not reopen closed Bastion I-II. M07 construction-state visual correction remains BLOCKED pending native/web captures; do not touch the completed SHARP sawmill or start M11 before verified M07 closure. Preserve claim ownership, no competing Windows runner, no paid API/credits without specific owner approval, no unrestricted AI-generated code execution, no auto-merge, no unlimited retry loop. Owner approval of a round is never automated. A green technical gate is not visual acceptance. Chat silence is not a workstream completion event.

## End-to-end gates still required before announcing entire studio operational
A. A real pre-authorized task is issued to a specific local agent.
B. Worker executes, returns cryptographically linked evidence, independent QA verifies.
C. A real failure routes to a bounded correction and recheck without a person passing messages.
D. Coordinator durably records closure and passes verified result to DG.
E. DG actually evaluates and owner receives a **proactive** notification; mere issue comments or hourly polling do not prove instant notification.
F. A dependency handoff to a distinct department executes without unauthorized scope or resource overlap.
G. 15 roles remain defined, but each **active** agent and each tested transfer must be marked separately; do not claim 15 agents deployed from one pilot.

Current blocking capability: no available connector provides programmatic ChatGPT Work task creation or a webhook that reopens this chat; GitHub comments alone do not implement E. Continue using the authorized hourly notification watch honestly while pursuing an explicit supported native event integration. Never ask owner to be messenger as substitute.
