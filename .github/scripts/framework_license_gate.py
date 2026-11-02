"""Block Personal activation when this workflow's previous Unity launch lacks a verified return.

Run before GameCI receives credentials. Requires GITHUB_TOKEN with actions:read,
GITHUB_REPOSITORY, GITHUB_RUN_ID and GITHUB_RUN_ATTEMPT. A manual
workflow_dispatch may set RECOVERED_RUN_ID after the owner has resolved that
exact run's uncertain seat. No workflow result or raw GameCI exit code proves a
return; only the dedicated positive-return verifier step in the same job does.
"""

import datetime
import json
import os
import re
import sys
import urllib.error
import urllib.request
from dataclasses import dataclass

WORKFLOW = 'unity-tests.yml'
LAUNCH_STEP = 'Run EditMode and PlayMode sequentially with graphics'
RETURN_STEP = 'Confirm Personal return from the final CLI return scope'
NOT_STARTED = {'queued', 'waiting', 'pending', 'requested'}
MAX_PAGES = 100


class GateError(ValueError):
    pass


@dataclass(frozen=True)
class Attempt:
    run_id: int
    number: int
    source: str
    started_at: datetime.datetime
    status: str


class ActionsApi:
    def __init__(self, repository, token):
        if not re.fullmatch(r'[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+', repository):
            raise GateError('GITHUB_REPOSITORY must identify this repository.')
        if not token:
            raise GateError('GITHUB_TOKEN with actions:read is required before activation.')
        self.prefix = '/repos/' + repository
        self.token = token

    def get(self, path):
        request = urllib.request.Request('https://api.github.com' + self.prefix + path,
            headers={'Authorization': 'Bearer ' + self.token,
                     'Accept': 'application/vnd.github+json',
                     'X-GitHub-Api-Version': '2022-11-28'})
        try:
            with urllib.request.urlopen(request, timeout=30) as response:
                return json.load(response)
        except urllib.error.HTTPError as error:
            raise GateError(f'Cannot establish license history: Actions API HTTP {error.code}.') from None
        except (urllib.error.URLError, TimeoutError, json.JSONDecodeError):
            raise GateError('Cannot establish license history: Actions API unavailable or invalid.') from None

    def pages(self, path, key):
        for page in range(1, MAX_PAGES + 1):
            data = self.get(f'{path}?per_page=100&page={page}')
            rows = data.get(key)
            if not isinstance(rows, list):
                raise GateError('Cannot establish license history: incomplete Actions API response.')
            yield from rows
            if len(rows) < 100:
                return
        raise GateError('License history exceeds the bounded API scan; review it before activation.')


def positive_integer(value, name):
    if not re.fullmatch(r'[1-9][0-9]*', str(value)):
        raise GateError(f'{name} must be a positive integer.')
    return int(value)


def read_attempt(data, run_id, number, expected_source=None):
    if data.get('id') != run_id or data.get('run_attempt') != number:
        raise GateError('Cannot establish license history: run/attempt identity mismatch.')
    source = data.get('head_sha', '')
    if not re.fullmatch(r'[0-9a-f]{40}', source) or expected_source and source != expected_source:
        raise GateError('Cannot establish license history: source identity mismatch.')
    try:
        started_at = datetime.datetime.fromisoformat(data['run_started_at'].replace('Z', '+00:00'))
        if started_at.tzinfo is None:
            raise ValueError()
    except (KeyError, TypeError, ValueError, AttributeError):
        raise GateError('Cannot order license history without native run timestamps.') from None
    return Attempt(run_id, number, source, started_at, data.get('status', ''))


def previous_attempts(api, current_run, current_attempt):
    attempts = []
    seen = set()
    for run in api.pages(f'/actions/workflows/{WORKFLOW}/runs', 'workflow_runs'):
        run_id = positive_integer(run.get('id'), 'Historical run ID')
        latest = positive_integer(run.get('run_attempt'), 'Historical run attempt')
        if run_id in seen:
            raise GateError('Cannot establish license history: duplicate run identity.')
        seen.add(run_id)
        if run_id == current_run and latest != current_attempt:
            raise GateError('Current run attempt differs from Actions API history.')
        # A retry is the same run ID. Exclude only the current attempt, not its history.
        for number in range(latest, 0, -1):
            if run_id == current_run and number == current_attempt:
                continue
            data = run if number == latest else api.get(f'/actions/runs/{run_id}/attempts/{number}')
            attempts.append(read_attempt(data, run_id, number, run.get('head_sha')))
    if current_run not in seen:
        raise GateError('Cannot establish license history: current workflow run is absent.')
    # Old runs can be retried today, so API creation order is insufficient.
    return sorted(attempts, key=lambda item: (item.started_at, item.run_id, item.number), reverse=True)


