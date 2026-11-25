# Eldoria Development Studio — piloto aislado

Estado: conexión mínima real verificada el 25-11-2026; calidad de los perfiles y generación de imágenes pendientes de prueba real. No requiere Codex ni Unity. No modifica el juego, no publica, no realiza llamadas automáticas ni usa Windows. Solo Python 3 estándar.

## Seguridad y prueba

- Usar la clave del proyecto Eldoria Development Studio mediante el secreto de Actions OPENAI_API_KEY o una variable de entorno de una sesión local privada. Nunca pegarla en código, chat, parámetros de CLI ni logs. No buscar claves en discos ni usar credenciales ajenas. El repositorio es público: no subir evidencias privadas ni publicar informes de análisis sensibles como artefactos.
- Pruebas sin credenciales: `python3 tools/openai-development-studio/studio.py --self-test` y `python3 tools/openai-development-studio/generate_candidate.py --self-test`.
- Prueba de conexión (una llamada, no modifica Unity): `python3 tools/openai-development-studio/studio.py smoke --live --out /tmp/eldoria-api-proof.json`. Omitir --live es un ensayo seco sin red.
- La ejecución manual por Actions necesita que el workflow exista en la rama predeterminada y que un administrador configure el secreto. Esta rama no toca main; el secreto se verificó mediante una única ejecución explícitamente autorizada en la rama piloto. No habilitar ninguna llamada automática.
- Los errores 401/403 indican autenticación o permisos; 429 puede indicar límites o saldo. No se imprime el cuerpo de error ni se reintenta automáticamente. Un timeout no demuestra consumo cero.
- Los modelos utilizados están documentados en https://developers.openai.com/api/docs/models y los precios son estimaciones según la tarifa estándar sin caché; consultar facturación real. No hay un límite monetario global ni se verifica aquí el saldo promocional. El límite por llamada es de tokens/archivos, no de dólares; no añadir medios de pago sin autorización. No ejecutar lotes hasta confirmar la prueba, consumo y calidad.

## Roles (herramientas de consulta, no agentes autónomos con acceso de escritura)

Ejemplos locales, deliberadamente sin --live; añadirlo únicamente para autorizar una llamada y escribir el resultado.
- Dirección artística: `python3 tools/openai-development-studio/studio.py visual --image captura-real.png --image referencia.png --out informe-visual.json`.
- Auditor técnico: `python3 tools/openai-development-studio/studio.py technical --evidence log-sanitizado.txt --out auditoria.json`.
- Programador: `python3 tools/openai-development-studio/studio.py programming --evidence codigo-relevante.txt --out propuesta.json`.
- Coordinador: `python3 tools/openai-development-studio/studio.py coordinator --evidence encargo-y-evidencias.txt --out cierre.json`.
- Generación de candidato visual: `python3 tools/openai-development-studio/generate_candidate.py --brief brief.txt --out-dir candidato-nuevo`. Una imagen 1024² de calidad baja para probar acceso, sin importación ni aprobación artística; recibo de uso si la API lo devuelve.

La entrada debe ser explícita y revisada; la detección básica de secretos no sustituye una revisión de logs. Las imágenes se envían a la API, no se descargan desde URLs remotas. Las respuestas son propuestas, no pruebas de ejecución; revisar antes de aplicar. Conservar fuentes, capturas Unity reales, referencia aprobada y revisión independiente. No reemplazar el QA existente, ni reintroducir Qwen.

## Próximos gates

1. Pruebas offline en Ubuntu, sin uso de Windows ni créditos.
2. Secreto seguro y solicitud real mínima: superados; falta contrastar el recibo con el panel de uso.
3. Evaluar un par de capturas genuinas de la misma cámara y contrastar observaciones con QA existente.
4. Revisar seguridad, calidad, coste medido y titularidad del workstream antes de integrar el workflow en main. No abrir PR duplicada ni fusionar sin la reclamación canónica correspondiente. No programar agentes con acceso de escritura sin un gate separado.

## Evidencia del 25-11-2026

- [Ejecución de conexión real](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45670034719), commit `15bad5acaa909a38c59e351ca70e0a17caa5fd99`. Los dos grupos de pruebas offline y la llamada real a GPT-6 Luna pasaron. El log registra `API_RESPONSE_OK` y coste estimado de 0,000007 USD. Artefacto de recibo 15049860461 (retención de 7 días).
- El disparador excepcional de una sola llamada se retiró en `bbfa2423067ef1bf5ce1f4233687eac3800bec42`. Los pushes posteriores solo ejecutan pruebas offline. No se ha verificado la factura real ni se ha modificado main o Unity.
