# Unity Framework CI

The workflow in `.github/workflows/unity-tests.yml` runs the repository-root project in a real Unity 6000.6.3f1 Editor. It uses the GameCI flow adapted from [Canvas Auto Framing PR 24](https://github.com/bezoro/unity--canvas-auto-framing/pull/24). It does not check out or run a consuming game project.

## Activation setup

The owner must configure `UNITY_EMAIL` and `UNITY_PASSWORD` in this repository's [Actions secrets](https://github.com/bezoro/unity--bezoro-framework/settings/secrets/actions). Use the intended Unity Personal account with its entitlement and terms already accepted. Never put credentials in commits, comments, artifacts or chat.

The workflow checks these secrets before downloading the Editor or attempting activation. A missing secret fails the run explicitly; it does not mean Unity executed. Manual dispatch also requires `allow_activation=true`. An Actions-history interlock blocks activation after an earlier Unity launch without successful dedicated return verification, including retries of the same run. Launches are ordered by their native step start time, not workflow scheduling time. After resolving that exact seat with the owner, a manual dispatch can explicitly provide its exact `recovered_run_id` and `recovered_run_attempt`. A completed attempt with missing job evidence or an unrecognizable launch step also blocks activation. After the owner confirms cleanup, the manual dispatch records a successful step naming that historical run and attempt. Future checks read that step as a recovery receipt. The receipt applies only to that attempt, even when a different unresolved attempt blocks the dispatch. For multiple blocked attempts, resolve and acknowledge each separately. Missing API responses and inconsistent identities still require investigation.

## When to run

Relevant integrations into `main` start the native workflow automatically. Working-branch pushes and ordinary PR changes do not. Use manual dispatch only for a concrete unresolved concern; do not repeat a run of unchanged source.

Run these inexpensive checks before integration:

```sh
python3 -m unittest discover -s .github/scripts -p 'test_*.py' -v
bash -n .github/scripts/framework_unity_wrapper.sh
git diff --check
```

## Native execution and evidence

The runner records the source commit, file hashes, image digest and dependency manifests, then verifies the copied bytes under `ci-source`. That directory is the disposable copy of the root project; its embedded package stays at `ci-source/Packages/com.bezoro.unity-framework`. No separate development project is used. The Framework and UniTask remain pinned Git dependencies; no framework DLLs are copied into Assets.

EditMode and PlayMode run sequentially in one GameCI activation with Xvfb/OpenGL and a cold Library. The job allows 180 minutes. The pre-activation stages have individual limits, and each Editor invocation has a 45-minute external deadline, including import, tests and synchronous compilation. Unity's test runner has a 40-minute internal deadline. A per-Editor timeout returns control to GameCI to release the seat inside the job budget. Each mode uses Unity's public Test Runner API to discover its complete `Bezoro.Unity.Core.Tests` assembly before executing that assembly in the same Editor process. Native discovery expands parameterized cases. Acceptance compares every executed case with that independent inventory and also checks coverage against the package's source methods. After both suites report success and the PlayMode job finishes its Editor cleanup, the same Editor compiles standalone Linux Player scripts using `PlayerBuildInterface.CompilePlayerScripts`. The check requires the compiler to return and write `Bezoro.Unity.Core.dll`. The workflow uploads `player-compilation.json` with the target, Editor version and assembly name. This check covers managed Player compilation. Running a built Player and testing other targets require separate validation. A native failure blocks later Editor launches while allowing the CLI to return the license. No home directories or personal preferences are mounted.

Before activation, a credential-free Docker probe verifies that the checksum-pinned GameCI CLI forwards the wrapper and that its real container test script invokes it. The probe records commands and failure propagation without launching Unity or producing test-result XML. It does not substitute for the subsequent native test run.

Acceptance requires complete native XML reports, distinct passing tests without failures, skips or inconclusive results, and confirmation that the Personal entitlement was returned in the final return scope. A green process exit, static source check, exported diagnostic or passing subset does not establish full native acceptance. Preparation saves the tracked input lock as `packages-lock-input.json`. After the native EditMode Editor confirms the embedded framework, the pure C# framework and UniTask are registered with Package Manager, it captures the current project lock as `packages-lock-resolved.json`. A successful resolution may retain identical bytes. A failure before this registration check leaves only the input snapshot.

Source identity, reports and sanitized logs are retained for three days. Inspect compiler/test failures and the license-return report before another activation. If return is unverified, stop and resolve the retained seat before retrying. Do not cancel an active run to replace it.

## Existing dependencies

The package uses the published Framework, UniTask and its declared Unity registry dependencies. The character-controller integration and commercial drawing integration have been removed. Their APIs are not replaced by substitutes. UI component tests use reflection directly and no longer require external editor helpers. Native resolution and compilation must still pass before claiming acceptance; static manifest checks do not establish either result.
