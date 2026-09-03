using System.Collections.Generic;
using System.Globalization;
using UnityEngine;



namespace Invariable
{
    public class AudioManager : MonoBehaviour
    {
        private const int MaxSfxChannels = 30;

        private static AudioManager m_instance = null;

        private AudioSource m_bgmSource = null;
        private Dictionary<AudioClip, AudioSource> m_sfxSources = null;
        private List<AudioClip> m_sfxLruList = null;
        private float m_masterVolume = 1f;
        private float m_bgmVolume = 1f;
        private float m_sfxVolume = 1f;
        private bool m_mute = false;
        private bool m_volumeSettingsLoaded = false;

        /// <summary>
        /// 实例是否存在
        /// </summary>
        public static bool HasInstance
        {
            get
            {
                return m_instance != null;
            }
        }

        public static AudioManager Instance
        {
            get
            {
                if (!HasInstance)
                {
                    GameLog.Error("AudioManager实例对象为空");
                }

                return m_instance;
            }
        }



        private void Awake()
        {
            m_instance = this;
            m_sfxSources = new Dictionary<AudioClip, AudioSource>();
            m_sfxLruList = new List<AudioClip>();
        }

        private void OnDestroy()
        {
            if (m_instance == this)
            {
                m_instance = null;
            }
        }



        /// <summary>
        /// 播放背景音乐（挂载 clip，单通道循环，切歌自动停上一首）
        /// </summary>
        public void PlayBGM(AudioClip clip)
        {
            EnsureVolumeSettingsLoaded();

            if (clip == null)
            {
                return;
            }

            EnsureBgmSource();

            if (m_bgmSource.clip == clip && m_bgmSource.isPlaying)
            {
                return;
            }

            if (m_bgmSource.clip == clip)
            {
                ApplyBgmVolume();
                m_bgmSource.Play();

                return;
            }

            m_bgmSource.Stop();
            m_bgmSource.clip = clip;
            m_bgmSource.loop = true;
            ApplyBgmVolume();
            m_bgmSource.Play();
        }

        /// <summary>
        /// 播放音效（挂载 clip，每 clip 独立通道，同 clip 打断重播）
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            EnsureVolumeSettingsLoaded();

            if (clip == null)
            {
                return;
            }

            AudioSource source = EnsureSfxSource(clip);

            if (source == null)
            {
                return;
            }

            source.clip = clip;
            source.loop = false;
            ApplySfxVolume(source);
            source.Stop();
            source.Play();
        }

        /// <summary>
        /// 停止当前背景音乐
        /// </summary>
        public void StopBGM()
        {
            if (m_bgmSource != null)
            {
                m_bgmSource.Stop();
            }
        }

        /// <summary>
        /// 暂停当前背景音乐
        /// </summary>
        public void PauseBGM()
        {
            if (m_bgmSource != null)
            {
                m_bgmSource.Pause();
            }
        }

        /// <summary>
        /// 恢复当前背景音乐
        /// </summary>
        public void ResumeBGM()
        {
            if (m_bgmSource != null)
            {
                m_bgmSource.UnPause();
            }
        }

        /// <summary>
        /// 停止指定 clip 的音效
        /// </summary>
        public void StopSFX(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            if (m_sfxSources.TryGetValue(clip, out AudioSource source))
            {
                source.Stop();
            }
        }

        /// <summary>
        /// 暂停指定 clip 的音效
        /// </summary>
        public void PauseSFX(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            if (m_sfxSources.TryGetValue(clip, out AudioSource source))
            {
                source.Pause();
            }
        }

        /// <summary>
        /// 恢复指定 clip 的音效
        /// </summary>
        public void ResumeSFX(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            if (m_sfxSources.TryGetValue(clip, out AudioSource source))
            {
                source.UnPause();
            }
        }

        /// <summary>
        /// 停止全部音频
        /// </summary>
        public void StopAllAudio()
        {
            StopBGM();

            foreach (KeyValuePair<AudioClip, AudioSource> item in m_sfxSources)
            {
                item.Value.Stop();
            }
        }

        /// <summary>
        /// 暂停全部音频
        /// </summary>
        public void PauseAllAudio()
        {
            PauseBGM();

            foreach (KeyValuePair<AudioClip, AudioSource> item in m_sfxSources)
            {
                item.Value.Pause();
            }
        }

        /// <summary>
        /// 恢复全部音频
        /// </summary>
        public void ResumeAllAudio()
        {
            ResumeBGM();

            foreach (KeyValuePair<AudioClip, AudioSource> item in m_sfxSources)
            {
                item.Value.UnPause();
            }
        }

