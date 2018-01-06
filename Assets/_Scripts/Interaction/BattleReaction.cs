using UnityEngine;

public class BattleReaction : DelayedReaction
{
    private SceneController sceneController;
    private Transform playerTr;

    protected override void SpecificInit()
    {
        sceneController = FindObjectOfType<SceneController>();
        playerTr = GameObject.FindGameObjectWithTag("Player").transform;
    }


    protected override void ImmediateReaction()
    {
        GameProgress.Instance.position = playerTr.position;
        sceneController.FadeAndLoadScene("BattleTest");
    }
}