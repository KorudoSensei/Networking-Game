using UnityEngine;

public class CheckpointManager : MonoBehaviour {
    public static CheckpointManager Instance;

    [SerializeField] private Transform[] checkpoints;

    public int LastCheckpointIndex => checkpoints.Length - 1;

    private void Awake() {
        Instance = this;
        AssignIndexesInOrder();
    }

    private void AssignIndexesInOrder() {
        for (int i = 0; i < checkpoints.Length; i++) {
            var checkpoint = checkpoints[i].GetComponent<Checkpoint>();
            if (checkpoint != null) {
                checkpoint.checkpointIndex = i;
            }
        }
    }

    public Transform GetCheckpointTransform(int index) {
        if (checkpoints == null || index < 0 || index >= checkpoints.Length)
            return null;
        return checkpoints[index];
    }
}