using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Bezoro.Framework.Unity.Jobs
{
	public class RaycastCommandRaycaster : IDisposable
	{
		public RaycastCommandRaycaster(int bufferSize)
		{
			_commands = new NativeArray<RaycastCommand>(bufferSize, Allocator.Persistent);
			_results = new NativeArray<RaycastHit>(bufferSize, Allocator.Persistent);
		}

		public void AddRaycastCommand(Ray ray, float distance, int index)
		{
			_commands[index] =
				new RaycastCommand(ray.origin, ray.direction, QueryParameters.Default);
		}

		public JobHandle ScheduleRaycastCommands()
		{
			return RaycastCommand.ScheduleBatch(_commands, _results, 1);
		}

		public void Dispose()
		{
			_commands.Dispose();
			_results.Dispose();
		}

		private NativeArray<RaycastCommand> _commands;
		private NativeArray<RaycastHit> _results;
	}
}
