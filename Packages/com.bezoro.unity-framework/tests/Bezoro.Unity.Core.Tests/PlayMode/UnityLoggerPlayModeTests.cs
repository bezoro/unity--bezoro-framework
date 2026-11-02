#nullable enable
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bezoro.Logging;
using Bezoro.Logging.Types;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Logger = Bezoro.Logging.Logger;
namespace Bezoro.Unity.Core.Tests.PlayMode
{
	public sealed class UnityLoggerPlayModeTests
	{
		[TestCase(LogLevel.Info, LogType.Log)]
		[TestCase(LogLevel.Warning, LogType.Warning)]
		[TestCase(LogLevel.Error, LogType.Error)]
		[TestCase(LogLevel.Exception, LogType.Exception)]
		[TestCase(LogLevel.Success, LogType.Log)]
		[TestCase(LogLevel.Divider, LogType.Log)]
		public void PublicLogging_ForwardsSupportedLevelExactlyOnce(LogLevel level, LogType expected)
		{
			Bezoro.Unity.Core.Logging.UnityLogger.Initialize();
			Bezoro.Unity.Core.Logging.UnityLogger.Initialize();
			Bezoro.Unity.Core.Logging.UnityLogger.Initialize();
			bool enabled = LoggerSettings.Enabled;
			LogLevel minimum = Logger.MinimumLevel;
			LoggerSettings.Enabled = true;
			Logger.MinimumLevel = LogLevel.Info;
			string token = "unity-logger-" + Guid.NewGuid().ToString("N");
			var observed = new List<LogType>();
			void Observe(string text, string stack, LogType type)
			{
				if (text.Contains(token)) observed.Add(type);
			}
			if (expected != LogType.Log) LogAssert.Expect(expected, new Regex(Regex.Escape(token)));
			UnityEngine.Application.logMessageReceived += Observe;
			try
			{
				if (level == LogLevel.Exception) Logger.LogException(new InvalidOperationException(token));
				else Logger.Log(token, level);
				Assert.That(observed, Is.EqualTo(new[] { expected }));
			}
			finally
			{
				UnityEngine.Application.logMessageReceived -= Observe;
				LoggerSettings.Enabled = enabled;
				Logger.MinimumLevel = minimum;
			}
		}
	}
}
