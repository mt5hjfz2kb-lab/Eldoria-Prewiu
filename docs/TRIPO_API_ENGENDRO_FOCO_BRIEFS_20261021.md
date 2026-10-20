# ELDORIA — Tripo API | Briefs de fuente 3D (21-10-2026)

Estado: **art-source experimental**, nunca promoción automática ni cambios al gameplay. Autoridad: `AGENTS.md`, `v0220/js/gameplay.js` y [norma Tripo](TRIPO_API_NORMA_PRODUCCION_V1.md).

## A. Engendro de la Fisura — `spawnling`

**Canon:** enemigo común, nivel 1; misión de Bastión II `c2-spawnling` («Derrota un Engendro de la Fisura»). Mantener id, estadísticas, economía y guardados.

**Intención aprobada:** ser *pequeño*, violeta, semihumanoide y animal, ligeramente deforme, encorvado, asimétrico e inquietante. Propuesta de altura aproximada 1,35 m para pruebas. Deformidad controlada: exactamente dos brazos y dos piernas, rostro singular, manos legibles y rasgos que permitan animación posterior. Piel mate violeta grisácea y fisuras tenues. No convertirlo en demonio, jefe ni guerrero.

**Prompt reproducible (text-to-3D):**

```text
ONE small early-game Rift Spawnling creature for medieval dark fantasy 4X game, full body standalone 3D creature in neutral crouched alert pose. Approximately 1.35 meters tall, hunched asymmetric silhouette, compact vaguely human torso but bestial stance, exactly TWO elongated forearms with short claws and TWO powerful digitigrade legs, one head with narrow distorted face and small fangs, broad low shoulders and curved back. Desaturated matte purple/violet leathery skin with charcoal patches, a few thin muted lavender fissure veins on back and shoulder, extremely subtle lilac inner glow, coherent anatomy suitable for future rigging. Recognizable at high isometric camera distance, eerie organic design, realistic stylized detailed PBR skin. No clothes, no armor, no weapon, no wings, no horns, no extra limbs or head, no enormous spikes, no exposed organs, no blood, no pedestal, no ground, no scenery, no text, no floating pieces.
```

**Riesgos:** manos fundidas, extremidades extra, perfil ilegible a distancia, daño excesivo por decimación, anomalías que impiden rigging. **Gate:** silueta distintiva, 4 extremidades, 4 cámaras, PBR 2K verificado, contacto al suelo, archivo .blend editable. No declarar animado por defecto. Guardar en `ArtSource/World4X/Spawnling_v1/`.

## B. Foco de corrupción de la Brecha — prop aislado experimental

**Límite de lore:** *no es* un nodo o instalación nueva de `v0220`. Es una **candidata de arte de entorno** (roca + corrupción) para estudiar las capacidades de Tripo; no añade campamento, nido funcional, reglas, click, rutas ni nuevas misiones. Se puede rechazar sin efectos en la partida. Tamaño orientativo 2,2 × 1,2 m.

**Prompt reproducible (text-to-3D):**

```text
ONE grounded fantasy 3D corruption focus environmental prop, 2.2m wide and 1.2m tall, a low compact naturally fused cluster of three weathered irregular slate-black mineral rocks merging into one contiguous rough rock base, restrained organic purple-black corrupted material within cracks, few thin veins of dim violet-magenta luminescence, a small uneven amethyst mineral seam protruding from center, subtle ashen deposits, no pedestal or platform. Readable asymmetrical silhouette at high isometric 4X map camera, realistic stylized matte rock physically based texture, grounded weathering and stone-to-corruption transitions. No character, no huge crystals, no altar, no shrine, no statues, no tents, no camp, no building, no ritual symbols, no runes, no weapons, no eggs, no floating objects, no neon landscape, no environment, no text.
```

**Riesgos:** cristales gigantes, altar o símbolos inventados, pedestal rectangular, flotación, brillo excesivo y textura sin lectura estratégica. **Gate:** roca orgánica baja, transición mineral y corrupción creíble, base apoyada, 4 vistas, albedo 2K real en Unity, GLB original y .blend. Fuente en `ArtSource/World4X/RiftFocus_v1/`.

## Producción con Tripo API — control de gasto

Una llamada H3.1 texto + textura estándar **por pieza**; referencia histórica comprobada con el Jabalí: **20 créditos/cada una**; objetivo de gasto **máximo 40 créditos en conjunto** si saldo y precio efectivo lo permiten. Autorización acotada a estas dos piezas, sin regeneraciones pagadas, sin suscripciones Tripo Studio. Primero Engendro, luego Foco. Comprobar sesión/balance/modelo antes de enviar. Tras cada solicitud conservar task-id, coste y GLB raw antes de Blender. Si falla Blender o Unity, recuperar la misma task: nunca cobrar dos veces por reintento técnico.

**NO integración automática:** la revisión Unity aislada no certifica inserción en la escena I–II ni rendimiento Android/iOS. Los puntos de interacción y el avance del juego se conservan intactos. Estándar de pruebas: [lecciones del Jabalí](TRIPO_API_JABALI_LECCIONES_20261021.md).
