using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StudioByStorm.UI {

    public class UIController : MonoBehaviour
    {
        public View CurrentViewScreen;
        public View PreviousViewScreen;

        public void ShowView(ViewName ViewName)
        {
            View View = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName);
            View.gameObject.SetActive(true);

            if (View.ViewType == ViewType.Screen) {
                PreviousViewScreen = CurrentViewScreen;
                CurrentViewScreen = View;
                PreviousViewScreen.gameObject.SetActive(false);
            }
        }

        public void Close(ViewName ViewName)
        {
            View View = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName);
            View.gameObject.SetActive(false);
        }

        public void Back()
        {
            if (CurrentViewScreen != null && PreviousViewScreen != null) {
                ShowView(PreviousViewScreen.ViewName);
            }
        }
    }

}