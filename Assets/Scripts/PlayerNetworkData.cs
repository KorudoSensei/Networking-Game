using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using Unity.Netcode.Components;

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
}
