using System;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class PropertySlider : MonoBehaviour {
    public string propertyName;
    public string displayText;
    public Text display;

    private Slider self;

    private void Awake() {
        self = GetComponent<Slider>();
        self.onValueChanged.AddListener(newValue => {
            display.text = string.Format(displayText, (int)newValue);
        });
    }

    private void OnDestroy() {
        PlayerPrefs.SetInt(propertyName, (int) self.value);
    }
}
