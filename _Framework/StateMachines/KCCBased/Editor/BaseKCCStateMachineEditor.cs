using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Bezoro.Core.Extensions;
using UnityEngine.Assertions;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	[CustomEditor(typeof(BaseKCCStateMachine), true)]
	public class BaseKCCStateMachineEditor : Editor
	{
		private BaseKCCStateMachine baseKCCStateMachine;

		private KinematicCharacterMotor _motor;
		private CapsuleCollider _capsuleCollider;
		private BlackBoard _blackBoard;
		private PropertyInfo _blackBoardProperty;
		private Type _blackBoardType;
		private MethodInfo _setBlackBoardMethod;

		private void OnEnable()
		{
			baseKCCStateMachine = target as BaseKCCStateMachine;
			Assert.IsNotNull(baseKCCStateMachine);

			ResolveHasBlackBoard();
			TryGetDependencies();
			TrySetDependencies();
			CreateComponentsThatWereNotFound();

			void ResolveHasBlackBoard()
			{
				_blackBoardProperty = baseKCCStateMachine.GetType().GetProperty("BlackBoard");
				_blackBoardType = _blackBoardProperty?.PropertyType;
				_setBlackBoardMethod = baseKCCStateMachine.GetType().GetMethod("SetBlackBoard");
			}

			void TryGetDependencies()
			{
				_motor = baseKCCStateMachine.gameObject.GetComponent<KinematicCharacterMotor>();

				_capsuleCollider = baseKCCStateMachine.gameObject.GetComponent<CapsuleCollider>();

				if (_blackBoardProperty is not null)
					_blackBoard = baseKCCStateMachine.gameObject.GetComponent<BlackBoard>();
			}

			void TrySetDependencies()
			{
				if (_motor is not null)
					baseKCCStateMachine.SetMotor(_motor);

				if (_blackBoard is not null)
				{
					object[] parameters = { _blackBoard };

					_setBlackBoardMethod?.Invoke(baseKCCStateMachine, parameters);
				}
			}

			void CreateComponentsThatWereNotFound()
			{
				if (_blackBoard is null && _blackBoardProperty is not null)
				{
					_blackBoard =
						baseKCCStateMachine.gameObject.AddComponent(_blackBoardType) as BlackBoard;

					object[] parameters = { _blackBoard };

					_setBlackBoardMethod?.Invoke(baseKCCStateMachine, parameters);
				}

				if (_motor is null)
				{
					_motor = baseKCCStateMachine.gameObject.AddComponent<KinematicCharacterMotor>();
					baseKCCStateMachine.SetMotor(_motor);
				}
			}
		}

		private void OnDisable()
		{
			if (target.EqualsNull())
			{
				DestroyImmediate(_motor);
				DestroyImmediate(_capsuleCollider);
				DestroyImmediate(_blackBoard);
			}
		}
	}
}