using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralGrid : MonoBehaviour
{
    Mesh mesh;
    public GameObject cube;
    private int cubeFallDownAxis = 0;
    private Vector3 cubePosition = new Vector3();
    
    Vector3[] vertices;
    int[] triangles;

    //grid settings
    public float cellSize = 2;
    public int gridSize;
    public Vector3 gridOffset; //Offset of entire grid

    private float vertexOffset;

    
    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    // Start is called before the first frame update
    void Start()
    {
        //set array sizes
        vertexOffset = cellSize * 0.5f;
        
        //Set cube size to specific cellSize
        cube.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
        MakeDiscreteProceduralGrid();
        UpdateMesh();
        UpdateCubePosition(0);
    }

    // Update is called once per frame
    private void Update()
    {
        //Cube Position
        if (Input.GetKey(KeyCode.Alpha0))
        {
            UpdateCubePosition(0);
        }else if(Input.GetKey(KeyCode.Alpha1))
        {
            UpdateCubePosition(1);
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            UpdateCubePosition(2);
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            UpdateCubePosition(3);
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            UpdateCubePosition(4);
        }
        else if (Input.GetKey(KeyCode.Alpha5))
        {
            UpdateCubePosition(5);
        }
        else if (Input.GetKey(KeyCode.Alpha6))
        {
            UpdateCubePosition(6);
        }
        else if (Input.GetKey(KeyCode.Alpha7))
        {
            UpdateCubePosition(7);
        }
        else if (Input.GetKey(KeyCode.Alpha8))
        {  
            UpdateCubePosition(8);
        }
    }

    private void UpdateCubePosition(int i)
    {
        //Set cube on a specifig position
        cubePosition = vertices[i*4]; // i*4 for translation to cube verices number
        cubePosition.x += vertexOffset;
        cubeFallDownAxis = 0; // TODO: Set fallDownAxix as time based here.
        cubePosition.y = vertexOffset + (cellSize * cubeFallDownAxis); 
        cubePosition.z += vertexOffset;
        cube.transform.position = cubePosition;
    }

    private void CheckCurrentCubePosition()
    {
        if((cubePosition.x >= -1.0f && cubePosition.z >= -1.0f) && (cubePosition.x <= 1.0f && cubePosition.z >= 1.0f))
        {
            Debug.Log("Detect cell 0");
        }
    }

    private void MakeDiscreteProceduralGrid()
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

                //populate the vertices and triangles arrays
                vertices[v + 0] = new Vector3(-vertexOffset, 0, -vertexOffset) + cellOffset + gridOffset;
                vertices[v + 1] = new Vector3(-vertexOffset, 0, vertexOffset) + cellOffset + gridOffset;
                vertices[v + 2] = new Vector3(vertexOffset, 0, -vertexOffset) + cellOffset + gridOffset;
                vertices[v + 3] = new Vector3(vertexOffset, 0, vertexOffset) + cellOffset + gridOffset;

                // TODO: Use this coordinates to detect mouspostion and then set the cube to the corresponding cell with UpdateCubePosition(int i) 
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