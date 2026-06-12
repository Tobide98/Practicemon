using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WhackAMole
{
    //READ ME, add BGM & SFX enum if you add new sfx files. format: "SFX_[SfxName]"
    public enum BGM
    {
        BGM_MainMenu,
        BGM_Gameplay,
        COUNT
    }

    public enum SFX
    {
        SFX_Hit,
        SFX_NegativeClick,
        SFX_Point,
        SFX_PositiveClick,
        SFX_Correct,
        SFX_Wrong,
        SFX_Win,
        SFX_Lose,
        SFX_Show,
        COUNT
    }

    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] AudioSO audioSO;
        [SerializeField] private float bgmVolume = 0.5f;
        [SerializeField] private float bgmFadeDuration = 1f;

        Dictionary<BGM, AudioClip> bgmDict;
        Dictionary<SFX, AudioClip> sfxDict;

        GameManager gameManager;

        Coroutine bgmCoroutine;
        BGM currentBGM;
        bool hasCurrentBGM;
        bool initialized;

        public void Init(GameManager inGameManager)
        {
            gameManager = inGameManager;   

            bgmDict = new Dictionary<BGM, AudioClip>();
            sfxDict = new Dictionary<SFX, AudioClip>();

            if (audioSO == null)
            {
                Debug.LogWarning($"{nameof(AudioManager)} needs an {nameof(AudioSO)} reference.", this);
                initialized = false;
                return;
            }

            BGMData[] bgmDatas = audioSO.bgmDatas;
            SFXData[] sfxDatas = audioSO.sfxDatas;

            int count = bgmDatas != null ? bgmDatas.Length : 0;
            for (int i = 0; i < count; i++)
            {
                BGMData data = bgmDatas[i];
                if (data == null || data.clip == null)
                {
                    continue;
                }

                bool hasKey = bgmDict.ContainsKey(data.name);
                if (!hasKey)
                {
                    bgmDict.Add(data.name, data.clip);
                }
            }

            count = sfxDatas != null ? sfxDatas.Length : 0;
            for (int i = 0; i < count; i++)
            {
                SFXData data = sfxDatas[i];
                if (data == null || data.clip == null)
                {
                    continue;
                }

                bool hasKey = sfxDict.ContainsKey(data.name);
                if (!hasKey)
                {
                    sfxDict.Add(data.name, data.clip);
                }
            }

            if (sfxSource == null)
            {
                Debug.LogWarning($"{nameof(AudioManager)} needs an SFX {nameof(AudioSource)} reference.", this);
                initialized = false;
                return;
            }

            if (bgmSource == null)
            {
                Debug.LogWarning($"{nameof(AudioManager)} needs a BGM {nameof(AudioSource)} reference.", this);
                initialized = false;
                return;
            }

            sfxSource.loop = false;
            sfxSource.playOnAwake = false;

            bgmSource.loop = false;
            bgmSource.playOnAwake = false;
            bgmSource.volume = 0f;

            initialized = true;
        }

        public void PlayBGM(BGM bgm)
        {
            if (!initialized || bgmDict == null)
            {
                Debug.LogWarning($"{nameof(AudioManager)} has not been initialized.", this);
                return;
            }

            if (!bgmDict.TryGetValue(bgm, out AudioClip toPlay))
            {
                Debug.LogWarning($"{nameof(AudioManager)} could not find BGM clip {bgm}.", this);
                return;
            }

            if (hasCurrentBGM && currentBGM == bgm && bgmSource.isPlaying)
            {
                return;
            }

            if (bgmCoroutine != null)
            {
                StopCoroutine(bgmCoroutine);
            }

            currentBGM = bgm;
            hasCurrentBGM = true;
            bgmCoroutine = StartCoroutine(LoopBGMWithFade(toPlay));
        }

        IEnumerator LoopBGMWithFade(AudioClip clip)
        {
            if (bgmSource.isPlaying)
            {
                yield return FadeBGMVolume(bgmSource.volume, 0f, bgmFadeDuration);
                bgmSource.Stop();
            }

            while (true)
            {
                bgmSource.clip = clip;
                bgmSource.volume = 0f;
                bgmSource.time = 0f;
                bgmSource.Play();

                yield return FadeBGMVolume(0f, bgmVolume, bgmFadeDuration);

                float fadeOutStartTime = Mathf.Max(0f, clip.length - bgmFadeDuration);
                while (bgmSource.isPlaying && bgmSource.time < fadeOutStartTime)
                {
                    yield return null;
                }

                yield return FadeBGMVolume(bgmSource.volume, 0f, bgmFadeDuration);
                bgmSource.Stop();
            }
        }

        IEnumerator FadeBGMVolume(float startVolume, float endVolume, float duration)
        {
            if (duration <= 0f)
            {
                bgmSource.volume = endVolume;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, endVolume, elapsed / duration);
                yield return null;
            }

            bgmSource.volume = endVolume;
        }

        public void PlaySFX(SFX sfx)
        {
            if (!initialized || sfxDict == null)
            {
                Debug.LogWarning($"{nameof(AudioManager)} has not been initialized.", this);
                return;
            }

            if (!sfxDict.TryGetValue(sfx, out AudioClip toPlay))
            {
                Debug.LogWarning($"{nameof(AudioManager)} could not find SFX clip {sfx}.", this);
                return;
            }

            sfxSource.PlayOneShot(toPlay);
        }

        public void StopBGM()
        {
            if (bgmCoroutine != null)
            {
                StopCoroutine(bgmCoroutine);
                bgmCoroutine = null;
            }

            if (initialized && bgmSource != null && bgmSource.isPlaying)
            {
                StartCoroutine(StopBGMWithFade());
            }
        }

        IEnumerator StopBGMWithFade()
        {
            yield return FadeBGMVolume(bgmSource.volume, 0f, bgmFadeDuration);
            bgmSource.Stop();
        }

        public void StopSFX()
        {
            sfxSource.Stop();
        }
    }
}
