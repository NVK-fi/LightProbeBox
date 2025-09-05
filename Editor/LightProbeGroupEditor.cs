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
			EditorGUILayout.HelpBox("The Light Probes are generated with the attached Light Probe Box.", MessageType.Info);

		base.OnInspectorGUI();
	}
}