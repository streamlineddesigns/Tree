using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Data;

namespace StudioByStorm {

    public class Translator : MonoBehaviour
    {        
        public List<TextData> textData;
        private LanguagePackName currentLanguage;

        void OnEnable() {
            StartCoroutine(DelayedOnEnable());
        }

        IEnumerator DelayedOnEnable()
        {
            yield return null;

            int savedLanguageInt = GameManager.Singleton.ProgressManager.GetPlayerLanguage();
            LanguagePackName savedLanguageEnum = (LanguagePackName) savedLanguageInt;

            if (savedLanguageInt != -1) {
                currentLanguage = savedLanguageEnum;
                SetLanguage(currentLanguage);
                //Debug.Log("setting text");
            }
        }

        void SetLanguage(LanguagePackName lpn) {

            for (int i = 0; i < textData.Count; i++) {
                if (textData[i].textFields != null && textData[i].textFields.Length > 0) {
                    for (int j = 0; j < textData[i].textFields.Length; j++) {
                        textData[i].textFields[j].text = textData[i].translations[(int)lpn];
                    }
                } else {
                    if (textData[i].textField != null) textData[i].textField.text = textData[i].translations[(int)lpn];
                }
            }

            for (int i = 0; i < textData.Count; i++) {
                if (textData[i].multipleTextFields != null && textData[i].multipleTextFields.Length > 0) {
                    for (int j = 0; j < textData[i].multipleTextFields.Length; j++) {
                        textData[i].multipleTextFields[j].text = textData[i].translations[(int)lpn];
                    }
                } else {
                    if (textData[i].singleTextField != null) textData[i].singleTextField.text = textData[i].translations[(int)lpn];
                }
            }
        }
    }

}