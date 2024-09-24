using System;
using System.Collections.Generic;
using System.Threading;
using GifComponents;
using UnityEngine;
using System.Collections;

public class GifMaker : MonoBehaviour
{
    public delegate void StartFrameCapture();
    public delegate void EndFrameCapture();
    public delegate void SaveComplete();

    #region Structs
    [Serializable]
    public struct GifRecordOptions
    {
        public Camera[] TargetCameras;
        public float FrameInterval;
        public Rect FrameRect;
        public int FrameRectWidth;
        public int FrameRectHeight;

        public StartFrameCapture OnStartFrameCapture;
        public EndFrameCapture OnEndFrameCapture;

        /// <summary>
        /// Frame recording options for GifMaker
        /// </summary>
        /// <param name="targetCameras">reference to the array of cameras that will record the frames</param>
        /// <param name="frameInterval">time between frame capture in seconds</param>
        /// <param name="frameRect">rectange of the screen which will determine the frame region</param>
        /// <param name="frameRectWidth">resulting width of the frame</param>
        /// <param name="frameRectHeight">resulting height of the frame</param>
        /// <param name="onStartFrameCapture">called before capturing frame</param>
        /// <param name="onEndFrameCapture">called after capturing frame</param>
        public GifRecordOptions(Camera[] targetCameras, float frameInterval, Rect frameRect, int frameRectWidth, int frameRectHeight, StartFrameCapture onStartFrameCapture, EndFrameCapture onEndFrameCapture)
        {
            TargetCameras = targetCameras;
            FrameInterval = frameInterval;
            FrameRect = frameRect;
            FrameRectWidth = frameRectWidth;
            FrameRectHeight = frameRectHeight;
            OnStartFrameCapture = onStartFrameCapture;
            OnEndFrameCapture = onEndFrameCapture;
        }
        /// <summary>
        /// Frame recording options for GifMaker
        /// </summary>
        /// <param name="targetCamera">reference to the camera that will record the frames</param>
        /// <param name="frameInterval">time between frame capture in seconds</param>
        /// <param name="frameRect">rectange of the screen which will determine the frame region</param>
        /// <param name="frameRectWidth">resulting width of the frame</param>
        /// <param name="frameRectHeight">resulting height of the frame</param>
        /// <param name="onStartFrameCapture">called before capturing frame</param>
        /// <param name="onEndFrameCapture">called after capturing frame</param>
        public GifRecordOptions(Camera targetCamera, float frameInterval, Rect frameRect, int frameRectWidth, int frameRectHeight, StartFrameCapture onStartFrameCapture, EndFrameCapture onEndFrameCapture)
        {
            TargetCameras = new Camera[1];
            TargetCameras[0] = targetCamera;
            FrameInterval = frameInterval;
            FrameRect = frameRect;
            FrameRectWidth = frameRectWidth;
            FrameRectHeight = frameRectHeight;
            OnStartFrameCapture = onStartFrameCapture;
            OnEndFrameCapture = onEndFrameCapture;
        }
        /// <summary>
        /// Frame recording options for GifMaker
        /// </summary>
        /// <param name="targetCameras">reference to the array of cameras that will record the frames</param>
        /// <param name="frameInterval">time between frame capture in seconds</param>
        /// <param name="frameRect">rectange of the screen which will determine the frame region</param>
        /// <param name="frameRectWidth">resulting width of the frame</param>
        /// <param name="frameRectHeight">resulting height of the frame</param>
        public GifRecordOptions(Camera[] targetCameras, float frameInterval, Rect frameRect, int frameRectWidth, int frameRectHeight)
        {
            TargetCameras = targetCameras;
            FrameInterval = frameInterval;
            FrameRect = frameRect;
            FrameRectWidth = frameRectWidth;
            FrameRectHeight = frameRectHeight;
            OnStartFrameCapture = null;
            OnEndFrameCapture = null;
        }
        /// <summary>
        /// Frame recording options for GifMaker
        /// </summary>
        /// <param name="targetCamera">reference to the camera that will record the frames</param>
        /// <param name="frameInterval">time between frame capture in seconds</param>
        /// <param name="frameRect">rectange of the screen which will determine the frame region</param>
        /// <param name="frameRectWidth">resulting width of the frame</param>
        /// <param name="frameRectHeight">resulting height of the frame</param>
        public GifRecordOptions(Camera targetCamera, float frameInterval, Rect frameRect, int frameRectWidth, int frameRectHeight)
        {
            TargetCameras = new Camera[1];
            TargetCameras[0] = targetCamera;
            FrameInterval = frameInterval;
            FrameRect = frameRect;
            FrameRectWidth = frameRectWidth;
            FrameRectHeight = frameRectHeight;
            OnStartFrameCapture = null;
            OnEndFrameCapture = null;
        }
    }

