using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chrono
{
    public class Fragile : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == 8) Destroy(gameObject);
        }
    }
}
