using UnityEditor;
using UnityEngine;
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

		private void OnEnable()
		{
			baseKCCStateMachine = target as BaseKCCStateMachine;
			Assert.IsNotNull(baseKCCStateMachine);

			TryGetDependencies();
			TrySetDependencies();

			CreateComponentsThatWereNotFound();

			void TryGetDependencies()
			{
				_motor = baseKCCStateMachine.gameObject.GetComponent<KinematicCharacterMotor>();

				_capsuleCollider = baseKCCStateMachine.gameObject.GetComponent<CapsuleCollider>();

				if (baseKCCStateMachine is IHaveBlackBoard)
					_blackBoard = baseKCCStateMachine.gameObject.GetComponent<BlackBoard>();
			}

			void TrySetDependencies()
			{
				if (_motor != null)
					baseKCCStateMachine.SetMotor(_motor);

				if (_blackBoard != null)
					if (baseKCCStateMachine is IHaveBlackBoard haveBlackBoard)
						haveBlackBoard.SetBlackBoard(_blackBoard);
			}

			void CreateComponentsThatWereNotFound()
			{
				if (_motor == null)
				{
					_motor = baseKCCStateMachine.gameObject.AddComponent<KinematicCharacterMotor>();
					baseKCCStateMachine.SetMotor(_motor);
				}

				if (_blackBoard == null
				    &&
				    baseKCCStateMachine is IHaveBlackBoard haveBlackBoard)
				{
					var blackBoardProperty =
						baseKCCStateMachine.GetType().GetProperty("BlackBoard");

					Assert.IsNotNull(blackBoardProperty);
					var blackBoardType = blackBoardProperty.PropertyType;
					_blackBoard = baseKCCStateMachine.gameObject.AddComponent
									  (blackBoardType) as BlackBoard;

					haveBlackBoard.SetBlackBoard(_blackBoard);
				}
			}
		}

		private void OnDisable()
		{
			if (target.IsNull())
			{
				DestroyImmediate(_motor);
				DestroyImmediate(_capsuleCollider);
				DestroyImmediate(_blackBoard);
			}
		}
	}
}