using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleAnimationEvents : MonoBehaviour {

	public void RegisterAttackLaunched(){
        BattleManager.Instance.targetImpactReached = true;
    }
}