        /// <summary>
        /// 设置主音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            EnsureVolumeSettingsLoaded();
            m_masterVolume = Mathf.Clamp01(volume);
            ApplyAllVolumes();
            SaveVolumeSettings();
        }

        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            EnsureVolumeSettingsLoaded();
            m_bgmVolume = Mathf.Clamp01(volume);
            ApplyBgmVolume();
            SaveVolumeSettings();
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            EnsureVolumeSettingsLoaded();
            m_sfxVolume = Mathf.Clamp01(volume);
            ApplyAllSfxVolumes();
            SaveVolumeSettings();
        }

        /// <summary>
        /// 设置静音
        /// </summary>
        public void SetMute(bool mute)
        {
            EnsureVolumeSettingsLoaded();
            m_mute = mute;
            ApplyAllVolumes();
            SaveVolumeSettings();
        }

        /// <summary>
        /// 确保 BGM AudioSource 已创建
        /// </summary>
        private void EnsureBgmSource()
        {
            if (m_bgmSource != null)
            {
                return;
            }

            GameObject go = new GameObject("BGM");
            go.transform.SetParent(transform);
            m_bgmSource = go.AddComponent<AudioSource>();
            m_bgmSource.playOnAwake = false;
            m_bgmSource.loop = true;
        }

        /// <summary>
        /// 确保指定 clip 的音效 AudioSource 已创建，达到上限且无空闲通道时返回 null
        /// </summary>
        private AudioSource EnsureSfxSource(AudioClip clip)
        {
            if (m_sfxSources.TryGetValue(clip, out AudioSource source))
            {
                m_sfxLruList.Remove(clip);
                m_sfxLruList.Add(clip);

                return source;
            }

            if (m_sfxSources.Count >= MaxSfxChannels)
            {
                AudioClip evicted = FindIdleSfxClip();

                if (evicted == null)
                {
                    GameLog.Error($"AudioManager SFX 通道已达上限 {MaxSfxChannels} 且全部正在播放: {clip.name}");

                    return null;
                }

                DestroySfxChannel(evicted);
            }

            GameObject go = new GameObject("SFX_" + clip.name);
            go.transform.SetParent(transform);
            source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.clip = clip;
            m_sfxSources.Add(clip, source);
            m_sfxLruList.Add(clip);

            return source;
        }

        /// <summary>
        /// 找最久未用的空闲音效通道，暂停视为空闲，无空闲返回 null
        /// </summary>
        private AudioClip FindIdleSfxClip()
        {
            for (int i = 0; i < m_sfxLruList.Count; i++)
            {
                AudioClip clip = m_sfxLruList[i];

                if (!m_sfxSources.TryGetValue(clip, out AudioSource source) || source == null || !source.isPlaying)
                {
                    return clip;
                }
            }

            return null;
        }

        /// <summary>
        /// 销毁指定 clip 的音效通道并移出管理
        /// </summary>
        private void DestroySfxChannel(AudioClip clip)
        {
            if (m_sfxSources.TryGetValue(clip, out AudioSource source))
            {
                m_sfxSources.Remove(clip);

                if (source != null)
                {
                    UnityEngine.Object.Destroy(source.gameObject);
                }
            }

            m_sfxLruList.Remove(clip);
        }

        /// <summary>
        /// 应用 BGM 音量
        /// </summary>
        private void ApplyBgmVolume()
        {
            if (m_bgmSource == null)
            {
                return;
            }

            m_bgmSource.volume = m_mute ? 0f : m_masterVolume * m_bgmVolume;
        }

        /// <summary>
        /// 应用指定音效音量
        /// </summary>
        private void ApplySfxVolume(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.volume = m_mute ? 0f : m_masterVolume * m_sfxVolume;
        }

        /// <summary>
        /// 应用全部音效音量
        /// </summary>
        private void ApplyAllSfxVolumes()
        {
            foreach (KeyValuePair<AudioClip, AudioSource> item in m_sfxSources)
            {
                ApplySfxVolume(item.Value);
            }
        }

        /// <summary>
        /// 应用全部通道音量
        /// </summary>
        private void ApplyAllVolumes()
        {
            ApplyBgmVolume();
            ApplyAllSfxVolumes();
        }

        /// <summary>
        /// 首次使用时从本地存储加载音量设置
        /// </summary>
        private void EnsureVolumeSettingsLoaded()
        {
            if (m_volumeSettingsLoaded)
            {
                return;
            }

            m_volumeSettingsLoaded = true;
            LoadVolumeSettings();
        }

        /// <summary>
        /// 从本地存储加载音量设置
        /// </summary>
        private void LoadVolumeSettings()
        {
            m_masterVolume = ParseVolume(SdkManager.Instance.GetLocalData(InvariableConst.LocalKey_AudioMasterVolume, "1"), 1f);
            m_bgmVolume = ParseVolume(SdkManager.Instance.GetLocalData(InvariableConst.LocalKey_AudioBgmVolume, "1"), 1f);
            m_sfxVolume = ParseVolume(SdkManager.Instance.GetLocalData(InvariableConst.LocalKey_AudioSfxVolume, "1"), 1f);
            m_mute = SdkManager.Instance.GetLocalData(InvariableConst.LocalKey_AudioMute, "0") == "1";
        }

        /// <summary>
        /// 将音量设置写入本地存储
        /// </summary>
        private void SaveVolumeSettings()
        {
            SdkManager.Instance.SetLocalData(InvariableConst.LocalKey_AudioMasterVolume, m_masterVolume.ToString());
            SdkManager.Instance.SetLocalData(InvariableConst.LocalKey_AudioBgmVolume, m_bgmVolume.ToString());
            SdkManager.Instance.SetLocalData(InvariableConst.LocalKey_AudioSfxVolume, m_sfxVolume.ToString());
            SdkManager.Instance.SetLocalData(InvariableConst.LocalKey_AudioMute, m_mute ? "1" : "0");
        }

        /// <summary>
        /// 解析音量字符串
        /// </summary>
        private float ParseVolume(string value, float defaultValue)
        {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float volume))
            {
                return Mathf.Clamp01(volume);
            }

            return defaultValue;
        }
    }
}