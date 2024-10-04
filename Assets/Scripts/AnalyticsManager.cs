using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using StudioByStorm.EventPublishers;

namespace StudioByStorm {

    public class AnalyticsManager : MonoBehaviour
    {
        public static bool isDebugging = false;
        private static int playerAge = 0;
        private static int minAge = 13;

        public static void InitSDK()
        {
            playerAge = GameManager.Singleton.ProgressManager.GetPlayerAge();
            if (playerAge >= minAge) {
                GameAnalytics.Initialize();
            }
        }

        public static void NewProgressionEvent(GAProgressionStatus status, int chapter, int level)
        {
            if (playerAge >= minAge) {
                if (isDebugging) {
                    Debug.Log("Status: " + status.ToString() + " Chapter: " + chapter.ToString()  + " Level: " + level.ToString());
                }
                GameAnalytics.NewProgressionEvent(status, chapter.ToString(), level.ToString());
            }
        }

        public static void NewProgressionEvent(GAProgressionStatus status, int chapter, int level, int score)
        {
            if (playerAge >= minAge) {
                if (isDebugging) {
                    Debug.Log("Status: " + status.ToString() + " Chapter: " + chapter.ToString()  + " Level: " + level.ToString()  + " Score: " + score.ToString());
                }
                GameAnalytics.NewProgressionEvent(status, chapter.ToString(), level.ToString(), score.ToString());
            }
        }
    }

}