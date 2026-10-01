# Unity framework agent instructions

This directory is the authoritative editable source for `bezoro/unity--bezoro-framework`, imported by Chess Crush as `Assets/_Bezoro/Unity`. Read `~Docs/SHARING.md` and the consuming game's `AGENTS.md`. Before changes, inspect both repositories' status and relevant diffs; preserve dirty work. Edit source here directly without copy-back scripts.

Keep changes in `src` and `tests` scoped to Unity utilities. Preserve `.meta` files, GUIDs, assembly names, references, and serialized consumers. Game-owned third-party content, including the modified KCC assembly, and editor feedback audio live outside this repository. `V:\Repos\Rider\Bezoro.Framework` is the separate pure C# DLL source. Inspect dependencies when their APIs matter.

Validate through the consuming game using the supported editor recorded in its `ProjectSettings/ProjectVersion.txt`, following its test and compilation instructions. Do not substitute a different editor without authorization. This asset checkout is not a standalone Unity project and needs the game's KCC, Bezoro precompiled dependencies, and packages. Refresh generated Unity project state as needed; do not edit generated `.csproj` files.

Commit and push only when explicitly authorized. Publish the framework revision first and verify the remote contains it before recording and publishing the game's tested gitlink. Framework commits and pushes do not update the game automatically. A normal submodule update may detach HEAD; choose an intended branch before editing and preserve dirty files. Never force checkout or discard work to update the dependency.
