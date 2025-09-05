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
	[Tooltip("The priority used when regenerating multiple Light Probe Boxes at once.\nHigher are processed first, ties are broken by density.")]
	[SerializeField] private int priority;
	public int Priority => priority;
	[Header("Lattice")]
	[Tooltip("Defines how the lattice points are arranged.")]
	[SerializeField] private LatticeStructureType structureType;
	public LatticeStructureType StructureType => structureType;
	[Tooltip("The minimum distance between the lattice points.")]
	[SerializeField, Min(1f)] private float minSpacing = 4f;
	public float MinSpacing => minSpacing;

	[Header("Collision Resolver")]
	[Tooltip("A set of resolver step biases to iterate through.\nTweak these to get better coverage in complex scenes.")]
	[SerializeField] private float[] iterationBiases = { .6f, 1f, 1.2f, 1f };
	public float[] IterationBiases => iterationBiases;
	[Tooltip("Select which layers to use for collision detection.")]
	[SerializeField] private LayerMask collisionLayers = 1;
	public LayerMask CollisionLayers => collisionLayers;
	[Tooltip("The minimum distance a Light Probe must have to the nearest collider.")]
	[SerializeField, Min(.01f)] private float minClearance = .5f;
	public float MinClearance => minClearance;

	[SerializeField, HideInInspector] private Bounds bounds = new(Vector3.up * 3, new Vector3(10, 6, 10));

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