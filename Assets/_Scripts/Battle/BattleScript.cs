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

    public Character character;
    public bool dead;
    public bool canAct;
    public bool canCast;
    public Animator anim;
    public GameObject damagePopUpPrefab;
    public bool damageTaken;
    public bool doneApplied;
    public StatusCollection statusCollection;
    public Dictionary<string, int> battleStatusesDurations;


    void Start()
    {
        dead = false;
        canAct = true;
        canCast = true;
        battleStatusesDurations = new Dictionary<string, int>();
        //pour l'instant ça c toujours vide donc on rentre pas dans le foreach
        foreach (Status status in character.statuses)
        {   
            battleStatusesDurations.Add(status.id, 0);
        }

        anim = GetComponent<Animator>();
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
        anim.runtimeAnimatorController = battleAnimController;


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

            yield return StartCoroutine(battleAction.fromUnit.GetComponent<BattleScript>().RunToTarget(battleAction.targets[0]));

        }


        string trigger;
        if (battleAction.ability.abilityType.Equals(AbilityType.Magic))
            trigger = "CastMagic";
        else
            trigger = battleAction.ability.id;

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

    public IEnumerator TakeDamage(int dmg)
    {

        character.stats.hpNow.baseValue = Mathf.Clamp(character.stats.hpNow.GetValue() + dmg, 0, character.stats.hp.GetValue());
        if (character.stats.hpNow.GetValue() == 0)
        {
            dead = true;
        }

        GameObject damagePopup = Instantiate(damagePopUpPrefab, transform);
        damagePopup.GetComponentInChildren<Text>().text = Math.Abs(dmg).ToString();

        EventManager.TriggerEvent(BattleEventMessages.DamageApplied.ToString());

        if (dmg > 0)
            Debug.Log("should launch heal anim");
        //  anim.SetTrigger("Healed");
        else if (dead)
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
            BattleManager.Instance.monsterUnits.Remove(gameObject);
            Destroy(gameObject);
        }

    }


    public bool CanAddStatus(Status status)
    {
        status.successRatePercent = 100; //hack for debug
        bool blocked = false;
        foreach (string blockerStatusId in status.blockedByStatuses)
        {
            foreach (string presentStatusId in battleStatusesDurations.Keys.ToList())
            {
                if (blockerStatusId.Equals(presentStatusId))
                    blocked = true;
            }
        }
        Debug.Log("Contains key" + battleStatusesDurations.ContainsKey(status.id));
        return !battleStatusesDurations.ContainsKey(status.id) && Rng.GetSuccess(status.successRatePercent) && !blocked;

    }

    //Adds status and removes status as described in data (heavy removes light, Toxic removes poison, etc.)
    public IEnumerator AddStatus(Status status)
    {
        foreach (string presentStatusId in battleStatusesDurations.Keys.ToList())
        {
            foreach (string statusIdToRemove in status.removesStatusesOnAdd)
            {
                if (statusIdToRemove.Equals(presentStatusId))
                    RemoveStatus(presentStatusId);
            }
        }

        battleStatusesDurations.Add(status.id, 0);
        if (status.applyMoment.Equals(StatusApplyMoment.add))
            yield return StartCoroutine(ApplyStatus(status));
        
    }

    //on end turn
    public IEnumerator ApplyEndRageStatusEffects()
    {

        foreach (string statusId in battleStatusesDurations.Keys.ToList())
        {
            Status status = statusCollection.FindStatusFromId(statusId);
            if (status.applyMoment.Equals(StatusApplyMoment.endTurn))
            {
                yield return StartCoroutine(ApplyStatus(status));
            }

            battleStatusesDurations[statusId]++;

            if (battleStatusesDurations[statusId] >= status.maxTurns)
                RemoveStatus(statusId);
        }
    }

    public IEnumerator ApplyStatus(Status status)
    {
        if (status.applyType.Equals(StatusApplyType.damage))
        {
            //base value car on veut appliquer le poison sur la stat hpmax reelle pas la boostée
            int damage = Mathf.RoundToInt(character.stats.hp.baseValue * ((float)status.powerPercent / 100));
            yield return StartCoroutine(TakeDamage(damage));
        }
        else if (status.applyType.Equals(StatusApplyType.modifier))
            character.GetStat(status.statName).modifiers.Add(status.powerPercent);
        else if (status.applyType.Equals(StatusApplyType.disableCommands))
            canAct = false;
        else if (status.applyType.Equals(StatusApplyType.disableMagic))
            canCast = false;

        yield return null;

    }

    public void RemoveStatus(string statusId)
    {
        Status status = statusCollection.FindStatusFromId(statusId);
        if (status.applyType.Equals(StatusApplyType.modifier))
            character.GetStat(status.statName).modifiers.Remove(status.powerPercent);
        else if (status.applyType.Equals(StatusApplyType.disableCommands))
            canAct = true;
        if (status.applyType.Equals(StatusApplyType.disableMagic))
            canCast = true;
        battleStatusesDurations.Remove(statusId);
    }


    public void removeMp(Ability ability)
    {
        character.stats.mpNow.baseValue = Mathf.Clamp(character.stats.mpNow.GetValue() - ability.mpCost, 0, character.stats.mp.GetValue());
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
