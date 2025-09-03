using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static BoundsExtensions;

#if UNITY_EDITOR
[RequireComponent(typeof(LightProbeGroup)), DisallowMultipleComponent]
public class LightProbeBox : MonoBehaviour
{
	[field: Header("Lattice")]
	[field: Tooltip("Defines how the lattice points are arranged.")]
	[field:SerializeField] public LatticeStructureType StructureType { get; private set; }
	[field: Tooltip("The minimum distance between the lattice points.")]
	[field: SerializeField, Min(1f)] public float MinSpacing { get; private set; } = 4f;

	[field: Header("Collision Resolver")]
	[field: Tooltip("Determines whether the external bounds must be respected.")]
	[field: SerializeField] public bool ConstrainWithinBounds { get; private set; } = true;
	[field: Tooltip("Select which layers to use for collision detection.")]
	[field: SerializeField] public LayerMask CollisionLayers { get; private set; } = 1;
	[field: Tooltip("The minimum distance a Light Probe must have to the nearest collider.")]
	[field: SerializeField,Min(.01f)] public float MinClearance { get; private set; } = 0.5f;
	
	[SerializeField, HideInInspector]
	private Bounds bounds = new(Vector3.up * 3, new Vector3(10, 6, 10));
	public Bounds Bounds
	{
		get => bounds;
		set
		{
			if (Equals(bounds, value)) return;
			
			Undo.RecordObject(this, "Modify LightProbeBox Bounds");
			bounds = value;
			
			EditorUtility.SetDirty(this);
		}
	}

	private LightProbeGroup _lightProbeGroup;

	/// <summary>
	/// Generates optimal positions for the probes in LightProbeGroup.
	/// </summary>
	public void Generate(List<LightProbeBox> processedBoxes, LightProbeBoxCollisionResolver collisionResolver)
	{
		EnsureLightProbeGroupExists();
		ClearProbePositions();
		GenerateNewProbePositions();
		return;

		void GenerateNewProbePositions()
		{
			var newProbeLocalPositions = new List<Vector3>();
			var latticeSize = Bounds.size - Vector3.one * (2 * MinClearance);
			var latticeBounds = new Bounds(Bounds.center, latticeSize);

			foreach (var sampleLocalPosition in latticeBounds.GetLattice(StructureType, MinSpacing))
			{
				var sampleWorldPosition = transform.TransformPoint(sampleLocalPosition);

				// Skip if the position is already occupied by another LightProbeBox.
				if (processedBoxes.CanReachPosition(sampleWorldPosition)) continue;

				// Skip if the position is obstructed and the collision resolver fails to move the probe.
				if (!collisionResolver.TryGetUnobstructedLocalPosition(this, sampleWorldPosition, out var unobstructedLocalPosition))
					continue;

				newProbeLocalPositions.Add(unobstructedLocalPosition);
			}

			_lightProbeGroup.probePositions = newProbeLocalPositions.ToArray();
			EditorUtility.SetDirty(_lightProbeGroup);
		}
	}

	private void EnsureLightProbeGroupExists()
	{
		if (TryGetComponent(out _lightProbeGroup)) return;

		_lightProbeGroup = Undo.AddComponent<LightProbeGroup>(gameObject);
		EditorUtility.SetDirty(gameObject);
	}

	private void ClearProbePositions()
	{
		if (!_lightProbeGroup) return;

		_lightProbeGroup.probePositions = Array.Empty<Vector3>();
		EditorUtility.SetDirty(_lightProbeGroup);
	}

	// Called when the component is added or reset.
	private void Reset()
	{
		EnsureLightProbeGroupExists();
		ClearProbePositions();
		ReorderComponents();
		return;

		// Moves the LightProbeGroup component below the LightProbeBox in the inspector.
		void ReorderComponents()
		{
			var components = GetComponents<Component>();
			var lightProbeBoxIndex = Array.IndexOf(components, this);
			var lightProbeGroupIndex = Array.IndexOf(components, _lightProbeGroup);
			for (var i = 0; i < lightProbeBoxIndex - lightProbeGroupIndex; i++)
				ComponentUtility.MoveComponentDown(_lightProbeGroup);
		}
	}
}
#endif