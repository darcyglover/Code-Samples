using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.VFX;

//Written by Darcy Glover

public class PuzzleController : MonoBehaviour
{
    //strings
    public string word, currentStreet;
    string playersWord, letter, altarName;

    //random things
    UIController uiController;
    EventInstance eventInstance;
    [HideInInspector]
    public WordCollision wordCollision;

    //TMP
    [SerializeField]
    TextMeshProUGUI mistakeText;
    [HideInInspector]
    public TextMeshProUGUI streetText;

    //ints
    int wordLength, mistakeCount, completedWords, letterPoint, objectPoint;
    public int wordsInPuzzle;

    //lists
    //[HideInInspector]
    public List<TextMeshProUGUI> canvasLetters = new List<TextMeshProUGUI>();
    [HideInInspector]
    public List<GameObject> storedObjects = new List<GameObject>();
    public List<GameObject> wordObjects = new List<GameObject>();

    //bools
    public bool tutorial;
    bool tenPlayed, petPlayed, trophyPlayed, cheating;

    //unity events
    public UnityEvent winEvent, loseEvent, tutorialEvent, tutorialMistakeEvent;

    void Start()
    {
        uiController = FindObjectOfType<UIController>();

        if (tutorial) //manually setting things at the start for the tutorial
        {
            uiController.currentWordDisplay = uiController.wordDisplayObjects[1];

            SetUpLetters(0);
        }
    }

    public void SetUpLetters(int whichObject) //this method sets up the canvas letters list to have the correct game objects stored within it
    {
        objectPoint = whichObject; //takes in an int, so that a particular variable in the list can be accessed if needed.

        canvasLetters.Clear();

        foreach (TextMeshProUGUI tmp in wordObjects[objectPoint].GetComponentsInChildren<TextMeshProUGUI>())
        {
            canvasLetters.Add(tmp); //adds all the child objects of the particluar word that is chosen by the caller script.
        }

        wordLength = canvasLetters.Count; //the word length is then equal to the amount of objects in the list

        if (word == wordObjects[objectPoint].name) //if the current word is the same as the one that was just added, it will update the UI to reflect the new word.
        {
            if (wordCollision == null) //checking to see if it is allowed to write to the UI
            {
                WriteToUI();
            }
            else if (!wordCollision.dontWrite)
            {
                WriteToUI();
            }
        }
    }

    public void ReceiveLetterAndName(string firstLetter, string altarOrigName) //receiving the letter of the object, and the name of the altar it came from, from the determine letter script
    {
        letter = firstLetter;
        altarName = altarOrigName;
        DisplayLetter();
        PlayerWordControl();
    }

    void DisplayLetter() //this method decides which TMP component's text to change in the canvas letters script.
    {
        string[] splitName = altarName.Split('[', ']');

        string firstLength = splitName[0]; 

        for (int i = 0; i < wordObjects.Count; i++)
        {
            if (wordObjects[i].name == GameObject.Find(altarName).GetComponent<DetermineLetter>().wordName)
            {
                SetUpLetters(i); //calls the set up letters script to re-set up the canvas letters list if it hasn't been already 
                break;
            }
        }

        letterPoint = firstLength.ToIntArray().Length; //the length of this string determines which TMP component's text to change.
                                                       //if the altar that the object came from was LA[K]E, the length of the first string in the array would be 2.
                                                       //therefore, the text that needs to be changed is at index 2 in the list.

        canvasLetters[letterPoint].text = letter;

        //TextMeshProUGUI[] uiLetters = uiController.GetComponentsInChildren<TextMeshProUGUI>();

        //uiController.wordPop.transform.SetParent(uiLetters[letterPoint].transform);

        //uiController.wordPop.GetComponent<VisualEffect>().Play();
    }

