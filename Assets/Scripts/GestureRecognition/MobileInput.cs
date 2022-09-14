using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.EventPublishers;

namespace StudioByStorm.GestureRecognition {

    public class MobileInput : MonoBehaviour 
    {
        
        public bool LongTap  { get { return longtap; } }
        public bool Tap { get { return tap; } }
        public Vector2 SwipeDelta { get { return swipeDelta; } }
        public bool SwipeLeft { get { return swipeLeft; } }
        public bool SwipeRight { get { return swipeRight; } }
        public bool SwipeUp { get { return swipeUp; } }
        public bool SwipeDown { get { return swipeDown; } }
        public Vector2 StartTouch { get { return startTouch; } }

        private bool isLive;
        private int tapCount = 0;
        private float doubleTapTimer = 0.0f;

        private bool longTapTimerStarted;
        private float timeToCountAsLongTap = 1.0f;
        private float longTapTimer = 0.0f;

        private const float DEADZONE = 50.0f;
        private bool longtap, tap, swipeLeft, swipeRight, swipeUp, swipeDown;
        private Vector2 swipeDelta, startTouch;

        void OnEnable()
        {
            GameEventPublisher.OnStateChange += OnStateChange;
        }

        void OnDisable()
        {
            GameEventPublisher.OnStateChange -= OnStateChange;
        }

        protected void OnStateChange(GameState state)
        {
            if (state == GameState.GameStart) {
                isLive = true;
            }
        }
        
        private void Update ()
        {
            if (! isLive) {
                return;
            }
            //Resetting all the booleans
            tap = swipeLeft = swipeRight = swipeDown = swipeUp = false;

            #region Standalone Inputs
            if (Input.GetMouseButtonDown(0)) {
                tap = true;
                startTouch = Input.mousePosition;
                //For double click
                tapCount++;
            } else if(Input.GetMouseButtonUp(0)) {
                if (! LongTap) {
                    if (GameEventPublisher.Singleton != null && startTouch != null) {
                        GameEventPublisher.PublishTap(startTouch);
                    }
                }
                
                longtap = false;
                startTouch = swipeDelta = Vector2.zero;
                longTapTimerStarted = false;
                longTapTimer = 0.0f;
            }
            #endregion

            #region Mobile Inputs
            if (Input.touches.Length != 0) {
                if (Input.touches[0].phase == TouchPhase.Began) 
                {
                    tap = true;

                    /* for double click
                    tapCount++;*/

                    startTouch = Input.touches[0].position;
                } 
                else if(Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled) 
                {
                    startTouch = swipeDelta = Vector2.zero;
                }
            } 
            #endregion
            if (tap) {
                longTapTimerStarted = true;                    
            }


            //Calculate Distance
            swipeDelta = Vector2.zero;
            if(startTouch != Vector2.zero) 
            {
                //Check with mobile
                if(Input.touches.Length != 0) {
                    swipeDelta = Input.touches[0].position - startTouch;
                }

                //Check with standalone
                else if (Input.GetMouseButton(0))
                {
                    swipeDelta = (Vector2)Input.mousePosition - startTouch;
                }
            }

            if (longTapTimerStarted) {
                if (longTapTimer < timeToCountAsLongTap) {
                    longTapTimer += Time.deltaTime;
                } else if (! LongTap) {
                    longtap = true;
                    if (GameEventPublisher.Singleton != null) {
                        GameEventPublisher.PublishLongTap(swipeDelta);
                    }
                    
                }
            }

            //Check if we're beyond the deadzone
            if (swipeDelta.magnitude > DEADZONE) 
            {
                //publish event
                GameEventPublisher.PublishSwipe(swipeDelta);
                //Reset tapCount thats used for double tap
                tapCount = 0;

                //This is a confirmed swipe
                float x = swipeDelta.x;
                float y = swipeDelta.y;

                if(Mathf.Abs(x) > Mathf.Abs(y))
                {
                    //Left or Right
                    if (x < 0)
                        swipeLeft = true;
                    else
                        swipeRight = true;
                }
                else 
                {
                    //up or down
                    if (y < 0)
                        swipeDown = true;
                    else
                        swipeUp = true;
                }
                //reset
                startTouch = swipeDelta = Vector2.zero;
            } else {
                //Double Tap
                if (tapCount >= 2) {
                    //What you want to do
                    doubleTapTimer = 0.0f;
                    tapCount = 0;
                }
            }
            
            #region DoubleTap
            if (tapCount > 0) {
                doubleTapTimer += Time.deltaTime;
            }
            if (doubleTapTimer > 0.5f) {
                doubleTapTimer = 0f;
                tapCount = 0;
            }
            #endregion
        }

        public int getSwipeDirection()
        {
            if (SwipeLeft) {
                return 0;
            } else if (SwipeUp) {
                return 1;
            }  else if (SwipeRight) {
                return 2;
            }  else if (SwipeDown) {
                return 3;
            }
            return -1;
        }
    }

}