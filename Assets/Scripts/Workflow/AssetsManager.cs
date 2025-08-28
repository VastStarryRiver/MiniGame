using System.Collections.Generic;
using UnityEngine;



public class AssetsManager : MonoBehaviour
{
    public static AssetsManager Instance = null;

    public List<Sprite> m_images = new List<Sprite>();
    public List<AudioClip> m_clips = new List<AudioClip>();
    public List<GameObject> m_gameObjects = new List<GameObject>();
    public List<Material> m_materials = new List<Material>();
    public List<GameObject> m_effects = new List<GameObject>();



    private void Awake()
    {
        Instance = this;
    }
}