using System.Collections.Generic;
using KartGame.KartSystems;
using UnityEngine;

namespace KartGame.Track
{
    /// <summary>
    /// The default implementation of the IRacer interface.  This is a representation of all the timing information for a particular kart as it goes through a race.
    /// </summary>
    public class Racer : MonoBehaviour, IRacer
    {
        [Tooltip ("A reference to the IControllable for the kart.  Normally this is the KartMovement script.")]
        [SerializeField, RequireInterface (typeof(IControllable))]
        protected Object m_kartMovementObject;

        // Internal
        protected IControllable m_KartMovement;
        protected bool m_IsTimerPaused = true;
        protected float m_Timer = 0f;
        protected int m_CurrentLap = 0;
        protected List<float> m_LapTimes = new List<float> (9);

        protected bool m_countdownTimerEnabled;

        protected virtual void Awake ()
        {
            m_KartMovement = m_kartMovementObject as IControllable;
        }

        protected virtual void Update ()
        {
            if (m_CurrentLap > 0 && !m_IsTimerPaused)
            {
                m_Timer += Time.deltaTime;
            }
        }

        public virtual void PauseTimer ()
        {
            m_IsTimerPaused = true;
        }

        public virtual void UnpauseTimer ()
        {
            m_IsTimerPaused = false;
        }

        public virtual void HitStartFinishLine ()
        {
            if (m_CurrentLap > 0)
            {
                m_LapTimes.Add (m_Timer);
                m_Timer = 0f;
            }

            m_CurrentLap++;
        }

        public virtual int GetCurrentLap ()
        {
            return m_CurrentLap;
        }

        public virtual List<float> GetLapTimes ()
        {
            return m_LapTimes;
        }

        public virtual float GetLapTime ()
        {
            return m_Timer;
        }

        public virtual float GetRaceTime ()
        {
            float raceTime = m_Timer;
            for (int i = 0; i < m_LapTimes.Count; i++)
            {
                raceTime += m_LapTimes[i];
            }

            return raceTime;
        }

        public virtual void EnableControl ()
        {
            m_KartMovement.EnableControl ();
        }

        public virtual void DisableControl ()
        {
            m_KartMovement.DisableControl ();
        }

        public virtual bool IsControlled ()
        {
            return m_KartMovement.IsControlled ();
        }

        public virtual string GetName ()
        {
            return name;
        }

        public virtual void SetCountdownTimer(float time, System.Action<float> onUpdateCountdown)
        {
            throw new System.NotImplementedException();
        }

        public virtual void ResetRacerInfo()
        {
            m_IsTimerPaused = true;
            m_Timer = 0f;
            m_CurrentLap = 0;
            m_countdownTimerEnabled = false;
        }
    }
}