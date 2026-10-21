# Resumen operativo existente

`node tools/work-status/report.mjs` consulta las autoridades locales; `--json` ofrece la misma información estructurada. En GitHub, `Eldoria concise work status` añade observaciones de Actions a Summary y al artefacto existente. No despacha builds, cambia estados, publica, cancela jobs ni reserva Windows. Sus permisos siguen siendo de lectura.

Al iniciar un encargo, añadir a su propia entrada del registro existente, sin crear otro registro:

```json
"delivery_definition": {
  "deliverable": "Resultado concreto que debe recibir el usuario",
  "evidence": "Pruebas y evidencia requeridas para ese resultado",
  "handoff": "Artefacto o cambio verificado que se entregará al cerrar"
}
```

Estos campos son opcionales para compatibilidad; su ausencia se muestra explícitamente. No rellenarlos por otro propietario ni interpretar una definición como autorización adicional. Los contratos y gates actuales siguen vigentes.

El informe conserva por separado HEAD de desarrollo, revisión candidata, QA técnica registrada para esa misma revisión, última revisión WebGL con confirmación remota registrada y versión del canal Android habilitado. No comprueba de nuevo el host ni un teléfono, no aprueba arte y no equipara un canal Android con una versión instalada. `technical_qa_recorded` consulta únicamente los dos indicadores técnicos del manifiesto y exige coincidencia de revisión; no certifica todos los gates de entrega.

Para medir sin añadir procesos pesados, se consulta como máximo el run más reciente cuyo nombre indique native/Unity/Android/WebGL/progression entre los últimos 20 runs de cada rama activa, con hasta 100 jobs del último intento. Cada muestra conserva run, intento y SHA, incluso cuando pertenece a una revisión anterior. Los checks del HEAD se presentan aparte. Un fallo de API se muestra como desconocido o parcial.

Las marcas de cada job proporcionan espera desde su creación hasta el inicio y tiempo transcurrido desde el inicio. La espera no incluye todo el tiempo desde que el usuario pidió el trabajo ni las dependencias anteriores a la creación del job. Los pasos completados se agrupan por nombre en preparación, compilación, pruebas, entrega y otros: son estimaciones, no instrumentación interna de Unity. Los pasos omitidos, incompletos o sin marcas válidas no aportan segundos; JSON informa cuántos no se midieron. Un cero medido y un dato desconocido son diferentes. Los huecos entre pasos no se asignan artificialmente y los tiempos de jobs paralelos no se suman como duración total.

La vista de jobs Windows en ejecución consulta Actions, no todos los procesos del PC. Ninguna cola vacía ni error de API equivale a permiso para ocupar el runner. El modo local carece de estas observaciones remotas.

Comprobación: `node --test tools/work-status/report.test.mjs`.
