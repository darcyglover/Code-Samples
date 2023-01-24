using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//Written by Darcy Glover

public class LightManager : MonoBehaviour
{
    public int lightCount = 0;

    string masterPlayerName, otherPlayername;

    public GameObject[] feedbackLights, visibleLights;

    PhotonView photonView;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void TurnOffAllLights()
    {
        photonView.RPC("RPC_TurnOffAllLights", RpcTarget.All);
    }

    public void SendLightUsage()
    {
        photonView.RPC("RPC_SendLightUsage", RpcTarget.All);
    }

    public void CountUp()
    {
        lightCount++;
    }

    public void CountDown()
    {
        lightCount--;
    }

    [PunRPC]
    void RPC_TurnOffAllLights()
    {
        Debug.Log("turning off");
        for (int i = 0; i < visibleLights.Length; i++) //this is for puzzle 3, to turn off all the lights at the same time
        {
            if (!visibleLights[i].GetComponent<Light>())
            {
                visibleLights[i].GetComponentInChildren<Light>().enabled = false;
            }
            else
            {
                visibleLights[i].GetComponent<Light>().enabled = false;
            }
            visibleLights[i].GetComponent<LightControl>().SpriteOff();
            visibleLights[i].GetComponent<LightControl>().FeedbackLightOff();
            lightCount = 0;
        }
    }

    [PunRPC]
    void RPC_SendLightUsage()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            DarcyAnalyticMethods analyticMethods = FindObjectOfType<DarcyAnalyticMethods>();

            PuzzleCompletionManager puzzleCompletionManager = FindObjectOfType<PuzzleCompletionManager>();

            masterPlayerName = PhotonNetwork.MasterClient.NickName;
            otherPlayername = PhotonNetwork.LocalPlayer.NickName;

            string names = masterPlayerName + " and " + otherPlayername; 

            int level = puzzleCompletionManager.GetCurrentLevel();

            for(int i = 0; i < visibleLights.Length; i++)
            {
                analyticMethods.LightsTurnedOn(visibleLights[i].GetComponent<LightControl>().lightUsed, visibleLights[i].gameObject.name, names, level);
            }
        }
    }
}
