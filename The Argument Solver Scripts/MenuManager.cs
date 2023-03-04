using System.Collections;
using DG.Tweening;
using UnityEngine;
using MText;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Transform _victoryScreen, _lossScreen, _base, _pressAnyText;
    [SerializeField] float _cycleLength;

    public Transform[] transforms;

    [SerializeField] Camera _mainCamera;

    //Stuff Darcy added
    bool passedTitle;

    [SerializeField] string[] menuSections; //this is purely for reference, so we know how many sections we have / the names of the sections
    public string currentMenuSection;
    
    public bool playing;

    GameManager gameMan;

    WeaponSelect weaponSelect;

    RelayManager relayMan;

    PlayerData playerData;

    CameraManager cameraMan;

    void Awake()
    {
        gameMan = FindObjectOfType<GameManager>();

        playerData = FindObjectOfType<PlayerData>();

        relayMan = FindObjectOfType<RelayManager>();

        weaponSelect = FindObjectOfType<WeaponSelect>();

        cameraMan = FindObjectOfType<CameraManager>();
    }

    void Update()
    {
        if (Input.anyKeyDown && !passedTitle)
        {
            passedTitle = true;

            _base.gameObject.SetActive(false);
            _pressAnyText.gameObject.SetActive(false);

            MainSection(currentMenuSection);
        }

        if (Input.GetMouseButtonUp(0))
        {         
            OnClick();
        }
    }

    public void MainSection(string previousSection)
    {
        currentMenuSection = "Main";

        switch (previousSection)
        {
            case "Title":
                {
                    MoveTransform('y', new Vector3(0, -3.2f, 0), _cycleLength, false, "Select Client");
                    MoveTransform('y', new Vector3(0, -2.8f, 0), _cycleLength, false, "Select Host");
                    MoveTransform('y', new Vector3(0, -5f, 0), _cycleLength, false, "Client");
                    MoveTransform('y', new Vector3(0, -4.6f, 0), _cycleLength, false, "Host");

                    RotateTransform(new Vector3(0, -360, 0), 3, RotateMode.WorldAxisAdd, "Host", false);
                    RotateTransform(new Vector3(0, 360, 0), 3, RotateMode.WorldAxisAdd, "Client", false);
                    break;
                }
            case "Host Game Setup":
                {
                    MoveTransform('y', new Vector3(0, -3.2f, 0), _cycleLength, false, "Select Client");
                    MoveTransform('y', new Vector3(0, -5f, 0), _cycleLength, false, "Client");
                    MoveTransform('y', new Vector3(0f, -2.9f, 0f), _cycleLength, false, "Select Host");
                    MoveTransform('a', new Vector3(9.5f, -4.6f, 2f), _cycleLength, false, "Host");

                    RotateTransform(new Vector3(0, -360, 0), 3, RotateMode.WorldAxisAdd, "Client", false);

                    ExitSection(previousSection);
                    break;
                }
            case "Join Game Setup":
                {
                    MoveTransform('y', new Vector3(0, -3.2f, 0), _cycleLength, false, "Select Client");
                    MoveTransform('y', new Vector3(0, -2.9f, 0), _cycleLength, false, "Select Host");
                    MoveTransform('a', new Vector3(9.5f, -4.6f, 1.9f), _cycleLength, false, "Host");
                    MoveTransform('a', new Vector3(13f, -3.75f, 0), 1, false, "Join Lobby Button");

                    RotateTransform(new Vector3(0, 360, 0), 3, RotateMode.WorldAxisAdd, "Host", false);

                    ExitSection(previousSection);
                    break;
                }
            case "Victory Screen":
            case "Loss Screen":
                {
                    break;
                }
        }
    }

    public void GameSetup(bool isHost)
    {
        if (isHost)
        {
            currentMenuSection = "Host Game Setup";

            cameraMan.LobbySection(currentMenuSection);

            MoveTransform('a', new Vector3(8.75f, -2f, 0.5f), _cycleLength, false, "Host");
            MoveTransform('a', new Vector3(9.7f, -3.65f, 6.5f), _cycleLength, false, "The Problem");
            MoveTransform('a', new Vector3(9.3f, -3.7f, 5.75f), _cycleLength, false, "Argument");
            MoveTransform('a', new Vector3(9.2f, -5.1f, 6f), _cycleLength, false, "The Solution");
            MoveTransform('a', new Vector3(9.5f, -6.4f, 6.2f), _cycleLength, false, "Weapon Previews");
            MoveTransform('a', new Vector3(11f, -6f, 4.3f), _cycleLength, false, "Create Lobby Button");

            RotateTransform(new Vector3(0, 360, 0), 4, RotateMode.LocalAxisAdd, "Weapon Previews", true);

            weaponSelect.rightArrow.SetActive(true);

            InputFieldToggle("Argument - Input Field", true);
        }
        else
        {
            currentMenuSection = "Join Game Setup";

            cameraMan.LobbySection(currentMenuSection);

            gameMan.JoinGameSetup();

            MoveTransform('a', new Vector3(10.5f, -3.4f, 3.7f), _cycleLength, false, "Courtroom Number");
            MoveTransform('a', new Vector3(8.25f, -5f, 6f), _cycleLength, false, "Client");

            InputFieldToggle("Lobby Code - Input Field", true);
        }
    }

    public void ClientJoinTheLobby(string previousSection) //this is for Host to see the client on their screen
    {
        switch (previousSection)
        {
            case "Host Game Lobby":
                {
                    currentMenuSection = "Both Players In Lobby";

                    cameraMan.LobbySection(currentMenuSection);

                    MoveTransform('a', new Vector3(7.3f, -5.1f, 5.3f), 2, false, "Client");
                    MoveTransform('a', new Vector3(8, -3.8f, 6.6f), 2, false, "Client Stance");
                    MoveTransform('y', new Vector3(0, 5, 0), 4, false, "Lobby Code Generator");

                    RotateTransform(new Vector3(0, 180, 0), 3, RotateMode.Fast, "Client", false);

                    break;
                }
        }
    }

    public void GameLobby(bool isHost)
    {
        if (isHost)
        {
            currentMenuSection = "Host Game Lobby";

            cameraMan.LobbySection(currentMenuSection);

            MoveTransform('a', new Vector3(10.5f, -2.75f, 5f), _cycleLength, false, "Argument");
            MoveTransform('a', new Vector3(8.5f, -3.5f, 0f), _cycleLength, false, "Host");
            MoveTransform('a', new Vector3(9.5f, -2f, 0.3f), _cycleLength, false, "Host Stance");
            MoveTransform('y', new Vector3(0f, -2.25f, 0f), 3, false, "Lobby Code Generator");
            MoveTransform('a', new Vector3(9.5f, -4f, 4.25f), _cycleLength, false, "Weapon Previews");

            RotateTransform(new Vector3(22, -315, 0), _cycleLength, RotateMode.Fast, "Argument", false);
          
            InputFieldToggle("Argument - Input Field", false);
            InputFieldToggle("Host Stance - Input Field", true);
        }
        else
        {
            currentMenuSection = "Both Players In Lobby";

            cameraMan.LobbySection(currentMenuSection);

            MoveTransform('a', new Vector3(10.9f, -9.1f, 4.1f), _cycleLength, false, "Courtroom Number");
            MoveTransform('a', new Vector3(8.5f, -3.7f, 6.6f), _cycleLength, false, "Client Stance");
            MoveTransform('a', new Vector3(7.3f, -5.1f, 5.3f), _cycleLength, false, "Client");
            MoveTransform('a', new Vector3(8.5f, -3.5f, 0f), _cycleLength, false, "Host");
            MoveTransform('a', new Vector3(9.5f, -2f, 0.3f), _cycleLength, false, "Host Stance");
            MoveTransform('a', new Vector3(9.5f, -4f, 4.25f), _cycleLength, false, "Weapon Previews");
            MoveTransform('a', new Vector3(10.5f, -2.75f, 5f), _cycleLength, false, "Argument");

            RotateTransform(new Vector3(0, 360, 0), 4, RotateMode.LocalAxisAdd, "Weapon Previews", true);
            RotateTransform(new Vector3(22, -315, 0), _cycleLength, RotateMode.Fast, "Argument", false);

            InputFieldToggle("Client Stance - Input Field", true);
        }
    }

    public void ExitSection(string previousSection, bool isHost)
    {
        switch (previousSection)
        {
            case "Main":
                {
                    if (isHost)
                    {
                        //MoveTransform('a', new Vector3(8.75f, -2f, 0.5f), _cycleLength, false, "Host");
                        MoveTransform('y', new Vector3(0f, 3f, 0f), _cycleLength, false, "Select Host");

                        MoveTransform('y', new Vector3(0f, 3f, 0f), _cycleLength, false, "Select Client");
                        MoveTransform('y', new Vector3(0f, -8f, 0f), _cycleLength, false, "Client");

                        RotateTransform(new Vector3(0, -360, 0), 3, RotateMode.WorldAxisAdd, "Client", false);
                    }
                    else
                    {
                        MoveTransform('y', new Vector3(0f, 3f, 0f), _cycleLength, false, "Select Host");
                        MoveTransform('y', new Vector3(0f, 3f, 0f), _cycleLength, false, "Select Client");
                        MoveTransform('y', new Vector3(0f, -8f, 0f), _cycleLength, false, "Host");

                        RotateTransform(new Vector3(0, 360, 0), 3, RotateMode.WorldAxisAdd, "Host", false);
                    }
                    break;
                }
        }
    }

    public void ExitSection(string previousSection)
    {
        switch (previousSection) 
        {
            case "Host Game Setup":
                {
                    MoveTransform('a', new Vector3(0f, -3.3f, 6.5f), _cycleLength, false, "The Problem");
                    MoveTransform('a', new Vector3(3.5f, -5.5f, 7f), _cycleLength, false, "The Solution");
                    MoveTransform('a', new Vector3(3.5f, -5.5f, 7f), _cycleLength, false, "Weapon Previews");
                    MoveTransform('a', new Vector3(0f, -4.2f, 6f), _cycleLength, false, "Argument");
                    MoveTransform('a', new Vector3(14f, -8f, -3f), _cycleLength, false, "Create Lobby Button");

                    InputFieldToggle("Argument - Input Field", false);

                    break;
                }
            case "Join Game Setup":
                {
                    MoveTransform('a', new Vector3(11f, -9f, 4f), _cycleLength, false, "Courtroom Number");
                    MoveTransform('a', new Vector3(7.8f, -5f, 4.75f), _cycleLength, false, "Client");

                    InputFieldToggle("Lobby Code - Input Field", false);

                    break;
                }
            case "Host Game Lobby":
                {

                    MoveTransform('a', new Vector3(8.75f, -2f, 0.5f), 1, false, "Host");
                    MoveTransform('y', new Vector3(0f, -10f, 0f), 4, false, "Lobby Code Generator");
                    MoveTransform('a', new Vector3(13f, -6f, -4f), _cycleLength, false, "Host Stance");
                    MoveTransform('a', new Vector3(2f, -2.2f, 4f), _cycleLength, false, "Client Stance");

                    RotateTransform(new Vector3(20, 29, -3.1f), _cycleLength, RotateMode.Fast, "Argument", false);

                    InputFieldToggle("Host Stance - Input Field", false);


                    break;
                }
            case "Both Players In Lobby":
                {                
                    MoveTransform('a', new Vector3(13f, -6f, -4f), _cycleLength, false, "Host");
                    MoveTransform('a', new Vector3(13f, -6f, -4f), _cycleLength, false, "Host Stance");
                    MoveTransform('y', new Vector3(0, -12, 0), _cycleLength, false, "Client");
                    MoveTransform('y', new Vector3(0, -11, 0), _cycleLength, false, "Client Stance");
                    MoveTransform('y', new Vector3(0, -7.8f, 0), _cycleLength, false, "Weapon Previews");
                    MoveTransform('y', new Vector3(0, -1.4f, 0), _cycleLength, false, "Argument");

                    //RotateTransform(new Vector3(20, 29, -3.1f), _cycleLength, RotateMode.Fast, "Argument", false);

                    InputFieldToggle("Client Stance - Input Field", false);

                    break;
                }
        }
    }


    public void MoveTransform(char axis, Vector3 target, float duration, bool snapping, string transformName)
    {
        Transform transformToMove = null;

        foreach(var t in transforms) //instead of setting each transform manually, we can have an array of all the transforms, and just search for the one we want each time
        {
            if(t.name == transformName)
            {
                transformToMove = t;
            }
        }

        if(transformToMove == null) //if the transform is still null, something went wrong
        {
            Debug.LogError("The sent name, " + transformName + ", did not match one in the Transform array.");
        }

        switch (axis) //'a' stands for 'all', as in all axes
        {
            case 'a':
                {
                    transformToMove.DOMove(target, duration, snapping);

                    break;
                }
            case 'x':
                {
                    transformToMove.DOMoveX(target.x, duration, snapping);

                    break;
                }
            case 'y':
                {
                    transformToMove.DOMoveY(target.y, duration, snapping);

                    break;
                }
            case 'z':
                {
                    transformToMove.DOMoveZ(target.z, duration, snapping);

                    break;
                }
        }
    }

    public void RotateTransform(Vector3 target, float duration, RotateMode rotateMode, string transformName, bool loops)
    {
        Transform transformToRotate = null;

        foreach(var t in transforms) //instead of setting each transform manually, we can have an array of all the transforms, and just search for the one we want each time
        {
            if(t.name == transformName)
            {
                transformToRotate = t;
            }
        }

        if(transformToRotate == null) //if the transform is still null, something went wrong
        {
            Debug.LogError("The sent name, " + transformName + ", did not match one in the Transform array.");
        }

        if (loops)
        {
            transformToRotate.DORotate(target, duration, rotateMode).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
        }
        else
        {
            transformToRotate.DORotate(target, duration, rotateMode);
        }
    }

    public void InputFieldToggle(string inputFieldName, bool toggleOn)
    {
        MText_UI_InputField inputField = null;

        foreach(var i in transforms)
        {
            if (i.GetComponent<MText_UI_InputField>())
            {
                if(i.name == inputFieldName)
                {
                    inputField = i.GetComponent<MText_UI_InputField>();
                }
            }
        }

        if (inputField == null) //if the transform is still null, something went wrong
        {
            Debug.LogError("The sent name, " + inputFieldName + ", did not match one in the Transform array.");
        }

        if (toggleOn)
        {
            inputField.interactable = true;
            inputField.Focus(true);
        }
        else
        {
            inputField.interactable = false;
            inputField.Focus(false);
        }
    }

    public void AllInputFieldsToggle(bool toggleOff)
    {
        MText_UI_InputField[] inputFields = FindObjectsOfType<MText_UI_InputField>();

        if (toggleOff)
        {
            foreach (var i in inputFields)
            {
                i.interactable = false;
                i.Focus(false);
            }
        }
        else
        {
            foreach (var i in inputFields)
            {
                i.interactable = true;
                i.Focus(true);
            }
        }
    }

    [SerializeField] Ray _ray;
    [SerializeField] RaycastHit _hit;

    public void OnClick()   
    {
        if (playing)
        {
            return;
        }


        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(_ray, out _hit, 1000))
        {
            if (_hit.collider.CompareTag("Design Lobby"))
            {
                if(currentMenuSection == "Host Game Setup" || currentMenuSection == "Host Game Lobby" || currentMenuSection == "Both Players In Lobby")
                {
                    return;
                }

                ExitSection(currentMenuSection, true);

                GameSetup(true);
            }

            if (_hit.collider.CompareTag("Join Lobby Setup"))
            {
                if (currentMenuSection == "Join Game Setup" || currentMenuSection == "Both Players In Lobby")
                {
                    return;
                }

                ExitSection(currentMenuSection, false);

                GameSetup(false);
            }

            if (_hit.collider.CompareTag("Join Lobby"))
            {
                gameMan.JoinGame();           
            }

            if (_hit.collider.CompareTag("Create Lobby"))
            {
                ExitSection(currentMenuSection);

                GameLobby(true);

                gameMan.HostGameSetup();

                gameMan.joinCodeText.gameObject.SetActive(true);
            }

            if(_hit.collider.CompareTag("Arrow"))
            {
                weaponSelect.ArrowClicked(_hit.collider.name);
            }

            if(_hit.collider.CompareTag("Quit Game"))
            {
                relayMan.EndSession();

                Application.Quit();
            }

            if (_hit.collider.CompareTag("Back To Menu"))
            {
                _victoryScreen.gameObject.SetActive(false);
                _lossScreen.gameObject.SetActive(false);

                gameMan.joinCodeInput.GetComponent<MText_UI_InputField>().Text = "";

                MainSection(currentMenuSection);

                gameMan.isLeft = false;

                relayMan.EndSession();
            }
        }
    }

    public void PlayerWon()
    {
        gameMan.menuCamera.SetActive(true);

        _victoryScreen.gameObject.SetActive(true);
    }

    public void PlayerLost()
    {
        gameMan.menuCamera.SetActive(true);

        _lossScreen.gameObject.SetActive(true);
    }

    public void TurnOffTexts() //might need this to turn off the objects sometime?
    {
        foreach(var t in transforms)
        {
            t.gameObject.SetActive(false);
        }
    }

    //public void SavePlayerData()
    //{
    //    playerData.theProblem = _argumentText.GetComponentInChildren<MText_UI_InputField>().Text; //the problem
    //    playerData.hostStance = _hostStance.GetComponentInChildren<MText_UI_InputField>().Text; //host's stance
    //    playerData.clientStance = _clientStance.GetComponentInChildren<MText_UI_InputField>().Text; //client's stance
    //}

    IEnumerator FadeOut(GameObject objectToFade, float time) //set this up in future to be able to fade out objects, for mvp is fine to just instant deactivate
    {
        yield return new WaitForSeconds(time);       
    }
}
