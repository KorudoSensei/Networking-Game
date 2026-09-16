using System.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

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

    // Race Time
    public NetworkVariable<double> raceStartServerTime = new NetworkVariable<double>(0);

    private int finishedCount = 0;

    [Header("Car Prefabs")]
    [SerializeField] private GameObject[] carPrefabs; 

    private List<PlayerNetworkData> activePlayers = new List<PlayerNetworkData>();

    [Header("Audio SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip countdownSFX;
    [SerializeField] private AudioClip finishLineSFX;

    public void ConfigureConnectionApproval() {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
                                NetworkManager.ConnectionApprovalResponse response) {
        response.Approved = true;
        response.CreatePlayerObject = false; // 不用默认方式生成，我们自己手动生成正确的 car prefab
        response.Pending = false;
    }

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

        if (CurrentState.Value == RaceState.Waiting && ConnectedPlayerCount.Value < maxPlayers) {
            TimeRemaining.Value = normalWaitTime;
        }

        SpawnPlayerCar(clientId);
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

        //calculate for live ui
        if (CurrentState.Value == RaceState.Racing) 
        {
        CalculatePositionsAndGaps();
        return;
        }

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

        PlayCountdownSoundClientRpc();

        // CountdownNumber for WaitingRoomUi text Update
        CountdownNumber.Value = 3;
        yield return new WaitForSeconds(1f);
        CountdownNumber.Value = 2;
        yield return new WaitForSeconds(1f);
        CountdownNumber.Value = 1;
        yield return new WaitForSeconds(1f);
        CountdownNumber.Value = 0; 
        yield return new WaitForSeconds(1f);

        raceStartServerTime.Value = NetworkManager.Singleton.ServerTime.Time; 
        CurrentState.Value = RaceState.Racing;
    }
    
    [ClientRpc]
    private void PlayCountdownSoundClientRpc() 
    {
        audioSource.PlayOneShot(countdownSFX);
    }

    [ClientRpc]
    private void PlayFinishSoundClientRpc(ulong finisherClientId) 
    {
        if (NetworkManager.Singleton.LocalClientId == finisherClientId) 
        {
            audioSource.PlayOneShot(finishLineSFX);
        }
    }   

    private void SpawnPlayerCar(ulong clientId) {
        int index = GetNextSpawnIndex();
        GameObject prefabToSpawn = carPrefabs[Mathf.Min(index, carPrefabs.Length - 1)];

        GameObject carInstance = Instantiate(prefabToSpawn);

        var playerData = carInstance.GetComponent<PlayerNetworkData>();
        if (playerData != null) {
            playerData.assignedSpawnIndex.Value = index;
        }

        var netObj = carInstance.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId);
    }

    public int RegisterFinish(PlayerNetworkData player) {
        finishedCount++;
        PlayFinishSoundClientRpc(player.OwnerClientId);

        return finishedCount; // 谁先调用这个方法，谁就拿到较小的名次数字
    }

    private void CalculatePositionsAndGaps() 
    {
        activePlayers.Clear();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList) {
            if (client.PlayerObject != null) {
                var data = client.PlayerObject.GetComponent<PlayerNetworkData>();
                if (data != null) activePlayers.Add(data);
            }
        }

        //Sort based on checkpoint index and time
        activePlayers.Sort((a, b) => {
            if (a.hasFinished.Value && b.hasFinished.Value)
                return a.finalRank.Value.CompareTo(b.finalRank.Value);
            if (a.hasFinished.Value) return -1;
            if (b.hasFinished.Value) return 1;

            //Higher checkpoint index comes first
            if (a.currentCheckpointIndex.Value != b.currentCheckpointIndex.Value) {
                return b.currentCheckpointIndex.Value.CompareTo(a.currentCheckpointIndex.Value);
            }

            //Same checkpoint then earlier timestamp comes first
            int cp = a.currentCheckpointIndex.Value;
            if (cp > 0 && a.CheckpointTimes != null && b.CheckpointTimes != null) {
                return a.CheckpointTimes[cp].CompareTo(b.CheckpointTimes[cp]);
            }

            return 0;
        });

        //rankings and gaps
        for (int i = 0; i < activePlayers.Count; i++) {
            activePlayers[i].currentRank.Value = i + 1;

            if (i == 0) {
                activePlayers[i].gapToLeaderOrAhead.Value = 0f;
            } else {
                PlayerNetworkData leader = activePlayers[0];
                PlayerNetworkData current = activePlayers[i];

                int cp = current.currentCheckpointIndex.Value;

                if (cp > 0 && leader.CheckpointTimes != null && current.CheckpointTimes != null && 
                    leader.CheckpointTimes[cp] > 0 && current.CheckpointTimes[cp] > 0) 
                {
                    float gap = current.CheckpointTimes[cp] - leader.CheckpointTimes[cp];
                    current.gapToLeaderOrAhead.Value = Mathf.Max(0f, gap);
                } else {
                    current.gapToLeaderOrAhead.Value = 0f;
                }
            }
        }
    }
}