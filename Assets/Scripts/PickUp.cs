using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public GameObject tetromino;
    public ProceduralGrid grid;

    private void Start()
    {
        //tetromino.transform.position = grid.GetComponent<ProceduralGrid>().TransToRasterPosition(new Vector3(0f, 0f, 0f));
        //tetromino.transform.rotation = Quaternion.LookRotation(Vector3.up);
    }

    void OnTriggerStay(Collider o)
    {
        if (o.name == "Tetromino0")
        {
            if (Input.GetKey(KeyCode.T))
            {
                //Debug.Log("Tetromino Triggered");
                tetromino.transform.parent = transform.parent;
            }
        }
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            //Debug.Log("Relased Tetromino");
            tetromino.transform.SetParent(null, true);
            Vector3 position = tetromino.transform.position;

            tetromino.transform.position = grid.GetComponent<ProceduralGrid>().TransToRasterPosition(position);
            tetromino.transform.rotation = Quaternion.LookRotation(Vector3.up);
        }
    }


    



  

}
