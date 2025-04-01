using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Bezoro.Framework.Unity.Utilities.String;
using UnityEngine;
using Debug = UnityEngine.Debug;
using LogType = Bezoro.Framework.Core.LogType;
using Object = UnityEngine.Object;

namespace Bezoro.Framework.Unity
{
	public static class Logger
	{
		public static int Max_Message_Length => 1_000;

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(
			string message,
			object context = null,
			Log_Category category = Log_Category.Default
		)
		{
			LogInternal(message, context, category, LogType.Log);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void LogWarning(
			string message,
			object context = null,
			Log_Category category = Log_Category.Default
		)
		{
			LogInternal(message, context, category, LogType.Warning);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Error(
			string message,
			object context = null,
			Log_Category category = Log_Category.Default
		)
		{
			LogInternal(message, context, category, LogType.Error);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void LogSuccess(
			string message,
			object context = null,
			Log_Category category = Log_Category.Default
		)
		{
			LogInternal(message, context, category, LogType.Success);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Exception(
			Exception exception,
			bool throw_exception = true,
			object context = null,
			Log_Category category = Log_Category.Default
		)
		{
			LogInternal(exception.Message, context, category, LogType.Exception);

			if (throw_exception)
				ExceptionDispatchInfo.Capture(exception).Throw();
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		private static void LogInternal(
			string message,
			object context,
			Log_Category category,
			LogType type
		)
		{
			if (string.IsNullOrEmpty(message)) return;
			if (!Configuration.Should_Log) return;
			if (!Configuration.IsCategoryEnabled(category)) return;
			if (!Configuration.IsLogTypeAtLeastMinimumLevel(type)) return;

			var truncatedMessage = message.Length <= Max_Message_Length
				? message
				: message[..Max_Message_Length] + "... (truncated)";

			var entry        = new LogEntry(truncatedMessage, context, category, type);
			var unityContext = context as Object;

			// Dictionary of log actions based on type
			var logActions = new Dictionary<LogType, Action<LogEntry>>
			{
				[LogType.Log]     = e => Debug.Log(Format_Log_Message(e), unityContext),
				[LogType.Warning] = e => Debug.LogWarning(Format_Log_Message(e).Yellow(), unityContext),
				[LogType.Error]   = e => Debug.LogError(Format_Log_Message(e).Red(), unityContext),
				[LogType.Success] = e => Debug.Log(Format_Log_Message(e).Green(), unityContext),
				[LogType.Exception] = e =>
				{
					var exception = new Exception(Format_Log_Message(e));
					Debug.LogException(exception, unityContext);
				}
			};

			if (logActions.TryGetValue(type, out var action))
				action(entry);
		}

		private static string Format_Log_Message(LogEntry entry)
		{
			var frame = Time.frameCount;
			var time  = entry.Time.ToString("HH:mm:ss.fff");

			var context = entry.Context is Object
				? "Object: " + entry.Context
				: entry.Context?.ToString() ?? "Context Not Specified";

			var category = entry.Category.ToString();
			var message  = entry.Message;

			return $"[ Frame {frame} | {time} ] [ {context.Bold()} ] [ {category.Bold()} ] : {message}";
		}

		public static class Configuration
		{
			static Configuration()
			{
				Should_Log = true;
				Initialize_Enabled_Categories();
				Initialize_Minimum_Log_Level();
			}

			public static ConcurrentDictionary<Log_Category, bool> Enabled_Categories => _EnabledCategories;
			public static bool                                     Should_Log         { get; }

			public static LogType Minimum_Log_Level
			{
				get => _MinimumLogLevel;
				set => _MinimumLogLevel = value;
			}

			private static void Initialize_Minimum_Log_Level()
			{
				_MinimumLogLevel = LogType.Log;
			}

			private static void Initialize_Enabled_Categories()
			{
				_EnabledCategories = new();
				var categories = Enum.GetValues(typeof(Log_Category));

				foreach ( Log_Category category in categories )
				{
					_EnabledCategories[category] = true;
				}
			}

			public static bool IsLogTypeAtLeastMinimumLevel(LogType type)
			{
				return type >= _MinimumLogLevel;
			}

			public static void Enable_Category(Log_Category category)
			{
				_EnabledCategories.AddOrUpdate(category, true, (_, _) => true);
			}

			public static void Disable_Category(Log_Category category)
			{
				_EnabledCategories.AddOrUpdate(category, false, (_, _) => false);
			}

			public static void Enable_All_Categories()
			{
				_EnabledCategories.Clear();
				var categories = Enum.GetValues(typeof(Log_Category));

				foreach ( Log_Category category in categories )
				{
					_EnabledCategories[category] = true;
				}
			}

			public static void Disable_All_Categories()
			{
				_EnabledCategories.Clear();
			}

			public static bool IsCategoryEnabled(Log_Category category)
			{
				return _EnabledCategories.GetValueOrDefault(category);
			}

			private static volatile LogType                                  _MinimumLogLevel;
			private static          ConcurrentDictionary<Log_Category, bool> _EnabledCategories;
		}
	}

	public class LogEntry
	{
		public LogEntry(
			string message,
			object context,
			Log_Category category,
			LogType type
		)
		{
			_message  = message;
			_context  = context;
			_category = category;
			_time     = DateTime.Now;
			_type     = type;
		}

		public string       Message  => _message;
		public object       Context  => _context;
		public Log_Category Category => _category;
		public DateTime     Time     => _time;
		public LogType      Type     => _type;

		private string       _message;
		private object       _context;
		private Log_Category _category;
		private DateTime     _time;
		private LogType      _type;
	}

	public enum Log_Category
	{
		// Core
		Default,
		System,
		Other,

		// Development & Debugging
		Debug,
		Test,
		Profiling,
		Editor,
		Analytics,
		Performance,

		// System Core
		Memory,
		Security,
		Database,
		FileIO,
		Configuration,
		Loading,
		SaveSystem,
		Resources,
		AssetBundle,
		SceneManagement,

		// Rendering & Graphics
		Rendering,
		Particles,
		Shaders,
		PostProcessing,
		Lighting,

		// Game Systems
		Gameplay,
		Combat,
		Inventory,
		Quest,
		Dialog,
		LevelGeneration,
		Progression,
		Achievement,
		Economy,

		// Input & UI
		Input,
		UI,

		// Animation & Physics
		Animation,
		Physics,
		Physics2D,

		// AI & Behavior
		AI,

		// Audio
		Audio,

		// Networking & Services
		Network,
		Cloud,
		Authentication,
		Social,
		Purchasing,

		// Localization
		Localization
	}
}
