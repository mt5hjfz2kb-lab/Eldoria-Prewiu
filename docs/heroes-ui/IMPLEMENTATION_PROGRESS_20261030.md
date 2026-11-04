# PR135 — interactive implementation, awaiting integrated native certification

The former flattened reference display is replaced with real Unity controls on the existing HUD safe area: hero hall, narrative unlocks, selection, summary, abilities, locked talents/equipment pages, available-hero navigation and rotation of a cached 3D render. Every statistic is supplied by the host; no mock values, local save or parallel progression are created.

The model preview copies render data from an already loaded canonical model, including a baked skinned pose. It does not clone gameplay behaviours, import a second GLB, alter the city actor or run an extra per-frame camera. Its render texture and temporary meshes are released. Missing hero art is not replaced by another character.

Approved provenance: six-screen master SHA256 `48cbc4e73d4144c45af12731773f0fd60b45e81d2a871256b33fe2a00f7d6800`, package `Eldoria_Heroes_Integracion_Preparada_v14.zip`. The original master and flattened parts remain references, never interactive backdrops with fake values.

Pending: authoritative host binding, exact Unity compilation and interaction tests, real model orientation, landscape/portrait screenshots and comparison with the approved screens. This change is not a fidelity certificate and is not ready for publication on its own.


## Recovery continuation — 30 October 2026

PR136 remains the integration route; do not reopen or duplicate PR135. Actual native captures of0fdaa4c3/acdf2e71 still show major differences from the original six-screen master. They are functional evidence, never artistic acceptance.

- Lyra profile now centers her exact approved portrait rather than relying on preserveAspect with a top-left pivot.
- The Talents page now builds the three reference paths (Ofensiva, Defensa, Soporte) as separate live uGUI hierarchies instead of displaying one empty explanatory panel. All nine nodes remain disabled. No talent level, price, bonus, allocation, new progression or saved data is invented. The host's existing hero model remains loaded for native identity verification, while the talent diagram owns this page's visible composition. All tabs, real hero selection and existing state checks remain.
- Native V4.1 explicitly checks three paths, nine disabled nodes and the existing byte-identical save after browsing all six screens. Exact new captures are still required. Materials/environment/ornamentation and the other profiles remain visually incomplete.

### Lyra source identity discrepancy — preserve both sources

Read PR127 at3a27851755b9a000ef4b177e3360d6cbfddf3713 and recovered finished artifact12890387724 from40858960344. Optimized Lyra GLB is4770000bytes, SHA256a9fa7899e7ead2ad26e7b26d368c9006085ae8b8ded2df6515ca466c863f6725,30000triangles; Blender source/review preserved. It is unrigged, has no Unity/mobile certificate, and formal_art_approved isfalse. Source reference hash120a8e277cf8e25479fb13db4a41400d38605e50e5a0c4a393685701b93a8ca3.

The inspected model is a brown-haired, leather-clad ranger. The v14 approved master48cbc4e73d4144c45af12731773f0fd60b45e81d2a871256b33fe2a00f7d6800 depicts a light-haired elf with different equipment. Do NOT silently substitute that model for the master's Lyra or claim equivalence. No GLB/model was altered or imported into production for this investigation. No Tripo task, rig operation or paid generation was run. Keep the authentic v14 portrait until identity is reconciled or a matching approved model is available.


## Decisión expresa del propietario — identidad de Lyra, 30/10/2026

Ante las dos fuentes recuperadas, el propietario seleccionó «Modelo 3D existente»: la exploradora castaña de PR127 prevalece para unificar ficha y modelo; conservar la composición de v14. Esto resuelve la discrepancia documentada arriba, no autoriza generación, rigging de pago ni una aprobación global de la UI.

Se reutiliza sin editar el GLB optimizado de la entrega persistente Eldoria-lyra-entrega.zip: 4770000 bytes; SHA256 a9fa7899e7ead2ad26e7b26d368c9006085ae8b8ded2df6515ca466c863f6725, Git blob290c9803242911741ff810fdbddb7591ea39737a; fuente Tripo f4670d45-00c9-43eb-a658-902efdebee87 / artifact12890387724. Se empaqueta como TextAsset .bytes, conservando exactamente los bytes GLB para el importador glTFast existente, con comprobación de tamaño/hash al cargar. Sin nueva generación/créditos.

HeroesLyraModel carga bajo demanda tras desbloqueo narrativo; su geometría fuente permanece inactiva, sin actor jugable ni comandos. HeroesModelSnapshot renderiza la ficha y produce un retrato cacheado de esa misma geometría para Salón/Marchas; se descarta el retrato de identidad contradictoria en el host real. Giro real en ficha; el modelo sigue sin rig, sin animaciones inventadas. Todos los objetos/importador/texturas temporales tienen propietario y liberación. La prueba V4.1 espera la importación, exige geometría/RawImage y retrato derivados del modelo y añade captura girada; el guardado debe seguir idéntico al finalizar.

Pendiente: compilación/importación/capturas y regresión exacta de esta candidata. No trasladar el PASS del padre. Mobile físico y acabado artístico siguen sin certificar.
