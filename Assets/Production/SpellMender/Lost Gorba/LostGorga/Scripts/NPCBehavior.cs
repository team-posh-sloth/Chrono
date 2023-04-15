using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    [SerializeField]
    Animator dialogue;
    
    Animator npc;
    
    [SerializeField]
    string npcPrefix;

    [SerializeField]
    bool coinNPC, gemNPC, heartNPC;

    [SerializeField]
    GameObject giberish;
    [SerializeField]
    GameObject translation;

    [SerializeField]
    AudioClip open, close;
    AudioSource npcAudio;

    private void Start()
    {
        npc = GetComponent<Animator>();
        npcAudio = GetComponent<AudioSource>();

        // Default translation state
        translation.SetActive(false);
        giberish.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            CheckTranslation();
            dialogue.Play("open");
            npc.Play($"{npcPrefix}_talking");

            PlaySound(open);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            dialogue.Play("close");
            npc.Play($"{npcPrefix}_idle");

            PlaySound(close);
        }
    }

    // Has the translation token for this NPC been collected?
    private void CheckTranslation()
    {
        if (!translation.activeSelf)
        {
            if (coinNPC && Game.Data.Coin)
            {
                translation.SetActive(true);
                giberish.SetActive(false);
                return;
            }
            if (gemNPC && Game.Data.Gem)
            {
                translation.SetActive(true);
                giberish.SetActive(false);
                return;
            }
            if (heartNPC && Game.Data.Heart)
            {
                translation.SetActive(true);
                giberish.SetActive(false);
                return;
            }
        }
    }

    void PlaySound(AudioClip sound)
    {
        npcAudio.clip = sound;
        npcAudio.Play();
    }
}
