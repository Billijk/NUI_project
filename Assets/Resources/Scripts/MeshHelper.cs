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
		return resultMesh;
	}

	public static GameObject ApplyMeshCollider(GameObject gameObject) {
		// first destroy automatically generated collider
		Collider[] colliders = gameObject.GetComponents<Collider> ();
		foreach(Collider c in colliders) {
			GameObject.Destroy (c);
		}
		// replace with a mesh collider
		gameObject.AddComponent<MeshCollider> ();
		gameObject.GetComponent<MeshCollider> ().sharedMesh = MeshHelper.Simplify(gameObject.GetComponent<MeshFilter> ().mesh, 255);
		gameObject.GetComponent<MeshCollider> ().convex = true;
		gameObject.GetComponent<MeshCollider> ().isTrigger = true;
		return gameObject;
	}
}