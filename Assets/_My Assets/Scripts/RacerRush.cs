using UnityEngine;
using System;
using System.Collections.Generic;
using KartGame.Track;

public class RacerRush : Racer
{
    [SerializeField]
    private List<TrailRenderer> m_boostTrail;

    private float m_countdownTimer;

    private Action<float> onUpdateCountdown;

    protected override void Update()
    {
        base.Update();

        if (m_countdownTimerEnabled && IsControlled())
        {
            m_countdownTimer -= Time.deltaTime;

            if (m_countdownTimer <= 0)
            {
                m_countdownTimer = 0;
                DisableControl();
            }

            onUpdateCountdown?.Invoke(m_countdownTimer);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for collectable items
        if (other.TryGetComponent(out ICollectable collectable))
        {
            CollectableType collectableType;
            var info = collectable.GetCollectableInfo(out collectableType);

            switch (collectableType)
            {
                case CollectableType.Timer:
                {
                    AddTime((float)info);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Add time to countdown timer.
    /// </summary>
    public void AddTime(float timeToAdd)
    {
        m_countdownTimer += timeToAdd;
        onUpdateCountdown?.Invoke(m_countdownTimer);
    }

    /// <summary>
    /// IRushable: Set initial time for countdown timer.
    /// </summary>
    public override void SetCountdownTimer(float time, Action<float> onUpdateCountdown)
    {
        m_countdownTimerEnabled = true;
        m_countdownTimer = time;
        this.onUpdateCountdown = onUpdateCountdown;
        this.onUpdateCountdown?.Invoke(m_countdownTimer);
    }

    public override void ResetRacerInfo()
    {
        base.ResetRacerInfo();

        m_countdownTimer = 0;
    }

    public void EnableBoostTrail(bool enable)
    {
        for (int i = 0; i < m_boostTrail.Count; i++)
        {
            m_boostTrail[i].emitting = enable;
        }
    }
}
