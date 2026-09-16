using UnityEngine;

public class Checkpoint : MonoBehaviour {
    public int checkpointIndex;
    public bool isFinishLine = false; // 起点/终点专用勾选

    private void OnTriggerEnter2D(Collider2D other) {
        var playerData = other.GetComponentInParent<PlayerNetworkData>();
        if (playerData == null) return;

        if (!playerData.IsOwner) return;

        if (isFinishLine) {
            playerData.ReportFinishLineServerRpc();
        } else {
            playerData.ReportCheckpointServerRpc(checkpointIndex);
        }
    }
}