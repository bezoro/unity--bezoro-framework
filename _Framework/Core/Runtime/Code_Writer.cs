using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bezoro.Framework.Core
{
	public class Code_Writer
	{
		private const string _INDENT_STRING = "    "; // 4 spaces

		private readonly StringBuilder   _builder = new();
		private readonly HashSet<string> _usings  = new();

		private int  _indentLevel;
		private bool _isNewLine = true;

		public Code_Writer AddUsing(string nameSpace)
		{
			_usings.Add(nameSpace);
			return this;
		}

		public Code_Writer AddUsingRange(IEnumerable<string> namespaces)
		{
			foreach ( var @namespace in namespaces ) AddUsing(@namespace);
			return this;
		}

		public IDisposable BeginScope(string declaration = null)
		{
			if (declaration != null) WriteLine(declaration);
			WriteLine("{");
			_indentLevel++;
			return new ScopeGuard(this);
		}

		public void EndScope(string closer = "}")
		{
			_indentLevel--;
			WriteLine(closer);
		}

		public Code_Writer WriteLine(string line = "")
		{
			if (!string.IsNullOrEmpty(line))
			{
				WriteIndent();
				_builder.AppendLine(line);
			}
			else _builder.AppendLine();

			_isNewLine = true;
			return this;
		}

		public Code_Writer Write(string text)
		{
			if (_isNewLine)
			{
				WriteIndent();
				_isNewLine = false;
			}

			_builder.Append(text);
			return this;
		}

		public override string ToString()
		{
			var final_builder = new StringBuilder();

			// Write file header
			final_builder.AppendLine("// This file is auto-generated. Do not modify.");
			final_builder.AppendLine();

			// Write usings
			foreach ( var @using in _usings.OrderBy(x => x) ) final_builder.AppendLine($"using {@using};");
			if (_usings.Any()) final_builder.AppendLine();

			// Write main content
			final_builder.Append(_builder.ToString());
			return final_builder.ToString();
		}

		private void WriteIndent()
		{
			for (var i = 0 ; i < _indentLevel ; i++) _builder.Append(_INDENT_STRING);
		}

		private class ScopeGuard : IDisposable
		{
			public ScopeGuard(Code_Writer writer)
			{
				_writer = writer;
			}

			private readonly Code_Writer _writer;

			public void Dispose()
			{
				_writer.EndScope();
			}
		}
	}

	public class CSharpCodeBuilder
	{
		private readonly Stack<string> _namespaceStack = new();
		private readonly Code_Writer   _writer         = new();

		private bool _isInClass;

		public CSharpCodeBuilder AddUsing(string nameSpace)
		{
			_writer.AddUsing(nameSpace);
			return this;
		}

		public CSharpCodeBuilder BeginNamespace(string nameSpace)
		{
			_namespaceStack.Push(nameSpace);
			_writer.WriteLine($"namespace {nameSpace}");
			_writer.BeginScope();
			return this;
		}

		public CSharpCodeBuilder EndNamespace()
		{
			if (_namespaceStack.Count == 0)
				throw new InvalidOperationException("No namespace to end");

			_namespaceStack.Pop();
			_writer.EndScope();
			return this;
		}

		public CSharpCodeBuilder BeginClass(
			string className,
			string accessibility = "public",
			bool isStatic = false,
			string baseClass = null,
			string[] interfaces = null
		)
		{
			if (_isInClass) throw new InvalidOperationException("Already in a class definition");

			var declaration = new StringBuilder();
			declaration.Append($"{accessibility} ");
			if (isStatic) declaration.Append("static ");
			declaration.Append($"class {className}");
			if (baseClass != null) declaration.Append($" : {baseClass}");

			if (interfaces is { Length: > 0 })
			{
				declaration.Append(baseClass == null ? " : " : ", ");
				declaration.Append(string.Join(", ", interfaces));
			}

			_writer.WriteLine(declaration.ToString());
			_writer.BeginScope();
			_isInClass = true;
			return this;
		}

		public CSharpCodeBuilder EndClass()
		{
			if (!_isInClass) throw new InvalidOperationException("Not in a class definition");

			_writer.EndScope();
			_isInClass = false;
			return this;
		}

		public CSharpCodeBuilder AddMethod(
			string name,
			string returnType = "void",
			string accessibility = "public",
			bool isStatic = false,
			string[] parameters = null,
			Action<Code_Writer> bodyWriter = null,
			string[] attributes = null
		)
		{
			if (!_isInClass) throw new InvalidOperationException("Must be in a class to add a method");

			// Write attributes
			if (attributes != null)
			{
				foreach ( var attribute in attributes )
					_writer.WriteLine($"[{attribute}]");
			}

			var declaration = new StringBuilder();
			declaration.Append($"{accessibility} ");

			if (isStatic) declaration.Append("static ");

			declaration.Append($"{returnType} {name}(");

			if (parameters is { Length: > 0 }) declaration.Append(string.Join(", ", parameters));

			declaration.Append(")");
			_writer.WriteLine(declaration.ToString());

			using ( _writer.BeginScope() ) bodyWriter?.Invoke(_writer);

			return this;
		}

		public CSharpCodeBuilder AddProperty(
			string name,
			string type,
			string accessibility = "public",
			bool hasGetter = true,
			bool hasSetter = true,
			string setterAccessibility = null
		)
		{
			if (!_isInClass) throw new InvalidOperationException("Must be in a class to add a property");

			var declaration = $"{accessibility} {type} {name}";

			if (!hasGetter && !hasSetter)
				throw new ArgumentException("Property must have at least a getter or setter");

			_writer.Write(declaration);

			using ( _writer.BeginScope(" {") )
			{
				if (hasGetter) _writer.WriteLine("get;");
				if (hasSetter) _writer.WriteLine($"{setterAccessibility ?? ""} set;".TrimStart());
			}

			return this;
		}

		public CSharpCodeBuilder AddField(
			string name,
			string type,
			string accessibility = "private",
			bool isReadonly = false,
			string initialValue = null
		)
		{
			if (!_isInClass)
				throw new InvalidOperationException("Must be in a class to add a field");

			var declaration = new StringBuilder();
			declaration.Append($"{accessibility} ");

			if (isReadonly) declaration.Append("readonly ");

			declaration.Append($"{type} {name}");

			if (initialValue != null) declaration.Append($" = {initialValue}");

			declaration.Append(";");

			_writer.WriteLine(declaration.ToString());
			return this;
		}

		public string Generate()
		{
			if (_namespaceStack.Count > 0)
				throw new InvalidOperationException("Unclosed namespace definitions");

			if (_isInClass) throw new InvalidOperationException("Unclosed class definition");

			return _writer.ToString();
		}
	}

	public class CSharpFileGenerator
	{
		public CSharpFileGenerator(string outputPath)
		{
			_outputPath = outputPath;
			_builder    = new();
		}

		private readonly CSharpCodeBuilder _builder;
		private readonly string            _outputPath;

		public CSharpCodeBuilder GetBuilder()
		{
			return _builder;
		}

		public void Generate()
		{
			var code = _builder.Generate();
			File.WriteAllText(_outputPath, code);
		}
	}
}
