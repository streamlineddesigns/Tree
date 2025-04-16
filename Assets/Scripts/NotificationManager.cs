using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Notifications.Android;
using UnityEngine.Android;

namespace StudioByStorm {

    public class NotificationManager : MonoBehaviour
    {
        public string[] churnNotificationHeaderTranslations;
        public string[] churnNotificationBodyTranslations;
        public string[] churnNotificationChannelNameTranslations;
        public string[] churnNotificationChannelDescriptions;
        protected int minutesInADay = 1440;

        protected void Start()
        {
            //if its not the FTUE ask user about notifications
            if (!FTUEManager.singleton.isFTUE) {
                StartCoroutine(SendRoutine());
            }
        }

        IEnumerator SendRoutine()
        {
            //request permissions
            RequestNotificationsPermission();
            //wait for age verification to be completed (only required on first open and it will always pass through otherwise)
            yield return new WaitUntil(() => AnalyticsManager.playerAge != 0);
            //register channel for notifications
            RegisterNotificationChannel();
            //cancel all notificiations
            AndroidNotificationCenter.CancelAllNotifications();
            //send churn notification after 4320 minutes ie 72 hours
            SendNotification(churnNotificationHeaderTranslations[GameManager.Language], churnNotificationBodyTranslations[GameManager.Language], 3 * minutesInADay);
        }

        private void RequestNotificationsPermission()
        {
            if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS")) {
                Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            }
        }

        private void RegisterNotificationChannel()
        {
            var channel = new AndroidNotificationChannel()
            {
                Id = "default_channel",
                Name = churnNotificationChannelNameTranslations[GameManager.Language],
                Importance = Importance.Default,
                Description = churnNotificationChannelDescriptions[GameManager.Language],
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }

        private void SendNotification(string title, string text, int fireTimeInMinutes)
        {
            var notification = new AndroidNotification();
            notification.Title = title;
            notification.Text = text;
            notification.FireTime = System.DateTime.Now.AddMinutes(fireTimeInMinutes);
            notification.LargeIcon = "icon_0";

            AndroidNotificationCenter.SendNotification(notification, "default_channel");
        }
    }

}