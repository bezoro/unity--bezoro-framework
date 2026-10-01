#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.Application;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Logging;
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
/// <summary>
/// Adapts framework logging calls to Unity's Debug logger.
/// </summary>
public static class UnityLogger
{
	static UnityLogger()
	{
		LoggerSettings.Stage      = StageConfig.Create(() => ApplicationLifecycle.CurrentStage.ToString());
		LoggerSettings.FrameCount = FrameCountConfig.Create(() => Time.frameCount);

		Logger.OnLog -= HandleLog;
		Logger.OnLog += HandleLog;
	}

	[HideInCallstack]
	private static void HandleLog(LogPayload logPayload)
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
				break;
			case LogLevel.Divider:
				Debug.Log("[DIVIDER]" + logPayload.Message, unityObject);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}
