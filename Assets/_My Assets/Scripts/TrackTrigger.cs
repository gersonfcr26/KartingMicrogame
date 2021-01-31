using KartGame.KartSystems;
using KartGame.Track;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerType
{
    Boost,
    NoControl,
    Ramp
}

public class TrackTrigger : MonoBehaviour
{
    [SerializeField]
    private TriggerType m_triggerType;
    [SerializeField]
    private float m_duration;

    [Header("Boost")]
    [SerializeField]
    private float m_boostToAdd = 5;

    [Header("Ramp")]
    [SerializeField]
    private Animator m_rampAnimator;

    // Internal
    private RacerRush m_racerRush;
    private KartMovement m_kartMovement;
    private MultiplicativeKartModifier m_boostStats;

    private void Awake()
    {
        m_boostStats = new MultiplicativeKartModifier();
        m_boostStats.modifiers.acceleration = m_boostToAdd * 5;
        m_boostStats.modifiers.topSpeed = m_boostToAdd;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for collectable items
        if (other.TryGetComponent(out IRacer racer))
        {
            // Apply effect
            switch (m_triggerType)
            {
                case TriggerType.Boost:
                {
                    m_kartMovement = other.GetComponent<KartMovement>();
                    m_kartMovement.AddKartModifier(m_boostStats);
                    m_racerRush = other.GetComponent<RacerRush>();
                    if (m_racerRush != null)
                    {
                        m_racerRush.EnableBoostTrail(true);
                        StartCoroutine(WaitForTrails());
                    }
                    break;
                }
                case TriggerType.NoControl:
                {
                    racer.DisableControl();
                    break;
                }
                case TriggerType.Ramp:
                {
                    m_rampAnimator.SetBool("ramp", true);
                    break;
                }
            }

            StartCoroutine(WaitForEffect(other, racer));
        }
    }

    private IEnumerator WaitForEffect(Collider other, IRacer racer)
    {
        yield return new WaitForSeconds(m_duration);

        // Disable effect after duration
        switch (m_triggerType)
        {
            case TriggerType.Boost:
            {
                m_kartMovement.RemoveKartModifier(m_boostStats);
                m_kartMovement = null;
                break;
            }
            case TriggerType.NoControl:
            {
                racer.EnableControl();
                break;
            }
            case TriggerType.Ramp:
            {
                m_rampAnimator.SetBool("ramp", false);
                break;
            }
        }
    }

    private IEnumerator WaitForTrails()
    {
        yield return new WaitForSeconds(1);

        m_racerRush.EnableBoostTrail(false);
        m_racerRush = null;
    }
}
