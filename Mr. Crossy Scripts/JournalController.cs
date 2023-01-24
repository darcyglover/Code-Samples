using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.UI;

public class JournalController : MonoBehaviour //this script controls the journal once the player has it open
{
    [SerializeField]
    GameObject[] logPages;

    public GameObject[] notePages;

    JournalOnSwitch journalOnSwitch;

    Player_Controller player;

    [HideInInspector]
    public List<int> noteList = new List<int>(0); 

    [SerializeField]
    GameObject log, tutMap, gameMap, notes, howTo, rightArrow, leftArrow;

    GameObject mapPage;

    int whichTab = 1;

    [HideInInspector]
    public int whichNotesPage;

    public bool disabled = false, tutorial = true; //the disabled bool is checked before anything is opened, the journal won't work at all if it is true.
    [HideInInspector]
    public bool readingHowTo = false, waitForCrossy = false, logTab, notesTab;
    bool fromArrow;

    EventInstance eventInstance;

    void Start()
    {
        player = FindObjectOfType<Player_Controller>();

        mapPage = tutMap; //setting the tutorial map at the start

        if (!tutorial)
        {
            SetToGameMap(); //sets to game map if the tutorial is already finished
        }
        journalOnSwitch = FindObjectOfType<JournalOnSwitch>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D) && rightArrow.activeInHierarchy) //will only do this if the right arrow is active in the heirarchy, to prevent the player from turning pages when not supposed to
        {
            Arrows(true);
        }
        else if (Input.GetKeyDown(KeyCode.A) && leftArrow.activeInHierarchy) //the same, but for the left arrow
        {
            Arrows(false);
        }

        if(Input.GetKeyDown(KeyCode.Tab)) //specific closing events to happen when scripted events are happening, like when mr crossy is talking to the player
        {
            if (!FindObjectOfType<CrossKeyManager>().puzzleOn)
            {
                if (readingHowTo) //this part occurs when the player is presented with the how to play page of the journal for the first time.
                {
                    readingHowTo = false;

                    GetComponent<TutorialSectionStart>().ReadHowTo();

                    player.EnableController(); //gives the camera control back to the player

                    GetComponent<JournalTimer>().StartTimer(); //the timer starts at this point

                    OpenMap();
                }

                if (waitForCrossy) //this part occurs when mr crossy is talking to the player outside the first gate.
                {
                    waitForCrossy = false;

                    GetComponent<TutorialSectionStart>().WaitForCrossy();

                    player.DisableController(); //stopping the player from moving the camera while mr crossy is talking

                    OpenMap();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.M)) //opening the map using the M key
        {
            if(!FindObjectOfType<CrossKeyManager>().puzzleOn) //making sure that there isn't a crossy key puzzle active
            {
                if (!journalOnSwitch.open) //making sure the jounral isn't already open
                {
                    journalOnSwitch.OpenOrClose();
                }
                OpenMap();
            }
        }

        if (Input.GetKeyDown(KeyCode.N) && notesTab) //opening the notes using the N key, but only after it has been unlocked
        {
            if (!FindObjectOfType<CrossKeyManager>().puzzleOn) 
            {
                if (!journalOnSwitch.open)
                {
                    journalOnSwitch.OpenOrClose();
                }
                OpenNotes();
            }
        }

        if (Input.GetKeyDown(KeyCode.L) && logTab) //opening the log using the L key, but only after it has been unlocked
        {
            if (!FindObjectOfType<CrossKeyManager>().puzzleOn)
            {
                if (!journalOnSwitch.open)
                {
                    journalOnSwitch.OpenOrClose();
                }
                OpenLog();
            }
        }
    }

    public void SetToGameMap()
    {
        mapPage = gameMap;
    }

    public void OpenLog() //this method opens the log
    {
        if (!disabled)
        {
            log.SetActive(true);
            mapPage.SetActive(false);
            notes.SetActive(false);
            howTo.SetActive(false);

            leftArrow.SetActive(false); //no arrows needed for the log
            rightArrow.SetActive(false);

            whichTab = 1;

            for (int i = 0; i < logPages.Length; i++)
            {
                logPages[i].SetActive(true);
            }
        }
    }

    public void OpenMap() //this method opens the map
    {
        if (!disabled && mapPage != null)
        {
            log.SetActive(false);
            mapPage.SetActive(true);
            notes.SetActive(false);
            howTo.SetActive(false);

            rightArrow.SetActive(false); //no arrows needed for map
            leftArrow.SetActive(false);

            whichTab = 2;
        }
    }

    public void OpenNotes() //this method opens the notes, and also checks to see which notes have been picked up by the player, so it can display them
    {
        if (!disabled)
        {
            if (noteList.Count >= 1) //will only do this if there has been at least one note picked up
            {
                log.SetActive(false);
                mapPage.SetActive(false);
                notes.SetActive(true);
                howTo.SetActive(false);

                leftArrow.SetActive(false); //arrows off at the start by default
                rightArrow.SetActive(false);

                whichTab = 3;

                for (int i = 0; i < notePages.Length; i++) //turns off all the note pages frst for simplicity
                {
                    notePages[i].SetActive(false);
                }

                if (!fromArrow) //this for loop below counts the number of image components that are active in the next note page, to see which ones have been picked up
                { //it won't do this if the call for the notes to be opened came from the arrows, though
                    for (int i = 0; i < notePages.Length; i++)
                    {
                        Image[] images = notePages[i].GetComponentsInChildren<Image>();
                        bool found = false;
                        int imageCount = 0;

                        for (int x = 0; x < images.Length; x++)
                        {
                            if (images[x].sprite == null)
                            {
                                imageCount++;
                                found = true; //increments and marks as found whenever it finds at least one image component that is enabled.
                            }
                        }

                        if (found)
                        {
                            if (imageCount == 2) //if it found two in the next page, it means both have been picked up, and stays on the current page
                            {
                                whichNotesPage = i - 1;
                            }
                            else //if it only found one, to goes to the next page
                            {
                                whichNotesPage = i;
                            }
                            break;
                        }
                    }
                }

                fromArrow = false;

                notePages[whichNotesPage].SetActive(true);

                if (whichNotesPage == 0) //if it is on the first page, the left arrow needs to be off, otherwise it can be on
                {
                    leftArrow.SetActive(false);
                }
                else if(whichNotesPage == 1 || whichNotesPage == 2)
                {
                    leftArrow.SetActive(true);
                }

                if(whichNotesPage != 2) //this loops does essentially the same thing as the first one, but checks to see if the next page has an active image component
                { //if it does have one, it will turn the right arrow on, so the player can navigate there
                    for (int i = whichNotesPage + 1; i < notePages.Length; i++)
                    {
                        Image[] images = notePages[i].GetComponentsInChildren<Image>();
                        bool found = false;

                        for (int x = 0; x < images.Length; x++)
                        {
                            if (images[x].sprite != null)
                            {
                                found = true;
                            }
                        }

                        if (found)
                        {
                            rightArrow.SetActive(true);
                        }
                        else if (!found)
                        {
                            rightArrow.SetActive(false);
                        }
                        break;
                    }
                }
                else
                {
                    rightArrow.SetActive(false);
                }             
            }
        }
    }

    public void OpenHowTo() //thie method opens the how to section
    {
        if (!disabled && !tutorial)
        {
            log.SetActive(false);
            mapPage.SetActive(false);
            notes.SetActive(false);
            howTo.SetActive(true);

            leftArrow.SetActive(false);
            rightArrow.SetActive(false);

            whichTab = 4;
        }
    }

    public void Arrows(bool right) //this method controls the arrows
    {
        if (right) //if right is true, it means the player clicked or activated the right arrow, and if its false, the left one
        {
            switch (whichTab) //checking which tab the journal is in
            {
                case 1:
                    {
                        //whichLogPage++;
                        OpenLog();
                        break;
                    }
                case 3:
                    {
                        whichNotesPage++;
                        fromArrow = true;
                        OpenNotes();
                        break;
                    }
            }

            eventInstance = RuntimeManager.CreateInstance("event:/2D/Paper/Paper Turn");

            eventInstance.start();
        }
        else
        {
            switch (whichTab)
            {
                case 1:
                    {
                        //whichLogPage--;
                        OpenLog();
                        break;
                    }
                case 3:
                    {
                        whichNotesPage--;
                        fromArrow = true;
                        OpenNotes();
                        break;
                    }
            }

            eventInstance = RuntimeManager.CreateInstance("event:/2D/Paper/Paper Turn");

            eventInstance.start();
        }
    }

    //the bottom few methods are just public ones that the Unity Events use to turn on or off bools.

    public void DisableJournal()
    {
        disabled = true;
    }

    public void EnableJournal()
    {
        disabled = false;
    }

    public void SetTutorialBool(bool set)
    {
        tutorial = set;
    }

    public void SetCrossyBool(bool set)
    {
        waitForCrossy = set;
    }

    public void SetLogTabBool(bool set)
    {
        logTab = set;
    }

    public void SetNotesTabBool(bool set)
    {
        notesTab = set;
    }
}
