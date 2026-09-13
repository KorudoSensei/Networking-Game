using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameplayUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI livesText;

    private PlayerNetworkData localPlayerData;

    private void Update() {
        if (localPlayerData == null &&
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.LocalClient != null &&
            NetworkManager.Singleton.LocalClient.PlayerObject != null) {
            localPlayerData = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetworkData>();
            if (localPlayerData != null) {
                localPlayerData.lives.OnValueChanged += (_, newVal) => UpdateLivesText(newVal);
                UpdateLivesText(localPlayerData.lives.Value);
            }
        }
    }

    private void UpdateLivesText(int lives) {
        livesText.text = lives.ToString();
    }
}