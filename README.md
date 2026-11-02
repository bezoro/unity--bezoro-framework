# Bezoro Unity Framework

Unity runtime utilities and native editor tools. The source is a local UPM package; `DevProject~` is its development project for Unity 6000.6.3f1.

## Framework dependency

`DevProject~/Packages/manifest.json` installs `com.bezoro.framework` from the published Git package at `c9ed5cf0224afdd7256968cde5e3b3a58b80bc3e`, version `0.1.0-preview.1671`. It also imports this checkout through `file:../..` and pins UniTask to a Git revision.

Open `DevProject~` in the exact Editor recorded in `ProjectSettings/ProjectVersion.txt`. Package Manager resolves the Git dependencies. Keep its generated `Packages/packages-lock.json` with the project after resolution. No lock file or PackageCache is fabricated outside Unity.

The published Framework package contains the compiled .NET Standard assemblies and their dependency closure. Entries such as `Bezoro.Core.dll` in an assembly definition select assemblies supplied by UPM. Do not copy those DLLs into `Assets`, `src`, or another package, and remove duplicate manually installed copies from the development project before import.

Unity permits Git dependencies in a [project manifest](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-git.html), not as Git URLs in a package's `dependencies`. This is why the Framework and UniTask Git URLs live in `DevProject~/Packages/manifest.json`, while this package declares its Unity registry dependencies in the root `package.json`.

## Existing integration requirements

The complete utilities still require dependencies that this repository does not distribute:

- The modified KCC runtime assembly, GUID `3425d614f9c95a249bba44bc0d17f503`, used by the character-controller state machines.
- ALINE, GUID `de4e6084e6d474788bb8c799d6b461ec`, used by the KCC editor labels.
- The EditMode suite currently references a consuming game's editor helpers, GUID `a0b9c4d55ebc710408ee5bc6fd470b8f`, in two button tests.

These are existing integration requirements, separate from the Framework package. Install legitimately available integrations into the development project itself. Do not point local package paths at another game's folders, copy proprietary integrations into this repository, or replace missing integrations with stubs to claim validation. Full compilation and the existing test suite remain blocked until these requirements are supplied or extracted into independent integrations.

## Package development

`src` and `tests` keep their existing assembly names, paths and Unity GUIDs. The development project's `testables` entry enables this package's tests. Unity excludes the `DevProject~` folder when importing the package, so its project files are not compiled as package content.

For another project, add the Framework and UniTask Git dependencies to that project's manifest, supply the integration requirements above, then install this package from disk using its root `package.json` or from its Git repository. Use either the package installation or the legacy Assets/submodule installation for the utilities, never both in the same project.

The development project does not change any existing game, its package lock, or its submodule revision. Native Unity resolution, compilation and tests need their own recorded results; package metadata checks alone do not establish those results.
