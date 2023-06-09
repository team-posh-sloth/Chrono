﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chrono
{
    public class Player : MonoBehaviour
    {
        public Checkpoint checkpoint;
        [SerializeField] float speed = 5, jumpMagnitude = 9, gravity = 9.8f, worldTimeDilation = 0.25f, PlayerTimeDilation = 0.75f;

        [SerializeField] AudioClip jumpSound, splashStepSound, slowSplashSound, hardStepSound;
        [SerializeField] int jumpSoundResetTime = 8;
        int jumpSoundResetTimer;

        float moveDelta, moveInput, jumpDelta, gravDelta, gravVelocity, platformDeltaX, platformDeltaY;
        bool isMoving, isJumping, jumpInput, isGrounded, flipX, isOnPlatform;

        Vector3 motionVector, platformVector;

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
            #if !UNITY_WEBGL
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            #endif

            if (Input.GetKeyDown(KeyCode.R))
            {
                transform.position = checkpoint.transform.position;
            }

            // Returns player to checkpoint if fallen below ground level
            if (transform.position.y < -5.5) transform.position = checkpoint.transform.position;

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

        Vector3 Platform()
        {
            platformDeltaX = platformVector.x * Time.deltaTime;
            platformDeltaY = platformVector.y * Time.deltaTime;

            //if (platformDeltaY < 0) { platformDeltaY -= gravVelocity; }

            if (isOnPlatform) return new Vector3(platformDeltaX, platformDeltaY, 0);
            return Vector3.zero;

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
            if (isMoving && isGrounded && !isJumping && !jumpInput)
            {
                playerAudio.volume = 1;
                if (isOnPlatform) PlaySound(hardStepSound, true);
                else if (Time.timeScale == 1) PlaySound(splashStepSound, true);
                else
                {
                    PlaySound(splashStepSound, true, PlayerTimeDilation);
                }
            }

            // Reset gravity if we're on the ground
            if (isGrounded)
            {
                gravVelocity = 0;
                /*if (!isOnPlatform) */PlaySound(false);
            }
            else
            {
                // Fall if we're not
                motionVector += Fall();
                if (!isMotionDelta) isMotionDelta = true;

                if (playerAudio.clip == hardStepSound) PlaySound(false);
            }

            // Add jump from input to landing
            if ((isGrounded && jumpInput) || (isJumping && !isGrounded))
            {
                motionVector += Jump();
                if (!isMotionDelta) isMotionDelta = true;
            }

            motionVector += Platform();
            if (!isMotionDelta) isMotionDelta = true;

            // If there is a change apply movement
            if (isMotionDelta) playerBody.MovePosition(motionVector);

        }

        bool IsGrounded()
        {
            Vector3 boundsLimiter = new Vector3(0.15f, 0.4f, 0);
            float detectionRange = 0.05f + boundsLimiter.y / 2;
            BoxCollider2D playerCollider = GetComponent<BoxCollider2D>();
            RaycastHit2D groundDetection = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size - boundsLimiter, 0f, Vector2.down, detectionRange, LayerMask.GetMask("Ground"));

            RaycastHit2D platformDetection = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size - boundsLimiter, 0f, Vector2.down, detectionRange, LayerMask.GetMask("Platform"));
            if (platformDetection.collider != null)
            {
                isOnPlatform = true;
                platformVector = platformDetection.collider.attachedRigidbody.velocity;
            }
            else isOnPlatform = false;

            Color rayColor;
            if (groundDetection.collider != null) rayColor = Color.green; else rayColor = Color.red;
            Debug.DrawRay(playerCollider.bounds.center + new Vector3(-playerCollider.bounds.extents.x + boundsLimiter.x / 2, 0), new Vector3(playerCollider.bounds.size.x - boundsLimiter.x, 0), rayColor);
            Debug.DrawRay(playerCollider.bounds.center + new Vector3(0, playerCollider.bounds.extents.y - boundsLimiter.y / 2), new Vector3(0, -playerCollider.bounds.size.y + boundsLimiter.y - detectionRange, 0f), rayColor);

            return groundDetection.collider != null || platformDetection.collider != null;
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
                    playerAudio.volume = 0.2f;
                    if (Time.timeScale == 1) PlaySound(jumpSound, false);
                    else PlaySound(jumpSound, false, PlayerTimeDilation);
                    jumpSoundResetTimer = jumpSoundResetTime;
                }
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                isJumping = false;
                gravVelocity = 0;
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

        void PlaySound(AudioClip sound, bool looping = false, float pitch = 1)
        {
            if (playerAudio.pitch != pitch) playerAudio.pitch = pitch;
            if (playerAudio.clip != sound) playerAudio.clip = sound;
            if (playerAudio.loop != looping) playerAudio.loop = looping;
            if (!playerAudio.isPlaying) playerAudio.Play();
        }

        void PlaySound(bool stop)
        {
            if (stop) playerAudio.Stop();
            playerAudio.loop = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("HurtsPlayer"))
            {
                transform.position = checkpoint.transform.position;
            }

            if (collision.CompareTag("Finish"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }


    }
}
