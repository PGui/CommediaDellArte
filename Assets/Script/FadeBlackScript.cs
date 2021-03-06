﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof(Image))]
public class FadeBlackScript : MonoBehaviour {

	public float fadeSpeed;
    public AudioClip _sound;
	private Image _image;
    private AudioSource _audioSource;

	// Use this for initialization
	void Start () {
		_image = this.GetComponent<Image> ();
		_image.color = new Color (0, 0, 0, 0);
        _audioSource = this.GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void fadeToBlack(float duration){
		StartCoroutine (fadeCoroutine (duration));
	}

	IEnumerator fadeCoroutine(float time){

        _audioSource.PlayOneShot(_sound);

		while (	_image.color.a != 1) {

			if(	_image.color.a>=0.9)
					_image.color = new Color (0, 0, 0, 1);
			else
					_image.color = new Color (0, 0, 0, Mathf.Lerp (	_image.color.a, 1, fadeSpeed * Time.deltaTime));
			yield return null;
		}
		yield return new WaitForSeconds(time);

		while (	_image.color.a != 0) {

			if(	_image.color.a<=0.1)
					_image.color = new Color (0, 0, 0, 0);
			else
					_image.color = new Color(0,0,0,Mathf.Lerp (	_image.color.a, 0, fadeSpeed * Time.deltaTime));
			yield return null;
		}
		
		yield break;
	}
}