    public void PlayerWordControl() //this method forms the players word as they place objects, and also monitors the win condition
    {
        int playersWordLength;

        if (!cheating) //'cheating' is a dev tool bool
        {
            playersWord = "";

            for (int i = 0; i < wordLength; i++) //the player's word becomes equal to all the texts within the canvas letters combined
            {
                playersWord += canvasLetters[i].text;
            }
        }

        playersWordLength = playersWord.ToIntArray().Length;

        if (GameObject.Find(altarName) != null) //storing the altar for later
        {
            storedObjects.Add(GameObject.Find(altarName));
        }

        if (word == wordObjects[objectPoint].name)
        {
            if (wordCollision == null) //re-updates the UI
            {
                WriteToUI();
            }
            else if (!wordCollision.dontWrite)
            {
                WriteToUI();
            }
        }

        if (tutorial) //win condition and other events to play during the tutorial
        {
            TutorialController tutorialController = FindObjectOfType<TutorialController>();

            tutorialController.ChangeConLetter(letter);

            eventInstance = RuntimeManager.CreateInstance("event:/MR_C_Tutorial/TUT.0.5");

            eventInstance.start();

            tutorialEvent.Invoke();
            tutorial = false;
        }

        AudioChecks(); //checking for any audio events that need to play

        if (playersWord == word && wordCollision == null) //the method then checks to see if the player has completed this section of the crossword, without checking for the street collider
        {
            CompletionCheck();
        }

        if (wordCollision != null)
        {
            if (playersWord == word) //if the word that the player has made is correct, it will mark it as complete in the street collider
            {
                wordCollision.puzzleComplete = true;
                wordCollision.DisableAltars();
                CompletionCheck(); //the method then checks to see if the player has completed this section of the crossword
            }

            Debug.Log("Word Collision's overlapped streets length is: " + wordCollision.overlappedStreets.Length);

            if (wordCollision.overlappedStreets.Length > 0) //if there is an overlap, the method needs to go through a few variable and methods to complete the marking.
            {
                Debug.Log("Starting the overlapped streets loop.");
                for (int i = 0; i < wordCollision.overlappedStreets.Length; i++)
                {
                    if (!GameObject.Find(wordCollision.overlappedStreets[i]).GetComponent<WordCollision>().altarsDisabled) //will only do it if the altars are not disabled
                    {
                        WordCollision temp = wordCollision; //makes a temporary word collision variable so that it has the original one and the new one.
                        Debug.Log("Altars disabled if statement has been accessed for: " + wordCollision.overlappedStreets[i] + ". Temp is: " + temp.gameObject.name);
                        wordCollision = GameObject.Find(wordCollision.overlappedStreets[i]).GetComponent<WordCollision>(); //finds the new one
                        wordCollision.dontWrite = true; //this boolean stops the overlapped street from being written to the UI.
                        wordCollision.SetUpController(); //sets up the overlapped street's puzzle controller
                        wordCollision.dontWrite = false;
                        Debug.Log("Just completed SetUpController for: " + wordCollision.gameObject.name);
                        wordCollision = temp; //sets the word collision back to the original one, the street that the player is currently on
                        Debug.Log("Word collision has been reverted back to: " + wordCollision.gameObject.name + " from: " + temp.gameObject.name);
                        wordCollision.dontCheck = true; //this boolean stops it from checking if the word is complete again. this would cause an infinite loop
                        wordCollision.SetUpController(); //sets the puzzle controller back what it was at the start of this loop.
                        wordCollision.dontCheck = false;
                    }
                }
            }
        }

        if (playersWordLength == wordLength && playersWord != word) //if the player has put all the letters on the altar but hasnt gotten the word right, it counts down a mistake.
        {
            if (wordCollision != null)
            {
                if (!wordCollision.puzzleComplete) //checks to make sure that this word hasn't already been completed.
                {
                    AudioEvents audio = FindObjectOfType<AudioEvents>();

                    audio.WordSpeltIncorrectly();

                    MistakeCounter(); 
                }
            }
            else if(wordCollision == null) //this is purely for the home altars, since the home doesn't have a word collision script.
            {
                AudioEvents audio = FindObjectOfType<AudioEvents>();

                audio.WordSpeltIncorrectly();

                MistakeCounter();
            }
        }
    }

