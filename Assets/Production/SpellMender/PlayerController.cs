using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chrono
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController instance { get; private set; }

        public bool Interacting { get; private set; }
        public bool Moving { get; private set; }
        public bool Jumping { get; private set; }
        public bool FlipX { get; private set; }

        [SerializeField]
        float moveSpeed = 5f;
        [SerializeField]
        float jumpMagnitude = 5f;

        float inputMagnitude = 0;

        [SerializeField]
        AudioClip jumpSound;

        Rigidbody2D player;
        SpriteRenderer sprite;
        Animator anim;
        AudioSource playerAudio;

        //DEBUG
        Vector3 movement;

        [SerializeField]
        int timeScale;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            player = GetComponent<Rigidbody2D>();
            sprite = GetComponent<SpriteRenderer>();
            anim = GetComponentInChildren<Animator>();
            playerAudio = GetComponent<AudioSource>();
        }

        private void FixedUpdate()
        {
            Move();
            //Jump();
        }

        void Update()
        {
            //TimeDilation();
            //SetInputFlags();
            Interact();
            Animate();
        }

        void Move()
        {
            if (Time.timeScale != 1)
            {
                player.velocity = new Vector2(inputMagnitude * moveSpeed * 16f, player.velocity.y);
            }
            player.velocity = new Vector2(inputMagnitude * moveSpeed, player.velocity.y);
        }

        void Jump()
        {
            if (Input.GetKey(KeyCode.Space) && IsGrounded())
            {
                PlaySound(jumpSound);
                player.velocity = new Vector2(player.velocity.x, jumpMagnitude);
            }
        }


        //-----------------------------------------------------
        // Source: Code Monkey - 3 ways to do a Ground Check in Unity
        // Link: https://youtu.be/c3iEl5AwUF8?t=576
        //-----------------------------------------------------
        bool IsGrounded()
        {
            float detectionRange = 0.05f;
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            RaycastHit2D groundDetection = Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.down, detectionRange, LayerMask.GetMask("Ground"));

            Color rayColor;
            if (groundDetection.collider != null) rayColor = Color.green; else rayColor = Color.red;
            Debug.DrawRay(collider.bounds.center + new Vector3(collider.bounds.center.x, 0), Vector2.down * (collider.bounds.extents.y + detectionRange), rayColor);
            Debug.DrawRay(collider.bounds.center + new Vector3(collider.bounds.center.x, 0), Vector2.down * (collider.bounds.extents.y + detectionRange), rayColor);
            Debug.DrawRay(collider.bounds.center + new Vector3(0, collider.bounds.center.y), Vector2.right * (collider.bounds.extents.x), rayColor);
            return groundDetection.collider != null;
        }

        void SetInputFlags()
        {
            // Resolves input issue between scenes by reading horizontal input every frame on each specific key
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                inputMagnitude = Input.GetAxisRaw("Horizontal");
            }
            else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || IsGrounded())
            {
                inputMagnitude = 0;
            }

            if (inputMagnitude != 0)
            {
                // Set Moving Flag
                if (!Moving) Moving = true;

                // Set FlipX Flag
                if (inputMagnitude < 0) { FlipX = true; }
                else if (inputMagnitude > 0) { FlipX = false; }
            }
            else if (Moving) Moving = false;

            if (!IsGrounded() && !Jumping) Jumping = true;
            else if (Jumping && IsGrounded()) Jumping = false;
        }

        void Interact()
        {
            if (Input.GetKeyUp(KeyCode.E)) { Interacting = true; }
        }

        void Animate()
        {
            // Face the sprite using FlipX flag
            if (sprite.flipX != FlipX) sprite.flipX = FlipX;

            // Change between animation states
            if (Jumping)
            {
                anim.Play("player_jump");
            }
            else if (Moving)
            {
                anim.Play("player_run");
            }
            else
            {
                anim.Play("player_idle");
            }
        }

        void PlaySound(AudioClip sound)
        {
            playerAudio.clip = sound;
            playerAudio.Play();
        }

        void TimeDilation()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                Time.timeScale = 0.25f;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                Time.timeScale = 1;
            }
        }


    }

}
