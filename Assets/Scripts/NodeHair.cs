using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm {

    public class NodeHair : MonoBehaviour
    {
        protected Quaternion originalRotation;
        protected bool isRotateOn = true;

        public Vector3 degreesToRotate = new Vector3(0, 0, 30);


        void OnEnable()
        {
            // Store the game object's original rotation
            originalRotation = transform.rotation;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Player" && isRotateOn) {
                //turn rotate off
                isRotateOn = false;
                //get direction
                Vector3 direction = other.gameObject.transform.position - transform.position;

                if (Vector3.Distance(other.gameObject.transform.right, direction) < Vector3.Distance(- other.gameObject.transform.right, direction)) {
                    StartCoroutine(DOLeftRotate());
                } else {
                    StartCoroutine(DORightRotate());
                }

                
            }
        }

        IEnumerator DOLeftRotate()
        {
            float counter = 0;
            while(counter < 16) {
                counter++;
                transform.Rotate(new Vector3(0, 0, 2));
                yield return new WaitForSeconds(0.016f);
            }

            

            counter = 0;
            while(counter < 16) {
                counter++;
                transform.Rotate(new Vector3(0, 0, -2));
                yield return new WaitForSeconds(0.016f);
            }

            isRotateOn = true;
        }

        IEnumerator DORightRotate()
        {
            float counter = 0;
            while(counter < 16) {
                counter++;
                transform.Rotate(new Vector3(0, 0, -2));
                yield return new WaitForSeconds(0.016f);
            }

            

            counter = 0;
            while(counter < 16) {
                counter++;
                transform.Rotate(new Vector3(0, 0, 2));
                yield return new WaitForSeconds(0.016f);
            }

            isRotateOn = true;
        }
    }

}