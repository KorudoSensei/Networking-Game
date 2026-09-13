using UnityEngine;

public class Checkpoint : MonoBehaviour {
    public int checkpointIndex;

    private void OnTriggerEnter2D(Collider2D other) {
        var playerData = other.GetComponentInParent<PlayerNetworkData>();
        if (playerData == null) return;

        if (!playerData.IsOwner) return;

        playerData.ReportCheckpointServerRpc(checkpointIndex);
    }
}