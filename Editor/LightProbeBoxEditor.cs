using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Object = UnityEngine.Object;

[CustomEditor(typeof(LightProbeBox)), CanEditMultipleObjects]
public class LightProbeBoxEditor : Editor
{
	private BoxBoundsHandle _boxBoundsHandle;

	private static bool _drawDebugGizmos = true;
	private static readonly Color WireFrameColor = new(.8f, .8f, .8f, 1);
	private static readonly Color HandleColor = new(1, .7f, .1f, 1f);
	private static float HandleSize(Vector3 position) => Mathf.Max(HandleUtility.GetHandleSize(position) * 0.05f, 0.2f);

	private static Texture2D _iconTexture;
	private static Texture2D _proIconTexture;
	
	private const int ButtonWidth = 230;
	private const int ButtonHeight = 24;

	// A set of biases for the collision resolver to iterate through. 
	private static readonly float[] IterationBiases = { .6f, 1f, 1.2f, 1f };
	private const int MaxOverlapColliders = 10;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		DrawDebugGizmosToggle();

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		DrawGenerateButton();
		return;

		static void DrawDebugGizmosToggle()
		{
			EditorGUI.BeginChangeCheck();
			_drawDebugGizmos = EditorGUILayout.Toggle("Show Clearance", _drawDebugGizmos);
			if (EditorGUI.EndChangeCheck())
				SceneView.RepaintAll();
		}

		static void DrawGenerateButton()
		{
			var buttonRect = GUILayoutUtility.GetRect(ButtonWidth, ButtonHeight);
			buttonRect.x = (EditorGUIUtility.currentViewWidth - ButtonWidth) / 2;
			buttonRect.width = ButtonWidth;
			
			if (GUI.Button(buttonRect, "Regenerate Probes"))
			{
				var lightProbeBoxes = Selection.GetFiltered<LightProbeBox>(SelectionMode.TopLevel);

				var processedBoxes = new List<LightProbeBox>(lightProbeBoxes.Length);
				using (var collisionResolver = new LightProbeBoxCollisionResolver(MaxOverlapColliders, IterationBiases))
				{
					foreach (var lightProbeBox in lightProbeBoxes.OrderBy(box => box.DensityEstimate()))
					{
						lightProbeBox.Generate(processedBoxes, collisionResolver);
						processedBoxes.Add(lightProbeBox);
					}
				}

				var currentSelection = Selection.objects;
				Selection.objects = Array.Empty<Object>();
				EditorApplication.delayCall += () => Selection.objects = currentSelection;
			}
		}
	}

	private void OnEnable()
	{
		InitializeBoxBoundsHandle();
		SetEditorIcons();
		return;

		void InitializeBoxBoundsHandle()
		{
			_boxBoundsHandle = new BoxBoundsHandle
			{
				wireframeColor = WireFrameColor,
				handleColor = HandleColor,
				midpointHandleSizeFunction = HandleSize
			};
		}

		void SetEditorIcons()
		{
			if (!_proIconTexture || !_iconTexture)
			{
				_proIconTexture =
					AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LightProbeBox/Icon/d_LightProbeBoxIcon.png");
				_iconTexture =
					AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LightProbeBox/Icon/LightProbeBoxIcon.png");

				var script = MonoScript.FromMonoBehaviour((LightProbeBox)target);
				EditorGUIUtility.SetIconForObject(script, EditorGUIUtility.isProSkin ? _proIconTexture : _iconTexture);
			}
		}
	}

	private void OnSceneGUI()
	{
		if (target is not LightProbeBox lightProbeBox) return;
		if (!lightProbeBox || !lightProbeBox.gameObject) return;

		DrawBoundsHandle();
		DrawDebugGizmos();
		return;

		void DrawBoundsHandle()
		{
			var handleMatrix = Matrix4x4.TRS(
				lightProbeBox.transform.position,
				lightProbeBox.transform.rotation,
				lightProbeBox.transform.lossyScale
			);

			using (new Handles.DrawingScope(handleMatrix))
			{
				var bounds = lightProbeBox.Bounds;
				_boxBoundsHandle.center = bounds.center;
				_boxBoundsHandle.size = bounds.size;

				EditorGUI.BeginChangeCheck();
				_boxBoundsHandle.DrawHandle();
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(lightProbeBox, "Modify LightProbeBox bounds");

					bounds.center = _boxBoundsHandle.center;
					bounds.size = _boxBoundsHandle.size;
					lightProbeBox.Bounds = bounds;

					EditorUtility.SetDirty(lightProbeBox);
				}
			}
		}

		void DrawDebugGizmos()
		{
			if (!_drawDebugGizmos) return;
			if (!lightProbeBox.TryGetComponent(out LightProbeGroup lightProbeGroup) ||
				lightProbeGroup.probePositions.Length == 0) return;

			// Handles.color = ColliderColor;
			Handles.color = WireFrameColor;
			Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

			var camera = SceneView.lastActiveSceneView.camera;
			var normal = camera.transform.forward;
			foreach (var probePosition in lightProbeGroup.probePositions)
			{
				var worldPosition = lightProbeBox.transform.TransformPoint(probePosition);
				Handles.DrawWireDisc(worldPosition, normal, lightProbeBox.MinClearance);
			}
		}
	}
}