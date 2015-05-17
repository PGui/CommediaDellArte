﻿using UnityEngine;
using System.Collections;

public class CoffreScript : MonoBehaviour {

	public GameManager gameManager;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void jugggle(){
		StartCoroutine (juggleCoroutine ());
	}
	public void eggJugggle(){
		StartCoroutine (eggJuggleCoroutine ());
	}
	public void talk(int type){
		StartCoroutine (talkCoroutine (type));
	}
	public void touch(int type){
		StartCoroutine (touchCoroutine (type));
	}


	IEnumerator juggleCoroutine(){

		gameManager.guiManager.active = false;

		Vector3 moveToChestEvent = new Vector3 (13, 7, 30);
		gameManager.character.goTo (moveToChestEvent);
		while (gameManager.character.transform.position !=  moveToChestEvent) {
			yield return null;
		}

		//animation prendre;

		Vector3 goToCenterEvent = new Vector3 (-1.5f, 7, 30);
		gameManager.character.goTo (goToCenterEvent);
		while (gameManager.character.transform.position !=  goToCenterEvent) {
			yield return null;
		}

		gameManager.character.GetComponentInChildren<Animator> ().SetTrigger ("juggling");
		yield return new WaitForSeconds(gameManager.character.GetComponentInChildren<Animator> ().GetCurrentAnimatorStateInfo(0).length + 0.2f);

		//retour negatif du souffleur

		gameManager.character.goTo (moveToChestEvent);
		while (gameManager.character.transform.position !=  moveToChestEvent) {
			yield return null;
		}

		//animation reposer

		gameManager.guiManager.active = true;

		yield break;
	}

	IEnumerator eggJuggleCoroutine(){
		
		gameManager.guiManager.active = false;
		
		Vector3 moveEvent = new Vector3 (13, 7, 30);
		gameManager.character.goTo (moveEvent);
		
		while (gameManager.character.transform.position !=  moveEvent) {
			yield return null;
		}

		//animation prendre
		gameManager.character.GetComponentInChildren<Animator> ().SetTrigger ("eggJuggling");
		yield return new WaitForSeconds(gameManager.character.GetComponentInChildren<Animator> ().GetCurrentAnimatorStateInfo(0).length);
		gameManager.character.GetComponentInChildren<Animator> ().SetTrigger ("brokenEggs");
		yield return new WaitForSeconds(gameManager.character.GetComponentInChildren<Animator> ().GetCurrentAnimatorStateInfo(0).length);

		gameManager.character.GetComponentInChildren<Animator> ().SetTrigger ("disappointed");
		yield return new WaitForSeconds (0.2f);
		gameManager.publicOnScene.happy (2, 2);
		yield return new WaitForSeconds(gameManager.character.GetComponentInChildren<Animator> ().GetCurrentAnimatorStateInfo(0).length - 0.2f);

		gameManager.guiManager.active = true;

		StartCoroutine (gameManager.event2 ());
		
		yield break;
	}


	IEnumerator talkCoroutine(int type){

		gameManager.guiManager.active = false;
		
		Vector3 moveEvent = new Vector3 (13, 7, 30);
		gameManager.character.goTo (moveEvent);
		
		while (gameManager.character.transform.position !=  moveEvent) {
			yield return null;
		}

		gameManager.character.transform.Rotate (0, 180, 0);

		if(type == 0)
			gameManager.character.GetComponentInChildren<Animator> ().SetTrigger ("niceTalking");
		else if (type == 1)
			gameManager.character.GetComponentInChildren<Animator> ().SetTrigger ("angryTalking");


		yield return new WaitForSeconds(gameManager.character.GetComponentInChildren<Animator> ().GetCurrentAnimatorStateInfo(0).length/2);
		gameManager.souffleur.saySomething ("Mais ques fais tu , Ce coffre n'est pas un comedien !", false);

		while (gameManager.souffleur.talking == true) {
			yield return null;
		}

		gameManager.character.transform.Rotate (0, 180, 0);


		gameManager.guiManager.active = true;

		yield break;
	}

	IEnumerator touchCoroutine(int type){
		
		gameManager.guiManager.active = false;
		
		Vector3 moveEvent = new Vector3 (13, 7, 30);
		gameManager.character.goTo (moveEvent);
		
		while (gameManager.character.transform.position !=  moveEvent) {
			yield return null;
		}

		gameManager.character.transform.Rotate (0, 180, 0);
		if(type == 0)
			gameManager.character.GetComponentInChildren<Animator> ().SetTrigger ("poke");
		else if (type == 1)
			gameManager.character.GetComponentInChildren<Animator> ().SetTrigger ("frappe");
		
		
		yield return new WaitForSeconds(gameManager.character.GetComponentInChildren<Animator> ().GetCurrentAnimatorStateInfo(0).length/2);
		gameManager.souffleur.saySomething ("Mais ques fais tu , Ce coffre n'est pas un comedien !", false);

		while (gameManager.souffleur.talking == true) {
			yield return null;
		}
		gameManager.character.transform.Rotate (0, 180, 0);

		gameManager.guiManager.active = true;
		
		yield break;
	}



}

