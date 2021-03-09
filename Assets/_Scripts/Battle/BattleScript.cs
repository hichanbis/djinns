using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BattleScript : MonoBehaviour
{
    public RuntimeAnimatorController battleAnimController;

    public BattleUnits battleUnits;
    public Character character;
    public bool dead;
    public bool canAct;
    public bool canCast;
    public Animator anim;
    public GameObject damagePopUpPrefab;
    public StatusCollection statusCollection;
  
    void Start()
    {
        dead = false;
        canAct = true;
        canCast = true;

        anim = GetComponent<Animator>();
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
        anim.runtimeAnimatorController = battleAnimController;

    }

    public void SetCharacter(Character character)
    {
        this.character = character;

        character.stats.hpNow.OnStatChanged += HpNowChangedHandler;
    }

    private void HpNowChangedHandler(int hpNow, int dmg)
    {
        dmgPopup(dmg);
        if (AmIAPlayer())
        {
            EventManager.TriggerEvent(BattleEventMessages.PlayerHpChanged.ToString());
        }

        if (dmg > 0)
        {
            //anim.SetTrigger("Healed");
        }
        else
        {
            StartCoroutine(TakeDamageAnim(hpNow));
        }
    }

    //launch anim battleTaunt
    void battleTaunt()
    {
        anim.SetTrigger("Taunt");
    }

    public IEnumerator LaunchChoiceAnim()
    {

        anim.SetTrigger("ChoiceAnim");
        yield return null;
    }

    public IEnumerator LaunchAbilityWithAnim(BattleAction battleAction)
    {
        removeMp(battleAction.ability);

        Vector3 initPos = transform.position;
        Quaternion initRot = transform.rotation;
        //Debug.Log(battleAction.ability);

        if (battleAction.ability.distance.Equals(Distance.Close))
        {
            EventManager.TriggerEvent(BattleEventMessages.MeleeAttack.ToString());

            yield return StartCoroutine(battleAction.fromUnit.RunToTarget(battleAction.targets[0]));

        }


        string trigger;
        if (battleAction.ability.abilityType.Equals(AbilityType.Magic))
            trigger = "CastMagic";
        else
            trigger = battleAction.ability.name;

        AnimatorControllerParameter[] animParams = anim.parameters;
        if (!Array.Exists(animParams, animParam => animParam.name.Equals(trigger)))
        {
            Debug.LogWarning(trigger + " trigger missing in the battle controller. Won't launch the animation");
            BattleManager.Instance.targetImpactReached = true;
            yield break;
        }

        anim.SetTrigger(trigger);



        //wait for anim to start
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(trigger))
        {
            yield return null;
        }

        //en attendant de creer l'animation event
        if (trigger.Equals("CastMagic"))
            BattleManager.Instance.targetImpactReached = true;

        //wait for anim to finish
        while (anim.GetCurrentAnimatorStateInfo(0).IsName(trigger))
        {
            yield return null;
        }


        if (battleAction.ability.distance.Equals(Distance.Close))
        {
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName("WalkBack"))
            {
                yield return null;
            }
            yield return StartCoroutine(MoveToPositionPercentDistance(gameObject, initPos, 2f, 25));
            while (anim.GetCurrentAnimatorStateInfo(0).IsName("WalkBack"))
            {
                yield return null;
            }
        }

        if (battleAction.ability.distance.Equals(Distance.Close))
            battleAction.fromUnit.transform.position = initPos;

        battleAction.fromUnit.transform.rotation = initRot;

    }


    public IEnumerator RunToTarget(BattleScript target)
    {
        yield return StartCoroutine(RotateTowardsPoint(this, target.transform.position, 100f));
        StartCoroutine(RotateTowardsPoint(target, transform.position, 200f));

        anim.SetTrigger("Run");
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            yield return null;
        }
        //plutot que pourcentage je devrais m'arreter à distance de bras vers l'extremité de la cible (a optimiser plus tard)
        yield return StartCoroutine(MoveToPositionPercentDistance(gameObject, target.transform.position, 10f, 85));

    }

    public IEnumerator RotateTowardsPoint(BattleScript objectToMove, Vector3 target, float speed)
    {
        while (true)
        {
            Vector3 dir = (target - objectToMove.transform.position).normalized;
            Quaternion rotTo = Quaternion.LookRotation(dir);

            objectToMove.transform.rotation = Quaternion.RotateTowards(objectToMove.transform.rotation, rotTo, Time.deltaTime * speed);

            if (Vector3.Angle(objectToMove.transform.forward, dir) < 1)
                break;

            yield return null;
        }
    }

    public IEnumerator MoveToPositionPercentDistance(GameObject objectToMove, Vector3 target, float speed, int distancePercent)
    {

        Vector3 diff = target - objectToMove.transform.position;
        float totalDist = diff.magnitude;

        // speed should be 1 unit per second
        while ((target - objectToMove.transform.position).magnitude > (totalDist * (100 - distancePercent) / 100))
        {
            objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, target, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator TakeDamageAnim(int hpNow)
    {
        if (hpNow == 0)
        {
            dead = true;
        }

        if (dead)
        {
            string expectedState = "ERROR";
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Guard"))
                expectedState = "GuardDie";
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("MagicReady"))
                expectedState = "MagicReadyDie";
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("MeleeReady"))
                expectedState = "MeleeReadyDie";
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                expectedState = "IdleDie";

            anim.SetTrigger("Die");
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName(expectedState))
                yield return null;
            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                yield return null;
        }
        else
        {
            string expectedState = "ERROR";
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Guard"))
                expectedState = "GuardHit";
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("MagicReady"))
                expectedState = "MagicReadyHit";
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("MeleeReady"))
                expectedState = "MeleeReadyHit";
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                expectedState = "IdleHit";

            anim.SetTrigger("Hit");
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName(expectedState))
                yield return null;
            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                yield return null;
        }

        DestroyIfDead();


    }

    public void DestroyIfDead()
    {
        if (!AmIAPlayer() && dead)
        {
            Debug.Log(battleUnits);
            Debug.Log(battleUnits.enemyUnits);
            battleUnits.enemyUnits.Remove(this);
            Destroy(gameObject);
        }

    }



    //Adds status and removes status as described in data (heavy removes light, Toxic removes poison, etc.)
    public IEnumerator AddStatus(Status status)
    {
        bool result = status.Add(character);
        Debug.Log(string.Format("Result of status {0} add: {1}", status, result));
        yield return null;
    }

    //on new turn
    public IEnumerator NewTurn()
    {
        foreach (Status status in character.statuses.ToList<Status>())
        {
            status.NewTurn(character);
        }

        yield return null;
            
    }

    public void dmgPopup(int dmg)
    {
        GameObject damagePopup = Instantiate(damagePopUpPrefab, transform);
        damagePopup.GetComponentInChildren<Text>().text = Math.Abs(dmg).ToString();
    }
    
    public void removeMp(Ability ability)
    {
        character.stats.mpNow.SetValue(Mathf.Clamp(character.stats.mpNow.GetValue() - ability.mpCost, 0, character.stats.mp.GetValue()));
    }

    bool AmIAPlayer()
    {
        return (System.Enum.IsDefined(typeof(PlayerName), this.name));
    }

    public bool KnowsMagic()
    {
        return character.abilities.FindIndex(a => a.abilityType.Equals(AbilityType.Magic)) >= 0;
    }

    //animation event listener
    public void RegisterAttackLaunched()
    {
        BattleManager.Instance.targetImpactReached = true;
    }
}
