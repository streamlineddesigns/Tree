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

        public delegate void SwipeEvent(Vector2 Direction);//delegate signature
        public static event SwipeEvent OnSwipe;//subscribable event

        public delegate void TapEvent(Vector2 position);//delegate signature
        public static event TapEvent OnTap;//subscribable event
        public static event TapEvent OnLongTap;//subscribable event

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
            OnStateChange(state);
        }

        public static void PublishSwipe(Vector2 Direction)
        {
            OnSwipe(Direction);
        }

        public static void PublishTap(Vector2 Position)
        {
            OnTap(Position);
        }

        public static void PublishLongTap(Vector2 Position)
        {
            OnLongTap(Position);
        }
    }

}