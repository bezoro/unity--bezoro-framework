# Unity framework agent instructions

This directory is the authoritative editable source for `bezoro/unity--bezoro-framework`, imported by Chess Crush as `Assets/_Bezoro/Unity`. Read `~Docs/SHARING.md` and the consuming game's `AGENTS.md`. Before changes, inspect both repositories' status and relevant diffs; preserve dirty work. Edit source here directly without copy-back scripts.

Keep changes in `src` and `tests` scoped to Unity utilities. Preserve `.meta` files, GUIDs, assembly names, references, and serialized consumers. Game-owned third-party content, including the modified KCC assembly, and editor feedback audio live outside this repository. `V:\Repos\Rider\Bezoro.Framework` is the separate pure C# DLL source. Inspect dependencies when their APIs matter.

Validate through the consuming game using the supported editor recorded in its `ProjectSettings/ProjectVersion.txt`, following its test and compilation instructions. Do not substitute a different editor without authorization. This asset checkout is not a standalone Unity project and needs the game's KCC, Bezoro precompiled dependencies, and packages. Refresh generated Unity project state as needed; do not edit generated `.csproj` files.

Commit and push only when explicitly authorized. Publish the framework revision first and verify the remote contains it before recording and publishing the game's tested gitlink. Framework commits and pushes do not update the game automatically. A normal submodule update may detach HEAD; choose an intended branch before editing and preserve dirty files. Never force checkout or discard work to update the dependency.

## Design, changes and review

Adapted from [Canvas Auto Framing PR 24](https://github.com/bezoro/unity--canvas-auto-framing/pull/24), instruction source `be5577f066cb77cbbe327319fda044f2f29e9081`, and [Framework AGENTS.md](https://github.com/bezoro/framework/blob/e728f39595fe381d209cbfc62acf5d6293e0093f/AGENTS.md). The rules below apply to this package; that product's framing, Help, preset and screenshot matrices and the other repository's .NET build commands do not.

- Prefer readable APIs and native Unity workflows with explicit ownership, lifecycle, side effects and failure behavior. Keep implementations cohesive and dependencies justified; convenience must not introduce unnecessary polling, allocations or mandatory services.
- Accept incompatible changes when they improve the design. Document migrations and use Conventional Commits with `!` or a `BREAKING CHANGE` footer, with Semantic Versioning for releases. The root `package.json` owns this package's version; the upstream Framework retains its separate version resolver.
- Keep PR descriptions in English, starting with the concrete behavior and followed by `## Changes` and `## Verification`. Separate passed, failed and pending checks. Require a completed clean Codex review of the final head before integrating reviewed changes.
- Commit cohesive, locally checked increments and open a draft PR after the first coherent push when integration is intended. Keep the description and release/migration guidance current. Initial CI bootstrap may record native validation as pending until the workflow is integrated and executes; that does not establish native acceptance.
- Preserve existing UTF-8 bytes, line endings, metadata GUIDs, assembly names and serialized consumers. Keep test-owned objects and windows isolated and restore user selection, focus, scene dirtiness and preferences after local tests.
- Track stable `.meta` files for every imported package asset and directory, including XML documentation. Reject missing, malformed, zero or duplicate GUIDs before publication, and preserve existing GUIDs across rebuilds. Never repair immutable Git package metadata by editing PackageCache; correct it in the package owner.

## CI economy and native Unity evidence

- Use this repository's `.github/workflows/unity-tests.yml` and `DevProject~`. Do not run or alter another game's project to validate this package.
- Run automatic Unity validation on relevant integrations into `main`. Working-branch pushes and PR updates do not start Unity; authorized manual dispatch is a deliberate checkpoint.
- Run inexpensive local safeguards before activation. Repeat CI only for changed source, a new failure or an unresolved validation concern. Do not schedule duplicate runs or cancel an active run to replace it.
- Use `unity` for local Unity operations and `bezoro-unity-workflow` when installed. If unavailable, report that limitation. The approved remote alternative is pinned GameCI CLI `v0.1.72` with the unmodified Unity Editor in disposable Linux containers. Pass `--coverageEnabled=false` directly.
- Match the exact Editor and revision in `DevProject~/ProjectSettings/ProjectVersion.txt`. Record the commit, image digest, source/test-copy paths, dependency manifests and source hashes, and verify copied bytes before launch. Report the original local Editor state separately from CI results.
- Execute EditMode and PlayMode sequentially with graphics. Never mount personal preferences, license files or a user's home into CI. A failed native launch must block later Editor launches while preserving GameCI's license cleanup.
- Require complete, nonempty, distinct passing NUnit cases with no failures, skips or inconclusive results. A passing subset, static parse or standalone C# compilation cannot establish full native Unity acceptance. Record actual discovered inventories rather than inventing test counts.
- Check Unity account secrets before activation, never print credentials, and sanitize logs and reports before upload. Require positive Personal return evidence from the final return scope; a green process exit alone is insufficient. Stop further activation if return is uncertain.
- Do not cache licenses, preferences, reports or user homes. This initial validation uses a cold Library and retains source identity and sanitized native evidence for three days. Do not publish a release or claim native acceptance without the matching reports and confirmed return.
