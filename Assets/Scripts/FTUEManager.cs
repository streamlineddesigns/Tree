using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm {

    public class FTUEManager : MonoBehaviour
    {
        public static FTUEManager singleton;
        public bool isFirstOpen = false;
        public bool isFTUE = false;
        private float ftueTimerInSeconds = 300.0f;

        void Awake()
        {
            //init singleton
            if (singleton == null) {
                singleton = this;
                DontDestroyOnLoad(this.gameObject);
            } else {
                Destroy(this);
            }
        }

        void Start()
        {
            //try to get ftue date time string
            string FTUEDateTimeString = GameManager.Singleton.ProgressManager.GetFTUEDateTimeString();
            //if theres no ftue date time string, set current date time
            if (FTUEDateTimeString == "") {
                FTUEDateTimeString = GetCurrentDateTimeString();
                GameManager.Singleton.ProgressManager.UpdateFTUEDateTimeString(FTUEDateTimeString);
                GameManager.Singleton.ProgressManager.Save();
                FTUEManager.singleton.isFirstOpen = true;
            }

            //convert saved ftue date time string into date time
            DateTime FTUEDateTime = ParseDateTimeFromString(FTUEDateTimeString);

            //check if current time is some seconds passed the ftue time
            if (IsPastThreshold(FTUEDateTime, ftueTimerInSeconds)) {
                FTUEManager.singleton.isFTUE = false;
            //FTUE TIME
            } else {
                FTUEManager.singleton.isFTUE = true;
            }
        }

        public void SetFTUEFirstOpenComplete()
        {
            FTUEManager.singleton.isFirstOpen = false;
        }

        private string GetCurrentDateTimeString()
        {
            return DateTime.Now.ToString("o"); // ISO 8601 format (round-trip, great for storing/parsing)
        }

        private DateTime ParseDateTimeFromString(string dateTimeString)
        {
            return DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
        }

        private bool IsPastThreshold(DateTime savedDateTime, float thresholdInSeconds)
        {
            return DateTime.Now > savedDateTime.AddSeconds(thresholdInSeconds);
        }

    }
}