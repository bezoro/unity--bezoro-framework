// using System;
// using System.Diagnostics;
// using System.Text;
// using Bezoro.Framework.Unity.Utilities.String;
// using UnityEngine;
// using Debug = UnityEngine.Debug;
// using Object = UnityEngine.Object;
//
// namespace Bezoro.Framework.Unity.Utilities
// {
// 	public sealed class Log_Builder
// 	{
// 		public Log_Builder(object message)
// 		{
// 			_message = message;
// 		}
//
// 		~Log_Builder()
// 		{
// 			if (!_has_been_sent)
// 			{
// 				Debug.LogWarning(
// 					$"Log_Builder was not sent. Did you forget to call Send()?: {_message}",
// 					_context
// 				);
// 			}
// 		}
//
// 		private readonly StringBuilder _format_builder = new();
// 		private readonly object        _message;
// 		private          string        _caller_name;
// 		private          string        _category  = Debug_Settings.DEFAULT_CATEGORY;
// 		private          string        _color_tag = string.Empty;
// 		private          Object        _context;
// 		private          bool          _has_been_sent;
// 		private          LogType       _log_type = LogType.Log;
// 		private          string        _method_name;
//
// 		// Converts to properties
// 		public Object Context
// 		{
// 			set => _context = value;
// 		}
//
// 		public LogType Type
// 		{
// 			set => _log_type = value;
// 		}
//
// 		public string Category
// 		{
// 			set => _category = value.Is_Null_Or_Empty() ? Debug_Settings.DEFAULT_CATEGORY : value;
// 		}
//
// 		public Color Colour
// 		{
// 			set => _color_tag = $"<color=#{ColorUtility.ToHtmlStringRGB(value)}>";
// 		}
//
// 		// Retain capability of text formatting
// 		public Log_Builder Bold()
// 		{
// 			return ApplyTextFormat(s => s.Bold());
// 		}
//
// 		public Log_Builder Italic()
// 		{
// 			return ApplyTextFormat(s => s.Italic());
// 		}
//
// 		public Log_Builder Underline()
// 		{
// 			return ApplyTextFormat(s => s.Underline());
// 		}
//
// 		public Log_Builder Strikethrough()
// 		{
// 			return ApplyTextFormat(s => s.Strikethrough());
// 		}
//
// 		public Log_Builder Size(int size)
// 		{
// 			return ApplyTextFormat(s => s.Size(size));
// 		}
//
// 		[Conditional("UNITY_EDITOR")]
// 		[Conditional("DEVELOPMENT_BUILD")]
// 		public void Send()
// 		{
// 			if (!Debug_Settings.Instance.enableLogging)
// 			{
// 				return;
// 			}
//
// 			_has_been_sent = true;
// 			var categorySettings = BDebug.GetCategorySettings(_category);
//
// 			if (categorySettings is not { isEnabled: true })
// 			{
// 				return;
// 			}
//
// 			var message = BuildAndSendMessage(categorySettings);
// 			SendToUnityConsole(message);
// 		}
//
// 		public Log_Builder Caller(object caller)
// 		{
// 			_caller_name = caller.GetType().Name.Bold().Color((Color)new( 0.2f, 0.4f, 1f ));
//
// 			return this;
// 		}
//
// 		public Log_Builder Method(string methodName)
// 		{
// 			_method_name = methodName.Bold().Color((Color)new( 0.2f, 0.4f, 1f ));
//
// 			return this;
// 		}
//
// 		// Private method to apply text format
// 		private Log_Builder ApplyTextFormat(Func<string, string> formatFunc)
// 		{
// 			_format_builder.Clear();
// 			_format_builder.Append(formatFunc(_message.ToString()));
// 			return this;
// 		}
//
// 		private string BuildAndSendMessage(Debug_Settings.DebugCategory category)
// 		{
// 			var prefix = $"[{category.name}]: ".Color(category.categoryColor);
//
// 			var content = _format_builder.Length > 0
// 				? _format_builder.ToString()
// 				: $"{_color_tag}{_message}{( _color_tag == string.Empty ? string.Empty : "</color>" )}";
//
// 			var methodInfo = string.IsNullOrEmpty(_caller_name) || string.IsNullOrEmpty(_method_name)
// 				? string.Empty
// 				: $"{_caller_name}.{_method_name}() => ";
//
// 			return $"{prefix}{methodInfo}{content}";
// 		}
//
// 		private void SendToUnityConsole(string finalMessage)
// 		{
// 			switch ( _log_type )
// 			{
// 				case LogType.Log:
// 					Debug.Log(
// 						finalMessage,
// 						_context
// 					);
//
// 					break;
//
// 				case LogType.Warning:
// 					Debug.LogWarning(
// 						finalMessage,
// 						_context
// 					);
//
// 					break;
//
// 				case LogType.Error:
// 					Debug.LogError(
// 						finalMessage,
// 						_context
// 					);
//
// 					break;
//
// 				case LogType.Exception:
// 					Debug.LogException(
// 						_message as Exception ?? new Exception(finalMessage),
// 						_context
// 					);
//
// 					break;
//
// 				default:
// 					Debug.LogError(
// 						$"Unknown log type: {_log_type}",
// 						_context
// 					);
//
// 					break;
// 			}
// 		}
// 	}
// }


