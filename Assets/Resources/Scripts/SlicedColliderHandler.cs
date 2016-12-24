using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Sliceable))]
public class SlicedColliderHandler : AbstractSliceHandler {
	public override void handleSlice (GameObject[] results) {
		// here I need to recalculate mesh collider to fit sliced object
		for (int i = 0; i < results.Length; ++ i) {
			MeshHelper.ApplyMeshCollider (results[i]);
		}
	}
}
