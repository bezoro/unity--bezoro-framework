#!/usr/bin/env bash
# Keep GameCI's activation/return scope intact; launch only its requested test mode.
set -euo pipefail
editor="$1"
shift
workspace="${GITHUB_WORKSPACE:?}"
marker="$workspace/artifacts/unity-failed"
helper="$workspace/.github/scripts/framework_unity_ci.py"
if [[ -f "$marker" ]]; then
  echo 'An earlier native Unity launch failed; blocking later Editor launches.' >&2
  exit 1
fi
mode=''
report=''
log=''
read_next=''
arguments=()
for argument in "$@"; do
  case "$read_next" in
    mode) mode="${argument,,}" ;;
    report) report="$argument" ;;
    log) log="$argument" ;;
  esac
  read_next=''
  case "$argument" in
    -testPlatform) read_next=mode ;;
    -testResults) read_next=report ;;
    -logFile) read_next=log ;;
    -nographics) continue ;;
  esac
  arguments+=("$argument")
done
fail_run() {
  printf '%s\n' "$1" > "$marker"
  if [[ -n "$log" ]]; then python3 "$helper" diagnostics --log "$log" || true; fi
  exit "$1"
}
case "$mode" in editmode|playmode) ;; *) fail_run 1 ;; esac
case "$mode" in
  editmode) assembly='Bezoro.Unity.Core.Tests.EditMode' ;;
  playmode) assembly='Bezoro.Unity.Core.Tests.PlayMode' ;;
esac
expected="$workspace/artifacts/$mode-results.xml"
[[ "$report" == "$expected" ]] || fail_run 1
code=0
xvfb-run -a "$editor" "${arguments[@]}" -force-glcore -assemblyNames "$assembly" || code=$?
if (( code != 0 )); then fail_run "$code"; fi
python3 "$helper" verify-mode --mode "$mode" --report "$report" || fail_run 1