    public void WriteToUI() //this method displays the current word that the player has formed using objects on the UI
    {
        TextMeshProUGUI[] letters = uiController.currentWordDisplay.GetComponentsInChildren<TextMeshProUGUI>(); //getting an array of the TMP components in the word object's children

        for(int i = 0; i < letters.Length; i++)
        {
            if (canvasLetters[i].text.ToIntArray().Length < 1) //if the canvas letter doesn't have any text, sets the TMP text to nothing and the alpha to 0.
            {
                letters[i].text = "";
                letters[i].alpha = 0f;
            }
            else //if it has 1 or more, it will change the text to be the same, and the alpha gets set to 1. 
            {
                letters[i].text = canvasLetters[i].text;
                letters[i].alpha = 1f; //this will need to be changed in the future to instead gradually gradient up to 1.
            }
        }
    }

    void CompletionCheck()
    {
        if (!cheating) //won't count if doing a dev skip
        {
            completedWords++;
        }

        if (gameObject.name.Contains("Tutorial")) //specific event for the tutorial
        {
            TutorialAltarDisable();
        }

        AudioEvents audio = FindObjectOfType<AudioEvents>();

        audio.WordSpeltCorrectly();

        for (int i = 0; i < wordObjects.Count; i++)
        {
            if (word == wordObjects[i].name) //this turns on the line through on the map
            {
                Debug.Log(wordObjects[i].name);
                wordObjects[i].GetComponentInChildren<Image>().enabled = true;
                break;
            }
        }

        if (completedWords == wordsInPuzzle) //if the completed words is equal to the amount of words in this section, it will do the win event for this controller.
        {
            winEvent.Invoke();
        }
    }

    public void MistakeCounter()
    {
        if (!gameObject.name.Contains("Tutorial")) //won't count mistakes if it is the tutorial
        {
            Debug.Log("MISTAKE");
            mistakeCount++; //updating the mistakes on the UI and in the counter.
            mistakeText.text = mistakeCount.ToString();
        }
        else if (gameObject.name.Contains("Tutorial"))
        {
            tutorialMistakeEvent.Invoke(); //specific event that plays during the tutorial
        }
    }

    void TutorialAltarDisable() //disabling the altars for the tutorial once the player has completed the word
    {
        for(int i = 0; i < storedObjects.Count; i++)
        {
            storedObjects[i].GetComponent<Outline>().enabled = false;
            storedObjects[i].GetComponentInChildren<ObjectPlacement>().enabled = false;
            storedObjects[i].GetComponent<DetermineLetter>().storedObject.GetComponent<Outline>().enabled = false;
            storedObjects[i].GetComponent<DetermineLetter>().storedObject.GetComponent<ObjectHolder>().enabled = false;
        }
    }

    void AudioChecks() //this method just checks if certain audio events need to be played, or if they only need to be played once
    {
        if (playersWord == "TEN" && !tenPlayed)
        {
            tenPlayed = true;

            eventInstance = RuntimeManager.CreateInstance("event:/MR_C_SolvedPuzzles/I.SP.1_Ten");

            eventInstance.start();
        }

        if (playersWord == "PET" && !petPlayed)
        {
            petPlayed = true;

            eventInstance = RuntimeManager.CreateInstance("event:/MR_C_SolvedPuzzles/I.SP.4.1_Pet");

            eventInstance.start();
        }

        if (playersWord == "TROPHY" && !trophyPlayed)
        {
            trophyPlayed = true;

            eventInstance = RuntimeManager.CreateInstance("event:/MR_C_SolvedPuzzles/I.SP.3_Trophy");

            eventInstance.start();

            GetComponent<TutorialSectionStart>().ObjectsTeach();
        }
    }

    public void DevSkip() 
    {
        cheating = true;
        completedWords = wordsInPuzzle;
        CompletionCheck();
        cheating = false;
    }

    //these two methods are for the devs (otherwise known as cheaters)

    public void CompleteCurrentWord()
    {
        playersWord = word;
        cheating = true;
        PlayerWordControl();
        cheating = false;
    }
}
