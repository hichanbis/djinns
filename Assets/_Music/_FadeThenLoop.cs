using UnityEngine;
using System.Collections;

public class _FadeThenLoop : MonoBehaviour {
	
	/* This script is for fading seamless audio loops in and out.
	   Drop this script onto an audio source to enter a seamless loop smoothly if desired.
	  
	   Seamless looping requires the reverb tail (echo) at the end of the track to be applied to the start 
	   of the loop, which can make some music start suddenly or abruptly.
	   This script simply fades the start when first entering the audio loop. 
	   
	   A fadeInSpeed of 4f is good for starting a seamless loop smoothly if required.	   
	   If a real fade-in is actually needed then set the fadeInSpeed between 0.25f and 2f.
	   
	   Make sure Play On Awake and Loop are checked for each audio source.
	   
	   Set the fadeOutSpeed in the Inspector, and then just call the fadeOut() function when required.
	   
	   info@ryancalador.com 
	*/
	
	public float fadeInSpeed = 4f; // how fast to fade in the audio.	
	public float startFadeIn = .025f; // how much to remove from the start. Normally leave this at 0.025f	
	public float fadeOutSpeed = .75f;
	public float Volume; // moves volume slider in Inspector
	
	
	// Use this for initialization
	void Start () {
		
		GetComponent<AudioSource>().volume = 0f;
		Invoke("fadeIn",startFadeIn);		
		//Invoke("fadeOut",10f); // for testing fade out speed
	}
	 
	
	void fadeIn() {
		
		if (GetComponent<AudioSource>().volume < 1){
			GetComponent<AudioSource>().volume += fadeInSpeed * Time.deltaTime;	
			Volume = GetComponent<AudioSource>().volume; 
			Invoke("fadeIn",.1f);
		}
	}
	
	
	void fadeOut() {
		
		if (GetComponent<AudioSource>().volume > 0){
			GetComponent<AudioSource>().volume -= fadeOutSpeed * Time.deltaTime;	
			Volume = GetComponent<AudioSource>().volume;
			Invoke("fadeOut",.1f);
		}
		else{
			GetComponent<AudioSource>().Stop();	
		}
	}
	
	
	// Update is called once per frame
	void Update () {
		
		
	}
}
