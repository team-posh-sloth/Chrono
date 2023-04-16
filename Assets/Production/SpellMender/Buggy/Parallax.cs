using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chrono
{

    //-----------------------------------------------------
    // Source: Dani - How to infinite scrolling background
    // Link: https://www.youtube.com/watch?v=zit45k6CUMk
    //-----------------------------------------------------

    public class Parallax : MonoBehaviour
    {
        float lengthX, startposX, lengthY, startposY;
        public GameObject cam;
        public float parallaxEffectX, parallaxEffectY;

        // Start is called before the first frame update
        void Start()
        {
            startposX = transform.position.x;
            startposY = transform.position.y;

            lengthX = GetComponent<SpriteRenderer>().bounds.size.x;
            lengthY = GetComponent<SpriteRenderer>().bounds.size.y;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            float tempX = (cam.transform.position.x * (1 - parallaxEffectX));
            //float tempY = (cam.transform.position.y * (1 - parallaxEffectY));

            float distX = (cam.transform.position.x * parallaxEffectX);
            //float distY = (cam.transform.position.y * parallaxEffectY);

            //transform.position = new Vector3(startposX + distX, startposY + distY, transform.position.z);
            transform.position = new Vector3(startposX + distX, transform.position.y, transform.position.z);

            if (tempX > startposX + lengthX) startposX += lengthX;
            else if (tempX < startposX - lengthX) startposX -= lengthX;
        }
    }

}