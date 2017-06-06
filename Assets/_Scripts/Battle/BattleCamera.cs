using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class BattleCamera : MonoBehaviour
{
    public float xOffset = 0f;
    public float yOffset = 4f;
    public float zOffset = -5.5f;

    private BattleManager battleManager;
    private bool lookAtTarget = false;
    private bool lookAtActingUnit = false;
    private bool moveBehindPlayer = false;
    private bool lookAtEnemy = false;

    private UnityAction playerUnitsExistListener;
    private UnityAction playerChoiceExpectedListener;
    private UnityAction targetsPanelDisplayedListener;
    private UnityAction unitsLoadedListener;


    // EXPECTED BEHAVIOUR

    // INIT
    // Units Loaded - 1 overview apres load pendant le taunt

    // ACTIONCHOICE
    // Player Choice Expected - move devant player - player au centre écran
    // Target Choice Expected - move devant target pendant choice target (target au centre écran)
    // Targets Choice Players - move devant les players
    // Targets Choice Monsters - move devant les monsters
    // Action Choice All Done - montre battlefield de derriere les joueurs pendant 1 sec apres choice

    // RAGE
    // Melee attack - 
    // Attaque melee
    // Dans un premier temps 
    // simple vue de coté par rapport à acting unit (en follow et look at acting puis target)
    // Phase 2
    // move derriere player et follow début rage juska attak quand acting est joueur et target unique
    // move devant enemy et follow juska moitié attak puis vue de coté pendant rage quand acting est enemy et target unique

    // Si pas melee
    // Other Attack - move devant player pendant anim de cast

    // Take Damage - move devant target quand affiche damage si target est unique
    // Take Damage All - move dans les targets quand affiche damage si multiple

    // VICTORY OU FAILURE
    // Battle Ended - move devant les players quand battle finished

    // Use this for initialization
    void Start()
    {
        battleManager = BattleManager.Instance;

        playerChoiceExpectedListener = new UnityAction(MoveBehindPlayer);
        EventManager.StartListening(BattleEventMessages.playerChoiceExpected.ToString(), playerChoiceExpectedListener);

        targetsPanelDisplayedListener = new UnityAction(LookAtTarget);
        EventManager.StartListening(BattleEventMessages.targetsPanelDisplayed.ToString(), targetsPanelDisplayedListener);

        unitsLoadedListener = new UnityAction(RotateAnim);
        EventManager.StartListening(BattleEventMessages.unitsLoaded.ToString(), unitsLoadedListener);
    }

    void SetInitialCamPos()
    {

    }

    void RotateAnim()
    {
        GetComponent<Animation>().Play();
    }

    void MoveBehindPlayer()
    {
        moveBehindPlayer = true;
        lookAtActingUnit = false;
        lookAtTarget = false;
        lookAtEnemy = true;
    }

    void LookAtTarget()
    {
        lookAtActingUnit = false;
        lookAtTarget = true;
        lookAtEnemy = false;
    }

    void LateUpdate()
    {
        if (lookAtTarget && battleManager.currentTargetUnit != null)
            transform.LookAt(battleManager.currentTargetUnit.transform);
        else if (lookAtActingUnit && battleManager.currentActingUnit != null)
            transform.LookAt(battleManager.currentActingUnit.transform);
        else if (lookAtEnemy && battleManager.GetCurrentEnemy() != null)
            transform.LookAt(battleManager.GetCurrentEnemy().transform);

        if (moveBehindPlayer && battleManager.currentActingUnit != null)
        {
            Vector3 offset = new Vector3(xOffset, yOffset, zOffset);
            //float desiredAngle = battleManager.currentActingUnit.transform.eulerAngles.y;
            //Quaternion rotation = Quaternion.Euler(0, desiredAngle, 0);
            Quaternion rotation = Quaternion.Euler(0, 0, 0);
            transform.position = battleManager.currentActingUnit.transform.position + (rotation * offset);
            moveBehindPlayer = false;
        }
    }
}
