using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum CollectableType
{
    Timer
}

public interface ICollectable
{
    object GetCollectableInfo(out CollectableType collectableType);
}

public class CollectableTrigger : MonoBehaviour, ICollectable
{
    [SerializeField]
    private CollectableType m_collectableType;
    [SerializeField, Tooltip("Time to add to the player accumulated time.")]
    private float m_timeToAdd;
    [SerializeField]
    private TextMeshPro m_TimerText;

    private void Awake()
    {
        m_TimerText.SetText( (m_timeToAdd >= 0 ? "+" : "-") + m_timeToAdd.ToString("F0"));
    }

    public object GetCollectableInfo(out CollectableType collectableType)
    {
        collectableType = m_collectableType;
        gameObject.SetActive(false);
        return m_timeToAdd;
    }

    private IEnumerator EnableBack()
    {
        yield return new WaitForSeconds(1);

        gameObject.SetActive(true);
    }
}
