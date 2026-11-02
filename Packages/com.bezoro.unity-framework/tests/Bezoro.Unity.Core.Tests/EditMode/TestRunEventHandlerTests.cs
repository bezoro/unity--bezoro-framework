using Bezoro.Unity.Core.Editor.Utilities;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class TestRunEventHandlerTests
	{
		[Test]
		public void TestRunEventHandler_WhenUnityTestRunnerStarts_ReportsTestRunInProgress()
		{
			Assert.That(
				TestRunEventHandler.IsTestRunInProgress,
				Is.True
			);
		}
	}
}
