using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugTest : NetworkBehaviour {
    private PlayerNetworkData playerData;

    public override void OnNetworkSpawn() {
        playerData = GetComponent<PlayerNetworkData>();
    }

    void Update() {
        if (!IsOwner) return;

        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame) {
            Debug.Log("Take Damage");
            playerData.TakeDamageServerRpc();
        }
    }
}