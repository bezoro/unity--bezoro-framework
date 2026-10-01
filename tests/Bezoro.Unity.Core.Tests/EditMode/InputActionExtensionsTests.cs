using Bezoro.Unity.Core.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class InputActionExtensionsTests
	{
		[Test]
		public void Disable_WhenArgumentIsNull_ThrowsSpecificInputException()
		{
			Assert.Throws<InputActionExceptions.NullInputActionAssetException>(() => InputActionExtensions.Disable(
																				   (InputActionAsset)null
																			   )
			);

			Assert.Throws<InputActionExceptions.NullInputActionAssetException.NullInputActionException>(() =>
						InputActionExtensions.Disable((InputAction)null)
			);

			Assert.Throws<InputActionExceptions.NullInputActionAssetException.NullInputActionMapException>(() =>
						InputActionExtensions.Disable((InputActionMap)null)
			);
		}

		[Test]
		public void Enable_WhenArgumentIsNull_ThrowsSpecificInputException()
		{
			Assert.Throws<InputActionExceptions.NullInputActionAssetException>(() => InputActionExtensions.Enable(
																				   (InputActionAsset)null
																			   )
			);

			Assert.Throws<InputActionExceptions.NullInputActionAssetException.NullInputActionException>(() =>
						InputActionExtensions.Enable((InputAction)null)
			);

			Assert.Throws<InputActionExceptions.NullInputActionAssetException.NullInputActionMapException>(() =>
						InputActionExtensions.Enable((InputActionMap)null)
			);
		}

		[Test]
		public void EnableDisable_WhenActionAssetIsProvided_UpdatesAssetState()
		{
			var actionAsset = ScriptableObject.CreateInstance<InputActionAsset>();

			try
			{
				actionAsset.AddActionMap("Gameplay")
						   .AddAction("Jump");

				InputActionExtensions.Enable(actionAsset);

				Assert.IsTrue(actionAsset.enabled);

				InputActionExtensions.Disable(actionAsset);

				Assert.IsFalse(actionAsset.enabled);
			}
			finally
			{
				Object.DestroyImmediate(actionAsset);
			}
		}

		[Test]
		public void EnableDisable_WhenActionIsProvided_UpdatesActionState()
		{
			using var action = new InputAction("Jump");

			InputActionExtensions.Enable(action);

			Assert.IsTrue(action.enabled);

			InputActionExtensions.Disable(action);

			Assert.IsFalse(action.enabled);
		}

		[Test]
		public void EnableDisable_WhenActionMapIsProvided_UpdatesMapState()
		{
			using var actionMap = new InputActionMap("Gameplay");
			actionMap.AddAction("Jump");

			InputActionExtensions.Enable(actionMap);

			Assert.IsTrue(actionMap.enabled);

			InputActionExtensions.Disable(actionMap);

			Assert.IsFalse(actionMap.enabled);
		}
	}
}
