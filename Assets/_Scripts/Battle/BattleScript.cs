using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class BattleScript : MonoBehaviour
{
    public RuntimeAnimatorController battleAnimController;

    private Character character;
    private bool dead = false;
    private Animator anim;
    private UnityAction endRageStatusListener;
    private UnityAction launchTaunt;
    public GameObject damagePopUpPrefab;
    public bool damageTaken;
    public bool doneApplied;

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

        endRageStatusListener = new UnityAction(ApplyEndRageStatusEffects);
        EventManager.StartListening(BattleEventMessages.EndRageStatusTime.ToString(), endRageStatusListener);
        launchTaunt = new UnityAction(battleTaunt);
        EventManager.StartListening(BattleEventMessages.Taunt.ToString(), launchTaunt);
    }

    //launch anim battleTaunt
    void battleTaunt()
    {
        anim.SetTrigger("Taunt");
    }


    public IEnumerator LaunchAbilityAnim(String trigger)
    {
        AnimatorControllerParameter[] animParams = anim.parameters;


        if (!Array.Exists(animParams, animParam => animParam.name.Equals(trigger)))
        {
            Debug.LogError(trigger + " parameter missing in the battle controller. Won't launch the animation");
            BattleManager.Instance.targetImpactReached = true;
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
        //plutot que pourcentage je devrais m'arreter à distance de bras vers l'extremité de la cible (a optimiser plus tard)
        yield return StartCoroutine(MoveToPositionPercentDistance(gameObject, target.transform.position, 10f, 85));

    }

    public IEnumerator RotateTowardsPoint(GameObject objectToMove, Vector3 target, float speed)
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
    public void ApplyEndRageStatusEffects()
    {
        doneApplied = false;
        for (int i = 0; i < character.statuses.Count; i++)
        {
            Status status = character.statuses[i];

            if (status.id.Equals("Poison"))
            {
                //add a bool and sync BattleManager to all units who have poison... maybe not a message.
                // applyendturn as coroutine serait mieux
                int damage = Mathf.RoundToInt(character.GetStat(StatName.hp).baseValue * ((float)20 / 100));

                StartCoroutine(TakeDamage(damage));
                while (!damageTaken)
                {
                    Debug.Log("waiting damage taken");
                }
                Debug.Log("Done damage taken");
            }

        }
        doneApplied = true;
    }

    public IEnumerator TakeDamage(int dmg)
    {
        
        character.GetStat(StatName.hpNow).baseValue = Mathf.Clamp(character.GetStat(StatName.hpNow).baseValue + dmg, 0, character.GetStat(StatName.hp).GetValue());
        if (character.GetStat(StatName.hpNow).baseValue == 0)
        {
            dead = true;
            //if (!AmIAPlayer())
                //BattleManager.Instance.monsterUnits.Remove(gameObject);
        }
        
        GameObject damagePopup = Instantiate(damagePopUpPrefab, transform);
        damagePopup.GetComponentInChildren<Text>().text = Math.Abs(dmg).ToString();

        //EventManager.TriggerEvent(BattleEventMessages.DamageApplied.ToString());

        if (dmg > 0)
            Debug.Log("should launch heal anim");
        else if (dmg == 0)
            Debug.Log("no dmg");
        //  anim.SetTrigger("Healed");
        else if (dmg < 0 && character.GetStat(StatName.hpNow).baseValue == 0)
        {
            anim.SetTrigger("Die");
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Die"))
                yield return null;
            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                yield return null;
            
        }
        else
        {
            anim.SetTrigger("Hit");
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
                yield return null;
            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                yield return null;
        }

        yield return null;

    }

    public void DestroyIfDead()
    {
        if (!AmIAPlayer() && dead)
        {
            BattleManager.Instance.monsterUnits.Remove(gameObject);
            Destroy(gameObject);
        }
        
    }

    public void TryAddStatus(Status status)
    {
        //Applied if not already present and depending on success rate of the status
        if (!character.statuses.Exists(s => s.GetType() == status.GetType()) && Rng.GetSuccess(status.successRatePercent))
        {
            character.statuses.Add(status);
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

    public void RegisterAttackLaunched()
    {
        BattleManager.Instance.targetImpactReached = true;
    }
}
