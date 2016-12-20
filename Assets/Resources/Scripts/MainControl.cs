using UnityEngine;
using System.Collections;

public class MainControl : MonoBehaviour {

	private const float CAMERA_MOVE_SPEED = 0.0001f;
	private const float CAMERA_ROTATE_SPEED = 0.005f;

	public Camera mainCamera;

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

	public void RightHandGrab(GameObject grabbed) {
		if (active != null)
			active.GetComponent<Renderer> ().materials[1].shader = Shader.Find ("Standard");
		active = grabbed;
		active.GetComponent<Renderer> ().materials[1].shader = Shader.Find ("Outlined/Silhouette Only"); // highlight
	}

	public void RightHandRelease() {
	}

}
