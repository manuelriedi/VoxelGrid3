using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LevelText : MonoBehaviour {
    
    public string textFormat = "Total Cleared Levels: {0}";
    public int score = 0;

    private int oldScore;
    private Text self;

    private void Awake() {
        this.self = GetComponent<Text>();

        oldScore = score;
        self.text = string.Format(textFormat, score);
    }

    private void Update() {
        if (oldScore == score) return;
        
        oldScore = score;
        self.text = string.Format(textFormat, score);
    }

    public void UpdateScore(int newScore) {
        score = newScore;
    }
}
