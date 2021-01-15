using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public enum HoldRotation {
    Left,
    Right,
    Up,
    Down,
    Clockwise,
    CounterClockwise
}

public class Pickup : MonoBehaviour {

    private Carriable heldItem = null;
    private bool itemHeld = false;
    private Carriable lastLookedAt = null;
    private bool hasLastLookedAt = false;

    public ProceduralGrid grid;

    public float maxDistance = 10f;
    public float minDistance = 1.5f;

    [Range(2.5f, 25.0f)]
    public float zoomSmoothing = 5f;
    
    // Store buttons to register handlers
    public Button pickupButton;
    public Button turnUpButton;
    public Button turnLeftButton;
    public Button turnDownButton;
    public Button turnRightButton;


    void Start() {
        pickupButton.onClick.AddListener(PressPickupButton);
        turnUpButton.onClick.AddListener(() => RotateHeldItem(HoldRotation.Up));
        turnLeftButton.onClick.AddListener(() => RotateHeldItem(HoldRotation.Left));
        turnDownButton.onClick.AddListener(() => RotateHeldItem(HoldRotation.Down));
        turnRightButton.onClick.AddListener(() => RotateHeldItem(HoldRotation.Right));
    }

    // Update is called once per frame
    private void Update() {
        if (itemHeld) {
            HandleMouseZoom();

            if (Input.GetMouseButtonUp(0) && Cursor.lockState == CursorLockMode.Locked) {
                DropHeldItem();
            }
            else if (Input.GetKeyUp(KeyCode.J)) {
                RotateHeldItem(HoldRotation.Left);
            }
            else if (Input.GetKeyUp(KeyCode.K)) {
                RotateHeldItem(HoldRotation.Down);
            }
            else if (Input.GetKeyUp(KeyCode.L)) {
                RotateHeldItem(HoldRotation.Right);
            }
            else if (Input.GetKeyUp(KeyCode.I)) {
                RotateHeldItem(HoldRotation.Up);
            }
            else if (Input.GetKeyUp(KeyCode.U)) {
                RotateHeldItem(HoldRotation.Clockwise);
            }
            else if (Input.GetKeyUp(KeyCode.O)) {
                RotateHeldItem(HoldRotation.CounterClockwise);
            }
        }
        else {
            if (RaycastCarriable(out var hit, out var carriable)) {

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

    private void PressPickupButton() {
        if (itemHeld) {
            DropHeldItem();
        } else if (RaycastCarriable(out var hit, out var carriable)) {
            carriable.PickUp(this, hit);
            heldItem = carriable;
            itemHeld = true;
        }
    }

    private void DropHeldItem() {
        heldItem.DropObject();

        grid.CheckPosition(ref heldItem);

        heldItem = null;
        itemHeld = false;
    }

    private bool RaycastCarriable(out RaycastHit hit, out Carriable carriable) {
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance) &&
            FindInHierarchy(hit.collider, out carriable)) {
            return true;
        }

        carriable = null;
        return false;
    }

    void HandleMouseZoom() {
        var currentPos = heldItem.transform.localPosition;
        var zDelta =
            Mathf.Clamp(currentPos.z + Input.mouseScrollDelta.y / zoomSmoothing,
                        minDistance, maxDistance) - currentPos.z;

        heldItem.transform.Translate(
            0, 0, zDelta,
            this.transform
        );
    }

    private static bool FindInHierarchy<T>(Component c, out T component) {
        component = c.GetComponentInParent<T>();
        return component != null;
    }

    private void RotateHeldItem(HoldRotation rotation) {
        if (!itemHeld) return;
        var worldAxisRight = NearestWorldAxis(transform.right);
        var worldAxisForward = NearestWorldAxis(transform.forward);
        var worldAxisUp = NearestWorldAxis(transform.up);
        
        switch (rotation) {
            case HoldRotation.Up:
                heldItem.Rotate(worldAxisRight, 90);
                break;
            case HoldRotation.Down:
                heldItem.Rotate(worldAxisRight, -90);
                break;
            case HoldRotation.Right:
                heldItem.Rotate(worldAxisUp, -90);
                break;
            case HoldRotation.Left:
                heldItem.Rotate(worldAxisUp, 90);
                break;
            case HoldRotation.Clockwise:
                heldItem.Rotate(worldAxisForward, -90);
                break;
            case HoldRotation.CounterClockwise:
                heldItem.Rotate(worldAxisForward, 90);
                break;
        }
    }

    private static Vector3 NearestWorldAxis(Vector3 v) {
        if (Mathf.Abs(v.x) < Mathf.Abs(v.y)) {
            v.x = 0;
            if (Mathf.Abs(v.y) < Mathf.Abs(v.z))
                v.y = 0;
            else
                v.z = 0;
        }
        else {
            v.y = 0;
            if (Mathf.Abs(v.x) < Mathf.Abs(v.z))
                v.x = 0;
            else
                v.z = 0;
        }

        return v;
    }
}
