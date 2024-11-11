using System;
using System.Diagnostics;
using System.Text;
using Bezoro.Framework.Unity.Utilities;
using Bezoro.Framework.Unity.Utilities.Extensions;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public sealed class LogBuilder
{
	private string _callerName;
	private string _methodName;
	private readonly object _message;
	private Object _context;
	private LogType _logType = LogType.Log;
	private string _category = DebugSettings.DEFAULT_CATEGORY;
	private string _colorTag = string.Empty;
	private readonly StringBuilder _formatBuilder = new();
	private bool _hasBeenSent;

	public LogBuilder(object message)
	{
		_message = message;
	}

	~LogBuilder()
	{
		if (!_hasBeenSent)
		{
			Debug.LogWarning($"LogBuilder was not sent. Did you forget to call Send()?: {_message}", _context);
		}
	}

	public LogBuilder ContextObject(Object contextObject)
	{
		_context = contextObject;

		return this;
	}

	public LogBuilder Type(LogType type)
	{
		_logType = type;

		return this;
	}

	public LogBuilder Category(string category)
	{
		if (category.IsNullOrEmpty())
		{
			category = DebugSettings.DEFAULT_CATEGORY;
		}

		_category = category;

		return this;
	}

	public LogBuilder Colour(Color color)
	{
		_colorTag = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";

		return this;
	}

	public LogBuilder Bold()
	{
		return ApplyTextFormat(str => str.Bold());
	}

	public LogBuilder Italic()
	{
		return ApplyTextFormat(str => str.Italic());
	}

	public LogBuilder Underline()
	{
		return ApplyTextFormat(str => str.Underline());
	}

	public LogBuilder Strikethrough()
	{
		return ApplyTextFormat(str => str.Strikethrough());
	}

	public LogBuilder Size(int size)
	{
		return ApplyTextFormat(str => str.Size(size));
	}

	private LogBuilder ApplyTextFormat(Func<string, string> formatFunc)
	{
		_formatBuilder.Clear();
		_formatBuilder.Append(formatFunc(_message.ToString()));

		return this;
	}

	[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
	public void Send()
	{
		_hasBeenSent = true;
		var categorySettings = BDebug.GetCategorySettings(_category);
		if (categorySettings is not { isEnabled: true })
		{
			return;
		}

		var finalMessage = BuildMessage(categorySettings, _callerName, _methodName);
		SendToUnityConsole(finalMessage);
	}

	private string BuildMessage(DebugSettings.DebugCategory category, string callerName, string methodName)
	{
		var prefix = $"[{category.name}]: ".Color(category.categoryColor);
		var content = _formatBuilder.Length > 0
			? _formatBuilder.ToString()
			: $"{_colorTag}{_message}{( _colorTag == string.Empty ? string.Empty : "</color>" )}";

		var methodInfo = string.IsNullOrEmpty(callerName) || string.IsNullOrEmpty(methodName)
			? string.Empty
			: $"{callerName}.{methodName}() => ";

		return $"{prefix}{methodInfo}{content}";
	}

	[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
	private void SendToUnityConsole(string finalMessage)
	{
		switch ( _logType )
		{
			case LogType.Log:
				Debug.Log(finalMessage, _context);

				break;
			case LogType.Warning:
				Debug.LogWarning(finalMessage, _context);

				break;
			case LogType.Error:
				Debug.LogError(finalMessage, _context);

				break;
			case LogType.Exception:
				Debug.LogException(_message as Exception ?? new Exception(finalMessage), _context);

				break;
			default:
				Debug.LogError($"Unknown log type: {_logType}", _context);

				break;
		}
	}

	public LogBuilder Caller(object caller)
	{
		_callerName = caller.GetType().Name.Bold().Color(new Color(0.2f, 0.4f, 1f));

		return this;
	}

	public LogBuilder Method(string methodName)
	{
		_methodName = methodName.Bold().Color(new Color(0.2f, 0.4f, 1f));

		return this;
	}
}
