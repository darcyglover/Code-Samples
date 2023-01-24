using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using Photon.Pun;

//Written by Darcy Glover - Any FMOD scripting was done by Sean Casey

public class PuzzleCompletionManager : MonoBehaviour
{
    [SerializeField]
    StudioEventEmitter[] doorCloseSounds; //When wrong answer

    [SerializeField]
    TimeController timeController;

    PuzzleManager puzzleManager;

    [SerializeField]
    GameObject[] symbols;

    [SerializeField]
    GameObject dashboard, door;

    Sprite[] clickedSymbols = new Sprite[4];

    [SerializeField]
    Sprite neutral;

    [Tooltip("The bool or trigger name for the animation")] public string animParameter;
    [SerializeField]
    public Animator intelPuzzleAnims;

    PhotonView photonView;

    bool isVRPlayer;
        
    //[HideInInspector]
    public int correctSymbolCount = 0, puzzleThreeAttempts = 0;
    int incorrectSymbolCount = 0, puzzleOneAttempts = 0, puzzleTwoAttempts = 0;

    string masterPlayerName, otherPlayerName;

    void Start()
    {
        timeController.StartMainTimer();
        puzzleManager = FindObjectOfType<PuzzleManager>();
    }

    public bool CorrectSymbolCheck(Sprite clickedSymbol)
    {
        Debug.Log("Starting the check script");

        SkitterEventP3 skitterEvent = FindObjectOfType<SkitterEventP3>();

        bool condition;

        if (puzzleManager.whichPuzzle != 2 || skitterEvent.eventHappening) 
        {
            Debug.Log("Checking symbols");

            isVRPlayer = GameObject.Find("GameSetup").GetComponent<GameSetup>().isVRPlayer;

            if (photonView == null)
            {
                photonView = GetComponent<PhotonView>();
            }

            incorrectSymbolCount = 0;

            for (int i = 0; i < 4; i++) //checking to see if the clicked colour is one of the ones in the "spawned" symbols on the map
            {
                if (clickedSymbol == symbols[i].GetComponent<SpriteRenderer>().sprite && correctSymbolCount < 4)
                {
                    for (int z = 0; z < 4; z++)
                    {
                        if (clickedSymbol == clickedSymbols[z])
                        {
                            timeController.EndBetweenTimer();
                            PuzzleAttempt();
                            condition = false;
                            return condition;
                        }
                    }

                    timeController.EndBetweenTimer();
                    clickedSymbols[correctSymbolCount] = clickedSymbol;
                    correctSymbolCount++;
                    Debug.Log(correctSymbolCount.ToString() + " " + name);

                    if (correctSymbolCount >= 4) //"win" condition
                    {
                        switch (door.name) //checking which puzzle the player is currently doing
                        {
                            case "Door 1":
                                {
                                    photonView.RPC("RPC_PuzzleOneComplete", RpcTarget.All);
                                    condition = true;
                                    return condition;
                                }
                            case "Door 2":
                                {
                                    if (gameObject.name == "Inside")
                                    {
                                        photonView.RPC("RPC_DoorTwoLocked", RpcTarget.All);
                                    }
                                    else
                                    {
                                        photonView.RPC("RPC_DoorTwoUnlocked", RpcTarget.All);
                                    }
                                    condition = true;
                                    return condition;
                                }
                        }
                    }
                }
                else
                {
                    timeController.EndBetweenTimer();
                    incorrectSymbolCount++; //counts when the player hits a wrong button
                    Debug.Log(incorrectSymbolCount.ToString());
                    if (incorrectSymbolCount >= 4)
                    {
                        Debug.Log("Incorrect count when calling attempt : " + incorrectSymbolCount);
                        PuzzleAttempt();
                    }
                }
            }
            incorrectSymbolCount = 0;
            if (correctSymbolCount > 0)
            {
                condition = true;
            }
            else
            {
                condition = false;
            }
            return condition;
        }
        else 
        {
            condition = false;
            return condition;
        }
    }

    [PunRPC]
    void RPC_PuzzleOneComplete()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            DarcyAnalyticMethods analyticMethods = FindObjectOfType<DarcyAnalyticMethods>();
            Debug.Log("PuzzleOne Analytic Object: " + analyticMethods.gameObject.name);

            dashboard.GetComponent<ClickOnSymbols>().OrangeButtonColours(); //this turns the buttons back to orange for feedback
            dashboard.GetComponent<DoorControl>().UnlockDoor();

            Debug.Log("PuzzleOne Ending Timer");
            timeController.EndMainTimer();

            Debug.Log("PuzzleOne Attempting to set names");
            masterPlayerName = PhotonNetwork.MasterClient.NickName;
            otherPlayerName = PhotonNetwork.LocalPlayer.NickName;
            Debug.Log("PuzzleOne Names: " + masterPlayerName + " and " + otherPlayerName);

            float mainElaspedTime = timeController.GetTime(0);

            Debug.Log("PuzzleOne Time: " + mainElaspedTime);

