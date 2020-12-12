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

    //NewWay
    private List<Vector3> snapPosition = new List<Vector3>();
    private List<GameObject> childCubes = new List<GameObject>();

    Carriable tetromino;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start()
    {

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

    public void CheckPosition(ref Carriable tetro)
    {
        tetromino = tetro;

        childCubes.Clear();
        snapPosition.Clear();
        if (tetromino.transform.position.y >= (vertices[vertices.Length - 4].y + vertexOffset)) //Check if tetromino is above the 3D-grid...
        {
            Transform parent = tetromino.GetComponent<Transform>();
            Transform[] childrenWithParent = tetromino.GetComponentsInChildren<Transform>();
            int cellId = 0;
            int countCubes = 0;
            foreach (Transform child in childrenWithParent)
            {
                if (child != parent)
                {
                    var p = child.gameObject.transform.position;

                    //Check for one grid level if all cubes are inside the grid
                    for (int i = 0; i <= (vertices.Length - 4) / gridLevels; i += 4) 
                    {
                        if (p.x >= vertices[i].x && p.x <= vertices[i + 3].x && p.z >= vertices[i].z && p.z <= vertices[i + 3].z)
                        {
                            snapPosition.Add(GetRasterPosition(cellId));
                            countCubes++;
                        }
                        cellId++;
                    }
                }
            }

            
            //If all cubes inside 3D-Grid, snap its position
            if (countCubes == childrenWithParent.Length-1)
            {
                SnapPosition();              
            }
            else
            {
                Debug.Log("Position too far out of field");
                //tetromino.transform.position = TetrominoDefaultPosition(); //TODO: De-comment out this line
            }
        }
        else
        {
            Debug.Log("Position to low");
            //tetromino.transform.position = TetrominoDefaultPosition(); //TODO: De-comment out this line
        }
    }

    private void SnapPosition()
    {
        tetromino.transform.position = snapPosition[1]; //TODO: u.u muss muss die position für alle children ink. parent seperat gesetzt werden
        InvokeRepeating("FallDownUntilCollision", 1.0f, 1.0f);
    }

    private void FallDownUntilCollision()
    {
        float xPos = tetromino.gameObject.transform.position.x;
        float yPos = tetromino.gameObject.transform.position.y;
        float zPos = tetromino.gameObject.transform.position.z;
        yPos -= cellSize;
        if(yPos >= vertices[0].y) //TODO: Add here more conditions => collision with occepiuded cells
        {
            Vector3 newPos = new Vector3(xPos, yPos, zPos);
            tetromino.gameObject.transform.position = newPos;
        }
        else
        {
            CancelInvoke();
            SaveCubePositions();

            var meshCombiner = tetromino.GetComponent<MeshCombiner>();
            Destroy(meshCombiner.combinedMesh);
            var carriable = tetromino.GetComponent<Carriable>();
            Destroy(carriable);

            DestroyFullLevels();
        }
    }

    private void SaveCubePositions()
    {
        Transform[] childrenAndParent = tetromino.GetComponentsInChildren<Transform>();
        Dictionary<int, GameObject> temp_CellToCube = new Dictionary<int, GameObject>();
        int cellId = 0;
        foreach (Transform child in childrenAndParent)
        {
            var p = child.gameObject.transform.position;
            for (int i = 0; i <= vertices.Length - 4; i += 4)
            {
                //Check on which cell the cubes are
                if (p.x >= vertices[i].x && p.x <= vertices[i + 3].x && p.z >= vertices[i].z && p.z <= vertices[i + 3].z && p.y >= (vertices[i].y - vertexOffset) && p.y <= (vertices[i].y + vertexOffset))
                {                   
                    temp_CellToCube[cellId] = child.gameObject;
                }
                cellId++;
            }
            cellId = 0;
        }

        foreach (var id in temp_CellToCube.Keys)
        {
            cell[id] = temp_CellToCube[id];
        }
    }

    //TODO: Define standart position for tetrominos
    private Vector3 TetrominoDefaultPosition()
    {
        return new Vector3((cellSize * gridSize + 1), 0, cellSize * cellSize);
    }

    private void PrintCurrentCellOccupations()
    {
        int count = 0;
        cell.Where(o => o.Value != null)
            .ToList()
            .ForEach(item => { count++; /*Debug.Log("Cell " + item.Key + " : " + item.Value.name);*/ });
        Debug.Log("Total occupied cells: " + count + " of " + cell.Count);
    }

    private void DestroyFullLevels()
    {
        bool moveUperLevels = false;

        for (int i = 0; i < gridLevels; i++)
        {
            if (!cell.Where(x => (x.Key >= i * cellsPerLevel) && (x.Key < (i + 1) * cellsPerLevel)).Any(x => x.Value == null))
            {
                //Destroy full Level
                var under = cell.Where(x => (x.Key >= i * cellsPerLevel) && (x.Key < (i + 1) * cellsPerLevel)).Select(c => c.Value);
                foreach (var item in under)
                {
                    Destroy(item);
                }
                moveUperLevels = true;
            }

            if (moveUperLevels)
            {
                //Move Levels which are on top of destroyed level
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
        pos.y = (cellSize * gridLevels) - vertexOffset;
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
