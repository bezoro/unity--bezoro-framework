using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Bezoro.Unity.Core.Jobs;

[BurstCompile]
/// <summary>
/// Generates evenly spaced raycast origins on a plane.
/// </summary>
public struct GeneratePointsOnPlaneJob : IJobParallelFor
{
	#region Outputs

	/// <summary>
	///     The resulting points generated on the plane.
	/// </summary>
	[WriteOnly] public NativeArray<float3> ResultPoints;

	#endregion

	#region Interface Implementations

	public void Execute(int index)
	{
		if (NumPointsWidth <= 0 && NumPointsHeight <= 0) return;

		int row    = index / NumPointsWidth;
		int column = index % NumPointsWidth;

		float stepSizeX = Width / (NumPointsWidth - 1);
		float stepSizeY = Height / (NumPointsHeight - 1);

		var point = Center +
					(Width * 0.5f - column * stepSizeX) * RightAxis +
					(Height * 0.5f - row * stepSizeY) * UpAxis;

		ResultPoints[index] = point;
	}

	#endregion

	#region Inputs

	/// <summary>
	///     The plane's center.
	/// </summary>
	[ReadOnly] public float3 Center;

	/// <summary>
	///     The plane's width.
	/// </summary>
	[ReadOnly] public float Width;

	/// <summary>
	///     The plane's height.
	/// </summary>
	[ReadOnly] public float Height;

	/// <summary>
	///     The number of points along the plane's width.
	/// </summary>
	[ReadOnly] public int NumPointsWidth;

	/// <summary>
	///     The number of points along the plane's height.
	/// </summary>
	[ReadOnly] public int NumPointsHeight;

	/// <summary>
	///     The plane's right axis.
	///     <para>Make sure its normalized.</para>
	/// </summary>
	[ReadOnly] public float3 RightAxis;

	/// <summary>
	///     The plane's up axis.
	///     <para>Make sure its normalized.</para>
	/// </summary>
	[ReadOnly] public float3 UpAxis;

	#endregion
}
