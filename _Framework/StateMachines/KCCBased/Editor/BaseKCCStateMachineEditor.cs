using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	[CustomEditor(typeof(BaseKCCStateMachine), true)]
	public class BaseKCCStateMachineEditor : Editor
	{
		private BaseKCCStateMachine KCCStateMachine;

		private KinematicCharacterMotor _motor;
		private CapsuleCollider _capsuleCollider;
		private BlackBoard _blackBoard;

		private void OnEnable()
		{
			KCCStateMachine = target as BaseKCCStateMachine;
			Assert.IsNotNull(KCCStateMachine);

			TryGetComponents();

			CreateComponentsThatWereNotFound();

			void TryGetComponents()
			{
				_motor = KCCStateMachine.gameObject.GetComponent<KinematicCharacterMotor>();
				if (_motor != null)
					KCCStateMachine.SetMotor(_motor);

				_capsuleCollider = KCCStateMachine.gameObject.GetComponent<CapsuleCollider>();

				if (KCCStateMachine is IHaveBlackBoard haveBlackBoard)
				{
					_blackBoard = KCCStateMachine.gameObject.GetComponent<BlackBoard>();
					if (_blackBoard != null)
						haveBlackBoard.SetBlackBoard(_blackBoard);
				}
			}

			void CreateComponentsThatWereNotFound()
			{
				if (_motor == null)
				{
					var motor = KCCStateMachine.gameObject.AddComponent<KinematicCharacterMotor>();
					KCCStateMachine.SetMotor(motor);
				}

				if (KCCStateMachine is IHaveBlackBoard haveBlackBoard
				    &&
				    _blackBoard == null)
				{
					var blackBoardProperty = KCCStateMachine.GetType().GetProperty("BlackBoard");
					Assert.IsNotNull(blackBoardProperty);
					var blackBoardType = blackBoardProperty.PropertyType;
					var blackBoard = KCCStateMachine.gameObject.AddComponent
										 (blackBoardType) as BlackBoard;

					haveBlackBoard.SetBlackBoard(blackBoard);
				}
			}
		}

		private void OnDisable()
		{
			if (target == null)
			{
				DestroyImmediate(_motor);
				DestroyImmediate(_capsuleCollider);
				DestroyImmediate(_blackBoard);
			}
		}
	}
}