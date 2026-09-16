using System.Collections;
using Unity.Netcode;
using UnityEngine;

public enum RaceState {
    Waiting,
    Countdown,
    Racing,
    Finished,
    Results
}

public class RaceManager : NetworkBehaviour {
    public static RaceManager Instance;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints; 

    [Header("Lobby Settings")]
    [SerializeField] private int minPlayers = 1;   
    [SerializeField] public int maxPlayers = 4;
    [SerializeField] private float normalWaitTime = 60f;
    [SerializeField] private float readyWaitTime = 10f;

    public NetworkVariable<int> ConnectedPlayerCount = new NetworkVariable<int>(0);
    public NetworkVariable<int> ReadyPlayerCount = new NetworkVariable<int>(0);
    public NetworkVariable<float> TimeRemaining = new NetworkVariable<float>(0);
    public NetworkVariable<RaceState> CurrentState = new NetworkVariable<RaceState>(RaceState.Waiting);

    // For Counting Number
    public NetworkVariable<int> CountdownNumber = new NetworkVariable<int>(3);

    // Spawn Point Position Index
    private int nextSpawnIndex = 0;

    private void Awake() {
        Instance = this;
    }

    public override void OnNetworkSpawn() {
        if (IsServer) {
            TimeRemaining.Value = normalWaitTime;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public override void OnNetworkDespawn() {
        if (IsServer && NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    // ----- Position Index -----
    public int GetNextSpawnIndex() {
        int index = Mathf.Min(nextSpawnIndex, spawnPoints.Length - 1);
        nextSpawnIndex++;
        return index;
    }

    public Transform GetSpawnPointByIndex(int index) {
        if (spawnPoints == null || index < 0 || index >= spawnPoints.Length)
            return null;
        return spawnPoints[index];
    }

    // ----- Participant Count/Ready Status -----
    private void OnClientConnected(ulong clientId) {
        if (!IsServer) return;
        ConnectedPlayerCount.Value = NetworkManager.Singleton.ConnectedClientsList.Count;

        // Waiting timer reset when new player joins, unless is full
        if (CurrentState.Value == RaceState.Waiting && ConnectedPlayerCount.Value < maxPlayers) {
            TimeRemaining.Value = normalWaitTime;
        }
    }

    // I haven't test this function ^=^
    private void OnClientDisconnected(ulong clientId) {
        if (!IsServer) return;
        ConnectedPlayerCount.Value = Mathf.Max(0, NetworkManager.Singleton.ConnectedClientsList.Count - 1);
        RecalculateReadyCount();
    }

    // Count Ready Player
    public void RecalculateReadyCount() {
        if (!IsServer) return;

        int count = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList) {
            if (client.PlayerObject != null) {
                var data = client.PlayerObject.GetComponent<PlayerNetworkData>();
                if (data != null && data.isReady.Value)
                    count++;
            }
        }
        ReadyPlayerCount.Value = count;
    }

    // ----- Timer -----
    private void Update() {
        if (!IsServer) return;
        if (CurrentState.Value != RaceState.Waiting) return;

        TimeRemaining.Value -= Time.deltaTime;

        bool allReady = ConnectedPlayerCount.Value > 0 &&
                         ReadyPlayerCount.Value == ConnectedPlayerCount.Value;

        if (allReady) {
            // if all ready timer change to 10s
            // If it is already less than 10 seconds, it remains unchanged.
            TimeRemaining.Value = Mathf.Min(TimeRemaining.Value, readyWaitTime);
        }

        bool timeUp = TimeRemaining.Value <= 0f;
        bool roomFull = ConnectedPlayerCount.Value >= maxPlayers;

        // if Room Full or All Player ready start Counting 3 2 1 
        if ((timeUp || roomFull) && ConnectedPlayerCount.Value >= minPlayers) {
            TimeRemaining.Value = 0f;
            CurrentState.Value = RaceState.Countdown;
            StartCoroutine(RunCountdown());
        }
    }

    private IEnumerator RunCountdown() {
        // CountdownNumber for WaitingRoomUi text Update
        CountdownNumber.Value = 3;
        yield return new WaitForSeconds(1f);
        CountdownNumber.Value = 2;
        yield return new WaitForSeconds(1f);
        CountdownNumber.Value = 1;
        yield return new WaitForSeconds(1f);
        CountdownNumber.Value = 0; 
        yield return new WaitForSeconds(1f);

        CurrentState.Value = RaceState.Racing;
    }
}