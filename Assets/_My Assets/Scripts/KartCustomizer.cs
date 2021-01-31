using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartCustomizer : MonoBehaviour
{
    [Header("Wheels")]
    [SerializeField]
    private List<Transform> m_wheelPivots;
    [SerializeField]
    private List<GameObject> m_wheelPrefabs;
    [SerializeField]
    private List<GameObject> m_currentWheels;

    [Header("Hat")]
    [SerializeField]
    private Transform m_headPivot;
    [SerializeField]
    private List<GameObject> m_hatPrefabs;
    [SerializeField, Space()]
    private GameObject m_currentHat;

    [Header("Materials")]
    [SerializeField]
    private Renderer m_kartRenderer;
    [SerializeField]
    private Renderer m_characterRenderer;
    [SerializeField]
    private List<Material> m_kartMaterials;
    [SerializeField]
    private List<Material> m_charMaterials;

    private int wheelsCount => m_wheelPivots.Count;

    /// <summary>
    /// Change wheels (visual).
    /// </summary>
    public void ChangeWheels(int index)
    {
        for (int i = 0; i < wheelsCount; i++)
        {
            // Destroy previous wheels
            Destroy(m_currentWheels[i]);

            // Instantiate new wheels
            m_currentWheels.Add(Instantiate(m_wheelPrefabs[index], m_wheelPivots[i]));
        }
    }

    /// <summary>
    /// Change player character (visual).
    /// </summary>
    public void ChangeCharacterHat(int index)
    {
        // Destroy previous hat
        if(m_currentHat != null)
        {
            Destroy(m_currentHat);
        }

        m_currentHat = Instantiate(m_hatPrefabs[index], m_headPivot);
    }

    /// <summary>
    /// Change kart and character visuals.
    /// </summary>
    public void ChangeMaterials(int bodyIndex, int charIndex)
    {
        m_kartRenderer.sharedMaterial = m_kartMaterials[bodyIndex];
        m_characterRenderer.sharedMaterial = m_charMaterials[charIndex];
    }
}
