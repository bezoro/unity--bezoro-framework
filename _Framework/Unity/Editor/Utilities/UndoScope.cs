using System;
using UnityEditor;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	public class UndoScope : IDisposable
	{
		public UndoScope( string scopeName )
		{
			Undo.IncrementCurrentGroup();
			Undo.SetCurrentGroupName( scopeName );
		}

		public void Dispose()
		{
			Undo.CollapseUndoOperations( Undo.GetCurrentGroup() );
		}
	}
}
