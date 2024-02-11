using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StudioByStorm.Data {

    [System.Serializable]
    public class SoundFXData
    {
        public SoundType soundType;
        public List<AudioClip> audioClips;
        public float volume;
        public bool doFade = true;
        public Ease easingIn = Ease.InSine;
        public Ease easingOut = Ease.InSine;
    }

}