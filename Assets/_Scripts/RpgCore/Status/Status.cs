using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public enum StatusApplyMoment
{
    endTurn,
    add
}

[System.Serializable]
public enum StatusType
{
    ailment,
    support
}

[System.Serializable]
public enum StatusApplyType
{
    damage,
    disableCommands,
    disableMagic,
    modifier,
    addStatus
}

[CreateAssetMenu()]
public class Status : ScriptableObject
{
    public string id;
    public StatusType statusType;
    public int successRatePercent;
    public string description;
    public StatusApplyType applyType;
    public StatName statName;
    public int powerPercent;
    public int maxTurns;
    public int currentTurns;
    public List<Status> removesStatusesOnAdd;
    public List<Status> blockedByStatuses;

    public override string ToString()
    {
        return string.Format("[Status:{0} rate:{1}% type:{2} stat:{3} pow:{4}% maxTurns:{5}]", name, successRatePercent, applyType, statName, powerPercent, maxTurns);
    }

    //Necessary as the status are cloned for the turns counter
    public override bool Equals(System.Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            Status status = (Status)obj;
            return status.id.Equals(this.id);
        }
    }

    public override int GetHashCode()
    {
        return this.id.GetHashCode();
    }

    public bool CanAddStatus(Character character)
    {
        bool blocked = false;
        foreach (Status blockerStatus in blockedByStatuses)
        {
            foreach (Status possessedStatus in character.statuses)
            {
                if (blockerStatus.Equals(possessedStatus))
                    blocked = true;
            }
        }
        Debug.Log("Contains key " + character.statuses.Contains(this));
        return Rng.GetSuccess(successRatePercent) && !blocked;

    }


    public bool Add(Character character)
    {
        if (!CanAddStatus(character))
        {
            return false;
        }

        foreach (Status statusToRemove in removesStatusesOnAdd)
        {
            if (character.statuses.Contains(statusToRemove))
                    statusToRemove.Remove(character);
        }

        //we add only if not already present in character
        if (!character.statuses.Contains(this))
        {
            character.statuses.Add(this);
        }
        currentTurns = 0;

        ApplyStatusEffects(character);

        return true;
    }

    public void NewTurn(Character character)
    {
        currentTurns++;
        
        if (currentTurns < maxTurns)
            ApplyStatusEffects(character);
        else
        {
            Remove(character);
        }
    }

    private void ApplyStatusEffects(Character character)
    {
        if (applyType.Equals(StatusApplyType.damage))
        {
            //base value car on veut appliquer le poison sur la stat hp reelle pas la boostée
            int dmg = Mathf.RoundToInt(character.stats.hp.GetBaseValue() * ((float)powerPercent / 100));
            character.stats.hpNow.SetValue(Mathf.Clamp(character.stats.hpNow.GetValue() + dmg, 0, character.stats.hp.GetValue()));
        }
        else if (applyType.Equals(StatusApplyType.modifier))
            character.GetStat(statName).modifiers.Add(powerPercent);
        else if (applyType.Equals(StatusApplyType.disableCommands))
            character.canAct = false;
        else if (applyType.Equals(StatusApplyType.disableMagic))
            character.canCast = false;
    }

    public void Remove(Character character)
    {
        RemoveStatusEffects(character);
        character.statuses.Remove(this);
    }

    private void RemoveStatusEffects(Character character)
    {
        if (applyType.Equals(StatusApplyType.modifier))
            character.GetStat(statName).modifiers.Remove(powerPercent);
        else if (applyType.Equals(StatusApplyType.disableCommands))
            character.canAct = true;
        else if (applyType.Equals(StatusApplyType.disableMagic))
            character.canCast = true;
    }

}


