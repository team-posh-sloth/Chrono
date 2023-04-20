using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chrono
{
    public class Checkpoint : MonoBehaviour
    {
        // Will be retrieved when player enters the checkpoint trigger
        Player player;

        // Will be set when the instance is loaded (Awake())
        public Checkpoint instance;
        Animator anim;

        private void Awake()
        {
            instance = this;
            anim = GetComponent<Animator>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Associate this checkpoint with the Player
            if (collision.gameObject.TryGetComponent(out player) && player.checkpoint != instance)
            {
                if (player.checkpoint != null) player.checkpoint.instance.SetActive(false);

                player.checkpoint = instance;
                SetActive(true);
            }
        }

        // Sets the animation state of the checkpoint
        public void SetActive(bool isActive)
        {
            if (isActive) anim.Play("active");
            else anim.Play("inactive");
        }
    }
}
