using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lockedown
{
    public class Checkpoint : MonoBehaviour
    {

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                //TryGetComponent(collision.gameObject.GetComponent<Player>);
            }
        }
    }
}
