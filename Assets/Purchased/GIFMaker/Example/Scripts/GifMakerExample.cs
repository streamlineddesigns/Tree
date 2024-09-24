using UnityEngine;
using UnityEngine.UI;

public class GifMakerExample : MonoBehaviour
{
    public int steps = 0;

    private void Update()
        {
            // Check if the space bar is pressed
            if (Input.GetKeyDown(KeyCode.Space) && steps == 0)
            {
                steps += 1;
                StartRecord();

            } else if (Input.GetKeyDown(KeyCode.Space) && steps == 1) {
                steps += 1;
                StopRecord();

            } else if (Input.GetKeyDown(KeyCode.Space) && steps == 2) {
                steps = 0;
                SaveGif();
            }
        }

    //1. Start capture frames
    public void StartRecord()
    {
        GifMaker.StartRecord(GifMaker.Instance.RecordOptions);
    }

    //2. Stop capture frames
    public void StopRecord()
    {
        GifMaker.StopRecord();                                  
    }

    //3. Start encoding thead and save Test.gif file to player folder
    public void SaveGif()
    {
        GifMaker.GifEncodeOptions encodeOptions = new GifMaker.GifEncodeOptions(
                Application.persistentDataPath + "\\Test.gif",  //Set destination folder for gif animation file
                0,                                              //Set animation repeat count to loop
                1,                                              //Set the best possible quality for gif animation
                0.1f,                                           //Set delay for animation to 10 frames per second
                OnSaveComplete);                                //Assign OnSaveComplete method

        GifMaker.SaveGif(encodeOptions);
    }

    private void OnSaveComplete()
    {
        //This method will be called when gif file will be fully created
    }

    private void OnEndFrameCapture()
    {
        //This method will be immediately called before capturing frame
    }

    private void OnStartFrameCapture()
    {
        //This method will be immediately called after capturing frame
    }

}
