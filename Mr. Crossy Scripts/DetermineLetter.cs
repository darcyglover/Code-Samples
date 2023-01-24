using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Written by Darcy Glover

public class DetermineLetter : MonoBehaviour //This script determines the first letter of an object when they are placed on altars. It also sends this letter to the current Puzzle Controller.
{
    [HideInInspector]
    public GameObject storedObject;

    PuzzleController puzzleController;

    string firstJLetter, wholeName;

    [HideInInspector]
    public string wordName;

    bool altarAssigned;

    public void AssignPuzzleController() //this method finds the correct puzzle controller for the gameobject
    {
        PuzzleController[] puzzleControllers = FindObjectsOfType<PuzzleController>(); //makes an array of all the puzzle controllers in case there is more than one active.

        string[] splitName = gameObject.name.Split('[', ']', ',', ' '); //splits the name of this gameobject into an array that doesn't include the stuff in the brackets.

        wordName  = "";

        int count = 0;

        for (int i = 0; i < splitName.Length; i++)
        {
            if (splitName[i] != null)
            {
                wordName += splitName[i]; //it then combines the array into one word
                count++;
                if (count == 3)
                {
                    break;
                }
            }
        }

        Debug.Log("Combined the split array and have made: " + wordName + " from the altar: " + gameObject.name); //eg: Combined the split array and have made: LAKE from the altar: [L]AKE Altar

        Debug.Log("Puzzle Controllers array length: " + puzzleControllers.Length);

        for (int i = 0; i < puzzleControllers.Length; i++)
        {
            for (int x = 0; x < puzzleControllers[i].wordObjects.Count; x++)
            {
                if (wordName == puzzleControllers[i].wordObjects[x].name) //it then searches the puzzle controllers, and if any of them have a word in their puzzle that matches the name of this altar, it chooses that puzzle controller.
                {
                    puzzleController = puzzleControllers[i];
                    Debug.Log("Puzzle Controller for: " + gameObject.name + " is: " + puzzleController.gameObject.name);
                }
            }
        }

        if (!altarAssigned && wordName != "GATE")
        {
            AssignAltar();
        }
    }

    void AssignAltar() //this method adds this altar to a list within the found street collider for disabling purposes when the word is complete
    {
        WordCollision[] collisions = FindObjectsOfType<WordCollision>();

        for(int i = 0; i < collisions.Length; i++)
        {
            if (collisions[i].mainWord == wordName) //same method of finding the correct one as the assign puzzle controller method
            {
                if (!GetComponentInParent<OverlappedAltar>()) //if it doesn't have an overlapped altar, it can just add this object normally.
                {
                    collisions[i].altars.Add(gameObject);
                    altarAssigned = true;
                }
                else //if it does, however, it will need to add the parent of this object instead.
                {
                    collisions[i].altars.Add(GetComponentInParent<OverlappedAltar>().gameObject);
                    altarAssigned = true;
                }
            }
        }
    }

    public void ObjectPlaced(GameObject placedObject) //this gets called when the player places an object down on an altar
    {
        storedObject = placedObject; 
        wholeName = placedObject.name; 
        firstJLetter = wholeName.Substring(1, 1); //finds the first letter of the object, from the second character as objects are named like this: [H]AT (would find H)
        SendLetterAndName(firstJLetter); 
    }

    public void ObjectPickedUp() //this method just reverts the letter to empty after an object is picked back up from an altar
    {
        firstJLetter = ""; 

        SendLetterAndName(firstJLetter);
    }

    void SendLetterAndName(string letter) //sending the letter to the controller, as well as the name of the object it came from
    {
        if(puzzleController == null) //will assign the puzzle controller once, to it doesn't create lag
        {
            AssignPuzzleController();
        }

        puzzleController.ReceiveLetterAndName(letter, gameObject.name); //finally, sending all of the relevant variables to the assigned puzzle controller.
    }
}
