using System;
using System.Linq;
using System.Reflection;
using Bezoro.Unity.Core.Editor.Utilities;
using NUnit.Framework;
using UnityEditor.Build.Reporting;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class EditorBuildMonitorTests
	{
		[Test]
		public void CompletionWithoutBuildReport_HasUnknownResult()
		{
			var details = CreateReport(null);

			Assert.That(details.result, Is.EqualTo(BuildResult.Unknown));
			Assert.That(details.buildPath, Is.EqualTo("Build/Player"));
		}

		[Test]
		public void CompletionWithCancelledSummaryAndZeroErrors_RemainsCancelled()
		{
			// Unity builds this read-only value natively. Set only the outcome on a boxed
			// summary so no player build is required to exercise the report conversion.
			object summary = default(BuildSummary);
			var result = typeof(BuildSummary).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
				.Single(field => field.FieldType == typeof(BuildResult));
			result.SetValue(summary, BuildResult.Cancelled);

			var details = CreateReport((BuildSummary)summary);

			Assert.That(details.result, Is.EqualTo(BuildResult.Cancelled));
			Assert.That(details.errorCount, Is.Zero);
		}

		private static EditorBuildMonitor.CustomBuildReport CreateReport(BuildSummary? summary)
		{
			var createReport = typeof(EditorBuildMonitor).GetMethod("CreateCompletionReport",
				BindingFlags.Static | BindingFlags.NonPublic);
			return (EditorBuildMonitor.CustomBuildReport)createReport.Invoke(null,
				new object[] { summary, DateTime.UtcNow, "Build/Player" });
		}
	}
}
