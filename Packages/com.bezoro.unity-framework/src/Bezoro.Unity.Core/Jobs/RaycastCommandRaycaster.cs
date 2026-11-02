using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Bezoro.Unity.Core.Jobs;

/// <summary>
///     Schedules batched physics raycasts through RaycastCommand jobs.
/// </summary>
public class RaycastCommandRaycaster : IDisposable
{
	private bool                        _disposed;
	private NativeArray<RaycastCommand> _commands;
	private NativeArray<RaycastHit>     _results;

	public RaycastCommandRaycaster(int bufferSize)
	{
		if (bufferSize <= 0)
			throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be greater than zero.");

		_commands = new(bufferSize, Allocator.Persistent);
		_results  = new(bufferSize, Allocator.Persistent);
	}

	public JobHandle ScheduleRaycastCommands()
	{
		ThrowIfDisposed();

		return RaycastCommand.ScheduleBatch(_commands, _results, 1);
	}

	public void AddRaycastCommand(Ray ray, float distance, int index)
	{
		ThrowIfDisposed();

		if (index < 0 || index >= _commands.Length)
			throw new ArgumentOutOfRangeException(nameof(index), "Index must be inside the command buffer.");

		if (distance <= 0f || float.IsNaN(distance))
			throw new ArgumentOutOfRangeException(nameof(distance), "Distance must be a positive number.");

		_commands[index] = new(ray.origin, ray.direction, QueryParameters.Default, distance);
	}

	public void Dispose()
	{
		if (_disposed) return;

		_disposed = true;
		if (_commands.IsCreated) _commands.Dispose();
		if (_results.IsCreated) _results.Dispose();
	}

	private void ThrowIfDisposed()
	{
		if (_disposed) throw new ObjectDisposedException(nameof(RaycastCommandRaycaster));
	}
}
