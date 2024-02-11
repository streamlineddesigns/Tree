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
        public AudioSource musicAudioSource;

        //just hold SoundType : int of corresponding index in soundFXData
        private Dictionary<SoundType, int> soundFX = new Dictionary<SoundType, int>();

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
        }

        public void Play(SoundType soundType)
        {
            AudioSource audioSourceToUse = audioSources.Where(x => !x.isPlaying).First();

            if (soundFX.ContainsKey(soundType)) {

                int index = soundFX[soundType];

                if (soundFXData[index].audioClips.Count <= 0) {
                    return;
                }

                SoundFXData currentSoundFXData = soundFXData[index];
                List<AudioClip> currentAudioClips = currentSoundFXData.audioClips;
                float targetVolume = currentSoundFXData.volume;
                Ease easeIn = currentSoundFXData.easingIn;
                Ease easeOut = currentSoundFXData.easingOut;
                bool doFade = currentSoundFXData.doFade;
                
                int randomAudioClipIndex = UnityEngine.Random.Range(0, currentAudioClips.Count);
                audioSourceToUse.clip = currentAudioClips[randomAudioClipIndex];
                

                float duration = audioSourceToUse.clip.length / 2.0f;
                
                if (doFade) {
                    audioSourceToUse.volume = 0.0f;
                    audioSourceToUse.Play();
                    Sequence soundFade = DOTween.Sequence();
                    soundFade.Append(audioSourceToUse.DOFade(targetVolume, duration).SetEase(easeIn))
                             .Append(audioSourceToUse.DOFade(0.0f, duration).SetEase(easeOut));
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
    }
}