using UnityEngine;
using Unity.Netcode;
using System;

public class TestUIManager : MonoBehaviour
{
    LobbyManager lobbyManager;
    NetworkManager networkManager;
    void Start()
    {
        lobbyManager = GetComponent<LobbyManager>();
        networkManager = GetComponent<NetworkManager>();
    }

    void OnGUI()
    {
        if (networkManager.IsClient || networkManager.IsHost) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (lobbyManager.InLobby)
        {
            LobbyUI();
        }
        else
        {
            GameUI();
        }

        GUILayout.EndArea();
    }

    void LobbyUI()
    {
        if (lobbyManager.IsLobbyLeader)
        {
            if (GUILayout.Button($"{lobbyManager.LobbyCode}")) Debug.Log($"{lobbyManager.LobbyCode}");
        }

        if (GUILayout.Button("Leave Lobby")) lobbyManager.LeaveLobbyAsync();
    }

    void GameUI()
    {
        if (GUILayout.Button("Create Lobby")) lobbyManager.CreateLobbyAsync();
        if (GUILayout.Button("Connect to Lobby")) lobbyManager.ConnectToLobbyAsync();
        
    }

}
