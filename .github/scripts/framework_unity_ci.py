"""Prepare and verify this package's native Unity tests in a disposable project."""

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import xml.etree.ElementTree as ET
from xml.sax.saxutils import escape

EDITOR = '6000.6.3f1'
REVISION = '45d8eee7de74'
IMAGE = 'unityci/editor@sha256:59bfffa0e0b8254ce0606502d9b8362f72258de028e21b6de186b78fcb715b83'
MODES = ('editmode', 'playmode')
ROOT = Path(__file__).resolve().parents[2]
ARTIFACTS = ROOT / 'artifacts'


def write_json(path, value):
    path.write_text(json.dumps(value, indent=2, sort_keys=True) + '\n', encoding='utf-8')


def source_test_inventory(root):
    """Require every source test method; parameterized cases remain native evidence."""
    inventory = {mode: {} for mode in MODES}
    # Preserve offsets while removing comments/literals before reading declarations.
    literals = re.compile(r'//[^\n]*|/\*.*?\*/|@"(?:[^"]|"")*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'', re.S)
    attribute = re.compile(r'\[\s*(Test|TestCase|UnityTest)(?:Attribute)?\b')
    method = re.compile(r'((?:\s*\[[^\]]*\])+)'
                        r'\s*(?:public|internal)\s+(?:(?:static|async)\s+)*'
                        r'[\w.<>?,]+\s+(\w+)\s*\(', re.M)
    for path in sorted((root / 'tests').rglob('*.cs')):
        text = path.read_text(encoding='utf-8-sig')
        code = literals.sub(lambda match: ''.join('\n' if char == '\n' else ' ' for char in match.group()), text)
        if re.search(r'\[\s*(?:TestCaseSource|TestFixtureSource|Theory)\b', code):
            raise ValueError('Source-driven test expansion requires explicit native inventory discovery.')
        declarations = [(match, attribute.findall(match.group(1))) for match in method.finditer(code)]
        declarations = [(match, attributes) for match, attributes in declarations if attributes]
        if sum(len(attributes) for _, attributes in declarations) != len(attribute.findall(code)):
            raise ValueError(f'Test inventory cannot resolve every annotated method in {path.relative_to(root)}.')
        if not declarations:
            continue
        if re.search(r'^\s*#\s*(?:if|elif|else)\b', code, re.M):
            raise ValueError('Conditional test inventory needs explicit Editor-specific discovery before acceptance.')
        namespaces = re.findall(r'\bnamespace\s+([\w.]+)\s*[;{]', code)
        if len(namespaces) != 1:
            raise ValueError(f'Cannot resolve one test namespace in {path.relative_to(root)}.')
        mode = next((mode for folder, mode in (('EditMode', 'editmode'), ('PlayMode', 'playmode')) if folder in path.parts), None)
        if mode is None:
            raise ValueError(f'Test mode is unknown for {path.relative_to(root)}.')
        prefix = 'Bezoro.Unity.Core.Tests.' + ('EditMode' if mode == 'editmode' else 'PlayMode')
        if not (namespaces[0] == prefix or namespaces[0].startswith(prefix + '.')):
            raise ValueError(f'Test namespace does not match its mode in {path.relative_to(root)}.')
        opened, pairs = [], {}
        for index, character in enumerate(code):
            if character == '{':
                opened.append(index)
            elif character == '}':
                if not opened:
                    raise ValueError('Test source contains unbalanced declarations.')
                pairs[opened.pop()] = index
        classes = []
        for match in re.finditer(r'\bclass\s+(\w+)([^{};]*)\{', code):
            start = match.end() - 1
            classes.append((match.group(1), match.group(2), start, pairs.get(start, -1)))
        for match, attributes in declarations:
            enclosing = [item for item in classes if item[2] < match.start() < item[3]]
            if len(enclosing) != 1 or ':' in enclosing[0][1] or '<' in enclosing[0][1]:
                raise ValueError('Nested, inherited or generic test fixtures require explicit native inventory discovery.')
            fixture = namespaces[0] + '.' + enclosing[0][0]
            name = fixture + '.' + match.group(2)
            if name in inventory[mode]:
                raise ValueError(f'Duplicate source test method: {name}')
            inventory[mode][name] = {'fixture': fixture, 'source': path.relative_to(root).as_posix(),
                                     'attributes': sorted(set(attributes))}
    if any(not methods for methods in inventory.values()):
        raise ValueError('Both native test modes need nonempty source test inventories.')
    return inventory


def prepare():
    destination = ROOT / 'ci-source'
    if destination.exists() or ARTIFACTS.exists():
        raise ValueError('Preparation requires fresh ci-source and artifacts directories.')
    commit = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip()
    expected = os.environ.get('SOURCE_COMMIT', commit)
    if commit != expected:
        raise ValueError('Checked-out commit differs from the requested source commit.')
    subprocess.run(['git', 'diff', '--exit-code', '--quiet', 'HEAD', '--'], cwd=ROOT, check=True)
    names = subprocess.check_output(['git', 'ls-files', '-z'], cwd=ROOT).decode().split('\0')
    files = [Path(name) for name in names if name]
    required = {Path('package.json'), Path('DevProject~/Packages/manifest.json'),
                Path('DevProject~/ProjectSettings/ProjectVersion.txt')}
    if not required.issubset(files):
        raise ValueError('The package and development project must be committed before testing.')
    manifest = json.loads((ROOT / 'DevProject~/Packages/manifest.json').read_text())
    package = json.loads((ROOT / 'package.json').read_text())
    if manifest['dependencies'].get(package['name']) != 'file:../..' or package['name'] not in manifest.get('testables', []):
        raise ValueError('The development project must test the real local package at file:../..')
    version = (ROOT / 'DevProject~/ProjectSettings/ProjectVersion.txt').read_text()
    if f'm_EditorVersion: {EDITOR}\n' not in version or f'({REVISION})' not in version:
        raise ValueError('The development project does not use the pinned Unity Editor.')
    hashes = {}
    for relative in files:
        source = ROOT / relative
        if source.is_symlink() or not source.is_file():
            raise ValueError(f'Unsupported tracked entry: {relative}')
        target = destination / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
        digest = hashlib.sha256(source.read_bytes()).hexdigest()
        if hashlib.sha256(target.read_bytes()).hexdigest() != digest:
            raise ValueError(f'Copied source differs: {relative}')
        hashes[relative.as_posix()] = digest
    inventory = source_test_inventory(destination)
    (destination / 'DevProject~/Assets').mkdir(exist_ok=True)
    ARTIFACTS.mkdir()
    write_json(ARTIFACTS / 'source.json', {
        'schema': 1, 'commit': commit, 'editor': EDITOR, 'revision': REVISION, 'image': IMAGE,
        'source_path': str(ROOT), 'test_package_path': str(destination),
        'test_project_path': str(destination / 'DevProject~'), 'source_sha256': hashes,
        'package_manifest': package, 'project_manifest': manifest,
        'run_id': os.environ.get('GITHUB_RUN_ID'), 'run_attempt': os.environ.get('GITHUB_RUN_ATTEMPT'),
    })
    write_json(ARTIFACTS / 'test-inventory.json', inventory)
    write_json(ARTIFACTS / 'test-plan.json', {
        'schema': 1, 'modes': list(MODES), 'scope': 'all package tests',
        'graphics': 'Xvfb/OpenGL', 'cold_library': True, 'consumer_projects': False,
    })
    if os.environ.get('GITHUB_OUTPUT'):
        with open(os.environ['GITHUB_OUTPUT'], 'a') as output:
            output.write(f'image={IMAGE}\n')
    print(f'Prepared exact package copy for Unity {EDITOR} at source {commit}.')


def verify_mode(mode, report):
    if mode not in MODES or report.is_symlink() or not report.is_file() or report.stat().st_size > 64 * 1024 * 1024:
        raise ValueError('Missing or unsupported native NUnit report.')
    result = ET.parse(report).getroot()
    cases = list(result.iter('test-case'))
    names = [case.get('fullname', '') for case in cases]
    count = len(cases)
    expected = json.loads((ARTIFACTS / 'test-inventory.json').read_text())[mode]
    native_inventory = {
        'mode': mode, 'source_method_count': len(expected), 'cases': sorted(names),
        'native_result': result.get('result'), 'source_method_coverage': 'unverified',
        'parameterized_expansion': 'recorded from native NUnit XML; not independently predicted',
    }
    write_json(ARTIFACTS / f'{mode}-native-inventory.json', native_inventory)
    if not (result.tag == 'test-run' and result.get('result') == 'Passed' and count > 0
            and int(result.get('total', '-1')) == count and int(result.get('passed', '-1')) == count
            and all(int(result.get(name, '-1')) == 0 for name in ('failed', 'skipped', 'inconclusive'))
            and len(set(names)) == count and all(name.startswith('Bezoro.Unity.Core.Tests.') for name in names)
            and all(case.get('result') == 'Passed' for case in cases)):
        raise ValueError(f'{mode}: expected nonempty distinct package tests, all passing without skips.')
    actual_methods = {name.split('(', 1)[0] for name in names}
    mode_prefix = 'Bezoro.Unity.Core.Tests.' + ('EditMode.' if mode == 'editmode' else 'PlayMode.')
    if any(not name.startswith(mode_prefix) for name in names):
        raise ValueError(f'{mode}: report contains cases from another test mode.')
    missing = set(expected) - actual_methods
    extra = actual_methods - set(expected)
    if missing or extra or not expected:
        raise ValueError(f'{mode}: incomplete source method coverage; missing {len(missing)}, unexpected {len(extra)}.')
    native_inventory['source_method_coverage'] = 'complete'
    write_json(ARTIFACTS / f'{mode}-native-inventory.json', native_inventory)
    print(f'{mode}: {count} native cases passed and cover all {len(expected)} source test methods.')
    return count


def license_return_report(log):
    text = re.sub(r'\x1b\[[0-?]*[ -/]*[@-~]', '', log)
    start = text.lower().rfind('returning personal license seat')
    scope = text[start:].lower() if start >= 0 else ''
    evidence = next((kind for marker, kind in (
        ('successfully returned ulf license', 'returned_ulf'),
        ('successfully returned the entitlement license', 'returned_entitlement'),
        ('license has been returned', 'license_returned')) if marker in scope), None)
    failed = re.search(r'failed to return (?:the )?personal license seat', scope) is not None
    confirmed = start >= 0 and evidence is not None and not failed
    return {'schema': 1, 'gameci_version': '0.1.72', 'strategy': 'personal',
            'return_started': start >= 0, 'result': 'Passed' if confirmed else 'Unverified',
            'evidence': evidence if confirmed else None}


def license_return():
    log = ARTIFACTS / 'gameci.log'
    report = license_return_report(log.read_text(errors='replace') if log.is_file() else '')
    write_json(ARTIFACTS / 'license-return.json', report)
    if report['result'] != 'Passed':
        raise ValueError('Personal return is unverified. Resolve the seat before another activation.')
    print('Personal license return confirmed from the final GameCI return scope.')


def verify():
    if (ARTIFACTS / 'unity-failed').exists():
        raise ValueError('A native Unity launch failed; later launches were blocked.')
    source = json.loads((ARTIFACTS / 'source.json').read_text())
    if source.get('editor') != EDITOR or source.get('image') != IMAGE or not source.get('source_sha256'):
        raise ValueError('Native source identity is missing or does not match the pinned Editor.')
    report = json.loads((ARTIFACTS / 'license-return.json').read_text())
    if report.get('result') != 'Passed' or report.get('evidence') not in ('returned_ulf', 'returned_entitlement', 'license_returned'):
        raise ValueError('Personal license return has not been confirmed.')
    counts = {mode: verify_mode(mode, ARTIFACTS / f'{mode}-results.xml') for mode in MODES}
    write_json(ARTIFACTS / 'verification.json', {'result': 'Passed', 'commit': source['commit'], 'counts': counts})


def sanitize(text):
    secrets = [os.environ.get(name, '') for name in ('UNITY_EMAIL', 'UNITY_PASSWORD')]
    values = {value for secret in secrets if secret
              for value in (secret, escape(secret), escape(secret, {'"': '&quot;'}))}
    for value in sorted(values, key=len, reverse=True):
        text = text.replace(value, '[REDACTED]')
    text = re.sub(r'[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}', '[email redacted]', text, flags=re.I)
    return re.sub(r'\b(?:[A-Z0-9]{4}-){4}[A-Z0-9]{4}\b', '[serial redacted]', text, flags=re.I)


def redact():
    # The resolved lock is native output. Never manufacture a successful dependency resolution.
    lock = ROOT / 'ci-source/DevProject~/Packages/packages-lock.json'
    if lock.is_file():
        shutil.copyfile(lock, ARTIFACTS / 'packages-lock.json')
    for path in ARTIFACTS.rglob('*'):
        if not path.is_file() or path.suffix not in ('.log', '.xml', '.json'):
            continue
        text = sanitize(path.read_text(encoding='utf-8', errors='replace'))
        if path.suffix == '.log':
            text = ''.join(line for line in text.splitlines(keepends=True)
                           if not any(marker in line.lower() for marker in (
                               'licensing', 'license system', 'serial number', 'access token', 'password')))
        path.write_text(text, encoding='utf-8')
    print('Artifact logs, reports and source identity sanitized.')


def diagnostics(log):
    if not log.is_file():
        return
    with log.open('rb') as stream:
        stream.seek(max(0, log.stat().st_size - 1024 * 1024))
        text = stream.read().decode('utf-8', errors='replace')
    errors = [line for line in text.splitlines() if re.search(r'\berror\s+CS\d{4}\s*:', line, re.I)]
    for line in list(dict.fromkeys(errors))[:20]:
        print(sanitize(line)[:800])


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('command', choices=('prepare', 'verify', 'license-return', 'redact', 'verify-mode', 'diagnostics'))
    parser.add_argument('--mode', choices=MODES)
    parser.add_argument('--report', type=Path)
    parser.add_argument('--log', type=Path)
    arguments = parser.parse_args()
    try:
        if arguments.command == 'verify-mode':
            if arguments.mode is None or arguments.report is None:
                raise ValueError('verify-mode requires --mode and --report.')
            verify_mode(arguments.mode, arguments.report)
        elif arguments.command == 'diagnostics':
            if arguments.log is None:
                raise ValueError('diagnostics requires --log.')
            diagnostics(arguments.log)
        else:
            {'prepare': prepare, 'verify': verify, 'license-return': license_return, 'redact': redact}[arguments.command]()
    except (ValueError, OSError, ET.ParseError, KeyError, subprocess.CalledProcessError) as error:
        raise SystemExit(sanitize(str(error)))
