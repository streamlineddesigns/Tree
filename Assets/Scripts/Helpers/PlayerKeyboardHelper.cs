using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using StudioByStorm.Gravity.Player;
using StudioByStorm.UI.Controllers;

namespace StudioByStorm.Helpers {

    [System.Serializable]
    public struct KeyboardDirection
    {
        public string key;
        public Vector3 direction;
    }

    public class PlayerKeyboardHelper : MonoBehaviour
    {
        public List<KeyboardDirection> KeyboardDirections = new List<KeyboardDirection>();

        private PlayerController PlayerController;
        private ActionController ActionController;

        protected void Start()
        {
            StartCoroutine(DelayedStart());
        }

        IEnumerator DelayedStart()
        {
            PlayerController = GameManager.Singleton.PlayerController;
            ActionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
            yield return null;
        }

        protected void Update()
        {
            string key = "";

            // Check for up arrow key press
            if (Input.GetKey(KeyCode.UpArrow)) 
            {
                key += "U";
            }
            
            // Check for down arrow key press
            if (Input.GetKey(KeyCode.DownArrow)) 
            {
                key += "D";
            }
            
            // Check for left arrow key press
            if (Input.GetKey(KeyCode.LeftArrow)) 
            {
                key += "L";
            }
            
            // Check for right arrow key press
            if (Input.GetKey(KeyCode.RightArrow)) 
            {
                key += "R";
            }
            
            // Check for spacebar press
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                List<KeyboardDirection> directions = KeyboardDirections.Where(x => x.key == key).ToList();

                if (directions.Count > 0) {
                    FindNearby(directions[0].direction);
                }
            }

        }

        protected void FindNearby(Vector3 direction)
        {
            Node currentNode = ActionController.ActionModel.CurrentNode;
        }

        protected void Jump(Vector3 direction)
        {
            
        }

        //need to check what the current node is
        //need to check the direction I'm currently pressing with the keyboard
        //jump button is going to be space. Need to get direction when space is pressed, and pass it to PlayerController.JumpOverride()
        //Need to check all neighboring nodes
        //Need to find the node whose the closest to whatever direction the keyboard is saying
        //Need to pull back player jump indicator and enable/disable it properly
    }
}