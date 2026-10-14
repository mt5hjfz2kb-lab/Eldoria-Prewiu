#!/usr/bin/env bash
# Isolated Debian extraction; no system installation and no shared Windows runner.
set -euo pipefail
: "${ELDORIA_LOCAL_BLENDER_ROOT:?Set the isolated extracted Blender root}"
export LD_LIBRARY_PATH="$ELDORIA_LOCAL_BLENDER_ROOT/usr/lib:$ELDORIA_LOCAL_BLENDER_ROOT/usr/lib/x86_64-linux-gnu${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
export BLENDER_SYSTEM_SCRIPTS="$ELDORIA_LOCAL_BLENDER_ROOT/usr/share/blender/scripts"
export BLENDER_SYSTEM_DATAFILES="$ELDORIA_LOCAL_BLENDER_ROOT/usr/share/blender/datafiles"
export PYTHONPATH="$ELDORIA_LOCAL_BLENDER_ROOT/usr/lib/python3/dist-packages:/usr/lib/python3.12/lib-dynload${PYTHONPATH:+:$PYTHONPATH}"
exec "$ELDORIA_LOCAL_BLENDER_ROOT/usr/bin/blender" "$@"
