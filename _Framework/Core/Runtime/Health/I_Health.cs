namespace Bezoro.Framework.Core
{
	public interface I_Health
	{
		public int   Current    { get; }
		public int   Max        { get; }
		public float Percentage { get; }

		// public int GetHashCode();
		public object Clone();

		public void Decrease_Current_Health_By(int value);
		public void Decrease_Max_Health_By(int value);
		public void Deplete_Current_Health();

		public bool Equals(I_Health other);
		public void Fully_Restore_Current_Health();
		public void Increase_Current_Health_By(int value);
		public void Increase_Max_Health_By(int value);
		public void Restore_Current_Health_By(int value);
		public void Set_Current_Health_To(int value);
		public void Set_Max_Health_To(int value);
	}
}
