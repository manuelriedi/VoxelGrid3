using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedTetrominos : MonoBehaviour
{
    //public GameObject spawnee;
    public bool stopSpawing = false;
    public List<GameObject> spawnObjects;
    // Start is called before the first frame update
    void Start()
    {
        var prefabs = Resources.LoadAll<GameObject>("Prefabs/");
        foreach (GameObject obj in prefabs)
        {
            spawnObjects.Add(obj);
        }
        // InvokeRepeating("SpawnObject");
        while (!stopSpawing)
        {
            SpawnObject();
        }

    }

    public void SpawnObject()
    {

        int selection = Random.Range(0, spawnObjects.Count);
        Instantiate(spawnObjects[selection], transform.position, transform.rotation);
        
       // Instantiate(spawnee, transform.postion, transform.rotation);

    }


}
