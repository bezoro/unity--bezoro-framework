using UnityEngine;
using Bezoro.Framework.Core;

namespace Bezoro.Framework.StateMachines
{
	[DisallowMultipleComponent]
	public sealed class TickableStateMachineOwner : Tickable
	{
		public BaseTickableStateMachine StateMachine => stateMachine;

		[SerializeField]
		private BaseTickableStateMachine stateMachine;

		private void Awake()
		{
			var type = stateMachine.GetType();
			var newStateMachineInstance = ScriptableObject.CreateInstance(type);
			stateMachine = newStateMachineInstance as BaseTickableStateMachine;
			stateMachine.Init(this);
		}

		private void Start() =>
			stateMachine.Start();

		protected override void OnEnable()
		{
			base.OnEnable();
			stateMachine.Enable();
		}

		public override void EarlyTick() =>
			stateMachine.EarlyTick();

		public override void Tick() { stateMachine.Tick(); }

		public override void FixedTick() =>
			stateMachine.FixedTick();

		public override void LateTick() =>
			stateMachine.LateTick();

		protected override void OnDisable()
		{
			base.OnDisable();
			stateMachine.Disable();
		}

		private void OnDestroy() =>
			stateMachine.Destroy();

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
	}
}