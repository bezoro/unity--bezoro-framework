using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Bezoro.Framework.Unity.Editor.Tools;
using Bezoro.Framework.Unity.Editor.Utilities.EditorPrefs;
using Moq;
using NUnit.Framework;

namespace Bezoro.Framework.Unity.Tests.EditMode
{
	[TestFixture]
	public class AutoUnityTestRunnerWindowUnitTests
	{
		[SetUp]
		public void SetUp()
		{
			_logic                = new( new EditorPrefsSettingsStorage() );
			_mockStorage          = new();
			_originalAutoRunValue = _logic.AutoRun;
		}

		[TearDown]
		public void TearDown()
		{
			_logic.SetAutoRun( _originalAutoRunValue );
		}

		private Mock<ISettingsStorage>          _mockStorage;
		private AutoTestRunnerEditorWindowLogic _logic;
		private bool                            _originalAutoRunValue;

		[Test]
		public void Constructor_NullSettingsStorage_ShouldThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>( () => new AutoTestRunnerEditorWindowLogic( null ) );
		}

		[Test]
		public void GetAutoRun_StorageThrowsException_ShouldHandleGracefully()
		{
			_mockStorage.Setup( m => m.GetBool( "AutoRunTests_Enabled" ) ).Throws<Exception>();

			var logic = new AutoTestRunnerEditorWindowLogic( _mockStorage.Object );

			Assert.DoesNotThrow( () => _ = logic.AutoRun );
			Assert.IsFalse( logic.AutoRun );
		}

		[Test]
		public void InitialState_ShouldReflectStoredValue()
		{
			var mockStorage = new Mock<ISettingsStorage>();
			mockStorage.Setup( m => m.GetBool( "AutoRunTests_Enabled" ) ).Returns( true );

			var logic = new AutoTestRunnerEditorWindowLogic( mockStorage.Object );

			Assert.IsTrue( logic.AutoRun );
		}

		[Test]
		public void GetAutoRun_WhenKeyDoesNotExist_ShouldReturnDefaultValue()
		{
			_mockStorage.Setup( m => m.GetBool( "AutoRunTests_Enabled" ) ).Returns( false );

			var logic = new AutoTestRunnerEditorWindowLogic( _mockStorage.Object );

			Assert.IsFalse( logic.AutoRun );
		}

		[Test]
		public void SetAutoRun_RepeatedCalls_ShouldNotCauseIssues()
		{
			var logic = new AutoTestRunnerEditorWindowLogic( _mockStorage.Object );

			logic.SetAutoRun( true );
			logic.SetAutoRun( true );
			logic.SetAutoRun( true );

			Assert.IsTrue( logic.AutoRun );
			_mockStorage.Verify(
				m => m.SetBool(
					"AutoRunTests_Enabled",
					true ),
				Times.Exactly( 3 ) );
		}

		[Test]
		public void SetAutoRun_ShouldReturnCorrectValue()
		{
			_logic.SetAutoRun( true );
			Assert.IsTrue( _logic.AutoRun );

			_logic.SetAutoRun( false );
			Assert.IsFalse( _logic.AutoRun );
		}

		[Test]
		public void SetAutoRun_Enables_ShouldRunTests()
		{
			_logic.SetAutoRun( true );
			Assert.IsTrue( _logic.ShouldRunTests() );

			_logic.SetAutoRun( false );
			Assert.IsFalse( _logic.ShouldRunTests() );
		}

		[Test]
		public void SetAutoRun_ShouldPersistAcrossInstances()
		{
			_logic.SetAutoRun( true );

			var newLogic = new AutoTestRunnerEditorWindowLogic( new EditorPrefsSettingsStorage() );

			Assert.IsTrue( newLogic.AutoRun );
		}

		[Test]
		public void SetAutoRun_ConcurrentAccess_ShouldRemainThreadSafe()
		{
			var logic = new AutoTestRunnerEditorWindowLogic( _mockStorage.Object );

			Parallel.For(
				0,
				1000,
				_ => { logic.SetAutoRun( true ); } );

			Assert.IsTrue( logic.AutoRun );
		}

		[Test]
		public void SetAutoRun_LargeNumberOfCalls_ShouldNotCausePerformanceIssues()
		{
			var logic = new AutoTestRunnerEditorWindowLogic( _mockStorage.Object );

			var sw = Stopwatch.StartNew();

			for ( var i = 0 ; i < 10000 ; i++ )
			{
				logic.SetAutoRun( i % 2 == 0 );
			}

			sw.Stop();

			Assert.Less(
				sw.ElapsedMilliseconds,
				1000,
				"SetAutoRun took too long for 10000 calls" );
		}
	}
}
