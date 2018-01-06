using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    public GameObject dialogueCanvas;
    public Text talkerName;
    public Text sentence;
    public GameObject LeftActor;
    public GameObject RightActor;
    bool validated;

    // Use this for initialization
    void Start()
    {
        validated = false;
    }

    public void PlayDialogue(string dialogueId)
    {
        StartCoroutine(PlayDialogueCr(dialogueId));
      
    }

    public IEnumerator PlayDialogueCr(string dialogueId)
    {
        dialogueCanvas.SetActive(true);
        setTalkerName("Cassim");
        sentence.text = "Bonjour ca va? \n Je suis sensé lire le dialogue " + dialogueId;
        yield return StartCoroutine(WaitForValidation());
        validated = false;
        dialogueCanvas.SetActive(false);
    }

    public IEnumerator WaitForValidation()
    {
        
        while (!validated)
        {
            yield return null;
        }
    }

    void setTalkerName(string name)
    {
        talkerName.text = name;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Submit"))
        {
            validated = true;
        }
    }
}
