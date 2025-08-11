using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
public static class LightProbeBoxExtensions
{
	/// <summary>
	/// Returns whether the world position is within the reach of any LightProbeBox in the list. 
	/// </summary>
	public static bool CanReachPosition(this List<LightProbeBox> lightProbeBoxes, Vector3 worldPosition)
	{
		if (lightProbeBoxes.Count == 0) return false;

		foreach (var lightProbeBox in lightProbeBoxes)
		{
			var localPosition = lightProbeBox.transform.InverseTransformPoint(worldPosition);
			
			if (lightProbeBox.Bounds.GetDistance(localPosition) < lightProbeBox.MinSpacing * .5f)
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