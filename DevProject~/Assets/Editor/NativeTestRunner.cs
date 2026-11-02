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
            if (!SessionState.GetBool(SessionPrefix + "started", false) ||
                SessionState.GetBool(SessionPrefix + "finished", false)) return;
            if (long.TryParse(SessionState.GetString(SessionPrefix + "deadline", ""),
                    NumberStyles.None, CultureInfo.InvariantCulture, out var deadline) && DateTime.UtcNow.Ticks > deadline)
                Fail(new TimeoutException("Native discovery/execution exceeded its bounded deadline."));
        }

        private static void Fail(Exception exception)
        {
            Debug.LogException(exception);
            Finish(1);
        }

        private static void Finish(int exitCode)
        {
            if (SessionState.GetBool(SessionPrefix + "finished", false)) return;
            SessionState.SetBool(SessionPrefix + "finished", true);
            EditorApplication.update -= CheckDeadline;
            EditorApplication.delayCall += () => EditorApplication.Exit(exitCode);
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

        private sealed class Callbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_settings.ResultsPath));
                    TestRunnerApi.SaveResultToFile(result, _settings.ResultsPath);
                    Finish(result.PassCount > 0 && result.FailCount == 0 && result.SkipCount == 0 &&
                        result.InconclusiveCount == 0 ? 0 : 1);
                }
                catch (Exception exception) { Fail(exception); }
            }
        }
    }
}
