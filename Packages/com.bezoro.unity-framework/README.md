# Bezoro Unity Framework

Unity runtime utilities and native editor tools, distributed as `com.bezoro.unity-framework`. This directory is the UPM package root. The enclosing repository is the Unity 6000.6.3f1 project used to develop and test this embedded package.

## Framework dependency

Version `0.4.1` uses `com.bezoro.framework` version `0.1.0-preview.1736`. Use the published Git package at commit `68f51f8484585174bce586b9c015da483dfdbaf7` in `bezoro/csharp-framework`. The repository's `Packages/manifest.json` pins that commit and the UniTask revision. Unity imports this embedded package directly from `Packages/com.bezoro.unity-framework`.

Open the repository root in the exact Editor recorded in `ProjectSettings/ProjectVersion.txt`. Package Manager resolves the Git dependencies. Keep its generated `Packages/packages-lock.json` with the project after resolution. No lock file or PackageCache is fabricated outside Unity.

The published Framework package contains the compiled .NET Standard assemblies and their dependency closure. Entries such as `Bezoro.Core.dll` in an assembly definition select assemblies supplied by UPM. Do not copy those DLLs into `Assets`, `src`, or another package, and remove duplicate manually installed copies from the development project before import.

Unity permits Git dependencies in a [project manifest](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-git.html), not as Git URLs in a package's `dependencies`. This is why each consuming project must declare the Framework and UniTask Git URLs, while this package declares its Unity registry dependencies in its own `package.json`.

## Upgrading to 0.4.0

- Call `Safe_Dispose()` on the writable `NativeArray<T>` variable that owns the allocation. Static calls must pass it by `ref`. The helper clears that variable when it disposes the allocation.
- Use `await PlayerLoopHelpers.ReturnToMainThread()` to resume the calling method on the Unity thread. Use the action overload to post a callback.
- For a deferred scene load, check `SceneLoadHandle.IsReadyForActivation` and call `Activate()` on the Unity main thread before awaiting completion. Canceling the handle also releases the activations it owns. Started Unity operations finish and emit their completion notifications. Loads and unloads that already match the requested state complete successfully.
- `EventBus.Publish` and `PublishAsync` complete after their own handlers return. A handler can queue a nested publication with `.Forget()` and return so the bus can dispatch it. Higher priorities go first among pending events, with FIFO ordering at equal priority. A payload event created by the bus returns to its pool after publication. `CreateEvent<TEvent>()` creates a fresh instance.
- `ObservableProperty` invokes subscribers outside its state lock on the calling thread. Subscribers that can receive concurrent updates must handle overlapping callbacks. A disposed `ServiceLocator` is terminal. Create a new locator for the next lifetime.
- For checked `InputAction` operations using extension syntax, use `EnableChecked()` and `DisableChecked()`. The existing static wrappers remain available. NavMesh random sampling defaults to a finite search radius of 10 world units and returns the agent's position if it finds no valid sample.
- Assign the runtime `GameInitializationConfig` in the resource-backed `GameSettings` to satisfy application-wide provider requirements during build validation. Prefab conversion and retargeting refuse edits when Unity cannot preserve the target's object identities or map its overrides safely.

## Fix in 0.3.3

Component enabled-state helpers now read and write native Renderer and Collider properties, as well as other public boolean `enabled` properties such as LODGroup and Cloth. Components without an enabled member remain unchanged. `ComponentState` and hierarchy-cache snapshots therefore retain each component's actual initial state, including intentionally disabled renderers and colliders.

Consumers with serialized hierarchy caches must rebuild and save them through the native Editor after upgrading. Caches produced by older helpers may record an enabled Renderer or Collider as disabled; updating the package alone does not rewrite consumer assets.

## Fix in 0.3.2

In Edit Mode, hierarchy caches exclude GameObjects and components marked `DontSaveInEditor` or `DontSaveInBuild`, including descendants of excluded GameObjects. `StartDisabled` references use the same hierarchy rule. Visibility flags alone do not exclude authored content. Equivalent disabled-target membership preserves the existing serialized order, avoiding unnecessary prefab overrides.

Runtime rebuilding and public `GameObjectState` snapshots still capture temporary objects and components. Existing caches containing temporary references rebuild on the next normal cache refresh. Legacy prefab or scene data may still require a separate native migration. This change does not rewrite consumer assets.

## Fix in 0.3.1

The Array Helpers configuration window treats a missing configuration as its supported empty state without logging a missing-singleton warning. It retains an assigned configuration and searches project assets in ordinal path order when none is assigned.

## Breaking changes in 0.3.0

All state-machine code has been removed. This includes event-based and tickable machines, states, owners, conditions, transitions, hierarchy extensions, editor menus, gizmos and templates. Consumers using `Bezoro.Unity.Core.StateMachines` or `Bezoro.Unity.Core.Editor.StateMachine`, including their serialized components, must migrate to the separately maintained implementation before upgrading. No replacement dependency is added here.

The independent `Bezoro.Unity.Core.Tickable` update dispatcher and its tick interfaces are retained unchanged. UI component tests use reflection directly and do not require a game's editor helpers.

Version 0.2.0 moved the package into `Packages/com.bezoro.unity-framework` and removed the character-controller and commercial drawing integrations. Its Git subpath migration still applies.

## Package development

`src` and `tests` keep their existing assembly names, internal paths and Unity GUIDs. The repository's `testables` entry enables this package's tests. Project-owned assets, settings and CI tools are outside the package and are not imported by consumers.

For another project, add the Framework and UniTask Git dependencies to that project's manifest. Install this package from disk using this directory's `package.json`, or use `https://github.com/bezoro/unity--bezoro-framework.git?path=Packages/com.bezoro.unity-framework#<tag-or-commit>`, replacing the revision with a published tag or commit containing this layout. The old repository-root URL must be updated. Remove a legacy Assets/submodule installation before using UPM; never import both copies.

Install [Git LFS](https://git-lfs.com/) and run `git lfs install` before installing from Git. For an existing checkout used by a disk installation, run `git lfs pull` to materialize the Symlink Utility documentation image.

This project does not change any existing game, its package lock, or its submodule revision. Native Unity resolution, compilation and tests need their own recorded results; package metadata checks alone do not establish those results.

## License

Bezoro-authored code is licensed under the [MIT License](LICENSE.md). Package metadata declares the SPDX identifier `MIT`. Version 0.3.4 adds the repository and package license notices.

Bundled Symlink Utility code retains its original [MIT license and copyright notice](src/Bezoro.Unity.Core.Editor/SymlinkUtility/LICENSE), Copyright (c) 2016 Karl Henkel. Other dependencies retain their respective licenses.
