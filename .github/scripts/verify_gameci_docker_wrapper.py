"""Prove the pinned GameCI Docker test script invokes the wrapper without starting Unity."""

import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile

TEST_SCRIPT_SHA256 = '017674703b92f802fd861786c9cb4277ba5c829fcaf5112a558876e826ac3a62'
CLI_ARCHIVE_SHA256 = '1de550a620ee92e05e920b6db7d431621440b60b2577f04b4a997dc92a58cb0a'
IMAGE = 'unityci/editor@sha256:59bfffa0e0b8254ce0606502d9b8362f72258de028e21b6de186b78fcb715b83'
ROOT = Path(__file__).resolve().parents[2]


def verify(gameci_directory, image, output):
    test_script = gameci_directory / 'dist/platforms/ubuntu/steps/test.sh'
    if hashlib.sha256(test_script.read_bytes()).hexdigest() != TEST_SCRIPT_SHA256:
        raise ValueError('The Docker probe must use the exact test script from the checksum-verified GameCI release.')
    with tempfile.TemporaryDirectory(prefix='framework-docker-wrapper-') as temporary:
        workspace = Path(temporary)
        scripts = workspace / '.github/scripts'
        scripts.mkdir(parents=True)
        for name in ('framework_unity_wrapper.sh', 'framework_unity_ci.py'):
            shutil.copyfile(ROOT / '.github/scripts' / name, scripts / name)
        shutil.copyfile(test_script, workspace / 'pinned-gameci-test.sh')
        (workspace / 'artifacts').mkdir()
        (workspace / 'project/ProjectSettings').mkdir(parents=True)
        (workspace / 'project/ProjectSettings/ProjectVersion.txt').write_text('m_EditorVersion: 6000.6.3f1\n')
        host_binaries = workspace / 'host-probe-bin'
        host_binaries.mkdir()
        docker_recorder = host_binaries / 'docker'
        docker_recorder.write_text('''#!/usr/bin/env python3
import json, os, sys
from pathlib import Path
if sys.argv[1:2] == ['version']:
    print('linux')
    raise SystemExit(0)
if sys.argv[1:2] == ['run']:
    Path(os.environ['FRAMEWORK_DOCKER_ARGS']).write_text(json.dumps(sys.argv[1:]))
    raise SystemExit(73)
raise SystemExit(74)
''')
        docker_recorder.chmod(0o755)
        docker_arguments_path = workspace / 'docker-arguments.json'
        hook = 'bash /github/workspace/.github/scripts/framework_unity_wrapper.sh'
        environment = {name: value for name, value in os.environ.items() if not name.startswith('UNITY_')}
        environment.update(PATH=str(host_binaries) + ':' + os.environ['PATH'],
                           ENGINE_LAUNCH_WRAPPER=hook, FRAMEWORK_DOCKER_ARGS=str(docker_arguments_path))
        # Exercise the downloaded binary's actual Docker command construction.
        # Its Docker executable is a recorder, so this phase cannot start a container or activate.
        cli_probe = subprocess.run([str(gameci_directory / 'game-ci'), 'test', '.', '--docker', '--engine=unity',
                        '--engineVersion=6000.6.3f1', '--customImage=' + image,
                        '--testPlatforms=editmode;playmode', '--coverageEnabled=false'],
                       cwd=ROOT, env=environment, capture_output=True, text=True, timeout=60)
        if not docker_arguments_path.is_file():
            raise ValueError('CLI Docker command probe did not reach its recorder: ' + (cli_probe.stdout + cli_probe.stderr)[-1800:])
        docker_arguments = json.loads(docker_arguments_path.read_text())
        if ('ENGINE_LAUNCH_WRAPPER=' + hook not in docker_arguments
                or not any('/platforms/ubuntu/steps:/steps:' in argument for argument in docker_arguments)
                or docker_arguments[-2:] != ['/bin/bash', '/entrypoint.sh']):
            raise ValueError('The pinned CLI binary did not forward the wrapper into its normal Docker lifecycle.')
        binaries = workspace / 'probe-bin'
        binaries.mkdir()
        editor = binaries / 'unity-editor'
        editor.write_text('''#!/usr/bin/env python3
import json, os, sys
from pathlib import Path
root = Path(os.environ['GITHUB_WORKSPACE'])
is_test = '-executeMethod' in sys.argv
name = 'test' if is_test else 'passthrough'
(root / (name + '-arguments.json')).write_text(json.dumps(sys.argv[1:]))
with (root / 'probe-calls.txt').open('a') as calls:
    calls.write(name + '\\n')
if '-logFile' in sys.argv:
    Path(sys.argv[sys.argv.index('-logFile') + 1]).write_text('Infrastructure probe. No Unity process was started.\\n')
raise SystemExit(73 if is_test else 17)
''')
        editor.chmod(0o755)
        xvfb = binaries / 'xvfb-run'
        xvfb.write_text('#!/bin/bash\n[[ "$1" == -a ]] && shift\nexec "$@"\n')
        xvfb.chmod(0o755)
        probe = workspace / 'probe.sh'
        probe.write_text('''#!/bin/bash
set -uo pipefail
export GITHUB_WORKSPACE=/github/workspace
export PATH="$GITHUB_WORKSPACE/probe-bin:$PATH"
export PROJECT_PATH=project ARTIFACTS_PATH=artifacts COVERAGE_RESULTS_PATH=coverage
export UNITY_VERSION=6000.6.3f1 COVERAGE_ENABLED=false PACKAGE_MODE=false
export CUSTOM_PARAMETERS=-force-glcore TEST_PLATFORMS='editmode;playmode'
export PRIVATE_REGISTRY_TOKEN='' CHOWN_FILES_TO=''
export ENGINE_LAUNCH_WRAPPER='bash /github/workspace/.github/scripts/framework_unity_wrapper.sh'
source "$GITHUB_WORKSPACE/pinned-gameci-test.sh" > "$GITHUB_WORKSPACE/probe-step.log" 2>&1
[[ "${TEST_RUNNER_EXIT_CODE:-0}" -ne 0 ]] || exit 1
# A prior test failure must never block GameCI's non-test license-return command.
code=0
bash "$GITHUB_WORKSPACE/.github/scripts/framework_unity_wrapper.sh" unity-editor \\
  -batchmode -quit -returnlicense -nographics --probe-only || code=$?
[[ "$code" == 17 ]] || exit 1
''')
        completed = subprocess.run([
            'docker', 'run', '--rm', '--network', 'none', '--entrypoint', '/bin/bash',
            '--volume', f'{workspace}:/github/workspace', '--workdir', '/github/workspace',
            image, '/github/workspace/probe.sh'], capture_output=True, text=True)
        if completed.returncode != 0:
            raise ValueError('Pinned Docker wrapper probe failed before activation. Inspect the local probe command; no Unity license was requested.')
        calls = (workspace / 'probe-calls.txt').read_text().splitlines()
        arguments = json.loads((workspace / 'test-arguments.json').read_text())
        passthrough = json.loads((workspace / 'passthrough-arguments.json').read_text())
        expected_passthrough = ['-batchmode', '-quit', '-returnlicense', '-nographics', '--probe-only']
        if calls != ['test', 'passthrough'] or passthrough != expected_passthrough:
            raise ValueError('Docker wrapper did not block the second test or preserve non-test commands.')
        expected_arguments = {
            '-executeMethod': 'Bezoro.Unity.Framework.Validation.NativeTestRunner.Run',
            '-frameworkTestMode': 'editmode',
            '-frameworkTestResults': '/github/workspace/artifacts/editmode-results.xml',
            '-frameworkTestDiscovery': '/github/workspace/artifacts/editmode-discovery.json',
        }
        if (any(argument in arguments for argument in ('-runTests', '-testPlatform', '-testResults', '-nographics', '-quit'))
                or '-force-glcore' not in arguments
                or any(flag not in arguments or arguments[arguments.index(flag) + 1] != value
                       for flag, value in expected_arguments.items())
                or not (workspace / 'artifacts/unity-failed').is_file()
                or not (workspace / 'artifacts/editmode-wrapper.json').is_file()):
            raise ValueError('Docker wrapper did not invoke the native runner with its exact mode/report/discovery contract.')
    image_id = subprocess.check_output(['docker', 'image', 'inspect', '--format', '{{.Id}}', image], text=True).strip()
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps({
        'schema': 1, 'result': 'Passed', 'gameci_version': '0.1.72',
        'gameci_archive_sha256': CLI_ARCHIVE_SHA256, 'gameci_test_script_sha256': TEST_SCRIPT_SHA256,
        'image': image, 'image_id': image_id, 'cli_docker_hook_forwarded': True, 'test_hook_invocations': 1,
        'later_test_launch_blocked': True, 'non_test_passthrough_preserved': True,
        'native_unity_started': False, 'activation_attempted': False,
        'scope': 'real Docker execution of the pinned GameCI test script with recorder commands; no NUnit evidence',
    }, indent=2) + '\n', encoding='utf-8')
    print('Pinned GameCI Docker wrapper hook, runner arguments, failure stop and non-test passthrough verified. No Unity activation.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--gameci-dir', type=Path, required=True)
    parser.add_argument('--image', default=IMAGE)
    parser.add_argument('--output', type=Path, required=True)
    arguments = parser.parse_args()
    try:
        verify(arguments.gameci_dir, arguments.image, arguments.output)
    except (ValueError, OSError, subprocess.CalledProcessError) as error:
        raise SystemExit(str(error))
