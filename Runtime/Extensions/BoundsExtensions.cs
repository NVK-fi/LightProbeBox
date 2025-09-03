using System.Collections.Generic;
using UnityEngine;
using static BoundsExtensions.LatticeStructureType;

public static class BoundsExtensions
{
	private const float Sqrt2 = 1.41421f;

	public enum LatticeStructureType
	{
		SimpleCubic = 0,
		BodyCenteredCubic = 1,
	}

	/// <summary>
	/// Generates a lattice within specified bounds based on structure requirements.
	/// Returns local positions.
	/// </summary>
	public static IEnumerable<Vector3> GetLattice(this Bounds bounds, LatticeStructureType latticeStructureType, float minSpacing)
	{
		minSpacing = Mathf.Max(1f, minSpacing);
		if (latticeStructureType is BodyCenteredCubic) minSpacing /= Sqrt2;

		var count = Vector3.Max(Vector3.one, new Vector3(
			Mathf.CeilToInt(bounds.size.x / minSpacing),
			Mathf.CeilToInt(bounds.size.y / minSpacing),
			Mathf.CeilToInt(bounds.size.z / minSpacing))
		);

		var spacing = new Vector3(
			bounds.size.x / Mathf.Max(1, count.x - 1),
			bounds.size.y / Mathf.Max(1, count.y - 1),
			bounds.size.z / Mathf.Max(1, count.z - 1)
		);

		for (var x = 0; x < count.x; x++)
			for (var y = 0; y < count.y; y++)
				for (var z = 0; z < count.z; z++)
				{
					if (latticeStructureType is BodyCenteredCubic && (x+y+z) % 2 == 1) continue;
					
					yield return bounds.min + new Vector3(
						count.x <= 1 ? bounds.size.x * 0.5f : x * spacing.x,
						count.y <= 1 ? bounds.size.y * 0.5f : y * spacing.y,
						count.z <= 1 ? bounds.size.z * 0.5f : z * spacing.z
					);
				}
	}
}