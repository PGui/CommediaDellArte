﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CursorScript : MonoBehaviour {


	public Vector2 hotSpot = Vector2.zero;
	private float planeDistance;

	void Start(){

		Cursor.visible = false;
		planeDistance = this.transform.parent.GetComponent<Canvas> ().planeDistance;
	}

	void FixedUpdate(){
		
		Vector3 newPos = Camera.main.ScreenToWorldPoint(new Vector3 (Input.mousePosition.x,Input.mousePosition.y,planeDistance));
		this.transform.position = new Vector3(newPos.x + hotSpot.x, newPos.y + hotSpot.y,this.transform.position.z) ;

	}

	public void setInteractionAnim(bool anim){
        this.GetComponent<Animator>().SetBool("wait", false);
		this.GetComponent<Animator> ().SetBool ("interaction", anim);
	}

    public void wait()
    {
        this.GetComponent<Animator>().SetBool("wait", true);
    }

}