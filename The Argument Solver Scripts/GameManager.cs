using System.Collections;
using Unity.Netcode;
using UnityEngine;
using MText;

public class GameManager : NetworkBehaviour
{
    public Modular3DText joinCodeText;
    public MText_UI_InputField joinCodeInput;

    public GameObject menuCamera;
    [SerializeField] GameObject joinGameButton;
    
    public bool playersJoined, isLeft, joiningLocked;

    ReadyManager readyMan;

    RelayManager relayMan;

    MenuManager menuMan;

    PlayerData playerData;

    void Awake()
    {
        relayMan = FindObjectOfType<RelayManager>();

        readyMan = FindObjectOfType<ReadyManager>();

        playerData = FindObjectOfType<PlayerData>();

        menuMan = FindObjectOfType<MenuManager>();
    }

    void Update()
    {
        if(joinCodeInput.Text.Length == 6)
        {
            if (!isLeft)
            {
                MoveJoinLobbyButton();
            }

            joinGameButton.GetComponent<BoxCollider>().enabled = true;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                joinGameButton.GetComponent<BoxCollider>().enabled = false;

                JoinGame();
            }
        }
        else
        {
            if (isLeft)
            {
                MoveJoinLobbyButton();

                relayMan.wrongCode.SetActive(false);
            }

            joinGameButton.GetComponent<BoxCollider>().enabled = false;
        }

        if (!IsServer)
        {
            return;
        }

        if (!playersJoined && !joiningLocked && FindObjectsOfType<PlayerController>().Length == 2)
        {
            playersJoined = true;

            readyMan.PlayersJoined();

            menuMan.ClientJoinTheLobby("Host Game Lobby");
        }
    }

    public void StanceInput(Modular3DText input)
    {
        if (IsServer)
        {
            StanceInputClientRpc(input.name, input.Text);
        }
        else
        {
            StanceInputServerRpc(input.name, input.Text);
        }
    }

    void UpdateStance(string name, string text)
    {
        Modular3DText otherStance = GameObject.Find(name).GetComponent<Modular3DText>();

        otherStance.Text = text;
    }

    public void ClientJoined()
    {
        joinCodeInput.GetComponent<AudioSource>().mute = true; //stops a random typewriter sound from playing when it resets the code to null
        StartCoroutine(UnmuteSource());

        joinCodeInput.Text = "";
        menuMan.InputFieldToggle("Lobby Code - Input Field", false);

        menuMan.GameLobby(false);

        MoveJoinLobbyButton();
    }

    public void SetJoinCode()
    {
        joinCodeText.Text = relayMan.joinCode;

        playersJoined = false;
    }

    public void HostGameSetup()
    {
        relayMan.HostOrJoin(1, "");
    }

    public void JoinGameSetup()
    {
        joinCodeInput.GetComponent<AudioSource>().mute = true; //stops a random typewriter sound from playing when it resets the code to null
        StartCoroutine(UnmuteSource());

        joinCodeInput.Text = "";

        joinGameButton.SetActive(true);
    }

    public void JoinGame()
    {
        relayMan.HostOrJoin(2, joinCodeInput.Text);
    }

    public void MoveJoinLobbyButton()
    {
        if (!isLeft)
        {
            menuMan.MoveTransform('a', new Vector3(11f, -4.15f, 2.9f), 0.75f, false, "Join Lobby Button");

            isLeft = true;
        }
        else
        {
            menuMan.MoveTransform('a', new Vector3(13f, -3.75f, 0f), 0.75f, false, "Join Lobby Button");

            isLeft = false;
        }
    }

    IEnumerator UnmuteSource()
    {
        yield return new WaitForSeconds(0.2f);

        joinCodeInput.GetComponent<AudioSource>().mute = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void StanceInputServerRpc(string name, string text)
    {
        UpdateStance(name, text);
    }

    [ClientRpc]
    public void StanceInputClientRpc(string name, string text)
    {
        UpdateStance(name, text);
    }
}
