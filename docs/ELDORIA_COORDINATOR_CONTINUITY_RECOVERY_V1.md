# Eldoria — contrato de continuidad de agente → coordinador → Dirección General
2026-10-08. Primera etapa de corrección, **no certificación de autonomía completa**.

## Condición real encontrada
Run 37814990519 falló en maintenance-worker y omitió maintenance-review. Los seis JSON eran válidos; el verificador Windows rechazó falsamente tres. PR #26 tiene un parser corregido, pero falta ejecución Windows del SHA corregido. La definición de intake previa filtraba eventos y registraba comentarios, pero no garantizaba reasignación, reparación, retest ni notificación al usuario.

## Flujo exigido para considerar la orquestación terminada
1. Cada orden tiene owner, id, autorización, recursos, límites, SHA, criterios de aceptación y estado durable, independientes del chat.
2. Coordinator detecta el resultado del run y los artifacts, deduplica por run, mantiene el trabajo ABIERTO si falta una prueba.
3. Si el fallo es transitorio y el SHA permanece válido, puede solicitar un reintento limitado (máximo dos), respetando runner claims e idempotencia. Nunca reintentar indefinidamente un bug determinista.
4. Si es defecto de código/fixture, devolver diagnóstico al agente responsable con contrato acotado y proponer PR. Ejecución automática del SHA reparado requiere workflow autorizado, permisos mínimos y runner libre.
5. Solo el verificador independiente con prueba del SHA exacto concede PASS. El coordinador registra artifact y devuelve resultado a Dirección General; Dirección General actualiza Issue #23.
6. Notificar al propietario por un **canal push real configurado** únicamente al cerrar o cuando una decisión humana sea imprescindible. GitHub issues no envía mensajes automáticos a un chat de ChatGPT cerrado.
7. Ante bloqueo, conservar claim según la política canónica; prohibido declarar cierre ficticio o cambiar Unity/arte sin autorización.

## Alcance de esta primera PR
La nueva automatización recibe **fallos** del workflow local, consulta sus jobs, los identifica de forma idempotente y deja un handoff explícito en Issue #25 (mantenimiento) o #23 (piloto). Los runs skipped no cuentan como fallos; no duplica reportes por el mismo run. No autocorrige código ni lanza nuevos builds: hacerlo sin permisos y alcance validado podría corromper el juego.

## Prueba pendiente
Tras merge, ejecutar fallo controlado nuevo del workflow dentro del piloto autorizado; confirmar comment único y enlace SHA/run, corregir con agente, ejecutar en Windows el SHA reparado, revisar evidencia y verificar handoff a Dirección General. Confirmar aparte el canal push del propietario. **Hasta demostrar todo, coordinación extremo a extremo sigue NO PASS.**
