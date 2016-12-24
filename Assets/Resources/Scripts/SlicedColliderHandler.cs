using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Sliceable))]
public class SlicedColliderHandler : AbstractSliceHandler {
	public override void handleSlice (GameObject[] results) {
		// here I need to recalculate mesh collider to fit sliced object
		foreach (GameObject obj in results) {
			MeshHelper.ApplyMeshCollider (obj);
		}
	}
}
