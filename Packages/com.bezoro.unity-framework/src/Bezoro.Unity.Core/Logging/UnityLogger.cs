#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.Application;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Logging;
/// <summary>
/// Adapts framework logging calls to Unity's Debug logger.
/// </summary>
public static class UnityLogger
{
	private static int _frameCount;
	private static int _session;

	private sealed class FrameSampler : IPlayerLoopItem
	{
		private readonly int _generation = _session;
		public bool MoveNext()
		{
			if (_generation != _session) return false;
			CaptureFrame();
			return true;
		}
	}

	private static void CaptureFrame() => Volatile.Write(ref _frameCount, Time.frameCount);

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void StartFrameSampler() =>
		PlayerLoopHelper.AddAction(PlayerLoopTiming.EarlyUpdate, new FrameSampler());

	internal static void Initialize()
	{
		CaptureFrame();
		LoggerSettings.Stage      = StageConfig.Create(() => ApplicationLifecycle.CurrentStage.ToString());
		LoggerSettings.FrameCount = FrameCountConfig.Create(() => Volatile.Read(ref _frameCount));

		Logger.OnLog -= ForwardToUnity;
		Logger.OnLog += ForwardToUnity;
	}

#if UNITY_EDITOR
	[InitializeOnLoadMethod]
	private static void InitializeInEditor()
	{
		Initialize();
		EditorApplication.update -= CaptureFrame;
		EditorApplication.update += CaptureFrame;
	}
#endif

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void InitializeAtRuntime()
	{
		_session++;
		Initialize();
	}

	[HideInCallstack]
	internal static void ForwardToUnity(LogPayload logPayload)
	{
		Object? unityObject = null;
		if (logPayload.ContextObject is Object contextObject)
			unityObject = contextObject;

		switch (logPayload.Level)
		{
			case LogLevel.Info:
				Debug.Log(logPayload.FormattedMessage, unityObject);
				break;
			case LogLevel.Warning:
				Debug.LogWarning(logPayload.FormattedMessage, unityObject);
				break;
			case LogLevel.Error:
				Debug.LogError(logPayload.FormattedMessage, unityObject);
				break;
			case LogLevel.Exception:
				Debug.LogException(logPayload.Exception);
				break;
			case LogLevel.Success:
				Debug.Log(logPayload.FormattedMessage, unityObject);
				break;
			case LogLevel.Divider:
				Debug.Log("[DIVIDER]" + logPayload.Message, unityObject);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}
