using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainControl : MonoBehaviour {

	private const float CAMERA_MOVE_SPEED = 0.0001f;
	private const float CAMERA_ROTATE_SPEED = 0.005f;
	private const float CAMERA_CLOSE_LIMIT = 1f;
	private const float CAMERA_FAR_LIMIT = 10f;
	private const float OBJECT_SCALING_SPEED = 0.005f;
	private const float OBJECT_THRINK_LIMIT = 0.5f;
	private const float OBJECT_ENLARGE_LIMIT = 4f;
	private const float GRAB_SELECT_DISTANCE = 0.5f;
	private const float DRAG_THRESHOLD = 0.2f;
	private const float DRAG_STRENGTH = 50f;
	private const int HAND_LAYER_INDEX = 8;
	private Vector3 INITIAL_CAMERA_POS = new Vector3 (0, 2.23f, -3.18f);
	private Vector3 INITIAL_CAMERA_ROT = new Vector3 (35f, 0, 0);

	private bool isEnable = true;

	public Camera mainCamera;
	public Image PointedPos;

	// prefabs
	public Transform cube;
	public Transform sphere;

	// slider
	public ScrollControl sliderPanel;

	// current active object
	private GameObject active = null;

	// combine timer
	private float timer = float.PositiveInfinity;
	private float swipeTimer = 0;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		timer -= Time.deltaTime;
		swipeTimer -= Time.deltaTime;
		if (timer < 0f) {
			CombineObjects ();
			timer = float.PositiveInfinity;
		}

		if (Input.GetKey (KeyCode.Delete)) {
			if (active != null) {
				active.GetComponent<ObjectController> ().notifyDestroy ();
				Destroy (active);
			}
		}

		if (Input.GetKey (KeyCode.R)) {
			mainCamera.transform.position = INITIAL_CAMERA_POS;
			mainCamera.transform.eulerAngles = INITIAL_CAMERA_ROT;
			transform.position = mainCamera.transform.position + mainCamera.transform.forward * 2 + Vector3.down;
			transform.eulerAngles = Vector3.zero;
		}
	}

	// combine active object with touched ones
	private void CombineObjects() {
		Debug.Log ("Combine!");
		GameObject[] touchedOnes = active.GetComponent<ObjectController> ().getTouchedObjects ();
//		for (int i = 0; i < touchedOnes.Length; ++i) {
//			touchedOnes [i].transform.SetParent (active.transform);
//			touchedOnes [i].GetComponent<ObjectController>().init (active);
//		}
		// calculate new center point
		Vector3 newCenter = active.transform.position;
		foreach (GameObject obj in touchedOnes)
			newCenter += obj.transform.position;
		newCenter /= (touchedOnes.Length + 1);
		// translate objects to origin respectively
		active.transform.position -= newCenter;
		foreach(GameObject obj in touchedOnes)
			obj.transform.position -= newCenter;
		// combine mesh
		CombineInstance[] combine = new CombineInstance[touchedOnes.Length + 1];
		combine [0].mesh = active.GetComponent<MeshFilter> ().sharedMesh;
		combine [0].transform = active.GetComponent<MeshFilter> ().transform.localToWorldMatrix;
		for (int i = 0; i < touchedOnes.Length; ++i) {
			combine [i + 1].mesh = touchedOnes [i].GetComponent<MeshFilter> ().sharedMesh;
			combine [i + 1].transform = touchedOnes [i].GetComponent<MeshFilter> ().transform.localToWorldMatrix;
			Destroy (touchedOnes [i]);
		}
		active.transform.localScale = Vector3.one;
		active.GetComponent<ObjectController>().init();
		active.GetComponent<MeshFilter> ().mesh = new Mesh ();
		active.GetComponent<MeshFilter> ().mesh.CombineMeshes (combine);
		active.GetComponent<MeshFilter> ().mesh.Optimize ();
		MeshHelper.ApplyMeshCollider (active);
		// translate back
		active.GetComponent<ObjectController>().moveTo(newCenter);
	}

	public void LeftHandMove(Leap.Vector delta) {
		if(!isEnable) {
			return;
		}
		mainCamera.transform.RotateAround (Vector3.zero, Vector3.up, delta.x * CAMERA_ROTATE_SPEED);
		transform.RotateAround(Vector3.zero, Vector3.up, delta.x * CAMERA_ROTATE_SPEED);
		if (mainCamera.transform.eulerAngles.x > 5 && mainCamera.transform.eulerAngles.x < 85) {
			mainCamera.transform.RotateAround (Vector3.zero, mainCamera.transform.right, -delta.z * CAMERA_ROTATE_SPEED);
			transform.RotateAround (Vector3.zero, mainCamera.transform.right, -delta.z * CAMERA_ROTATE_SPEED);
			// if over-rotate then rotate back
			if (mainCamera.transform.eulerAngles.x <= 5 || mainCamera.transform.eulerAngles.x >= 85) {
				mainCamera.transform.RotateAround (Vector3.zero, mainCamera.transform.right, delta.z * CAMERA_ROTATE_SPEED);
				transform.RotateAround (Vector3.zero, mainCamera.transform.right, delta.z * CAMERA_ROTATE_SPEED);
			}

		}
		mainCamera.transform.position += (mainCamera.transform.forward * - delta.y * CAMERA_MOVE_SPEED);
		if (mainCamera.transform.position.magnitude < CAMERA_CLOSE_LIMIT || mainCamera.transform.position.magnitude > CAMERA_FAR_LIMIT)
			mainCamera.transform.position += mainCamera.transform.forward * delta.y * CAMERA_MOVE_SPEED;
		transform.position = mainCamera.transform.position + mainCamera.transform.forward * 2 + Vector3.down;
	}

	public void Swipe(Leap.Vector dir, float speed) {
		if(swipeTimer > 0) {
			return;
		}
		bool horizontal = Mathf.Abs (dir.x) > Mathf.Abs (dir.y);
		if (horizontal) {
			if(dir.x > 0) {
				sliderPanel.swipeSlider(0);
			} else {
				sliderPanel.swipeSlider(1);
			}
			swipeTimer = 1.0f;
		}
		if(sliderPanel.getSliderStatus() != 1) {
			isEnable = false;
		} else {
			isEnable = true;
		}
	}

	public void Tap() {
		if(!isEnable) {
			return;
		}
		Debug.Log ("Tap");
	}

	public void RightHandPinch(float pinchStrength) {
		if(!isEnable) {
			return;
		}
		if (active != null) {
			if (pinchStrength > 0.9 && active.transform.localScale.magnitude > OBJECT_THRINK_LIMIT)
				// shrink
				active.transform.localScale -= Vector3.one * OBJECT_SCALING_SPEED;
			else if (pinchStrength < 0.5 && active.transform.localScale.magnitude < OBJECT_ENLARGE_LIMIT)
				// enlarge
				active.transform.localScale += Vector3.one * OBJECT_SCALING_SPEED;
		}
	}

	public void RightHandGrab(Vector2 pos2d, Vector3 pos3d) {
		if(!isEnable) {
			if(sliderPanel.getSliderStatus() == 0 && sliderPanel.getClick(pos2d, pos3d) != -1) {
				Swipe(new Leap.Vector(-3, -1, 0), 1.0f);
			}
		} else {
			timer = float.PositiveInfinity;	// cancel timer
			Collider grabbed = null;
			int layerMask = ~(1 << HAND_LAYER_INDEX);
			Collider[] closeThings = Physics.OverlapSphere (pos3d, GRAB_SELECT_DISTANCE, layerMask);
			float shortestDist = GRAB_SELECT_DISTANCE;
			for (int i = 0; i < closeThings.Length; ++i) {
				Vector3 dist = pos3d - closeThings [i].transform.position;
				if (dist.magnitude < shortestDist) {
					grabbed = closeThings [i];
					shortestDist = dist.magnitude;
				}
			}
			
			if (grabbed != null) {
				if (active != null && active != grabbed.gameObject) {
					active.GetComponent<Renderer> ().materials [1].shader = Shader.Find ("Standard (Vertex Color)");
				} 
				active = grabbed.gameObject;
				//active = grabbed.gameObject.GetComponent<ObjectController>().getParent();
				active.GetComponent<Renderer> ().materials[1].shader = Shader.Find ("Outlined/Silhouette Only"); // highlight	
//				foreach (Renderer renderer in active.GetComponentsInChildren<Renderer>()) {
//					renderer.materials [1].shader = Shader.Find ("Outlined/Silhouette Only");
//				}
			}

		}
	}

	public void RightHandDrag(Vector3 pos3d, Vector3 v) {
		if (isEnable && active != null) {
			active.GetComponent<ObjectController>().moveTo(pos3d);
		}
	}
		
	public void RightHandRelease() {
		// if collider intersects, start a timer: 
		// if not grab again in 2s: combine objects
		if(active != null && active.GetComponent<ObjectController>().combinable()) {
			timer = 2f;
			Debug.Log("Combine?");
		}
	}

	public void RightHandSlice(Plane plane) {
		if (active != null) {
			Debug.Log ("Cut!");
			active.GetComponent<Renderer> ().materials [1].shader = Shader.Find ("Standard (Vertex Color)");
			TurboSlice.InfillConfiguration[] infillers = new TurboSlice.InfillConfiguration[2];
			for (int i = 0; i < 2; ++i) {
				infillers [i] = new TurboSlice.InfillConfiguration ();
				infillers [i].material = active.GetComponent<Renderer> ().materials [i];
				infillers [i].regionForInfill = new Rect (0, 0, 1, 1);
			}
			active.GetComponent<Sliceable> ().infillers = infillers;
			TurboSlice.instance.splitByPlane (active, new Vector4 (plane.normal.x, plane.normal.y, plane.normal.z, plane.distance), true);
			active = null;
		}
	}

	public void RightHandPoint(Vector2 target) {
		if(!isEnable) {
			if(sliderPanel.getSliderStatus() == 0) {
				sliderPanel.selectGeometry(new Vector3(target.x, target.y));
			} else if(sliderPanel.getSliderStatus() == 2) {
				Color color = 
					sliderPanel.setSurfaceColor(new Vector3(target.x, target.y));
				if(active != null) {
					Mesh m = active.GetComponent<MeshFilter> ().sharedMesh;
					Color[] colors = new Color[m.vertexCount];
					for (int i = 0; i < m.vertexCount; ++i)
						colors [i] = color;
					m.colors = colors;
//					active.GetComponent<MeshRenderer> ().material.color = color;
//					MeshRenderer[] activeRenderers = active.GetComponentsInChildren<MeshRenderer>();
//					foreach (MeshRenderer renderer in activeRenderers)
//						renderer.material.color = color;
				}
			}
		}
		//PointedPos.enabled = true;
		//PointedPos.rectTransform.localPosition = new Vector3(target.x, target.y);
	}

	public void RightHandNotPoint() {
		if(!isEnable) {
			return;
		}
		//PointedPos.enabled = false;
	}

	public void RightPalmMove(float delta) {
		if(!isEnable) {
			return;
		}
		if(active != null) {
			if(delta > 30.0f) {
				active.transform.localScale = new Vector3(
					active.transform.localScale.x,
					active.transform.localScale.y * (1 + OBJECT_SCALING_SPEED),
					active.transform.localScale.z
				);
			} else if(delta < -30.0f) {
				active.transform.localScale = new Vector3(
					active.transform.localScale.x,
					active.transform.localScale.y * (1 - OBJECT_SCALING_SPEED),
					active.transform.localScale.z
				);
			}
		}
	}
		
}
