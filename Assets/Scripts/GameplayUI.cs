using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameplayUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI rankingText;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private TextMeshProUGUI finalRankText; 
    private PlayerNetworkData localPlayerData;
    private double frozenTime = -1;

    private void Update() {
        if (localPlayerData == null &&
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.LocalClient != null &&
            NetworkManager.Singleton.LocalClient.PlayerObject != null) {
            localPlayerData = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetworkData>();
            if (localPlayerData != null) {
                localPlayerData.lives.OnValueChanged += (_, newVal) => UpdateLivesText(newVal);
                UpdateLivesText(localPlayerData.lives.Value);
            }
        }

        UpdateRankingText();
        UpdateTotalTimeText();
        UpdateFinalRankText();
    }

    private void UpdateLivesText(int lives) {
        livesText.text = lives.ToString();
    }

    private void UpdateRankingText() {
        if (localPlayerData == null || NetworkManager.Singleton == null)
            return;

        var players = NetworkManager.Singleton.ConnectedClientsList
            .Where(c => c.PlayerObject != null)
            .Select(c => c.PlayerObject.GetComponent<PlayerNetworkData>())
            .Where(p => p != null)
            .OrderByDescending(p => p.currentCheckpointIndex.Value)
            .ToList();

        int myRank = players.IndexOf(localPlayerData) + 1;
        int totalPlayers = players.Count;

        rankingText.text = $"{myRank}/{totalPlayers}";

        if (localPlayerData.hasFinished.Value) {
            rankingText.text = $"{localPlayerData.finalRank.Value}/{totalPlayers}";
        }
    }

    private void UpdateTotalTimeText() {
        if (RaceManager.Instance == null || NetworkManager.Singleton == null)
            return;

        if (RaceManager.Instance.CurrentState.Value != RaceState.Racing) {
            totalTimeText.text = "0:00";
            return;
        }

        if (localPlayerData.hasFinished.Value) {
            if (frozenTime < 0) {
                frozenTime = NetworkManager.Singleton.ServerTime.Time - RaceManager.Instance.raceStartServerTime.Value;
            }
            DisplayTime(frozenTime);
            return;
        }

        double elapsed = NetworkManager.Singleton.ServerTime.Time - RaceManager.Instance.raceStartServerTime.Value;
        if (elapsed < 0) elapsed = 0;
        DisplayTime(elapsed);
    }

    private void DisplayTime(double elapsed) {
        int minutes = Mathf.FloorToInt((float)elapsed / 60f);
        int seconds = Mathf.FloorToInt((float)elapsed % 60f);
        totalTimeText.text = $"{minutes}:{seconds:D2}";
    }

    private void UpdateFinalRankText() {
        if (localPlayerData == null || finalRankText == null)
            return;

        if (localPlayerData.hasFinished.Value) {
            finalRankText.gameObject.SetActive(true);
            finalRankText.text = $"Finished! Rank: {localPlayerData.finalRank.Value}";
        } else {
            finalRankText.gameObject.SetActive(false);
        }
    }
}