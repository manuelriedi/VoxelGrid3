using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralGrid : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    public float cellSize;
    public int gridSize;
    public int gridLevels;
    public Vector3 gridOffset;
    
    private Carriable tetromino;
    private Vector3 snapPosition;
    public bool lookTetrominoAdding = false;
    
    private readonly Dictionary<int, GameObject> cells = new Dictionary<int, GameObject>();
    private float vertexOffset;
    private int cellsPerLevel;
    private int levelsDestroyed = 0;
    
    private List<GameObject> childCubes = new List<GameObject>();
    private List<Transform> onlyChildren;
    private List<Transform> tempChilds = new List<Transform>();
    private Transform lowestChild;
    
    private Color32[] colors;
    private Color32 gridColor = new Color(245f,94f,97f);
    private Color32 markColor = new Color(0f, 0f, 238f); 
    
    public LevelText levelText;
    public Text statusText;
    private float textSetTime = 0;
    public Image backgroundPanel;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start()
    {
        levelText.UpdateScore(0);
        backgroundPanel.enabled = false;
        
        gridSize = PlayerPrefs.GetInt("gridSize", gridSize);
        onlyChildren = new List<Transform>();
        InitCells();
        vertexOffset = cellSize * 0.5f;
        cellsPerLevel = gridSize * gridSize;
        MakeProceduralGrid();
        InitializeMesh();
    }

    /// <summary>
    /// Initializes the entire playfield with empty cells
    /// </summary>
    private void InitCells()
    {
        for (int i = 0; i < (gridSize * gridSize * gridLevels); i++)
        {
            cells.Add(i, null);
        }
    }
    
    private void Update()
    {
        if ((Time.time * 1000) - textSetTime > 2500)
        {
            statusText.text = "";
            backgroundPanel.enabled = false;
        }
    }
    
    /// <summary>
    /// Creates a gridded mark on the playing field that indicates where the tetromino will fall before it is dropped
    /// </summary>
    public void MarkSnapPosition(Carriable tetro)
    {
        CleanMarkers();
        tetromino = tetro;
        
        if (tetromino.transform.position.y >= vertices[vertices.Length - 4].y + vertexOffset)
        {
            Transform parent = tetromino.GetComponent<Transform>();
            Transform[] childrenWithParent = tetromino.GetComponentsInChildren<Transform>();
            int[] tempMarkCells = new int[childrenWithParent.Length-1];
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

            if (marksCounter == childrenWithParent.Length-1)
            {
                MarkCell(tempMarkCells);
            }
        }
    }
    
    /// <summary>
    /// Searches for a suitable raster position on the board after the tetromino has been dropped. 
    /// </summary>
    public void TryToSnapPosition(ref Carriable tetro)
    {
        onlyChildren.Clear();
        tempChilds.Clear();
        tetromino = tetro;
        childCubes.Clear();
        snapPosition = new Vector3();
        float lowestCubeY = float.MaxValue;
        
        if (tetromino.transform.position.y >= vertices[vertices.Length - 4].y + vertexOffset) //Check if tetromino is above the play field
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

                    //Check if all childs inside x- and z- axis of play fileds
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

            //Snaps only position, if all cubes inside play field
            if (countCubes == childrenWithParent.Length - 1)
            {
                SnapPosition();
                lookTetrominoAdding = true;
            }
            else
            {
                SetStatusMessage("You positioned the tetromino to far out of field");
                tetromino.transform.position = TetrominoDefaultPosition();
            }
        }
        else
        {
            SetStatusMessage("You positioned the tetromino to low");
            tetromino.transform.position = TetrominoDefaultPosition();
        }
    }

    public void SetStatusMessage(string message)
    {
        statusText.text = message;
        textSetTime = Time.time * 1000;
        backgroundPanel.enabled = true;
    }

    /// <summary>
    /// Makes the lowest part of the tetromino move to the defined snaping position. 
    /// </summary>
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

    /// <summary>
    /// Drops the tetromino down one whole cell at a time at a given rate until it collides with something (either the
    /// ground or with another tetromino)
    /// </summary>
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

    /// <summary>
    /// Checks for each child of the tetromino if it collides with an already full Celle.
    /// </summary>
    /// <returns>
    /// Return true if a child collided with a full cell
    /// </returns>
    private bool ChildCollided(Vector3 newPos)
    {
        foreach (var cell in cells)
        {
            if (cell.Value != null)
            {
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
                SetStatusMessage("GAME OVER");
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

    /// <summary>
    /// Set the tetromino on a defaultposition, which is always positioned next to the playing field depending
    /// on the grid size and grid position
    /// </summary>
    private Vector3 TetrominoDefaultPosition()
    {
        return new Vector3((gridSize * cellSize) + 2 * cellSize, gridOffset.y, 0); //TODO: Define standart position for tetrominos
    }

    /// <summary>
    /// Prints occupation of cells to console
    /// </summary>
    private void PrintCurrentCellOccupations()
    {
        int count = 0;
        cells.Where(o => o.Value != null)
            .ToList()
            .ForEach(item => { count++; /*Debug.Log("Cell " + item.Key + " : " + item.Value.name); */ });
        Debug.Log("Total occupied cells: " + count + " of " + cells.Count);
        levelText.UpdateScore(levelsDestroyed);
    }

    /// <summary>
    /// Finds full levels and destroys them accordingly.
    /// It also moves down the tetreminos that are above the destroyed levels. 
    /// </summary>
    private void DestroyFullLevels()
    {
        bool moveUperLevels = false;
        for (int i = 0; i < gridLevels; i++)
        {
            if (!cells.Where(x => (x.Key >= i * cellsPerLevel) && (x.Key < (i + 1) * cellsPerLevel))
                .Any(x => x.Value == null))
            {
                //Destroy full Level
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

    /// <returns>
    /// Return the rastered position for every child of the tetromino
    /// </returns>
    private Vector3 GetRasterPosition(int i)
    {
        Vector3 pos;
        pos = vertices[i * 4];
        pos.x += vertexOffset;
        pos.y = (cellSize * gridLevels) - vertexOffset + gridOffset.y;
        pos.z += vertexOffset;
        return pos;
    }

    /// <summary>
    /// Creates new playfield based on the filed size and number of grid sizes set by the user.
    /// </summary>
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

    /// <summary>
    /// Creates a mesh over the playing field. It also colorizes the playfield initially
    /// </summary>
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
    
    /// <summary>
    /// Set markings that indicate the grid positions when you move the tetromino over the playfield. 
    /// </summary>
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
    
    /// <summary>
    /// Deletes the old playfield markings that indicate the grid positions when you move the tetromino over
    /// the playfield. 
    /// </summary>
    void CleanMarkers()
    {
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            colors[i] = gridColor;
        }
        mesh.colors32 = colors;
    }
}