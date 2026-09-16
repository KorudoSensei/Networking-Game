using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankingRowUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image rowBackground;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text gapText;

    [Header("Trackmania Color Scheme")]
    [SerializeField] private Color localPlayerColor = new Color(0.1f, 0.8f, 0.2f, 0.8f);
    [SerializeField] private Color opponentColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);

    public void SetData(int rank, string name, float gap, bool isLeader, bool isLocalPlayer)
    {
        if (rowBackground != null)
        {
            rowBackground.color = isLocalPlayer ? localPlayerColor : opponentColor;
        }

        if (rankText != null) rankText.text = $"{rank}";
        if (nameText != null) nameText.text = name;
        if (gapText != null)
        {
            if (isLeader)
            {
                gapText.text = "LEADER";
                gapText.color = Color.white;
            }
            else if (gap > 0f)
            {
                gapText.text = $"+{gap:F3}s";
                gapText.color = new Color(1f, 0.3f, 0.3f);
            }
            else
            {
                gapText.text = "--";
                gapText.color = Color.gray;
            }
        }
    }
}
