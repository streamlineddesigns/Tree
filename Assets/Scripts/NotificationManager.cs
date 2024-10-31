using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Notifications.Android;
using UnityEngine.Android;

namespace StudioByStorm {

    public class NotificationManager : MonoBehaviour
    {
        protected float waitTimer = 0.0f;

        protected void Start()
        {
            StartCoroutine(SendRoutine());
        }

        protected void Update()
        {
            waitTimer += Time.deltaTime;
        }

        IEnumerator SendRoutine()
        {
            //request permissions
            RequestNotificationsPermission();
            //wait for permissions
            yield return new WaitUntil(() => waitTimer >= 15.0f || Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"));
            //register channel for notifications
            RegisterNotificationChannel();
            //cancel all notificiations
            AndroidNotificationCenter.CancelAllNotifications();
            //send retention notification after 1440 minutes ie 24 hours
            SendNotification("Level Packs!", "Come try different level packs!", 1 * 1440);
            //send churn notification after 4320 minutes ie 72 hours
            SendNotification("Level Packs!", "Come try different level packs!", 3 * 1440);
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
                Name = "Default Channel",
                Importance = Importance.Default,
                Description = "Reminder to Play",
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