    [Serializable]
    public struct GifEncodeOptions
    {
        public string OutputPath;

        public int RepeatCount;
        public float Quality;
        public float Delay;

        public SaveComplete OnSaveComplete;

        /// <summary>
        /// Encode options for GifMaker
        /// </summary>
        /// <param name="outputPath">full path to the folder where gif file will be created (including file name with format)</param>
        /// <param name="repeatCount">amount of GIF animation plays (0 for loop)</param>
        /// <param name="quality">quality factor for gif animation (from 0.05 to 1)</param>
        /// <param name="delay">delay time of the GIF animation in seconds (from 0.1 to ...)</param>
        /// <param name="onSaveComplete">called when gif file will be fully created</param>
        public GifEncodeOptions(string outputPath, int repeatCount, float quality, float delay, SaveComplete onSaveComplete)
        {
            OutputPath = outputPath;
            if (repeatCount < 0)
                repeatCount = 0;
            RepeatCount = repeatCount;
            if (quality < 0.05f)
                quality = 0.05f;
            if (quality > 1)
                quality = 1;
            Quality = quality;
            Delay = delay;
            OnSaveComplete = onSaveComplete;
        }

        /// <summary>
        /// Encode options for GifMaker
        /// </summary>
        /// <param name="outputPath">full path to the folder where gif file will be created (including file name with format)</param>
        /// <param name="repeatCount">amount of GIF animation plays (0 for loop)</param>
        /// <param name="quality">quality factor for gif animation (from 0.05 to 1)</param>
        /// <param name="delay">delay time of the GIF animation in seconds</param>
        public GifEncodeOptions(string outputPath, int repeatCount, float quality, float delay)
        {
            OutputPath = outputPath;
            if (repeatCount < 0)
                repeatCount = 0;
            RepeatCount = repeatCount;
            if (quality < 0.05f)
                quality = 0.05f;
            if (quality > 1)
                quality = 1;
            Quality = quality;
            Delay = delay;
            OnSaveComplete = null;
        }
    }
    #endregion

    public GifRecordOptions RecordOptions;
    public GifEncodeOptions EncodeOptions;

    public static bool IsRecording { get { return Instance._recording; } }
    public static bool IsEncoding { get { return Instance._encoding; } }

    public static GifMaker Instance { get; private set; }

    #region TempFields
    private bool _recording;
    private bool _encoding;

    private List<Texture2D> _textures = new List<Texture2D>();
    private GifImage[] _gifImages;

    private RenderTexture _rendTex;
    private Thread _encodeThread;
    private float _lastTime;
    private bool _encodeThreadAlive;
    #endregion

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        if (_recording)
        {
            if (Time.realtimeSinceStartup >= _lastTime + RecordOptions.FrameInterval)
            {
                StartCoroutine(CaptureFrame());
                _lastTime = Time.realtimeSinceStartup;
            }
        }

