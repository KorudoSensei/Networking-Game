using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomUI : MonoBehaviour {
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private TextMeshProUGUI waitingText;   
    [SerializeField] private TextMeshProUGUI timerText;     
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI readyButtonText;

    [Header("Countdown Panel")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Gameplay Panel")]
    [SerializeField] private GameObject HealthPanel;
    [SerializeField] private GameObject livesText;
    [SerializeField] private GameObject TimeText;
    [SerializeField] private GameObject RankText;
    [SerializeField] private GameObject liveUIPanel;


    private PlayerNetworkData localPlayerData;

    private void Start() {
        readyButton.onClick.AddListener(OnReadyClicked);
        StartCoroutine(InitWhenRaceManagerReady());
    }

    private IEnumerator InitWhenRaceManagerReady() {
        while (RaceManager.Instance == null)
            yield return null;

        RaceManager.Instance.ConnectedPlayerCount.OnValueChanged += (_, _) => UpdateWaitingText();
        RaceManager.Instance.TimeRemaining.OnValueChanged += (_, _) => UpdateTimerText();
        RaceManager.Instance.CurrentState.OnValueChanged += (_, newState) => OnRaceStateChanged(newState);
        RaceManager.Instance.CountdownNumber.OnValueChanged += (_, newVal) => UpdateCountdownText(newVal);

        UpdateWaitingText();
        UpdateTimerText();
        UpdateCountdownText(RaceManager.Instance.CountdownNumber.Value);
    }

    private void Update() {
        if (localPlayerData == null &&
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.LocalClient != null &&
            NetworkManager.Singleton.LocalClient.PlayerObject != null) {
            localPlayerData = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetworkData>();
            if (localPlayerData != null)
                localPlayerData.isReady.OnValueChanged += (_, newVal) => UpdateReadyButtonText(newVal);
        }
    }

    private void OnReadyClicked() {
        localPlayerData?.ToggleReadyServerRpc();
    }

    private void UpdateWaitingText() {
        waitingText.text = $"Wait Other Player Join({RaceManager.Instance.ConnectedPlayerCount.Value}/{RaceManager.Instance.maxPlayers})";
    }

    private void UpdateTimerText() {
        int seconds = Mathf.CeilToInt(RaceManager.Instance.TimeRemaining.Value);
        timerText.text = $"After {seconds}s Start";
    }

    private void UpdateReadyButtonText(bool isReady) {
        readyButtonText.text = isReady ? "Im Ready" : "Ready";
    }

    private void OnRaceStateChanged(RaceState newState) {
        Debug.Log($"[WaitingRoomUI] Received status change: {newState}");
        if (newState == RaceState.Countdown) {
            waitingPanel.SetActive(false);
            countdownPanel.SetActive(true);
            //livesText.SetActive(false);
            TimeText.SetActive(false);
            RankText.SetActive(false);
            liveUIPanel.SetActive(false);
        }

        if (newState == RaceState.Racing) {
            countdownPanel?.SetActive(false);
            HealthPanel.SetActive(true);
            //livesText.SetActive(true);
            TimeText.SetActive(true);
            RankText.SetActive(true);
            liveUIPanel.SetActive(true);
        }
    }

    private void UpdateCountdownText(int number) {
        countdownText.text = number > 0 ? number.ToString() : "Start!";
    }
}