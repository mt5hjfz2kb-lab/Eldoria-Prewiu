#!/usr/bin/env bash
set -euo pipefail
TASK_SCRIPT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
TASK_ARGS=("$@")
TASK_ORIGINAL=''
for ((i=0;i<${#TASK_ARGS[@]}-1;i++)); do
  if [[ "${TASK_ARGS[i]}" == '--python' ]]; then
    case "${TASK_ARGS[i+1]##*/}" in
      inspect_source.py|blender_toolbox.py)
        TASK_ORIGINAL="${TASK_ARGS[i+1]}"
        TASK_ARGS[i+1]="$TASK_SCRIPT_ROOT/ground_workshop_entry.py"
        ;;
    esac
  fi
done
if [[ -n "$TASK_ORIGINAL" ]]; then TASK_ARGS+=(--ground-studio-original "$TASK_ORIGINAL"); fi
exec "${ELDORIA_GROUND_BLENDER_BINARY:-blender}" "${TASK_ARGS[@]}"
