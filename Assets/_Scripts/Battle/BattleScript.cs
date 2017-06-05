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


    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        anim.runtimeAnimatorController = battleAnimController;

        endTurnStatusListener = new UnityAction(ApplyEndTurnStatusEffects);
        EventManager.StartListening(BattleEventMessages.applyEndTurnStatus.ToString(), endTurnStatusListener);
        launchTaunt = new UnityAction(battleTaunt);
        EventManager.StartListening(BattleEventMessages.beginFight.ToString(), launchTaunt);
    }

    //launch anim battleTaunt
    void battleTaunt()
    {
        anim.SetTrigger("Taunt");
    }

    public IEnumerator ExecuteBattleAnim(Ability ability, List<GameObject> targets)
    {
        
        Vector3 initPos = transform.position;

        if (targets.Count == 1 && ability.targetType.Equals(TargetType.Opposite))
            yield return StartCoroutine(MoveOverSpeed(gameObject, targets[0].transform.position, 20f));
        
        anim.SetTrigger(ability.name);
        //depends on ability duration we should wait for exec
        yield return new WaitForSeconds(1f);

        if (targets.Count == 1 && ability.targetType.Equals(TargetType.Opposite))
            yield return StartCoroutine(MoveOverSeconds(gameObject, initPos, 1f)); 

    }

    public IEnumerator MoveOverSpeed(GameObject objectToMove, Vector3 end, float speed)
    {

        Vector3 diff = end - objectToMove.transform.position;
        float totalDist = diff.magnitude;

        // speed should be 1 unit per second
        while ((end - objectToMove.transform.position).magnitude > (totalDist * 0.1))
        {


            objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, end, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
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
            status.ApplyEndTurn(character);
            AfterDamage();

            if (status.Finished) //
            {
                status.Remove(character);
                //character.status.Remove(status);
                status = null;
            }
        }
    }

    public void ApplyActionImpact(GameObject fromUnit, Ability ab)
    {
        int rawDmg = 0;
        int dmg = 0;

        int multiplyingStat = 0;
        if (ab.abilityType.Equals(AbilityType.Magic))
            multiplyingStat = fromUnit.GetComponent<BattleScript>().Character.GetStat(StatName.intelligence).GetValue();
        else
            multiplyingStat = fromUnit.GetComponent<BattleScript>().Character.GetStat(StatName.strength).GetValue();

        rawDmg = ab.power * multiplyingStat * 6;

        if (ab.targetType.Equals(TargetType.Self) || ab.targetType.Equals(TargetType.Same) || ab.targetType.Equals(TargetType.AllSame))
            dmg = rawDmg;
        else //if opposite dmg = rawDmg with defense reduction
            dmg = Mathf.CeilToInt(rawDmg / (character.GetStat(StatName.defense).GetValue() * 2));

        character.GetStat(StatName.hpNow).baseValue = Mathf.Clamp(character.GetStat(StatName.hpNow).baseValue + dmg, 0, character.GetStat(StatName.hp).GetValue());

        AfterDamage();
    }

    public void TryAddStatus(Status status)
    {
        //Applied if not already present and depending on success rate of the status
        if (!character.status.Exists(s => s.GetType() == status.GetType()) && Rng.GetSuccess(status.SuccessRatePercent))
        {
            status.Add(character);
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

    void AfterDamage()
    {
        //if (!AmIAPlayer())
        //Debug.Log(gameObject + " remaining hp: " + character.GetStat(StatName.hpNow).baseValue);
        

        EventManager.TriggerEvent("damageApplied");
        if (character.GetStat(StatName.hpNow).baseValue == 0)
        {
            dead = true;
            if (!AmIAPlayer())
            {
                BattleManager.Instance.monsterUnits.Remove(gameObject);
                Destroy(gameObject);
            }
        }
    }

    public bool KnowsMagic()
    {
        return character.abilities.FindIndex(a => a.abilityType.Equals(AbilityType.Magic)) >= 0;
    }

}
