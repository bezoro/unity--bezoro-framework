# Bezoro Unity Framework

Unity runtime utilities and native editor tools, distributed as `com.bezoro.unity-framework`. This directory is the UPM package root. The enclosing repository is the Unity 6000.6.3f1 project used to develop and test this embedded package.

## Framework dependency

The repository's `Packages/manifest.json` installs `com.bezoro.framework` from the published Git package at `c9ed5cf0224afdd7256968cde5e3b3a58b80bc3e`, version `0.1.0-preview.1671`, and pins UniTask to a Git revision. Unity imports this embedded package directly from `Packages/com.bezoro.unity-framework`.

Open the repository root in the exact Editor recorded in `ProjectSettings/ProjectVersion.txt`. Package Manager resolves the Git dependencies. Keep its generated `Packages/packages-lock.json` with the project after resolution. No lock file or PackageCache is fabricated outside Unity.

The published Framework package contains the compiled .NET Standard assemblies and their dependency closure. Entries such as `Bezoro.Core.dll` in an assembly definition select assemblies supplied by UPM. Do not copy those DLLs into `Assets`, `src`, or another package, and remove duplicate manually installed copies from the development project before import.

Unity permits Git dependencies in a [project manifest](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-git.html), not as Git URLs in a package's `dependencies`. This is why each consuming project must declare the Framework and UniTask Git URLs, while this package declares its Unity registry dependencies in its own `package.json`.

## Fix in 0.3.1

The Array Helpers configuration window treats a missing configuration as its supported empty state without logging a missing-singleton warning. It retains an assigned configuration and searches project assets in ordinal path order when none is assigned.

## Breaking changes in 0.3.0

All state-machine code has been removed. This includes event-based and tickable machines, states, owners, conditions, transitions, hierarchy extensions, editor menus, gizmos and templates. Consumers using `Bezoro.Unity.Core.StateMachines` or `Bezoro.Unity.Core.Editor.StateMachine`, including their serialized components, must migrate to the separately maintained implementation before upgrading. No replacement dependency is added here.

The independent `Bezoro.Unity.Core.Tickable` update dispatcher and its tick interfaces are retained unchanged. UI component tests use reflection directly and do not require a game's editor helpers.

Version 0.2.0 moved the package into `Packages/com.bezoro.unity-framework` and removed the character-controller and commercial drawing integrations. Its Git subpath migration still applies.

## Package development

`src` and `tests` keep their existing assembly names, internal paths and Unity GUIDs. The repository's `testables` entry enables this package's tests. Project-owned assets, settings and CI tools are outside the package and are not imported by consumers.

For another project, add the Framework and UniTask Git dependencies to that project's manifest. Install this package from disk using this directory's `package.json`, or use `https://github.com/bezoro/unity--bezoro-framework.git?path=Packages/com.bezoro.unity-framework#<tag-or-commit>`, replacing the revision with a published tag or commit containing this layout. The old repository-root URL must be updated. Remove a legacy Assets/submodule installation before using UPM; never import both copies.

This project does not change any existing game, its package lock, or its submodule revision. Native Unity resolution, compilation and tests need their own recorded results; package metadata checks alone do not establish those results.
