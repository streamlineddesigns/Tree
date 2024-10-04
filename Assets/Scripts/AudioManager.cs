using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using StudioByStorm.Data;

namespace StudioByStorm {

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Singleton;
        public List<SoundFXData> soundFXData = new List<SoundFXData>();
        public List<AudioSource> audioSources = new List<AudioSource>();
        public List<AudioClip> music = new List<AudioClip>();
        public AudioSource musicAudioSource;
        public AudioSource narrationAudioSource;

        //just hold SoundType : int of corresponding index in soundFXData
        private Dictionary<SoundType, int> soundFX = new Dictionary<SoundType, int>();
        private bool isMusicOn = true;
        private bool isMusicFadingOut = false;
        private float musicFadeDuration = 7.0f;

        private bool isChangingMusic;
        private int musicChapterID;
        private float musicTargetVolume = 0.25f;
        private float soundFXMultiplier = 1.25f;

        protected void Awake()
        {
            if (Singleton == null) {
                Singleton = this;
                transform.parent = null;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }

        protected void Start()
        {
            for (int i = 0; i < soundFXData.Count; i++) {
                SoundFXData currentSoundFXData = soundFXData[i];
                soundFX.Add(currentSoundFXData.soundType, i);
            }

            int mostRecentlyPlayedChapterID = GameManager.Singleton.ProgressManager.GetMostRecentlyPlayedChapterIDProgress();
            musicChapterID = (mostRecentlyPlayedChapterID == -1) ? 0 : mostRecentlyPlayedChapterID;
            AudioClip chapterMusic = music[musicChapterID];
            StartCoroutine(UpdateCurrentPlayingMusic(chapterMusic));
            StartCoroutine(LoopMusic());
        }

        public void SetChapterMusic(int chapterID)
        {
            if (musicChapterID != chapterID) {
                musicChapterID = chapterID;
                PlayChapterMusic();
            }
        }

        public void PlayNarration(AudioClip narrationToPlay)
        {
            narrationAudioSource.clip = narrationToPlay;
            narrationAudioSource.Play();
        }

        public void StopNarration()
        {
            narrationAudioSource.Stop();
        }

        protected void PlayChapterMusic()
        {
            AudioClip chapterMusic = music[musicChapterID];
            StartCoroutine(UpdateCurrentPlayingMusic(chapterMusic));
        }

        IEnumerator UpdateCurrentPlayingMusic(AudioClip clipToPlay)
        {
            isChangingMusic = true;

            if (musicAudioSource.isPlaying) {
                FadeMusicOut();
                yield return new WaitUntil(() => musicAudioSource.volume == 0.0f);
            }
            
            musicAudioSource.clip = clipToPlay;
            FadeMusicIn();
            yield return new WaitUntil(() => musicAudioSource.volume == musicTargetVolume);
            isChangingMusic = false;
        }

        IEnumerator LoopMusic()
        {
            while(isMusicOn) {

                if (!isChangingMusic) {
                    float timeLeft = musicAudioSource.clip.length - musicAudioSource.time;

                    if (! musicAudioSource.isPlaying) {
                        FadeMusicIn();

                    } else if (!isMusicFadingOut && timeLeft <= musicFadeDuration) {
                        isMusicFadingOut = true;
                        FadeMusicOut();
                    }
                }

                yield return new WaitForSeconds(0.0333f);
            }
        }

        protected void FadeMusicIn()
        {
            musicAudioSource.volume = 0.0f;
            musicAudioSource.Play();
            musicAudioSource.DOFade(musicTargetVolume, musicFadeDuration).SetEase(Ease.Linear).OnComplete(() => {
                //we'll put this in here so there's no chance of fade out being called because the music isn't playing so technically "timeLeft <= 10.0f"
                isMusicFadingOut = false;
            }); 
        }

        protected void FadeMusicOut()
        {
            musicAudioSource.DOFade(0.0f, musicFadeDuration).SetEase(Ease.Linear); 
        }

        public void Play(SoundType soundType)
        {
            AudioSource audioSourceToUse = audioSources[(int) soundType];

            if (soundFX.ContainsKey(soundType)) {

                int index = soundFX[soundType];

                if (soundFXData[index].audioClips.Count <= 0) {
                    return;
                }

                SoundFXData currentSoundFXData = soundFXData[index];
                List<AudioClip> currentAudioClips = currentSoundFXData.audioClips;
                float targetVolume = currentSoundFXData.volume * soundFXMultiplier;
                Ease easeIn = currentSoundFXData.easingIn;
                Ease easeOut = currentSoundFXData.easingOut;
                bool doFade = currentSoundFXData.doFade;
                
                int randomAudioClipIndex = UnityEngine.Random.Range(0, currentAudioClips.Count);
                audioSourceToUse.clip = currentAudioClips[randomAudioClipIndex];
                

                float halfLifeDuration = audioSourceToUse.clip.length / 2.0f;
                
                audioSourceToUse.Stop();

                if (doFade) {
                    audioSourceToUse.volume = 0.0f;
                    audioSourceToUse.Play();
                    Sequence soundFade = DOTween.Sequence();
                    soundFade.Append(audioSourceToUse.DOFade(targetVolume, halfLifeDuration).SetEase(easeIn))
                             .Append(audioSourceToUse.DOFade(0.0f, halfLifeDuration).SetEase(easeOut));
                } else {
                    audioSourceToUse.volume = targetVolume;
                    audioSourceToUse.Play();
                }
                
            }
        }
    }

    public enum SoundType
    {
        GameStart,//
        LevelComplete,//
        LevelLost,//
        ChapterIntro,
        ColoredRingsAdded,//
        GetEdge,//
        SetEdge,//
        TravelEdgeIndicator,//
        Travel,//
        EnergyTravel,//
        Jump,//
        CellLand,//
        WrongObstacleHit,//
        CorrectObstacleHit,//
        Fireworks,
        Dash,//
        Typing,
        ButtonPress,
        CantSetEdge,
        StarAwarded,
        Applause,
    }
}