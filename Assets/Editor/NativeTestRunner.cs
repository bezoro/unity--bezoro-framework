using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Bezoro.Unity.Framework.Validation
{
    // Test infrastructure only. Discovery and execution use the real Unity Test Runner API.
    [InitializeOnLoad]
    public static class NativeTestRunner
    {
        private const string SessionPrefix = "com.bezoro.unity-framework.ci.";
        private static readonly Callbacks Listener = new Callbacks();
        private static TestRunnerApi _api;
        private static Settings _settings;
        // UTF's command-line runner waits for this job-level signal, which includes PlayMode exit
        // and post-run cleanup. It is internal in UTF 1.4.6; use a strict signature lookup so the
        // pinned UTF 1.8.0 must expose the same signal instead of exiting from RunFinished early.
        private static readonly System.Reflection.MethodInfo IsRunActiveMethod = typeof(TestRunnerApi).GetMethod(
            "IsRunActive", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Static, null, Type.EmptyTypes, null);

        static NativeTestRunner()
        {
            if (!TryReadSettings(out _settings)) return;
            // Register again after PlayMode domain reloads; SessionState belongs to this Editor process.
            _api = ScriptableObject.CreateInstance<TestRunnerApi>();
            _api.hideFlags = HideFlags.HideAndDontSave;
            _api.RegisterCallbacks(Listener, -1000);
            EditorApplication.update += CheckDeadline;
        }

        public static void Run()
        {
            try
            {
                if (_settings == null || _api == null)
                    throw new InvalidOperationException("Missing framework native-test command arguments.");
                if (SessionState.GetBool(SessionPrefix + "started", false))
                    throw new InvalidOperationException("Native test execution was already started in this process.");
                if (IsRunActiveMethod == null || IsRunActiveMethod.ContainsGenericParameters || IsRunActiveMethod.ReturnType != typeof(bool))
                    throw new InvalidOperationException("The pinned Unity Test Framework must expose static bool TestRunnerApi.IsRunActive().");
                SessionState.SetBool(SessionPrefix + "started", true);
                SessionState.SetString(SessionPrefix + "deadline",
                    DateTime.UtcNow.AddMinutes(40).Ticks.ToString(CultureInfo.InvariantCulture));

                _api.RetrieveTestList(_settings.Mode, tree =>
                {
                    try
                    {
                        var cases = new List<Case>();
                        CollectCases(tree, false, cases);
                        if (cases.Count == 0 || cases.Select(item => item.fullname).Distinct().Count() != cases.Count)
                            throw new InvalidOperationException("Native discovery must return nonempty distinct package cases.");
                        var discovery = new Discovery
                        {
                            mode = _settings.ModeName,
                            assembly = _settings.Assembly,
                            editor = Application.unityVersion,
                            cases = cases.OrderBy(item => item.fullname, StringComparer.Ordinal).ToArray()
                        };
                        Directory.CreateDirectory(Path.GetDirectoryName(_settings.DiscoveryPath));
                        File.WriteAllText(_settings.DiscoveryPath, JsonUtility.ToJson(discovery, true));
                        _api.Execute(new ExecutionSettings(new Filter
                        {
                            testMode = _settings.Mode,
                            assemblyNames = new[] { _settings.Assembly }
                        }) { runSynchronously = false });
                    }
                    catch (Exception exception) { Fail(exception); }
                });
            }
            catch (Exception exception) { Fail(exception); }
        }

        private static void CollectCases(ITestAdaptor node, bool ownedAssembly, List<Case> cases)
        {
            if (node.IsTestAssembly)
                ownedAssembly = node.Name == _settings.Assembly || node.Name == _settings.Assembly + ".dll";
            if (!node.IsSuite && ownedAssembly)
            {
                if (node.Method == null || node.TypeInfo == null)
                    throw new InvalidOperationException("Native discovery returned a case without its method/type identity.");
                cases.Add(new Case
                {
                    fullname = node.FullName,
                    classname = node.TypeInfo.FullName,
                    methodname = node.Method.Name
                });
            }
            if (node.HasChildren)
                foreach (var child in node.Children) CollectCases(child, ownedAssembly, cases);
        }

        private static void CheckDeadline()
        {
            if (!SessionState.GetBool(SessionPrefix + "started", false)) return;
            if (SessionState.GetBool(SessionPrefix + "finished", false))
            {
                Exit(SessionState.GetInt(SessionPrefix + "exitCode", 1));
                return;
            }
            if (!long.TryParse(SessionState.GetString(SessionPrefix + "deadline", ""),
                    NumberStyles.None, CultureInfo.InvariantCulture, out var deadline) || DateTime.UtcNow.Ticks > deadline)
            {
                Debug.LogException(new TimeoutException("Native discovery/execution/cleanup exceeded its bounded deadline."));
                Exit(1);
                return;
            }
            if (!SessionState.GetBool(SessionPrefix + "resultReceived", false)) return;
            try
            {
                if (IsRunActiveMethod == null || IsRunActiveMethod.ContainsGenericParameters || IsRunActiveMethod.ReturnType != typeof(bool))
                    throw new InvalidOperationException("The pinned Unity Test Framework no longer exposes its job completion signal.");
                if ((bool)IsRunActiveMethod.Invoke(null, null)) return;
                Exit(SessionState.GetInt(SessionPrefix + "exitCode", 1));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Exit(1);
            }
        }

        private static void Fail(Exception exception)
        {
            Debug.LogException(exception);
            if (!SessionState.GetBool(SessionPrefix + "started", false))
            {
                Exit(1);
                return;
            }
            Finish(1);
        }

        private static void Finish(int exitCode)
        {
            // These values survive Enter/Exit PlayMode domain reloads. A later cleanup error
            // must override a passing test result, and a reload must not lose a pending exit.
            if (exitCode != 0 || !SessionState.GetBool(SessionPrefix + "resultReceived", false))
                SessionState.SetInt(SessionPrefix + "exitCode", exitCode);
            SessionState.SetBool(SessionPrefix + "resultReceived", true);
        }

        private static void Exit(int exitCode)
        {
            SessionState.SetInt(SessionPrefix + "exitCode", exitCode);
            SessionState.SetBool(SessionPrefix + "finished", true);
            EditorApplication.update -= CheckDeadline;
            if (_api != null) _api.UnregisterCallbacks(Listener);
            EditorApplication.Exit(exitCode);
        }

        private static bool TryReadSettings(out Settings settings)
        {
            settings = null;
            var args = Environment.GetCommandLineArgs();
            string Read(string key)
            {
                var index = Array.IndexOf(args, key);
                return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
            }
            var mode = Read("-frameworkTestMode");
            var report = Read("-frameworkTestResults");
            var discovery = Read("-frameworkTestDiscovery");
            if ((mode != "editmode" && mode != "playmode") || string.IsNullOrEmpty(report) || string.IsNullOrEmpty(discovery))
                return false;
            settings = new Settings
            {
                ModeName = mode,
                Mode = mode == "editmode" ? TestMode.EditMode : TestMode.PlayMode,
                Assembly = mode == "editmode" ? "Bezoro.Unity.Core.Tests.EditMode" : "Bezoro.Unity.Core.Tests.PlayMode",
                ResultsPath = Path.GetFullPath(report),
                DiscoveryPath = Path.GetFullPath(discovery)
            };
            return true;
        }

        private sealed class Settings
        {
            public string ModeName;
            public TestMode Mode;
            public string Assembly;
            public string ResultsPath;
            public string DiscoveryPath;
        }

        [Serializable]
        private sealed class Discovery
        {
            public int schema = 1;
            public string producer = "Unity TestRunnerApi.RetrieveTestList";
            public string mode;
            public string assembly;
            public string editor;
            public Case[] cases;
        }

        [Serializable]
        private sealed class Case
        {
            public string fullname;
            public string classname;
            public string methodname;
        }

        private sealed class Callbacks : IErrorCallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }

            public void OnError(string message)
            {
                if (SessionState.GetBool(SessionPrefix + "started", false))
                    Fail(new InvalidOperationException("Native test infrastructure failed: " + message));
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                if (!SessionState.GetBool(SessionPrefix + "started", false)) return;
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_settings.ResultsPath));
                    TestRunnerApi.SaveResultToFile(result, _settings.ResultsPath);
                    // SaveResultToFile catches I/O errors internally; do not treat an absent report as success.
                    if (!File.Exists(_settings.ResultsPath) || new FileInfo(_settings.ResultsPath).Length == 0)
                        throw new IOException("Unity did not write a nonempty native test result report.");
                    Finish(result.TestStatus == TestStatus.Passed && result.PassCount > 0 && result.FailCount == 0 && result.SkipCount == 0 &&
                        result.InconclusiveCount == 0 ? 0 : 1);
                }
                catch (Exception exception) { Fail(exception); }
            }
        }
    }
}
