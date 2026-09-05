using Unity.Netcode;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
    [SerializeField] private GameObject playerCameraGameObject;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            if (playerCameraGameObject != null)
            {
                playerCameraGameObject.SetActive(true);
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
}
