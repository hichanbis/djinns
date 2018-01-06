using UnityEditor;

[CustomEditor(typeof(DialogueReaction))]
public class DialogueReactionEditor : ReactionEditor
{
    protected override string GetFoldoutLabel ()
    {
        return "Dialogue Reaction";
    }
}
