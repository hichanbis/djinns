using UnityEngine;

public class BattleReaction : DelayedReaction
{
    private SceneController sceneController;

    protected override void SpecificInit()
    {
        sceneController = FindObjectOfType<SceneController>();
    }


    protected override void ImmediateReaction()
    {
        //GameProgress.Instance.position = playerPosition;
        sceneController.FadeAndLoadScene("BattleTest");
    }
}