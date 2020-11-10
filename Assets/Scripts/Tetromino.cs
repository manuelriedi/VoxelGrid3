using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Tetromino : MonoBehaviour {
    public ProceduralGrid grid;

    void Start() {
        this.transform.localScale = new Vector3(grid.cellSize, grid.cellSize, grid.cellSize);
        this.transform.position = new Vector3(-grid.cellSize, grid.cellSize * 0.5f, -grid.cellSize); //Start Position
    }


}
