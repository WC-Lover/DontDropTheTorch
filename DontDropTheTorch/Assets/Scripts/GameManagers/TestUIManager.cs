using UnityEngine;
using Unity.Netcode;

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
        if (lobbyManager.InLobby || networkManager.IsClient || networkManager.IsHost) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        LobbyUI();
        GUILayout.EndArea();
    }

    void LobbyUI()
    {
        if (GUILayout.Button("Create Lobby")) lobbyManager.CreateLobbyAsync();
        if (GUILayout.Button("Connect to Lobby")) lobbyManager.QuickJoinLobbyAsync();
        if (GUILayout.Button("Leave Lobby")) lobbyManager.LeaveLobbyAsync();
        if (GUILayout.Button("Host")) GetComponent<NetworkManager>().StartHost();
        if (GUILayout.Button("Client")) GetComponent<NetworkManager>().StartClient();
    }

}
