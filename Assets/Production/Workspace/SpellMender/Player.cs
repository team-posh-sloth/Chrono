using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chrono
{
    public class Player : MonoBehaviour
    {
        public Checkpoint checkpoint;
        [SerializeField] float speed = 5, jumpMagnitude = 9, gravity = 9.8f, worldTimeDilation = 0.25f, PlayerTimeDilation = 0.75f;

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

            // Sets a checkpoint if none is assigned.
            if (checkpoint == null) GameObject.FindGameObjectWithTag("Respawn").TryGetComponent(out checkpoint); 
            checkpoint.SetActive(true);
        }

        private void Update()
        {
            ReadAxes();
            SetMovementFlags();
            SetJumpFlags();
            TimeDilation();
            Animate();
        }
        void FixedUpdate()
        {
            isGrounded = IsGrounded();
            ApplyMotion();
        }

        float GetPlayerDeltaTime()
        {
            if (Time.timeScale != 1)
            {
                return (Time.deltaTime / worldTimeDilation) * PlayerTimeDilation;
            }
            else return Time.deltaTime;
        }

        Vector3 Move()
        {
            moveDelta = moveInput * speed * GetPlayerDeltaTime();

            return new Vector3(moveDelta, 0, 0);
        }

        Vector3 Jump()
        {
            jumpDelta = jumpMagnitude * GetPlayerDeltaTime();

            return new Vector3(0, jumpDelta, 0);
        }

        Vector3 Fall()
        {
            gravDelta = gravity * GetPlayerDeltaTime();
            gravVelocity -= gravDelta * GetPlayerDeltaTime();

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

            // Reset gravity if we're on the ground
            if (isGrounded) gravVelocity = 0;

            //// Reset gravity if it's too high (we're stuck on an edge)
            //if (gravVelocity < -1) { gravVelocity = 0; }
            //else
            //{
            motionVector += Fall();
            if (!isMotionDelta) isMotionDelta = true;
            //}

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

        void SetMovementFlags()
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
        }

        void SetJumpFlags()
        {
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
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                isJumping = false;
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

        void TimeDilation()
        {
            // Dilate time if either shift key is down
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                Time.timeScale = worldTimeDilation;
                playerAnim.speed = PlayerTimeDilation / worldTimeDilation;
            }

            // Return time to normal if either shift key is up
            if (Time.timeScale != 1 && (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift)))
            {
                Time.timeScale = playerAnim.speed = 1;
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
            if (collision.CompareTag("HurtsPlayer"))
            {
                transform.position = checkpoint.transform.position;
            }
        }
    }

}
