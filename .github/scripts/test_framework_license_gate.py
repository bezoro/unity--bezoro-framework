"""Actions history regressions for the cross-run Personal activation interlock."""

import unittest
from unittest.mock import patch

import framework_license_gate as gate

SOURCE = 'a' * 40


def run(run_id, attempt=1, started='2026-11-02T12:00:00Z', status='completed'):
    return {'id': run_id, 'run_attempt': attempt, 'head_sha': SOURCE,
            'run_started_at': started, 'status': status}


def job(run_id, attempt=1, launched=True, returned=True, conclusion='success', started='2026-11-02T12:15:00Z'):
    launch = {'name': gate.LAUNCH_STEP, 'number': 5, 'status': 'completed',
              'conclusion': 'success' if launched else 'skipped', 'started_at': started}
    steps = [launch]
    if returned is not None:
        steps.append({'name': gate.RETURN_STEP, 'number': 6, 'status': 'completed',
                      'conclusion': 'success' if returned else 'failure'})
    return {'run_id': run_id, 'run_attempt': attempt, 'head_sha': SOURCE,
            'status': 'completed', 'conclusion': conclusion, 'steps': steps}


class FakeApi(gate.ActionsApi):
    def __init__(self, runs, jobs=None, attempts=None):
        super().__init__('owner/repository', 'test-token-not-a-secret')
        self.responses = {f'/actions/workflows/{gate.WORKFLOW}/runs?per_page=100&page=1':
                          {'workflow_runs': runs}}
        for identity, value in (jobs or {}).items():
            self.responses[f'/actions/runs/{identity[0]}/attempts/{identity[1]}/jobs?per_page=100&page=1'] = {'jobs': value}
        for identity, value in (attempts or {}).items():
            self.responses[f'/actions/runs/{identity[0]}/attempts/{identity[1]}'] = value
        self.calls = []

    def get(self, path):
        self.calls.append(path)
        if path not in self.responses:
            raise gate.GateError('Missing mocked Actions evidence: ' + path)
        return self.responses[path]


