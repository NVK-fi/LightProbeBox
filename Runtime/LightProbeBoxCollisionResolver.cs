using System;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
public class LightProbeBoxCollisionResolver : IDisposable
{
	// A tiny offset for the collision calculations.
	private const float E = 1e-4f;
	private bool _isDisposed;

	private readonly GameObject _detectorObject;
	private readonly SphereCollider _detectorCollider;
	private readonly Collider[] _overlapColliders;

	/// <summary>
	/// Handles collision detection and resolution for light probes.
	/// </summary>
	public LightProbeBoxCollisionResolver()
	{
		_overlapColliders = new Collider[10];
		_detectorObject = new GameObject("Probe Collider", typeof(SphereCollider));
		_detectorCollider = _detectorObject.GetComponent<SphereCollider>();
	}

	/// <summary>
	/// Attempts to find an unobstructed position for a light probe by resolving collisions if possible.
	/// </summary>
	public bool TryGetUnobstructedLocalPosition(LightProbeBox lightProbeBox, Vector3 sampleWorldPosition, out Vector3 unobstructedLocalPosition)
	{
		if (_isDisposed) throw new ObjectDisposedException(nameof(LightProbeBoxCollisionResolver));

		_detectorCollider.radius = lightProbeBox.MinClearance;
		unobstructedLocalPosition = lightProbeBox.transform.InverseTransformPoint(sampleWorldPosition);
		if (ComputeOverlaps(sampleWorldPosition, lightProbeBox) == 0) return true;
		
		if (lightProbeBox.IterationBiases.Length <= 0) return false;
		foreach (var bias in lightProbeBox.IterationBiases)
		{
			var newWorldPosition = ResolveCollisionStep(sampleWorldPosition, lightProbeBox, bias);
			var deltaSqrMagnitude = (newWorldPosition - sampleWorldPosition).sqrMagnitude;
			sampleWorldPosition = newWorldPosition;

			if (deltaSqrMagnitude < E * E) break;
		}
		
		unobstructedLocalPosition = lightProbeBox.transform.InverseTransformPoint(sampleWorldPosition);
		if (!lightProbeBox.Bounds.Contains(unobstructedLocalPosition)) return false;
		
		return ComputeOverlaps(sampleWorldPosition, lightProbeBox) == 0;
	}

	private int ComputeOverlaps(Vector3 worldPosition, LightProbeBox lightProbeBox)
	{
		if (lightProbeBox.CollisionLayers == 0) return 0;
		
		_detectorCollider.enabled = false;
		var count = Physics.OverlapSphereNonAlloc(worldPosition, lightProbeBox.MinClearance - E, _overlapColliders,
			lightProbeBox.CollisionLayers);
		_detectorCollider.enabled = true;
		return count;
	}

	private Vector3 ResolveCollisionStep(Vector3 worldPosition, LightProbeBox lightProbeBox, float bias)
	{
		var overlapCount = ComputeOverlaps(worldPosition, lightProbeBox);
		var totalCorrection = Vector3.zero;
		
		for (var i = 0; i < overlapCount; i++)
		{
			if (!Physics.ComputePenetration(
					_detectorCollider, worldPosition, Quaternion.identity,
					_overlapColliders[i], _overlapColliders[i].transform.position, _overlapColliders[i].transform.rotation,
					out var direction, out var distance)) continue;
			
			totalCorrection += direction * distance;
		}

		return worldPosition + totalCorrection * bias;
	}

	public void Dispose()
	{
		if (_isDisposed) return;

		if (_detectorObject)
			Object.DestroyImmediate(_detectorObject);
		
		_isDisposed = true;
	}
}
#endif