using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralGrid : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    public float cellSize;
    public int gridSize;
    public int gridLevels;
    public Vector3 gridOffset; //TODO: Not working with y-axis

    private float vertexOffset;
    private Dictionary<int, GameObject> cell = new Dictionary<int, GameObject>();
    private int cellsPerLevel;

    List<Vector3> temp_Positions;
    Dictionary<int, GameObject> temp_CellToCube;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start()
    {
        InvokeRepeating("Downward", 0f, 1f);

        InitCells();
        vertexOffset = cellSize * 0.5f;
        cellsPerLevel = gridSize * gridSize;
        MakeProceduralGrid();
        UpdateMesh();
    }

    private void InitCells()
    {
        for (int i = 0; i < (gridSize * gridSize * gridLevels); i++)
        {
            cell.Add(i, null);
        }
    }


    private bool CheckRasterPositions(Transform[] allChildren)
    {
        int cellId = 0;
        temp_Positions = new List<Vector3>();
        temp_CellToCube = new Dictionary<int, GameObject>();
        foreach (Transform child in allChildren)
        {
            var p = child.gameObject.transform.position;
            for (int i = 0; i <= vertices.Length - 4; i += 4)
            {
                if (p.x >= vertices[i].x && p.x <= vertices[i + 3].x && p.z >= vertices[i].z && p.z <= vertices[i + 3].z && p.y >= (vertices[i].y - vertexOffset) && p.y <= (vertices[i].y + vertexOffset))
                {
                    temp_Positions.Add(GetRasterPosition(cellId));
                    temp_CellToCube[cellId] = child.gameObject;
                }
                cellId++;
            }
            cellId = 0;
        }

        return temp_Positions.Count == allChildren.Length;
    }

    public void TransToRasterPosition(ref Carriable tetromino)
    {   
       

     
        Transform[] allChildren = tetromino.GetComponentsInChildren<Transform>();
        

        if (CheckRasterPositions(allChildren))
        {
            foreach (var id in temp_CellToCube.Keys)
            {
                cell[id] = temp_CellToCube[id];
            }

            tetromino.transform.position = temp_Positions[0];

            var mc = tetromino.GetComponent<MeshCombiner>();
            Destroy(mc.combinedMesh);
            var c = tetromino.GetComponent<Carriable>();
            Destroy(c);
            //tetromino.GetComponent<Carriable>().enabled = false;

            DestroyFullLevels();

            PrintCurrentCellOccupations();

        } else {
            tetromino.transform.position = new Vector3((cellSize * gridSize + 1), 0, cellSize* cellSize); //TODO: Define standart position for tetrominos

            Debug.Log("(Part of) tetromino was outside grid");
            PrintCurrentCellOccupations();
        }
    }

    private void PrintCurrentCellOccupations()
    {
        int count = 0;
        cell.Where(o => o.Value != null)
            .ToList()
            .ForEach(item => { count++; Debug.Log("Cell " + item.Key + " : " + item.Value.name); });
        Debug.Log("Total occupied cells: " + count + " of " + cell.Count);
    }

    private void DestroyFullLevels()
    {
        bool moveUperLevels = false;
        //List<int> fullLevels = new List<int>();

        for (int i = 0; i < gridLevels; i++)
        {
            if (!cell.Where(x => (x.Key >= i*cellsPerLevel) && (x.Key < (i+1)*cellsPerLevel)).Any(x => x.Value == null))
            {                
                //Destroy full Level
                var under = cell.Where(x => (x.Key >= i * cellsPerLevel) && (x.Key < (i + 1) * cellsPerLevel)).Select(c => c.Value);                             
                foreach (var item in under)
                {
                    Destroy(item);
                }
                moveUperLevels = true;
            }

            if(moveUperLevels)
            {
                //Move Levels top of destroyed level
                var over = cell.Where(x => (x.Key >= (i + 1) * cellsPerLevel) && (x.Key < (i + 2) * cellsPerLevel)).Select(c => c.Value);
                foreach (var item in over)
                {
                    if (item != null)
                    {
                        item.transform.position += new Vector3(0, -cellSize, 0);
                    }
                }              
            }
        }       
    }

    private Vector3 GetRasterPosition(int i)
    {
        Vector3 pos;
        pos = vertices[i * 4];
        pos.x += vertexOffset;
        pos.y += vertexOffset; // TODO: adjust y-axis
        pos.z += vertexOffset;
        return pos;
    }

    private void MakeProceduralGrid()
    {
        float floorHight = 0f;

        vertices = new Vector3[gridSize * gridSize * 4 * gridLevels];
        triangles = new int[gridSize * gridSize * 6 * gridLevels];

        int v = 0;
        int t = 0;

        for (int floor = 0; floor < gridLevels; floor++)
        {         
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Vector3 cellOffset = new Vector3(x * cellSize, 0, y * cellSize);

                    vertices[v + 0] = new Vector3(-vertexOffset, floorHight, -vertexOffset) + cellOffset + gridOffset;
                    vertices[v + 1] = new Vector3(-vertexOffset, floorHight, vertexOffset) + cellOffset + gridOffset;
                    vertices[v + 2] = new Vector3(vertexOffset, floorHight, -vertexOffset) + cellOffset + gridOffset;
                    vertices[v + 3] = new Vector3(vertexOffset, floorHight, vertexOffset) + cellOffset + gridOffset;

                    triangles[t] = v;
                    triangles[t + 1] = triangles[t + 4] = v + 1;
                    triangles[t + 2] = triangles[t + 3] = v + 2;
                    triangles[t + 5] = v + 3;

                    v += 4;
                    t += 6;
                }
            }
            floorHight += cellSize;
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
