using System;
using UnityEngine;

namespace WhackAMole
{
    [Serializable]
    public class BGMData
    {
        public BGM name;
        public AudioClip clip;
    }

    [Serializable]
    public class SFXData
    {
        public SFX name;
        public AudioClip clip;
    }

    [CreateAssetMenu(fileName = "AudioSO", menuName = "SO/Audio")]
    public class AudioSO : ScriptableObject
    {
        public BGMData[] bgmDatas;
        public SFXData[] sfxDatas;

        public void GatherAssets()
        {
            string dirBGM = "Audio/BGM/";
            string dirSFX = "Audio/SFX/";

            int count = (int)BGM.COUNT;
            bgmDatas = new BGMData[count];

            AudioClip clip;
            BGMData bgmData = null;

            for (int i = 0; i < count; i++)
            {
                BGM bgm = (BGM)i;
                string name = bgm.ToString();
                string path = dirBGM + name;
                clip = Resources.Load<AudioClip>(path);

                bgmData = new BGMData();
                bgmData.name = bgm;
                bgmData.clip = clip;

                bgmDatas[i] = bgmData;
            }

            count = (int)SFX.COUNT;
            sfxDatas = new SFXData[count];

            SFXData sfxData = null;

            for (int i = 0; i < count; i++)
            {
                SFX sfx = (SFX)i;
                string name = sfx.ToString();
                string path = dirSFX + name;
                clip = Resources.Load<AudioClip>(path);

                sfxData = new SFXData();
                sfxData.name = sfx;
                sfxData.clip = clip;

                sfxDatas[i] = sfxData;
            }
        }
    }
}
