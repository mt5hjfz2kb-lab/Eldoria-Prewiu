# Eldoria — entrada al proyecto

El juego principal se desarrolla en **Unity/**, con Unity **6000.3.23f1**. **main** es la base integrada; las ramas con trabajo autorizado pendiente se consultan en el registro y las PR abiertas. Una candidata, una prueba aprobada y una versión distribuida son estados distintos.

## Empezar aquí

| Necesito saber… | Consultar |
| --- | --- |
| Qué reglas aplicar | [AGENTS.md](AGENTS.md), especialmente el pacto propietario–IA y las órdenes posteriores que revocan requisitos históricos |
| Quién lleva cada trabajo y qué puede editar | [Registro de trabajos](pipeline/active-workstreams.json): `active`, `owner`, `ownership_state`, `ownership_revision`, `scope` y `resources` |
| Qué está integrado y qué sigue pendiente | HEAD de [main](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/tree/main), [PR abiertas](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pulls) y sus cambios; después, solo la última cabecera pertinente de [estado](PROJECT_STATE.md) y [relevo](SESSION_HANDOFF.md) |
| Qué versión se distribuye | [Manifiesto WebGL](pipeline/playtest-delivery.json) y [canal Android familiar](updates/android/family.json), con SHA, versión y evidencia de entrega |
| Dónde encontrar contratos, referencias y pruebas | [Índice documental](docs/DOCUMENTATION_INDEX.md) |
| Cómo continuar sin duplicar trabajo | [Arranque técnico](tools/ELDORIA_AGENT_SESSION_BOOTSTRAP.md) y [protocolo de concurrencia](docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md) |

Con checkout y Node, `node tools/work-status/report.mjs` presenta una consulta breve de trabajos y canales. Es una vista derivada: no modifica el registro ni comprueba por sí sola la actividad de otros chats, la disponibilidad física de Windows o el juego publicado.

## Desarrollo, candidatas y entrega

- **Integrado:** código presente en `main`. El origen native111 es un hito histórico de consolidación, no la versión actual del producto.
- **Candidata:** cambios de una PR todavía sin integrar. Verificar su HEAD y las pruebas de ese mismo candidato; la descripción puede citar un SHA anterior.
- **Distribuido:** versión identificada por el manifiesto de su canal y evidencia de activación/descarga. No atribuirle automáticamente los cambios de `main` o de una PR.

**WebGL:** el canal operativo documentado es el proyecto restringido [Eldoria pruebas en itch.io](https://chrismad.itch.io/eldoria-pruebas). Consultar el [manifiesto](pipeline/playtest-delivery.json) y la [guía de entrega](docs/ELDORIA_PLAYTEST_DELIVERY.md) antes de afirmar qué contiene o publicar. Las rutas antiguas de Pages `unity-owner/` y el despliegue Sites de Valoria son históricos, no la entrada actual de pruebas.

**Android:** usar la versión y el enlace del [canal familiar](updates/android/family.json), según la [guía de actualizaciones](docs/ELDORIA_ANDROID_FAMILY_UPDATES.md). No mantener aquí otra copia del número de APK.

Una build verde, un upload aceptado, una versión activada, la aprobación artística y la prueba física móvil son comprobaciones diferentes. Esta página no autoriza publicación, gasto ni transferencia de reservas.

## Mapa de archivos

| Ruta | Función |
| --- | --- |
| `Unity/` | Proyecto principal: código, escenas, recursos, paquetes y ajustes |
| `ArtSource/`, `art-source/` | Fuentes artísticas existentes; revisar procedencia y dependencias antes de consolidar nombres o mover archivos |
| `docs/` | Contratos, guías y referencias; [catálogo](docs/documentation-catalog.json) para vigencia |
| `docs/evidence/`, `docs/archive/` | Evidencias e historial; sus antiguos estados no son órdenes nuevas |
| `pipeline/` | Registros y configuración de producción; distinguir campos actuales de históricos |
| `tools/`, `scripts/`, `.github/workflows/` | Herramientas y automatización; [índice de workflows](docs/WORKFLOW_INDEX.md) |
| `references/` | Referencias conservadas; usar el contrato vigente del área para identificar el canon |
| `updates/` | Canales de actualización |

`ArtTests` es un nombre de carpeta, no una garantía de que sus recursos estén fuera del juego. Parte de la escena integrada reside allí. Auditar referencias y conservar GUID y `.meta` antes de reorganizar Unity.

## Prototipo web heredado

- `v0220/index.html` y `v0220/js/`: fuente editable del prototipo histórico y referencia de paridad; no son el runtime Unity.
- `playtest/`: salida generada del prototipo; no editar directamente.
- `tester-v0265/`: instantánea congelada Closed Playtest T1 / 0.26.5-test.2.
- `v019*`, `v020*`, `v0210`: versiones históricas; conservar sin reactivar su producción.
- Los metadatos v0.32.0 pertenecen al prototipo web, no identifican una entrega Unity.

## Verificación y mantenimiento

Seguir [QA_AND_DEPLOY.md](QA_AND_DEPLOY.md) para Unity y el candidato exacto. `npm run validate:local` corresponde al prototipo heredado y no certifica Unity.

Para documentación, aplicar la [política de vigencia](docs/DOCUMENTATION_POLICY.md). Mantener cada dato vivo en su fuente: este README orienta, el registro asigna trabajos y los manifiestos identifican entregas. Conservar el historial y comprobar enlaces antes de mover documentos.
