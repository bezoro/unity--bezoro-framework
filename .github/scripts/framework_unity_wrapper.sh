#!/usr/bin/env bash
# Invoked by the pinned GameCI Docker scripts inside their activation/return scope.
set -euo pipefail
editor="$1"
shift

# Activation and license-return commands keep their exact original arguments.
test_invocation=false
for argument in "$@"; do
  case "$argument" in -runTests|-testPlatform|-testResults) test_invocation=true ;; esac
done
if [[ "$test_invocation" != true ]]; then
  exec "$editor" "$@"
fi

workspace="${GITHUB_WORKSPACE:?}"
marker="$workspace/artifacts/unity-failed"
helper="$workspace/.github/scripts/framework_unity_ci.py"
mode=''
report=''
log=''
arguments=()
fail_run() {
  printf '%s\n' "$1" > "$marker"
  if [[ -n "$log" ]]; then python3 "$helper" diagnostics --log "$log" || true; fi
  exit "$1"
}
if [[ -f "$marker" ]]; then
  echo 'An earlier native Unity launch failed; blocking later test Editor launches.' >&2
  exit 1
fi
while (( $# > 0 )); do
  argument="$1"
  shift
  case "$argument" in
    -testPlatform)
      (( $# > 0 )) || fail_run 1
      mode="${1,,}"
      shift
      ;;
    -testResults)
      (( $# > 0 )) || fail_run 1
      report="$1"
      shift
      ;;
    -logFile)
      (( $# > 0 )) || fail_run 1
      log="$1"
      arguments+=("$argument" "$1")
      shift
      ;;
    -runTests|-quit|-nographics) ;;
    -testFilter|-assemblyNames|-executeMethod|-frameworkTestMode|-frameworkTestResults|-frameworkTestDiscovery)
      echo 'Unexpected test selection or execution override.' >&2
      fail_run 1
      ;;
    *) arguments+=("$argument") ;;
  esac
done
case "$mode" in editmode|playmode) ;; *) fail_run 1 ;; esac
expected="$workspace/artifacts/$mode-results.xml"
discovery="$workspace/artifacts/$mode-discovery.json"
[[ "$report" == "$expected" ]] || fail_run 1
[[ ! -e "$report" && ! -e "$discovery" ]] || fail_run 1
python3 - "$mode" "$report" "$discovery" "$workspace/artifacts/$mode-wrapper.json" <<'PY'
import json
from pathlib import Path
import sys
mode, report, discovery, output = sys.argv[1:]
Path(output).write_text(json.dumps({
    'schema': 1, 'mode': mode, 'runner': 'Bezoro.Unity.Framework.Validation.NativeTestRunner.Run',
    'report': report, 'discovery': discovery, 'graphics': 'Xvfb/OpenGL',
}) + '\n', encoding='utf-8')
PY
code=0
xvfb-run -a "$editor" "${arguments[@]}" -force-glcore \
  -executeMethod Bezoro.Unity.Framework.Validation.NativeTestRunner.Run \
  -frameworkTestMode "$mode" -frameworkTestResults "$report" \
  -frameworkTestDiscovery "$discovery" || code=$?
if (( code != 0 )); then fail_run "$code"; fi
python3 "$helper" verify-mode --mode "$mode" --report "$report" || fail_run 1
