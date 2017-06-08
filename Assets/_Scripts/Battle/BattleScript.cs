using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class BattleScript : MonoBehaviour
{
    public RuntimeAnimatorController battleAnimController;

    private Character character;
    private bool dead = false;
    private Animator anim;
    private UnityAction endTurnStatusListener;
    private UnityAction launchTaunt;

    public Animator Anim
    {
        get
        {
            return anim;
        }
    }

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        anim.runtimeAnimatorController = battleAnimController;

        endTurnStatusListener = new UnityAction(ApplyEndTurnStatusEffects);
        EventManager.StartListening(BattleEventMessages.applyEndTurnStatus.ToString(), endTurnStatusListener);
        launchTaunt = new UnityAction(battleTaunt);
        EventManager.StartListening(BattleEventMessages.taunt.ToString(), launchTaunt);
    }

    //launch anim battleTaunt
    void battleTaunt()
    {
        anim.SetTrigger("Taunt");
    }


    public IEnumerator LaunchAndWaitAnim(String trigger)
    {
        AnimatorControllerParameter[] animParams = anim.parameters;

       
        if (!Array.Exists(animParams, animParam => animParam.name.Equals(trigger)))
        {
            Debug.LogError(trigger + " parameter missing in the battle controller. Won't launch the animation");
            yield break;
        }

        anim.SetTrigger(trigger);
        //wait for anim to start
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(trigger))
        {
            yield return null;
        }


    }


    public IEnumerator RunToTarget(GameObject target)
    {
        yield return StartCoroutine(RotateTowardsPoint(gameObject, target.transform.position, 100f));
        StartCoroutine(RotateTowardsPoint(target, transform.position, 200f));

        anim.SetTrigger("Run");
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            yield return null;
        }
        yield return StartCoroutine(MoveToPositionPercentDistance(gameObject, target.transform.position, 10f, 85));

    }

    public IEnumerator RotateTowardsPoint(GameObject objectToMove, Vector3 target, float speed)
    {
        while (true)
        {
            Vector3 dir = (target - objectToMove.transform.position).normalized;
            Quaternion rotTo = Quaternion.LookRotation(dir); 

            objectToMove.transform.rotation = Quaternion.RotateTowards(objectToMove.transform.rotation, rotTo, Time.deltaTime * speed);

            if(Vector3.Angle(objectToMove.transform.forward, dir) < 1)
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



    public Character Character
    {
        get
        {
            return this.character;
        }
        set
        {
            this.character = value;
        }
    }

    public bool Dead
    {
        get
        {
            return this.dead;
        }
        set
        {
            this.dead = value;
        }
    }

    //on end turn
    public void ApplyEndTurnStatusEffects()
    {
        for (int i = 0; i < character.status.Count; i++)
        {
            Status status = character.status[i];
            status.ApplyEndTurn(gameObject);
           
            if (status.Finished) //
            {
                status.Remove(gameObject);
                //character.status.Remove(status);
                status = null;
            }
        }
    }

    public IEnumerator TakeDamage(int dmg)
    {
        character.GetStat(StatName.hpNow).baseValue = Mathf.Clamp(character.GetStat(StatName.hpNow).baseValue + dmg, 0, character.GetStat(StatName.hp).GetValue());
        EventManager.TriggerEvent(BattleEventMessages.damageApplied.ToString());

        //if (dmg > 0)
        //  anim.SetTrigger("Healed");
        //else
        //{
        if (character.GetStat(StatName.hpNow).baseValue == 0)
        {
            anim.SetTrigger("Die");
            dead = true;
            if (!AmIAPlayer())
                BattleManager.Instance.monsterUnits.Remove(gameObject);
            
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Die"))
                yield return null;
            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                yield return null;

            if (!AmIAPlayer())
                Destroy(gameObject);
        }
        else
            anim.SetTrigger("Hit");
        //}
            
        yield return null;
    }

    public void TryAddStatus(Status status)
    {
        //Applied if not already present and depending on success rate of the status
        if (!character.status.Exists(s => s.GetType() == status.GetType()) && Rng.GetSuccess(status.SuccessRatePercent))
        {
            status.Add(gameObject);
            //Debug.Log(gameObject + " status added to me: " + status.GetType());
        }
    }

    public void removeMp(Ability ability)
    {
        character.GetStat(StatName.mpNow).baseValue = Mathf.Clamp(character.GetStat(StatName.mpNow).baseValue - ability.mpCost, 0, character.GetStat(StatName.mp).baseValue);
    }

    bool AmIAPlayer()
    {
        return (System.Enum.IsDefined(typeof(PlayerName), this.name));
    }

    public bool KnowsMagic()
    {
        return character.abilities.FindIndex(a => a.abilityType.Equals(AbilityType.Magic)) >= 0;
    }

    public void RegisterAttackLaunched(){
        BattleManager.Instance.attackLaunched = true;
    }
}
