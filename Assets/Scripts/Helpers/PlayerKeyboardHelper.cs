using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
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

        public Texture2D cursorTexture;
        public TMP_Text BottomText;

        private PlayerController PlayerController;
        private ActionController ActionController;

        protected void Start()
        {
            DontDestroyOnLoad(gameObject);
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

            if (Input.GetKey(KeyCode.Space)) 
            {
                Cursor.SetCursor(cursorTexture, Vector3.zero, CursorMode.Auto);
                StartCoroutine(PrintText("Teleport"));
            }

            // Check for up arrow key press
            if (Input.GetKey(KeyCode.UpArrow)) 
            {
                key += "U";
                StartCoroutine(PrintText("Dash"));
            }
            
            // Check for down arrow key press
            if (Input.GetKey(KeyCode.DownArrow)) 
            {
                key += "D";
                StartCoroutine(PrintText("Connect"));
            }
            
            // Check for left arrow key press
            if (Input.GetKey(KeyCode.LeftArrow)) 
            {
                key += "L";
                
                StartCoroutine(PrintText("Jump"));
            }
            
            // Check for right arrow key press
            if (Input.GetKey(KeyCode.RightArrow)) 
            {
                key += "R";
                StartCoroutine(PrintText("Phase"));
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

        IEnumerator PrintText(string textToPrint)
        {
            BottomText.text = "";
            textToPrint = textToPrint.Replace("<br>", Environment.NewLine);

            //get characters from current chapter heading
            char[] charArray = textToPrint.ToCharArray();
            List<char> charList = new List<char>();
            //iterate over characters and show them one at a time
            for (int i = 0; i < charArray.Length; i++) {
                AudioManager.Singleton.Play(SoundType.Typing);
                charList.Add(charArray[i]);
                char[] currentCharacters = charList.ToArray();
                BottomText.text = new string(currentCharacters);
                yield return new WaitForSeconds(0.075f);
            }
        }

        //need to check what the current node is
        //need to check the direction I'm currently pressing with the keyboard
        //jump button is going to be space. Need to get direction when space is pressed, and pass it to PlayerController.JumpOverride()
        //Need to check all neighboring nodes
        //Need to find the node whose the closest to whatever direction the keyboard is saying
        //Need to pull back player jump indicator and enable/disable it properly
    }
}