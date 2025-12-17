using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Obstacles.Animations {

    public class SpinAnimation : Animation
    {
        protected override IEnumerator AnimationUpdate()
        {
            yield return new WaitUntil(() => animate);

            float speed = 2.0f;

            if (GameManager.Singleton != null && GameManager.Singleton.PlayerController.controlType == ControlType.Animate) {
                speed = 6.0f;
            } else if (GameManager.Singleton != null && GameManager.Singleton.PlayerController.controlType == ControlType.Tap) {
                float currentLevelID = GameManager.Singleton.LevelManager.displayLevelID * 1.0f;
                float currentChapterID = GameManager.Singleton.LevelManager.currentChapterID * 15.0f;//will be 0 or 15 for chapter 1 vs chapter 2
                float actualCurrentLevel = currentLevelID + currentChapterID;
                float maxLevel = GameManager.Singleton.LevelManager.levelChapters.chapters.Count * 15.0f;
                float percentage = actualCurrentLevel / maxLevel;
                float clampedPercent = Mathf.Min(percentage, 1.0f);
                float value = DOVirtual.EasedValue(3.0f, 6.0f, clampedPercent, Ease.Linear);
                speed = value;
            }

            while(animate) {
                if (direction == 0) {
                    gameObject.transform.Rotate(0.0f, 0.0f, -speed);
                } else {
                    gameObject.transform.Rotate(0.0f, 0.0f, speed);
                }

                /*float speed = (360.0f / time) / 30.0f;
                gameObject.transform.Rotate(0f, 0f, speed);*/

                
                yield return new WaitForSeconds(0.0333f);
                
            }
        }

    }

}