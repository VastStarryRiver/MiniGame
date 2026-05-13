using System.Collections.Generic;
using UnityEngine;



namespace Invariable
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        private Dictionary<string, AudioSource> m_audioSources;



        private void Awake()
        {
            m_audioSources = new Dictionary<string, AudioSource>();
            Instance = this;
        }



        public void PlayAudio(string name, bool isLoop = false)
        {
            if (!m_audioSources.ContainsKey(name))
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(transform);
                AudioSource audioSource = go.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                m_audioSources.Add(name, audioSource);

                audioSource.loop = isLoop;

                YooAssetManager.Instance.AsyncLoadAsset<AudioClip>("Audios_" + name, (clip) =>
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                });
            }
            else if (m_audioSources[name].clip != null && !m_audioSources[name].isPlaying)
            {
                m_audioSources[name].loop = isLoop;
                m_audioSources[name].Play();
            }
        }

        public void StopAudio(string name = "")
        {
            if (string.IsNullOrEmpty(name))
            {
                foreach (var item in m_audioSources)
                {
                    item.Value.Stop();
                }
            }
            else if (m_audioSources.ContainsKey(name))
            {
                m_audioSources[name].Stop();
            }
        }

        public void PauseAudio(string name = "")
        {
            if (string.IsNullOrEmpty(name))
            {
                foreach (var item in m_audioSources)
                {
                    item.Value.Pause();
                }
            }
            else if (m_audioSources.ContainsKey(name))
            {
                m_audioSources[name].Pause();
            }
        }
    }
}