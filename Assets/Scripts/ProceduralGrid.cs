using System.Collections.Generic;
using System.Linq;
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
    public Vector3 gridOffset;

    private float vertexOffset;
    private Dictionary<int, GameObject> cells = new Dictionary<int, GameObject>();
    private int cellsPerLevel;

    private Vector3 snapPosition;
    private List<GameObject> childCubes = new List<GameObject>();
    List<Transform> onlyChildren;

    Carriable tetromino;

    List<Transform> tempChilds = new List<Transform>();
    Transform lowestChild;
    private int levelsDestroyed = 0;

    private Color32[] colors;
    private Color32 gridColor = new Color(0, 0, 255);   //245,94,97
    private Color32 markColor = new Color(0, 255f, 0);  //201,218,248

    public bool lookTetrominoAdding = false;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start()
    {
        onlyChildren = new List<Transform>();
        InitCells();
        vertexOffset = cellSize * 0.5f;
        cellsPerLevel = gridSize * gridSize;
        MakeProceduralGrid();
        InitializeMesh();
    }

    private void InitCells()
    {
        for (int i = 0; i < (gridSize * gridSize * gridLevels); i++)
        {
            cells.Add(i, null);
        }
    }
    
    public void MarkSnapPosition(Carriable tetro)
    {
        CleanMarkers();
        tetromino = tetro;
        
        if (tetromino.transform.position.y >= (vertices[vertices.Length - 4].y + vertexOffset)) //Check if tetromino is above the play field
        {
            Transform parent = tetromino.GetComponent<Transform>();
            Transform[] childrenWithParent = tetromino.GetComponentsInChildren<Transform>();
            
            int[] tempMarkCells = new int[childrenWithParent.Length-1]; //-1 because only children are relevant
            int marksCounter = 0;
            
            foreach (Transform child in childrenWithParent)
            {
                if (child != parent)
                {
                    var p = child.gameObject.transform.position;
                    for (int i = 0; i <= (vertices.Length - 4) / gridLevels; i += 4)
                    {
                        if (p.x >= vertices[i].x && p.x <= vertices[i + 3].x && p.z >= vertices[i].z &&
                            p.z <= vertices[i + 3].z)
                        {
                            tempMarkCells[marksCounter] = i + (4 * cellsPerLevel * (gridLevels - 1));
                            marksCounter++;
                        }
                    }
                }
            }
            //Marks cells for every single cube
            if (marksCounter == childrenWithParent.Length-1)
            {
                MarkCell(tempMarkCells);
            }
        }
    }

    public void TryToSnapPosition(ref Carriable tetro)
    {
        onlyChildren.Clear();
        tempChilds.Clear();

        tetromino = tetro;
        childCubes.Clear();
        snapPosition = new Vector3();
        float lowestCubeY = float.MaxValue;
        if (tetromino.transform.position.y >= (vertices[vertices.Length - 4].y + vertexOffset)) //Check if tetromino is above the play field
        {
            lowestChild = null;
            Transform parent = tetromino.GetComponent<Transform>();
            Transform[] childrenWithParent = tetromino.GetComponentsInChildren<Transform>();
            int cellId = 0;
            int countCubes = 0;
            foreach (Transform child in childrenWithParent)
            {
                if (child != parent)
                {
                    var p = child.gameObject.transform.position;

                    //Check if all cubes of tetromino inside x- and z- axis of play fileds
                    for (int i = 0; i <= (vertices.Length - 4) / gridLevels; i += 4)
                    {
                        if (p.x >= vertices[i].x && p.x <= vertices[i + 3].x && p.z >= vertices[i].z &&
                            p.z <= vertices[i + 3].z)
                        {
                            if (p.y < lowestCubeY)
                            {
                                lowestCubeY = p.y;
                                lowestChild = child;
                                snapPosition = GetRasterPosition(cellId);
                            }

                            tempChilds.Add(child);
                            countCubes++;
                        }
                        cellId++;
                    }
                }
            }
            CleanMarkers();

            //If all cubes of tetromino inside the play field, snap its positions
            if (countCubes == childrenWithParent.Length - 1)
            {
                SnapPosition();
                lookTetrominoAdding = true;
            }
            else
            {
                Debug.Log("Position too far out of field");
                tetromino.transform.position = TetrominoDefaultPosition();
            }
        }
        else
        {
            Debug.Log("Position to low");
            tetromino.transform.position = TetrominoDefaultPosition();
        }
    }

    private void SnapPosition()
    {
        onlyChildren.Add(lowestChild); //Add lowest child first because of ground detection
        foreach (var child in tempChilds)
        {
            if (!child.Equals(lowestChild))
            {
                child.gameObject.transform.SetParent(lowestChild.transform, true);
                onlyChildren.Add(child);
            }
        }

        lowestChild.transform.parent = null;
        lowestChild.transform.position = snapPosition;
        tetromino.gameObject.SetActive(false);
        lowestChild.transform.DetachChildren();

        InvokeRepeating("FallDownUntilCollision", 1.0f, 1.0f);
    }

    private void FallDownUntilCollision()
    {
        bool noCollision = true;
        Dictionary<Transform, Vector3> safePos = new Dictionary<Transform, Vector3>();
        foreach (var child in onlyChildren)
        {
            float xPos = child.gameObject.transform.position.x;
            float yPos = child.gameObject.transform.position.y;
            float zPos = child.gameObject.transform.position.z;
            yPos -= cellSize;
            Vector3 newPos = new Vector3(xPos, yPos, zPos);

            //Check if new positions of child collide with play filed ground or other cubes
            if (yPos < vertices[0].y || ChildCollided(newPos))
            {
                noCollision = false;
                CancelInvoke();
                SaveCubePositions();
                DestroyFullLevels();
                PrintCurrentCellOccupations();
                FindObjectOfType<TimedTetrominos>().LoadNewTetro();
                break;
            }
            else
            {
                safePos.Add(child, newPos);
            }
        }

        if (noCollision)
        {
            foreach (var sp in safePos)
            {
                sp.Key.transform.position = sp.Value;
            }
        }
    }

    private bool ChildCollided(Vector3 newPos)
    {
        foreach (var cell in cells)
        {
            if (cell.Value != null)
            {
                //Check if new position collides with a full cell
                if (cell.Value.GetComponent<Collider>().bounds.Contains(newPos))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void SaveCubePositions()
    {
        int cellId = 0;
        foreach (Transform child in onlyChildren)
        {
            var p = child.gameObject.transform.position;

            if (p.y > vertices[vertices.Length - 4].y)
            {
                Debug.Log("Game Over");
                break;
            }

            for (int i = 0; i <= vertices.Length - 4; i += 4)
            {
                //Check on which cell the tetrominos child are
                if (p.x >= vertices[i].x && p.x <= vertices[i + 3].x && p.z >= vertices[i].z &&
                    p.z <= vertices[i + 3].z && p.y >= vertices[i].y && p.y <= vertices[i].y + cellSize)
                {
                    cells[cellId] = child.gameObject;
                }
                cellId++;
            }
            cellId = 0;
        }

        lookTetrominoAdding = false;
    }

    private Vector3 TetrominoDefaultPosition()
    {
        return new Vector3((gridSize * cellSize) + 2 * cellSize, gridOffset.y,
            0); //TODO: Define standart position for tetrominos
    }

    private void PrintCurrentCellOccupations()
    {
        int count = 0;
        cells.Where(o => o.Value != null)
            .ToList()
            .ForEach(item => { count++; /*Debug.Log("Cell " + item.Key + " : " + item.Value.name); */ });
        Debug.Log("Total occupied cells: " + count + " of " + cells.Count);
        Debug.Log("Levels Destroyed: " + levelsDestroyed);
    }

    private void DestroyFullLevels()
    {
        bool moveUperLevels = false;
        for (int i = 0; i < gridLevels; i++)
        {
            if (!cells.Where(x => (x.Key >= i * cellsPerLevel) && (x.Key < (i + 1) * cellsPerLevel))
                .Any(x => x.Value == null))
            {
                //Destroy full Level (with LINQ)
                var under = cells.Where(x => (x.Key >= i * cellsPerLevel) && (x.Key < (i + 1) * cellsPerLevel))
                    .Select(c => c).ToList();
                foreach (var item in under)
                {
                    Destroy(item.Value);
                    cells[item.Key] = null;
                }

                moveUperLevels = true;
                levelsDestroyed++;
            }

            if (moveUperLevels)
            {
                //Move Levels which on top of destroyed level
                var over = cells
                    .Where(x => (x.Key >= (i + 1) * cellsPerLevel) && (x.Key < (gridLevels * cellsPerLevel)))
                    .Select(c => c).ToList();
                foreach (var item in over)
                {
                    if (item.Value != null)
                    {
                        item.Value.transform.position += new Vector3(0, -cellSize, 0);
                        cells[item.Key - cellsPerLevel] = item.Value;
                        cells[item.Key] = null;
                    }
                }

                moveUperLevels = false;
                DestroyFullLevels();
            }
        }
    }

    private Vector3 GetRasterPosition(int i)
    {
        Vector3 pos;
        pos = vertices[i * 4];
        pos.x += vertexOffset;
        pos.y = (cellSize * gridLevels) - vertexOffset + gridOffset.y;
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

    void InitializeMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        colors = new Color32[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            colors[i] = gridColor;
        }
        mesh.colors32 = colors;
    }

    void CleanMarkers()
    {
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            colors[i] = gridColor;
        }
        mesh.colors32 = colors;
    }

    void MarkCell(int[] cellsToMark)
    {
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            foreach (var cellToMark in cellsToMark)
            {
                if (i == cellToMark)
                {
                    colors[i + 0] = markColor;
                    colors[i + 1] = markColor;
                    colors[i + 2] = markColor;
                    colors[i + 3] = markColor;
                    i += 4;
                }
            }
        }
        mesh.colors32 = colors;
    }
}