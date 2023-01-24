using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using MText;

public class GameManager : NetworkBehaviour
{
    public Modular3DText joinCodeText;
    [SerializeField] MText_UI_InputField joinCodeInput;

    public GameObject menuCamera;
    [SerializeField] GameObject joinGameButton;
    [SerializeField] GameObject[] weapons;
    
    bool playersJoined;

    ReadyUpManager readyMan;

    RelayManager relaySetup;

    PlayerData playerData;

    void Awake()
    {
        relaySetup = FindObjectOfType<RelayManager>();

        readyMan = FindObjectOfType<ReadyUpManager>();

        playerData = FindObjectOfType<PlayerData>();
    }

    void Update()
    {
        if(joinCodeInput.Text.Length == 6)
        {
            joinGameButton.GetComponent<BoxCollider>().enabled = true;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                joinGameButton.GetComponent<BoxCollider>().enabled = false;

                JoinGame();
            }
        }
        else
        {
            joinGameButton.GetComponent<BoxCollider>().enabled = false;
        }

        if(joinCodeText.Text.Length < 1 && relaySetup.joinCode.Length > 1)
        {
            joinCodeText.Text = relaySetup.joinCode;
        }

        if (!IsServer)
        {
            return;
        }

        if (!playersJoined && FindObjectsOfType<PlayerController>().Length == 2)
        {
            playersJoined = true;

            readyMan.PlayersJoined();
        }
    }

    public void CreateGameSetup()
    {
        relaySetup.CreateOrJoin(1, "");
    }

    public void JoinGameSetup()
    {
        joinCodeText.gameObject.SetActive(false);

        joinGameButton.SetActive(true);
    }

    public void JoinGame()
    {
        joinGameButton.SetActive(false);

        relaySetup.CreateOrJoin(2, joinCodeInput.Text);
    }

    public void SelectWeapon(string weaponName)
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (var p in players)
        {
            if (IsServer && p.CompareTag("Host"))
            {
                foreach (var w in weapons)
                {
                    if (w.name.Contains(weaponName) && w.name.Contains("Host"))
                    {
                        p.projectilePrefab = w;

                        playerData.hostWeapon = w;
                    }
                }
            }
            else if(!IsServer && p.CompareTag("Client"))
            {
                foreach (var w in weapons)
                {
                    if (w.name.Contains(weaponName) && w.name.Contains("Client"))
                    {
                        p.projectilePrefab = w;

                        playerData.clientWeapon = w;
                    }
                }
            }
        }

        if (!IsServer)
        {
            SelectWeaponServerRpc(weaponName);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectWeaponServerRpc(string weaponName)
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (var p in players)
        {
            if (IsServer && p.CompareTag("Client"))
            {
                foreach (var w in weapons)
                {
                    if (w.name.Contains(weaponName) && w.name.Contains("Client"))
                    {
                        p.projectilePrefab = w;

                        playerData.clientWeapon = w;
                    }
                }
            }
        }
    }
}
