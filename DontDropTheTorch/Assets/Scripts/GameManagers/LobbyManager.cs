using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private Lobby CurrentLobby;
    public static int playersExpected = 0;

    public bool InLobby { get => CurrentLobby != null; }

    public async void CreateLobbyAsync()
    {
        try
        {
            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync("TestLobby", 4);

            var callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnLobbyChanged;
            try
            {
                var m_LobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);
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

    public async void QuickJoinLobbyAsync()
    {
        try
        {
            CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            var callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnLobbyChanged;
            try
            {
                var m_LobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);
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
            await SendHeartbeatPingAsync();
            await Task.Delay(8000);
        }
    }

    async Task SendHeartbeatPingAsync()
    {
        if (!InLobby)
            return;

        await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
    }

}
