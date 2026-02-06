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
            if (!m_audioSources.ContainsKey(name) || m_audioSources[name].clip == null)
            {
                CreateSource(name, isLoop);

                YooAssetManager.Instance.AsyncLoadAsset<AudioClip>("Audios_" + name, (clip) =>
                {
                    m_audioSources[name].clip ??= clip;

                    if (m_audioSources[name].isPlaying)
                    {
                        m_audioSources[name].Stop();
                    }

                    m_audioSources[name].Play();
                });
            }
            else
            {
                m_audioSources[name].Stop();
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

        private void CreateSource(string name, bool isLoop)
        {
            AudioSource audioSource;

            if (!m_audioSources.ContainsKey(name))
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(transform);
                audioSource = go.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                m_audioSources.Add(name, audioSource);
            }
            else
            {
                audioSource = m_audioSources[name];
            }

            audioSource.loop = isLoop;
        }
    }
}