using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Bezoro.Framework.Unity.Jobs
{
	public class RaycastCommandRaycaster : IDisposable
	{
		private NativeArray<RaycastCommand> _commands;
		private NativeArray<RaycastHit>     _results;

		public RaycastCommandRaycaster( int bufferSize )
		{
			_commands = new(
				bufferSize,
				Allocator.Persistent );

			_results = new(
				bufferSize,
				Allocator.Persistent );
		}

		public void Dispose()
		{
			_commands.Dispose();
			_results.Dispose();
		}

		public void AddRaycastCommand( Ray ray, float distance, int index )
		{
			_commands[ index ] =
				new(
					ray.origin,
					ray.direction,
					QueryParameters.Default );
		}

		public JobHandle ScheduleRaycastCommands()
		{
			return RaycastCommand.ScheduleBatch(
				_commands,
				_results,
				1 );
		}
	}
}
