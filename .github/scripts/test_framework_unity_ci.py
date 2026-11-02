"""Regressions for native report acceptance and licensing cleanup safeguards."""

import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest
from unittest.mock import patch

import framework_unity_ci as ci


class ProjectLayoutTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        for name in ('Assets', 'ProjectSettings', str(ci.PACKAGE_PATH / 'src'), str(ci.PACKAGE_PATH / 'tests')):
            (self.root / name).mkdir(parents=True)
        self.manifest = {'dependencies': {}, 'testables': [ci.PACKAGE_PATH.name]}
        self.package = {'name': ci.PACKAGE_PATH.name, 'dependencies': {'com.unity.test-framework': '1.8.0'}}
        ci.write_json(self.root / 'Packages/manifest.json', self.manifest)
        ci.write_json(self.root / ci.PACKAGE_PATH / 'package.json', self.package)
        (self.root / 'ProjectSettings/ProjectVersion.txt').write_text(
            f'm_EditorVersion: {ci.EDITOR}\nm_EditorVersionWithRevision: {ci.EDITOR} ({ci.REVISION})\n')

    def test_repository_root_is_the_project_and_package_is_embedded_once(self):
        self.assertEqual(ci.project_layout(self.root), (self.manifest, self.package))

    def test_nested_or_duplicate_legacy_layout_is_rejected(self):
        for name in ('DevProject~', 'src', 'tests', 'package.json'):
            with self.subTest(name=name):
                legacy = self.root / name
                legacy.touch()
                with self.assertRaisesRegex(ValueError, 'Legacy project/package roots'):
                    ci.project_layout(self.root)
                legacy.unlink()
        (self.root / 'Assets').rmdir()
        with self.assertRaisesRegex(ValueError, 'Assets is required'):
            ci.project_layout(self.root)

    def test_embedded_package_cannot_resolve_to_another_copy(self):
        for source in ('file:../..', 'https://example.com/utilities.git'):
            self.manifest['dependencies'][ci.PACKAGE_PATH.name] = source
            ci.write_json(self.root / 'Packages/manifest.json', self.manifest)
            with self.assertRaisesRegex(ValueError, 'second local or remote copy'):
                ci.project_layout(self.root)

    def test_package_tests_must_be_enabled(self):
        self.manifest['testables'] = []
        ci.write_json(self.root / 'Packages/manifest.json', self.manifest)
        with self.assertRaisesRegex(ValueError, 'enable tests'):
            ci.project_layout(self.root)

    def test_package_identity_and_sources_are_required(self):
        self.package['name'] = 'com.example.wrong'
        ci.write_json(self.root / ci.PACKAGE_PATH / 'package.json', self.package)
        with self.assertRaisesRegex(ValueError, 'embedded package'):
            ci.project_layout(self.root)
        self.package['name'] = ci.PACKAGE_PATH.name
        ci.write_json(self.root / ci.PACKAGE_PATH / 'package.json', self.package)
        (self.root / ci.PACKAGE_PATH / 'src').rmdir()
        with self.assertRaisesRegex(ValueError, 'missing src'):
            ci.project_layout(self.root)

    def test_git_and_disk_dependencies_cannot_be_declared_inside_the_package(self):
        for version in ('https://example.com/dependency.git#commit', 'file:../another-package', 'latest'):
            self.package['dependencies']['com.example.dependency'] = version
            ci.write_json(self.root / ci.PACKAGE_PATH / 'package.json', self.package)
            with self.assertRaisesRegex(ValueError, 'registry versions'):
                ci.project_layout(self.root)

    def test_editor_revision_must_match(self):
        (self.root / 'ProjectSettings/ProjectVersion.txt').write_text(f'm_EditorVersion: {ci.EDITOR}\n')
        with self.assertRaisesRegex(ValueError, 'pinned Unity Editor'):
            ci.project_layout(self.root)

    def test_prepared_project_copies_the_embedded_package_and_records_both_paths(self):
        # This temporary repository is a CI fixture, not a Unity acceptance run.
        (self.root / ci.PACKAGE_PATH / 'src/Example.cs').write_text('// Package source fixture.\n')
        for mode in ('EditMode', 'PlayMode'):
            folder = self.root / ci.PACKAGE_PATH / 'tests' / mode
            folder.mkdir()
            (folder / 'Example.cs').write_text(
                f'namespace Bezoro.Unity.Core.Tests.{mode} {{ public class Example {{ '
                '[Test] public void ExampleTest() {} } }')
        (self.root / 'Assets/Editor').mkdir()
        (self.root / 'Assets/Editor/Runner.cs').write_text('// Project-owned runner fixture.\n')
        def git(*args):
            return subprocess.check_output(['git', *args], cwd=self.root, text=True, stderr=subprocess.DEVNULL).strip()
        git('init', '--quiet')
        git('add', '.')
        git('-c', 'user.name=CI fixture', '-c', 'user.email=ci@example.invalid',
            '-c', 'commit.gpgsign=false', 'commit', '--quiet', '-m', 'CI fixture')
        with patch.object(ci, 'ROOT', self.root), patch.object(ci, 'ARTIFACTS', self.root / 'artifacts'), \
                patch.dict(os.environ, {'SOURCE_COMMIT': git('rev-parse', 'HEAD'), 'GITHUB_OUTPUT': ''}):
            ci.prepare()
        source = json.loads((self.root / 'artifacts/source.json').read_text())
        destination = self.root / 'ci-source'
        self.assertEqual(source['test_project_path'], str(destination))
        self.assertEqual(source['test_package_path'], str(destination / ci.PACKAGE_PATH))
        self.assertEqual(ci.project_layout(destination), (self.manifest, self.package))
        for name in source['source_sha256']:
            self.assertEqual((self.root / name).read_bytes(), (destination / name).read_bytes())
        self.assertFalse((destination / 'DevProject~').exists())
        self.assertFalse((destination / 'Packages/packages-lock.json').exists())


class NativeEvidenceTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.folder = Path(self.temporary.name)
        self.report = self.folder / 'results.xml'
        self.artifacts_patch = patch.object(ci, 'ARTIFACTS', self.folder)
        self.artifacts_patch.start()
        self.inventory = {
            'editmode': {'Bezoro.Unity.Core.Tests.EditMode.Example.Test': {'fixture': 'Bezoro.Unity.Core.Tests.EditMode.Example'}},
            'playmode': {'Bezoro.Unity.Core.Tests.PlayMode.Example.Test': {'fixture': 'Bezoro.Unity.Core.Tests.PlayMode.Example'}},
        }
        ci.write_json(self.folder / 'test-inventory.json', self.inventory)
        for mode in ci.MODES:
            method = next(iter(self.inventory[mode]))
            ci.write_json(self.folder / f'{mode}-discovery.json', {
                'schema': 1, 'producer': 'Unity TestRunnerApi.RetrieveTestList',
                'mode': mode, 'editor': ci.EDITOR,
                'assembly': 'Bezoro.Unity.Core.Tests.' + ('EditMode' if mode == 'editmode' else 'PlayMode'),
                'cases': [{'fullname': method, 'classname': method.rsplit('.', 1)[0], 'methodname': 'Test'}],
            })

    def tearDown(self):
        self.artifacts_patch.stop()
        self.temporary.cleanup()

    def xml(self, result='Passed', total=1, passed=1, skipped=0, case_result='Passed', name='Bezoro.Unity.Core.Tests.EditMode.Example.Test'):
        text = (f'<test-run result="{result}" total="{total}" passed="{passed}" failed="0" skipped="{skipped}" inconclusive="0">'
                f'<test-case fullname="{name}" result="{case_result}"/></test-run>')
        self.report.write_text(text)

    def test_native_nonempty_package_report_is_accepted(self):
        self.xml()
        self.assertEqual(ci.verify_mode('editmode', self.report), 1)

    def test_missing_empty_skipped_and_unrelated_reports_are_rejected(self):
        with self.assertRaises(ValueError):
            ci.verify_mode('editmode', self.report)
        for changes in ({'total': 0, 'passed': 0}, {'skipped': 1}, {'case_result': 'Skipped'}, {'name': 'AnotherPackage.Example.Test'}):
            with self.subTest(changes=changes):
                self.xml(**changes)
                with self.assertRaises(ValueError):
                    ci.verify_mode('editmode', self.report)

    def test_passing_partial_report_cannot_hide_a_missing_fixture_or_method(self):
        self.xml()
        for name in ('Bezoro.Unity.Core.Tests.EditMode.OtherFixture.Test',
                     'Bezoro.Unity.Core.Tests.EditMode.Example.AnotherMethod'):
            with self.subTest(name=name):
                inventory = json.loads(json.dumps(self.inventory))
                inventory['editmode'][name] = {'fixture': name.rsplit('.', 1)[0]}
                ci.write_json(self.folder / 'test-inventory.json', inventory)
                with self.assertRaisesRegex(ValueError, 'incomplete source method coverage'):
                    ci.verify_mode('editmode', self.report)

    def test_passing_report_from_another_mode_is_rejected(self):
        self.xml(name='Bezoro.Unity.Core.Tests.PlayMode.Example.Test')
        with self.assertRaisesRegex(ValueError, 'another test mode'):
            ci.verify_mode('editmode', self.report)

    def test_passing_subset_of_parameterized_cases_is_rejected(self):
        method = 'Bezoro.Unity.Core.Tests.EditMode.Example.Test'
        path = self.folder / 'editmode-discovery.json'
        discovery = json.loads(path.read_text())
        discovery['cases'] = [{'fullname': method + '(' + str(value) + ')',
                               'classname': method.rsplit('.', 1)[0], 'methodname': 'Test'}
                              for value in range(4)]
        ci.write_json(path, discovery)
        self.xml(name=method + '(0)')
        with self.assertRaisesRegex(ValueError, 'execution omits or adds native-discovered cases'):
            ci.verify_mode('editmode', self.report)

    def test_native_discovery_cannot_be_replaced_by_execution_results(self):
        self.xml()
        (self.folder / 'editmode-discovery.json').unlink()
        with self.assertRaises(FileNotFoundError):
            ci.verify_mode('editmode', self.report)

    def test_source_inventory_records_parameterized_methods_without_guessing_case_counts(self):
        for mode in ('EditMode', 'PlayMode'):
            folder = self.folder / ci.PACKAGE_PATH / 'tests' / mode
            folder.mkdir(parents=True)
            (folder / 'Example.cs').write_text(
                'namespace Bezoro.Unity.Core.Tests.' + mode + ' { public class Example {\n'
                '// [Test] public void NotATest() {}\n'
                '[TestCase(1)] [TestCase(2)] public void Parameters(int number) {}\n'
                '[UnityTest] public IEnumerator Coroutine() { yield return null; } } }')
        inventory = ci.source_test_inventory(self.folder)
        for mode, methods in inventory.items():
            self.assertEqual(len(methods), 2)
            self.assertTrue(any(name.endswith('.Parameters') for name in methods))
            self.assertTrue(any(name.endswith('.Coroutine') for name in methods))
            self.assertFalse(any('NotATest' in name for name in methods))
            self.assertTrue(all(item['source'].startswith(ci.PACKAGE_PATH.as_posix() + '/tests/')
                                for item in methods.values()))

    def test_only_success_inside_the_final_return_scope_confirms_personal_return(self):
        success = 'Returning personal license seat\nSuccessfully returned the entitlement license\n'
        self.assertEqual(ci.license_return_report(success)['result'], 'Passed')
        for text in (success + 'Returning personal license seat\n',
                     'Successfully returned ULF license\nReturning personal license seat\n',
                     success + 'Failed to return the Personal license seat after 1 attempt(s).\n'):
            with self.subTest(text=text):
                self.assertEqual(ci.license_return_report(text)['result'], 'Unverified')

    def test_return_report_contains_no_raw_log_or_account_identifiers(self):
        report = ci.license_return_report('private@example.com\nReturning personal license seat\nLicense has been returned\n')
        self.assertNotIn('private@example.com', json.dumps(report))
        self.assertEqual(report['result'], 'Passed')

    def test_redaction_removes_credentials_and_licensing_lines_before_upload(self):
        artifact = self.folder / 'gameci.log'
        artifact.write_text('secret&password private@example.com\n[Licensing::Module] assigned-account\nerror CS0001: missing type\n')
        with patch.object(ci, 'ARTIFACTS', self.folder), patch.object(ci, 'ROOT', self.folder), patch.dict(os.environ, {'UNITY_EMAIL': 'private@example.com', 'UNITY_PASSWORD': 'secret&password'}):
            ci.redact()
        text = artifact.read_text()
        self.assertNotIn('secret&password', text)
        self.assertNotIn('private@example.com', text)
        self.assertNotIn('assigned-account', text)
        self.assertIn('error CS0001: missing type', text)

    def test_native_failure_blocks_every_later_editor_launch(self):
        workspace = self.folder / 'workspace'
        scripts = workspace / '.github/scripts'
        scripts.mkdir(parents=True)
        (workspace / 'artifacts').mkdir()
        shutil.copyfile(Path(ci.__file__), scripts / 'framework_unity_ci.py')
        executable = self.folder / 'editor'
        executable.write_text('#!/bin/sh\necho launched >> "$CALLS"\nexit 0\n')
        executable.chmod(0o755)
        binary = self.folder / 'bin'
        binary.mkdir()
        xvfb = binary / 'xvfb-run'
        xvfb.write_text('#!/bin/sh\nshift\nexec "$@"\n')
        xvfb.chmod(0o755)
        environment = dict(os.environ, GITHUB_WORKSPACE=str(workspace), CALLS=str(self.folder / 'calls'), PATH=str(binary) + ':' + os.environ['PATH'])
        wrapper = Path(ci.__file__).with_name('framework_unity_wrapper.sh')
        for mode in ci.MODES:
            completed = subprocess.run(['bash', str(wrapper), str(executable), '-testPlatform', mode,
                                        '-testResults', str(workspace / f'artifacts/{mode}-results.xml')],
                                       env=environment, capture_output=True, text=True)
            self.assertNotEqual(completed.returncode, 0)
        self.assertEqual((self.folder / 'calls').read_text().splitlines(), ['launched'])
        self.assertTrue((workspace / 'artifacts/unity-failed').is_file())


if __name__ == '__main__':
    unittest.main()
