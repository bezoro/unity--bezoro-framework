using System;
using Bezoro.Framework.Core;
using Bezoro.Framework.Unity.Utilities;

namespace Bezoro.Framework.Unity
{
	public sealed class Health :
		I_Health,
		IEquatable<I_Health>,
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

		public void Decrease_Current_Health_By(int value)
		{
			Validation_Helpers.Validate_Positive_Value(this, nameof(Decrease_Current_Health_By), value);

			Current = Math.Max(0, Current - value);
		}

		public void Deplete_Current_Health()
		{
			Current = 0;
		}

		public void Set_Current_Health_To(int value)
		{
			Validation_Helpers.Validate_Positive_Value(this, nameof(Set_Current_Health_To), value);

			Current = Math.Clamp(value, 0, Max);
		}

		public void Increase_Current_Health_By(int value)
		{
			Validation_Helpers.Validate_Positive_Value(this, nameof(Increase_Current_Health_By), value);

			Current = Math.Min(Current + value, Max);
		}

		public void Fully_Restore_Current_Health()
		{
			Current = Max;
		}

		public void Restore_Current_Health_By(int value)
		{
			Current = Math.Min(Current + value, Max);
		}

		public void Decrease_Max_Health_By(int value)
		{
			Validation_Helpers.Validate_Positive_Value(this, nameof(Decrease_Max_Health_By), value);

			Max     = Math.Max(1, Max - value);
			Current = Math.Min(Current, Max);
		}

		public void Increase_Max_Health_By(int value)
		{
			Validation_Helpers.Validate_Positive_Value(this, nameof(Increase_Max_Health_By), value);

			Max = Math.Min(Max + value, int.MaxValue);
		}

		public void Set_Max_Health_To(int value)
		{
			Validation_Helpers.Validate_Positive_Value(this, nameof(Set_Max_Health_To), value);

			Max     = Math.Max(1, value);
			Current = Math.Min(Current, Max);
		}

		public bool Equals(I_Health other)
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
