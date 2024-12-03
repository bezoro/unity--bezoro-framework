using System;

namespace Bezoro.Framework.Unity.Utilities
{
	public class IncorrectConditionException : Exception
	{
		public IncorrectConditionException() : base( "Condition must be true, but it was false" )
		{
		}

		public IncorrectConditionException( string message ) : base( message )
		{
		}

		public IncorrectConditionException( string message, Exception innerException )
			: base(
				message,
				innerException )
		{
		}
	}
}
