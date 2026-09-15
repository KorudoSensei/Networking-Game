using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameplayHUD : NetworkBehaviour {

    [Header("Health")]
    [SerializeField] private GameObject healthBar;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Rank")]
    [SerializeField] private GameObject rankPanel;
    [SerializeField] private TextMeshProUGUI rankText;

    [Header("Timer")]
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TextMeshProUGUI timerText;

    private NetworkVariable<float> raceTime = new NetworkVariable<float>(0f);

    private PlayerNetworkData localPlayerData;
    private bool isRacing = false;

    private void Start() {
        SetAllActive(false);
        StartCoroutine(WaitForRaceManager());
    }

    private IEnumerator WaitForRaceManager() {
        while (RaceManager.Instance == null)
            yield return null;

        RaceManager.Instance.CurrentState.OnValueChanged += OnStateChanged;

        OnStateChanged(RaceState.Waiting, RaceManager.Instance.CurrentState.Value);
    }

    private void OnStateChanged(RaceState oldState, RaceState newState) {
        isRacing = newState == RaceState.Racing;
        SetAllActive(isRacing);

        if (IsServer && isRacing && oldState != RaceState.Racing) {
            raceTime.Value = 0f;
        }
    }

    private void SetAllActive(bool active) {
        if (healthBar != null) healthBar.SetActive(active);
        if (rankPanel != null) rankPanel.SetActive(active);
        if (timerPanel != null) timerPanel.SetActive(active);
    }

    private void Update() {
        if (!isRacing) return;

        if (IsServer) {
            raceTime.Value += Time.deltaTime;
        }

        UpdateHealth();
        UpdateRank();
        UpdateTimer();
    }

    private void UpdateHealth() {
        if (localPlayerData == null) {
            if (NetworkManager.Singleton != null &&
                NetworkManager.Singleton.LocalClient != null &&
                NetworkManager.Singleton.LocalClient.PlayerObject != null) {
                localPlayerData = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetworkData>();
            }
            if (localPlayerData == null) return;
        }

        if (healthText != null)
            healthText.text = localPlayerData.lives.Value.ToString();
    }

    private void UpdateRank() {
        if (NetworkManager.Singleton == null || localPlayerData == null) return;

        var players = NetworkManager.Singleton.ConnectedClientsList
            .Where(c => c.PlayerObject != null)
            .Select(c => c.PlayerObject.GetComponent<PlayerNetworkData>())
            .Where(p => p != null)
            .OrderByDescending(p => p.currentCheckpointIndex.Value)
            .ToList();

        int myRank = players.FindIndex(p => p == localPlayerData) + 1;

        if (rankText != null)
            rankText.text = $"{myRank} / {players.Count}";
    }

    private void UpdateTimer() {
        float t = raceTime.Value;
        int minutes = Mathf.FloorToInt(t / 60f);
        float seconds = t % 60f;

        if (timerText != null)
            timerText.text = $"{minutes:00}:{seconds:00.0}";
    }
}