using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.Events;

public class TutorialController : MonoBehaviour //this script controls all the events that need to happen during the tutorial
{
    JournalController journalController;

    JournalOnSwitch journalOnSwitch;

    Player_Controller playerController;

    OverseerController overseerController;

    EventInstance eventInstance;

    public GameObject[] objectsToSwitchOn;

    public UnityEvent showTabs;

    [SerializeField]
    TextMeshProUGUI conLetter;

    bool directed, doneTalk, spotted, fiveMinute, canPlayFive = true;

    void Start()
    {
        journalController = FindObjectOfType<JournalController>();
        playerController = FindObjectOfType<Player_Controller>();
        journalOnSwitch = FindObjectOfType<JournalOnSwitch>();
        overseerController = FindObjectOfType<OverseerController>();

        StartTutorial();

        StartCoroutine(DoneTalk());
    }

    void Update()
    {
        if(overseerController != null)
        {
            if (overseerController.State == 3 && !spotted) //if the player has been spotted by mr crossy for the first time, the tutorial for it plays
            {
                spotted = true;
                GetComponent<TutorialSectionStart>().sectionStart.Invoke();
            }
        }
    }

    void StartTutorial() //this method starts the tutorial
    {
        playerController.EnableController(); //player controller is enabled, but journal isn't

        journalController.EnableJournal();
        journalController.OpenMap(); //needs to open the map first though
        journalController.DisableJournal();

        eventInstance = RuntimeManager.CreateInstance("event:/MR_C_Tutorial/TUT.0.1");

        eventInstance.start();

        for (int i = 0; i < objectsToSwitchOn.Length; i++)
        {
            objectsToSwitchOn[i].SetActive(true);
        }
    }

    public void DirectToNote() //this method plays a voiceline if the player tries to go in the house without picking up the note at the gate first
    {
        if (!directed && doneTalk) //will only play if they haven't been directed in 15 seconds, or if mr crossy is finished his introduction
        {
            eventInstance = RuntimeManager.CreateInstance("event:/MR_C_Tutorial/TUT.0.2");

            eventInstance.start();

            directed = true;

            StartCoroutine(DirectionTimer()); //starts the timer
        }
    }

    public void ChangeConLetter(string letter) //this changes the letter in one of the UI prompts to whichever letter the player put down on the altar
    {
        conLetter.text = "[" + letter + "]";
    }

    public void StreetNameBlank()
    {
        //PuzzleController puzzleController = FindObjectOfType<PuzzleController>();

        //puzzleController.streetText.text = "";
    }

    public void CrossyWait() //waiting for mr crossy to talk 
    {
        StartCoroutine(WaitForCrossy());
    }

    public void StartFiveMinuteTimer() //after five minutes, mr crossy reveals the word for the player
    {
        StartCoroutine(FiveMinuteTimer());
    }

    public void SetCanPlayFiveBool(bool set) //this is to turn it off if they complete it before five minutes, so it doesn't play after the tutorial
    {
        canPlayFive = set;
    }

    IEnumerator WaitForCrossy() //waits for 5 seconds, for mr crossy to appear over the wall
    {
        yield return new WaitForSeconds(5f);

        eventInstance = RuntimeManager.CreateInstance("event:/MR_C_Tutorial/TUT.0.6");

        eventInstance.start();

        yield return new WaitForSeconds(31.5f); //it then waits this long, as that is how long the voiceline is

        journalController.EnableJournal();
        journalController.readingHowTo = true;

        if (!journalOnSwitch.open)
        {
            journalOnSwitch.OpenOrClose();
        }
        journalController.OpenHowTo(); //opens the how to part of the journal at the end of it
        showTabs.Invoke();
    }
    
    IEnumerator DoneTalk()
    {
        yield return new WaitForSeconds(11f);

        doneTalk = true;

        StopCoroutine(DoneTalk());
    }

    IEnumerator DirectionTimer()
    {
        yield return new WaitForSeconds(15f);

        directed = false;

        StopCoroutine(DirectionTimer());
    }

    IEnumerator FiveMinuteTimer()
    {
        yield return new WaitForSeconds(300f);

        if (!fiveMinute && canPlayFive)
        {
            eventInstance = RuntimeManager.CreateInstance("event:/MR_C_Tutorial/TUT.0.5.1.3");

            eventInstance.start();
        }
    }
}
