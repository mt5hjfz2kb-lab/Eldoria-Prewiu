# SLM preflight de instalación permanente (sin tocar Unity)

La instalación debe **fallar** si no existe una reserva registrada y exclusiva del ejecutor Windows para el trabajo `eldoria-slm-permanent-bootstrap-20261104`, o si el ejecutor tiene otro propietario. No se permite deducir una liberación por una ejecución de Actions finalizada.

Las pruebas Python del paquete local terminaron **6/6 PASS** en un entorno independiente. La sintaxis PowerShell y la instalación Windows todavía **no se han ejecutado ni certificado**. No llamar a `slm connect` sobre el perfil de Codex del usuario hasta confirmar su identidad y los permisos. El acceso web requiere autorización personal y no puede automatizarse en un repositorio público.
