using System;
using UnityEngine;
using nobnak.Geometry;

public class MeshHelper {
	public static Mesh Simplify(Mesh mesh, int target) {
		var simp = new Simplification (mesh.vertices, mesh.triangles);
		if (simp.faceDb.FaceCount * 3 <= target)
			return mesh;
		while (target < simp.faceDb.FaceCount * 3) {
			var edge = simp.costs.RemoveFront ();
			simp.CollapseEdge (edge);
		}
		Vector3[] outVertices;
		int[] outTriangles;
		simp.ToMesh (out outVertices, out outTriangles);
		Mesh resultMesh = new Mesh ();
		resultMesh.vertices = outVertices;
		resultMesh.triangles = outTriangles;
		resultMesh.RecalculateNormals ();
		Debug.Log (resultMesh.triangles.Length);
		return resultMesh;
	}

	public static void ApplyMeshCollider(GameObject gameObject) {
		// first destroy automatically generated collider
		Collider[] colliders = gameObject.GetComponents<Collider> ();
		foreach(Collider c in colliders) {
			GameObject.Destroy (c);
		}
		// replace with a mesh collider
		MeshCollider collider = gameObject.AddComponent<MeshCollider> () as MeshCollider;
		// mesh is simplified to ensure it has less than 256 faces
		//if (gameObject.GetComponent<MeshCollider> ().sharedMesh != null) {
		collider.sharedMesh = null;
		//}
		collider.sharedMesh = 
			MeshHelper.Simplify(gameObject.GetComponent<MeshFilter> ().sharedMesh, 255);
		collider.convex = true;
		collider.isTrigger = true;
	}
}