using UnityEngine;

public class DialogueReaction : DelayedReaction
{
    public string dialogueID;
    private DialogueManager dialogueManager;

    protected override void SpecificInit()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();

    }


    protected override void ImmediateReaction()
    {
        dialogueManager.PlayDialogue(dialogueID);
    }
}