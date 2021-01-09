using UnityEngine;

public class ToggleOnEscape : MonoBehaviour {

    public GameObject toggledElement;
    public bool initiallyVisible = false;

    private bool currentlyVisible;

    // Start is called before the first frame update
    void Start() {
        toggledElement.SetActive(initiallyVisible);
        currentlyVisible = initiallyVisible;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            toggledElement.SetActive(currentlyVisible = !currentlyVisible);
        }
    }
}
