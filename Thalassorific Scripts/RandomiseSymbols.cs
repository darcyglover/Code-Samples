using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

//Written by Darcy Glover, Jack Hobbs and Jasper von Riegen
//All programmers assisted with the Photon parts of this script.

public class RandomiseSymbols : MonoBehaviour
{
    public GameObject[] symbols;

    [SerializeField]
    GameObject[] symbolsOnMap;

    //[HideInInspector]
    GameObject outside;

    [SerializeField]
    Sprite[] sprites;

    int randomNumber, numberChecker, arrayLength;
    int[] fourStoredRandomNumbers = new int[4], eightStoredRandomNumbers = new int[8];

    PhotonView photonView;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PickUniqueRandomNumbers();
            ApplySymbols();
        }
    }

    public void PickUniqueRandomNumbers()
    {
        if (name == "Inside") //checking for the second puzzle, make sure that the symbols are unique
        {
            outside = GameObject.Find("Outside");
            for (int i = 0; i < 8; i++)
            {
                randomNumber = Random.Range(0, 20);
                eightStoredRandomNumbers[i] = randomNumber;
            }
            arrayLength = 8;
            CheckNumbers(eightStoredRandomNumbers, arrayLength);
        }
        else if(name != "Outside") //we dont want to apply symbols to outside twice
        {
            for (int i = 0; i < 4; i++) //applying the random numbers into the array
            {
                randomNumber = Random.Range(0, 20);
                fourStoredRandomNumbers[i] = randomNumber;
            }
            arrayLength = 4;
            CheckNumbers(fourStoredRandomNumbers, arrayLength);
        }
    }

    void CheckNumbers(int[] storedNumbers, int arrayL)
    {
        for (int i = 0; i < arrayL; i++) //cross referencing the array to make sure all the numbers are unique.
        {
            numberChecker = storedNumbers[i];
            for (int x = 0; x < arrayL; x++)
            {
                if (x == i)
                {
                    continue;
                }
                else if (numberChecker == storedNumbers[x])
                {
                    storedNumbers[x] = Random.Range(0, 20);
                    CheckNumbers(storedNumbers, arrayL);
                }
            }
        }
    }

    public void ApplySymbols()
    {
        photonView.RPC("RPC_ApplySymbols", RpcTarget.All, eightStoredRandomNumbers, fourStoredRandomNumbers);
    }

    [PunRPC]
    void RPC_ApplySymbols(int[] eightStored, int[] fourStored) //applying the symbols to the images to 'spawn' them in
    {
        Debug.Log(name);
        GameObject outsideSymbols = GameObject.Find("Outside");

        if(name == "Inside") //inside applies half its array to outside and half to inside
        {
            for (int i = 0; i < 4; i++)
            {
                symbols[i].GetComponent<SpriteRenderer>().sprite = sprites[eightStored[i]];
            }
            for (int x = 4; x < 8; x++)
            {
                outsideSymbols.GetComponent<RandomiseSymbols>().symbols[x - 4].GetComponent<SpriteRenderer>().sprite = sprites[eightStored[x]];
            }
        }
        else if(name != "Outside") //we dont want to apply symbols to outside twice
        {
            for (int i = 0; i < 4; i++)
            {
                symbols[i].GetComponent<SpriteRenderer>().sprite = sprites[fourStored[i]];
            }
        }

        if (name == "Puzzle 3") //for the third puzzle, also needs to apply the symbols to the intelligence's map
        {
            for (int x = 0; x < 4; x++)
            {
                symbolsOnMap[x].GetComponent<Image>().sprite = sprites[fourStored[x]];
            }
        }
    }
}
