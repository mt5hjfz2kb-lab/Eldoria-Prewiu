# Android1.18 — entrega de PR120 y PR122

Publicada y disponible en el canal family18.
[Descargar APK1.18](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code18/Eldoria-Valoria-code18.apk).

PR120 y PR122 están integradas. La validación compartida40789374485 pasó
47/47EditMode y73/73PlayMode, con12capturas finales de Aldric abiertas
e inspeccionadas individualmente contra su referencia original. Las19
vistas nativas de los sistemas120 y sus pruebas se conservan en su
aceptación original. Main40795797367 volvió a pasar47/73, compiló Windows
y completó las capturas del editor. El límite externo de PlayMode se
alineó a15min sin cambiar ningún test ni sus exigencias.

## Identidad exacta de la entrega

| Campo | Valor |
|---|---|
| Versión |1.18 / código18|
| Paquete |com.eldoria.valoria.v5test|
| Compilación firmada |[40798180869](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40798180869)|
| Fuente APK |0f150e6a719f93d312ebb18d1ceac5edd7435ba9|
| Publicación |[40801382525](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40801382525)|
| Activo público |686976107 / Eldoria-Valoria-code18.apk|
| APK bytes |458.971.568|
| APK SHA256 |7396c5a9e879a2566b99663e78169c4895ade569c5e528f9935f3186e3b0d644|
| Certificado original SHA256 |f479ca92ccfa256a991936b23d0153ffad247b352a8a5ed569457db8274443d8|
| ZIP de origen |Artefacto12864609668;445.956.695bytes|
| ZIP SHA256 |9c3aaf77fc6cff494d9ebaa21bf20b6772ab24b5d7efe655118f377c6627ba6b|

El SDK verificó firma criptográfica, paquete, versión, ARM64 y permisos.
Se descargó el artefacto y se comprobaron sus bytes y la APK. El certificado
APKv2 se extrajo además independientemente y coincide con el original.
El publicador descargó el activo PÚBLICO y lo comparó byte a byte antes
de actualizar el canal. La API pública confirma tamaño y SHA256 idénticos.

El modelo aprobado está dentro de la APK, comprobado directamente:
assets/Eldoria/Aldric/SirAldric_Approved_RiggedAnimated_v1.glb,10.140.372bytes,
SHA256c28a65b6040225256ae047d8fd773849771895ac378041e0882f279517e78c8b.
El GLB legado vetado no está en el paquete. El constructor usa exclusivamente
Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity.
Las imágenes del benchmark genérico antiguo no se presentan como aceptación
visual de esta escena; corresponden a otra comprobación histórica.

## Estado y límites

Recompensas provisionales, como autorizó el propietario. Mismo paquete
y certificado, sin borrado de datos solicitado. Sin prueba ni perfilado
de teléfono físico; las capturas verticales no se presentan como esa prueba.
No se afirma identidad de píxeles entre cámara/iluminación estáticas de una
referencia y animaciones de la ciudad. Es reutilización del modelo aprobado,
no un nuevo ciclo de autoría artística.

Las solicitudes de construcción y publicación quedan desactivadas tras la
entrega para evitar repeticiones. Reserva Windows liberada, revisión51
del workstream principal; los encargos124/125/126 permanecen intactos.
