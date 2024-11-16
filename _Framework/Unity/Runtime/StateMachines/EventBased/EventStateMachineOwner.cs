using Bezoro.Framework.Unity.States;
using Bezoro.Framework.Unity.Utilities;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	[DisallowMultipleComponent]
	public sealed class EventStateMachineOwner : MonoBehaviour
	{
		public BaseEventStateMachine StateMachine => stateMachine;

		[SerializeField]
		private BaseEventStateMachine stateMachine;

		private void Awake()
		{
			var type = stateMachine.GetType();
			var newStateMachineInstance = ScriptableObject.CreateInstance( type );
			stateMachine = newStateMachineInstance as BaseEventStateMachine;
			stateMachine.Init( this );
		}

		private void Start()
		{
			stateMachine.Start();
		}

		private void OnEnable()
		{
			stateMachine.Enable();
		}

		private void OnDisable()
		{
			stateMachine.Disable();
		}

		private void OnDestroy()
		{
			stateMachine.Destroy();
		}

		public bool TryFindStateInCurrentHierarchy<T>( out T outState )
		where T : class, IState
		{
			var boolean = StateMachine.TryFindStateInCurrentHierarchy( out T state );
			outState = state;

			return boolean;
		}

		public bool TryFindStateInLastHierarchy<T>( out T outState )
		where T : class, IState
		{
			var boolean = StateMachine.TryFindStateInLastHierarchy( out T state );
			outState = state;

			return boolean;
		}

		public void SetStateMachine( BaseEventStateMachine stateMachine )
		{
			this.stateMachine.Exit();
			this.stateMachine = stateMachine;
			this.stateMachine.Init( this );
			this.stateMachine.Start();
		}
	}
}
