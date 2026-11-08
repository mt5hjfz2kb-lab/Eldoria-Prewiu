# Tesoros de Valoria · estado de entrega · PR #181

**8 de noviembre de 2026.** Este documento no sustituye a la ejecución nativa ni al criterio artístico del propietario.

## Construido en la rama independiente
- Un calendario real de siete días, rachas que no reinician los premios ya acumulados, importes progresivos, bonus limitados por continuidad y vuelta al primer día después del séptimo.
- Reclamación por `LocalGateway` sobre `PlayerState` canónico, duplicados impedidos, saldo y aceleradores acreditados antes del espectáculo y un justificante informativo en el buzón. Leer o borrar el mensaje no vuelve a pagar.
- Pantalla de siete tarjetas con día 7 de doble anchura, progreso ya recogido, acciones táctiles, fallo de reclamación visible, ceremomia saltable y un único Canvas/eventos compartidos.
- Cofre de madera, hierro y bronce con dos `Sprite` originales 512×512 **ya importados a la PR**, cinco componentes visuales adicionales (tres marcos originales con nine-slice, iconos de madera/piedra/comida/reloj), brillo y brasas. Son **nueve PNG** originales verificables y cada uno tiene importador `.meta` estable.
- Línea de tiempo determinista de unos 2,35 s, con exposición de cofre abierto y todos los premios claros después. Percusión y timbre procedurales de prueba aún no escuchados dentro de Unity.
- Ceremonia de cierre de capítulo Vigilantes, solo de presentación. Ningún premio ficticio ni segundo cobro.
- Vigilia PR173, beta native325/PR179, APK y WebGL siguen **inalterados**.

## Evidencia verificable existente
- **4.118** comprobaciones de reglas y fotogramas temporizados en C#.
- **82** comprobaciones del gateway real: siete fechas, recursos, correo, reintento, persistencia, saldos y rollover.
- **8** disposiciones geométricas con verificación de solapes y objetivos táctiles, además de análisis sintáctico Roslyn C#.
- Fuente real de arte en Git (no enlaces): SHA256 de los **nueve PNG** y metadatos de Unity validados por CI Ubuntu.
- Ensayo de UI independiente en navegador: aperturas y cierre en ocho tamaños, inspección visual y exportaciones. No certifica Unity ni un juego móvil.

La evidencia CI correspondiente a cada SHA debe consultarse en Actions; no utilizar un PASS de un commit anterior como certificación de un commit posterior.

## Trabajo no terminado — no aprobado aún
- [ ] Formalizar la liberación del runner Windows por PR179 antes de usarlo para PR181.
- [ ] Compilar Unity 6000.3.23f1 y generar 12 capturas **nativas** (cuatro tamaños × calendario/cofre cerrado/abierto), 16 eventos de puntero y 4 pagos auténticos en estado descartable, con hashes.
- [ ] Confirmar que la apariencia alcanza al menos la calidad de la previsualización del cofre ya aprobada. Si no, cambiar el enfoque artístico, no apilar pequeños parches.
- [ ] Integrar en la ciudad real, evitar duplicar HUD cuando Vigilia de la Brecha esté lista, comprobar tutorial, Vigilantes y buzón, sin alterar Bastión I–V.
- [ ] Validar render, sonido, memorias, pantallas reales iOS/Android/WebGL y equilibrio económico.
- [ ] Autorizar merge, publicar y distribuir únicamente si esos gates son satisfactorios.

Revisión artística y requisitos completos: [ART_AND_NATIVE_ACCEPTANCE_20261108.md](ART_AND_NATIVE_ACCEPTANCE_20261108.md).

**Estado inequívoco: IMPLEMENTADO EN PR181 / ARTE BINARIO IMPORTADO / CI DE DOMINIO Y ASSETS / UNITY NATIVO PENDIENTE / ART_APPROVED=FALSE / NO MERGE NI RELEASE.**
