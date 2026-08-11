using System.Collections.Generic;
using System.Globalization;
using UnityEngine;



namespace Invariable
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager m_instance = null;

        private AudioSource m_bgmSource = null;
        private string m_currentBgmName = null;
        private bool m_bgmLoading = false;
        private string m_pendingBgmName = null;
        private Dictionary<string, AudioSource> m_sfxSources = null;
        private HashSet<string> m_sfxLoading = null;
        private float m_masterVolume = 1f;
        private float m_bgmVolume = 1f;
        private float m_sfxVolume = 1f;
        private bool m_mute = false;

        /// <summary>
        /// 实例是否存在（判空检查用，不触发错误日志）
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
            m_sfxSources = new Dictionary<string, AudioSource>();
            m_sfxLoading = new HashSet<string>();
            LoadVolumeSettings();
        }

        private void OnDestroy()
        {
            if (m_instance == this)
            {
                m_instance = null;
            }
        }



        /// <summary>
        /// 播放背景音乐（单通道循环，切歌自动停上一首）
        /// </summary>
        public void PlayBGM(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (m_currentBgmName == name && m_bgmSource != null && m_bgmSource.isPlaying)
            {
                return;
            }

            if (m_bgmLoading)
            {
                m_pendingBgmName = name;

                return;
            }

            EnsureBgmSource();

            if (m_bgmSource.clip != null && m_currentBgmName == name)
            {
                ApplyBgmVolume();
                m_bgmSource.Play();

                return;
            }

            m_bgmLoading = true;
            m_pendingBgmName = null;
            string requestName = name;

            YooAssetManager.Instance.AsyncLoadAsset<AudioClip>("Audios_" + name, (clip) =>
            {
                m_bgmLoading = false;

                if (!string.IsNullOrEmpty(m_pendingBgmName) && m_pendingBgmName != requestName)
                {
                    string next = m_pendingBgmName;
                    m_pendingBgmName = null;
                    PlayBGM(next);

                    return;
                }

                m_pendingBgmName = null;

                if (clip == null)
                {
                    GameLog.Error($"PlayBGM 加载失败: {requestName}");

                    return;
                }

                EnsureBgmSource();
                m_bgmSource.Stop();
                m_bgmSource.clip = clip;
                m_bgmSource.loop = true;
                m_currentBgmName = requestName;
                ApplyBgmVolume();
                m_bgmSource.Play();
            });
        }

        /// <summary>
        /// 播放音效（同名打断重播）
        /// </summary>
        public void PlaySFX(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (m_sfxSources.TryGetValue(name, out AudioSource source) && source.clip != null)
            {
                ApplySfxVolume(source);
                source.Stop();
                source.Play();

                return;
            }

            if (m_sfxLoading.Contains(name))
            {
                return;
            }

            if (source == null)
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(transform);
                source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                m_sfxSources.Add(name, source);
            }

            m_sfxLoading.Add(name);

            YooAssetManager.Instance.AsyncLoadAsset<AudioClip>("Audios_" + name, (clip) =>
            {
                m_sfxLoading.Remove(name);

                if (clip == null)
                {
                    GameLog.Error($"PlaySFX 加载失败: {name}");

                    return;
                }

                if (!m_sfxSources.TryGetValue(name, out AudioSource loadedSource))
                {
                    return;
                }

                loadedSource.clip = clip;
                ApplySfxVolume(loadedSource);
                loadedSource.Stop();
                loadedSource.Play();
            });
        }

        /// <summary>
        /// 停止指定或全部音频
        /// </summary>
        public void StopAudio(string name = "")
        {
            if (string.IsNullOrEmpty(name))
            {
                if (m_bgmSource != null)
                {
                    m_bgmSource.Stop();
                }

                foreach (KeyValuePair<string, AudioSource> item in m_sfxSources)
                {
                    item.Value.Stop();
                }

                return;
            }

            if (m_currentBgmName == name && m_bgmSource != null)
            {
                m_bgmSource.Stop();

                return;
            }

            if (m_sfxSources.TryGetValue(name, out AudioSource source))
            {
                source.Stop();
            }
        }

        /// <summary>
        /// 暂停指定或全部音频
        /// </summary>
        public void PauseAudio(string name = "")
        {
            if (string.IsNullOrEmpty(name))
            {
                if (m_bgmSource != null)
                {
                    m_bgmSource.Pause();
                }

                foreach (KeyValuePair<string, AudioSource> item in m_sfxSources)
                {
                    item.Value.Pause();
                }

                return;
            }

            if (m_currentBgmName == name && m_bgmSource != null)
            {
                m_bgmSource.Pause();

                return;
            }

            if (m_sfxSources.TryGetValue(name, out AudioSource source))
            {
                source.Pause();
            }
        }

        /// <summary>
        /// 设置主音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            m_masterVolume = Mathf.Clamp01(volume);
            ApplyAllVolumes();
            SaveVolumeSettings();
        }

        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            m_bgmVolume = Mathf.Clamp01(volume);
            ApplyBgmVolume();
            SaveVolumeSettings();
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            m_sfxVolume = Mathf.Clamp01(volume);
            ApplyAllSfxVolumes();
            SaveVolumeSettings();
        }

        /// <summary>
        /// 设置静音
        /// </summary>
        public void SetMute(bool mute)
        {
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
            foreach (KeyValuePair<string, AudioSource> item in m_sfxSources)
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