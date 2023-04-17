using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    float speed, jumpMagnitude, gravity = 9.8f;

    [SerializeField]
    AudioClip jumpSound;

    float moveDelta, moveInput, jumpDelta, gravDelta, gravVelocity;
    bool isMoving, isJumping, jumpInput, isGrounded, isFalling, flipX;

    Vector3 motionVector;

    Rigidbody2D playerBody;
    SpriteRenderer playerSprite;
    Animator playerAnim;
    AudioSource playerAudio;

    void Start()
    {
        playerBody = GetComponent<Rigidbody2D>();
        playerSprite = GetComponent<SpriteRenderer>();
        playerAnim = GetComponentInChildren<Animator>();
        playerAudio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        ReadAxes();
        SetFlags();
        Animate();
    }
    void FixedUpdate()
    {
        isGrounded = IsGrounded();
        ApplyMotion();
    }

    Vector3 Move()
    {
        moveDelta = moveInput * speed * Time.deltaTime;

        return new Vector3(moveDelta, 0, 0);
    }

    Vector3 Jump()
    {
        jumpDelta = jumpMagnitude * Time.deltaTime;

        return new Vector3(0, jumpDelta, 0);
    }

    Vector3 Fall()
    {
        gravDelta = gravity * Time.deltaTime;
        gravVelocity -= gravDelta * Time.deltaTime;

        return new Vector3(0, gravVelocity, 0);
    }

    void ApplyMotion()
    {
        motionVector = transform.position;

        // Add horizontal movement
        if (isMoving) motionVector += Move();

        // Add gravity
        if (isGrounded) gravVelocity = 0;
        else motionVector += Fall();

        // Add jump
        if (isJumping) motionVector += Jump();

        // If there is move input or we're not grounded apply movement
        if (isMoving || isJumping || !isGrounded) playerBody.MovePosition(motionVector);

    }

    bool IsGrounded()
    {
        float detectionRange = 0.05f;
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        RaycastHit2D groundDetection = Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.down, detectionRange, LayerMask.GetMask("Ground"));

        Color rayColor;
        if (groundDetection.collider != null) rayColor = Color.green; else rayColor = Color.red;
        Debug.DrawRay(collider.bounds.center + new Vector3(collider.bounds.extents.x, collider.bounds.extents.y), Vector2.down * (collider.bounds.size.y + detectionRange), rayColor);
        Debug.DrawRay(collider.bounds.center + new Vector3(-collider.bounds.extents.x, collider.bounds.extents.y), Vector2.down * (collider.bounds.size.y + detectionRange), rayColor);
        Debug.DrawRay(collider.bounds.center + new Vector3(-collider.bounds.extents.x, collider.bounds.extents.y), Vector2.right * (collider.bounds.size.x), rayColor);
        Debug.DrawRay(collider.bounds.center + new Vector3(-collider.bounds.extents.x, -collider.bounds.extents.y), Vector2.right * (collider.bounds.size.x), rayColor);

        return groundDetection.collider != null;
    }

    void ReadAxes()
    {
        // Resolves input issue between scenes by reading horizontal input every frame on each specific key
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }
        else if ((moveInput != 0) && (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || isGrounded))
        {
            moveInput = 0;
        }
    }

    void SetFlags()
    {
        if (moveInput != 0)
        {
            // Set Moving Flag
            if (!isMoving) isMoving = true;

            // Set FlipX Flag
            if (moveInput < 0 && !flipX) { flipX = true; }
            else if (moveInput > 0 && flipX) { flipX = false; }
        }
        else if (isMoving) isMoving = false;

        //if (isJumping && isFalling && isGrounded) isJumping = false; isFalling = false;

        //// If motionVector.y is negative set the falling flag
        //if (motionVector.y < 0) isFalling = true;

        // Set jump flags when we push space on the ground
        if (Input.GetKey(KeyCode.Space) && isGrounded && !jumpInput && !isJumping)
        {
            jumpInput = true;
            isJumping = true;
            PlaySound(jumpSound);
        }

        // Unset jumpInput flag when we're off the ground
        if (!isGrounded && jumpInput && jumpInput)
        {
            jumpInput = false;
        }

        // Unset isJumping when we're back on the ground
        if (isGrounded && !jumpInput && isJumping)
        {
            isJumping = false;
        }
    }

    void Animate()
    {
        // Face the sprite using flipX flag
        if (playerSprite.flipX != flipX) playerSprite.flipX = flipX;

        // Set animation state
        if (!isGrounded) playerAnim.Play("player_jump");
        else if (isMoving) playerAnim.Play("player_run");
        else playerAnim.Play("player_idle");
    }

    void PlaySound(AudioClip sound)
    {
        playerAudio.clip = sound;
        playerAudio.Play();
    }

}
