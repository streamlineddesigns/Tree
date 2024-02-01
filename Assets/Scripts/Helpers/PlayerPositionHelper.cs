using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Helpers {

    public class PlayerPositionHelper : MonoBehaviour
    {
        private float timer = 0.5f;
        private float currentTimer = 0.0f;
        private List<Vector3> waypointPositions = new List<Vector3>();
        private Vector3 lastWaypointPosition;
        private Vector3 startPosition;
        private bool isRecording = false;
        private bool isPlayingBack = false;
        private bool isOn;

        protected void OnEnable()
        {
            isOn = true;
            StartCoroutine(Record());
        }

        protected void OnDisable()
        {
            isOn = false;
            StopCoroutine(Record());
        }

        public void SetRecording(bool ir)
        {
            if (isRecording == ir) {
                return;
            }

            startPosition = gameObject.transform.position;
            isRecording = ir;
            ResetPositions();
        }

        public void PlayBack()
        {
            if (isRecording && !isPlayingBack) {
                isPlayingBack = true;
                waypointPositions.Reverse();
                waypointPositions.Add(startPosition);
                lastWaypointPosition = waypointPositions[waypointPositions.Count - 1];
                Vector3[] reversedWaypoints = waypointPositions.ToArray();
                gameObject.transform.DOPath(reversedWaypoints, timer, PathType.Linear).SetEase(Ease.Linear);
                SetRecording(false);
                ResetPositions();
                StartCoroutine(BreakOutTimer());
            }
        }

        public void ResetPositions()
        {
            waypointPositions = new List<Vector3>();
        }

        public IEnumerator WaitUntilFinished()
        {
            yield return new WaitUntil(() => (currentTimer > timer) || Vector3.Distance(gameObject.transform.position, lastWaypointPosition) <= 0.1f);
            isPlayingBack = false;
        }

        IEnumerator BreakOutTimer()
        {
            currentTimer = 0.0f;
            while (currentTimer <= 0.5f) {
                yield return new WaitForSeconds(0.1f);
                currentTimer += 0.1f;
            }
        }

        protected IEnumerator Record()
        {
            while(isOn) {
                if (isRecording) {
                    waypointPositions.Add(gameObject.transform.position);
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

}