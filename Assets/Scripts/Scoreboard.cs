using TMPro;
using UnityEngine;

public class Scoreboard : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI checkpointText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI deathText;

    public void SetData(string playerName, int checkpoint, int lives, int deaths) {
        nameText.text = playerName;
        checkpointText.text = checkpoint.ToString();
        livesText.text = lives.ToString();
        deathText.text = deaths.ToString();
    }
}