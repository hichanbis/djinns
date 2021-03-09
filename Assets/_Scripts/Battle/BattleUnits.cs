using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BattleUnits : ScriptableObject
{

    public List<BattleScript> playerUnits;
    public List<BattleScript> enemyUnits;
    public List<BattleScript> targetUnits;
    public BattleScript currentChoosingUnit;
    public BattleScript currentActingUnit;

    public void Reset()
    {
        playerUnits = new List<BattleScript>();
        enemyUnits = new List<BattleScript>();
        targetUnits = new List<BattleScript>();
        currentChoosingUnit = null; 
    }

    public void SetPlayerUnits(List<BattleScript> playerUnits)
    {
        this.playerUnits = playerUnits;
    }

    public void SetEnemyUnits(List<BattleScript> enemyUnits)
    {
        this.enemyUnits = enemyUnits;
    }

    public void SetTargetUnits(List<BattleScript> targetUnits)
    {
        this.targetUnits = targetUnits;
    }

    public void SetTargetUnit(BattleScript targetUnit)
    {
        targetUnits.Clear();
        targetUnits.Add(targetUnit);
    }

    public void SetCurrentChoosingUnit(BattleScript currentChoosingUnit)
    {
        this.currentChoosingUnit = currentChoosingUnit;
    }



}
