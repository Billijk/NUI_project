using UnityEngine;
using System.Collections;

public class ObjectController : MonoBehaviour {

	public float velocity;
	
	private ArrayList touchedObjects;
	private Vector3 destination;

	// Use this for initialization
	void Start () {
		touchedObjects = new ArrayList ();
		init();
	}
	
	// Update is called once per frame
	void Update () {
		GetComponent<Rigidbody>().velocity = (destination - transform.position) * Time.deltaTime * velocity;
	}

	public void init() {
		touchedObjects.Clear();
		destination = transform.position;
	}

	public GameObject[] getTouchedObjects() {
		GameObject[] res = new GameObject[touchedObjects.Count];
		for (int i = 0; i < touchedObjects.Count; ++ i) {
			res [i] = (GameObject) touchedObjects [i];
		}
		return res;
	}

	public bool combinable() {
		return touchedObjects.Count != 0;
	}

	public void moveTo(Vector3 destination) {
		this.destination = destination;
	}

	public void OnTriggerEnter(Collider other) {
		Debug.Log("Collide");
		if (other.gameObject.layer != 8)
			touchedObjects.Add (other.gameObject);
	}

	public void OnTriggerExit(Collider other) {
		if (other.gameObject.layer != 8)
			touchedObjects.Remove (other.gameObject);
	}
}
