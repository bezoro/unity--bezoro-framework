using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityApplication = UnityEngine.Application;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies Addressables helper behavior that is deterministic in EditMode.
	/// </summary>
	[TestFixture]
	public sealed class AddressableHelperTests
	{
		[Test]
		public void DoesAssetExistInAddressables_InEditMode_ReturnsFalse()
		{
			Assert.IsFalse(UnityApplication.isPlaying);

			Assert.IsFalse(AddressableHelper.DoesAssetExistInAddressables<GameObject>("missing-key"));
		}

		[Test]
		public void DoesAssetExistInAddressables_WhenKeyIsNull_ReturnsFalse()
		{
			Assert.IsFalse(AddressableHelper.DoesAssetExistInAddressables<GameObject>(null));
		}
	}
}
