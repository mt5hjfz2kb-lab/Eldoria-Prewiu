# Valoria 2.5D — pruebas de interacción y control artístico

Fecha: 2026-11-17. Rama: `art/ciudad-2d-hitos-1-5-20261116`.

## Contrato para no confundir pruebas

| Evidencia | Qué verifica | Qué no verifica |
| --- | --- | --- |
| `QA/validate_*.py` | texturas, SHA, distribución y contenido de código | ejecución real de Unity |
| Chromium/WebKit | gestos y controles del inspector web | la implementación Unity |
| `City2DNativeEvidence.Capture` | la escena real en Unity Editor (RenderTexture) y progresión por snapshots | PlayMode, toques físicos en iPhone |
| `QA/PlayModeProof/City2DRealPlayModeTests.cs` | entrada real a PlayMode, escena aislada, EventSystem, raycast físico, botón del HUD, cámara en retrato/horizontal, aplicación sin escritura de `PlayerState` | pantalla táctil real, Apple GPU o performance móvil |
| Dispositivos iOS/Android | memoria, input, rendimiento, fidelidad y estabilidad | pendiente |

### Lecciones verificadas y correcciones

1. Unity 6000.3.23f1 usa `com.unity.test-framework 1.6.0`. En el `.asmdef` editor-only **no** añadir referencias manuales a `UnityEngine.TestRunner` / `UnityEditor.TestRunner` si `optionalUnityReferences` incluye `TestAssemblies`: provoca error de referencias duplicadas en la compilación del editor. Esto se observó en la ejecución 44026959806 y se corrigió.
2. El código de producción permanece en `Assembly-CSharp`, que una assembly de tests no puede referenciar directamente. Para mantener el aislamiento, los tests usan reflexión sobre los componentes **reales** de la escena guardada y usan `EditorSceneManager.LoadSceneAsyncInPlayMode`. Esta API permite cargar una escena que no está en Build Settings. La primera llamada a `City2DNativeEvidence.Capture` genera la escena sandbox en el checkout desechable.
3. `City2DProgressionPolicy` es de lectura y no ejecuta comandos ni escribe guardados. Desbloqueo: Aserradero I, Cuartel II, Granero III, Cantera productiva V. Salón de Héroes no tiene contrato de parcela ni nivel persistido, por lo que sigue bloqueado.
4. La variante `City2DGroundingBake` está *sin aprobar*: solo se genera en el proyecto aislado, nunca sustituye el sprite original. Disminuye alfa en los bordes de la plataforma inferior sin alterar los otros píxeles; la inspección local de las 4 fuentes comprueba que conserva más del 97% de los píxeles opacos. La prueba nativa A/B debe determinar si mejora el acabado **visual**, no solo si compila.
5. No confundir un resultado `success` de GitHub Actions con calidad premium: el candidato anterior del Cuartel desaparecía pese a una ejecución técnica correcta. Los criterios de aprobación deben incluir comprobación humana de capturas, independencia de elementos y coherencia de materiales/iluminación.

## Puertas de aceptación para este pase

- La ejecución `ciudad2d-native-proof.yml` debe incluir 15 PNG con SHA y un resultado NUnit XML con al menos un test completado; no basta el retorno 0 del Editor.
- Capturas válidas deben mostrar Cuartel, Aserradero, Granero en sus etapas y Cantera **solo construida** tras completar su obra.
- No aprobar la variante Grounded si a 1170×2532 introduce halos, césped sobre muros, desapariciones, patas flotantes o pérdida de nitidez.
- No incorporar soluciones gráficas al juego actual hasta contar con capturas, test táctil y presupuesto de memoria por plataforma.
