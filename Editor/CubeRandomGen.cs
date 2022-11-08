using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[ExecuteInEditMode]
class RandomGenWindow : EditorWindow {
	Object spawnableObject;
	Object targetCube;
	int numberOfEncryptions = 1;
	
	[MenuItem ("Tools/Random Encryption Placer")]
	
	public static void ShowWindow () {
		EditorWindow.GetWindow(typeof(RandomGenWindow));
	}
	
	void OnGUI () {
		GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
		spawnableObject = EditorGUILayout.ObjectField("Object to Clone:", spawnableObject, typeof(GameObject), true);
		targetCube = EditorGUILayout.ObjectField("Target Cube:", targetCube, typeof(GameObject), true);
		numberOfEncryptions = EditorGUILayout.IntSlider("Object Count:", numberOfEncryptions, 1, 100);
		if (GUILayout.Button("Spawn Objects")) {
///			if (spawnableObject == null)
///				this.ShowNotification(GUIContent("No spawnable object selected."));
///			else if (targetCube == null)
///				this.showNotification(GUIContent("No target cube selected"));
///			else
			{
				for (int i = 0; i < numberOfEncryptions; i++)
				{
					spawnObject((GameObject)spawnableObject, (GameObject)targetCube);
				}
			}
		}
	}
	
	public void spawnObject(GameObject spawnableThing, GameObject targetLocation)
	{
		Vector3 rndPosWithin;
		rndPosWithin = targetLocation.transform.TransformPoint(Random.Range(-1f, 1f) * .5f, Random.Range(-1f, 1f) * .5f, Random.Range(-1f, 1f) * .5f);
		Instantiate(spawnableThing, rndPosWithin, targetLocation.transform.rotation);
	}
}