        if (_encodeThreadAlive)
        {
            if (!_encoding)
            {
                if (EncodeOptions.OnSaveComplete != null)
                    EncodeOptions.OnSaveComplete();
                _encodeThreadAlive = false;
            }
        }

    }

    #region StaticVoidMembers 

    public static void StartRecord()
    {
        Instance.StartRecordThis();
    }

    public static void StartRecord(GifRecordOptions gifRecordOptionses)
    {
        Instance.RecordOptions = gifRecordOptionses;
        Instance.StartRecordThis();
    }

    public static void StopRecord()
    {
        Instance.StopRecordThis();
    }

    public static void SaveGif()
    {
        Instance.SaveGifThis();
    }

    public static void SaveGif(GifEncodeOptions gifEncodeOptionses)
    {
        Instance.EncodeOptions = gifEncodeOptionses;
        Instance.SaveGifThis();
    }
    #endregion

    void StartRecordThis()
    {
        if (_recording)
            return;

        if (_encodeThread != null)
            if (_encodeThread.IsAlive)
                _encodeThread.Abort();

        _rendTex = new RenderTexture(RecordOptions.FrameRectWidth, RecordOptions.FrameRectHeight, 24);

        _textures.Clear();
        _lastTime = Time.realtimeSinceStartup;

        _recording = true;
    }

    void StopRecordThis()
    {
        _recording = false;
    }

    void SaveGifThis()
    {
        if (_textures.Count == 0 || _recording)
            return;

        _encoding = true;
        _encodeThreadAlive = true;

        _gifImages = new GifImage[_textures.Count];

        for (int i = 0; i < _gifImages.Length; i++)
        {
            Color32[] pixColors = _textures[i].GetPixels32();
            GifColor[] pixColorsGif = new GifColor[pixColors.Length];

            for (int j = 0; j < pixColorsGif.Length; j++)
            {
                pixColorsGif[j] = new GifColor(pixColors[j].r, pixColors[j].g, pixColors[j].b);
            }

            _gifImages[i] = new GifImage(_textures[i].width, _textures[i].height, pixColorsGif);
        }

        _encodeThread = new Thread(EncodeThread);
        _encodeThread.Start();
    }

    void EncodeThread()
    {
        AnimatedGifEncoder gifEncoder = new AnimatedGifEncoder();

        gifEncoder.SetRepeat(EncodeOptions.RepeatCount);
        gifEncoder.SetQuality((int)EncodeOptions.Quality * 20);
        gifEncoder.SetDelay((int)EncodeOptions.Delay * 1000);
        gifEncoder.Start(EncodeOptions.OutputPath);

        for (int i = 0; i < _gifImages.Length; i++)
        {
            gifEncoder.AddFrame(_gifImages[i]);
        }

        gifEncoder.Finish();

        _encoding = false;
    }

    IEnumerator CaptureFrame()
    {
        if (RecordOptions.OnStartFrameCapture != null)
            RecordOptions.OnStartFrameCapture();

        yield return new WaitForEndOfFrame();

        _textures.Add(GetFrameTexture());

        if (RecordOptions.OnEndFrameCapture != null)
            RecordOptions.OnEndFrameCapture();
    }

    Texture2D GetFrameTexture()
    {
        for (int i = 0; i < RecordOptions.TargetCameras.Length; i++)
        {
            if (RecordOptions.TargetCameras != null)
            {
                RecordOptions.TargetCameras[i].targetTexture = _rendTex;
                RecordOptions.TargetCameras[i].Render();
            }
        }
        
        RenderTexture currentActiveRt = RenderTexture.active;

        RenderTexture.active = _rendTex;

        Texture2D tex = new Texture2D(_rendTex.width, _rendTex.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        
        RenderTexture.active = currentActiveRt;

        for (int i = 0; i < RecordOptions.TargetCameras.Length; i++)
        {
            if (RecordOptions.TargetCameras != null)
                RecordOptions.TargetCameras[i].targetTexture = null;
        }

        return tex;
        
    }

    void OnDisable()
    {
        if (_encodeThread != null)
            if (_encodeThread.IsAlive)
                _encodeThread.Abort();
    }
    
}
