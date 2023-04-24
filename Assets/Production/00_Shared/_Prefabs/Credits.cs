using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credits : MonoBehaviour
{
    [SerializeField] float speed = 1, maxHeight = 5;
    //// Start is called before the first frame update
    //void Start()
    //{
    //    gameObject = GetComponent<GameObject>();
    //}

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.position.y < maxHeight)
        {
            gameObject.transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
    }
}
