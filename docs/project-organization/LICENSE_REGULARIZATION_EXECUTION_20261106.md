# Eldoria — regularización de licencias sin romper las versiones distribuidas

**Fecha:** 06/11/2026. **Naturaleza:** aplicación preventiva y no destructiva, más plan verificable para subsanar los derechos no acreditados.  
**Origen:** [auditoría y 16 familias](LICENSE_PROVENANCE_AUDIT_20261106.md), [registro detallado](LICENSE_PROVENANCE_EVIDENCE_20261106.json).  
**Estado de derechos:** **NO CERTIFICADO** hasta obtener prueba de titularidad/condiciones por recurso y comprobar un Player concreto. Este documento no concede licencias ni altera los contratos de terceros.

## 1. Congelación de la entrega: comprobada antes de cambiar nada

- **APK:** `updates/android/family.json` aún señala `versionCode=21`, `versionName=1.21`, checksum `b90d72811e89bc3e00ff3b88806d8d550794a6e94f96261213228a30cedd7913` y URL pública `https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code21/Eldoria-Valoria-code21.apk`. La API de GitHub confirma el release y archivo (**470.229.532 bytes**), pero no se ha descargado/descomprimido aquí el APK para inspeccionar dependencias.
- **WebGL:** `pipeline/playtest-delivery.json` documenta `native299` en canal restringido `chrismad/eldoria-pruebas` de itch.io, publicación desactivada para nuevas subidas (`publish_enabled:false`). La evidencia de activación remota es anterior a esta ejecución; no es un nuevo test de juego alojado o móvil físico.
- **GitHub:** repositorio fuente configurado **público** y `main` actualmente **sin protección de rama** (`protected:false` en API). No se ha puesto en privado, reescrito historial, regenerado la APK ni modificado los manifiestos de distribución.

**Consecuencia técnica:** cambiar hoy de público a privado puede dejar inaccesible la URL de descarga pública utilizada por el actualizador de Android y afectar a Github Pages de versiones antiguas. No hacerlo antes de migrar esa descarga a un destino público independiente **y verificar la actualización de una instalación anterior**. Itch restringido y Releases públicos son flujos distintos.

## 2. Implementado: barreras preventivas, no cambios de arte

1. **`tools/license-compliance/public-source-gate.mjs`** lee listados de archivos de una PR desde GitHub (API de solo lectura) y **señala como bloqueo** archivos fuente de licencias no acreditadas que se añadan, cambien o renombren dentro de las siete carpetas de paquetes de terceros de Unity y la carpeta Quaternius. Una importación de una carpeta de vendedor nueva/desconocida en el primer nivel de `Unity/Assets` también necesita revisión. Un borrado o cambio de `.meta` no se interpreta como nueva redistribución de fuente.
2. **`.github/workflows/third-party-source-license-gate.yml`** inspecciona PR relevantes con `pull_request_target`, pero **checkout exclusivo de `main`** y token `contents:read`/`pull-requests:read`. No descarga ni ejecuta mallas o código de la PR y no accede a credenciales de proveedores. Emite avisos (no bloqueos) para arte propio/Tripo en `ArtSource/`, `art-source/` y `Unity/Assets/Eldoria/`, para no detener el desarrollo visual mientras se revisa su procedencia.
3. **Validación de catalogación:** el mismo script comprueba que una futura promoción `license_certified:true` en el catálogo de 26 assets del vault exija una ficha de licencia con `licensor`, `license_id`, referencia **privada** de comprobante, revisor, fecha y permisos explícitos de incorporación al juego y de publicación de fuentes. El registro público solo debe enlazar la prueba privada, **nunca contener recibos personales ni credenciales**.
4. **Pruebas de regresión:** `tools/license-compliance/public-source-gate.test.mjs` comprueba casos bloqueados/permitidos, ausencia de impacto en código/arte propio, borrados, movimientos, valores falsos y afirmaciones de licencia sin prueba. Se ejecuta también dentro del **guard de arquitectura ya existente**, sin arrancar Unity, cambiar archivos del Player ni añadir una compilación nocturna.
5. **Limitación intencionada:** un PR con binarios nuevos en zonas propias se marca como advertencia porque no existe una forma automática fiable de distinguir arte propio de obra de terceros por la extensión. El guard **no demuestra que un paquete anterior ya expuesto tenga licencia**, ni garantiza bloqueo de una escritura directa a `main`: la rama no está protegida. Para enforcement real habrá que activar las comprobaciones obligatorias en las reglas del repositorio, solo después de revisar el impacto de las políticas existentes.

## 3. Riesgos aún abiertos: no confundir esta medida con regularización legal completada

