using System;
using Bezoro.Core.Utilities;
using UnityEngine;

namespace Bezoro.Unity.Core.Common.Serialization;

/// <summary>
///     Stores a string value constrained by a tag type.
/// </summary>
[Serializable]
public class TaggedString
{
	[Tooltip("Template string with tags that will be processed. Use {tag} syntax for replaceable content.")]
	[TextArea(1, 5)]
	[SerializeField] private string template = string.Empty;

	public TaggedString() { }

	public TaggedString(string template)
	{
		this.template = template ?? string.Empty;
	}

	public string Processed => StringTags.Process(template ?? string.Empty);

	public string Template
	{
		get => template ?? string.Empty;
		set => template = value ?? string.Empty;
	}

	public static implicit operator string(TaggedString taggedString) => taggedString?.Processed ?? string.Empty;
	public static implicit operator TaggedString(string template)     => new(template);

	public override string ToString() => Processed;
}
