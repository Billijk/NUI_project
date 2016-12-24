using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollControl : MonoBehaviour {
	private int sliderStatus = 0;
	private int onGeometryIcon = -1;
	private bool needMove = false;
	private float moveSpeed = 0.0f;
	private float targetValue = 0.5f;

	public Scrollbar scrollBar;
	public Image pointer;

	public Transform cube;
	public Transform sphere;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0)) {
			swipeSlider(0);
		} else if(Input.GetMouseButtonDown(1)) {
			swipeSlider(1);
		}
		if(needMove) {
			if (Mathf.Abs(scrollBar.value - targetValue) < 0.001f) {
				scrollBar.value = targetValue;
				needMove = false;
				return;
			}
			scrollBar.value = Mathf.SmoothDamp(
				scrollBar.value, targetValue, ref moveSpeed, 0.2f
			);
		}
	}

	public void swipeSlider(int dir) {
		if(dir == 0) {
			if(sliderStatus == 1) {
				sliderStatus = 0;
				targetValue = 0.0f;
				needMove = true;
				pointer.enabled = true;
			} else if(sliderStatus == 2) {
				sliderStatus = 1;
				targetValue = 0.5f;
				needMove = true;
				pointer.enabled = false;
			}
		} else if(dir == 1) {
			if(sliderStatus == 1) {
				sliderStatus = 2;
				targetValue = 1.0f;
				needMove = true;
				pointer.enabled = true;
			} else if(sliderStatus == 0) {
				sliderStatus = 1;
				targetValue = 0.5f;
				needMove = true;
				pointer.enabled = false;
			}
		}
	}

	public void movePointer(Vector3 target) {
		if(pointer.enabled) {
			pointer.rectTransform.localPosition = target;
			float x = target.x;
			float y = target.y;

			if(x <= -50 && x >= -250 && y <= 0 && y >= -200) {
				onGeometryIcon = 1;
			} else if(x >= 50 && x <= 250 && y <= 0 && y >= -200) {
				onGeometryIcon = 2;
			} else if(x >= -100 && x <= 100 && y >= 0 && y <= 200) {
				onGeometryIcon = 0;
			} else {
				onGeometryIcon = -1;
			}
		}
		Debug.Log(onGeometryIcon);
	}

	public int getClick(Vector2 pos2d, Vector3 pos3d) {
		if(onGeometryIcon == 1) {
			Instantiate(cube, pos3d, Quaternion.identity);
		} else if(onGeometryIcon == 2) {
			Instantiate(sphere, pos3d, Quaternion.identity);
		} else if(onGeometryIcon == 0) {
			Debug.Log("Prism");
		}

		return onGeometryIcon;
	}

	public int getSliderStatus() {
		return sliderStatus;
	}
}
