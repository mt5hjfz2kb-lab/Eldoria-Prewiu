# Bastión III — migración canónica en curso

Encargo del propietario 27-10-2026. Base entregada Android 1.16: `ee35c87986370bf4d7e0e62e57897957cd9bce98`. Recursos liberados y reclamados en el mismo workstream integrado; no modifica la entrega code16.

## Contrato

Autoridad: `docs/BASTION_III_FISSURE_CANON_20261027.md`, tabla I–X y comportamiento real `v0220`. La continuidad antigua que situaba Lyra antes de III está superada. Ascenso III: 650 madera/500 piedra, 14 segundos, cuartel y 41 tropas. Granero: 240/180, 9 segundos. Investigación: 1200 segundos persistentes. Contacto no es reclutamiento. Victoria entrega 300 piedra, 80 XP Lyra y manuscrito único, además de recompensas de misión originales; cierre local de 6,5 segundos, nunca respawn de fisura. Brecha principal intacta.

Conservar IDs, esquema de guardado 1 y sesión única. No representar un portal tecnológico ni convertir fisura en recurso renovable. Los cambios grandes de edificios nivel5 están fuera de este encargo. Cada nivel exige diferencia visible moderada y A/B real.

## Implementación y pruebas

Datos aditivos `ChapterThree`; comandos del gateway conservan propietario/revisión/idempotencia. Prueba ejecutable `tools/bastion-iii/DomainProbe.cs` compila las fuentes reales sin Unity; cubre ascenso, 20 minutos, reinicio, reclutamiento tras victoria, recompensa única, caza, límite de niveles y aislamiento de snapshot. No sustituye Unity ni QA visual.

PENDIENTE: integración gráfica/narrativa completa, navegación, Arcón/héroes, comparación A/B real en cámara jugable, regresión I–II y pruebas Unity del candidato exacto. No integrado, no ART_APPROVED, no nueva APK.

## Decisión artística inicial

Revisados los tres catálogos. `Valoria_Granero_BIII_v1` es fuente reutilizable, no aprobación de escena. `Granero_CFSv1` rechazado por calidad procedural; no recuperar. RCF es fuente rica pero el ensayo histórico falló por composición/crop móvil y no fue promovido. Evaluar un único volumen principal de esa familia contra la Valoria actual, evitando importar el conjunto de tres edificios y duplicar superficie. Fuente editable y pruebas históricas disponibles; debe verificarse antes de escoger.
