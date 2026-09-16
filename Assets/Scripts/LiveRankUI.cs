using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LiveRankUI : MonoBehaviour
{
    [SerializeField] private List<RankingRowUI> uiRows;
    [SerializeField] private GameObject rankingPanel;

    private List<PlayerNetworkData> sortedPlayers = new List<PlayerNetworkData>();

    private void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient)
        {
            if (rankingPanel != null) rankingPanel.SetActive(false);
            return;
        }

        if (RaceManager.Instance == null || RaceManager.Instance.CurrentState.Value != RaceState.Racing)
        {
            if (rankingPanel != null) rankingPanel.SetActive(false);
            return;
        }

        if (rankingPanel != null) rankingPanel.SetActive(true);

        UpdateRankingsDisplay();
    }

    private void UpdateRankingsDisplay()
    {
        sortedPlayers.Clear();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                var data = client.PlayerObject.GetComponent<PlayerNetworkData>();
                if (data != null) sortedPlayers.Add(data);
            }
        }

        //Sort by rank
        sortedPlayers.Sort((a, b) => a.currentRank.Value.CompareTo(b.currentRank.Value));

        for (int i = 0; i < uiRows.Count; i++)
        {
            uiRows[i].gameObject.SetActive(false);
        }

        //Active players
        for (int i = 0; i < sortedPlayers.Count && i < uiRows.Count; i++)
        {
            var p = sortedPlayers[i];
            uiRows[i].gameObject.SetActive(true);

            bool isLeader = (p.currentRank.Value == 1);
            bool isLocalPlayer = p.IsOwner;

            uiRows[i].SetData(
                p.currentRank.Value,
                p.username.Value.ToString(),
                p.gapToLeaderOrAhead.Value,
                isLeader,
                isLocalPlayer
            );
        }
    }
}
