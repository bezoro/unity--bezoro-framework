using System;

namespace Bezoro.Unity.Core.Tickable
{
	/// <summary>
	///     Stores Tickable registration data.
	/// </summary>
	public static class TickableRegistration
	{
		public static bool Register(ITickable tickable)
		{
			if (tickable == null)
				throw new ArgumentNullException(nameof(tickable));

			if (!TickableManager.TryGetInstance(out var manager))
				return false;

			manager.AddUnique(tickable);
			return true;
		}

		public static void Unregister(ITickable tickable)
		{
			if (tickable == null)
				throw new ArgumentNullException(nameof(tickable));

			if (TickableManager.TryGetInstance(out var manager))
				manager.Remove(tickable);
		}
	}
}
