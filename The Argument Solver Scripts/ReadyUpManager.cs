using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ReadyUpManager : NetworkBehaviour
{
    [SerializeField] GameObject button;

    GameManager gameMan;

    MenuMaster menuMaster;

    PlayerController[] players;

    int playersReadyAmount;

    bool hostReady, clientReady;

    void Awake()
    {
        menuMaster = FindObjectOfType<MenuMaster>();
    }

    public void PlayersJoined()
    {
        if (IsServer)
        {
            ActivateButtonClientRpc();
            ActivateButton();
        }
    }

    void ActivateButton()
    {
        gameMan = FindObjectOfType<GameManager>();

        button.SetActive(true);

        button.GetComponent<Image>().color = Color.red;

        menuMaster._chooseWeapon.gameObject.SetActive(true);
    }

    public void ButtonClicked()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        
        foreach(var p in players)
        {
            if (IsServer && p.CompareTag("Host"))
            {
                if(p.projectilePrefab == null)
                {
                    menuMaster._noWeaponSelection.gameObject.SetActive(true);
                    return;
                }
            }
            else if(!IsServer && p.CompareTag("Client"))
            {
                if (p.projectilePrefab == null)
                {
                    menuMaster._noWeaponSelection.gameObject.SetActive(true);
                    return;
                }
            }
        } //returns and doesn't allow player to ready up if they haven't selected a weapon

        if (button.GetComponent<Image>().color == Color.red)
        {
            button.GetComponent<Image>().color = Color.green;

            if (IsHost)
            {
                hostReady = true;
            }
            else
            {
                clientReady = true;
            }
        }
        else
        {
            button.GetComponent<Image>().color = Color.red;

            if (IsHost)
            {
                hostReady = false;
            }
            else
            {
                clientReady = false;
            }
        }

        CheckReadyStatus();
    }

    void CheckReadyStatus() //checking the ready status of the players
    {
        if (!IsServer)
        {
            if (!IsHost && clientReady)
            {
                playersReadyAmount++;
                ConfirmReadyServerRpc(1);
            }
            else if (!IsHost && !clientReady)
            {
                playersReadyAmount--;
                ConfirmReadyServerRpc(2);
            }

            return;
        }

        if (IsHost && hostReady)
        {
            ConfirmReadyClientRpc(1);
        }
        else if (IsHost && !hostReady)
        {
            ConfirmReadyClientRpc(2);     
        }
    }

    void ReadyUp(int picked)
    {
        if(picked == 1)
        {
            playersReadyAmount++;
        }
        else
        {
            playersReadyAmount--;
        }

        if (IsServer)
        {
            CheckPlayersReadyClientRpc();
        }
        else
        {
            CheckPlayersReady();
        }
    }

    void CheckPlayersReady() //checks to see if both players are ready, and activates them if so
    {
        if (playersReadyAmount == 2)
        {
            players = FindObjectsOfType<PlayerController>();

            foreach (var p in players)
            {
                p.playersReady = true;
            }

            button.SetActive(false);

            gameMan.menuCamera.SetActive(false);          

            menuMaster.playing = true;

            menuMaster.TurnOffTexts();

            menuMaster.SavePlayerData();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ConfirmReadyServerRpc(int picked)
    {
        ReadyUp(picked);
    }

    [ClientRpc]
    public void ActivateButtonClientRpc()
    {
        ActivateButton();
    }

    [ClientRpc]
    public void ConfirmReadyClientRpc(int picked)
    { 
        ReadyUp(picked);
    }

    [ClientRpc]
    public void CheckPlayersReadyClientRpc()
    {
        CheckPlayersReady();
    }
}
