using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Helpers {

    public class PlayerPositionHelper : MonoBehaviour
    {
        private List<Vector3> waypointPositions = new List<Vector3>();
        private Vector3 lastWaypointPosition;
        private Vector3 startPosition;
        private bool isRecording = false;
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
            if (isRecording) {
                waypointPositions.Reverse();
                waypointPositions.Add(startPosition);
                lastWaypointPosition = waypointPositions[waypointPositions.Count - 1];
                Vector3[] reversedWaypoints = waypointPositions.ToArray();
                gameObject.transform.DOPath(reversedWaypoints, 0.5f, PathType.Linear).SetEase(Ease.Linear);
                SetRecording(false);
                ResetPositions();
            }
        }

        public void ResetPositions()
        {
            waypointPositions = new List<Vector3>();
        }

        public IEnumerator WaitUntilFinished()
        {
            yield return new WaitUntil(() => Vector3.Distance(gameObject.transform.position, lastWaypointPosition) <= 0.1f);
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