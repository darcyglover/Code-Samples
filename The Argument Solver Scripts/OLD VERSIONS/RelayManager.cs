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
    [SerializeField] GameObject wrongCode;

    GameManager gameMan;

    PlayerData playerData;

    [HideInInspector] public string joinCode;

    UnityTransport transport;
    const int MaxPlayers = 2;

    async void Awake()
    {
        transport = FindObjectOfType<UnityTransport>();

        await Authenticate();

        gameMan = FindObjectOfType<GameManager>();

        playerData = FindObjectOfType<PlayerData>();
    }

    public void CreateOrJoin(int picked, string joinCodeSent)
    {
        if(picked == 1)
        {
            CreateGame();
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

    async void CreateGame()
    {
        Allocation a = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
        joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        playerData.lobbyCode = joinCode;

        transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        NetworkManager.Singleton.StartHost();
    }

    async void JoinGame(string sentJoinCode)
    {
        wrongCode.SetActive(false);

        JoinAllocation a;

        try
        {
             a = await RelayService.Instance.JoinAllocationAsync(sentJoinCode);
        }
        catch
        {
            wrongCode.SetActive(true);

            gameMan.JoinGameSetup();

            return;
        }

        transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        NetworkManager.Singleton.StartClient();
    }

    public void EndSession(bool quitting)
    {
        if (!IsServer)
        {
            EndSessionServerRpc(quitting);

            return;
        }

        NetworkManager.Singleton.DisconnectClient(1);

        NetworkManager.Singleton.DisconnectClient(0);

        NetworkManager.Singleton.Shutdown();
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndSessionServerRpc(bool quitting)
    {
        NetworkManager.Singleton.DisconnectClient(1);

        NetworkManager.Singleton.DisconnectClient(0);

        NetworkManager.Singleton.Shutdown();
    }
}
