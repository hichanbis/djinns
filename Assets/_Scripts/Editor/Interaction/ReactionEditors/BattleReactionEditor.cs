using UnityEditor;

[CustomEditor(typeof(BattleReaction))]
public class BattleReactionEditor : ReactionEditor
{
    protected override string GetFoldoutLabel ()
    {
        return "Battle Reaction";
    }
}
