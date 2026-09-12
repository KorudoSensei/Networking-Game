using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public static CameraFollow Instance;

    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); 
    [SerializeField] private float followSpeed = 10f;

    private Transform target;

    void Awake() {
        Instance = this;
    }

    public void SetTarget(Transform newTarget) {
        target = newTarget;
        if (target != null)
            transform.position = target.position + offset;
    }

    void FixedUpdate() {
        if (target == null)
            return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}