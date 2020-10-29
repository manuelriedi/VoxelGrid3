﻿using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralGrid : MonoBehaviour
{
    Mesh mesh;
    private int fallDownAxis = 0;
    
    Vector3[] vertices;
    int[] triangles;

    public float cellSize = 2;
    public int gridSize;
    public Vector3 gridOffset;

    private float vOffset;

    
    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start()
    {
        vOffset = cellSize * 0.5f;
        MakeProceduralGrid();
        UpdateMesh();
    }

    private Vector3 GetRasterPosition(int i)
    {
        Vector3 pos;
        pos = vertices[i*4]; // i*4 for translation to cube verices number
        pos.x += vOffset;
        fallDownAxis = 0; // TODO: Set fallDownAxix as time based here.
        pos.y = vOffset + (cellSize * fallDownAxis); 
        pos.z += vOffset;
        return pos;
    }

    public Vector3 TransToRasterPosition(Vector3 p)
    {
        int cellId = 0;
        for (int i = 0; i <= vertices.Length - 4; i += 4)
        {
            if (p.x >= vertices[i].x && p.x <= vertices[i + 3].x && p.z >= vertices[i].z && p.z <= vertices[i + 3].z)
            {
                return GetRasterPosition(cellId);
            }
            cellId++;
        }

        return new Vector3(p.x, cellSize-vOffset, p.z); //Case no cell detected
    }

    private void MakeProceduralGrid()
    {
        //set array size
        vertices = new Vector3[gridSize * gridSize * 4];
        triangles = new int[gridSize * gridSize * 6];

        //set tracker integers
        int v = 0;
        int t = 0;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 cellOffset = new Vector3(x * cellSize, 0, y * cellSize); //Offset of one cell

                vertices[v + 0] = new Vector3(-vOffset, 0, -vOffset) + cellOffset + gridOffset;
                vertices[v + 1] = new Vector3(-vOffset, 0, vOffset) + cellOffset + gridOffset;
                vertices[v + 2] = new Vector3(vOffset, 0, -vOffset) + cellOffset + gridOffset;
                vertices[v + 3] = new Vector3(vOffset, 0, vOffset) + cellOffset + gridOffset;

                //Debug.Log("Vertices" + (v + 0) + vertices[v + 0]);
                //Debug.Log("Vertices" + (v + 1) + vertices[v + 1]);
                //Debug.Log("Vertices" + (v + 2) + vertices[v + 2]);
                //Debug.Log("Vertices" + (v + 3) + vertices[v + 3]);

                triangles[t] = v;
                triangles[t + 1] = triangles[t + 4] = v + 1;
                triangles[t + 2] = triangles[t + 3] = v + 2;
                triangles[t + 5] = v + 3;

                v += 4;
                t += 6;
            }

        }
    }


    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }


}