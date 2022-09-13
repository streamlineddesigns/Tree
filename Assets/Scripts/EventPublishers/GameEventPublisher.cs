using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.EventPublishers {

    public class GameEventPublisher : MonoBehaviour
    {
        public delegate void GameEvent(GameState state);//delegate signature
        public static event GameEvent OnStateChange;//subscribable event

        public static void PublishGameStateChange(GameState state)
        {
            OnStateChange(state);
        }
    }

}