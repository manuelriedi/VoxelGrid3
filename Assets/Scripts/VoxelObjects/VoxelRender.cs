using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelRender : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;

    public float scale = 1f;
    public int yAxis = 1;

    float adjScale;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;

        adjScale = scale * 0.5f;
    }

    void Start()
    {
        GenerateVoxelMesh(new VoxelData());
        UpdateMesh();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void GenerateVoxelMesh(VoxelData data)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int z = 0; z < data.Depth; z++)
        {
            for (int x = 0; x < data.Width; x++)
            {
                if(data.GetCell (x,z) == 0)
                {
                    continue; //Aus diesem Loop springen und zum äusseren gehen...
                }

                //With neighbor detection
                //MakeCube(adjScale, new Vector3((float)x * scale, 0, (float)z * scale), x, z, data);
                
                //Wothout neighbor detection
                MakeCube(adjScale, new Vector3((float)x * scale, yAxis, (float)z * scale));
            }
        }
    }

    //With neighbor detection
    //void MakeCube(float cubeScale, Vector3 cubePos, int x, int z, VoxelData data)
    //{
    //    for (int i = 0; i < 6; i++)  //Weil Cube 6 Seiten
    //    {
    //        if (data.GetNeighbor(x, z, (Direction)i) == 0)
    //        {
    //            MakeFace((Direction)i, cubeScale, cubePos); //i=für welches Face wir handeln. 
    //        }

    //    }
    //}

    //Without neighbor detection
    void MakeCube(float cubeScale, Vector3 cubePos)
    {
        Debug.Log("CubPos: "+ cubePos );

        for (int i = 0; i < 6; i++)  //Weil Cube 6 Seiten
        {           
            MakeFace((Direction)i, cubeScale, cubePos); //i = für welches Face wir handeln.         
        }
    }



    void MakeFace(Direction dir, float faceScale, Vector3 facePos)
    {
        vertices.AddRange(CubeMashData.faceVertices(dir, faceScale, facePos));
        int vCount = vertices.Count;

        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 1);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4 + 3);
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}
