using Bezoro.Unity.Core.MonoBehaviours;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class UnloadSceneOnLoadingCompleteModelTests
	{
		[TestCase(true,  false, true)]
		[TestCase(true,  true,  false)]
		[TestCase(false, false, false)]
		[TestCase(false, true,  false)]
		public void ShouldStartUnload_RequiresActiveCancellationSource(
			bool hasCancellationSource,
			bool isCancellationRequested,
			bool expected)
		{
			var model = new UnloadSceneOnLoadingCompleteModel();

			Assert.AreEqual(expected, model.ShouldStartUnload(hasCancellationSource, isCancellationRequested));
		}
	}
}
