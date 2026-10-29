# Aldric animado en la ciudad — PR122

Estado actual: integración nativa completa aprobada:47/47EditMode y
73/73PlayMode, sin fallos ni omitidos;12capturas finales inspeccionadas.
El historial de intentos y diagnósticos se conserva en
[NATIVE_ATTEMPT_HISTORY.md](NATIVE_ATTEMPT_HISTORY.md).

## Resultado y alcance

Una única instancia visual de Sir Aldric pasea por el pavimento real de
Valoria a escala humana1,85m. Combina Idle/Walk originales y pausas en los
hitos. El apoyo se comprueba sobre el MeshCollider auténtico mediante
contactos que rodean los pies, junto con la cápsula corporal completa.
La ruta se valida cada2,5cm para detectar las juntas3–5cm entre las losas.
No se modifica la escena/cámara de producción ni se crea otra entidad
lógica de héroe o un segundo host.

Aldric se oculta en Mundo, marcha real e investigación de la fisura.
Al regresar retoma el tramo interrumpido y continúa caminando. La recarga
conserva una investigación en curso. El caminante solo consulta el gateway;
la prueba exige que el paseo no cambie el estado del jugador.
Las recompensas continúan provisionales, según la orden del propietario.

## Fuente aprobada e importación

- GLB original:10.140.372bytes,26huesos,10mallas,5clips originales.
- SHA256:`c28a65b6040225256ae047d8fd773849771895ac378041e0882f279517e78c8b`.
- Archivo:`Unity/Assets/StreamingAssets/Eldoria/Aldric/SirAldric_Approved_RiggedAnimated_v1.glb`.
- Referencia original:[cuatro vistas](reference-approved-model-4views.png),
  con procedencia/hash en[su manifiesto](reference-approved-model-4views.json).

El original carece de NORMAL y glTFast6.14.1 rechazaba la combinación
Position/Tangent. Se añaden normales geométricas verificadas solo a una
copia transitoria en memoria, conservando todos los bytes originales y
las demás propiedades. SHA256transitorio:
`d64f765625340365ad760e7e8d0639d640ebdd828d55c0635b3aefc0c974081b`.
[normal-import-proof.json](normal-import-proof.json) acredita equivalencia
de fuente; no debe interpretarse como prueba de Unity por sí solo.

La alineación de reposo de ciudad es una capa aditiva técnica en memoria
de los brazos. El GLB, rig, pesos, materiales y cinco clips fuente se
conservan. No hay otro modelo, generación externa ni créditos consumidos.

## Evidencia actual

Ejecución[40789374485](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40789374485),
fuente`e47c3bb525439949c19227673e66c88730d0349b`.
El caso completo de Aldric ha pasado, con12imágenes auténticas revisadas:
reposo/marcha en horizontal/vertical, Mundo/marcha/investigación/retorno
y cuatro detalles de cara/armadura/capa. Se coteja el modelo aprobado
en la luz y cámara reales de la ciudad; no se afirma identidad de píxeles
con una referencia estática de otra cámara o iluminación.

La batería compartida completa ha pasado:47EditMode y73PlayMode, cero
fallos/omitidos. Las12capturas FINALES se han abierto e inspeccionado
individualmente. [Certificado e identidades](NATIVE_CERTIFICATE_20261029.json)
y [capturas originales](native-40789374485/) conservan los bytes auténticos.
El mismo certificado solo se reutiliza si el árbol Unity y TODO el job
nativo ejecutado siguen idénticos; ante cualquier duda se ejecuta completo.

## Límites de la evidencia

Es reutilización del modelo aprobado y validación de su integración,
no un nuevo ciclo de autoría artística. `art_approved` permanece falso.
No hay prueba ni perfilado en teléfono físico; ninguna captura vertical
se presenta como certificación de Android/iOS o rendimiento móvil.
