using System;
using UnityEngine.InputSystem;

namespace Bezoro.Unity.Core.Input;

/// <summary>
///     Defines input action exceptions.
/// </summary>
public static class InputActionExceptions
{
	/// <summary>
	///     Defines the Null Input Action Asset Exception.
	/// </summary>
	public class NullInputActionAssetException : Exception
	{
		public NullInputActionAssetException() : base("InputActionAsset is null.") { }

		public static void ThrowIfNull(InputActionAsset actionAsset)
		{
			if (actionAsset == null) throw new NullInputActionAssetException();
		}

		/// <summary>
		///     Defines the Null Input Action Exception.
		/// </summary>
		public class NullInputActionException : Exception
		{
			public NullInputActionException() : base("InputAction is null.") { }

			public static void ThrowIfNull(InputAction action)
			{
				if (action == null) throw new NullInputActionException();
			}
		}

		/// <summary>
		///     Defines the Null Input Action Map Exception.
		/// </summary>
		public class NullInputActionMapException : Exception
		{
			public NullInputActionMapException() : base("InputActionMap is null.") { }

			public static void ThrowIfNull(InputActionMap actionMap)
			{
				if (actionMap == null) throw new NullInputActionMapException();
			}
		}
	}
}
