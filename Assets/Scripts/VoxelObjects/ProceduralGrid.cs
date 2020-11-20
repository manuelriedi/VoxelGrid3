using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralGrid : MonoBehaviour {
    
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public float cellSize = 2;
    public int gridSize;
    public Vector3 gridOffset;

    private float vertexOffset;
    private Dictionary<int, GameObject> occupiedFields = new Dictionary<int, GameObject>();


    void Awake() {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start() {
        initFields();
        vertexOffset = cellSize * 0.5f;
        MakeProceduralGrid();
        //MakeContiguousProceduralGrid();
        UpdateMesh();
    }

    private void initFields()
    {
        for (int i = 0; i < (gridSize * gridSize); i++)
        {
            occupiedFields.Add(i, null);
        }
    }

    private Vector3 GetRasterPosition(int i) {
        Vector3 pos;
        pos = vertices[i * 4]; // i*4 for translation to cube verices number
        pos.x += vertexOffset;
        pos.y = vertexOffset * cellSize; // TODO: adjust y-axis
        pos.z += vertexOffset;
        return pos;
    }

    public Vector3 TransToRasterPosition(ref Carriable tetromino) {
        int cellId = 0;

        Transform[] allChildren = tetromino.GetComponentsInChildren<Transform>();
        List<Vector3> temp_Positions = new List<Vector3>();
        Dictionary<int, GameObject> temp_FieldIdToCube = new Dictionary<int, GameObject>();
        foreach (Transform child in allChildren)
        {
            var p = child.gameObject.transform.position;

            for (int i = 0; i <= vertices.Length - 4; i += 4)
            {
                if (p.x >= vertices[i].x && p.x <= vertices[i + 3].x && p.z >= vertices[i].z && p.z <= vertices[i + 3].z)
                {
                    temp_Positions.Add(GetRasterPosition(cellId));
                    temp_FieldIdToCube[cellId] = child.gameObject;
                }
                cellId++;
            }
            cellId = 0;
        }

        if(temp_Positions.Count == allChildren.Length)
        {
            foreach (var id in temp_FieldIdToCube.Keys)
            {
                occupiedFields[id] = temp_FieldIdToCube[id];
            }

            ////Print current field occupations
            int count = 0;
            foreach (var item in occupiedFields)
            {

                if (item.Value != null)
                {
                    Debug.Log("Field: " + item.Key + " occupied with: " + item.Value.name);
                    count++;
                }
            }
            Debug.Log("Total occupied fields: " + count + " of " + occupiedFields.Count);

            return temp_Positions[0];
        }
        else
        {
            Debug.Log("(Part of) Tetromino was outside grid");
            return new Vector3((cellSize * gridSize + 1), cellSize, cellSize); //TODO: Define standart position for tetrominos
        }        
    }

 
    private void MakeProceduralGrid() {
        //set array size
        vertices = new Vector3[gridSize * gridSize * 4];
        triangles = new int[gridSize * gridSize * 6];

        //set tracker integers
        int v = 0;
        int t = 0;

        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                Vector3 cellOffset = new Vector3(x * cellSize, 0, y * cellSize); //Offset of one cell

                vertices[v + 0] = new Vector3(-vertexOffset, 0+y, -vertexOffset) + cellOffset + gridOffset;
                vertices[v + 1] = new Vector3(-vertexOffset, 0+y, vertexOffset) + cellOffset + gridOffset;
                vertices[v + 2] = new Vector3(vertexOffset, 0+y, -vertexOffset) + cellOffset + gridOffset;
                vertices[v + 3] = new Vector3(vertexOffset, 0+y, vertexOffset) + cellOffset + gridOffset;

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

    private void MakeContiguousProceduralGrid()
    {
        //set array size
        vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];
        triangles = new int[gridSize * gridSize * 6];

        //set tracker integers
        int v = 0;
        int t = 0;

        //set vertex grid
        for (int x = 0; x <= gridSize; x++)
        {
            for (int y = 0; y <= gridSize; y++)
            {
                vertices[v] = new Vector3((x * cellSize) - vertexOffset, 0, (y * cellSize) - vertexOffset);
                v++;
            }
        }

        //reset vertex tracker
        v = 0;

        //setting each cells triangles
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                triangles[t] = v;
                triangles[t + 1] = triangles[t + 4] = v + 1;
                triangles[t + 2] = triangles[t + 3] = v+ (gridSize + 1);
                triangles[t + 5] = v + (gridSize + 1) + 1;
                v++;
                t += 6;
            }
            v++;
        }
    }

    void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
