using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.EventPublishers {

    public class GameEventPublisher : MonoBehaviour
    {
        public static GameEventPublisher Singleton;
        public delegate void GameEvent(GameState state);//delegate signature
        public static event GameEvent OnStateChange;//subscribable event

        public delegate void JoystickEvent(Vector2 Direction);//delegate signature
        public static event JoystickEvent OnJoystickDirectionChange;//subscribable event
        public static event JoystickEvent OnTravelJoystickDirectionChange;//subscribable event

        public delegate void TapEvent(Vector2 position);//delegate signature
        public static event TapEvent OnTap;//subscribable event
        public static event TapEvent OnLongTap;//subscribable event

        public delegate void NodeChangeEvent(int NodeID);//delegate signature
        public static event NodeChangeEvent OnPlayerNodeChange;//subscribable event
        public static event NodeChangeEvent OnPlayerEdgeChange;//subscribable event

        public delegate void PlayerEvent();
        public static event PlayerEvent OnPlayerJump;
        public static event PlayerEvent OnPlayerDash;
        public static event PlayerEvent OnPlayerTravel;
        public static event PlayerEvent OnPlayerHitCorrectObstacle;
        public static event PlayerEvent OnPlayerHitWrongObstacle;

        public delegate void UIEvent(ViewName ViewName);//delegate signature
        public static event UIEvent OnViewChange;//subscribable event

        void Awake()
        {
            if (Singleton == null) {
                Singleton = this;
            } else {
                Destroy(this);
            }
        }

        public static void PublishGameStateChange(GameState state)
        {
            if (OnStateChange != null) {
                OnStateChange(state);
            }
        }

        public static void PublishJoystickDirectionChange(Vector2 Direction)
        {
            if (OnJoystickDirectionChange != null) {
                OnJoystickDirectionChange(Direction);
            }
        }

        public static void PublishTravelJoystickDirectionChange(Vector2 Direction)
        {
            if (OnTravelJoystickDirectionChange != null) {
                OnTravelJoystickDirectionChange(Direction);
            }
        }

        public static void PublishTap(Vector2 Position)
        {
            if (OnTap != null) {
                OnTap(Position);
            }
        }

        public static void PublishLongTap(Vector2 Position)
        {
            if (OnLongTap != null) {
                OnLongTap(Position);
            }
        }

        public static void PublishPlayerNodeChange(int NodeID)
        {
            if (OnPlayerNodeChange != null) {
                OnPlayerNodeChange(NodeID);
            }
        }

        public static void PublishPlayerEdgeChange(int ParentNodeID)
        {
            if (OnPlayerEdgeChange != null) {
                OnPlayerEdgeChange(ParentNodeID);
            }
        }

        public static void PublishPlayerJump()
        {
            if (OnPlayerJump != null) {
                OnPlayerJump();
            }
        }

        public static void PublishPlayerDash()
        {
            if (OnPlayerDash != null) {
                OnPlayerDash();
            }
        }

        public static void PublishPlayerTravel()
        {
            if (OnPlayerTravel != null) {
                OnPlayerTravel();
            }
        }

        public static void PublishPlayerHitCorrectObstacle()
        {
            if (OnPlayerHitCorrectObstacle != null) {
                OnPlayerHitCorrectObstacle();
            }
        }

        public static void PublishPlayerHitWrongObstacle()
        {
            if (OnPlayerHitWrongObstacle != null) {
                OnPlayerHitWrongObstacle();
            }
        }

        public static void PublishViewChange(ViewName ViewName)
        {
            if (OnViewChange != null) {
                OnViewChange(ViewName);
            }
        }
    }

}