using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterAnim : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Destroy(gameObject, GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
