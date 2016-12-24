using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollControl : MonoBehaviour {
	private float moveSpeed = 0.0f;
	private bool needMove = false;
	private float targetValue = 0.5f;
	public Scrollbar scrollBar;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0)) {
			/*
			if(!isOpen) {
				scrollBar.value = Mathf.SmoothDamp(
					0, 1, ref moveSpeed, 1f
				);
			} else {
				scrollBar.value = Mathf.SmoothDamp(
					1, 0, ref moveSpeed, 1f
				);
			}
			isOpen = !isOpen;
			*/
			targetValue = 0.5f - targetValue;
			needMove = true;
		} else if(Input.GetMouseButtonDown(1)) {
			targetValue = 1.5f - targetValue;
			needMove = true;
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
}