class LicenseGateTests(unittest.TestCase):
    def setUp(self):
        # These tests must never contact GitHub or receive Unity credentials.
        self.network = patch('urllib.request.urlopen', side_effect=AssertionError('Network is forbidden in unit tests.'))
        self.network.start()
        self.current = run(900, started='2026-11-02T14:00:00Z', status='in_progress')

    def tearDown(self):
        self.network.stop()

    def check(self, api, event='push', recovered='', recovered_attempt='', attempt=1):
        return gate.check_history(api, 900, attempt, event, recovered, recovered_attempt)

    def test_first_run_needs_no_historical_launch(self):
        api = FakeApi([self.current])
        self.assertEqual(self.check(api)['basis'], 'no_previous_launch')
        self.assertEqual(len(api.calls), 1)

    def test_successful_dedicated_return_allows_next_run_even_if_tests_failed(self):
        native = job(100, conclusion='failure')
        native['steps'][0]['conclusion'] = 'failure'
        api = FakeApi([self.current, run(100)], {(100, 1): [native]})
        decision = self.check(api)
        self.assertEqual(decision['basis'], 'verified_return')
        self.assertEqual(decision['source'], SOURCE)
        self.assertEqual(decision['run_id'], 100)

    def test_green_cli_or_workflow_does_not_replace_the_return_verifier(self):
        for returned in (False, None):
            with self.subTest(returned=returned):
                api = FakeApi([self.current, run(100)], {(100, 1): [job(100, returned=returned)]})
                with self.assertRaisesRegex(gate.GateError, 'Activation blocked after run 100, attempt 1'):
                    self.check(api)

    def test_preflight_only_run_does_not_clear_older_uncertain_seat(self):
        api = FakeApi([self.current, run(200, started='2026-11-02T13:00:00Z'), run(100)],
                      {(200, 1): [job(200, launched=False, returned=None, conclusion='failure')],
                       (100, 1): [job(100, returned=False)]})
        with self.assertRaisesRegex(gate.GateError, 'run 100, attempt 1'):
            self.check(api)

    def test_cancelled_native_attempt_without_return_blocks(self):
        native = job(100, returned=None, conclusion='cancelled')
        native['steps'][0]['conclusion'] = 'cancelled'
        api = FakeApi([self.current, run(100)], {(100, 1): [native]})
        with self.assertRaises(gate.GateError):
            self.check(api)

    def test_retry_checks_the_previous_attempt_of_its_own_run_id(self):
        current = run(900, attempt=2, started='2026-11-02T14:00:00Z', status='in_progress')
        api = FakeApi([current], {(900, 1): [job(900, returned=False)]},
                      {(900, 1): run(900, started='2026-11-02T13:00:00Z')})
        with self.assertRaisesRegex(gate.GateError, 'run 900, attempt 1'):
            self.check(api, attempt=2)
        self.assertTrue(any('/attempts/1/jobs' in path for path in api.calls))
        self.assertFalse(any('/attempts/2/jobs' in path for path in api.calls))

    def test_retry_after_a_verified_previous_attempt_is_allowed(self):
        current = run(900, attempt=2, status='in_progress')
        api = FakeApi([current], {(900, 1): [job(900)]}, {(900, 1): run(900)})
        self.assertEqual(self.check(api, attempt=2)['basis'], 'verified_return')

    def test_old_run_retried_today_takes_precedence_over_api_creation_order(self):
        api = FakeApi([self.current, run(200, started='2026-11-02T12:30:00Z'),
                       run(100, attempt=2, started='2026-11-02T13:00:00Z')],
                      {(100, 2): [job(100, attempt=2, returned=False, started='2026-11-02T13:15:00Z')],
                       (100, 1): [job(100, started='2026-11-01T10:15:00Z')],
                       (200, 1): [job(200)]},
                      {(100, 1): run(100, started='2026-11-01T10:00:00Z')})
        with self.assertRaisesRegex(gate.GateError, 'run 100, attempt 2'):
            self.check(api)

    def test_only_exact_uncertain_run_can_be_recovered_in_a_manual_dispatch(self):
        api = FakeApi([self.current, run(100)], {(100, 1): [job(100, returned=False)]})
        decision = self.check(api, event='workflow_dispatch', recovered='100', recovered_attempt='1')
        self.assertEqual(decision['basis'], 'manual_seat_recovery')
        self.assertEqual(decision['attempt'], 1)
        with self.assertRaisesRegex(gate.GateError, 'run 100, attempt 1'):
            self.check(api, event='workflow_dispatch', recovered='101', recovered_attempt='1')

    def test_recovery_input_cannot_authorize_a_push_or_silently_match_a_safe_run(self):
        api = FakeApi([self.current, run(100)], {(100, 1): [job(100)]})
        with self.assertRaisesRegex(gate.GateError, 'explicit manual'):
            self.check(api, recovered='100', recovered_attempt='1')
        with self.assertRaisesRegex(gate.GateError, 'stale recovery input'):
            self.check(api, event='workflow_dispatch', recovered='100', recovered_attempt='1')
        with self.assertRaisesRegex(gate.GateError, 'no matching uncertain'):
            self.check(FakeApi([self.current]), event='workflow_dispatch', recovered='100', recovered_attempt='1')

    def test_missing_job_evidence_is_not_assumed_to_mean_no_activation(self):
        api = FakeApi([self.current, run(100)], {(100, 1): []})
        with self.assertRaisesRegex(gate.GateError, 'native job evidence is missing'):
            self.check(api)

    def test_manual_recovery_cannot_bypass_missing_history_and_hide_an_older_seat(self):
        api = FakeApi([self.current, run(200, started='2026-11-02T13:00:00Z'), run(100)],
                      {(200, 1): [], (100, 1): [job(100, returned=False)]})
        with self.assertRaisesRegex(gate.GateError, 'native job evidence is missing'):
            self.check(api, event='workflow_dispatch', recovered='200', recovered_attempt='1')

    def test_queued_run_without_jobs_does_not_clear_an_older_uncertain_launch(self):
        api = FakeApi([self.current, run(200, started='2026-11-02T13:00:00Z', status='queued'), run(100)],
                      {(200, 1): [], (100, 1): [job(100, returned=False)]})
        with self.assertRaisesRegex(gate.GateError, 'run 100, attempt 1'):
            self.check(api)

    def test_source_or_attempt_mismatch_fails_closed(self):
        for changed in ({'head_sha': 'b' * 40}, {'run_attempt': 2}, {'run_id': 101}):
            with self.subTest(changed=changed):
                native = job(100)
                native.update(changed)
                api = FakeApi([self.current, run(100)], {(100, 1): [native]})
                with self.assertRaisesRegex(gate.GateError, 'identity mismatch'):
                    self.check(api)

    def test_return_step_before_launch_does_not_prove_cleanup(self):
        native = job(100)
        native['steps'][1]['number'] = 4
        api = FakeApi([self.current, run(100)], {(100, 1): [native]})
        with self.assertRaisesRegex(gate.GateError, 'dedicated Personal-return'):
            self.check(api)

    def test_every_launched_job_must_have_its_own_return_verification(self):
        api = FakeApi([self.current, run(100)], {(100, 1): [job(100), job(100, returned=False)]})
        with self.assertRaisesRegex(gate.GateError, 'dedicated Personal-return'):
            self.check(api)

    def test_missing_current_run_and_invalid_timestamps_fail_closed(self):
        with self.assertRaisesRegex(gate.GateError, 'current workflow run is absent'):
            self.check(FakeApi([]))
        native = job(100, started='not a timestamp')
        with self.assertRaisesRegex(gate.GateError, 'timestamps'):
            self.check(FakeApi([self.current, run(100)], {(100, 1): [native]}))

    def test_actual_native_start_overrides_run_scheduling_order(self):
        unsafe = job(100, returned=False, started='2026-11-02T13:20:00Z')
        safe = job(200, started='2026-11-02T13:00:00Z')
        api = FakeApi([self.current, run(200, started='2026-11-02T12:30:00Z'),
                       run(100, started='2026-11-02T12:00:00Z')],
                      {(200, 1): [safe], (100, 1): [unsafe]})
        with self.assertRaisesRegex(gate.GateError, 'run 100, attempt 1'):
            self.check(api)

    def test_recovery_of_attempt_one_cannot_acknowledge_new_failure_in_attempt_two(self):
        api = FakeApi([self.current, run(100, attempt=2)],
                      {(100, 2): [job(100, attempt=2, returned=False, started='2026-11-02T13:00:00Z')],
                       (100, 1): [job(100, returned=False)]}, {(100, 1): run(100)})
        with self.assertRaisesRegex(gate.GateError, 'run 100, attempt 2'):
            self.check(api, event='workflow_dispatch', recovered='100', recovered_attempt='1')
        decision = self.check(api, event='workflow_dispatch', recovered='100', recovered_attempt='2')
        self.assertEqual(decision['basis'], 'manual_seat_recovery')
        self.assertEqual(decision['attempt'], 2)

    def test_recovery_requires_both_identity_fields(self):
        api = FakeApi([self.current])
        for values in ({'recovered': '100'}, {'recovered_attempt': '1'}):
            with self.subTest(values=values):
                with self.assertRaisesRegex(gate.GateError, 'requires both'):
                    self.check(api, event='workflow_dispatch', **values)

    def test_missing_native_timestamp_cannot_fall_back_to_run_timestamp(self):
        native = job(100)
        del native['steps'][0]['started_at']
        api = FakeApi([self.current, run(100)], {(100, 1): [native]})
        with self.assertRaisesRegex(gate.GateError, 'actual native launch timestamps'):
            self.check(api)

    def test_tied_native_timestamps_of_distinct_attempts_fail_closed(self):
        api = FakeApi([self.current, run(100), run(200)],
                      {(100, 1): [job(100, returned=False)], (200, 1): [job(200)]})
        with self.assertRaisesRegex(gate.GateError, 'identical launch timestamps'):
            self.check(api)

    def test_queued_run_does_not_need_a_native_start_timestamp(self):
        queued = run(200, status='queued')
        queued['run_started_at'] = None
        api = FakeApi([self.current, queued, run(100)],
                      {(200, 1): [], (100, 1): [job(100)]})
        self.assertEqual(self.check(api)['basis'], 'verified_return')

    def test_pagination_cannot_ignore_older_history(self):
        api = FakeApi([])
        path = f'/actions/workflows/{gate.WORKFLOW}/runs'
        api.responses[path + '?per_page=100&page=1'] = {'workflow_runs': list(range(100))}
        api.responses[path + '?per_page=100&page=2'] = {'workflow_runs': [100]}
        self.assertEqual(list(api.pages(path, 'workflow_runs')), list(range(101)))
        self.assertEqual(len(api.calls), 2)

    def test_api_failure_never_authorizes_activation(self):
        api = FakeApi([self.current, run(100)])
        with self.assertRaisesRegex(gate.GateError, 'Missing mocked Actions evidence'):
            self.check(api)
        with self.assertRaisesRegex(gate.GateError, 'actions:read'):
            gate.ActionsApi('owner/repository', '')


if __name__ == '__main__':
    unittest.main()
