# Working on the shared Unity utilities

This repository contains the current Unity utility source imported directly by Chess Crush at `Assets/_Bezoro/Unity`. The source and test trees retain their Unity `.meta` files and assembly names. The repository history also retains the older framework implementation; the migration replaces its active asset tree with the evolved Chess Crush source.

Use the nested checkout in Chess Crush as the current editable source. Open that exact directory in another editor or workspace if desired. Edits appear both in Chess Crush and in this repository's Git status immediately, because both use the same directory. Independent clones share committed changes through normal Git operations.

Commit and publish utility changes here before publishing a game revision that records the new submodule commit. A framework commit or push does not update the game automatically. The game stores a pinned commit and must explicitly record a tested replacement. The game helper `Tools/UnityFramework.ps1` provides status, tracked diffs, fetch-only operation, and clean-checkout revision staging. It never commits or pushes.

After `git submodule update`, check the current branch before editing. The checkout may have a detached HEAD. Create or switch to an intended branch, preserve dirty work, and avoid forced updates.

Keep Unity and pure C# roles separate. `bezoro/framework` supplies the game's existing UPM pure C# dependency. Third-party content and editor feedback audio remain game-owned outside this repository, at `Assets/_Project/ThirdParty` and `Assets/_Project/Engine/EditorAudioFeedback/Resources`. The audio normalizer defaults to the latter project's `VO` folder; another consuming project should choose its own audio folder in the tool.

Validate through the consuming Unity project and its supported editor. This repository is an Assets tree, not a standalone Unity project. It requires the consuming game's modified KCC assembly, Bezoro precompiled dependencies, and packages. Preserve `.meta` files, GUIDs, assembly references, and serialized asset references when changing utilities. The `~Docs` directory is ignored by Unity asset import.

The root `.gitattributes` retains the consuming game's existing line-ending and Git LFS policy. Install Git LFS before cloning or publishing assets covered by its filters, and fetch LFS objects when needed. Keep this policy with the framework so its Git blobs use the same normalization as the game source. Before an authorized framework branch push, run `git lfs push origin <framework-branch>` if a Git LFS pre-push hook is unavailable; upload the LFS objects before publishing the game pointer.
