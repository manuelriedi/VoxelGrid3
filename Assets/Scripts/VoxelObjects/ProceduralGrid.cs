using System.Collections.Generic;
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
    //private List<int> occupiedFields = new List<int>();
    private Dictionary<int, GameObject> occupiedFields = new Dictionary<int, GameObject>();


    void Awake() {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start() {
        vertexOffset = cellSize * 0.5f;
        MakeProceduralGrid();
        //MakeContiguousProceduralGrid();
        UpdateMesh();
    }

    private Vector3 GetRasterPosition(int i) {
        Vector3 pos;
        pos = vertices[i * 4]; // i*4 for translation to cube verices number
        pos.x += vertexOffset;
        pos.y = cellSize; // = vOffset + (cellSize * fallDownAxis);
        pos.z += vertexOffset;
        return pos;
    }

    public Vector3 TransToRasterPosition(Carriable tetromino) {
        int cellId = 0;
        //var p = tetromino.transform.position;

        Transform[] allChildren = tetromino.GetComponentsInChildren<Transform>();
        List<Vector3> tempPositions = new List<Vector3>();
        List<int> tempCellId = new List<int>();
        foreach (Transform child in allChildren)
        {
            //childObjects.Add(child.gameObject);
            var p = child.gameObject.transform.position;

            for (int i = 0; i <= vertices.Length - 4; i += 4)
            {
                if (p.x >= vertices[i].x && p.x <= vertices[i + 3].x && p.z >= vertices[i].z && p.z <= vertices[i + 3].z)
                {
                    tempPositions.Add(GetRasterPosition(cellId));
                    tempCellId.Add(cellId);
                }
                cellId++;
            }
            cellId = 0;
        }

        if(tempPositions.Count == allChildren.Length)
        {
            Debug.Log("All cubes are inside Grid");

            for (int i = 0; i < allChildren.Length; i++)
            {
                //occupiedFields.Add(id);
                occupiedFields.Add(tempCellId[i], allChildren[i].gameObject); //TODO: connect with real gameobjects
            }


            //Debug
            foreach (var i in occupiedFields)
            {
                Debug.Log("Field " + i + " is occupied");
            }

            return tempPositions[0];
        }
        else
        {
            Debug.Log("At least one cube is outside the Grid");
            return new Vector3((cellSize * gridSize + 1), cellSize, cellSize); //Case no cell detected
        }

        //List<GameObject> childObjects = new List<GameObject>();
        //foreach (GameObject child in childObjects)
        //{
        //    occupiedPosition = TransToRasterPosition(child.transform.position);
        //}


        
    }

    //public void safeCubePositions(GameObject tetromino)
    //{
    //    Transform[] allChildren = tetromino.GetComponentsInChildren<Transform>();
    //    List<GameObject> childObjects = new List<GameObject>();
    //    foreach (Transform child in allChildren)
    //    {
    //        childObjects.Add(child.gameObject);
    //    }

    //    List <int> occupiedPosition = new List<int>;
    //    foreach (GameObject child in childObjects)
    //    {
    //        occupiedPosition = TransToRasterPosition(child.transform.position);
    //    }
    //}

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

                vertices[v + 0] = new Vector3(-vertexOffset, 0, -vertexOffset) + cellOffset + gridOffset;
                vertices[v + 1] = new Vector3(-vertexOffset, 0, vertexOffset) + cellOffset + gridOffset;
                vertices[v + 2] = new Vector3(vertexOffset, 0, -vertexOffset) + cellOffset + gridOffset;
                vertices[v + 3] = new Vector3(vertexOffset, 0, vertexOffset) + cellOffset + gridOffset;

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
