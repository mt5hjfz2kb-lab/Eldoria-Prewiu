# Forja Oscura — relevo y evidencia nativa

La referencia original y la investigación se conservan. No se ha repetido la producción del kit; se integra el marco pintado ya existente, con paridad binaria de la fuente y 42 PNG válidos.

## Probado

Unity 6000.3.23f1, run 42411196403, commit 3ffa4fece5c9cad6428a5f75d66dd263d48f929a: prueba nativa completa correcta. Ocho capturas reales en cuatro resoluciones; controles originales, mínimos táctiles de navegación y ficha contextual, navegación desbloqueada mediante puntero, cierre, igualdad de datos al aplicar/restaurar y construcción con persistencia en partida temporal. Consultar ForjaNativeEvidence.json del artefacto 13588694534 para medidas y rutas bloqueadas sin ejercitar. No equivale a ART_APPROVED ni a Android físico/WebGL.

## Correcciones implementadas

- CapturePrepared omitía Focus al recibir un callback: se reproduce el enfoque canónico, tras disponer de la geometría real.
- Se conservan imágenes de diagnóstico incluso si una resolución falla; la ejecución sigue fallando y no emite certificado correcto.
- Objetivo sin escudo superpuesto, progreso dentro del marco y CTA completo.
- Ficha lateral en horizontal, inferior en vertical; el viewport reserva su geometría real. Sin modificar terreno, luz ni parámetros de cámara.
- Controles cerrar/construir de al menos 44 px, además de navegación; comprobados en render nativo.
- Restauración de la distribución canónica en la resolución vigente: no reutilizar rectángulos de otra orientación.
- Las pruebas de clic sincronizan V2 y renderizan la cámara antes del puntero, como ocurre en un fotograma normal.

## Evidencia negativa conservada

Run 42409713043: Bastión queda parcialmente fuera del viewport permitido en 844×390, tanto en baseline como candidata anterior. El posterior A/B usa el aserradero real del tutorial; su éxito no resuelve ni certifica aquel caso del Bastión. No debilitar ese gate.

## Refinamiento actual y pendientes

La candidata siguiente cambia únicamente el fondo de la miniatura genuina de pergamino a metal oscuro y aumenta su tamaño en escritorio. Conserva mallas y luz y restaura el fondo anterior al retirar Forja. Necesita nueva captura exacta y revisión de los cuatro pares.

Continúan pendientes la revisión artística final respecto a la referencia, estados avanzados/consejo, prueba de Bastión horizontal con la composición nueva y las comprobaciones de Android/WebGL. La PR sigue en borrador; no publicar ni marcar ART_APPROVED por un PASS técnico.
