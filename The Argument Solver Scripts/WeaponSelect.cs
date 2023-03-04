using UnityEngine;
using Unity.Netcode;

public class WeaponSelect : NetworkBehaviour
{
    [SerializeField] GameObject[] allWeapons, weaponPreviews;

    public GameObject leftArrow, rightArrow;

    PlayerData playerData;

    string weaponSelected;

    void Awake()
    {
        playerData = FindObjectOfType<PlayerData>();

        foreach(var w in allWeapons) //setting weapons to briefcases by default in case the players don't select anything
        {
            if(w.name == "Host Briefcase")
            {
                playerData.hostWeapon = w;
            }
            else if(w.name == "Client Briefcase")
            {
                playerData.clientWeapon = w;
            }
        }
    }

    public void ArrowClicked(string arrowHit)
    {
        if(arrowHit == "Right Arrow")
        {
            leftArrow.SetActive(true);
            rightArrow.SetActive(false);

            weaponPreviews[0].SetActive(false);
            weaponPreviews[1].SetActive(true);

            weaponSelected = "Gavel";
        }
        else
        {
            leftArrow.SetActive(false);
            rightArrow.SetActive(true);

            weaponPreviews[0].SetActive(true);
            weaponPreviews[1].SetActive(false);

            weaponSelected = "Briefcase";
        }

        SelectWeapon();
    }


    public void SelectWeapon() //the host selects the weapon for both the players
    {
        foreach (var w in allWeapons)
        {
            if (w.name.Contains(weaponSelected) && w.name.Contains("Host"))
            {
                playerData.hostWeapon = w;
            }
            else if(w.name.Contains(weaponSelected) && w.name.Contains("Client"))
            {
                playerData.clientWeapon = w;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (var p in players)
        {
            if (IsServer && p.CompareTag("Host"))
            {
                p.projectilePrefab = playerData.hostWeapon;
            }
        }

        if (!IsServer)
        {
            SelectWeaponServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectWeaponServerRpc()
    {
        SelectWeaponClientRpc(playerData.clientWeapon.name);
    }

    [ClientRpc]
    public void SelectWeaponClientRpc(string weaponName)
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (var p in players)
        {
            if (p.CompareTag("Client"))
            {
                foreach (var w in allWeapons)
                {
                    if (w.name.Contains(weaponName) && w.name.Contains("Client"))
                    {
                        p.projectilePrefab = w;
                    }
                }
            }
        }
    }
}
