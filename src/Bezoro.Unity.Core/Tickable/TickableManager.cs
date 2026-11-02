using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Bezoro.Core.Extensions;
using Bezoro.Core.Helpers;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.MonoBehaviours;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Tickable
{
	[DefaultExecutionOrder(ExecutionOrder.Manager)]
	[RequireComponent(typeof(SingletonScope), typeof(GlobalReference))]
	/// <summary>
	/// Dispatches registered tick callbacks in Update, FixedUpdate, LateUpdate, and pre/post phases.
	/// </summary>
	public sealed class TickableManager : MonoBehaviour
	{
		[SerializeField]             private bool                enableLogs;
		[SerializeField]             private bool                logTickEvents;
		private IEarlyTickable?[]   _earlyTickables = new IEarlyTickable[128];
		private IRegularTickable?[] _tickables      = new IRegularTickable[128];
		private ILateTickable?[]    _lateTickables  = new ILateTickable[128];
		private IFixedTickable?[]   _fixedTickables = new IFixedTickable[128];

		private IPostEarlyTickable?[]   _postEarlyTickables = new IPostEarlyTickable[128];
		private IPostRegularTickable?[] _postTickables = new IPostRegularTickable[128];
		private IPostLateTickable?[]    _postLateTickables = new IPostLateTickable[128];
		private IPostFixedTickable?[]   _postFixedTickables = new IPostFixedTickable[128];

		private IPreEarlyTickable?[]   _preEarlyTickables = new IPreEarlyTickable[128];
		private IPreRegularTickable?[] _preTickables      = new IPreRegularTickable[128];
		private IPreLateTickable?[]    _preLateTickables  = new IPreLateTickable[128];
		private IPreFixedTickable?[]   _preFixedTickables = new IPreFixedTickable[128];

		public static bool IsAvailable => TryGetInstance(out _);

		public static TickableManager Instance => GlobalReferenceAccess<TickableManager>.Instance;

		public bool EnableLogs
		{
			get => enableLogs;
			set => enableLogs = value;
		}

		public bool LogTickEvents
		{
			get => logTickEvents;
			set => logTickEvents = value;
		}

		public static bool TryGetInstance([NotNullWhen(true)] out TickableManager? instance) =>
			GlobalReferenceAccess<TickableManager>.TryGetInstance(out instance);

		#region Unity Lifecycle

		private void Awake()
		{
			if (!GlobalReferenceAccess<TickableManager>.IsCurrent(this))
				return;

			LogTickable("Awake.");
		}

		private void Update()
		{
			LogTickEvent("Update.");
			ProcessEarlyTickablesGroup();
			ProcessRegularTickablesGroup();
		}

		private void FixedUpdate()
		{
			LogTickEvent("FixedUpdate.");
			ProcessFixedTickablesGroup();
		}

		private void LateUpdate()
		{
			LogTickEvent("LateUpdate.");
			ProcessLateTickablesGroup();
		}

		#endregion

		public void AddUnique(ITickable tickable)
		{
			tickable.ThrowIfNull();

			if (tickable is IPreEarlyTickable preEarlyTickable)
				ArrayHelper.AddUnique(
					ref _preEarlyTickables,
					preEarlyTickable
				);

			if (tickable is IEarlyTickable earlyTickable)
				ArrayHelper.AddUnique(
					ref _earlyTickables,
					earlyTickable
				);

			if (tickable is IPostEarlyTickable postEarlyTickable)
				ArrayHelper.AddUnique(
					ref _postEarlyTickables,
					postEarlyTickable
				);

			if (tickable is IPreFixedTickable preFixedTickable)
				ArrayHelper.AddUnique(
					ref _preFixedTickables,
					preFixedTickable
				);

			if (tickable is IFixedTickable fixedTickable)
				ArrayHelper.AddUnique(
					ref _fixedTickables,
					fixedTickable
				);

			if (tickable is IPostFixedTickable postFixedTickable)
				ArrayHelper.AddUnique(
					ref _postFixedTickables,
					postFixedTickable
				);

			if (tickable is IPreLateTickable preLateTickable)
				ArrayHelper.AddUnique(
					ref _preLateTickables,
					preLateTickable
				);

			if (tickable is ILateTickable lateTickable)
				ArrayHelper.AddUnique(
					ref _lateTickables,
					lateTickable
				);

			if (tickable is IPostLateTickable postLateTickable)
				ArrayHelper.AddUnique(
					ref _postLateTickables,
					postLateTickable
				);

			if (tickable is IPreRegularTickable preRegularTickable)
				ArrayHelper.AddUnique(
					ref _preTickables,
					preRegularTickable
				);

			if (tickable is IRegularTickable regularTickable)
				ArrayHelper.AddUnique(
					ref _tickables,
					regularTickable
				);

			if (tickable is IPostRegularTickable postRegularTickable)
				ArrayHelper.AddUnique(
					ref _postTickables,
					postRegularTickable
				);

			LogTickable(
				$"Registered {tickable.GetType().Name} as {DescribeTickableTypes(tickable)}. {DescribeCounts()}"
			);
		}

		public void Remove(ITickable tickable)
		{
			tickable.ThrowIfNull();

			if (tickable is IPreEarlyTickable preEarlyTickable)
				ArrayHelper.RemoveElement(
					ref _preEarlyTickables,
					preEarlyTickable
				);

			if (tickable is IEarlyTickable earlyTickable)
				ArrayHelper.RemoveElement(
					ref _earlyTickables,
					earlyTickable
				);

			if (tickable is IPostEarlyTickable postEarlyTickable)
				ArrayHelper.RemoveElement(
					ref _postEarlyTickables,
					postEarlyTickable
				);

			if (tickable is IPreFixedTickable preFixedTickable)
				ArrayHelper.RemoveElement(
					ref _preFixedTickables,
					preFixedTickable
				);

			if (tickable is IFixedTickable fixedTickable)
				ArrayHelper.RemoveElement(
					ref _fixedTickables,
					fixedTickable
				);

			if (tickable is IPostFixedTickable postFixedTickable)
				ArrayHelper.RemoveElement(
					ref _postFixedTickables,
					postFixedTickable
				);

			if (tickable is IPreLateTickable preLateTickable)
				ArrayHelper.RemoveElement(
					ref _preLateTickables,
					preLateTickable
				);

			if (tickable is ILateTickable lateTickable)
				ArrayHelper.RemoveElement(
					ref _lateTickables,
					lateTickable
				);

			if (tickable is IPostLateTickable postLateTickable)
				ArrayHelper.RemoveElement(
					ref _postLateTickables,
					postLateTickable
				);

			if (tickable is IPreRegularTickable preRegularTickable)
				ArrayHelper.RemoveElement(
					ref _preTickables,
					preRegularTickable
				);

			if (tickable is IRegularTickable regularTickable)
				ArrayHelper.RemoveElement(
					ref _tickables,
					regularTickable
				);

			if (tickable is IPostRegularTickable postRegularTickable)
				ArrayHelper.RemoveElement(
					ref _postTickables,
					postRegularTickable
				);

			LogTickable($"Removed {tickable.GetType().Name} as {DescribeTickableTypes(tickable)}. {DescribeCounts()}");
		}

		private static int CountActive<T>(T?[] items)
			where T : class
		{
			var count = 0;
			for (var i = 0; i < items.Length; i++)
			{
				if (!items[i].IsNull())
					count++;
			}

			return count;
		}

		private static string DescribeTickableTypes(ITickable tickable)
		{
			var types = new List<string>(12);

			if (tickable is IPreEarlyTickable) types.Add(nameof(IPreEarlyTickable));
			if (tickable is IEarlyTickable) types.Add(nameof(IEarlyTickable));
			if (tickable is IPostEarlyTickable) types.Add(nameof(IPostEarlyTickable));
			if (tickable is IPreRegularTickable) types.Add(nameof(IPreRegularTickable));
			if (tickable is IRegularTickable) types.Add(nameof(IRegularTickable));
			if (tickable is IPostRegularTickable) types.Add(nameof(IPostRegularTickable));
			if (tickable is IPreLateTickable) types.Add(nameof(IPreLateTickable));
			if (tickable is ILateTickable) types.Add(nameof(ILateTickable));
			if (tickable is IPostLateTickable) types.Add(nameof(IPostLateTickable));
			if (tickable is IPreFixedTickable) types.Add(nameof(IPreFixedTickable));
			if (tickable is IFixedTickable) types.Add(nameof(IFixedTickable));
			if (tickable is IPostFixedTickable) types.Add(nameof(IPostFixedTickable));

			return types.Count > 0 ? string.Join(", ", types) : nameof(ITickable);
		}

		[ContextMenu("Log Registered Tickables")]
		private void LogRegisteredTickables() =>
			Debug.Log(DescribeCounts(), this);

		private string DescribeCounts() =>
			$"Counts: PreEarly={CountActive(_preEarlyTickables)}, Early={CountActive(_earlyTickables)}, PostEarly={CountActive(_postEarlyTickables)}, " +
			$"PreRegular={CountActive(_preTickables)}, Regular={CountActive(_tickables)}, PostRegular={CountActive(_postTickables)}, " +
			$"PreLate={CountActive(_preLateTickables)}, Late={CountActive(_lateTickables)}, PostLate={CountActive(_postLateTickables)}, " +
			$"PreFixed={CountActive(_preFixedTickables)}, Fixed={CountActive(_fixedTickables)}, PostFixed={CountActive(_postFixedTickables)}";

		private void LogTickable(string message)
		{
			if (!enableLogs)
				return;

			Logger.Log(
				$"[{nameof(TickableManager)}] {message}",
				contextObject: this,
				category: LogCategory.Utilities
			);
		}

		private void LogTicked(ITickable tickable, string tickMethod)
		{
			if (!logTickEvents)
				return;

			LogTickable($"{tickMethod} -> {tickable.GetType().Name}");
		}

		private void LogTickEvent(string message)
		{
			if (!logTickEvents)
				return;

			LogTickable(message);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ProcessEarlyTickablesGroup()
		{
			TickAll(_preEarlyTickables);
			TickAll(_earlyTickables);
			TickAll(_postEarlyTickables);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ProcessFixedTickablesGroup()
		{
			TickAll(_preFixedTickables);
			TickAll(_fixedTickables);
			TickAll(_postFixedTickables);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ProcessLateTickablesGroup()
		{
			TickAll(_preLateTickables);
			TickAll(_lateTickables);
			TickAll(_postLateTickables);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ProcessRegularTickablesGroup()
		{
			TickAll(_preTickables);
			TickAll(_tickables);
			TickAll(_postTickables);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TickAll(IPreEarlyTickable?[] items)
		{
			items.ThrowIfNull();

			for (int i = 0, len = items.Length; i < len; i++)
			{
				var t = items[i];
				if (t.IsNull()) continue;

				t.PreEarlyTick();
				LogTicked(t, nameof(IPreEarlyTickable.PreEarlyTick));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TickAll(IEarlyTickable?[] items)
		{
			items.ThrowIfNull();

			for (int i = 0, len = items.Length; i < len; i++)
			{
				var t = items[i];
				if (t.IsNull()) continue;

				t.EarlyTick();
				LogTicked(t, nameof(IEarlyTickable.EarlyTick));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TickAll(IPostEarlyTickable?[] items)
		{
			items.ThrowIfNull();

			for (int i = 0, len = items.Length; i < len; i++)
			{
				var t = items[i];
				if (t.IsNull()) continue;

				t.PostEarlyTick();
				LogTicked(t, nameof(IPostEarlyTickable.PostEarlyTick));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TickAll(IPreFixedTickable?[] items)
		{
			items.ThrowIfNull();

			for (int i = 0, len = items.Length; i < len; i++)
			{
				var t = items[i];
				if (t.IsNull()) continue;

				t.PreFixedTick();
				LogTicked(t, nameof(IPreFixedTickable.PreFixedTick));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TickAll(IFixedTickable?[] items)
		{
			items.ThrowIfNull();

			for (int i = 0, len = items.Length; i < len; i++)
			{
				var t = items[i];
				if (t.IsNull()) continue;

				t.FixedTick();
				LogTicked(t, nameof(IFixedTickable.FixedTick));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TickAll(IPostFixedTickable?[] items)
		{
			items.ThrowIfNull();

			for (int i = 0, len = items.Length; i < len; i++)
			{
				var t = items[i];
				if (t.IsNull()) continue;

				t.PostFixedTick();
				LogTicked(t, nameof(IPostFixedTickable.PostFixedTick));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TickAll(IPreLateTickable?[] items)
		{
			items.ThrowIfNull();

			for (int i = 0, len = items.Length; i < len; i++)
			{
				var t = items[i];
				if (t.IsNull()) continue;

				t.PreLateTick();
				LogTicked(t, nameof(IPreLateTickable.PreLateTick));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TickAll(ILateTickable?[] items)
		{
			items.ThrowIfNull();

			for (int i = 0, len = items.Length; i < len; i++)
			{
				var t = items[i];
				if (t.IsNull()) continue;

				t.LateTick();
				LogTicked(t, nameof(ILateTickable.LateTick));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TickAll(IPostLateTickable?[] items)
		{
			items.ThrowIfNull();

			for (int i = 0, len = items.Length; i < len; i++)
			{
				var t = items[i];
				if (t.IsNull()) continue;

				t.PostLateTick();
				LogTicked(t, nameof(IPostLateTickable.PostLateTick));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TickAll(IPreRegularTickable?[] items)
		{
			items.ThrowIfNull();

			for (int i = 0, len = items.Length; i < len; i++)
			{
				var t = items[i];
				if (t.IsNull()) continue;

				t.PreRegularTick();
				LogTicked(t, nameof(IPreRegularTickable.PreRegularTick));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TickAll(IRegularTickable?[] items)
		{
			items.ThrowIfNull();

			for (int i = 0, len = items.Length; i < len; i++)
			{
				var t = items[i];
				if (t.IsNull()) continue;

				t.RegularTick();
				LogTicked(t, nameof(IRegularTickable.RegularTick));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TickAll(IPostRegularTickable?[] items)
		{
			items.ThrowIfNull();

			for (int i = 0, len = items.Length; i < len; i++)
			{
				var t = items[i];
				if (t.IsNull()) continue;

				t.PostRegularTick();
				LogTicked(t, nameof(IPostRegularTickable.PostRegularTick));
			}
		}
	}

	/// <summary>
	///     Marks callbacks that run before regular Update tickables.
	/// </summary>
	public interface IEarlyTickable : ITickable
	{
		void EarlyTick();
	}

	/// <summary>
	///     Marks callbacks that run during FixedUpdate.
	/// </summary>
	public interface IFixedTickable : ITickable
	{
		void FixedTick();
	}

	/// <summary>
	///     Marks callbacks that run during LateUpdate.
	/// </summary>
	public interface ILateTickable : ITickable
	{
		void LateTick();
	}

	/// <summary>
	///     Marks callbacks that run after early Update tickables.
	/// </summary>
	public interface IPostEarlyTickable : ITickable
	{
		void PostEarlyTick();
	}

	/// <summary>
	///     Marks callbacks that run after FixedUpdate tickables.
	/// </summary>
	public interface IPostFixedTickable : ITickable
	{
		void PostFixedTick();
	}

	/// <summary>
	///     Marks callbacks that run after LateUpdate tickables.
	/// </summary>
	public interface IPostLateTickable : ITickable
	{
		void PostLateTick();
	}

	/// <summary>
	///     Marks callbacks that run after regular Update tickables.
	/// </summary>
	public interface IPostRegularTickable : ITickable
	{
		void PostRegularTick();
	}

	/// <summary>
	///     Marks callbacks that run before early Update tickables.
	/// </summary>
	public interface IPreEarlyTickable : ITickable
	{
		void PreEarlyTick();
	}

	/// <summary>
	///     Marks callbacks that run before FixedUpdate tickables.
	/// </summary>
	public interface IPreFixedTickable : ITickable
	{
		void PreFixedTick();
	}

	/// <summary>
	///     Marks callbacks that run before LateUpdate tickables.
	/// </summary>
	public interface IPreLateTickable : ITickable
	{
		void PreLateTick();
	}

	/// <summary>
	///     Marks callbacks that run before regular Update tickables.
	/// </summary>
	public interface IPreRegularTickable : ITickable
	{
		void PreRegularTick();
	}

	/// <summary>
	///     Marks callbacks that run during regular Update.
	/// </summary>
	public interface IRegularTickable : ITickable
	{
		void RegularTick();
	}

	/// <summary>
	///     Defines a tick callback that can be registered with TickableManager.
	/// </summary>
	public interface ITickable { }
}
