using System;
using System.Diagnostics;

namespace Bezoro.Core
{
	public sealed class CustomFixedTick
	{
		public CustomFixedTick(float customFixedDeltaTime, Action customFixedTickCallback)
		{
			CustomFixedDeltaTime = customFixedDeltaTime;
			CustomFixedTickCallback = customFixedTickCallback;
		}

		public float CustomFixedDeltaTime { get; set; }

		public float MaxAllowedTimestep { get; set; } = 0.3f;

		public double ReferenceTime { get; private set; }

		public double FixedTime { get; private set; }

		public bool Tick(float deltaTime)
		{
			_stopWatch.Reset();
			_stopWatch.Start();

			ReferenceTime += deltaTime;
			while (FixedTime < ReferenceTime)
			{
				FixedTime += CustomFixedDeltaTime;
				CustomFixedTickCallback?.Invoke();

				if (_stopWatch.ElapsedMilliseconds / 1000.0f > MaxAllowedTimestep)
					return false;
			}

			return true;
		}

		private event Action CustomFixedTickCallback;

		private readonly Stopwatch _stopWatch = new();
	}
}