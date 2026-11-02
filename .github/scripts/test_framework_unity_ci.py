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

    def test_source_inventory_records_parameterized_methods_without_guessing_case_counts(self):
        for mode in ('EditMode', 'PlayMode'):
            folder = self.folder / 'tests' / mode
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
