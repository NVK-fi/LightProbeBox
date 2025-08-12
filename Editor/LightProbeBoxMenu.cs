using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

#if UNITY_EDITOR
public static class LightProbeBoxMenu
{
	[MenuItem("GameObject/Light/Light Probe Box", priority = 80003)]
	private static void CreateLightProbeBox()
	{
		var gameObject = new GameObject("Light Probe Box");
		Undo.RegisterCreatedObjectUndo(gameObject, "Create Light Probe Box");
		gameObject.AddComponent<LightProbeBox>();

		GameObjectUtility.SetParentAndAlign(gameObject, Selection.activeGameObject);
		StageUtility.PlaceGameObjectInCurrentStage(gameObject);

		if (EditorPrefs.GetBool("Create3DObject.PlaceAtWorldOrigin", false))
			gameObject.transform.position = Vector3.zero;
		else if (SceneView.lastActiveSceneView)
			SceneView.lastActiveSceneView.MoveToView(gameObject.transform);
		
		Selection.activeGameObject = gameObject;
	}
}
#endif