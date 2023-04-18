using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpellMender_01
{
    public class Player : MonoBehaviour
    {
        [SerializeField] Transform checkpoint;
        [SerializeField] float speed, jumpMagnitude, gravity = 9.8f;

        [SerializeField] AudioClip jumpSound;
        [SerializeField] int jumpSoundResetTime = 8;
        int jumpSoundResetTimer;

        float moveDelta, moveInput, jumpDelta, gravDelta, gravVelocity;
        bool isMoving, isJumping, jumpInput, isGrounded, flipX;

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
            bool isMotionDelta = false;

            // Motion is relative to current position
            motionVector = transform.position;

            // Add horizontal movement
            if (isMoving)
            {
                motionVector += Move();
                if (!isMotionDelta) isMotionDelta = true;
            }

            // Add gravity if we're not on the ground
            if (isGrounded) gravVelocity = 0;

            // Reset gravity if it's too high (we're stuck on an edge)
            if (gravVelocity < -1) { gravVelocity = 0; }
            else
            {
                motionVector += Fall();
                if (!isMotionDelta) isMotionDelta = true;
            }

            // Add jump from input to landing
            if ((isGrounded && jumpInput) || (isJumping && !isGrounded))
            {
                motionVector += Jump();
                if (!isMotionDelta) isMotionDelta = true;
            }

            // If there is a change apply movement
            if (isMotionDelta) playerBody.MovePosition(motionVector);

        }

        bool IsGrounded()
        {
            Vector3 boundsLimiter = new Vector3(0.15f, 0.4f, 0);
            float detectionRange = 0.05f + boundsLimiter.y / 2;
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            RaycastHit2D groundDetection = Physics2D.BoxCast(collider.bounds.center, collider.bounds.size - boundsLimiter, 0f, Vector2.down, detectionRange, LayerMask.GetMask("Ground"));

            Color rayColor;
            if (groundDetection.collider != null) rayColor = Color.green; else rayColor = Color.red;
            Debug.DrawRay(collider.bounds.center + new Vector3(-collider.bounds.extents.x + boundsLimiter.x / 2, 0), new Vector3(collider.bounds.size.x - boundsLimiter.x, 0), rayColor);
            Debug.DrawRay(collider.bounds.center + new Vector3(0, collider.bounds.extents.y - boundsLimiter.y / 2), new Vector3(0, -collider.bounds.size.y + boundsLimiter.y - detectionRange, 0f), rayColor);

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


            // Countdown jump sound reset timer
            if (jumpSoundResetTimer > 0) --jumpSoundResetTimer;

            // Set jumpInput flag when we push space on the ground
            if (Input.GetKey(KeyCode.Space) && isGrounded && !jumpInput && !isJumping)
            {
                jumpInput = true;
                if (jumpSoundResetTimer == 0)
                {
                    PlaySound(jumpSound);
                    jumpSoundResetTimer = jumpSoundResetTime;
                }
            }

            // Swap jumping flags when we're off the ground
            if (!isGrounded && jumpInput && !isJumping)
            {
                jumpInput = false;
                isJumping = true;
            }

            // Unset isJumping when we're back on the ground
            if (isGrounded && isJumping)
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


        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log("Trigger is entered");

            if (collision.tag == "HurtsPlayer")
            {
                if (checkpoint == null) checkpoint = GameObject.FindGameObjectWithTag("Respawn").transform;

                Debug.Log($"Attempting to teleport player to {checkpoint.position}");
                transform.position = checkpoint.position;
            }
        }
    }

}
