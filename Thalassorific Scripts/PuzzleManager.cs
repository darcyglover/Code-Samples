using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//Written by Darcy Glover

public class PuzzleManager : MonoBehaviour
{ 
    [SerializeField]
    GameObject[] puzzles;

    //[HideInInspector]
    //public LayerMask scannerMask; //this is for the hand scanner because of instantiating issues

    [HideInInspector]
    public int whichPuzzle = 0; //checking which puzzle the player is currently solving

    PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        ActivatePuzzle();
    }

    public void ActivatePuzzle() //method that activates puzzles
    {
        switch (whichPuzzle)
        {
            case 0: 
                {
                    photonView.RPC("RPC_ActivateFirstPuzzle", RpcTarget.All, whichPuzzle);
                    photonView.RPC("RPC_ActivateThirdPuzzle", RpcTarget.All, whichPuzzle);
                    break;
                }
            case 1:
                {
                    photonView.RPC("RPC_ActivateSecondPuzzle", RpcTarget.All, whichPuzzle);
                    break;
                }
            case 2:
                {
                    photonView.RPC("RPC_ActivateThirdPuzzle", RpcTarget.All, whichPuzzle);
                    break;
                }
        }
    }

    [PunRPC]
    void RPC_ActivateFirstPuzzle(int whichPuzzle)
    {
        puzzles[whichPuzzle].GetComponent<RandomiseSymbols>().enabled = true;
        puzzles[whichPuzzle].GetComponent<PuzzleCompletionManager>().enabled = true;
    }

    [PunRPC]
    void RPC_ActivateSecondPuzzle(int whichPuzzle)
    {
        foreach (var p in puzzles[whichPuzzle].GetComponentsInChildren<RandomiseSymbols>()) //turning on all the randomisers in the second puzzle
        {
            p.enabled = true;
        }
        foreach (var p in puzzles[whichPuzzle].GetComponentsInChildren<PuzzleCompletionManager>()) //turning on all the symbol checks
        {
            p.enabled = true;
        }
        GetComponent<DoorControl>().door = GameObject.Find("Door 2"); //switching to the second door for unlocking purposes
    }

    [PunRPC]
    void RPC_ActivateThirdPuzzle(int whichPuzzle)
    {
        puzzles[2].GetComponent<RandomiseSymbols>().enabled = true; //activates at the start, but,
        if (whichPuzzle == 2)
        {
            GetComponent<SpawnMapSymbols>().SpawnSymbols(); //spawning map symbols
            //puzzles[whichPuzzle].GetComponent<CorrectSymbolCheck>().enabled = true; //only activates the symbol check if puzzle 2 has been completed
        }
    }
}
