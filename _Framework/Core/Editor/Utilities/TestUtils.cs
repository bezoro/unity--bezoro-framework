using Moq;
using NUnit.Framework;

namespace Bezoro.Framework.Core.Editor.Editor.Utilities
{
	/// <summary>
	///     Provides utilities for testing purposes, including methods to create test cases
	///     and mock objects for dependencies commonly utilized in unit tests.
	/// </summary>
	public class TestUtils
	{
		public Mock<ILogger> LoggerMock => new();

		public static TestCaseData CreateTestCase<T>(string test_name, T data)
		{
			return new( data )
			{
				TestName = test_name
			};
		}
	}
}
