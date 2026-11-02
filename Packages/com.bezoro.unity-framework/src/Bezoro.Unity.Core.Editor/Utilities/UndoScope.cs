using System;
using UnityEditor;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Groups editor undo operations and collapses them when the scope is disposed.
	/// </summary>
	public class UndoScope : IDisposable
	{
		public UndoScope(string scopeName)
		{
			Undo.IncrementCurrentGroup();
			Undo.SetCurrentGroupName(
				scopeName
			);
		}

		public void Dispose() =>
			Undo.CollapseUndoOperations(
				Undo.GetCurrentGroup()
			);
	}
}
