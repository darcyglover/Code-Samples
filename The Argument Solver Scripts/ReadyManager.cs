using System.Reflection;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class ReadyManager : NetworkBehaviour
{
    [SerializeField] GameObject readyClick, hostTick, clientTick;

    GameManager gameMan;

    MenuManager menuMan;

    ArenaManager arenaMan;

    CameraManager cameraMan;

    PlayerController[] players;

    public int playersReadyAmount;

    bool hostReady, clientReady;

    void Awake()
    {
        menuMan = FindObjectOfType<MenuManager>();

        arenaMan = FindObjectOfType<ArenaManager>();

        cameraMan = FindObjectOfType<CameraManager>();

        gameMan = FindObjectOfType<GameManager>();
    }

    public void PlayersJoined()
    {
        if (IsServer)
        {
            MoveButton(true);
        }
    }

    public void MoveButton(bool goingUp)
    {
        MoveButtonActual(goingUp);

        if (IsServer)
        {
            MoveButtonClientRpc(goingUp);
        }
        else
        {           
            MoveButtonServerRpc(goingUp);
        }
    }

    void MoveButtonActual(bool goingUp)
    {
        if (goingUp)
        {
            menuMan.MoveTransform('y', new Vector3(0f, -3.9f, 0f), 0.75f, false, "Ready Click");
        }
        else
        {
            menuMan.MoveTransform('y', new Vector3(0f, -7.8f, 0f), 0.75f, false, "Ready Click");
        }
    }

    public void ButtonClicked(bool host)
    {
        bool success = false;

        if (host && IsServer)
        {
            if (!hostReady)
            {
                hostReady = true;
            }
            else
            {
                hostReady = false;
            }

            success = true;
        }
        else if(!host && !IsServer)
        {
            if (!clientReady)
            {
                clientReady = true;
            }
            else
            {
                clientReady = false;
            }

            success = true; //this bool makes sure it has checked one of the two, if it it hasn't, it doesn't allow the further checks to be done
        }

        if (success)
        {
            CheckReadyStatus();
        }
    }

    void CheckReadyStatus() //checking the ready status of the players
    {
        if (!IsServer)
        {
            if (!IsHost && clientReady)
            {
                playersReadyAmount++;

                clientTick.SetActive(true);
                TickSetActiveServerRpc(true, "Client Tick");

                ConfirmReadyServerRpc(1);
            }
            else if (!IsHost && !clientReady)
            {
                playersReadyAmount--;

                clientTick.SetActive(false);
                TickSetActiveServerRpc(false, "Client Tick");

                ConfirmReadyServerRpc(2);
            }

            return;
        }

        if (IsHost && hostReady)
        {
            TickSetActiveClientRpc(true, "Host Tick");

            ConfirmReadyClientRpc(1);
        }
        else if (IsHost && !hostReady)
        {
            TickSetActiveClientRpc(false, "Host Tick");

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

        ReadyCheck();
    }

    void ReadyCheck() //checks to see if both players are ready, and activates them if so
    {
        if (playersReadyAmount == 2)
        {
            if (IsServer)
            {
                EnterArena();
                EnterArenaClientRpc();
            }
            else
            {
                EnterArena();
                EnterArenaServerRpc();
            }

            StartCoroutine(CallMethodAfterTime(13f, "StartGameServerCheck")); //change the time to how long the camera takes to get to the arena, this is how long the script will wait before "starting" the game
        }
    }

    void EnterArena()
    {
        menuMan.AllInputFieldsToggle(true);
        arenaMan.RemoveText();
    }

    void StartGameServerCheck() 
    {
        if (IsServer)
        {
            StartGameClientRpc();
        }
        else
        {
            StartGame();
        }
    }

    void StartGame()
    {
        players = FindObjectsOfType<PlayerController>();

        foreach (var p in players)
        {
            p.playersReady = true;
        }

        Camera playerCam = FindObjectOfType<Camera>();

        if (IsServer)
        {
            foreach (var p in players)
            {
                if (p.CompareTag("Host"))
                {
                    p._playerCamera = playerCam;
                    cameraMan._setPlayer = p;
                }
            }
        }
        else
        {
            foreach (var p in players)
            {
                if (p.CompareTag("Client"))
                {
                    p._playerCamera = playerCam;
                    cameraMan._setPlayer = p;
                }
            }
        }

        readyClick.SetActive(false);

        //gameMan.menuCamera.SetActive(false);

        menuMan.playing = true;
        //menuMan.SavePlayerData();
        menuMan.currentMenuSection = "";

        //resetting in case the players want to play again after this game finishes
        playersReadyAmount = 0;

        hostTick.SetActive(false);
        clientTick.SetActive(false); 

        clientReady = false;
        hostReady = false;
    }

    void TickSetActive(bool active, string name)
    {
        if(hostTick.name == name)
        {
            hostTick.SetActive(active);
        }
        else
        {
            clientTick.SetActive(active);
        }
    }

    IEnumerator CallMethodAfterTime(float waitTime, string methodName) //this can only be used to call methods which don't take overloads for now. Will look into ways to make it more modular - Darcy
    {
        yield return new WaitForSeconds(waitTime);

        MethodInfo mi = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

        mi.Invoke(this, null);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ConfirmReadyServerRpc(int picked)
    {
        ReadyUp(picked);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TickSetActiveServerRpc(bool active, string tag)
    {
        TickSetActive(active, tag);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveButtonServerRpc(bool goingUp)
    {
        MoveButtonActual(goingUp);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EnterArenaServerRpc()
    {
        EnterArena();
    }

    [ClientRpc]
    public void EnterArenaClientRpc()
    {
        EnterArena();
    }

    [ClientRpc]
    public void MoveButtonClientRpc(bool goingUp)
    {
        MoveButtonActual(goingUp);
    }

    [ClientRpc]
    public void ConfirmReadyClientRpc(int picked)
    { 
        ReadyUp(picked);
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        StartGame();
    }

    [ClientRpc]
    public void TickSetActiveClientRpc(bool active, string tag)
    {
        TickSetActive(active, tag);
    }
}
