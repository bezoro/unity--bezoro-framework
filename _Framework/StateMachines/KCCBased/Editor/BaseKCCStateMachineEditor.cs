using UnityEditor;
using UnityEngine;
using Bezoro.Core.Extensions;
using KinematicCharacterController;
using Sirenix.OdinInspector.Editor;

namespace Bezoro.StateMachines.Editor
{
	[CustomEditor(typeof(BaseKCCStateMachine), true)]
	public class BaseKCCStateMachineEditor : OdinEditor
	{
		private BaseKCCStateMachine baseKCCStateMachine;

		private KinematicCharacterMotor _motor;
		private CapsuleCollider _capsuleCollider;

		protected override void OnEnable()
		{
			base.OnEnable();

			baseKCCStateMachine = target as BaseKCCStateMachine;

			TryGetDependencies();
			TrySetDependencies();
			CreateComponentsThatWereNotFound();

			void TryGetDependencies()
			{
				_motor = baseKCCStateMachine.gameObject.GetComponent<KinematicCharacterMotor>();

				_capsuleCollider = baseKCCStateMachine.gameObject.GetComponent<CapsuleCollider>();
			}

			void TrySetDependencies()
			{
				if (_motor is not null)
					baseKCCStateMachine.SetMotor(_motor);
			}

			void CreateComponentsThatWereNotFound()
			{
				if (_motor is null)
				{
					_motor = baseKCCStateMachine.gameObject.AddComponent<KinematicCharacterMotor>();
					baseKCCStateMachine.SetMotor(_motor);
				}
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			if (target == null)
			{
				DestroyImmediate(_motor);
				DestroyImmediate(_capsuleCollider);
			}
		}
	}
}