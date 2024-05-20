using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;

namespace StudioByStorm {

    public class AnalyticsManager : MonoBehaviour
    {
        void Start()
        {
            GameAnalytics.Initialize();
        }

    }

}