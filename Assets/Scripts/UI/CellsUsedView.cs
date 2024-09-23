using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

namespace StudioByStorm.UI {

    public class CellsUsedView : View
    {
        public GameObject cellsUsedContainer;
        public TMP_Text cellsUsedText;
        private float originalScale;
        private Queue<int> valuesToAdd = new Queue<int>();
        private int usedCells = 0;
        private int totalCells = 0;
        private bool isAnimating = false;
        private int xpMultiplier = 10;

        protected void OnEnable()
        {
            originalScale = cellsUsedContainer.transform.localScale.x;
            totalCells = GameManager.Singleton.LevelManager.currentLevelNodeCount * xpMultiplier;
            //set text
            string value = "0/" + totalCells.ToString();
            cellsUsedText.text = value;
        }
            
        public void incrementUsedCells(int val)
        {
            valuesToAdd.Enqueue(val * xpMultiplier);
        }

        private void Update()
        {
            if (! isAnimating && valuesToAdd.Count > 0) {
                isAnimating = true;
                StartCoroutine(AnimateText());
            }
        }

        IEnumerator AnimateText()
        {
            //increment used cells
            usedCells += valuesToAdd.Dequeue();
            //scale up
            float targetScale = originalScale * 1.5f;
            cellsUsedContainer.transform.DOScale(targetScale, 0.1f).OnComplete(() => {
                string value = usedCells.ToString() + "/" + totalCells.ToString();
                //set text
                cellsUsedText.text = value;
                //scale back down
                cellsUsedContainer.transform.DOScale(originalScale, 0.1f);
            });

            yield return new WaitForSeconds(0.2f);

            isAnimating = false;
        }
    }

}