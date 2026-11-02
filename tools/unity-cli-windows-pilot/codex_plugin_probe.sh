#!/usr/bin/env bash
set -Eeuo pipefail
test -n "$RUNNER_TEMP"
ROOT="$RUNNER_TEMP/eldoria-codex-plugin-$GITHUB_RUN_ID"
mkdir -p "$ROOT/home" "$ROOT/codex"
export HOME="$ROOT/home"
export CODEX_HOME="$ROOT/home/.codex"
mkdir -p "$CODEX_HOME"
export PATH="$ROOT/codex/node_modules/.bin:$PATH"
unset OPENAI_API_KEY CODEX_API_KEY
printf '%s\n' 'NO agent model calls or API auth permitted in this test' > "$ROOT/policy.txt"
npm install --prefix "$ROOT/codex" --no-audit --no-fund '@openai/codex@0.166.0' > "$ROOT/npm-install.log" 2>&1
codex --version > "$ROOT/codex-version.txt" 2>&1
timeout 120 codex plugin marketplace add Unity-Technologies/unity-agent-plugin > "$ROOT/marketplace.log" 2>&1
timeout 120 codex plugin add unity@unity-agent-plugin > "$ROOT/plugin-install.log" 2>&1
timeout 40 codex plugin list > "$ROOT/plugin-list.txt" 2>&1
grep -F 'unity@unity-agent-plugin' "$ROOT/plugin-list.txt" >/dev/null
set +e
timeout 12 codex login status > "$ROOT/codex-login-status-internal.txt" 2>&1
LOGIN_RC=$?
set -e
# Do not upload login output: may contain user identifiers.
rm -f "$ROOT/codex-login-status-internal.txt"
export ELDORIA_CODEX_LOGIN_EXIT_CODE="$LOGIN_RC"
python - "$ROOT" <<'PY'
import json,pathlib,sys
root=pathlib.Path(sys.argv[1])
ver=(root/'codex-version.txt').read_text().strip()
items=(root/'plugin-list.txt').read_text()
result={
  'environment':'GitHub-hosted Ubuntu, ephemeral HOME/CODEX_HOME',
  'codex_cli':ver,
  'unity_plugin_declared_installed': 'unity@unity-agent-plugin' in items,
  'plugin_status_excerpt':items[:900],
  'tested_model_or_authenticated_session':False,
  'no_chatgpt_api_or_model_calls':True,
  'auth_status_command_executed':True,
  'auth_status_exit_code':int(__import__('os').environ.get('ELDORIA_CODEX_LOGIN_EXIT_CODE','255')),
  'authenticated_model_session_observed':False,
  'no_windows_or_unity_editor_used':True,
  'no_game_changes':True,
  'no_new_paid_subscription':True,
  'next_validation':'Codex authenticated interactive agent must use plugin in live editor to establish complete Codex-to-Unity session'
}
(root/'proof.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result,indent=2))
PY
