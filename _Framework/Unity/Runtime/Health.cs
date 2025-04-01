using System;
using Bezoro.Framework.Core;
using Bezoro.Framework.Unity.Utilities;

namespace Bezoro.Framework.Unity
{
	public sealed class Health :
		IHealth,
		IEquatable<IHealth>,
		ICloneable
	{
		public Health(int max, int? current = null)
		{
			Validate_Health_Parameters(max, current, "Constructor");

			Max     = max;
			Current = current ?? max;
			_guid   = Guid.NewGuid();
		}

		private readonly Guid _guid;

		public void DecreaseCurrentHealthBy(int value)
		{
			Validation_Helpers.Validate_Positive_Value(this, nameof(DecreaseCurrentHealthBy), value);

			Current = Math.Max(0, Current - value);
		}

		public void DepleteCurrentHealth()
		{
			Current = 0;
		}

		public void SetCurrentHealthTo(int value)
		{
			Validation_Helpers.Validate_Positive_Value(this, nameof(SetCurrentHealthTo), value);

			Current = Math.Clamp(value, 0, Max);
		}

		public void IncreaseCurrentHealthBy(int value)
		{
			Validation_Helpers.Validate_Positive_Value(this, nameof(IncreaseCurrentHealthBy), value);

			Current = Math.Min(Current + value, Max);
		}

		public void FullyRestoreCurrentHealth()
		{
			Current = Max;
		}

		public void RestoreCurrentHealthBy(int value)
		{
			Current = Math.Min(Current + value, Max);
		}

		public void DecreaseMaxHealthBy(int value)
		{
			Validation_Helpers.Validate_Positive_Value(this, nameof(DecreaseMaxHealthBy), value);

			Max     = Math.Max(1, Max - value);
			Current = Math.Min(Current, Max);
		}

		public void IncreaseMaxHealthBy(int value)
		{
			Validation_Helpers.Validate_Positive_Value(this, nameof(IncreaseMaxHealthBy), value);

			Max = Math.Min(Max + value, int.MaxValue);
		}

		public void SetMaxHealthTo(int value)
		{
			Validation_Helpers.Validate_Positive_Value(this, nameof(SetMaxHealthTo), value);

			Max     = Math.Max(1, value);
			Current = Math.Min(Current, Max);
		}

		public bool Equals(IHealth other)
		{
			return other is not null && ReferenceEquals(this, other);
		}

		public override int GetHashCode()
		{
			return _guid.GetHashCode();
		}

		public object Clone()
		{
			return new Health(Max, Current);
		}

		public int Current { get; private set; }

		public int   Max        { get; private set; }
		public float Percentage => Current / (float)Max;

		#region Plumbing

		private void Validate_Health_Parameters(
			int max,
			int? current,
			string methodName
		)
		{
			Validation_Helpers.Validate_Positive_Value(
				this,
				methodName,
				max
			);

			if (current.HasValue)
			{
				Validation_Helpers.Validate_Positive_Value(
					this,
					methodName,
					current.Value
				);

				Validation_Helpers.Validate_Value_Not_Above_Max(
					this,
					methodName,
					current.Value,
					max
				);
			}
		}

		#endregion
	}
}
