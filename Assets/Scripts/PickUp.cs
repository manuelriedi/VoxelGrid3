using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public GameObject go;

    void OnTriggerStay(Collider cube)
    {
        if (cube.name == "Cube")
        {
            if (Input.GetKey(KeyCode.T)) {
                Debug.Log("Triggered");
                go.transform.parent = transform.parent;
            }else if(Input.GetKey(KeyCode.Z))
            {
                // TODO 1: drop cube down
                // TODO 2: pass position of cube to the ProceduralGrid.cs => It will snap the cube to the next best position. 
            }
        }
    }
}
