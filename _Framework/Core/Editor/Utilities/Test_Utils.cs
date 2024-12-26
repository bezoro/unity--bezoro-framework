using Moq;
using NUnit.Framework;

namespace Bezoro.Framework.Core.Tests.Edit_Mode.Eitor.Utilities
{
	/// <summary>
	///     Provides utilities for testing purposes, including methods to create test cases
	///     and mock objects for dependencies commonly utilized in unit tests.
	/// </summary>
	public class Test_Utils
	{
		public Mock<I_Logger> Logger_Mock => new();

		public static TestCaseData Create_Test_Case<T>(string test_name, T data)
		{
			return new( data )
			{
				TestName = test_name
			};
		}
	}
}
