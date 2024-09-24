using UnityEngine;
using System.IO;

namespace StudioByStorm.Helpers {

    public class ScreenshotHelper : MonoBehaviour
    {
        private void Update()
        {
            // Check if the space bar is pressed
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TakeScreenshot();
            }
        }

        private void TakeScreenshot()
        {
            string append = (GameManager.Singleton.UIController.CurrentViewScreen.ViewName == ViewName.GameView) ? "a" : (GameManager.Singleton.UIController.CurrentViewScreen.ViewName == ViewName.ZoomView) ? "b" : "c";
            string screenshotFileName = (GameManager.Singleton.LevelManager.currentLevelID + 1).ToString() + append + ".png";

            // Define the path where the screenshot will be saved
            string screenshotPath = Application.persistentDataPath + "/" + screenshotFileName;

            // Start the screenshot capture
            ScreenCapture.CaptureScreenshot(screenshotPath);

            // Log the action in the console
            Debug.Log($"Screenshot taken and saved as: {screenshotPath}");
        }
    }

}