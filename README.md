# Bezoro Unity Framework

This repository is a Unity 6000.6.3f1 project. Add the repository root to Unity Hub, or open it with the exact Editor recorded in `ProjectSettings/ProjectVersion.txt`.

```text
Assets/Editor/                         Project-owned native test runner
Packages/manifest.json                 Project dependencies and package testables
Packages/com.bezoro.unity-framework/    Embedded UPM package, edited in place
ProjectSettings/ProjectVersion.txt     Pinned Unity Editor and revision
```

There is one project and one editable package. There is no separate development project or source copy. Unity resolves the embedded package automatically from `Packages/com.bezoro.unity-framework`; the project manifest enables its tests without installing a second copy.

## Install in another Unity project

Install the published pure C# Framework and UniTask Git dependencies first. Add these entries to the other project's `Packages/manifest.json`, preserving its existing dependencies:

```json
{
  "dependencies": {
    "com.bezoro.framework": "https://github.com/bezoro/framework.git#c9ed5cf0224afdd7256968cde5e3b3a58b80bc3e",
    "com.bezoro.unity-framework": "https://github.com/bezoro/unity--bezoro-framework.git?path=Packages/com.bezoro.unity-framework#<tag-or-commit>",
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#ceac8d6946b1125fe782cd171fbcb245b567dbf9"
  }
}
```

Replace `<tag-or-commit>` with a published revision containing this layout. The Package Manager's **Add package from git URL** accepts the same utilities URL. The `?path=Packages/com.bezoro.unity-framework` part is required; the repository root is the project, not a package. To install from disk, select `Packages/com.bezoro.unity-framework/package.json` in **Add package from disk**.

Unity does not support Git URLs in a package's dependency list. Framework and UniTask therefore belong in each consuming project's manifest. Unity registry dependencies are declared by the utilities package. Let Unity generate `Packages/packages-lock.json` after a real resolution and commit that lock; no fabricated lock or copied Framework DLLs are provided.

## Dependencies and validation

The package uses the published pure C# Framework, UniTask and the Unity registry packages listed in its manifest. It no longer includes the character-controller integration or the commercial drawing integration. The UI component tests use reflection directly and do not depend on a consuming game's editor helpers.

Do not treat manifest or metadata checks as proof of successful Unity import. Native resolution, compilation and full EditMode/PlayMode validation remain pending. The existing CI run stopped at the missing Unity credentials check before launching the Editor. See [CI_GUIDE.md](CI_GUIDE.md).

## Migrate an existing installation

Version 0.2.0 moves the package from the repository root to `Packages/com.bezoro.unity-framework`. Update existing Git package URLs with the `?path=...` parameter, or update disk references to the package's new `package.json`. Retained source, test and assembly GUIDs are preserved.

KCC state machines, their extension method, script creation menu, template and editor drawing integration have been removed. Projects using `Bezoro.Unity.Core.StateMachines.KCCBased` must migrate those components and API usages before upgrading. Event-based and tickable state machines remain available. No replacement controller is included.

The old installation of the whole repository under a game's `Assets` directory is no longer supported by this project layout. Remove that legacy source installation before installing the UPM package, preserving any game-owned assets and their references. Do not import both copies. This change does not automatically modify another game's manifest, submodule or package lock.
