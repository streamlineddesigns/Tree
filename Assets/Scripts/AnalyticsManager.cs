using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using StudioByStorm.EventPublishers;

namespace StudioByStorm {

    public class AnalyticsManager : MonoBehaviour
    {
        public static bool isDebugging = false;
        public static int playerAge = 0;
        public static int minAge = 16;

        public static void InitSDK()
        {
            playerAge = GameManager.Singleton.ProgressManager.GetPlayerAge();
            //initialize analytics normally
            if (playerAge >= minAge) {
                GameAnalytics.Initialize();
            //initialize analytics without Personally Identifiable Information ie COPPA compliancy
            } else {
                GameAnalytics.EnableAdvertisingIdTracking(false);
                GameAnalytics.Initialize();
            }
        }

        public static void NewProgressionEvent(GAProgressionStatus status, LevelPackName lpn, int chapter, int level)
        {
            if (playerAge >= minAge) {
                string LevelPackChapter = lpn.ToString() + chapter.ToString();
                if (isDebugging) {
                    Debug.Log("Status: " + status.ToString() + " Level Pack: " + lpn.ToString() + " Chapter: " + chapter.ToString()  + " Level: " + level.ToString());
                }
                GameAnalytics.NewProgressionEvent(status, LevelPackChapter, level.ToString());
            }
        }

        public static void NewProgressionEvent(GAProgressionStatus status, LevelPackName lpn, int chapter, int level, int score)
        {
            if (playerAge >= minAge) {
                string LevelPackChapter = lpn.ToString() + chapter.ToString();
                if (isDebugging) {
                    Debug.Log("Status: " + status.ToString() + " Level Pack: " + lpn.ToString() + " Chapter: " + chapter.ToString()  + " Level: " + level.ToString()  + " Score: " + score.ToString());
                }
                GameAnalytics.NewProgressionEvent(status, LevelPackChapter, level.ToString(), score.ToString());
            }
        }
    }

}