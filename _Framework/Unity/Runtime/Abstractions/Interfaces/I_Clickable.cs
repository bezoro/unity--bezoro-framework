namespace Bezoro.Framework.Unity
{
	public interface I_Clickable
	{
		public bool Is_Click      => Is_Click_Down || Is_Click_Up;
		public bool Is_Click_Down { get; }
		public bool Is_Click_Up   { get; }

		public void On_Click();
		public void On_Click_Down();
		public void On_Click_Up();
	}
}
