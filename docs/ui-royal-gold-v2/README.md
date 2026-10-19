# Eldoria — Royal Gold V2 · trabajo aislado y coordinación

**Estado verificable:** SOURCE_PREPARED / UNITY_COMPILATION_PENDING / INTEGRATION_NOT_AUTHORIZED / NOT_PUBLISHED.

**Rama de trabajo:** ui/eldoria-royal-gold-v2-safe-20261019
**Fuente de partida:** main, SHA cb9e6114543bf7aa85324028cfbd68368b19e83b.
**Escena canónica protegida:** Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity.
**Presupuesto:** 0 EUR. Motor Unity 6000.3.23f1; uGUI existente, URP 17.3.0.

## Por qué existe este trabajo

La interfaz native143 ocupa demasiada superficie de Valoria durante la exploración.
La tarjeta «Ir al objetivo» es grande y el relato se muestra pequeño. El diseño
aprobado es Royal Gold: fantasía medieval seria, azul real y negro azulado, oro
envejecido, emblemas discretos y prioridad al paisaje. El prototipo web original
marcó el listón narrativo; se conservan exactamente los diálogos y reglas reales.

Fuente original de diseño aprobada el 18 de octubre:
ELDORIA_UI_ESPECIFICACION_V1.md, referencia visual
ELDORIA_UI_REFERENCIA_VISUAL.png, SHA-256 conocido
a2b688e147042728fc3586e11bb7793eacf463403d8e8a23ed7fe3613a2d9f98.
La imagen original no se ha regenerado ni falseado: se ha recuperado la
especificación escrita, no la imagen adjunta en bytes, en esta ejecución.
No tomar la imagen de referencia como captura nativa ni copiar VIP, moneda
premium, misiones, acciones o cifras ficticias.

## Entregables en la rama aislada

1. **RoyalGoldV2Hud.cs**, componente de presentación uGUI que utiliza como
   autoridad la interfaz RoyalHUD ya vinculada a ExistingSliceBinding:
   misión actual compacta, abrir capítulo voluntariamente, acceso a la acción
   «Ir al objetivo» ORIGINAL, ficha contextual con scroll, coste/estado/
   requisitos REALES y botón que invoca la oferta EXISTENTE. No modifica
   GameState, IClock, comandos, persistencia, navegación ni perfil activo.
2. **RoyalGoldV2Narrative.cs**, adaptación de tamaño y distribución del
   relato que utiliza el mismo RoyalNarrativePresentation, retrato ya
   autorizado, texto, secuencia de páginas y confirmación original.
3. **RoyalGoldV2PrefabBuilder.cs**, comando editorial manual que podrá
   generar un prefab candidato aparte del antiguo RoyalGoldHUD.prefab,
   además de una escena aislada copiando el TEST original. No se ejecuta solo.
4. Nuevas definiciones de ensamblado y GUIDs .meta de Unity, sin alterar
   ninguna referencia original ni instalar dependencias.

**No terminado:** no hay un prefab serializado generado desde Unity ni
captura nativa de V2 hasta ejecutar el comando anterior en Unity con el
recurso Windows disponible; no se debe decir UI_APPROVED, UNITY_PASS ni
que está dentro del enlace jugable. La nueva capa aún requiere prueba
de localización, safe area, interacción real y rendimiento móvil.

## Decisiones visuales

- La tarjeta de misión en reposo tiene solo capítulo, objetivo actual,
  progreso disponible y dos acciones auténticas. No es una ficha de medio móvil.
- El botón «Ir al objetivo» mantiene el evento oficial existente.
- La lista completa de objetivos es voluntaria; no permanece desplegada.
- La ficha de edificio ocupa un borde, con cuerpo desplazable y
  acción principal siempre visible, nunca permite gastar desde una réplica
  del estado ni crea nuevas funciones.
- Los diálogos ocupan más espacio **solo mientras se lee un evento narrativo**,
  con 29–33 unidades de letra como punto de partida sujeto a captura
  real. El avance de la historia permanece controlado por el presentador
  original y su guardado.
- La cabecera y la navegación conservan los controles efectivos V1 hasta
  que haya oportunidad de compararlos con el arte aprobado. Evitar
  sustituciones ciegas o iconos sin mecánica.
- Sin assets de pago, sin instalación de fuentes y sin shaders costosos.
  Se reutilizan las piezas del kit Royal Gold autorizado.

## Procedimiento de prueba que falta

**No ejecutar mientras el otro chat tenga el runner Windows o archivos
compartidos reservados.** Consultar pipeline/active-workstreams.json y
GitHub Actions actual antes de tocar el equipo.

A. En una copia de Unity 6000.3.23f1 y rama V2, con reserva Windows libre,
   usar menú «Eldoria / Royal Gold V2 / Create isolated reusable prefab»;
   comprobar resultado en Prefabs/RoyalGoldHUDV2.prefab. El constructor
   no debe guardar ni abrir escenas jugables.

B. Ejecutar menú «Eldoria / Royal Gold V2 / Create isolated fixture scene».
   Verificar Preview/RoyalGold_V2_ISOLATED.unity con un solo HUD, cámara,
   EventSystem y ningún script faltante. No tocar el original UI TEST.

C. En PlayMode de esa escena, comparar capturas reales con idénticas
   dimensiones de 1600×900, 900×1600, 1200×900, 20:9 apaisado y safe
   areas simuladas. Imágenes no equivalen a prueba en móvil real.
   Comprobar que los diálogos se leen y no se recortan, la misión es
   compacta en reposo, los paneles se cierran y el scroll funciona.

D. Verificar con la partida de prueba y botones uGUI reales que navegar al
   objetivo NO completa acciones, que el botón construir muestra coste
   y solo ejecuta el gateway original una vez, que el rechazo no cambia
   el guardado, que las etiquetas no interceptan la cámara y que diálogos
   de Aldric y Narrador retienen orden, texto y acuse de finalización.
   Revisar el comportamiento de vuelta/escape en todos los modales.

E. Corregir según capturas, aprobar la referencia visual, coordinar
   integración únicamente después de que acabe la fase gráfica.
   Hacer QA de Bastión I–II y persistencia/volcado previo antes de un
   merge explícito; publicar/actualizar APK/web solo bajo autorización
   separada. No usar cuatro checks verdes para afirmar aprobación visual.

## Reglas de convivencia

Este trabajo no toca main ni los archivos bajo
Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV1/**,
Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/**,
pipeline/active-workstreams.json, CI, Unity/Packages/** o
la escena principal. La rama existe para **preparación aislada**, no
como nueva autoridad de producción ni como excusa para reabrir los
problemas de validación del 19 de octubre.
