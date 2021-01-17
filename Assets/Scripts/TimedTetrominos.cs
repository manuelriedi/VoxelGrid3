using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class TimedTetrominos : MonoBehaviour
{
    public bool stopSpawing = false;
    public ProceduralGrid grid;
    public GameObject[] tetrominos;
    public float maxTime = 7000f; //7s
    private float timeNow;
    float deltaTime;


    void Start()
    {
        deltaTime = maxTime;
        timeNow = DateTime.Now.Millisecond;
        LoadNewTetro();

    }

    void Update()
    {
        GameObject[] tetrominosInGame = GameObject.FindGameObjectsWithTag("OnHeap");

        if (tetrominosInGame.Length > 3)
        {
            Debug.Log("Game Over");
            FindObjectOfType<GameOver>().GameIsOver();
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

   void LoadNewTetro()
    {

        GameObject[] tetrominosInGame = GameObject.FindGameObjectsWithTag("OnHeap");

        float offset = 0.5f;
        float spacing = tetrominosInGame.Length * offset;

        int selection = UnityEngine.Random.Range(0, tetrominos.Length - 1);
        float y = transform.position.y;
        GameObject tetromino = Instantiate(tetrominos[selection], new Vector3(transform.position.x, y + spacing, transform.position.z), Quaternion.identity);
        tetromino.tag = "OnHeap";
        tetromino.transform.localScale = new Vector3(grid.cellSize, grid.cellSize, grid.cellSize);

    }





}
