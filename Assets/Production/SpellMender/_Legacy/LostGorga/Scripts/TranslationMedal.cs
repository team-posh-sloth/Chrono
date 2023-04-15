using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslationMedal : MonoBehaviour
{
    [SerializeField]
    string itemName;

    [SerializeField]
    bool isCoin, isGem, isHeart;

    bool collided = false;

    private void Start()
    {
        if ((isCoin && Game.Data.Coin) || (isGem && Game.Data.Gem) || (isHeart && Game.Data.Heart))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collided == false)
        {
            collided = true;
            GetComponent<Animator>().Play("item_collect");
            Game.Data.Collect($"{itemName}");
            Destroy(gameObject, GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length);
        }
    }
}
