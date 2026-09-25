# Eldoria Unity — shortlist de assets 0 € (2026-09-25)

Objetivo: elevar Valoria/Frontier sin convertir Eldoria en un collage de packs ni comprometer rendimiento móvil. Esta lista es deliberadamente corta. No sustituye `docs/VISUAL_BIBLE.md`; todos los assets deben adaptarse a esa dirección mediante composición, materiales, iluminación, escala y selección.

## Descargar / evaluar primero

### 1. FREE Slavic Medieval Environment Town Interior and exterior
- Rol: edificios secundarios, viviendas, madera/piedra, props y vegetación de apoyo para Valoria.
- Precio observado: FREE.
- Licencia: Standard Unity Asset Store EULA.
- URP: compatible.
- Original Unity: 2021.3.43; versión del pack 1.6, actualización 2025-05-19.
- Tamaño: 28.8 MB.
- Decisión: **PRIORIDAD ALTA**. Usarlo como cantera modular, no como identidad visual completa de Valoria.

### 2. URP Tree Models
- Rol: reemplazar árboles/cilindros provisionales con vegetación 3D preparada para URP.
- Precio observado: FREE.
- Licencia: Standard Unity Asset Store EULA.
- URP: compatible.
- Original Unity: 6000.0.62; versión 1.2.3, actualización 2026-02-16.
- Tamaño: 18.1 MB.
- Decisión: **PRIORIDAD ALTA**. Mejor encaje técnico actual para Unity 6.

### 3. Free Fire VFX - URP
- Rol: antorchas, hogares, Aserradero, Cuartel/forja y feedback puntual.
- Precio observado: FREE.
- Licencia: Standard Unity Asset Store EULA.
- URP: compatible incluso en Unity 6000.0.23f1 según Asset Store.
- Versión 1.0.2023.1, actualización 2025-02-15.
- Tamaño: 8.9 MB.
- Decisión: **PRIORIDAD ALTA**.

### 4. Magic Effects FREE
- Rol: materia prima para La Brecha/corrupción/impactos, con uso muy selectivo.
- Precio observado: FREE.
- Licencia: Standard Unity Asset Store EULA.
- URP: compatible.
- Original Unity: 2021.3.15; versión 1.6, actualización 2025-06-04.
- Tamaño: 36.4 MB.
- Decisión: **PRIORIDAD MEDIA-ALTA**. No usar presets completos como estética final; extraer partículas/materiales útiles y reautorizar color/ritmo.

### 5. Free Stylized Smoke Effects Pack
- Rol: humo de hogares, actividad de ciudad y reconstrucción.
- Precio observado: FREE.
- Licencia: Standard Unity Asset Store EULA.
- URP: compatible.
- Original Unity: 2020.3.36; versión 1.0.1.
- Tamaño: 120.5 KB.
- Decisión: **PRIORIDAD MEDIA**. Probar compatibilidad visual y rendimiento antes de consolidar.

## Ya presentes

### Free Pack - Modular Terrain
- Ya integrado parcialmente.
- FREE, Standard Unity Asset Store EULA, URP compatible, original Unity 2022.3.3, actualizado 2026-08-10.
- Mantener para relieve/cliffs/hills mientras cumpla la Visual Bible.

### Simple Modular Castle Assets
- Ya integrado.
- FREE, URP compatible, original Unity 2022.3.36.
- Es minimalista/low-poly. Mantener como bloqueo o piezas invisibles/secundarias; **no debe definir el arte final del Bastión**.

## Evaluar con cautela / no descargar todavía

### European Forests - Realistic Trees
- FREE, URP compatible, Standard Unity Asset Store EULA, 198.1 MB.
- Visualmente puede aportar más naturalismo, pero es bastante más pesado y antiguo que URP Tree Models.
- Decisión: **RESERVA**. Probar solo si URP Tree Models no alcanza la densidad/tono buscado.

### RPG Poly Pack - Abandoned Village
- La ficha muestra compatibilidad URP incluso para Unity 6000.1.
- Puede aportar ruina/destrucción, pero su dirección "poly" puede chocar con el semi-realismo serio de Eldoria.
- Decisión: **RESERVA VISUAL**, no prioridad.

## Rechazados para esta etapa
- Packs de pago: no entran mientras la política siga en 0 €.
- Occult Stone Pillars: la ficha actual declara URP **no compatible**.
- Knight/archer gratuitos con estética low-poly/cartoon sin soporte URP adecuado: no fijarlos como Aldric/tropas.
- Assets que impongan toon shader o una identidad visual reconocible ajena a Eldoria.

## Regla de integración
1. Importar un pack cada vez.
2. Comprobar compilación y materiales URP.
3. Crear una capa/adaptador propia de Eldoria; gameplay nunca referencia directamente paths frágiles del pack.
4. Mantener prefabs originales intactos y construir derivados/adaptadores.
5. Validar en el runner Windows.
6. Comparar móvil/desktop antes de declarar una pieza como dirección visual.
7. Si un pack encarece demasiado draw calls, memoria o tamaño de build, queda fuera aunque sea gratuito.

## Próximo uso
Orden recomendado de importación manual del owner: **Slavic Medieval → URP Tree Models → Free Fire VFX URP → Magic Effects FREE → Smoke**. Tras cada importación, el agente integrará y certificará antes de pedir el siguiente si aparece algún conflicto.
