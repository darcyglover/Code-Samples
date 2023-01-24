using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlappedAltar : MonoBehaviour //this script handles situations where a word overlaps with another one
{
    GameObject placedObject;
 
    public void ReceiveObject(GameObject sentObject) //after an object is placed, it will receive it here 
    {
        placedObject = sentObject;
        SendToChildren();
    }

    public void ObjectPickedUp()
    {
        DetermineLetter[] childrenSend = GetComponentsInChildren<DetermineLetter>();

        for (int i = 0; i < childrenSend.Length; i++) //does the same thing, but lets each child know that the object was picked up.
        {
            childrenSend[i].ObjectPickedUp();
        }
    }

    void SendToChildren() //this method sends the placed object to both of its children, which have determine letter components.
    {
        DetermineLetter[] childrenSend = GetComponentsInChildren<DetermineLetter>();

        for(int i = 0; i < childrenSend.Length; i++) //this is purely so that each determine letter script can handle its own word, rather than one script juggling two.
        {
            childrenSend[i].ObjectPlaced(placedObject);
        }
    }
}
