using System.Collections;
using UnityEngine;


public class TimedTetrominos : MonoBehaviour
{
    public bool stopSpawing = false;
    public ProceduralGrid grid;
    public GameObject[] tetrominos;


    void Start()
    {
        LoadNewTetro();

    }

    public void LoadNewTetro()
    {
        int selection = Random.Range(0, tetrominos.Length - 1);

        GameObject tetromino = Instantiate(tetrominos[selection], transform.position, Quaternion.identity);
        tetromino.transform.localScale = new Vector3(grid.cellSize, grid.cellSize, grid.cellSize);
    }





}
