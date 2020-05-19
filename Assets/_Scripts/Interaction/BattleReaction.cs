using UnityEngine;

public class BattleReaction : DelayedReaction
{
    private SceneController sceneController;
    private Transform playerTr;
    public GameProgress gameProgress;

    protected override void SpecificInit()
    {
        sceneController = FindObjectOfType<SceneController>();
        playerTr = GameObject.FindGameObjectWithTag("Player").transform;
    }


    protected override void ImmediateReaction()
    {
        gameProgress.position = playerTr.position;
        sceneController.FadeAndLoadScene("BattleTest");
    }
}