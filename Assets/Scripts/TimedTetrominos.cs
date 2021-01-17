using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class TimedTetrominos : MonoBehaviour
{
    public bool stopSpawing = false;
    public ProceduralGrid grid;
    public GameObject[] tetrominos;
    // Timer till next tetromino is loaded
    public float maxTime = 7000f; //7s
    private float timeNow;
    float deltaTime;


    void Start()
    {
        deltaTime = maxTime;
        timeNow = DateTime.Now.Millisecond;
        LoadNewTetro();

    }

    /// <summary>
    /// Detect if game is over if not and timer is up trigger tetromino generation
    /// </summary>
    void Update()
    {
        GameObject[] tetrominosInGame = GameObject.FindGameObjectsWithTag("OnHeap");

        if (tetrominosInGame.Length > 3)
        {
            grid.SetStatusMessage("GAME OVER");
        }
        else if (deltaTime <= 0)
        {
            LoadNewTetro();
            deltaTime = maxTime;
        }
        else
        {
            deltaTime -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Initialize next Tetromino and place it on tetromino heap
    /// </summary>
    void LoadNewTetro()
    {
        float offset = getHeightHeap();

        int selection = UnityEngine.Random.Range(0, tetrominos.Length - 1);
        float y = transform.position.y;
        GameObject tetromino = Instantiate(tetrominos[selection], new Vector3(transform.position.x, y + offset, transform.position.z), Quaternion.identity);
        tetromino.tag = "OnHeap";
        tetromino.transform.localScale = new Vector3(grid.cellSize, grid.cellSize, grid.cellSize);

    }

    float getHeightHeap() {
        float spacing = 0;
        GameObject[] tetrominosInGame = GameObject.FindGameObjectsWithTag("OnHeap");
        foreach (GameObject tetrominoInGame in tetrominosInGame)
        {
            MeshRenderer renderer = tetrominoInGame.GetComponent<MeshRenderer>();
            spacing += renderer.bounds.size.y;
        }

        return spacing;
    }





}
