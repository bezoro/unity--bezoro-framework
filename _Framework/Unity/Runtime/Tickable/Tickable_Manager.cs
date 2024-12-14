using Bezoro.Framework.Core;

namespace Bezoro.Framework.Unity
{
	public sealed class Tickable_Manager : Singleton_MonoBehaviour<Tickable_Manager>
	{
		private Tickable[] _early_tickables = new Tickable[128];
		private int        _early_tickables_count;

		private Tickable[] _fixed_tickables = new Tickable[128];
		private int        _fixed_tickables_count;

		private Tickable[] _late_tickables = new Tickable[128];
		private int        _late_tickables_count;

		private Tickable[] _tickables = new Tickable[128];
		private int        _tickables_count;

		public void AddUniqueEarlyTickable(Tickable tickable)
		{
			Array_Helpers.Add_Unique(ref _early_tickables, tickable);
		}

		public void AddUniqueTickable(Tickable tickable)
		{
			Array_Helpers.Add_Unique(ref _tickables, tickable);
		}

		public void AddUniqueFixedTickable(Tickable tickable)
		{
			Array_Helpers.Add_Unique(ref _fixed_tickables, tickable);
		}

		public void AddUniqueLateTickable(Tickable tickable)
		{
			Array_Helpers.Add_Unique(ref _late_tickables, tickable);
		}

		public void RemoveEarlyTickable(Tickable tickable)
		{
			Array_Helpers.Remove_Element(ref _early_tickables, tickable);
		}

		public void RemoveTickable(Tickable tickable)
		{
			Array_Helpers.Remove_Element(ref _tickables, tickable);
		}

		public void RemoveFixedTickable(Tickable tickable)
		{
			Array_Helpers.Remove_Element(ref _fixed_tickables, tickable);
		}

		public void RemoveLateTickable(Tickable tickable)
		{
			Array_Helpers.Remove_Element(ref _late_tickables, tickable);
		}

		private void Update()
		{
			for (var i = 0 ; i < _early_tickables_count ; i++)
				_early_tickables[i].Early_Tick();

			for (var i = 0 ; i < _tickables_count ; i++)
				_tickables[i].Tick();
		}

		private void FixedUpdate()
		{
			for (var i = 0 ; i < _fixed_tickables_count ; i++)
				_fixed_tickables[i].Fixed_Tick();
		}

		private void LateUpdate()
		{
			for (var i = 0 ; i < _late_tickables_count ; i++)
				_late_tickables[i].Late_Tick();
		}
	}
}
