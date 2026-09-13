using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScoreboardUI : MonoBehaviour {
    [SerializeField] private GameObject scoreboardRoot;   
    [SerializeField] private GameObject rowPrefab;        
    [SerializeField] private Transform rowParent;         

    private List<Scoreboard> activeRows = new List<Scoreboard>();

    private void Start() {
        scoreboardRoot.SetActive(false);
    }

    private void Update() {
        bool tabHeld = Keyboard.current != null && Keyboard.current.tabKey.isPressed;

        if (tabHeld != scoreboardRoot.activeSelf) {
            scoreboardRoot.SetActive(tabHeld);
        }

        if (tabHeld) {
            RefreshScoreboard();
        }
    }

    private void RefreshScoreboard() {
        if (NetworkManager.Singleton == null) return;

        var players = NetworkManager.Singleton.ConnectedClientsList
            .Where(c => c.PlayerObject != null)
            .Select(c => c.PlayerObject.GetComponent<PlayerNetworkData>())
            .Where(p => p != null)
            .OrderByDescending(p => p.currentCheckpointIndex.Value)
            .ToList();

        while (activeRows.Count < players.Count) {
            var row = Instantiate(rowPrefab, rowParent);
            activeRows.Add(row.GetComponent<Scoreboard>());
        }

        for (int i = 0; i < activeRows.Count; i++) {
            if (i < players.Count) {
                activeRows[i].gameObject.SetActive(true);
                var p = players[i];
                activeRows[i].SetData(
                    p.username.Value.ToString(),
                    p.currentCheckpointIndex.Value,
                    p.lives.Value,
                    p.deathCount.Value
                );
            } else {
                activeRows[i].gameObject.SetActive(false);
            }
        }
    }
}