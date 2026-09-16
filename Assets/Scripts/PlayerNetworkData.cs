using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using Unity.Netcode.Components;
using System.Collections;
using System;

public class PlayerNetworkData : NetworkBehaviour
{
    // Player Name
    public NetworkVariable<FixedString64Bytes> username = new NetworkVariable<FixedString64Bytes>
    ("Unassigned", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private Text nameDisplayText;

    // Player Spawn Point
    public NetworkVariable<int> assignedSpawnIndex = new NetworkVariable<int>
    (-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Player Ready Status
    public NetworkVariable<bool> isReady = new NetworkVariable<bool>
    (false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Player CheckPoint Index
    public NetworkVariable<int> currentCheckpointIndex = new NetworkVariable<int>
        (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Player Lives
    public NetworkVariable<int> lives = new NetworkVariable<int>
        (3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool isInvulnerable = false; 
    [SerializeField] private float InvincibileTime = 1.5f; 

    // Player Death
    public NetworkVariable<int> deathCount = new NetworkVariable<int>
    (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    [ServerRpc]
    // ServerRpc mean this function can be called by the client, the actual code executes on the server.   
    // Client Join Room Click Ready Button
    // Sent to Server
    public void ToggleReadyServerRpc() {
        isReady.Value = !isReady.Value;

        if (RaceManager.Instance != null)
            RaceManager.Instance.RecalculateReadyCount();
    }

    public override void OnNetworkSpawn()
    {
        username.OnValueChanged += OnNameChanged;
        UpdateNameDisplay(username.Value.ToString());

        // Get Position Index (0/1/2/3)
        if (IsServer) {
            assignedSpawnIndex.Value = RaceManager.Instance.GetNextSpawnIndex();
            Debug.Log($"[Server] OwnerClientId={OwnerClientId} assign index: {assignedSpawnIndex.Value}");
        }

        // Move to Spawn Point
        if (IsOwner)
        {
            string typedName = NetworkUiManager.EnteredUsername;
            SetUsernameServerRpc(typedName);

            MoveToAssignedSpawnPoint();
        }
    }

    public override void OnNetworkDespawn()
    {
        username.OnValueChanged -= OnNameChanged;
    }
    private void UpdateNameDisplay(string name)
    {
        if (nameDisplayText != null)
        {
            nameDisplayText.text = name;
        }
    }

    private void OnNameChanged(FixedString64Bytes oldval, FixedString64Bytes newval)
    {
        UpdateNameDisplay(newval.ToString());
    }

    [ServerRpc]
    private void SetUsernameServerRpc(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            username.Value = name.Trim();
        }
    }

    private void MoveToAssignedSpawnPoint() {
        int index = assignedSpawnIndex.Value;
        if (index < 0) return;

        Transform spawnPoint = RaceManager.Instance.GetSpawnPointByIndex(index);
        if (spawnPoint == null) return;

        transform.position = spawnPoint.position;
    }

    // ----- CheckPoint -----
    [ServerRpc]
    public void ReportCheckpointServerRpc(int index) {
        if (index > currentCheckpointIndex.Value)
            currentCheckpointIndex.Value = index;
        Debug.Log(currentCheckpointIndex.Value);
    }

    // ----- Take Damage -----
    [ServerRpc]
    public void TakeDamageServerRpc() {
        if (isInvulnerable) return;
        if (lives.Value <= 0) return;

        lives.Value -= 1;

        if (lives.Value <= 0) {
            lives.Value = 3;
            deathCount.Value += 1;   

            var rpcParams = new ClientRpcParams {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { OwnerClientId } }
            };
            RespawnClientRpc(currentCheckpointIndex.Value, rpcParams);

            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    private IEnumerator InvulnerabilityRoutine() {
        isInvulnerable = true;
        yield return new WaitForSeconds(InvincibileTime); 
        isInvulnerable = false;
    }

    [ClientRpc]
    private void RespawnClientRpc(int checkpointIndex, ClientRpcParams rpcParams = default) {
        Transform cp = CheckpointManager.Instance.GetCheckpointTransform(checkpointIndex);
        if (cp == null) return;

        transform.position = cp.position;
        transform.rotation = cp.rotation;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
