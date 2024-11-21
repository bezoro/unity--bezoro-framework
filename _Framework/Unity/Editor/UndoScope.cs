using System;
using UnityEditor;

namespace Bezoro.Framework.Unity.Editor
{
	public sealed class UndoScope :
		IDisposable
	{
		private readonly int _undoGroup;

		public UndoScope( string name = "Unnamed Undo Group" )
		{
			Undo.IncrementCurrentGroup();
			_undoGroup = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName( name );
		}

		public void Dispose()
		{
			Undo.CollapseUndoOperations( _undoGroup );
		}
	}
}
