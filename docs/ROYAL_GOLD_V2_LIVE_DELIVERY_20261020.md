# Eldoria — Royal Gold V2 integrada y publicada para pruebas · 20-10-2026

## Estado final verificado

- **Autoridad de juego:** `main`, escena integrada `Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity`, Unity 6000.3.23f1.
- **Interfaz:** un solo `RoyalHUD` y una sola sesión; nueva capa Royal Gold V2 con objetivo compacto, ficha contextual con scroll, navegación inferior reescalada y diálogos cinematográficos de letra ampliada. Botones reales delegan al gateway/comandos existentes; sin inventar costes, mecánicas, monedas, NPCs, partidas ni guardados.
- **Correcciones regresivas incorporadas:** el diálogo de Sir Aldric ya no se recorta; los botones uGUI dinámicos conservan su registro de raycast; la ficha contextual no tapa «Mundo» en vertical; el botón «Ir al objetivo» permanece accesible cuando hay un recurso o edificio seleccionado.
- **Unity nativo:** run [39438288326](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39438288326), `SUCCESS`. Artefacto exacto WebGL `valoria-integrated-web-playtest-39438288326`, fuente `2642d3e10d7924fa61c537a93c19e71fd13d1eee`.
- **Prueba de navegador real:** run [39438406183](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39438406183), `SUCCESS` en Windows Chromium GPU con SystemClock y controles auténticos; artefacto de evidencia [12274010133](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39438406183/artifacts/12274010133). `report.json` = PASS; Bastión I–II terminado, 10 operaciones, 5 marchas y 20 revisiones de encuadre, capturas/vídeo auténticos, cuatro direcciones y zoom, accesos reales, guardado/recarga iguales y cero errores de ejecución o red.
- **Paquete de distribución:** Windows GPU QA artefacto [12273740274](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39438406183/artifacts/12273740274), SHA-256 `5e2d674a77e836133fa0a13a2279de90732f175b2116c0dd46d38259efbbab05`. Preparación exacta [39440643475](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39440643475), PASS.
- **Publicación de pruebas:** itch.io Restricted, [Eldoria pruebas](https://chrismad.itch.io/eldoria-pruebas). GitHub run [39440869260](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39440869260): Butler upload ACCEPTED, build remoto `2169803` (patch 175,55 MiB). La verificación independiente por Butler `status` en run [39441475526](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39441475526), reintento remoto a las **14:45:53 UTC**, confirmó `✓ #2169803 (from #2165127) | native191-97997478971c`. Versión anterior native152 desplazada del canal activo; conservar fallback Sites native143.
- **Límites honestos:** no se ha certificado que la página Restricted cargue y permita jugar en Safari físico/iPhone actual. Tampoco hay aceptación artística de ciudad/mundo (metas gráficas 7/10 y 5,5/10 siguen independientes); los cambios de interfaz no acreditan mejora de assets 3D. Los checks art-learning del guard de arquitectura tienen historial de fallos independiente y NO invalidan la prueba funcional de escena/navegador.
- **Coste:** 0 EUR, sin créditos de pago ni servicios visuales externos añadidos.

## Alcance protegido y continuidad

No se reactivaron ni redibujaron las cuatro parcelas visibles expresamente revocadas por el propietario, ni se añadieron barrios, campamentos, acantilados u otros artefactos artísticos. No se cambió la economía, la narrativa, el avance de Bastión I–II ni el perfil de guardado. Las fuentes y el formato de los envíos se conservan en GitHub con SHAs.

Para ver y probar **la V2 publicada**, usar el proyecto privado de itch.io con la cuenta autorizada. La versión anterior Sites sigue en `https://valoria-city-world.kbkvc5c45n.chatgpt.site` como fallback histórico, NO afirmar que su URL refleja la UI V2.

Antes de marcar `PHYSICAL_MOBILE_PASS` exigir apertura real de itch Restricted en un iPhone/Android, diálogos, navegación y guardado/recarga. No asumir que el navegador de escritorio sustituye al teléfono ni que Butler `status` prueba la página web.
