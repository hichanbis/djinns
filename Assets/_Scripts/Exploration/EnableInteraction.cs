using UnityEngine;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class EnableInteraction : MonoBehaviour
{
    bool canInteract;
    Interactable interactable;

    void Start(){
        canInteract = false;
        interactable = GetComponent<Interactable>();
    }

    void OnTriggerEnter(Collider other)
    {
        //prompt button displayed and enable player to click on play
         if (other.tag == "Player")
        {
            Debug.Log("Can interact");
            canInteract = true;

        }
    }

    void OnTriggerExit(Collider other)
    {
     
        if (other.tag == "Player")
        {
            Debug.Log("Cannot interact");
            canInteract = false;
        }
    }

    void Update()
    {
        if (canInteract && Input.GetButtonDown("Submit"))
        {
            //if interactionLocation is defined, move the player to the location
            //canInteract = false;
            interactable.Interact();
        }
    }
}