def inspect_attempt(api, attempt):
    jobs = list(api.pages(f'/actions/runs/{attempt.run_id}/attempts/{attempt.number}/jobs', 'jobs'))
    if not jobs:
        if attempt.status in NOT_STARTED:
            return None
        raise GateError(f'Cannot establish license history for run {attempt.run_id}, attempt {attempt.number}: native job evidence is missing.')
    launched = False
    for job in jobs:
        if (job.get('run_id') != attempt.run_id or job.get('run_attempt') != attempt.number
                or job.get('head_sha') != attempt.source):
            raise GateError('Cannot establish license history: job/source/attempt identity mismatch.')
        steps = job.get('steps')
        if not isinstance(steps, list):
            raise GateError('Cannot establish license history: job steps are missing.')
        launches = [step for step in steps if step.get('name') == LAUNCH_STEP]
        if not launches:
            if job.get('conclusion') == 'skipped' or job.get('status') in NOT_STARTED:
                continue
            raise GateError(f'Cannot establish license history for run {attempt.run_id}, attempt {attempt.number}: the native launch step is absent.')
        if len(launches) != 1:
            raise GateError('Cannot establish license history: duplicate native launch steps.')
        launch = launches[0]
        if launch.get('conclusion') == 'skipped' or launch.get('status') in NOT_STARTED:
            continue
        launched = True
        returns = [step for step in steps if step.get('name') == RETURN_STEP]
        if (len(returns) != 1 or returns[0].get('status') != 'completed'
                or returns[0].get('conclusion') != 'success'
                or not isinstance(launch.get('number'), int)
                or not isinstance(returns[0].get('number'), int)
                or returns[0]['number'] <= launch['number']):
            return 'The launched Unity job has no successful dedicated Personal-return verification.'
    return '' if launched else None


def check_history(api, current_run, current_attempt, event, recovered_run_id=''):
    recovered = positive_integer(recovered_run_id, 'RECOVERED_RUN_ID') if recovered_run_id else None
    if recovered is not None and event != 'workflow_dispatch':
        raise GateError('Seat recovery is accepted only through an explicit manual workflow_dispatch.')
    for attempt in previous_attempts(api, current_run, current_attempt):
        reason = inspect_attempt(api, attempt)
        if reason is None:
            continue  # A skipped/preflight-only run must never clear an older uncertain seat.
        identity = {'run_id': attempt.run_id, 'attempt': attempt.number, 'source': attempt.source}
        if not reason:
            if recovered is not None:
                raise GateError('RECOVERED_RUN_ID does not match an uncertain launch. Remove the stale recovery input.')
            return {'result': 'Passed', 'basis': 'verified_return', **identity}
        if recovered == attempt.run_id:
            return {'result': 'Passed', 'basis': 'manual_seat_recovery', **identity}
        raise GateError(f'Activation blocked after run {attempt.run_id}, attempt {attempt.number}. {reason} '
                        'Resolve the seat with its owner, then manually dispatch with '
                        f'recovered_run_id={attempt.run_id}. No Unity activation was attempted.')
    if recovered is not None:
        raise GateError('RECOVERED_RUN_ID has no matching uncertain launch.')
    return {'result': 'Passed', 'basis': 'no_previous_launch'}


def main():
    try:
        current_run = positive_integer(os.environ.get('GITHUB_RUN_ID'), 'GITHUB_RUN_ID')
        current_attempt = positive_integer(os.environ.get('GITHUB_RUN_ATTEMPT'), 'GITHUB_RUN_ATTEMPT')
        api = ActionsApi(os.environ.get('GITHUB_REPOSITORY', ''), os.environ.get('GITHUB_TOKEN', ''))
        decision = check_history(api, current_run, current_attempt,
                                 os.environ.get('GITHUB_EVENT_NAME', ''), os.environ.get('RECOVERED_RUN_ID', ''))
        print(json.dumps(decision, sort_keys=True))
        return 0
    except (GateError, TypeError, KeyError) as error:
        print('::error::' + str(error), file=sys.stderr)
        return 1


if __name__ == '__main__':
    raise SystemExit(main())
