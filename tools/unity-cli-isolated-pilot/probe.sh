#!/usr/bin/env bash
set -Eeuo pipefail
# Isolated GitHub-hosted Ubuntu CLI proof. No Unity Editor, no Windows runner.
umask 077
: "$RUNNER_TEMP"
: "$GITHUB_OUTPUT"
RESULT="$RUNNER_TEMP/eldoria-cli-proof"
mkdir -p "$RESULT"
export UNITY_NO_CLOUD=1
# Current Unity CLI release stream is beta; stable latest.json returned HTTP 404 in run 41465891072.
export UNITY_CLI_CHANNEL=beta
export UNITY_CLI_HOME="$RUNNER_TEMP/eldoria-cli-binary"
export PATH="$UNITY_CLI_HOME/bin:$PATH"
printf '%s\n' 'Unity CLI installation probe' > "$RESULT/notes.txt"
curl --fail --location --retry 2 --connect-timeout 15 --max-time 90 \
  https://unity.com/install.sh --output "$RESULT/unity-official-install.sh"
sha256sum "$RESULT/unity-official-install.sh" > "$RESULT/installer.sha256"
# Unity's own public installer in isolated CI temp only, not on the user's PC.
timeout 240 bash "$RESULT/unity-official-install.sh" > "$RESULT/installer.log" 2>&1
if ! command -v unity >/dev/null; then
  echo 'No unity executable on the isolated runner' >&2; exit 2
fi
timeout 25 unity --version > "$RESULT/version.txt" 2>&1
timeout 25 unity --help > "$RESULT/help.txt" 2>&1
set +e
timeout 25 unity editors --format json > "$RESULT/editors.json" 2> "$RESULT/editors-error.txt"
editors_rc=$?
timeout 25 unity pipeline list > "$RESULT/pipeline-list.txt" 2>&1
pipeline_rc=$?
set -e
# Inspect the official agent integration repository read-only.
git clone -q --depth=1 https://github.com/Unity-Technologies/unity-agent-plugin.git "$RESULT/plugin-source"
git -C "$RESULT/plugin-source" rev-parse HEAD > "$RESULT/plugin-sha.txt"
find "$RESULT/plugin-source/.codex-plugin" -maxdepth 2 -type f -printf '%P\n' | sort > "$RESULT/codex-plugin-files.txt"
test -s "$RESULT/codex-plugin-files.txt"
# Prevent the large source checkout from being included in the proof artifact.
rm -rf "$RESULT/plugin-source"
python3 - "$RESULT" "$editors_rc" "$pipeline_rc" <<'PY'
import json, pathlib, sys, platform
root=pathlib.Path(sys.argv[1])
data={
 "environment":"isolated GitHub-hosted Ubuntu runner",
 "host_os":platform.platform(),
 "unity_cli_version_stdout":(root/"version.txt").read_text().strip()[:500],
 "unity_help_present":(root/"help.txt").stat().st_size>50,
 "editor_list_exit_code":int(sys.argv[2]),
 "pipeline_list_exit_code":int(sys.argv[3]),
 "plugin_repo_sha":(root/"plugin-sha.txt").read_text().strip(),
 "codex_plugin_manifest_files":(root/"codex-plugin-files.txt").read_text().splitlines(),
 "no_windows_used":True,
 "no_unity_editor_installed":True,
 "no_unity_project_modified":True,
 "no_codex_editor_control_demonstrated":True,
 "art_approved":False,
 "next_required_step":"Run the Unity Editor and install Pipeline in an isolated copy with exclusive native resource permission"
}
(root/"proof.json").write_text(json.dumps(data,indent=2),encoding="utf-8")
print(json.dumps(data,indent=2))
PY
echo "result=verified-cli-only" >> "$GITHUB_OUTPUT"
