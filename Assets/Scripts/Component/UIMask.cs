using UnityEngine;
using UnityEngine.UI;



public class UIMask : MonoBehaviour
{
    private Image[] m_image;
    private ParticleSystem[] m_particleSystem;
    private MeshRenderer[] m_meshRenderer;



    public void ShowMask()
    {
        m_image = GetComponentsInChildren<Image>();
        m_particleSystem = GetComponentsInChildren<ParticleSystem>();
        m_meshRenderer = GetComponentsInChildren<MeshRenderer>();

        m_image[0].material = AssetsManager.Instance.m_materials[0];

        Material material2 = AssetsManager.Instance.m_materials[1];

        if (m_image.Length > 1)
        {
            for (int i = 1; i < m_image.Length; i++)
            {
                m_image[i].material = material2;
            }
        }

        if (m_particleSystem.Length > 0)
        {
            for (int i = 0; i < m_particleSystem.Length; i++)
            {
                m_particleSystem[i].GetComponent<ParticleSystemRenderer>().material = material2;
            }
        }

        if (m_meshRenderer.Length > 0)
        {
            for (int i = 0; i < m_meshRenderer.Length; i++)
            {
                m_meshRenderer[i].material = material2;
            }
        }
    }
}