# Eldoria — Tester Feedback Protocol

Applies to **Eldoria Closed Playtest T1** only.

## Frozen test cohort
- Build: **Eldoria Closed Playtest T1**
- Version: **0.26.5-test.1**
- Certified commit: `84c0a4071f3be639154f021689109719e9ba6cf7`
- Frozen ref: `frozen/testers-v0265-t1`
- Public URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/tester-v0265/
- During the round every tester must receive the same gameplay snapshot. Only a test-invalidating critical bug can justify a new tester build/version.

## Evidence handling
Do not implement individual opinions automatically. Raw feedback remains immutable evidence in Supabase table `public.eldoria_feedback`.

Classify findings as:
`bug`, `bloqueo`, `problema_ux`, `problema_comprension`, `navegacion`, `combate`, `progresion`, `economia`, `poder`, `heroes`, `cartas_codice`, `narrativa_lore`, `ritmo`, `equilibrio`, `interfaz`, `rendimiento`, `sugerencia`, `comentario_subjetivo`, `posible_falso_problema`.

## Grouping
Equivalent reports become one canonical finding. Track:
- total reports;
- distinct tester sessions;
- whether telemetry/context reveals the problem even without an explicit complaint;
- linked raw feedback IDs.

One report can open a finding. Repeated reports increase evidence, not the number of findings.

## Priority
Priority is 1 (highest) to 8 (lowest):
1. blocks play;
2. prevents understanding what to do;
3. repeats across testers;
4. materially harms enjoyment;
5. materially harms progression;
6. affects a central system;
7. minor improvement;
8. subjective preference.

Priority is evidence-based and may change as more sessions arrive.

## Yes / No aggregation
For each quick question calculate counts and percentages by build version and question ID. Do not interpret a percentage alone. Cross-check it with:
- the game moment/context where the answer occurred;
- Bastion/screen/progression state;
- blocker reports;
- optional follow-up reason;
- open comments;
- session completion/continuation responses.

Key vertical-slice signals include combat comprehension, feeling lost/blocked, hero interest, Power interest, card/Codex interest, desire for more cards, Breach curiosity, narrative continuation and willingness to continue playing.

## Traceability
Use:
- `public.eldoria_feedback` = immutable raw evidence;
- `public.eldoria_feedback_findings` = grouped problem/insight;
- `public.eldoria_feedback_finding_links` = evidence links.

Every consequential decision should preserve:
feedback → finding → analysis → decision → implementation/discard reason → commit/version.

Do not delete evidence because a suggestion is rejected or a report is determined to be a false problem.

## End-of-round decision gate
Before using T1 results to drive a large redesign or Unity migration, compare the evidence against the vertical-slice questions:
- Is the core Valoria → world → reward → growth loop understood?
- Where do players become blocked or lost?
- Does progression create desire for the next upgrade?
- Is Power motivating and understandable?
- Is combat readable enough for the intended direction?
- Are heroes worth developing?
- Do cards/Codex create collection and use/conserve interest?
- Does La Brecha generate curiosity?
- Does the story create desire to continue?
- Would players continue with more content?

The result should be a consolidated evidence report, not a list of raw opinions.
