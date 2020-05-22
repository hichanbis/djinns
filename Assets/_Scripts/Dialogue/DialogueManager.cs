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
        yield return null;
        validated = false;
        dialogueCanvas.SetActive(true);
        setTalkerName("Cassim");
        sentence.text = "Bonjour ca va? \n Je suis sensé lire le dialogue " + dialogueId;
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(WaitForValidation());
        
        dialogueCanvas.SetActive(false);
    }

    public IEnumerator WaitForValidation()
    {
        
        while (!validated)
        {
            yield return null;
        }
        validated = false;
    }

    void setTalkerName(string name)
    {
        talkerName.text = name;
    }

    // Update is called once per frame
    void Update()
    {

        // seulement si state = dialogue normalement
        if (Input.GetButtonDown("Submit"))
        {
            validated = true;
        }
    }
}
