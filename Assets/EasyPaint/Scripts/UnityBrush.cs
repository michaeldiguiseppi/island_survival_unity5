using UnityEngine;
using UnityEditor;
using System.Collections;

public class UnityBrush : EditorWindow {

	GameObject obj;
	Vector3 offset;
	Vector3 scale = new Vector3(1, 1, 1);
	Vector3 minScale;
	Vector3 maxScale;
	Vector3 rotation;
	Vector3 minRotation;
	Vector3 maxRotation;
	float radius = 5;
	int minObjects;
	int maxObjects;
	GameObject parentObject;

	GUISkin skin1;
	GUISkin skin2;
	string clickButton = "Click Mouse";
	string holdButton = "Hold Mouse";

	bool ready = false;
	bool spawnOne = true;
	bool onClick = true;
	bool holding = false;
	bool randomRotation = false;
	bool isParent = false;
	bool objectParent = false;
	bool changeScale = false;
	bool randomScale = false;
	bool dynamicOffset = false;
	float holdCd = 1f;
	float lastHold;

	// Add menu item named "My Window" to the Window menu
	[MenuItem("Window/Easy Paint: Prefabs")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(UnityBrush), false, "EasyPaint: Prefabs");
	}

	//Frontend
	void OnGUI()
	{
		GUILayout.Label ("Easy Paint", EditorStyles.boldLabel);

		//Check that tool is ready to paint
		if (ready) {
			if (GUILayout.Button ("Ready", skin1.GetStyle("Button"), GUILayout.Height(40))) {
				ready = false;
			}
		} else {
			if (GUILayout.Button ("Not ready", skin2.GetStyle("Button"), GUILayout.Height(40))) {
				ready = true;
			}
		}
		EditorGUILayout.Space ();

		//Object selection
		GUILayout.Label ("Select an object", EditorStyles.boldLabel);
		if (obj) {
			obj = (GameObject)EditorGUILayout.ObjectField (obj, typeof(GameObject), true);
		} else {
			GUILayout.Label("No object detected", skin1.GetStyle("Label"));
			obj = (GameObject)EditorGUILayout.ObjectField (obj, typeof(GameObject), true);
			GUILayout.Label("Please select an object", skin1.GetStyle("Label"));
		}
		EditorGUILayout.Space ();

		//Offset
		if (dynamicOffset) {
			if (GUILayout.Button ("Dynamic Offset", skin1.GetStyle("Button"), GUILayout.Height(30))) {
				dynamicOffset = false;
			}
		} else {
			if (GUILayout.Button ("Static Offset", skin2.GetStyle("Button"), GUILayout.Height(30))) {
				dynamicOffset = true;
			}
		}
		offset = EditorGUILayout.Vector3Field ("Offset", offset);
		EditorGUILayout.Space ();

		//Object rotation
		GUILayout.Label ("Object Rotation", EditorStyles.boldLabel);
		if (randomRotation) {
			if (GUILayout.Button ("Random Rotation", skin1.GetStyle("Button"), GUILayout.Height(30))) {
				randomRotation = false;
			}
			minRotation = EditorGUILayout.Vector3Field ("Minimum Rotation: ", minRotation);
			maxRotation = EditorGUILayout.Vector3Field ("Maximum Rotation: ", maxRotation);
		} else {
			if (GUILayout.Button ("Selected Rotation", skin2.GetStyle("Button"), GUILayout.Height(30))) {
				randomRotation = true;
			}
			rotation = EditorGUILayout.Vector3Field ("Rotation: ", rotation);
		}
		EditorGUILayout.Space ();	

		//Change between states (click/hold)
		GUILayout.Label ("Spawn mode", EditorStyles.boldLabel);
		if (onClick) {
			if (GUILayout.Button (clickButton, skin1.GetStyle("Button"), GUILayout.Height(35))) {
				onClick = false;
			}
		} else {
			if (GUILayout.Button (holdButton, skin2.GetStyle("Button"), GUILayout.Height(35))) {
				onClick = true;
			}
			holdCd = EditorGUILayout.FloatField ("Interval (seconds)", holdCd);
		}
		EditorGUILayout.Space ();	

		//Change between spawn (one/many)
		if (spawnOne) {
			if (GUILayout.Button ("One object", skin1.GetStyle("Button"), GUILayout.Height(35))) {
				spawnOne = false;
			}
		} else {
			if (GUILayout.Button ("Multiple objects", skin2.GetStyle("Button"), GUILayout.Height(35))) {
				spawnOne = true;
			}
			radius = EditorGUILayout.FloatField ("Brush Radius", radius);
			minObjects = EditorGUILayout.IntField ("Minimum Objects", minObjects);
			maxObjects = EditorGUILayout.IntField ("Maximum Objects", maxObjects);
		}
		EditorGUILayout.Space ();

		//Set parent
		isParent = EditorGUILayout.BeginToggleGroup ("Change Object's Parent", isParent);
		if (objectParent) {//change the parent to one of the objects in the scene
			if (GUILayout.Button ("Specific Object", skin1.GetStyle("Button"), GUILayout.Height(30))) {
				objectParent = false;
			}
			if (parentObject) {
				parentObject = (GameObject)EditorGUILayout.ObjectField (parentObject, typeof(GameObject), true);
			} else {
				GUILayout.Label ("No parent object detected", skin1.GetStyle ("Label"));
				parentObject = (GameObject)EditorGUILayout.ObjectField (parentObject, typeof(GameObject), true);
				GUILayout.Label ("Please select an object", skin1.GetStyle ("Label"));
			}
		} else {//change the parent to the raycays object
			if (GUILayout.Button ("Hit Object", skin2.GetStyle("Button"), GUILayout.Height(30))) {
				objectParent = true;
			}
		}
		EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.Space ();

		//Change scale
		changeScale = EditorGUILayout.BeginToggleGroup ("Change Object's Scale", changeScale);
		if (randomScale) {//change scale within a range
			if (GUILayout.Button ("Random Scale", skin1.GetStyle("Button"), GUILayout.Height(30))) {
				randomScale = false;
			}
			minScale = EditorGUILayout.Vector3Field ("Minimum Scale: ", minScale);
			maxScale = EditorGUILayout.Vector3Field ("Maximum Scale: ", maxScale);
		} else {//change scale
			if (GUILayout.Button ("Selected Scale", skin2.GetStyle("Button"), GUILayout.Height(30))) {
				randomScale = true;
			}
			scale = EditorGUILayout.Vector3Field ("Scale: ", scale);
		}
		EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.Space ();
	}

	//Backend
	public void OnSceneGUI(SceneView sceneView){
		if (ready){
			RaycastHit hit;
			if (Physics.Raycast (HandleUtility.GUIPointToWorldRay (Event.current.mousePosition), out hit)) {
				
				if (onClick) {//clicking
					if (spawnOne) {
						if (Event.current.button == 1) {
							if (Event.current.type == EventType.MouseDown){
							if (obj) {
								if (isParent) {
									if (objectParent) { //set parent to a specific object
										if (randomRotation) {
											if (changeScale) {//change scale
												if (randomScale) {//set to random scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
														GameObject newObj = Instantiate (obj, hit.point + (newOffset), Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.localScale = newScale;
														newObj.transform.parent = parentObject.transform;
													} else {//static offset
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.localScale = newScale;
														newObj.transform.parent = parentObject.transform;
													}
												} else {//set to selected scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.localScale = scale;
														newObj.transform.parent = parentObject.transform;
													} else {//static offset
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.localScale = scale;
														newObj.transform.parent = parentObject.transform;
													}
												}
											} else {//leave the scale as it is
												if (dynamicOffset) {//dynamic offset
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
													GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
													newObj.transform.parent = parentObject.transform;
												} else {//static offset
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
													newObj.transform.parent = parentObject.transform;
												}
											}
										} else {
											if (changeScale) { 
												if (randomScale) {//random scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.localScale = newScale;
														newObj.transform.parent = parentObject.transform;
													} else {//static offset
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.localScale = newScale;
														newObj.transform.parent = parentObject.transform;
													}
												} else {//change to selected scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.localScale = scale;
														newObj.transform.parent = parentObject.transform;
													} else {//static offset
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.localScale = scale;
														newObj.transform.parent = parentObject.transform;
													}
												}
											} else { //leave the scale
												if (dynamicOffset) {//dynamic offset
													Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
													GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
													newObj.transform.parent = parentObject.transform;
												} else {//static offset
													GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
													newObj.transform.parent = parentObject.transform;
												}
											}
										}
									} else { //set parent to hit object
										if (randomRotation) {
											if (changeScale) {
												if (randomScale) {//set random scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.localScale = newScale;
														newObj.transform.parent = hit.transform;
													} else {//static offset
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.localScale = newScale;
														newObj.transform.parent = hit.transform;
													}
												} else {//set selected scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.localScale = scale;
														newObj.transform.parent = hit.transform;
													} else {//static offset
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.localScale = scale;
														newObj.transform.parent = hit.transform;
													}
												}
											} else {//leave scale
												if (dynamicOffset) {//dynamic offset
													Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
													newObj.transform.parent = hit.transform;
												} else {//static offset
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
													newObj.transform.parent = hit.transform;
												}
											}
										} else { //selected rotation
											if (changeScale) {
												if (randomScale) {//set random scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.localScale = newScale;
														newObj.transform.parent = hit.transform;
													} else {//static offset
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.localScale = newScale;
														newObj.transform.parent = hit.transform;
													}
												} else {//selected scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.localScale = scale;
														newObj.transform.parent = hit.transform;
													} else {//static offset
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.localScale = scale;
														newObj.transform.parent = hit.transform;
													}
												}
											} else {//leave scale
												if (dynamicOffset) {//dynamic offset
													Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
													GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
													newObj.transform.parent = hit.transform;
												} else {//static offset
													GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
													newObj.transform.parent = hit.transform;
												}
											}
										}
									}
								} else {//spawn without a parent
									if (randomRotation) {
										if (changeScale) {
											if (randomScale) {//random scale
												if (dynamicOffset) {//dynamic offset
													Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
													GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
													newObj.transform.localScale = newScale;
												} else {//static offset
													Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
													newObj.transform.localScale = newScale;
												}
											} else {//selected scale
												if (dynamicOffset) {//dynamic offset
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
													GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
													newObj.transform.localScale = scale;
												} else {//static offset
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
													newObj.transform.localScale = scale;
												}
											}
										} else {//spawn with default scale
											if (dynamicOffset) {//dynamic offset
												Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
												Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
												Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation));
											} else {//static offset
												Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
												Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation));
											}
										}
									} else {
										if (changeScale) {
											if (randomScale) {//random scale
												if (dynamicOffset) {//dynamic offset
													Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
													GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
													newObj.transform.localScale = newScale;
												} else {//static offset
													Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
													newObj.transform.localScale = newScale;
												}
											} else {//selected scale
												if (dynamicOffset) {//dynamic offset
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
													GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
													newObj.transform.localScale = scale;
												} else {//static offset
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
													newObj.transform.localScale = scale;
												}
											}
										} else {//spawn with default scale
											if (dynamicOffset) {//dynamic offset
												Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
												Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
												Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation));
											} else {//static offset
												Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
												Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation));
											}
										}
									}
								}
							} else
								Debug.Log ("No object detected, please select an object");
							}
						}
					} else {//Spawn multiple
						Handles.color = Color.red;
						Handles.DrawWireDisc (hit.point, hit.normal, radius);
						SceneView.RepaintAll ();
						if (Event.current.button == 1) {
							if (Event.current.type == EventType.MouseDown){
							int numberOfObjects = Random.Range (minObjects, maxObjects);
							Vector3[] objectsPositions = new Vector3[numberOfObjects];
							for (int i = 0; i < objectsPositions.Length; i++) {
								objectsPositions [i] = RandomPosition (radius, objectsPositions, hit.point);
								if (obj) {
									if (isParent) {
										if (objectParent) {//spawn to a specific object
											if (randomRotation) {
												if (changeScale) {//change scale
													if (randomScale) {//random scale
														if (dynamicOffset) {//dynamic offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
															//GameObject newObj = Instantiate (obj, objectsPositions [i] + newOffset, Quaternion.Euler (newRotation)) as GameObject;
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (newRotation);
															newObj.transform.parent = parentObject.transform;

														} else {//static offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															//GameObject newObj = Instantiate (obj, objectsPositions [i] + offset, Quaternion.Euler (newRotation)) as GameObject;
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (newRotation);
															newObj.transform.parent = parentObject.transform;
														}
													} else {//selected scale
														if (dynamicOffset) {//dynamic offset
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (newRotation);
															newObj.transform.parent = parentObject.transform;
														} else {//static offset
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (newRotation);
															newObj.transform.parent = parentObject.transform;
														}
													}
												} else {//spawn with default scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (newRotation);
														newObj.transform.parent = parentObject.transform;
													} else {//static offset
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (newRotation);
														newObj.transform.parent = parentObject.transform;
													} 
												}
											} else {
												if (changeScale) {//change scale
													if (randomScale) {//random Scale
														if (dynamicOffset) {//dynamic offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (rotation);
															newObj.transform.parent = parentObject.transform;
														} else {//static offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (rotation);
															newObj.transform.parent = parentObject.transform;
														}
													} else {//selected scale
														if (dynamicOffset) {//dynamic offset
															Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (rotation);
															newObj.transform.parent = parentObject.transform;
														} else {//static offset
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (rotation);
															newObj.transform.parent = parentObject.transform;
														}
													}
												} else {//default scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (rotation);
														newObj.transform.parent = parentObject.transform;
													} else {//static offset
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (rotation);
														newObj.transform.parent = parentObject.transform;
													}
												}
											}
										} else {//spawn to hit object
											if (randomRotation) {
												if (changeScale) {//change scale
													if (randomScale) {//random scale
														if (dynamicOffset) {//dynamic offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (newRotation);
															newObj.transform.parent = hit.transform;
														} else {//static offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (newRotation);
															newObj.transform.parent = hit.transform;
														}
													} else {//selected scale
														if (dynamicOffset) {//dynamic offset
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (newRotation);
															newObj.transform.parent = hit.transform;
														} else {//static offset
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (newRotation);
															newObj.transform.parent = hit.transform;
														}
													}
												} else {//default scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (newRotation);
														newObj.transform.parent = hit.transform;
													} else {//static offset
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (newRotation);
														newObj.transform.parent = hit.transform;
													} 
												}
											} else {
												if (changeScale) {//change scale
													if (randomScale) {//random scale
														if (dynamicOffset) {//dynamic offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (rotation);
															newObj.transform.parent = hit.transform;
														} else {//static offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (rotation);
															newObj.transform.parent = hit.transform;
														}
													} else {//selected scale
														if (dynamicOffset) {//dynamic offset
															Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (rotation);
															newObj.transform.parent = hit.transform;
														} else {//static offset
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler (rotation);
															newObj.transform.parent = hit.transform;
														}
													}
												} else {//default scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (rotation);
														newObj.transform.parent = hit.transform;
													} else {//static offset
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (rotation);
														newObj.transform.parent = hit.transform;
													}
												}
											}
										}
									} else {//spawn without a parent
										if (randomRotation) {
											if (changeScale) {//change scale
												if (randomScale) {//random scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.localScale = newScale;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (newRotation);
													} else {//static offset
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.localScale = newScale;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (newRotation);
													}
												} else {//selected scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.localScale = scale;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (newRotation);
													} else {//static offset
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.localScale = scale;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (newRotation);
													}
												}
											} else {//default scale
												if (dynamicOffset) {//dynamic offset
													Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
													newObj.transform.rotation = hit.transform.rotation;
													newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
													newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
													newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
													newObj.transform.rotation = Quaternion.Euler (newRotation);

												} else {//static offset
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
													newObj.transform.rotation = hit.transform.rotation;
													newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
													newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
													newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
													newObj.transform.rotation = Quaternion.Euler (newRotation);
												}
											}
										} else {
											if (changeScale) {//change scale
												if (randomScale) {//random scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.localScale = newScale;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (rotation);
													} else {//static offset
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.localScale = newScale;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (rotation);
													}
												} else {//selected scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.localScale = scale;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (rotation);
													} else {//static offset
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.localScale = scale;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler (rotation);
													}
												}
											} else {//default scale
												if (dynamicOffset) {//dynamic offset
													Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
													GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
													newObj.transform.rotation = hit.transform.rotation;
													newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + newOffset.z), Space.Self);
													newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + newOffset.x), Space.Self);
													newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
													newObj.transform.rotation = Quaternion.Euler (rotation);
												} else {//static offset
													GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
													newObj.transform.rotation = hit.transform.rotation;
													newObj.transform.Translate (Vector3.right * (objectsPositions [i].z - hit.point.z + offset.z), Space.Self);
													newObj.transform.Translate (Vector3.forward * (objectsPositions [i].x - hit.point.x + offset.x), Space.Self);
													newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
													newObj.transform.rotation = Quaternion.Euler (rotation);
												}
											}
										}
									}
								} else
									Debug.Log ("No object detected, please select an object");
							}
						}
						}
					}
				} else {//holding
					if (Event.current.button == 1) {
						if (Event.current.type == EventType.MouseDown) {
							holding = true;
						}
					}
					if (Event.current.button == 1) {
						if (Event.current.type == EventType.MouseUp) {
							holding = false;
						}
					}

					if (spawnOne == false) {
						Handles.color = Color.red;
						Handles.DrawWireDisc (hit.point, hit.normal, radius);
						SceneView.RepaintAll ();
					}

					if (holding) {
							if (EditorApplication.timeSinceStartup > lastHold) {
								if (spawnOne) {//spawn one
								if (obj) {
									lastHold = (float)EditorApplication.timeSinceStartup + holdCd;
									if (isParent) {
										if (objectParent) {//spawn to a specific object
											if (randomRotation) {
												if (changeScale) {//change scale
													if (randomScale) {//random scale
														if (dynamicOffset) {//dynamic offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
															GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.parent = parentObject.transform;
														} else {//static offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.parent = parentObject.transform;
														}
													} else {//selected scale
														if (dynamicOffset) {//dynamic offset
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
															GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.parent = parentObject.transform;
														} else {//static offset
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.parent = parentObject.transform;
														}
													}
												} else {//default scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.parent = parentObject.transform;
													} else {//static offset
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.parent = parentObject.transform;
													}
												}
											} else {
												if (changeScale) {//change scale
													if (randomScale) {//random scale
														if (dynamicOffset) {//dynamic offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
															GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.parent = parentObject.transform;
														} else {//static offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.parent = parentObject.transform;
														}
													} else {//selected scale
														if (dynamicOffset) {//dynamic offset
															Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
															GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.parent = parentObject.transform;
														} else {//static offset
															GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.parent = parentObject.transform;
														}
													}
												} else {//default scale
													if (dynamicOffset) {//dynamic offset
														Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.parent = parentObject.transform;
													} else {//static offset
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.parent = parentObject.transform;
													}
												}
											}
										} else {//spawn to hit object
											if (randomRotation) {
												if (changeScale) {//change scale
													if (randomScale) {//random scale
														if (dynamicOffset) {//dynamic offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
															GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.parent = hit.transform;
														} else {//static offset
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.parent = hit.transform;
														}
													} else {//selected scale
														if (dynamicOffset) {
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
															GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.parent = hit.transform;
														} else {
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.parent = hit.transform;
														}
													}
												} else {//default scale
													if (dynamicOffset) {
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.parent = hit.transform;
													} else {
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.parent = hit.transform;
													}
												}
											} else {
												if (changeScale) {//change scale
													if (randomScale) {//random scale
														if (dynamicOffset) {
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
															GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.parent = hit.transform;
														} else {
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.parent = hit.transform;
														}
													} else {//selected scale
														if (dynamicOffset) {
															Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
															GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.parent = hit.transform;
														} else {
															GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.parent = hit.transform;
														}
													}
												} else {//default scale
													if (dynamicOffset) {
														Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.parent = hit.transform;
													} else {
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.parent = hit.transform;
													}
												}
											}
										}
									} else {//spawn without a parent
										if (randomRotation) {
											if (changeScale) {//change scale
												if (randomScale) {//random scale
													if (dynamicOffset) {
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.localScale = newScale;
													} else {
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.localScale = newScale;
													}
												} else {//selected scale
													if (dynamicOffset) {
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.localScale = scale;
													} else {
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation)) as GameObject;
														newObj.transform.localScale = scale;
													}
												}
											} else {//default scale
												if (dynamicOffset) {
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
													Instantiate (obj, hit.point + newOffset, Quaternion.Euler (newRotation));
												} else {
													Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
													Instantiate (obj, hit.point + offset, Quaternion.Euler (newRotation));
												}
											}
										} else {
											if (changeScale) {//change scale
												if (randomScale) {//random scale
													if (dynamicOffset) {
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.localScale = newScale;
													} else {
														Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.localScale = newScale;
													}
												} else {//selected scale
													if (dynamicOffset) {
														Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
														GameObject newObj = Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.localScale = scale;
													} else {
														GameObject newObj = Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation)) as GameObject;
														newObj.transform.localScale = scale;
													}
												}
											} else {//default scale
												if (dynamicOffset) {
													Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
													Instantiate (obj, hit.point + newOffset, Quaternion.Euler (rotation));
												} else {
													Instantiate (obj, hit.point + offset, Quaternion.Euler (rotation));
												}
											}
										}
									}
								}else
										Debug.Log ("No object detected, please select an object");
								} else {//spawn many
									lastHold = (float)EditorApplication.timeSinceStartup + holdCd;
									int numberOfObjects = Random.Range (minObjects, maxObjects);
									Vector3[] objectsPositions = new Vector3[numberOfObjects];
									for (int i = 0; i < objectsPositions.Length; i++) {
									objectsPositions [i] = RandomPosition (radius, objectsPositions, hit.point);
										if (obj) {
										if (isParent) {
											if (objectParent) {//spawn to specific object
												if (randomRotation) {
													if (changeScale) {//change scale
														if (randomScale) {//random scale
															if (dynamicOffset) {
																Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
																Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
																Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = newScale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(newRotation);
																newObj.transform.parent = parentObject.transform;
															} else {
																Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
																Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = newScale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(newRotation);
																newObj.transform.parent = parentObject.transform;
															}
														} else {//selected scale
															if (dynamicOffset) {
																Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
																Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = scale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(newRotation);
																newObj.transform.parent = parentObject.transform;
															} else {
																Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = scale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(newRotation);
																newObj.transform.parent = parentObject.transform;
															}
														}
													} else {//default scale
														if (dynamicOffset) {
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(newRotation);
															newObj.transform.parent = parentObject.transform;
														} else {
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(newRotation);
															newObj.transform.parent = parentObject.transform;
														}
													}
												} else {
													if (changeScale) {
														if (randomScale) {//random scale
															if (dynamicOffset) {
																Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
																Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = newScale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(rotation);
																newObj.transform.parent = parentObject.transform;
															} else {
																Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = newScale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(rotation);
																newObj.transform.parent = parentObject.transform;
															}
														} else {//selected scale
															if (dynamicOffset) {
																Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = scale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(rotation);
																newObj.transform.parent = parentObject.transform;
															} else {
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = scale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(rotation);
																newObj.transform.parent = parentObject.transform;
															}
														}
													} else {//default scale
														if (dynamicOffset) {
															Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(rotation);
															newObj.transform.parent = parentObject.transform;
														} else {
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(rotation);
															newObj.transform.parent = parentObject.transform;
														}
													}
												}
											} else {//spawn to hit object
												if (randomRotation) {
													if (changeScale) {//change scale
														if (randomScale) {//random scale
															if (dynamicOffset) {
																Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
																Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
																Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = newScale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(newRotation);
																newObj.transform.parent = hit.transform;
															} else {
																Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
																Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = newScale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(newRotation);
																newObj.transform.parent = hit.transform;
															}
														} else {//selected scale
															if (dynamicOffset) {
																Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
																Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = scale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(newRotation);
																newObj.transform.parent = hit.transform;
															} else {
																Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = scale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(newRotation);
																newObj.transform.parent = hit.transform;
															}
														}
													} else {//default scale
														if (dynamicOffset) {
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(newRotation);
															newObj.transform.parent = hit.transform;
														} else {
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(newRotation);
															newObj.transform.parent = hit.transform;
														}
													}
												} else {
													if (changeScale) {//change scale
														if (randomScale) {//random scale
															if (dynamicOffset) {
																Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
																Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = newScale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(rotation);
																newObj.transform.parent = hit.transform;
															} else {
																Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = newScale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(rotation);
																newObj.transform.parent = hit.transform;
															}
														} else {//selected scale
															if (dynamicOffset) {
																Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = scale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(rotation);
																newObj.transform.parent = hit.transform;
															} else {
																GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
																newObj.transform.localScale = scale;
																newObj.transform.rotation = hit.transform.rotation;
																newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
																newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
																newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
																newObj.transform.rotation = Quaternion.Euler(rotation);
																newObj.transform.parent = hit.transform;
															}
														}
													} else {//default scale
														if (dynamicOffset) {
															Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(rotation);
															newObj.transform.parent = hit.transform;
														} else {
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(rotation);
															newObj.transform.parent = hit.transform;
														}
													}
												}
											}
										} else {//spawn without parent
											if (randomRotation) {
												if (changeScale) {//change scale
													if (randomScale) {//random scale
														if (dynamicOffset) {
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(newRotation);
														} else {
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(newRotation);
														}
													} else {//selected scale
														if (dynamicOffset) {
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(newRotation);
														} else {
															Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(newRotation);
														}
													}
												} else {//default scale
													if (dynamicOffset) {
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler(newRotation);
													} else {
														Vector3 newRotation = new Vector3 (Random.Range (minRotation.x, maxRotation.x), Random.Range (minRotation.y, maxRotation.y), Random.Range (minRotation.z, maxRotation.z));
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler(newRotation);
													}
												}
											} else {
												if (changeScale) {//change scale
													if (randomScale) {//random scale
														if (dynamicOffset) {
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															Vector3 newOffset = new Vector3 (offset.x * newScale.x, offset.y * newScale.y, offset.z * newScale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(rotation);
														} else {
															Vector3 newScale = new Vector3 (Random.Range (minScale.x, maxScale.x), Random.Range (minScale.y, maxScale.y), Random.Range (minScale.z, maxScale.z));
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = newScale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(rotation);
														}
													} else {//selected scale
														if (dynamicOffset) {
															Vector3 newOffset = new Vector3 (offset.x * scale.x, offset.y * scale.y, offset.z * scale.z);
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(rotation);
														} else {
															GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
															newObj.transform.localScale = scale;
															newObj.transform.rotation = hit.transform.rotation;
															newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
															newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
															newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
															newObj.transform.rotation = Quaternion.Euler(rotation);
														}
													}
												} else {//default scale
													if (dynamicOffset) {
														Vector3 newOffset = new Vector3 (offset.x * obj.transform.localScale.x, offset.y * obj.transform.localScale.y, offset.z * obj.transform.localScale.z);
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + newOffset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + newOffset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * newOffset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler(rotation);
													} else {
														GameObject newObj = Instantiate (obj, hit.point, Quaternion.identity) as GameObject;
														newObj.transform.rotation = hit.transform.rotation;
														newObj.transform.Translate (Vector3.right * (objectsPositions[i].z - hit.point.z + offset.z), Space.Self);
														newObj.transform.Translate (Vector3.forward * (objectsPositions[i].x - hit.point.x + offset.x), Space.Self);
														newObj.transform.Translate (Vector3.up * offset.y, Space.Self);
														newObj.transform.rotation = Quaternion.Euler(rotation);
													}
												}
											}
										}
										} else
											Debug.Log ("No object detected, please select an object");
										}
								}
							}
					}
				}
			}
		}
	}

	//Add random position
	Vector3 RandomPosition(float withinRadius, Vector3[] array, Vector3 currentPosition){
		Vector3 circleUnits = Random.insideUnitCircle * withinRadius;
		Vector3 newPos = new Vector3 (currentPosition.x + circleUnits.x, currentPosition.y, currentPosition.z + circleUnits.y);
		bool isDublicate = true;
		while (isDublicate) {
			bool check = false;
			foreach (Vector3 position in array) {
				if (position == newPos) {
					check = true;
				}
			}
			if (check) {
				circleUnits = Random.insideUnitCircle * withinRadius;
				newPos = new Vector3 (currentPosition.x + circleUnits.x, currentPosition.y, currentPosition.z + circleUnits.y);
			} else {
				isDublicate = false;
			}
		}
		return newPos;
	}

	void OnEnable(){
		SceneView.onSceneGUIDelegate += OnSceneGUI;
		skin1 = (GUISkin) AssetDatabase.LoadAssetAtPath ("Assets/EasyPaint/Textures/Skin1.guiskin", typeof(GUISkin));
		skin2 = (GUISkin) AssetDatabase.LoadAssetAtPath ("Assets/EasyPaint/Textures/Skin2.guiskin", typeof(GUISkin));
	}
	void OnDisable(){
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
	}

}