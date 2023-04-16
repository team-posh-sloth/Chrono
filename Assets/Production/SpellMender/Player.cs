using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    float speed, jumpMagnitude;

    [SerializeField]
    AudioClip jumpSound;

    float moveDelta, jumpDelta, moveInput;
    bool isMoving, isJumping, flipX;

    Vector3 movement;
    Vector3 jumpVector;

    Rigidbody2D playerBody;
    SpriteRenderer playerSprite;
    Animator playerAnim;
    AudioSource playerAudio;


    // Start is called before the first frame update
    void Start()
    {
        playerBody = GetComponent<Rigidbody2D>();
        playerSprite = GetComponent<SpriteRenderer>();
        playerAnim = GetComponentInChildren<Animator>();
        playerAudio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        ReadInput();
        SetFlags();
        Animate();
    }
    void FixedUpdate()
    {
        Move();
        Jump();
    }

    void Move()
    {
        if (isMoving)
        {
            moveDelta = moveInput * speed * Time.deltaTime;
            movement = transform.position + new Vector3(moveDelta, 0, 0);
            playerBody.MovePosition(movement);
        }
    }

    void Jump()
    {
        if (isJumping)
        {
            jumpDelta = jumpMagnitude * Time.deltaTime;
            jumpVector = transform.position + new Vector3(0, jumpDelta, 0);
            playerBody.MovePosition(jumpVector);

            PlaySound(jumpSound);
        }
    }

    bool IsGrounded()
    {
        float detectionRange = 0.05f;
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        RaycastHit2D groundDetection = Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.down, detectionRange, LayerMask.GetMask("Ground"));
        return groundDetection.collider != null;
    }

    void ReadInput()
    {
        // Resolves input issue between scenes by reading horizontal input every frame on each specific key
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }
        else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || IsGrounded())
        {
            // consider optimizing the IsGrounded method check so it doesn't happen every frame
            moveInput = 0;
        }

        if (Input.GetKey(KeyCode.Space) && IsGrounded() && isJumping == false)
        {
            isJumping = true;
        }
        else isJumping = false;
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

        // Set Jumping Flag
        if (!IsGrounded() && !isJumping) isJumping = true;
        else if (isJumping && IsGrounded()) isJumping = false;
    }

    void Animate()
    {
        // Face the sprite using flipX flag
        if (playerSprite.flipX != flipX) playerSprite.flipX = flipX;

        // Set animation state
        if (isJumping) playerAnim.Play("player_jump");
        else if (isMoving) playerAnim.Play("player_run");
        else playerAnim.Play("player_idle");
    }

    void PlaySound(AudioClip sound)
    {
        playerAudio.clip = sound;
        playerAudio.Play();
    }

}
