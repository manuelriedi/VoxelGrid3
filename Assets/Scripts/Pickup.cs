using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.UI;

using Outline = cakeslice.Outline;

public class Pickup : MonoBehaviour {

    private Carriable heldItem = null;
    private bool itemHeld = false;
    private Carriable lastLookedAt = null;
    private bool hasLastLookedAt = false;

    public float maxDistance = 10f;
    public float minDistance = 1.5f;
    [Range(2.5f, 25.0f)]
    public float zoomSmoothing = 5f;

    void Start() { }

    // Update is called once per frame
    private void Update() {
        if (itemHeld) {
            var currentPos = heldItem.transform.localPosition;
            var zDelta =
                Mathf.Clamp(currentPos.z + Input.mouseScrollDelta.y / zoomSmoothing,
                            minDistance, maxDistance) - currentPos.z;

            heldItem.transform.Translate(
                0, 0, zDelta,
                this.transform
            );
            if (Input.GetKey(KeyCode.F)) {
                Debug.Log(Input.mouseScrollDelta);
            }

            if (Input.GetMouseButtonUp(0) && Cursor.lockState == CursorLockMode.Locked) {
                heldItem.DropObject();
                heldItem = null;
                itemHeld = false;
            }
        }
        else {
            if (Physics.Raycast(transform.position, transform.forward, out var hit, maxDistance) &&
                FindInHierarchy<Carriable>(hit.collider, out var carriable)) {

                if (Input.GetMouseButtonUp(0)) {
                    carriable.PickUp(this, hit);
                    heldItem = carriable;
                    itemHeld = true;
                }
                else {
                    if (!hasLastLookedAt) {
                        lastLookedAt = carriable;
                        lastLookedAt.outline.enabled = true;
                        hasLastLookedAt = true;
                    }
                    else if (lastLookedAt != carriable) {
                        lastLookedAt.outline.enabled = false;
                        lastLookedAt = carriable;
                        lastLookedAt.outline.enabled = true;
                    }
                }
            }
            else if (hasLastLookedAt) {
                lastLookedAt.outline.enabled = false;
                lastLookedAt = null;
                hasLastLookedAt = false;
            }
        }
    }

    private static bool FindInHierarchy<T>(Component c, out T component) {
        component = c.GetComponentInParent<T>();
        return component != null;
    }
}
