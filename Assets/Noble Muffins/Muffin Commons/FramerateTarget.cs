using UnityEngine;

public class FramerateTarget : MonoBehaviour
{
	public int target = 60;

	// Use this for initialization
	void Start () {
		Application.targetFrameRate = target;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
