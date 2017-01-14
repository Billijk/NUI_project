using UnityEngine;
using System.Collections;

public class ObjectController : MonoBehaviour {

	public float velocity;
	
	private ArrayList touchedObjects;
	private Vector3 destination;
	//private Vector3 relativeVec;
	//private GameObject parent;

	// Use this for initialization
	void Start () {
		touchedObjects = new ArrayList ();
		MeshHelper.ApplyMeshCollider (gameObject);
		init();
	}
	
	// Update is called once per frame
	void Update () {
		GetComponent<Rigidbody>().velocity = (destination - transform.localPosition) * Time.deltaTime * velocity;
		GetComponent<MeshCollider> ().convex = true;
		GetComponent<MeshCollider> ().isTrigger = true;
//		if (parent != gameObject)
//			destination = relativeVec;
//			//destination += (relativeVec - transform.localPosition);
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

//	public GameObject getParent() {
//		return parent;
//	}

	public bool combinable() {
		return touchedObjects.Count != 0;
	}

	public void moveTo(Vector3 destination) {
		this.destination = destination;
	}

	public void notifyDestroy() {
		foreach (GameObject other in touchedObjects) {
			other.GetComponent<ObjectController> ().OnTriggerExit (GetComponent<MeshCollider>());
		}
	}

	public void OnTriggerEnter(Collider other) {
		Debug.Log("Collide");
		if (other.gameObject.layer != 8)
			touchedObjects.Add (other.gameObject);
			//touchedObjects.Add (other.gameObject.GetComponent<ObjectController>().parent);
	}

	public void OnTriggerExit(Collider other) {
		if (other.gameObject.layer != 8)
			touchedObjects.Remove (other.gameObject);
			//touchedObjects.Remove (other.gameObject.GetComponent<ObjectController>().parent);
	}
}
