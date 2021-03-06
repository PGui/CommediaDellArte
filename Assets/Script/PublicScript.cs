﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent (typeof (AudioSource))]


public class PublicScript : MonoBehaviour {
	
	public List<AudioClip> laughtClips = new List<AudioClip>();//liste des son de rires
	public List<AudioClip> clapClips = new List<AudioClip>();//liste des sons d'applaudissements
	public List<publicEvent> EventList;
	public float value = 0;

	private float time;
	private int index = 0,publicSize = 0;
    private IEnumerator reactionCo;
	private AudioSource audioSource;
	private List<Animator> AnimatorList, presentSpectator, goneSpectator;

    private static PublicScript _instance;

    public static PublicScript instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PublicScript>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != _instance)
                Destroy(this.gameObject);
        }
    }

	void Start () {

		AnimatorList = new List<Animator> ();
		presentSpectator = new List<Animator> ();
		goneSpectator = new List<Animator> ();
		audioSource = this.GetComponent<AudioSource>();
		index = 0;

		for (int i = 0; i < this.transform.childCount; i++) {
			AnimatorList.Insert(AnimatorList.Count , this.transform.GetChild (i).transform.GetChild (0).GetComponent<Animator>());	
		}
		for (int i = 0; i < AnimatorList.Count; i++) {
			if( AnimatorList[i].GetBool("walkAway") == false){
				presentSpectator.Add(AnimatorList[i]);
			}
			else{
				goneSpectator.Add(AnimatorList[i]);
			}
		}
		publicSize = presentSpectator.Count;

		addValue (0);
		subValue (0);
		
	}

	void Update () {

		/*if (Input.GetButtonDown ("Fire1")) {
			for (int i = 0; i < this.transform.childCount; i++) {
				//walkAway(i);
			}
			//happy (1);
			subValue (25);
		}
		*/
	}



    public void reset()
    {
        audioSource.Stop();
        if (reactionCo != null)
        {
            Debug.Log("Reset");
            for (int i = 0; i < AnimatorList.Count; i++)
            {
                AnimatorList[i].SetBool("happy", false);
            }
            StopCoroutine(reactionCo);
        }
    }

	public void reaction(string soundName, float duration, float volume ){
	
		this.GetComponent<SoundController>().playRandomSoundPart(soundName,duration,volume);

        if(reactionCo != null){
            Debug.Log("Reset");
            for (int i = 0; i < AnimatorList.Count; i++) {
					AnimatorList[i].SetBool("happy", false);
				}
            StopCoroutine(reactionCo);
        }

		switch (soundName) {
		    case("Rire +1"):
			    reactionCo = reactionCoroutine("Rire +1", duration);
                StartCoroutine(reactionCo);
			    break;
		    case("Rire +5"):
			    reactionCo = reactionCoroutine("Rire +5", duration);
                StartCoroutine(reactionCo);
			    break;
		    case("Rire +10"):
			     reactionCo = reactionCoroutine("Rire +10", duration);
                 StartCoroutine(reactionCo);
			    break;
		    case("Boo -1"):
			     reactionCo = reactionCoroutine("Boo -1", duration);
                 StartCoroutine(reactionCo);
			    break;
		    case("Boo -5"):
			     reactionCo = reactionCoroutine("Boo -5", duration);
                 StartCoroutine(reactionCo);
			    break;
		}
	}

	IEnumerator reactionCoroutine(string name, float time ){
		

		for (int i = 0; i < AnimatorList.Count; i++) {
			AnimatorList[i].SetBool("happy", true);

            AnimatorList[i].SetTrigger(name);

            if (AnimatorList[i].GetBool("walkAway") == false)
                AnimatorList[i].CrossFade(name, 0.5f, 0, ((float)Random.Range(0, 100)) / 100);          
       
		}
		
		while (true) {
			if (time > 0) {
				time -= Time.deltaTime;
			}
			else{
				for (int i = 0; i < AnimatorList.Count; i++) {
					AnimatorList[i].SetBool("happy", false);
				}
				yield break;
			}
			yield return null;
		}
	}


	public void addValue(float val){
		value = Mathf.Min( value+val,100);
		while(value >= EventList[index].value ){
			while(publicSize <  EventList[index].publicSize && goneSpectator.Count > 0){
				int rand = Random.Range(0, goneSpectator.Count-1);
				presentSpectator.Insert(0,goneSpectator[rand]);
				goneSpectator[rand].SetBool("walkAway", false);
				//goneSpectator[rand].GetComponent<SpectatorEvent>().randomAnimationStart("idle");
				goneSpectator.RemoveAt(rand);
				publicSize++;
			}
			if( (index < EventList.Count-1)){
				index++;
			}
			else{
				return;
			}
		}
	}
	public void subValue(float val){
		value = Mathf.Max( value-val,0);
		while(value <= EventList[index].value ){
			while(publicSize >  EventList[index].publicSize && presentSpectator.Count > 0){
				int rand = Random.Range(0, presentSpectator.Count-1);
				goneSpectator.Insert(0,presentSpectator[rand]);
				presentSpectator[rand].SetBool("walkAway", true);
				presentSpectator.RemoveAt(rand);
				publicSize--;
			}
			if(index > 0)
				index--;
			else
				break;
		}

	}
	public void setValue(float val){
		if (val == value)
			return;
		else if (val > value) {
			addValue (val - value);
		} 
		else {
			subValue(value - val);
		}
	}


	public void happy(float time, float speed){
		StartCoroutine(happyTimer(time, speed));
	}
	
	public void happy(float time){
		StartCoroutine(happyTimer(time));
	}

	public void walkAway(int index){
		this.transform.GetChild (index).transform.GetChild (0).GetComponent<Animator> ().SetTrigger ("walkAway");
	}
	public void walkBack(int index){
		this.transform.GetChild (index).transform.GetChild (0).GetComponent<Animator> ().SetTrigger ("walkAway");
	}

	//coroutine qui gere la durée de l'animation du public 
	IEnumerator happyTimer(float time, float speed = 1 ){

		audioSource.clip = getRandomLaughtSound();
		audioSource.Play ();

		for (int i = 0; i < AnimatorList.Count; i++) {
			AnimatorList[i].SetBool("happy", true);
			AnimatorList[i].speed = speed;
		}

		while (true) {
			if (time > 0) {
				time -= Time.deltaTime;
			}
			else{
				for (int i = 0; i < AnimatorList.Count; i++) {
					AnimatorList[i].SetBool("happy", false);
					audioSource.Stop ();
					AnimatorList[i].speed = 1;
				}
				yield break;
			}
			yield return null;
		}
	}

	private AudioClip getRandomClapSound(){
		return clapClips [Random.Range (0, clapClips.Count)];
	}
	private AudioClip getRandomLaughtSound(){
		return laughtClips [Random.Range (0, laughtClips.Count)];
	}

	[System.Serializable]
	public class publicEvent{
		public int value, publicSize;
	}


}


