using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour {
    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        // When Player Spawn Set Target
        if (IsOwner) {
            CameraFollow.Instance.SetTarget(transform);
        }
    }


    //private IEnumerator AssignCameraWhenReady() {
    //    while (CameraFollow.Instance == null) {
    //        yield return null;
    //    }

    //    CameraFollow.Instance.SetTarget(transform);
    //}
}





/*
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();

    if (IsOwner)
    {
        if (playerCameraGameObject != null)
        {
            playerCameraGameObject.SetActive(true);
            cameraTransform = playerCameraGameObject.transform;
            cameraTransform.position = transform.position + cameraOffset;
        }
    }
    else
    {
        // If it belongs to another player (e.g., Player 2 on Player 1's screen), keep it disabled
        if (playerCameraGameObject != null)
        {
            playerCameraGameObject.SetActive(false);
        }
    }
}

void LateUpdate() {
    if (!IsOwner || cameraTransform == null)
        return;

    Vector3 targetPosition = transform.position + cameraOffset;
    cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, followSpeed * Time.deltaTime);

    // Rotation intentionally NOT synced to the car — camera stays fixed looking straight down.
    // If you ever want the camera to rotate with the car, replace the line above's absence with:
    // cameraTransform.rotation = transform.rotation;
}
}
*/
