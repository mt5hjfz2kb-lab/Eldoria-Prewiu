# Native325 iPhone restart at90% — verified findings

IMG_5042.jpeg (owner-provided,2026-11-07) confirms diagnosticv3; data parts7 and8, progress70,80,90 at22:34:59 then involuntary new boot at22:35:00. No ready/post-run or failure checkpoint visible. Do not equate the progress display with proof of a completed successful HTTP stream or infer an iOS jetsam termination without device evidence.

Downloaded all nine live gzip chunks and verified each decoded SHA256 and length against manifest. Parsed UnityWebData header, UnityFS block/file table, and serialized objects with UnityPy. No binary changed.

Exact measurements: .data585377820 bytes; data.unity3d577593703; UnityFS uncompressed object files1354926684; Texture2D serialized objects1089250768; Mesh231924472. These are serialized content totals, NOT measured simultaneous RAM residency. resources.assets809892872 and sharedassets0.assets537894404. Many largest textures are uncompressed RGB24/RGBA32/ARGB32, e.g. ContinuousLivingGeography3072x3072 with50331640 image bytes; numerous2048 textures have22369620 mipmapped image bytes. Full top60 and type totals in pipeline/eldoria-private-beta/native325-payload-audit.json. Path IDs are file-local; names+file mapping must be resolved before any edit.

Conclusion: known large uncompressed texture payload makes startup memory pressure a strong hypothesis; root cause and crash fix are NOT certified. Further wrapper cache links are not the remedy. Candidate work: resolve source texture identities and runtime dependencies from exact native325 source948ffa9837168a0b68dab80be76b66350422b23a and its real build entry; prepare isolated mobile texture compression with format-capability selection (ASTC-capable target), audit Resources inclusion without deleting original resources, preserve alpha/normal maps/UI/mips, compare dimensions and visual output. Do not blindly compress all data textures, edit frozen binary, shrink all textures, increase heap, or use stale SHARP build entry merely because its filename says current.

Acceptance: compare candidate object sizes, measure actual startup/peak memory, verify actual mobile-format support, desktop/mobile startup, visual fidelity, unchanged tutorial/gameplay/save, then physical iPhone startup and30-minute stability. No automatic promotion.

Available native Unity build resource is reserved by eldoria-forja-oscura-visual-proof-20261107 (owner codex-forja-55add623b635); do not steal/cancel it. This phase produced an exact binary diagnosis, not an optimized build.
