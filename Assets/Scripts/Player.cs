﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Rewinder
{

    void Awake() {
        // PlayerPrefs.DeleteAll();
    }
    // Attacking variables
    bool isAttacking;
    bool isBeingAttacked;
    bool notSuckedUpByPortal = true;

    // Movement and jump related variables
    public LayerMask groundLayer;
    bool isGrounded;
    public bool isJumping;
    Transform feetPos;
    float checkRadius = 0.25f;
    float movementSpeed = 5f;
    float jumpForce = 10f;
    float moveInput;
    float jumpTimer;
    float jumpDuration = 0.2f;
    int orbCount = 0;
    PortalControl portal;

    float coyoteTimer = 0;
    float coyoteTime = 0.3f;

    // Aiming stuff
    Vector2 direction;
    Vector3 mousePos;

    // Components
    Animator animator;
    Rigidbody2D rb;

    // Gun related
    Transform pivot;
    AudioManager audioManager;
    HUDControl hc;

    // Prefabs
    public GameObject jumpParticlePrefab;
    public GameObject deathParticlePrefab;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        afterimage = Instantiate(afterimagePrefab, transform.position, Quaternion.identity);
        afterimage.GetComponent<Animator>().runtimeAnimatorController = GetComponent<Animator>().runtimeAnimatorController;
        feetPos = transform.Find("Feet");
        pivot = transform.Find("Pivot");
        portal = GameObject.Find("Portal").GetComponent<PortalControl>();
        audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
        audioManager.Find("Orb").source.pitch = 1f;
        hc = GameObject.Find("HUD").GetComponent<HUDControl>();
    }

    void Update()
    {
        if (Time.timeScale == 1) {
            CheckInput();
            RotateGun();
        }
        
    }
    void FixedUpdate()
    {
        if (notSuckedUpByPortal)
        {
            MoveAfterImage();
            Move();
        }
    }

    void CheckInput()
    {
        if (notSuckedUpByPortal)
        {// Mouse position relative to player
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            direction = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
            direction.Normalize();

            // Check for movement and change animations accordingly
            moveInput = Input.GetAxisRaw("Horizontal");

            // Listen for rewind key press
            if (Input.GetKeyDown(KeyCode.E))
            {
                audioManager.Play("Rewind");
                Rewind();
            }

            // Listen for gun shot
            if (Input.GetMouseButtonDown(0))
            {
                audioManager.Play("FireLaser");
                StartCoroutine(transform.Find("Pivot").Find("Gun").GetComponent<FireBullets>().FireBullet(mousePos));
            }
        }
    }

    void Move()
    {

        // Set jump variables
        isGrounded = Physics2D.OverlapCircle(feetPos.position, checkRadius, groundLayer);
        rb.gravityScale = (isGrounded) ? 5f : 8f;

        if (isGrounded) 
            animator.SetFloat("yVelocity", 0);
        else {
            if (rb.velocity.y < 0 && coyoteTimer == 0) 
                coyoteTimer = coyoteTime;
            animator.SetFloat("yVelocity", rb.velocity.y);
        }
        animator.SetBool("moving", ((rb.velocity.x >= -0.1f && rb.velocity.x <= 0.1f) || !isGrounded) ? false : true);


        // Start the jump
        if (Math.Abs(rb.velocity.y) <= 0.1f && Input.GetKey(KeyCode.W) && (isGrounded || coyoteTimer > 0) && !isJumping)
        {
            audioManager.Play("Jump");
            rb.velocity = Vector2.up * jumpForce;
            isJumping = true;
            jumpTimer = jumpDuration;
            Instantiate(jumpParticlePrefab, transform.position, Quaternion.identity);
            coyoteTimer = 0;
        }

        // Jump higher while key is pressed
        if (isJumping)
        {
            if (jumpTimer > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimer -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
                rb.gravityScale = 8f;
            }
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            isJumping = false;
            rb.gravityScale = 8f;
        }

        rb.velocity = new Vector2(moveInput * movementSpeed, rb.velocity.y);
        // Rotate entity while moving
        if (rb.velocity.x != 0) transform.localRotation = (rb.velocity.x < 0) ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
        
        coyoteTimer -= Time.deltaTime;
    }

    void RotateGun()
    {
        pivot.localScale = (mousePos.x > transform.position.x) ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
        pivot.up = direction;
        
        

    }
    public bool getIsGrounded()
    {
        return isGrounded;
    }
    public void bounce()
    {
        rb.velocity = Vector2.up * jumpForce;
        isJumping = true;
        jumpTimer = jumpDuration;
    }
    public void Die()
    {
        audioManager.Play("PlayerDeath");
        Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        hc.PromptRestart();
        Destroy(afterimage);
        Destroy(gameObject);
    }
    public void EnterPortalWin(Vector2 portalPos)
    {
        audioManager.Play("EnterPortal");
        Destroy(afterimage);
        notSuckedUpByPortal = false;
        GetComponent<Collider2D>().enabled = false;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        StartCoroutine(EnterPortalSpin(portalPos));
        hc.StopTimer();
    }
    IEnumerator EnterPortalSpin(Vector2 portalPos)
    {
        float spinTimeElapsed = 0f;
        float rotationAcceleration = 5f;
        float duration = 2f;
        while (spinTimeElapsed < duration)
        {
            transform.position = Vector3.MoveTowards(transform.position, portalPos, 4f * Time.deltaTime);
            rb.rotation += rotationAcceleration;
            transform.localScale -= new Vector3(Time.deltaTime / duration, Time.deltaTime / duration, 0);
            spinTimeElapsed += Time.deltaTime;
            yield return null;
        }
        hc.LevelCleared();
    }
    public void gainOrb()
    {
        audioManager.Play("Orb");
        audioManager.Find("Orb").source.pitch+=0.1f;
        orbCount++;
        if (orbCount >= portal.orbsRequired) portal.ActivatePortal();
    }
    public int getOrbCount() { return orbCount; }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            audioManager.Play("PlayerLand");
            Instantiate(jumpParticlePrefab, transform.position, Quaternion.identity);
        }
    }
}
