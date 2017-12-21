using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu()]
public class StatusSO : ScriptableObject 
{
 
        public string id;
        public string name;
        public string type;
        public int successRatePercent;
        public string description;
        public StatusApplyMoment applyMoment;
        public StatusApplyType applyType;
        public StatName statName;
        public int powerPercent;
        public int maxTurns;
        public List<StatusSO> removesStatusesOnAdd;
        public List<StatusSO> blockedByStatuses;
}


