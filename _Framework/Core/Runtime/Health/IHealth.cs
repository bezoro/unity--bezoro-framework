namespace Bezoro.Framework.Core
{
	public interface IHealth
	{
		public int   Current    { get; }
		public int   Max        { get; }
		public float Percentage { get; }

		// public int GetHashCode();

		public void SetCurrentHealthTo(int value);
		public void DecreaseCurrentHealthBy(int value);
		public void IncreaseCurrentHealthBy(int value);
		public void RestoreCurrentHealthBy(int value);
		public void DepleteCurrentHealth();
		public void FullyRestoreCurrentHealth();

		public void SetMaxHealthTo(int value);
		public void DecreaseMaxHealthBy(int value);
		public void IncreaseMaxHealthBy(int value);
	}
}
