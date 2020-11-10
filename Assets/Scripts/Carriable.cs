using System;

using cakeslice;

using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Outline))]
public class Carriable : MonoBehaviour {
    private Transform initialParent;
    public Outline outline { get; private set; }

    public float distance = 2.0f;

    private Quaternion cachedRotation;
    private Pickup holder;
    private bool isHeld = false;

    // Use this for initialization
    void Start() {
        outline = GetComponent<Outline>();
        outline.enabled = false;

        initialParent = transform.parent;
    }

    // Update is called once per frame
    void Update() {
        if (isHeld) {
            transform.rotation = cachedRotation;
        }
    }

    public void PickUp(Pickup newHolder, RaycastHit raycastHit) {
        cachedRotation = transform.rotation;

        transform.position = raycastHit.point;
        transform.parent = newHolder.transform;
        this.holder = newHolder;
        isHeld = true;

        //holder.rotationHandles.transform.parent = transform;
    }

    public void DropObject() {
        // Last Update
        Update();

        transform.parent = initialParent;

        //holder.rotationHandles.transform.parent = null;
        this.holder = null;
        this.isHeld = false;
    }
}
