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

	public Camera mainCamera;
	public Image PointedPos;

	// prefabs
	public Transform cube;
	public Transform sphere;

	// current active object
	private GameObject active = null;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void LeftHandMove(Leap.Vector delta) {
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
		bool horizontal = Mathf.Abs (dir.x) > Mathf.Abs (dir.y);
		if (horizontal) {
			if (dir.x > 0)
				Instantiate (cube, new Vector3 (0, 2f, 0f), Quaternion.identity);
			else
				Instantiate (sphere, new Vector3 (0, 2f, 0f), Quaternion.identity);
		}
		
	}

	public void Tap() {
		Debug.Log ("Tap");
	}

	public void RightHandPinch(float pinchStrength) {
		if (active != null) {
			if (pinchStrength > 0.9 && active.transform.localScale.magnitude > OBJECT_THRINK_LIMIT)
				// shrink
				active.transform.localScale -= Vector3.one * OBJECT_SCALING_SPEED;
			else if (pinchStrength < 0.5 && active.transform.localScale.magnitude < OBJECT_ENLARGE_LIMIT)
				// enlarge
				active.transform.localScale += Vector3.one * OBJECT_SCALING_SPEED;
		}
	}

	public void RightHandGrab(GameObject grabbed) {
		if (active != null)
			active.GetComponent<Renderer> ().materials[1].shader = Shader.Find ("Standard");
		active = grabbed;
		active.GetComponent<Renderer> ().materials[1].shader = Shader.Find ("Outlined/Silhouette Only"); // highlight
	}

	public void RightHandRelease() {
	}

	public void RightHandSlice(Plane plane) {
		Debug.Log ("Slice");
	}

	public void RightHandPoint(float x, float y) {
		PointedPos.enabled = true;
		PointedPos.rectTransform.localPosition = new Vector3 (x, y);
	}

	public void RightHandNotPoint() {
		//PointedPos.enabled = false;
	}

}
