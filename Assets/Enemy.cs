﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Enemy : Rewinder
{
    // LayerMasks
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public bool spiked = false;
    public bool walksOffLedges = false;

    // Movement variables
    private Transform feetPos;
    private Transform lookAheadGroundPos;
    private Transform lookAheadWallPos;
    private float speed = 2f;
    public int direction = 1;

    // Components
    private Rigidbody2D rb;
    private AudioManager audioManager;

    // Prefab
    public GameObject deathParticlePrefab;
    void Start()
    {
        feetPos = transform.Find("Feet");
        lookAheadGroundPos = transform.Find("LookAheadGround");
        lookAheadWallPos = transform.Find("LookAheadWall");
        audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
        rb = GetComponent<Rigidbody2D>();
        afterimage = Instantiate(afterimagePrefab, transform.position, Quaternion.identity);
        afterimage.GetComponent<Animator>().runtimeAnimatorController = GetComponent<Animator>().runtimeAnimatorController;
        Color newColor = afterimage.GetComponent<SpriteRenderer>().color;
        newColor.a = 0.75f;
        afterimage.GetComponent<SpriteRenderer>().color = newColor;
        GetComponent<Animator>().SetBool("moving", true);
        transform.localRotation = (direction < 0) ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
        afterimage.transform.localRotation = transform.localRotation = (direction < 0) ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        Physics2D.IgnoreLayerCollision(9, 9);
        if (Math.Abs(rb.velocity.y) <= 0.01f)
        {
            lookAhead();
            rb.velocity = new Vector2(speed * direction, rb.velocity.y);
        }
    }
    void FixedUpdate()
    {
        MoveAfterImage();
    }

    void lookAhead()
    {
        if (!Physics2D.OverlapCircle(lookAheadGroundPos.position, 0.2f, groundLayer) && walksOffLedges == false)
        {
            turnAround();
        }
        if (Physics2D.OverlapCircle(lookAheadWallPos.position, 0.1f, groundLayer))
        {
            turnAround();
        }
    }

    public void turnAround()
    {
        direction *= -1;
        transform.localRotation = (direction < 0) ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (!spiked && other.transform.position.y > transform.position.y + GetComponent<Collider2D>().bounds.size.y) {
                Physics2D.IgnoreCollision(other.gameObject.GetComponent<Collider2D>(), GetComponent<BoxCollider2D>());
                other.gameObject.GetComponent<Player>().bounce();
                die();
            } else 
                other.gameObject.GetComponent<Player>().Die();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // if (other.gameObject.tag == "PlayerHurtBox" && spiked == false)
        // {
        //     Physics2D.IgnoreCollision(other, GetComponent<BoxCollider2D>());
        //     other.gameObject.transform.parent.GetComponent<Player>().bounce();
        //     die();
        // }
    }
    public bool isSpiked()
    {
        return spiked;
    }
    public void die()
    {
        Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        audioManager.Play("Death");
        Destroy(afterimage);
        Destroy(gameObject);
    }
    public override void Rewind()
    {
        base.Rewind();
        if (gameObject.CompareTag("Enemy"))
        {
            if (afterimage.transform.eulerAngles.y != transform.eulerAngles.y)
            {
                direction *= -1;
                transform.localRotation = (transform.localRotation == Quaternion.Euler(0, 0, 0)) ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
            }
        }
    }
}
