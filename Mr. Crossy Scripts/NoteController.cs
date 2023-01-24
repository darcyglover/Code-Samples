using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.UI;

public class NoteController : MonoBehaviour
{
    Player_Controller player;

    JournalController journalController;

    JournalOnSwitch journalOnSwitch;

    Sprite noteImage;

    Color alphaOne;

    int imageNumber;

    bool tutorialLine, found;

    EventInstance eventInstance;

    [SerializeField]
    TextMeshProUGUI prompt;

    void Start()
    {
        journalOnSwitch = FindObjectOfType<JournalOnSwitch>();
        journalController = FindObjectOfType<JournalController>();
        player = FindObjectOfType<Player_Controller>();

        alphaOne.a = 1f;
        alphaOne = Color.white;
    }

    void Update()
    {
        RaycastHit hit;

        if(Physics.Raycast(player.cam.position, player.cam.TransformDirection(Vector3.forward), out hit, 2f))
        {
            if(hit.transform.gameObject.CompareTag("Note")) //will only do stuff if the object that it hit has the tag of Note
            {
                prompt.text = "Press E to read note."; //changes the prompt
                prompt.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E)) //if the player then presses e, it will pick up the note
                {
                    noteImage = hit.transform.gameObject.GetComponent<NoteAssign>().assignedNote; //sets the image to the one that is assigned to it

                    hit.transform.gameObject.SetActive(false); //turns off the note
                    GameObject.Find(hit.transform.gameObject.name).SetActive(false); //finds the other note at the other house and turns that one off too

                    imageNumber = hit.transform.gameObject.GetComponent<NoteAssign>().imageNumber;

                    eventInstance = RuntimeManager.CreateInstance("event:/2D/Paper/Paper Up");

                    eventInstance.start();

                    PickUpNote();

                    if (!tutorialLine && GetComponent<TutorialSectionStart>()) //plays a voiceline if its the first one that the player has picked up
                    {
                        GetComponent<TutorialSectionStart>().NoteTutorialLine();
                        tutorialLine = true;
                    }
                }
            }
            else
            {
                prompt.gameObject.SetActive(false);
            }
        }
        else
        {
            prompt.gameObject.SetActive(false);
        }
    }

    void PickUpNote() //this method is all the journal controller related stuff with picking up the notes
    {
        GameObject[] notes = journalController.notePages;
        for(int i = 0; i < notes.Length; i++)
        {
            Image[] images = notes[i].GetComponentsInChildren<Image>();
            TextMeshProUGUI[] texts = notes[i].GetComponentsInChildren<TextMeshProUGUI>();

            for(int x = 0; x < images.Length; x++)
            {
                if (images[x].sprite == null) //finds the first image sprite that is null, and assigns itself to it
                {
                    images[x].sprite = noteImage;
                    images[x].color = alphaOne; //the alpha needs to be down when it is null, otherwise the player can see a big white sqaure

                    switch (imageNumber) //changes the text based on which note the player picked up, so there is a description of the note in the journal
                    {
                        case 1:
                            {
                                texts[x].text = "A Birthday Card";
                                break;
                            }
                        case 2:
                            {
                                texts[x].text = "An Insurance Letter";
                                break;
                            }
                        case 3:
                            {
                                texts[x].text = "An Autopsy Report";
                                break;
                            }
                        case 4:
                            {
                                texts[x].text = "Potential Pet 1";
                                break;
                            }
                        case 5:
                            {
                                texts[x].text = "Potential Pet 2";
                                break;
                            }
                        case 6:
                            {
                                texts[x].text = "Potential Pet 3";
                                break;
                            }
                    }

                    found = true;
                    break;
                }
            }

            if (found) //breaks after the first one found so that it doesn't apply to every single note page
            {
                break;
            }
        }

        found = false;

        journalController.whichNotesPage = imageNumber;
        journalController.noteList.Add(imageNumber); //adds itself to the journal list

        journalOnSwitch.OpenOrClose(); //opens the journal

        journalController.OpenNotes(); //and then opens to the note page 
    }
}
