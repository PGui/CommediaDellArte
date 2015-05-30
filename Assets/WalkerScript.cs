﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WalkerScript : MonoBehaviour {

    public float _moveSpeed;
    [HideInInspector]
    public bool isWalking = false;

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;


    public void Initialisation(){
        _spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        _animator = this.GetComponentInChildren<Animator>();
    }

	// Use this for initialization
	void Start () {
        _spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        _animator = this.GetComponentInChildren<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        
    }

    public void setSprite(Sprite sprite)
    {
        _spriteRenderer.sprite = sprite;
    }


    public void startWalking(Path path)
    {
        this.transform.position = path._start;
        StartCoroutine(walkingCoroutine(path));
        isWalking = true;
    }

    private IEnumerator walkingCoroutine(Path path)
    {
        Vector3 direction = (path._end - path._start).normalized;
        while (this.transform.position != path._end)
        {
            if (Vector3.Distance(this.transform.position, path._end) < (direction * (Time.deltaTime * _moveSpeed)).magnitude)
            {
                this.transform.position = path._end;
            }
            else {
                this.transform.position += direction * (Time.deltaTime * _moveSpeed);
            }
            yield return null;
        }
        isWalking = false;
        WalkerManager.currentWalker--;
        Destroy(this.gameObject);
        yield break;
    }


}