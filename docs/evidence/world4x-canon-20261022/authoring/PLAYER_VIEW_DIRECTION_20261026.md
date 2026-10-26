# Corrección de la cámara del jugador — 26-10-2026

Dirección: codex-world4x-player-art-22db827a. Encargo vigente del propietario: corregir el mundo, priorizar la imagen cercana, nodos al 85% de la ciudad, integrar después de demostrar calidad y jugabilidad. Presupuesto 0 €.

## Evidencia y diagnóstico

Se han visto los tres PNG originales canónicos y las capturas genuinas del candidato Unity 40253785923, en ambas orientaciones. Evaluación inicial aproximada: cercana 6,5; regional 5,8; panorámica 5,2. No es una certificación. La referencia muestra claros verdes y dorados conectados, sombras legibles, amenazas grandes, puentes cortos y rotura geológica irregular. El candidato presenta dominante amarilla, contraste entre parche central y continuación, recursos pequeños, puente excesivamente largo y paredes de Brecha regulares. Un build verde no resuelve estas diferencias.

Reutilización comprobada con tools/asset-vault/find-reuse.py sobre los índices de main: 14 candidatos. Se conserva la fuente real de Región 1 y sus recursos aprobados; no se añaden módulos arquitectónicos de Valoria ni primitivas decorativas ajenas. Fuente inspeccionada mediante BlenderMCP en 40287703090, SHA-256 fd3063a792eedf77d6908fe9f5d894b47514717720a5d03ee1ac4e08f0690cce. La inspección registra límites, vértices y distribución de materiales reales.

## Decisiones de esta iteración

1. Medir anchura horizontal de los vértices importados: ciudad 24, nodos 20,4. Ajustar el volumen seleccionable existente. Criaturas cercanas: jabalí 10,8 y engendro 13,2, conservando el comportamiento regional.
2. Sustituir la amplificación roja del césped por claros verdes/dorados a escala del mundo, manteniendo grano y caminos del pigmento fuente. Unificar el tratamiento a ambos lados de los límites de material.
3. Dar más lectura a las sombras mediante relleno acotado; reducir las tres luces violetas que se leen como focos.
4. Esculpir escalones y fracturas en los vértices existentes de las paredes, conservando núcleo AF/AG, imágenes originales, topología, reservas y máscaras jugables. Reproyectar las superficies geológicas sobre el terreno y regenerar los datos de altura correspondientes. Eliminar las líneas sinusoidales del acabado.

La ejecución de estas operaciones no constituye aceptación visual. La longitud del puente, el borde de cantera, diversidad de copas y continuidad panorámica requieren revisión y corrección posterior según las capturas. No afirmar que estos defectos estén ya resueltos.

## Criterio de cierre

Revisar capturas reales cercanas, regionales y panorámicas H/V y detalles de Brecha, nodos y criaturas. Mantener pruebas de soporte, selección, rutas, población, persistencia e integración I–II. Comparar con referencia original; valorar por composición y legibilidad. No integrar un candidato defectuoso para alcanzar una fecha, no rebajar tests y no declarar rendimiento físico iPhone sin medición.
