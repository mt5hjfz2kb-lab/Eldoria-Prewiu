# Ampliación de cobertura artística: ciudad y región — 26/11/2026

Rama aislada desde 3486754d5f4c28db2ae4ffa2a7def3b0f000fb3c. No sustituye ni modifica la rama Corona de validación final, Main, el lease de Unity, ni el presupuesto de agentes.

## Evidencia inspeccionada
Artefactos reales de GitHub Actions: 15037184906 (base 5ef2aee67cf8fde57ba2fc4b3d929b517ee67944) y 15114526641 (revisión 91b8de9874aed87219d733efa6e8e0aa56207f1e); ambas capturas 390×844. Se inspeccionaron las capturas de Bastión I–V y las tres vistas de región. Las 13 comparativas del artefacto 15114281960 son de la revisión anterior, no de la candidata final. Las referencias visuales del estudio están en 15113766981; no hay equivalente directo para cada vista de región. Los niveles I–IV mantienen mucho terreno abierto; V gana estructura urbana. No se cambia la composición ya aprobada de la ciudad. La región presenta una pradera poco diferenciada y un río de bajo contraste frente al verde/ocre; requiere riqueza espacial sin añadir capas de arte ni objetos de gameplay.

## Intervención candidata (real en fuente, aún no renderizada)
RegionPaint: modulación amplia del color de las praderas con los campos geográficos existentes; transición suave que protege el entorno del punto de entrada; agua más azul y legible con contraste entre canal y someros. Sin nuevos samplers, mallas, objetos, ni alteración de la máscara de agua, relieve, consultas físicas, cámara, spawn, rutas o jugabilidad. _RegionalColourPass=0 recupera el color anterior. No equivale a una aprobación artística; requiere captura real. No se altera el shader de ciudad.

## Herramienta de evidencia
world-art-ab.py produce nueve parejas auténticas de capturas de niveles I–V y región, exige 390×844 y SHA completos, registra hash de entradas y falla cuando falta una vista. Ejecutada localmente con los dos artefactos históricos citados: nueve parejas producidas; py_compile correcto. El SHA de cada entrada se comprueba contra Actions por separado; el programa no infiere una puntuación ni valida equivalencia de cámara.

## Puertas de aceptación pendientes
Ejecutar Unity en la revisión exacta cuando el carril reservado esté libre; comparar misma cámara y estado con _RegionalColourPass 0 y 1, cerca/región/panorama y cruce del río. Rechazar si se pierde coherencia ciudad-mundo o se afecta legibilidad, rendimiento o rutas. Auditoría artística de ciudad y mundo respecto a referencias comparables, notas antes/después independientes y explícita ausencia de referencias directas. Mantener sin fusionar/publicar. No se han realizado nuevas llamadas de pago en esta intervención manual.
