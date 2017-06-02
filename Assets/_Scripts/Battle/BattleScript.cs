using System;
using UnityEngine;
using UnityEngine.Events;

public class BattleScript : MonoBehaviour
{
    public RuntimeAnimatorController battleAnimController;

    private Character character;
    private bool dead = false;
    private Animator anim;
    private UnityAction endTurnStatusListener;
    private UnityAction unitsLoadedListener;


    void Start()
    {
        //ça pète pour les ennemis, normal
        anim = GetComponentInChildren<Animator>();
        anim.runtimeAnimatorController = battleAnimController;

        endTurnStatusListener = new UnityAction(ApplyEndTurnStatusEffects);
        EventManager.StartListening(BattleEventMessages.applyEndTurnStatus.ToString(), endTurnStatusListener);
        unitsLoadedListener = new UnityAction(InitAnim);
        EventManager.StartListening(BattleEventMessages.taunt.ToString(), unitsLoadedListener);
    }

    //launch anim battleTaunt
    void InitAnim()
    {
        Debug.Log("Setting the trigger yeah");
        anim.SetTrigger("Taunt");
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