| Dependencia pendiente | Evidencia concreta necesaria | Por qué se deja intacta |
| --- | --- | --- |
| **Asset Store (7 familias)** | Titular licenciado, SKU, fecha/versiones, términos particulares y referencia privada de recibo. Incluye `URP_Tree_Pack` con advertencia de Textures.com. | Las carpetas y algunas referencias serializadas pueden alimentar el Player; borrarlas sin estudio rompería imports o el juego. |
| **Tripo: 3 tareas/60 créditos históricos** | Modalidad API en **fecha de generación**, términos de esa fecha y derechos sobre imágenes de entrada, editables y derivados. | Créditos gastados no prueban licencia; original/final tienen usos diferentes y propietario de workstream de criatura activo. |
| **Quaternius (3 FBX)** | Pack, URL/fecha de descarga y licencia original: la FAQ CC0 y la QAL 2026 no son automáticamente equivalentes. | No reescribir licencias pasadas ni borrar comparación sin identificar consumidores. |
| **BuildReport y notices de APK/WebGL** | Informe de assets compilados, `Resources`, `StreamingAssets`, dependencias UPM / MIT / BSD / Apache, notices exigibles por licencia. | El manifiesto de paquetes o un pase de compilación no prueba qué binarios/texturas aparecen en el producto final. |
| **Exposición histórica en GitHub** | Diseño y verificación de un canal de release público **solo binarios**, desarrollo privado y control de forks/copies; probar updater Android con mismo `versionCode` y hash antes del cambio. | Volver privado de golpe arriesga las actualizaciones; hacerlo no borra copias descargadas ni forks públicos previos. |

## 4. Plan de migración segura — criterios de aceptación concretos

**Fase A — protecciones sin cambios de juego (esta PR):** verificar que solo se modifican los archivos nuevos del control y documentación; tests Node y arquitectura verdes. No tocar Unity, canales ni credenciales. No habilitar todavía otra publicación.

**Fase B — demostrar derechos (pendiente de datos de cuenta privada):** el propietario conecta de manera segura o proporciona referencias de adquisición (sin publicarlas). Revisar también términos específicos del editor de URP Tree Pack/Textures.com y demostrar derechos de los 3 Tripo. Cada ficha debe indicar por separado `EMBEDDED_GAME_ALLOWED`, `PUBLIC_SOURCE_ALLOWED`, `NOTICES_REQUIRED`, `UNVERIFIED`. **No marcar ningún `license_certified:true` sin evidencia.**

**Fase C — aislar los entregables del código fuente (pendiente de ejecución):** preparar repositorio/canal público **solo APK y manifiesto**, probar que el APK actual code21 se puede descargar sin autenticación, que versión/certificado/hash permanecen iguales y que la aplicación instalada recibe exactamente el aviso de actualización y conserva guardados. Mantener itch en su canal restringido y probar su URL real sin nueva subida. Solo entonces migrar enlaces del actualizador, crear una versión de prueba y comprobar reproducción. No introducir publicaciones duplicadas.

**Fase D — proteger la fuente original (pendiente de canal C):** pasar a privado el proyecto de desarrollo solo después de demostrar que los enlaces externos de APK/WebGL y workflows siguen funcionando. Vigilar forks públicos que GitHub no privatiza; documentar que la visibilidad no revoca descargas históricas. Evaluar caso por caso si fuentes redistribuidas requieren retirada/acción con el licenciante. No hacer `git filter-repo`, `force push` ni borrar `.meta` en una rama con trabajo concurrente.

**Fase E — verificación completa:** informe de compilación real, avisos de terceros y evidencia de cuentas/derechos contrastada; revisión de riesgos de copyright sin confundir calidad visual ni éxito técnico con licencia.

## 5. Comprobación y límites

**Investigado:** estado de publicación y descarga Android, distribución WebGL, fuente pública, ramas protegidas, catálogo de 26 recursos no certificados, derechos de Asset Store, Tripo, Textures.com, Quaternius y dependencias UPM.

**Aplicado:** pruebas y gate de nuevas importaciones, prevención de una licencia `true` sin justificación; documentación y plan de migración a dos canales. Estos controles no interrumpen los recursos ni las versiones en uso.

**Pendiente de verificar:** la eficacia del workflow sobre la siguiente PR de importación real, el derecho del dueño de la cuenta a todos los paquetes, condiciones Tripo de octubre, contenido exacto de APK/WebGL, funcionamiento de la APK instalada después de migrar a un nuevo servidor y reducción efectiva de la exposición pública.

**No realizado:** cambiar privacidad, subir/copiar APK, borrar archivos binarios, mover fuentes, renovar Tripo, cambiar el actualizador, cancelar workflows ajenos o certificar comercialmente el juego. La política deliberadamente favorece no dañar una compilación existente frente a un cierre prematuro de riesgos.
