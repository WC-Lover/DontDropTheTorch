using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class LobbyManager : MonoBehaviour
{
    private Lobby CurrentLobby;
    public bool InLobby { get => CurrentLobby != null; }
    public bool IsLobbyLeader { get; internal set; }
    public string LobbyCode { get; internal set; }
    public string KEY_START_GAME = "START_GAME";

    public static List<Transform> LobbyPlayersTransformsInludingLocal;

    public static List<Transform> LobbyPlayersTransforms;

    [SerializeField] private TMP_InputField lobbyCodeInput;
    [SerializeField] private TMP_InputField maxPlayersInput;

    private async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(CurrentLobby.MaxPlayers);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogWarning(e);
        }

        return "";
    }

    private async void JoinRelay(string relayCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            //QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            //{
            //    Count = 25,
            //    Filters = new List<QueryFilter>
            //    {
            //        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            //    },
            //    Order = new List<QueryOrder>
            //    {
            //        new QueryOrder(false, QueryOrder.FieldOptions.Created)
            //    }
            //};

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            JoinLobbyById(queryResponse.Results[0].Id);
            
        }
        catch (RelayServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    private async void JoinLobbyById(string LobbyId)
    {
        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(LobbyId);
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
                HandleLobbyPollForUpdates();
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

    public async void StartGame()
    {
        if (IsLobbyLeader)
        {
            try
            {
                string relayCode = await CreateRelay();

                if (relayCode.Length > 0)
                {
                    Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions { 
                        Data = new Dictionary<string, DataObject>
                        {
                            { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                        }
                    });

                    CurrentLobby = lobby;

                    BackgroundSFXSystem.Instance.StopBackgroundSFX();
                    BackgroundSFXSystem.Instance.StartInGameBackgroundRainSFX();
                    BackgroundSFXSystem.Instance.CrowsSFX();
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning(e);
            }
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        while (CurrentLobby != null)
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
            CurrentLobby = lobby;
            await Task.Delay(1000);
        }
    }

    public async void CreateLobbyAsync()
    {
        if (maxPlayersInput.text.Length == 0) return;
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {   KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync("TestLobby", int.Parse(maxPlayersInput.text), options);
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
                HandleLobbyPollForUpdates();
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
        if (lobbyCodeInput.text.Length <= 6) return;
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

        if (IsLobbyLeader) return;
        if (CurrentLobby.Data[KEY_START_GAME].Value != "0")
        {
            JoinRelay(CurrentLobby.Data[KEY_START_GAME].Value);
        }
    }

    async void HeartBeatLoop()
    {
        while (CurrentLobby != null)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            await Task.Delay(8000);
        }
    }

}
