using KinematicCharacterController;
using UnityEditor;
using UnityEngine;

namespace Bezoro.StateMachines
{
	[CustomEditor(typeof(BaseKCCStateMachine), true)]
	public class BaseKCCStateMachineEditor : Editor
	{
		private BlackBoard _blackBoard;
		private KinematicCharacterMotor _motor;
		private CapsuleCollider _capsuleCollider;

		private void OnEnable()
		{
			var kccStateMachine = target as BaseKCCStateMachine;

			EditorApplication.delayCall += SetReferences;

			void SetReferences()
			{
				_motor = kccStateMachine.Motor;
				_blackBoard = kccStateMachine.TryGetBlackBoard<BlackBoard>();
				_capsuleCollider = kccStateMachine.GetComponent<CapsuleCollider>();
			}
		}

		private void OnDisable()
		{
			if (target == null)
			{
				DestroyImmediate(_motor);
				DestroyImmediate(_blackBoard);
				DestroyImmediate(_capsuleCollider);
			}
		}
	}
}