            string names = masterPlayerName + " and " + otherPlayerName;
            Debug.Log(names);

            int level = GetCurrentLevel();

            analyticMethods.PuzzleOneCompletion(mainElaspedTime, names, level);

            timeController.StartMainTimer();

            analyticMethods.PuzzleOneAttemptsMade(puzzleOneAttempts, level, names);

            intelPuzzleAnims.SetBool(animParameter, true);
        }

        correctSymbolCount = 0;
        incorrectSymbolCount = 0;
    }

    [PunRPC]
    void RPC_DoorTwoLocked()
    {
        if (!PhotonNetwork.IsMasterClient && puzzleManager.whichPuzzle != 2)
        {
            DarcyAnalyticMethods analyticMethods = FindObjectOfType<DarcyAnalyticMethods>();

            masterPlayerName = PhotonNetwork.MasterClient.NickName;
            otherPlayerName = PhotonNetwork.LocalPlayer.NickName;

            float mainElapsedTime = timeController.GetTime(0);

            string names = masterPlayerName + " and " + otherPlayerName;

            int level = GetCurrentLevel();

            analyticMethods.PuzzleTwoCompletion(mainElapsedTime, names, gameObject.name, level);
        }

        dashboard.GetComponent<ClickOnSymbols>().OrangeButtonColours();
        dashboard.GetComponent<DoorControl>().LockDoor();

        correctSymbolCount = 0;
        incorrectSymbolCount = 0;
    }

    [PunRPC]
    void RPC_DoorTwoUnlocked()
    {
        if(!PhotonNetwork.IsMasterClient && puzzleManager.whichPuzzle != 2)
        {
            DarcyAnalyticMethods analyticMethods = FindObjectOfType<DarcyAnalyticMethods>();

            timeController.EndMainTimer();

            masterPlayerName = PhotonNetwork.MasterClient.NickName;
            otherPlayerName = PhotonNetwork.LocalPlayer.NickName;

            float mainElapsedTime = timeController.GetTime(0);

            string names = masterPlayerName + " and " + otherPlayerName;

            int level = GetCurrentLevel();

            analyticMethods.PuzzleTwoCompletion(mainElapsedTime, names, gameObject.name, level);

            analyticMethods.PuzzleTwoAttemptsMade(puzzleTwoAttempts, level, names);

            timeController.StartMainTimer();

            if (!intelPuzzleAnims.GetBool("puz2"))
            {
                intelPuzzleAnims.SetBool("puz2", true);
            }
        }

        dashboard.GetComponent<ClickOnSymbols>().OrangeButtonColours();
        dashboard.GetComponent<DoorControl>().UnlockDoor();

        correctSymbolCount = 0;
        incorrectSymbolCount = 0;
    }

    public void PuzzleAttempt()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            DarcyAnalyticMethods analyticMethods = FindObjectOfType<DarcyAnalyticMethods>();

            Debug.Log("Attempting to set names");
            masterPlayerName = PhotonNetwork.MasterClient.NickName;
            otherPlayerName = PhotonNetwork.LocalPlayer.NickName;
            Debug.Log("Names: " + masterPlayerName + " and " + otherPlayerName);

            float attemptTime = timeController.GetTime(0);
            float betweenElapsedTime = timeController.GetTime(1);

            Debug.Log("Time: " + betweenElapsedTime);

            string names = masterPlayerName + " and " + otherPlayerName;
            Debug.Log(names);

            int level = GetCurrentLevel();

            switch (puzzleManager.whichPuzzle)
            {
                case 0:
                    {
                        puzzleOneAttempts++;
                        analyticMethods.PuzzleOneAttempt(attemptTime, betweenElapsedTime, names, level);
                        break;
                    }
                case 1:
                    {
                        puzzleTwoAttempts++;
                        analyticMethods.PuzzleTwoAttempt(attemptTime, betweenElapsedTime, names, level);
                        break;
                    }
                case 2:
                    {
                        puzzleThreeAttempts++;
                        analyticMethods.PuzzleThreeAttempt(attemptTime, betweenElapsedTime, names, level);
                        break;
                    }
            }

            timeController.StartBetweenTimer();
        }

        if(puzzleManager.whichPuzzle != 2)
        {
            if (dashboard.GetComponent<PuzzleManager>().whichPuzzle < 2 && isVRPlayer)
            {
                doorCloseSounds[dashboard.GetComponent<PuzzleManager>().whichPuzzle].Play(); // Plays feedback song that players were wrong
            }

            Debug.Log("incorrect");
            correctSymbolCount = 0;
            incorrectSymbolCount = 0;
            for (int x = 0; x < 4; x++) //refreshing the array after a failed attempt
            {
                clickedSymbols[x] = neutral;
            }
        }
    }

    public int GetCurrentLevel()
    {
        int level = 0;
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Main_Level - Sub Test":
                {
                    level = 1;
                    break;
                }
        }

        return level;
    }
}
