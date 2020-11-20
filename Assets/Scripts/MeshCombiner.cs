using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshCombiner : MonoBehaviour {

    private MeshRenderer combinedRenderer;
    private MeshFilter combinedFilter;
    public Mesh combinedMesh;

    private void Start() {
        combinedRenderer = GetComponent<MeshRenderer>();
        combinedFilter = GetComponent<MeshFilter>();
        combinedMesh = combinedFilter.mesh = new Mesh();

        
        var vtxBuffer = new List<Vector3>();
        var trianglesBuffer = new List<(int[] tris, int Offset)>();

        var filters = GetComponentsInChildren<MeshFilter>().Where(mf => mf.gameObject != this.gameObject).ToList();
        combinedRenderer.materials = filters.SelectMany(f => f.GetComponent<Renderer>().materials).ToArray();
        combinedMesh.subMeshCount = filters.Sum(f => f.mesh.subMeshCount);

        foreach (var filter in filters) {
            //filter.GetComponent<Renderer>().enabled = false;

            var previousVtx = vtxBuffer.Count;
            var childMesh = filter.mesh;
            vtxBuffer.AddRange(
                from v in childMesh.vertices
                select
                    transform.InverseTransformPoint(
                        filter.transform.TransformPoint(v)
                    )
            );

            for (var i = 0; i < childMesh.subMeshCount; i++) {
                trianglesBuffer.Add((childMesh.GetTriangles(i), previousVtx));
            }
        }

        combinedMesh.SetVertices(vtxBuffer);
        var index = 0;
        foreach (var (triangles, offset) in trianglesBuffer) {
            combinedMesh.SetTriangles(triangles, index++, false, offset);
        }

        combinedMesh.RecalculateNormals();
        combinedMesh.RecalculateBounds();

    }
}
