using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightProbeGroup)), CanEditMultipleObjects]
public class LightProbeGroupEditor : Editor
{
	private bool _isManagedByLightProbeBox;

	private void OnEnable()
	{
		var lightProbeGroup = (LightProbeGroup)target;
		_isManagedByLightProbeBox = lightProbeGroup.GetComponent<LightProbeBox>();
	}

	public override void OnInspectorGUI()
	{
		if (_isManagedByLightProbeBox)
			EditorGUILayout.HelpBox("This Light Probe Group is generated with a Light Probe Box.", MessageType.Info);

		base.OnInspectorGUI();
	}
}