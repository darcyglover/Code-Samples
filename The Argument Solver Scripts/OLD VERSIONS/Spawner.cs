using UnityEngine;
using Unity.Netcode;

public class Spawner : NetworkBehaviour
{
    [SerializeField] GameObject hostPrefab, clientPrefab, readyUpManagerPrefab;

    [SerializeField] Transform hostSpawnPoint, clientSpawnPoint;

    NetworkObject netObj;

    [ServerRpc(RequireOwnership = false)] 
    public void SpawnPlayerServerRpc(ulong clientId, int prefabId)
    {
        GameObject newPlayer;

        if (prefabId == 0)
        {
            newPlayer = Instantiate(hostPrefab, hostSpawnPoint);
        }
        else
        {
            newPlayer = Instantiate(clientPrefab, clientSpawnPoint);
        }

        netObj = newPlayer.GetComponent<NetworkObject>();

        newPlayer.SetActive(true);

        netObj.SpawnAsPlayerObject(clientId, true);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, 0);
        }
        else
        {
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, 1);
        }
    }
}
