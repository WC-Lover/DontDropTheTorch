using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private NetworkManager networkManager;
    private Lobby CurrentLobby;

    private bool InGame;
    public bool InLobby { get => CurrentLobby != null; }
    public bool IsLobbyLeader { get; internal set; }
    //public bool IsLobbyFull { get => CurrentLobby.MaxPlayers == CurrentLobby.Players.Count; }
    public string LobbyCode { get; internal set; }

    public static List<Transform> LobbyPlayersTransformsInludingLocal;

    public static List<Transform> LobbyPlayersTransforms;

    [SerializeField] private TMP_InputField lobbyCodeInput;
    [SerializeField] private TMP_InputField maxPlayersInput;

    void Update()
    {
        if (InGame) return;
        if (InLobby && CurrentLobby.AvailableSlots == 0)
        {
            if (IsLobbyLeader) NetworkManager.Singleton.StartHost();
            else NetworkManager.Singleton.StartClient();
            InGame = true;
        }
    }

    public async void CreateLobbyAsync()
    {
        if (maxPlayersInput.text.Length == 0) return;
        try
        {
            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync("TestLobby", int.Parse(maxPlayersInput.text));
            LobbyPlayersTransformsInludingLocal = new List<Transform>(CurrentLobby.MaxPlayers);
            LobbyPlayersTransforms = new List<Transform>(CurrentLobby.MaxPlayers - 1);

            var callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnLobbyChanged;
            try
            {
                var m_LobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);
                IsLobbyLeader = true;
                LobbyCode = CurrentLobby.LobbyCode;
                lobbyCodeInput.gameObject.SetActive(false);
                maxPlayersInput.gameObject.SetActive(false);
                HeartBeatLoop();
            }
            catch (LobbyServiceException ex)
            {
                switch (ex.Reason)
                {
                    case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.Log($"Already subscribed to lobby[{CurrentLobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
                    case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.Log($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
                    case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.Log($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
                    default: throw;
                }
            }
        }
        catch (LobbyServiceException lobbyException)
        {
            Debug.LogWarning(lobbyException);
        }
    }

    public async void ConnectToLobbyAsync()
    {
        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCodeInput.text);
            LobbyPlayersTransformsInludingLocal = new List<Transform>(CurrentLobby.MaxPlayers);
            LobbyPlayersTransforms = new List<Transform>(CurrentLobby.MaxPlayers - 1);

            var callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnLobbyChanged;
            try
            {
                var m_LobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);
                IsLobbyLeader = false;
                lobbyCodeInput.gameObject.SetActive(false);
                maxPlayersInput.gameObject.SetActive(false);
            }
            catch (LobbyServiceException ex)
            {
                switch (ex.Reason)
                {
                    case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.Log($"Already subscribed to lobby[{CurrentLobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
                    case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.Log($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
                    case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.Log($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
                    default: throw;
                }
            }
        }
        catch (LobbyServiceException lobbyException)
        {
            Debug.LogWarning(lobbyException);
        }
    }

    public async void LeaveLobbyAsync()

    {
        if (!InLobby) return;

        string playerId = AuthenticationService.Instance.PlayerId;
        await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
        CurrentLobby = null;
    }

    private void OnLobbyChanged(ILobbyChanges obj)
    {
        obj.ApplyToLobby(CurrentLobby);
    }

    async void HeartBeatLoop()
    {
        while (CurrentLobby != null)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);//SendHeartbeatPingAsync();
            await Task.Delay(8000);
        }
    }

}
