using Bezoro.Framework.Unity.StateMachines.Utilities;
using UnityEngine;

namespace Bezoro.Framework.Unity.StateMachines.TickableBased
{
	[DisallowMultipleComponent]
	public sealed class TickableStateMachineOwner : Tickable
	{
		[SerializeField] private BaseTickableStateMachine stateMachine;
		public                   BaseTickableStateMachine StateMachine => stateMachine;

		public override void Early_Tick()
		{
			stateMachine.EarlyTick();
		}

		public override void Tick()
		{
			stateMachine.Tick();
		}

		public override void Fixed_Tick()
		{
			stateMachine.FixedTick();
		}

		public override void Late_Tick()
		{
			stateMachine.LateTick();
		}

		public bool TryFindStateInCurrentHierarchy<T>(out T outState)
		where T : class, IState
		{
			var boolean = StateMachine.TryFindStateInCurrentHierarchy(out T state);
			outState = state;

			return boolean;
		}

		public bool TryFindStateInLastHierarchy<T>(out T outState)
		where T : class, IState
		{
			var boolean = StateMachine.TryFindStateInLastHierarchy(out T state);
			outState = state;

			return boolean;
		}

		public void SetStateMachine(BaseTickableStateMachine stateMachine)
		{
			this.stateMachine.Exit();
			this.stateMachine = stateMachine;
			this.stateMachine.Init(this);
			this.stateMachine.Start();
		}

		private void Awake()
		{
			var type                    = stateMachine.GetType();
			var newStateMachineInstance = ScriptableObject.CreateInstance(type);
			stateMachine = newStateMachineInstance as BaseTickableStateMachine;
			stateMachine.Init(this);
		}

		private void Start()
		{
			stateMachine.Start();
		}

		private void OnDestroy()
		{
			stateMachine.Destroy();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			stateMachine.Enable();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			stateMachine.Disable();
		}
	}
}
