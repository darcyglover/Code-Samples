using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : NetworkBehaviour
{
    public GameObject wrongCode;

    GameManager gameMan;

    PlayerData playerData;

    [HideInInspector] public string joinCode;

    UnityTransport transport;
    const int MaxPlayers = 2;

    ulong clientId;

    async void Awake()
    {
        transport = FindObjectOfType<UnityTransport>();

        await Authenticate();

        gameMan = FindObjectOfType<GameManager>();

        playerData = FindObjectOfType<PlayerData>();
    }

    public void HostOrJoin(int picked, string joinCodeSent)
    {
        if(picked == 1)
        {
            HostGame();
        }
        else
        {
            JoinGame(joinCodeSent);
        }
    }

    static async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    async void HostGame()
    {
        Allocation a = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
        joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        gameMan.SetJoinCode();
        playerData.lobbyCode = joinCode;

        transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        NetworkManager.Singleton.StartHost();
    }

    async void JoinGame(string sentJoinCode)
    {
        JoinAllocation a;

        try
        {
             a = await RelayService.Instance.JoinAllocationAsync(sentJoinCode);
        }
        catch
        {
            wrongCode.SetActive(true);

            return;
        }

        transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        gameMan.ClientJoined();

        NetworkManager.Singleton.StartClient();
    }

    public void EndSession()
    {
        if (!IsServer)
        {
            EndSessionServerRpc();

            return;
        }

        if(IsServer && NetworkManager.ConnectedClientsList.Count > 0)
        {
            for(int i= MaxPlayers; i < NetworkManager.ConnectedClientsList.Count; i--)
            {
                NetworkManager.Singleton.DisconnectClient((ulong)i);
            }
        }

        NetworkManager.Singleton.Shutdown();
    }

    public void DisconnectClientOnly(ulong clientId)
    {
        if (!IsServer)
        {
            DisconnectClientOnlyServerRpc(clientId);
        }
        else
        {
            NetworkManager.Singleton.DisconnectClient(clientId); 
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndSessionServerRpc()
    {
        EndSession();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisconnectClientOnlyServerRpc(ulong clientId)
    {
        DisconnectClientOnly(clientId);
    }
}
