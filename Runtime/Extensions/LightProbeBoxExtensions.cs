using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
public static class LightProbeBoxExtensions
{
	/// <summary>
	/// An SDF to calculate the shortest distance from a world position to bounds' surface.
	/// Returns negative values for points inside and positive for points outside.
	/// </summary>
	public static float GetDistanceTo(this LightProbeBox lightProbeBox, Vector3 worldPosition)
	{
		var localPosition = lightProbeBox.transform.InverseTransformPoint(worldPosition);
		var bounds = lightProbeBox.Bounds;
		
		var q = new Vector3(
			Mathf.Abs(localPosition.x - bounds.center.x) - bounds.size.x * 0.5f,
			Mathf.Abs(localPosition.y - bounds.center.y) - bounds.size.y * 0.5f,
			Mathf.Abs(localPosition.z - bounds.center.z) - bounds.size.z * 0.5f
		);

		return Mathf.Min(Mathf.Max(q.x, Mathf.Max(q.y, q.z)), 0f) +
				new Vector3(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f), Mathf.Max(q.z, 0f)).magnitude;
	}
	
	
	/// <summary>
	/// Returns whether the world position is within the reach (spacing included) of any LightProbeBox in the list. 
	/// </summary>
	public static bool CanReachPosition(this List<LightProbeBox> lightProbeBoxes, Vector3 worldPosition)
	{
		if (lightProbeBoxes.Count == 0) return false;

		foreach (var lightProbeBox in lightProbeBoxes)
		{
			if (lightProbeBox.GetDistanceTo(worldPosition) < lightProbeBox.MinSpacing * .5f)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Quickly estimates the density of a LightProbeBox.
	/// Inaccurate but useful for sorting.
	/// </summary>
	public static float DensityEstimate(this LightProbeBox lightProbeBox)
	{
		var size = lightProbeBox.Bounds.size;
		var spacing = lightProbeBox.MinSpacing;
		var count = new Vector3(
			Mathf.Ceil(size.x / spacing),
			Mathf.Ceil(size.y / spacing),
			Mathf.Ceil(size.z / spacing)
		);
		
		var totalProbes = count.x * count.y * count.z;
		var volume = Mathf.Max(size.x * size.y * size.z, 1);
		return totalProbes / volume;
	}
}
#endif