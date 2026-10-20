# Regresión de iluminación detectada al reabrir

Native236 run39494769651 SUCCESS técnico, no aceptado como acabado final. Su BEFORE muestra las terrazas oscuras y el resplandor horneado de antorchas, a diferencia de native233/234 antes del guardado. La asignación lightmapIndex=-1 del renderer anterior no evita su asociación por LightingDataAsset tras reapertura. La geometría no cambió.

Corrección: conservar los dos MeshRenderer originales deshabilitados (sin romper referencias), colocar la MISMA malla y los MISMOS 19 materiales en hijos de transformación identidad, con nuevos IDs sin atlas, LightProbes explícitos. Reutilizar cuatro luces originales sin sombras adicionales. No borrar LightingDataAsset ni rehornear toda la escena.

Integración debe guardar, REABRIR y comparar otras seis capturas con el candidato aceptado antes de las 40 comprobaciones nativas. Si ese gate falla no se integra. La superficie de caminos es un acabado secundario; no se atribuye geometría nueva a native236.
