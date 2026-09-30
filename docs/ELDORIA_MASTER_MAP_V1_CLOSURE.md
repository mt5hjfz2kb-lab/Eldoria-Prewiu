# Eldoria — Cierre del Mapa Maestro V1

Estado: **CERRADO A NIVEL CONCEPTUAL**
Fecha de cierre: 2026-09-24
Documento base: `docs/ELDORIA_MASTER_MAP.md`

Este cierre forma parte de la documentación canónica del proyecto y resuelve la auditoría final del Mapa Maestro V1. No sustituye el detalle ya consolidado en `ELDORIA_MASTER_MAP.md`; fija cómo deben interpretarse sus apartados abiertos y el corte de transición a Unity.

## 1. Resultado de la auditoría

El Mapa Maestro V1 se considera suficientemente completo a nivel conceptual para dejar de ampliar la visión macro y pasar a planificación ejecutable.

Los sistemas principales ya tienen dirección de producto suficiente: núcleo Valoria→Mundo→Recompensa→Regreso, progresión, mundo persistente, servidores y Madurez del Mundo, La Brecha, Alianzas y territorio, Oráculos, PvP/guerra, Consejo de Valoria, Gobierno del Mundo, Puerto/Muelle, Eventos y expansión indefinida.

No es requisito cerrar ahora todas sus mecánicas finas.

## 2. Reclasificación de los antiguos «DEFINIR ANTES DE UNITY»

Las siguientes materias **NO bloquean el inicio de la migración a Unity** y quedan deliberadamente aplazadas a su fase de diseño mecánico correspondiente:

- protección PvP: reglas concretas para nuevos jugadores, diferencias extremas de poder y recuperación;
- conquista territorial: pérdida, recuperación, territorio desconectado y caída del Cuartel General;
- La Brecha: ciclo global detallado, fases, consecuencias, recuperación, recompensas y relación exacta con la Madurez del Mundo;
- Oráculos: balance, calendario, participación, jefes y recompensas;
- Gobierno del Mundo: acceso/legitimidad exactos, duración, sede, poderes concretos, composición/votación del contrapeso de Alianzas, cargos y destitución.

La dirección macro existente debe preservarse al diseñar estas mecánicas. Que estén aplazadas no significa que se eliminen ni que puedan ignorarse arquitectónicamente cuando condicionen datos o extensibilidad futura.

## 3. Gobierno del Mundo — último acuerdo conceptual

El Gobierno del Mundo será disputable y temporal, ligado a momentos relevantes de la evolución/Madurez del servidor. Una Alianza podrá alcanzar el derecho a gobernar mediante la competición del mundo compartido; la fórmula exacta se diseñará más adelante.

El Gobernante tendrá autoridad real pero limitada. Las decisiones de gran impacto estarán sometidas a un contrapeso ligado principalmente a las Alianzas más fuertes del servidor.

Quedan deliberadamente abiertos para diseño posterior la sede del Gobierno, su posible relación con Valoria, el formato exacto de disputa/conquista, duración, destitución y reglas de votación.

## 4. Estrategia canónica de migración a Unity

La migración se divide conceptualmente en dos etapas consecutivas:

### Etapa 1 — Consolidar el núcleo jugable en Unity

Primero se traslada y consolida el núcleo que el prototipo web está demostrando: Valoria, progresión, mundo, recolección, construcción, tropas/héroes, marcha, combate PvE, recompensas, economía, capítulos/misiones y los sistemas iniciales que formen parte del corte jugable.

El objetivo no es construir Eldoria Online completo desde la primera build de Unity, sino obtener un núcleo sólido, coherente y validable en el nuevo motor.

### Etapa 2 — Mundo multijugador persistente

El siguiente gran paso tras consolidar ese núcleo será activar la capa multijugador persistente: identidad/cuenta, estado autoritativo de servidor, mundo compartido, presencia de otros jugadores y, progresivamente, Alianzas, territorio, PvP y sistemas sociales/globales.

### Regla arquitectónica obligatoria

Aunque el multijugador persistente se active en la segunda etapa, **la Etapa 1 no debe diseñarse como un juego single-player sin futuro online**.

Los modelos de datos y límites de responsabilidad de jugador/reino, recursos, temporizadores, marchas, progresión, coordenadas/entidades del mundo y demás estado susceptible de ser compartido deben poder migrar a autoridad de servidor sin rehacer el núcleo completo.

No se fijan todavía backend, tecnología de red ni arquitectura técnica concreta; eso pertenece al plan de migración/diseño técnico posterior.

## 5. Qué significa «Mapa Maestro V1 cerrado»

Cerrar V1 no significa que todas las mecánicas de Eldoria estén diseñadas. Significa que existe una visión macro suficientemente coherente para:

1. evitar seguir inventando sistemas antes de tiempo;
2. distinguir dirección de producto de mecánica detallada;
3. preparar la migración a Unity sin sobredimensionar la primera build;
4. conservar desde el principio las dependencias del futuro mundo persistente;
5. pasar de visión a planificación ejecutable.

A partir de este cierre, las nuevas decisiones macro deben responder a una necesidad real detectada durante diseño, producción o validación, no a la obligación de completar artificialmente el mapa.

## 6. Siguiente fase

El siguiente bloque de trabajo es **traducir el Mapa Maestro V1 a un plan de diseño y migración ejecutable para Unity**.

Ese trabajo debe definir alcance, fases, dependencias, contratos de datos y criterios de validación antes de iniciar una implementación amplia en Unity. El prototipo web continúa siendo la especificación funcional jugable hasta que su núcleo quede certificado conforme a las reglas vigentes del proyecto.

## 7. Concreción posterior al cierre — núcleo militar v0.32.0

Esta sección **no reabre el Mapa Maestro V1**. Registra una concreción del núcleo militar detectada durante la consolidación jugable y compatible con la dirección ya cerrada para Unity/multijugador.

- En **PvE**, el desgaste de tropas se expresa mediante tropas disponibles y tropas heridas. Las bajas permanentes de tropas son siempre 0.
- En **PvP futuro**, el modelo podrá separar disponibles, heridos y bajas permanentes. Los porcentajes y el balance de bajas PvP siguen aplazados y no se simulan en el prototipo web.
- El **Hospital de Valoria** recupera tropas heridas mediante un proceso temporizado persistente. Nunca recupera bajas permanentes.
- La primera enseñanza jugable del Hospital se integra en Bastión X, después del enfrentamiento contra el Heraldo, como consecuencia natural del combate y sin requisito de recursos que pueda bloquear el cierre del prólogo.
- Una **Marcha** debe conservar identidad propia de propietario, héroes y tropas y puede, arquitectónicamente, pertenecer en el futuro a una operación superior.
- Las futuras **Concentraciones de Alianza** podrán coordinar múltiples contribuciones independientes y preservar para cada participante sus fuerzas, heridos, futuras bajas y recompensas.
- El prototipo web no simula aliados, jugadores remotos ni interfaces falsas de unirse a Concentraciones. La implementación actual solo protege el contrato de datos necesario para no acoplar el núcleo a una resolución exclusivamente individual.

Esta concreción refuerza la regla de la sección 4: el núcleo de la Etapa 1 debe seguir siendo trasladable a una futura autoridad multijugador sin fingir que esa capa ya existe.

