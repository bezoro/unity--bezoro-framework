using System;
using System.Reflection;
using UnityEngine;

namespace Bezoro.Unity.Core.Common.Serialization;

/// <summary>
///     Serialises a <see cref="System.Reflection.FieldInfo" /> reference.
/// </summary>
[Serializable]
public sealed class SerializedFieldInfo : ISerializationCallbackReceiver
{
	[SerializeField] private string? assemblyQualifiedDeclaringType;
	[SerializeField] private string? fieldName;

	[NonSerialized] private FieldInfo? _runtimeField;

	public SerializedFieldInfo() { }

	public SerializedFieldInfo(FieldInfo field)
	{
		Set(field);
	}

	public FieldInfo? RuntimeField => _runtimeField;

	public static implicit operator FieldInfo?(SerializedFieldInfo sfi) => sfi._runtimeField;
	public static implicit operator SerializedFieldInfo(FieldInfo  fi)  => new(fi);

	public void OnAfterDeserialize()
	{
		if (string.IsNullOrEmpty(assemblyQualifiedDeclaringType) || string.IsNullOrEmpty(fieldName))
		{
			_runtimeField = null!;
			return;
		}

		var declaringType = Type.GetType(assemblyQualifiedDeclaringType);

		_runtimeField = declaringType?.GetField(
			fieldName,
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
		);
	}

	public void OnBeforeSerialize()
	{
		/* nothing to do */
	}


	public void Set(FieldInfo field)
	{
		_runtimeField                  = field;
		fieldName                      = field.Name;
		assemblyQualifiedDeclaringType = field.DeclaringType?.AssemblyQualifiedName!;
	}
}
